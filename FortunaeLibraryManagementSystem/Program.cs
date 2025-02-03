using FortunaeLibraryManagementSystem.Infrastructure.Data;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using FortunaeLibraryManagementSystem.Service.Services;
using FortunaeLibraryManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using CloudinaryDotNet;
using AspNetCoreRateLimit;
using Amazon.S3;
using FortunaeLibraryManagementSystem.Service.Services.CacheService;
using FortunaeLibraryManagementSystem.Middleware;
using StackExchange.Redis;
using DotNetEnv;
using static FortunaeLibraryManagementSystem.AppSettings;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Enable logging (Important for AWS debugging)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


builder.Services.Configure<AWSSettings>(options =>
{
    options.AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
    options.SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
    options.S3BucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET");
    options.Region = Environment.GetEnvironmentVariable("AWS_REGION");
});

// Configure Redis
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "FortunaeCache:";
});

// Configure Cloudinary
builder.Services.Configure<CloudinarySettings>(options =>
{
    options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
});

// Configure JWT
builder.Services.Configure<JwtSettings>(options =>
{
    options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    options.ExpirationTime = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION") ?? "30");
    options.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
});

// Configure DB Connection
builder.Services.Configure<DatabaseSettings>(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
});

// Configure Database
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(Environment.GetEnvironmentVariable("DB_CONNECTION")));
// Set up database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Database connection string is missing!");
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure()));

//// Redis Configuration (Use Redis if available, otherwise fallback to in-memory cache)
//var redisConnection = builder.Configuration.GetConnectionString("Redis");
var redisOptions = ConfigurationOptions.Parse(redisConnection!);
redisOptions.ConnectRetry = 5;
redisOptions.ConnectTimeout = 5000;
redisOptions.AbortOnConnectFail = false;
redisOptions.SyncTimeout = 5000;

//// Register IConnectionMultiplexer as singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisOptions));

// Register IDistributedCache
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.ConfigurationOptions = redisOptions;
//    options.InstanceName = "FortunaeCache:";
//});

// Register Redis Service
builder.Services.AddScoped<IRedisService, RedisService>();

// Dependency Injection for Services & Repositories
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowingService, BorrowingService>();
builder.Services.AddScoped<IBorrowingRepository, BorrowingRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddHealthChecks();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();


// AWS S3 Configuration
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IImageService, ImageService>();

// Cloudinary Configuration
var cloudinaryAccount = new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]
);
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

builder.Services.AddControllers();

// JWT Authentication Setup
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    Console.WriteLine("⚠️ JWT SecretKey is missing! Check AWS environment variables.");
    throw new Exception("JWT SecretKey is missing!");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Kestrel Configuration for AWS Load Balancer
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
});

// Build Application BEFORE Using Services
var app = builder.Build();

// Configure Middleware

    //app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.ApplyMigrations();


// CORS Configuration for AWS
app.UseCors(policy => policy
    .SetIsOriginAllowed(_ => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.UseFortunaExceptionHandler();
app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.MapHealthChecks("/health");
// Enable Authentication & Authorization
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// API Controllers
app.MapControllers();

// Start Application
app.Run();
