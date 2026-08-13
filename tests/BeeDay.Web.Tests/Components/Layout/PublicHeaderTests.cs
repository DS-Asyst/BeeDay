using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Layout;
using BeeDay.Web.Services;
using BeeDay.Web.Services.Authentication;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class PublicHeaderTests
{
    [Fact]
    public void RendersHeaderLandmarkWithLoginAndRegistrationForAnonymousUser()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<PublicHeader>();

        Assert.NotNull(cut.Find("header.public-header"));
        Assert.NotNull(cut.Find(".public-header__brand .beeday-brand"));

        Assert.Equal("/login", cut.Find("a.public-header__login").GetAttribute("href"));
        Assert.Equal("/profile/create", cut.Find("a.public-header__create").GetAttribute("href"));
    }

    [Fact]
    public void RendersContinueCtaForAuthenticatedUser()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetAuthorized("test-user");
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<PublicHeader>();

        var cta = cut.Find("button");
        Assert.Equal("Continue to BeeDay", cta.TextContent.Trim());
    }

    [Fact]
    public void AnonymousActionsUseRealRoutes()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<PublicHeader>();
        Assert.Equal("/login", cut.Find("a.public-header__login").GetAttribute("href"));
        Assert.Equal("/profile/create", cut.Find("a.public-header__create").GetAttribute("href"));
    }

    [Fact]
    public void AuthenticatedWithoutProfile_ContinueCtaGoesToProfileCreate()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetAuthorized("test-user");
        RegisterDestinationResolver(context, hasProfile: false, hasCompletedOnboarding: false);

        var cut = context.Render<PublicHeader>();
        cut.Find("button").Click();

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/profile/create", navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticatedWithIncompleteOnboarding_ContinueCtaGoesToTutorial()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetAuthorized("test-user");
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: false);

        var cut = context.Render<PublicHeader>();
        cut.Find("button").Click();

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/onboarding/tutorial", navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticatedReady_ContinueCtaGoesToHome()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetAuthorized("test-user");
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<PublicHeader>();
        cut.Find("button").Click();

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/profile", navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void BrandLinksHome()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<PublicHeader>();

        var brandLink = cut.Find("a.public-header__brand");
        Assert.Equal("/", brandLink.GetAttribute("href"));
    }

    internal static void RegisterDestinationResolver(BunitContext context, bool hasProfile, bool hasCompletedOnboarding)
    {
        var response = new CurrentUserResponse(
            Guid.NewGuid(), "Test User", "test@beeday.invalid", "tester",
            UserLanguage.English, UserTheme.System, true, hasCompletedOnboarding, true, hasProfile);
        var store = new BeeDayWebService(new StubCurrentUserSender(response));
        context.Services.AddSingleton(new AuthenticatedEntryDestinationResolver(store));
    }

    private sealed class StubCurrentUserSender(CurrentUserResponse response) : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetCurrentUserQuery)
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
}
