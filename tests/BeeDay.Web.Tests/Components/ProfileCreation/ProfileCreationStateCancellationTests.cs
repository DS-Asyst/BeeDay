using BeeDay.Application.Features.Users.Commands;
using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.ProfileCreation;
using BeeDay.Web.Components.Features.ProfileCreation.State;
using BeeDay.Web.Resources;
using BeeDay.Web.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Components.ProfileCreation;

/// <summary>
/// EPIC 30 Sprint 30.11: ProfileCreationState is AddScoped (circuit lifetime), same as
/// DashboardState, but was missed by Sprint 30.8's BD30-F035 cancellation-propagation sweep — that
/// search only globbed *.razor/*.razor.cs, and this is a plain .cs state class. Proves the same
/// contract DashboardStateCancellationTests already proves for DashboardState: the token forwarded
/// to ISender.Send is real (not CancellationToken.None) and Dispose cancels it.
/// </summary>
public sealed class ProfileCreationStateCancellationTests
{
    [Fact]
    public async Task InitializeAsync_ForAnAuthenticatedSession_ForwardsANotYetCancelledToken()
    {
        var sender = new RecordingSender();
        var state = CreateState(sender);

        await state.InitializeAsync(hasAuthenticatedSession: true);

        Assert.True(sender.LastToken.CanBeCanceled);
        Assert.False(sender.LastToken.IsCancellationRequested);
    }

    [Fact]
    public async Task CompleteProfileAsync_ForwardsTheSameTokenThatDisposeLaterCancels()
    {
        var sender = new RecordingSender();
        var state = CreateState(sender);
        await state.InitializeAsync(hasAuthenticatedSession: true);
        state.Model.Name = "Ada Lovelace";
        state.Model.Nickname = "ada";

        await state.CompleteProfileAsync();
        var tokenUsedForTheCompletion = sender.LastToken;

        Assert.False(tokenUsedForTheCompletion.IsCancellationRequested);

        state.Dispose();

        Assert.True(tokenUsedForTheCompletion.IsCancellationRequested);
    }

    [Fact]
    public async Task CompleteProfileAsync_ForAnonymousRegistration_AlsoForwardsTheCancellableToken()
    {
        var sender = new RecordingSender();
        var state = CreateState(sender);
        await state.InitializeAsync(hasAuthenticatedSession: false);
        state.Model.Name = "Grace Hopper";
        state.Model.Email = "grace@beeday.invalid";
        state.Model.Password = "Password123!";
        state.Model.ConfirmPassword = "Password123!";
        state.Model.Nickname = "grace";

        await state.CompleteProfileAsync();

        Assert.True(sender.LastToken.CanBeCanceled);
        Assert.False(sender.LastToken.IsCancellationRequested);
    }

    private static ProfileCreationState CreateState(ISender sender)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        var provider = services.BuildServiceProvider();

        var toastService = new ToastService(provider.GetRequiredService<IStringLocalizer<SharedResources>>());
        var store = new BeeDayWebService(sender);
        return new ProfileCreationState(
            store,
            toastService,
            provider.GetRequiredService<IStringLocalizer<ProfileCreationResources>>(),
            provider.GetRequiredService<IStringLocalizer<SharedResources>>());
    }

    private sealed class RecordingSender : ISender
    {
        public CancellationToken LastToken { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastToken = cancellationToken;
            return request switch
            {
                GetCurrentUserQuery => Task.FromResult((TResponse)(object?)EmptyCurrentUser()!),
                CreateAccountCommand => Task.FromResult((TResponse)(object)Guid.NewGuid()),
                _ => Task.FromResult(default(TResponse)!)
            };
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            LastToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private static CurrentUserResponse EmptyCurrentUser() => new(
            Guid.NewGuid(), "Test User", "test@beeday.invalid", "", UserLanguage.English, UserTheme.Light, true, false, true, false);
    }
}
