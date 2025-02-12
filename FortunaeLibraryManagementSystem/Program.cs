using FortunaeLibraryManagementSystem.Infrastructure.Data;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using FortunaeLibraryManagementSystem.Service.Services;
using FortunaeLibraryManagementSystem.Infrastructure.Repositories;
using FortunaeLibraryManagementSystem.Middleware;
using FortunaeLibraryManagementSystem.Service.Services.CacheService;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using CloudinaryDotNet;
using AspNetCoreRateLimit;
using Amazon.S3;
using StackExchange.Redis;
using DotNetEnv;
using static FortunaeLibraryManagementSystem.AppSettings;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Amazon;
using System.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
//var redisOptions = ConfigurationOptions.Parse(builder.Configuration.GetValue<string>("Redis:ConnectionString"));
var redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddScoped<IRedisService, RedisService>();


var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger("Program");

try
{
    logger.LogInformation("Starting web application");
}
catch (Exception ex)
{
    logger.LogError(ex, "Application start-up failed");
    throw;
}
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? throw new Exception("JWT_ISSUER is missing!");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? throw new Exception("JWT_AUDIENCE is missing!");
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new Exception("JWT_SECRET_KEY is missing!");
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new Exception("DB_CONNECTION is missing!");
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION")
    ?? throw new Exception("REDIS_CONNECTION is missing!");


builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName");
});


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"Received token: {context.Token}");
            return Task.CompletedTask;
        }
    };
});
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

var awsConfig = builder.Configuration.GetSection("AWS").Get<AWSSettings>();

builder.Services.AddSingleton(new AmazonS3Client(
    awsConfig.AccessKeyId,
    awsConfig.SecretAccessKey,
    RegionEndpoint.GetBySystemName(awsConfig.Region)
));
builder.Services.Configure<AWSSettings>(options =>
{
    options.AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
    options.SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
    options.S3BucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET");
    options.Region = Environment.GetEnvironmentVariable("AWS_REGION");
});

builder.Services.Configure<CloudinarySettings>(options =>
{
    options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
});


builder.Services.Configure<JwtSettings>(options =>
{
    options.Issuer = jwtIssuer;
    options.Audience = jwtAudience;
    options.SecretKey = jwtSecretKey;
    options.ExpirationTime = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION") ?? "30");
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowingService, BorrowingService>();
builder.Services.AddScoped<IBorrowingRepository, BorrowingRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

builder.Services.AddAWSService<IAmazonS3>();
var cloudinaryAccount = new Account(
    Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
    Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
    Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
);
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddHealthChecks();


//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.Limits.MaxRequestHeadersTotalSize = 65536;
//    var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
//    serverOptions.ListenAnyIP(int.Parse(port), options =>
//    {
//        options.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
//    });
//});


var app = builder.Build();

app.UseForwardedHeaders();


if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    //app.UseHealthChecksUI();
    //app.UseHealthChecks("/health", new HealthCheckOptions()
    //{
    //    Predicate = _ => true,
    //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    //});
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(policy => policy
    .SetIsOriginAllowed(_ => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.UseFortunaExceptionHandler();
app.UseIpRateLimiting();
builder.Services.AddHealthChecks();

app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
    }
});
//app.MapHealthChecks("/health/redis", new HealthCheckOptions
//{
//    Predicate = check => check.Tags.Contains("redis"),
//    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//});



app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//string port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
//string ip = Environment.GetEnvironmentVariable("HOST") ?? "0.0.0.0";

app.Run();