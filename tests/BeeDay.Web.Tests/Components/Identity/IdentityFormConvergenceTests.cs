using BeeDay.Web.Components.Features.Identity.Pages;
using BeeDay.Web.Tests.Localization;
using MediatR;

namespace BeeDay.Web.Tests.Components.Identity;

public sealed class IdentityFormConvergenceTests
{
    [Fact]
    public void ForgotPassword_UsesSharedEmailFieldAndLocalizedAccessibleLabel()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ForgotPassword>());

        var field = cut.Find(".beeday-field");
        var input = field.QuerySelector("#forgot-password-email")!;

        Assert.Equal("forgot-password-email", field.GetAttribute("for"));
        Assert.Equal("email", input.GetAttribute("type"));
        Assert.Equal("email", input.GetAttribute("autocomplete"));
        Assert.Contains("beeday-field__control", input.ClassList);
        Assert.Contains("Email", field.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void ForgotPassword_InvalidSubmitKeepsDataAnnotationsAndSharedValidationFeedback()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ForgotPassword>());

        cut.Find("form").Submit();

        Assert.Equal("true", cut.Find("#forgot-password-email").GetAttribute("aria-invalid"));
        Assert.NotEmpty(cut.FindAll(".beeday-validation-message[role='alert']"));
        Assert.Equal(0, context.Services.GetRequiredService<RecordingSender>().SendCount);
    }

    [Fact]
    public void ResetPassword_PreservesPasswordAutocompleteAndSharedFieldContracts()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ResetPassword>());

        var inputs = cut.FindAll("input.beeday-field__control[type='password'][autocomplete='new-password']");

        Assert.Equal(2, inputs.Count);
        Assert.Equal("reset-password", inputs[0].Id);
        Assert.Equal("reset-password-confirmation", inputs[1].Id);
    }

    [Fact]
    public void ResendConfirmation_UsesSharedEmailFieldInPortuguese()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<ResendConfirmation>());

        var label = cut.Find("label[for='resend-confirmation-email']");
        var input = cut.Find("#resend-confirmation-email");

        Assert.Contains("E-mail", label.TextContent, StringComparison.Ordinal);
        Assert.Equal("email", input.GetAttribute("type"));
        Assert.Contains("beeday-field__control", input.ClassList);
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext().WithLocalization();
        context.Services.AddSingleton<RecordingSender>();
        context.Services.AddSingleton<ISender>(services => services.GetRequiredService<RecordingSender>());
        return context;
    }

    private sealed class RecordingSender : ISender
    {
        public int SendCount { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            SendCount++;
            return Task.CompletedTask;
        }
    }
}
