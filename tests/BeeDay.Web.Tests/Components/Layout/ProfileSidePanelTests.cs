using BeeDay.Application.Features.Dashboard.Queries;
using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Dashboard.State;
using BeeDay.Web.Components.Layout;
using BeeDay.Web.Services;
using MediatR;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class ProfileSidePanelTests
{
    [Fact]
    public void KeepsTheIdentityAreaPresentationalAndMakesTheAvatarTheAccessibleNavigationPath()
    {
        using var context = new BunitContext();
        var response = BuildResponseWithProfile(out var name);
        context.Services.AddSingleton(BuildState(response));

        var cut = context.Render<ProfileSidePanel>(parameters => parameters
            .Add(component => component.IsOpen, true));

        Assert.DoesNotContain("View profile", cut.Find(".profile-drawer").TextContent, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".profile-drawer__experience-link"));

        var profile = cut.Find(".profile-drawer__profile");
        Assert.Equal("div", profile.TagName, ignoreCase: true);
        Assert.Empty(cut.FindAll("a.profile-drawer__profile"));

        var avatarLink = cut.Find("a.profile-drawer__avatar");
        Assert.Equal("/account", avatarLink.GetAttribute("href"));
        Assert.Equal("View profile details", avatarLink.GetAttribute("aria-label"));
        Assert.Equal("J", avatarLink.TextContent.Trim());

        Assert.Contains(name, cut.Find(".profile-drawer__identity h2").TextContent);
        Assert.Contains("Level", cut.Markup);
    }

    [Fact]
    public void RendersLoadingStateWithoutTheAvatarLink()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new DashboardState(new BeeDayWebService(new PendingSender()), new ToastService()));

        var cut = context.Render<ProfileSidePanel>(parameters => parameters
            .Add(component => component.IsOpen, true));

        Assert.NotEmpty(cut.FindAll(".profile-drawer__loading"));
        Assert.Empty(cut.FindAll("a.profile-drawer__avatar"));
    }

    [Fact]
    public void RendersCreateProfileLinkWhenNoProfileExists()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(BuildState(BuildResponseWithoutProfile()));

        var cut = context.Render<ProfileSidePanel>(parameters => parameters
            .Add(component => component.IsOpen, true));

        var createLink = cut.Find("a.profile-drawer__link");
        Assert.Equal("/profile/create", createLink.GetAttribute("href"));
        Assert.Empty(cut.FindAll("a.profile-drawer__avatar"));
    }

    private static DashboardResponse BuildResponseWithProfile(out string name)
    {
        name = "Jane Doe";
        var profile = new UserProfileSummary(
            Guid.NewGuid(), "janedoe", name, string.Empty, UserLanguage.English, UserTheme.System, 0, 1, 0, 100);
        return new DashboardResponse(profile, [], [], [], null);
    }

    private static DashboardResponse BuildResponseWithoutProfile()
    {
        var profile = new UserProfileSummary(
            Guid.NewGuid(), string.Empty, string.Empty, string.Empty, UserLanguage.English, UserTheme.System, 0, 1, 0, 0);
        return new DashboardResponse(profile, [], [], [], null);
    }

    private static DashboardState BuildState(DashboardResponse response) =>
        new(new BeeDayWebService(new StubSender(response)), new ToastService());

    private sealed class StubSender(DashboardResponse response) : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetDashboardQuery)
            {
                return Task.FromResult((TResponse)(object)response);
            }

            throw new NotSupportedException($"Unexpected request: {request.GetType().Name}");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            throw new NotImplementedException();
        }
    }

    private sealed class PendingSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            new TaskCompletionSource<TResponse>().Task;

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            new TaskCompletionSource<object?>().Task;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            throw new NotImplementedException();
        }
    }
}
