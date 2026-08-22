using BeeDay.Domain.Exceptions;
using BeeDay.Web.Components.Features.Identity.Pages;
using BeeDay.Web.Tests.Localization;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Identity;

/// <summary>
/// EPIC 30 Sprint 30.10: this page's @rendermode (InteractiveServer, set globally on &lt;Routes&gt;
/// in App.razor) statically prerenders once before the interactive circuit takes over —
/// OnInitializedAsync runs on both passes. A real Chromium E2E test
/// (AccountLifecycleTests.CreateAccount_ConfirmsEmailThroughARealLink_ThenUnlocksLogin) proved the
/// full authentic flow now works; these bUnit tests give bUnit explicit control over
/// RendererInfo.IsInteractive (via TestContext.Renderer.SetRendererInfo, which raw-HttpClient
/// integration tests cannot do — that hitting only ever observes the static, non-interactive pass)
/// to assert both passes precisely, including localization of a post-mutation state that only the
/// interactive pass can reach. Replaces the two IdentityFlowLocalizationIntegrationTests assertions
/// that unknowingly depended on the double-invocation bug this Sprint fixed.
/// </summary>
public sealed class ConfirmEmailTests
{
    [Fact]
    public async Task StaticPrerenderPass_NeverSendsConfirmEmailCommand()
    {
        var sender = new RecordingSender(exceptionToThrow: null);
        var (context, _) = CreateContext(sender, interactive: false);

        var cut = await RenderWithTokenAsync(context, "any-token", "en-US");

        Assert.False(sender.SendCalled);
        Assert.Contains("Confirming email", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StaticPrerenderPass_WithMissingToken_StillShowsInvalidLinkMessage()
    {
        // Unlike the mutating Send call below, missing-token validation is pure input checking with
        // no replay-safety concern, so it deliberately runs on both passes, not just the interactive
        // one — proven here by asserting it under interactive: false.
        var sender = new RecordingSender(exceptionToThrow: null);
        var (context, _) = CreateContext(sender, interactive: false);

        var cut = await RenderWithTokenAsync(context, token: null, "en-US");

        Assert.False(sender.SendCalled);
        Assert.Contains("Invalid confirmation link", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("The confirmation token is missing.", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("en-US", "Confirmation link expired", "Confirmation links expire after a period of time for security reasons.")]
    [InlineData("pt-BR", "Link de confirmação expirado", "Links de confirmação expiram após um período por motivos de segurança.")]
    public async Task InteractivePass_WithExpiredToken_RendersTheLocalizedExpiredMessage_NotTheRawDomainText(
        string culture, string expectedHeadingFragment, string expectedMessageFragment)
    {
        var sender = new RecordingSender(new InvalidDomainStateException("Token has expired."));
        var (context, _) = CreateContext(sender, interactive: true);

        var cut = await RenderWithTokenAsync(context, "expired-token", culture);

        Assert.True(sender.SendCalled);
        Assert.Contains(expectedHeadingFragment, cut.Markup, StringComparison.Ordinal);
        Assert.Contains(expectedMessageFragment, cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Token has expired.", cut.Markup, StringComparison.Ordinal);
    }

    private static (BunitContext Context, RecordingSender Sender) CreateContext(RecordingSender sender, bool interactive)
    {
        var context = new BunitContext().WithLocalization();
        context.Services.AddSingleton<ISender>(sender);
        context.Renderer.SetRendererInfo(new RendererInfo("Server", interactive));
        return (context, sender);
    }

    private static async Task<Bunit.IRenderedComponent<ConfirmEmail>> RenderWithTokenAsync(BunitContext context, string? token, string culture)
    {
        Bunit.IRenderedComponent<ConfirmEmail>? cut = null;
        await BunitLocalizationSupport.WithUiCultureAsync(culture, () =>
        {
            var navigation = context.Services.GetRequiredService<NavigationManager>();
            if (token is not null)
            {
                navigation.NavigateTo(navigation.GetUriWithQueryParameter("token", token));
            }

            cut = context.Render<ConfirmEmail>();
            return Task.CompletedTask;
        });

        return cut!;
    }

    private sealed class RecordingSender(Exception? exceptionToThrow) : ISender
    {
        public bool SendCalled { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            SendCalled = true;
            return exceptionToThrow is null ? Task.CompletedTask : Task.FromException(exceptionToThrow);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
