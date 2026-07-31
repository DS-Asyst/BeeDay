using LevelUp.Application.Features.Dashboard.Queries;
using LevelUp.Application.Features.Dashboard.Responses;
using LevelUp.Domain.Entities;
using LevelUp.Web.Components.Features.Dashboard.State;
using LevelUp.Web.Components.Layout;
using LevelUp.Web.Services;
using MediatR;

namespace LevelUp.Web.Tests.Components.Layout;

public sealed class ProfileSidePanelTests
{
    [Fact]
    public void KeepsTheIdentityAreaPresentationalAndMakesTheAvatarTheAccessibleNavigationPath()
    {
        using var context = new BunitContext();
        var data = BuildDataWithProfile(out var user);
        context.Services.AddSingleton(BuildState(data));

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

        Assert.Contains(user.Name, cut.Find(".profile-drawer__identity h2").TextContent);
        Assert.Contains("Level", cut.Markup);
    }

    [Fact]
    public void RendersLoadingStateWithoutTheAvatarLink()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new DashboardState(new LevelUpWebService(new PendingSender()), new ToastService()));

        var cut = context.Render<ProfileSidePanel>(parameters => parameters
            .Add(component => component.IsOpen, true));

        Assert.NotEmpty(cut.FindAll(".profile-drawer__loading"));
        Assert.Empty(cut.FindAll("a.profile-drawer__avatar"));
    }

    [Fact]
    public void RendersCreateProfileLinkWhenNoProfileExists()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(BuildState(new LevelUpData()));

        var cut = context.Render<ProfileSidePanel>(parameters => parameters
            .Add(component => component.IsOpen, true));

        var createLink = cut.Find("a.profile-drawer__link");
        Assert.Equal("/profile/create", createLink.GetAttribute("href"));
        Assert.Empty(cut.FindAll("a.profile-drawer__avatar"));
    }

    private static LevelUpData BuildDataWithProfile(out User user)
    {
        var data = new LevelUpData();
        user = User.Create("Jane Doe", "jane@example.com");
        data.AddUser(user);
        data.CompleteUserProfile(user.Id, "janedoe");
        data.EnsureValidState();
        return data;
    }

    private static DashboardState BuildState(LevelUpData data) =>
        new(new LevelUpWebService(new StubSender(data)), new ToastService());

    private sealed class StubSender(LevelUpData data) : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetLevelUpQuery)
            {
                return Task.FromResult((TResponse)(object)new GetLevelUpResponse(data));
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
