using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

/// <summary>
/// Global error-handling middleware that converts exceptions into the standard
/// <see cref="ErrorResponse"/> shape (<c>type</c>/<c>error</c>/<c>detail</c>) with the
/// appropriate HTTP status code.
/// </summary>
public class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "ValidationError",
                    Error = "Invalid input data",
                    Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))
                }),

            DomainException domain => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "DomainRuleViolation",
                    Error = "Business rule violation",
                    Detail = domain.Message
                }),

            KeyNotFoundException notFound => (
                StatusCodes.Status404NotFound,
                new ErrorResponse
                {
                    Type = "ResourceNotFound",
                    Error = "Resource not found",
                    Detail = notFound.Message
                }),

            InvalidOperationException conflict => (
                StatusCodes.Status409Conflict,
                new ErrorResponse
                {
                    Type = "ConflictError",
                    Error = "The request could not be completed due to a conflict",
                    Detail = conflict.Message
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Type = "InternalServerError",
                    Error = "An unexpected error occurred",
                    Detail = "An unexpected error occurred while processing the request."
                })
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
