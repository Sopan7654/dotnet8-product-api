using System.Text.Json;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

/// <summary>
/// Global error handling middleware – maps domain exceptions to consistent RFC 7807 Problem Details responses.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, errors) = exception switch
        {
            NotFoundException e => (StatusCodes.Status404NotFound, "Resource Not Found", (object?)null),
            Domain.Exceptions.ValidationException e => (StatusCodes.Status422UnprocessableEntity, "Validation Failed", e.Errors),
            UnauthorizedException e => (StatusCodes.Status401Unauthorized, "Unauthorized", (object?)null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", (object?)null)
        };

        context.Response.StatusCode = statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail = exception.Message,
            errors
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
