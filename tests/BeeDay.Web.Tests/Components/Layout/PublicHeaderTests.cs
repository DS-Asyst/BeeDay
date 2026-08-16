using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Layout;
using BeeDay.Web.Services;
using BeeDay.Web.Services.Authentication;
using BeeDay.Web.Tests.Localization;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class PublicHeaderTests
{
    [Fact]
    public void RendersHeaderLandmarkWithBrandMarkAndLanguageSwitcherForAnonymousUser()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetNotAuthorized();
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<PublicHeader>());

        Assert.NotNull(cut.Find("header.public-header"));
        Assert.NotNull(cut.Find(".public-header__brand img.public-header__brand-mark"));

        Assert.Empty(cut.FindAll("a.public-header__login"));
        Assert.Empty(cut.FindAll("a[href='/profile/create']"));

        var portuguese = cut.Find("button[aria-label='Português (Brasil)']");
        var english = cut.Find("button[aria-label='English (United States)']");
        Assert.Equal("false", portuguese.GetAttribute("aria-pressed"));
        Assert.Equal("true", english.GetAttribute("aria-pressed"));
    }

    [Fact]
    public void RendersContinueCtaForAuthenticatedUser()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetAuthorized("test-user");
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<PublicHeader>());

        var cta = cut.Find("button.beeday-button");
        Assert.Equal("Continue to beeday", cta.TextContent.Trim());
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseAriaLabels()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetNotAuthorized();
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<PublicHeader>());

        Assert.Equal("Página inicial do beeday", cut.Find("a.public-header__brand").GetAttribute("aria-label"));
        Assert.Equal("Idioma", cut.Find("form.public-language-switcher").GetAttribute("aria-label"));
    }

    [Fact]
    public void LanguageSwitcher_PostsToTheOfficialCultureEndpointWithCorrectCultureValues()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetNotAuthorized();
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<PublicHeader>();

        var form = cut.Find("form.public-language-switcher");
        Assert.Equal("post", form.GetAttribute("method"));
        Assert.Equal("/culture/set", form.GetAttribute("action"));
        Assert.NotNull(cut.Find("input[name='returnUrl']"));

        var portuguese = cut.Find("button[aria-label='Português (Brasil)']");
        var english = cut.Find("button[aria-label='English (United States)']");
        Assert.Equal("culture", portuguese.GetAttribute("name"));
        Assert.Equal("pt-BR", portuguese.GetAttribute("value"));
        Assert.Equal("culture", english.GetAttribute("name"));
        Assert.Equal("en-US", english.GetAttribute("value"));
    }

    [Fact]
    public void AuthenticatedWithoutProfile_ContinueCtaGoesToProfileCreate()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetAuthorized("test-user");
        RegisterDestinationResolver(context, hasProfile: false, hasCompletedOnboarding: false);

        var cut = context.Render<PublicHeader>();
        cut.Find("button.beeday-button").Click();

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/profile/create", navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticatedWithIncompleteOnboarding_ContinueCtaGoesToTutorial()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetAuthorized("test-user");
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: false);

        var cut = context.Render<PublicHeader>();
        cut.Find("button.beeday-button").Click();

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/onboarding/tutorial", navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticatedReady_ContinueCtaGoesToHome()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetAuthorized("test-user");
        RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<PublicHeader>();
        cut.Find("button.beeday-button").Click();

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/profile", navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void BrandLinksHome()
    {
        using var context = new BunitContext().WithLocalization();
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
