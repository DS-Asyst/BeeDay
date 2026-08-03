using BeeDay.Application.Exceptions;
using BeeDay.Domain.Exceptions;
using BeeDay.Infrastructure.Persistence.Exceptions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BeeDay.Web.Diagnostics;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        var problem = Map(exception, httpContext, environment.IsDevelopment());

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                WebEventIds.RequestFailed,
                exception,
                "Unhandled request failure. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, CorrelationId: {CorrelationId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                problem.Status,
                httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                WebEventIds.RequestFailed,
                "Request rejected. ExceptionType: {ExceptionType}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, CorrelationId: {CorrelationId}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                problem.Status,
                httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });

        if (!written)
        {
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        }

        return true;
    }

    internal static ProblemDetails Map(Exception exception, HttpContext context, bool includeTechnicalDetails)
    {
        var problem = exception switch
        {
            ApplicationValidationException validation => new ValidationProblemDetails(validation.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = validation.Message,
                Type = "https://httpstatuses.com/400"
            },
            DomainValidationException validation => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = validation.Message,
                Type = "https://httpstatuses.com/400",
                Extensions = { ["field"] = validation.Field }
            },
            InvalidDomainStateException domainState => Create(
                StatusCodes.Status409Conflict,
                "Invalid domain state",
                domainState.Message),
            // BadHttpRequestException is minimal APIs' contract for "the request itself is
            // malformed" (missing/invalid antiforgery token, missing body, unparseable form
            // data, etc.) — always a client error, never a server fault. A bare
            // AntiforgeryValidationException is handled the same way in case another code path
            // throws it unwrapped.
            AntiforgeryValidationException or BadHttpRequestException => Create(
                StatusCodes.Status400BadRequest,
                "Malformed request",
                includeTechnicalDetails ? exception.Message : "The request could not be processed. Reload the page and try again."),
            ActivityNotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Activity not found",
                Detail = notFound.Message,
                Type = "https://httpstatuses.com/404",
                Extensions = { ["activityId"] = notFound.ActivityId }
            },
            PersistenceException => Create(
                StatusCodes.Status503ServiceUnavailable,
                "Persistence unavailable",
                "The application data could not be accessed. Try again shortly."),
            OperationCanceledException when context.RequestAborted.IsCancellationRequested => Create(
                499,
                "Request cancelled",
                "The request was cancelled by the client."),
            _ => Create(
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                includeTechnicalDetails ? exception.Message : "An unexpected error occurred.")
        };

        problem.Instance = context.Request.Path;
        problem.Extensions["correlationId"] = context.TraceIdentifier;
        problem.Extensions["requestId"] = context.TraceIdentifier;

        if (includeTechnicalDetails && exception.InnerException is not null)
        {
            problem.Extensions["innerException"] = exception.InnerException.Message;
        }

        return problem;
    }

    private static ProblemDetails Create(int status, string title, string detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail,
        Type = $"https://httpstatuses.com/{status}"
    };
}
