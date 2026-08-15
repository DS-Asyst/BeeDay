using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Responses;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Web.Resources;
using BeeDay.Web.Services;
using BeeDay.Web.Tests.Localization;
using MediatR;
using Microsoft.Extensions.Localization;
using AccountPage = BeeDay.Web.Components.Features.Account.Pages.Account;

namespace BeeDay.Web.Tests.Components.Account;

public sealed class AccountTests
{
    [Fact]
    public void UnderEnglishUiCulture_RendersEnglishAccountResources()
    {
        using var context = CreateContext(UserLanguage.English);
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AccountPage>());

        Assert.Contains("My Account", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Save Profile", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Save Preferences", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Change Password", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseAccountResources()
    {
        using var context = CreateContext(UserLanguage.Portuguese);
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<AccountPage>());

        Assert.Contains("Minha Conta", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Salvar Perfil", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Salvar Preferências", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Alterar Senha", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("My Account", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PreferencesLanguageSelect_ReflectsThePersistedAccountLanguage()
    {
        using var context = CreateContext(UserLanguage.Portuguese);
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<AccountPage>());

        var select = cut.Find("#account-language");
        Assert.Equal(UserLanguage.Portuguese.ToString(), select.GetAttribute("value"));
    }

    [Fact]
    public void RendersACultureSyncFormPointingAtTheOfficialCultureEndpoint()
    {
        using var context = CreateContext(UserLanguage.English);
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AccountPage>());

        var form = cut.Find("#culture-sync-form");
        Assert.Equal("post", form.GetAttribute("method"));
        Assert.Equal("/culture/set", form.GetAttribute("action"));
        Assert.NotNull(form.QuerySelector("input[name='culture']"));
        Assert.NotNull(form.QuerySelector("input[name='returnUrl']"));
    }

    private static BunitContext CreateContext(UserLanguage accountLanguage)
    {
        var context = new BunitContext().WithLocalization();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.AddAuthorization().SetAuthorized("test-user");

        var response = new CurrentUserResponse(
            Guid.NewGuid(), "Test User", "test@beeday.invalid", "tester",
            accountLanguage, UserTheme.System, true, true, true, true);
        var store = new BeeDayWebService(new StubAccountSender(response));
        context.Services.AddSingleton(store);
        context.Services.AddSingleton(sp => new ToastService(sp.GetRequiredService<IStringLocalizer<SharedResources>>()));
        context.Services.AddSingleton(sp => new AuthenticatedUserInitializer(
            sp.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(),
            new UnusedUserRepository()));

        return context;
    }

    private sealed class StubAccountSender(CurrentUserResponse response) : ISender
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

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            Task.CompletedTask;
    }

    /// <summary>
    /// AuthenticatedUserInitializer bails out before touching the repository whenever the
    /// authenticated principal carries no parseable NameIdentifier GUID claim — true for bUnit's
    /// default SetAuthorized("test-user") — so every member here is unreachable by these tests.
    /// </summary>
    private sealed class UnusedUserRepository : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> IsEmailInUseAsync(string normalizedEmail, Guid? excludingUserId = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> IsNicknameInUseAsync(string normalizedNickname, Guid? excludingUserId = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task UpdateAsync(Guid userId, Action<User> mutation, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
