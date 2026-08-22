using BeeDay.Application.Features.Dashboard.Contracts;
using BeeDay.Application.Features.Dashboard.Handlers;
using BeeDay.Application.Features.Dashboard.Queries;
using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;

namespace BeeDay.Application.Tests;

public sealed class DashboardHandlerTests
{
    [Fact]
    public async Task GetDashboard_ForwardsTheCurrentUserIdAndReturnsTheReadServiceResponse()
    {
        var userId = Guid.NewGuid();
        var readService = new RecordingDashboardReadService();
        var handler = new GetDashboardQueryHandler(readService, new FakeCurrentUserContext(userId));

        var response = await handler.Handle(new GetDashboardQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(userId, readService.RequestedUserId);
        Assert.Same(readService.Response, response);
    }

    private sealed class RecordingDashboardReadService : IDashboardReadService
    {
        public Guid? RequestedUserId { get; private set; }

        public DashboardResponse Response { get; } = new(
            new UserProfileSummary(Guid.NewGuid(), "tester", "Test User", "", UserLanguage.English, UserTheme.Light, 0, 1, 0, 100),
            [],
            [],
            [],
            null);

        public Task<DashboardResponse> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            RequestedUserId = userId;
            return Task.FromResult(Response);
        }
    }
}
