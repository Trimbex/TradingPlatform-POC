using System.Net;
using System.Text.Json;
using FluentValidation;
using TradingPlatform.Application.Exceptions;

namespace TradingPlatform.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (HttpStatusCode.BadRequest, CreateValidationErrorResponse(validationEx)),
            NotFoundException => (HttpStatusCode.NotFound, CreateErrorResponse(exception.Message)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, CreateErrorResponse(exception.Message)),
            _ => (HttpStatusCode.InternalServerError, CreateErrorResponse(
                _environment.IsDevelopment() ? exception.ToString() : "An unexpected error occurred."))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static object CreateErrorResponse(string message) => new { message };

    private static object CreateValidationErrorResponse(ValidationException ex) => new
    {
        message = "One or more validation errors occurred.",
        errors = ex.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage })
    };
}
