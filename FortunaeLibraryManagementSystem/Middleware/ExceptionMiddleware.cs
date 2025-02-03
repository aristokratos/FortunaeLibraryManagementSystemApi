namespace FortunaeLibraryManagementSystem.Middleware;

using FortunaeLibraryManagementSystem.Domain.Exceptions;
using FortunaeLibraryManagementSystem.DTOs;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new ResponseMessages.ApiErrorResponse();

        switch (exception)
        {
            case FortunaException ex:
                context.Response.StatusCode = ex.StatusCode;
                response.Status = ex.StatusCode;
                response.ErrorCode = ex.ErrorCode;
                response.Message = ex.Message;
                if (ex is ValidationException validationEx)
                {
                    response.Errors = validationEx.Errors;
                }
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Status = (int)HttpStatusCode.Unauthorized;
                response.ErrorCode = "UNAUTHORIZED";
                response.Message = "Unauthorized access";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.ErrorCode = "INTERNAL_SERVER_ERROR";
                response.Message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An internal server error occurred.";
                break;
        }

        if (_environment.IsDevelopment())
        {
            response.DeveloperMessage = new ResponseMessages.DeveloperMessage
            {
                Exception = exception.GetType().Name,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}



public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseFortunaExceptionHandler(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}