using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using WalletAPI.Exceptions;

namespace WalletAPI.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IWebHostEnvironment environment,
        ILogger<GlobalExceptionHandler> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            NotFoundException notFoundEx => CreateProblemDetails(
                httpContext,
                StatusCodes.Status404NotFound,
                "Not Found",
                notFoundEx.Message),

            BadRequestException badRequestEx => CreateProblemDetails(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Bad Request",
                badRequestEx.Message),

            ValidationException validationEx => CreateValidationProblemDetails(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "One or more validation errors occurred.",
                validationEx.Errors),

            _ => CreateProblemDetails(
                httpContext,
                StatusCodes.Status500InternalServerError,
                "Server Error",
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.")
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        IDictionary<string, string[]> errors)
    {
        return new ValidationProblemDetails(errors)
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }
}