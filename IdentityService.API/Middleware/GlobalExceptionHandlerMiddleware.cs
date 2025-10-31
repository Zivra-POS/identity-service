using System.Net;
using System.Text.Json;
using IdentityService.Core.Exceptions;
using IdentityService.Shared.Response;

namespace IdentityService.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, result) = exception switch
        {
            ValidationException validationEx => (
                (int)validationEx.StatusCode,
                Result<object>.Failure(
                    validationEx.Errors,
                    "Validation failed",
                    validationEx.StatusCode)
            ),

            BusinessException businessEx => (
                (int)businessEx.StatusCode,
                Result<object>.Failure(
                    new List<string> { businessEx.Message },
                    "Business rule violation",
                    businessEx.StatusCode)
            ),

            NotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                Result<object>.Failure(
                    new List<string> { notFoundEx.Message },
                    "Resource not found",
                    HttpStatusCode.NotFound)
            ),

            ForbiddenException forbiddenEx => (
                (int)HttpStatusCode.Forbidden,
                Result<object>.Failure(
                    new List<string> { forbiddenEx.Message },
                    "Access forbidden",
                    HttpStatusCode.Forbidden)
            ),

            UnauthorizedException unauthorizedEx => (
                (int)HttpStatusCode.Unauthorized,
                Result<object>.Failure(
                    new List<string> { unauthorizedEx.Message },
                    "Unauthorized access",
                    HttpStatusCode.Unauthorized)
            ),

            FluentValidation.ValidationException fluentValidationEx => (
                (int)HttpStatusCode.BadRequest,
                Result<object>.Failure(
                    fluentValidationEx.Errors.Select(x => x.ErrorMessage).ToList(),
                    "Validation failed")
            ),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                Result<object>.Failure(
                    new List<string> { "An internal server error occurred" },
                    "Internal server error",
                    HttpStatusCode.InternalServerError)
            )
        };

        context.Response.StatusCode = statusCode;

        var jsonResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
