using BeeDay.Application.Features.Users.Commands;
using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Web.Resources;
using BeeDay.Web.Services;
using BeeDay.Web.Tests.Localization;
using MediatR;
using Microsoft.Extensions.Localization;
using TutorialPage = BeeDay.Web.Components.Features.Onboarding.Pages.Tutorial;

namespace BeeDay.Web.Tests.Components.Onboarding;

public sealed class TutorialTests
{
    [Fact]
    public void UnderEnglishUiCulture_RendersTheFirstSlideInEnglish()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<TutorialPage>());

        Assert.Contains("STEP 1 OF 5", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Your command center", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Daily keeps the actions that move your life forward in one place.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Review today before starting", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersTheFirstSlideInPortuguese()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<TutorialPage>());

        Assert.Contains("PASSO 1 DE 5", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Seu centro de comando", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("O Diário reúne em um só lugar as ações que fazem sua vida avançar.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Revise o dia antes de começar", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Your command center", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("en-US", "NEXT")]
    [InlineData("pt-BR", "PRÓXIMO")]
    public async Task ClickingNext_AdvancesToTheSecondSlide(string culture, string nextButtonText)
    {
        await BunitLocalizationSupport.WithUiCultureAsync(culture, async () =>
        {
            using var context = CreateContext();
            var cut = context.Render<TutorialPage>();

            var next = cut.FindAll("button").First(button => button.TextContent.Trim() == nextButtonText);
            await next.ClickAsync();

            Assert.Contains(culture == "en-US" ? "STEP 2 OF 5" : "PASSO 2 DE 5", cut.Markup, StringComparison.Ordinal);
            Assert.Contains(culture == "en-US" ? "Build your rhythm" : "Construa seu ritmo", cut.Markup, StringComparison.Ordinal);
        });
    }

    [Theory]
    [InlineData("en-US", "NEXT", "ENTER beeday")]
    [InlineData("pt-BR", "PRÓXIMO", "ENTRAR NO beeday")]
    public async Task OnTheLastSlide_TheNextButtonBecomesEnterBeeDay(string culture, string nextButtonText, string enterButtonText)
    {
        await BunitLocalizationSupport.WithUiCultureAsync(culture, async () =>
        {
            using var context = CreateContext();
            var cut = context.Render<TutorialPage>();

            for (var step = 0; step < 4; step++)
            {
                var next = cut.FindAll("button").First(button => button.TextContent.Trim() == nextButtonText);
                await next.ClickAsync();
            }

            Assert.Contains(cut.FindAll("button"), button => button.TextContent.Trim() == enterButtonText);
        });
    }

    // BD30-F045 (EPIC 30 Sprint 30.11): NextAsync's final CompleteOnboardingAsync call had no
    // try/catch and Tutorial.razor injected no ToastService — a failure (network blip, transient
    // 5xx, expired session) propagated unhandled into the circuit with zero user feedback, unlike
    // every other save path in the app. Proves the now-added catch surfaces a localized toast
    // instead, and that the user stays on the last slide (no unhandled navigation/crash).
    [Theory]
    [InlineData("en-US", "NEXT", "ENTER beeday")]
    [InlineData("pt-BR", "PRÓXIMO", "ENTRAR NO beeday")]
    public async Task WhenCompletingOnboardingFails_ShowsALocalizedToast_InsteadOfAnUnhandledException(
        string culture, string nextButtonText, string enterButtonText)
    {
        await BunitLocalizationSupport.WithUiCultureAsync(culture, async () =>
        {
            using var context = CreateContext(throwOnComplete: true);
            var cut = context.Render<TutorialPage>();
            var toastService = context.Services.GetRequiredService<ToastService>();

            for (var step = 0; step < 4; step++)
            {
                var next = cut.FindAll("button").First(button => button.TextContent.Trim() == nextButtonText);
                await next.ClickAsync();
            }

            var enter = cut.FindAll("button").First(button => button.TextContent.Trim() == enterButtonText);
            await enter.ClickAsync();

            var toast = Assert.Single(toastService.Messages);
            Assert.DoesNotContain("Simulated", toast.Message, StringComparison.Ordinal);
            Assert.Contains(cut.FindAll("button"), button => button.TextContent.Trim() == enterButtonText);
        });
    }

    private static BunitContext CreateContext(bool throwOnComplete = false)
    {
        var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetAuthorized("test-user");

        var response = new CurrentUserResponse(
            Guid.NewGuid(), "Test User", "test@beeday.invalid", "tester",
            UserLanguage.English, UserTheme.System, true, false, true, true);
        context.Services.AddSingleton(new BeeDayWebService(new StubSender(response, throwOnComplete)));
        context.Services.AddSingleton(sp => new ToastService(sp.GetRequiredService<IStringLocalizer<SharedResources>>()));

        return context;
    }

    private sealed class StubSender(CurrentUserResponse response, bool throwOnComplete = false) : ISender
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
            if (throwOnComplete && request is CompleteCurrentUserOnboardingCommand)
            {
                throw new InvalidDomainStateException("Simulated provider failure.");
            }

            return Task.CompletedTask;
        }
    }
}
