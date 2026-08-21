using BeeDay.Infrastructure.Persistence.Exceptions;
using BeeDay.Web.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace BeeDay.Web.Tests.Diagnostics;

// EPIC 30 Sprint 30.23 (BD30-F065 investigation): ConcurrencyConflictException is IS-A
// PersistenceException, and only the InternalsVisibleTo grant added in BeeDay.Web.csproj lets this
// suite call the internal Map method directly. ProblemDetailsIntegrationTests already documents why
// this exception isn't reachable through a real HTTP round trip in this app (it only ever surfaces
// inside MediatR calls made from Razor components over the SignalR circuit) — a direct unit test on
// Map is the only way to cover the mapping without fabricating an endpoint.
public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public void Map_ConcurrencyConflictException_Returns409WithReloadGuidanceInsteadOfPersistenceUnavailable()
    {
        var exception = new ConcurrencyConflictException(
            "The record was modified or deleted by another operation since it was loaded.",
            new InvalidOperationException("inner"));
        var context = new DefaultHttpContext();

        var problem = GlobalExceptionHandler.Map(exception, context, includeTechnicalDetails: false);

        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal("Concurrency conflict", problem.Title);
        Assert.Equal("This record was changed by another operation. Reload the page and try again.", problem.Detail);
    }

    [Fact]
    public void Map_PlainPersistenceException_StillReturns503ServiceUnavailable()
    {
        var exception = new PersistenceException("The change could not be saved to SQL Server.");
        var context = new DefaultHttpContext();

        var problem = GlobalExceptionHandler.Map(exception, context, includeTechnicalDetails: false);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, problem.Status);
        Assert.Equal("Persistence unavailable", problem.Title);
    }
}
