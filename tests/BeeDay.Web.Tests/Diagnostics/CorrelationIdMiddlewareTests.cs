using BeeDay.Web.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace BeeDay.Web.Tests.Diagnostics;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_PreservesValidIncomingCorrelationId()
    {
        const string expected = "request-123";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = expected;
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(expected, context.TraceIdentifier);
        Assert.Equal(expected, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_ReplacesUnsafeIncomingCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "invalid correlation id";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        Assert.NotEmpty(context.TraceIdentifier);
        Assert.DoesNotContain(' ', context.TraceIdentifier);
        Assert.Equal(context.TraceIdentifier, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    private static CorrelationIdMiddleware CreateMiddleware() => new(
        _ => Task.CompletedTask,
        NullLogger<CorrelationIdMiddleware>.Instance);
}
