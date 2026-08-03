using MediatR;
using Microsoft.Extensions.Logging;

namespace BeeDay.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Handling application request {RequestName}", name);
        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("Handled application request {RequestName}", name);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application request {RequestName} failed", name);
            throw;
        }
    }
}
