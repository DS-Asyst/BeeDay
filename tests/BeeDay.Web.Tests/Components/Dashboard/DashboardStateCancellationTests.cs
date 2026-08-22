using BeeDay.Application.Features.Dashboard.Queries;
using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Dashboard;
using BeeDay.Web.Components.Features.Dashboard.State;
using BeeDay.Web.Components.Features.Habits.Models;
using BeeDay.Web.Resources;
using BeeDay.Web.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Components.Dashboard;

/// <summary>
/// EPIC 30 Sprint 30.8: proves DashboardState threads a real, component-owned CancellationToken
/// through every mutation/query it sends via BeeDayWebService, and that Dispose (called by the DI
/// container when the owning circuit's scope ends, since DashboardState is AddScoped) cancels that
/// same token — closing the gap where every Web mutation previously always passed
/// CancellationToken.None.
/// </summary>
public sealed class DashboardStateCancellationTests
{
    [Fact]
    public async Task GetDataAsync_ForwardsANotYetCancelledToken()
    {
        var sender = new RecordingSender();
        var (state, _) = CreateState(sender);

        await state.GetDataAsync();

        Assert.True(sender.LastToken.CanBeCanceled);
        Assert.False(sender.LastToken.IsCancellationRequested);
    }

    [Fact]
    public async Task SaveHabitAsync_ForwardsTheSameTokenThatDisposeLaterCancels()
    {
        var sender = new RecordingSender();
        var (state, _) = CreateState(sender);

        await state.SaveHabitAsync(new HabitEditorModel { Title = "Read", Direction = HabitDirection.Positive, Difficulty = HabitDifficulty.Easy, ResetCounter = HabitResetCounter.Daily });
        var tokenUsedForTheMutation = sender.LastToken;

        Assert.False(tokenUsedForTheMutation.IsCancellationRequested);

        state.Dispose();

        Assert.True(tokenUsedForTheMutation.IsCancellationRequested);
    }

    [Fact]
    public async Task Dispose_DuringAnInFlightOperation_IsSwallowedWithoutShowingAGenericErrorToast()
    {
        var sender = new CancelingSender();
        var (state, toastService) = CreateState(sender);
        sender.CancelDuringSend = () => state.Dispose();

        await state.SaveHabitAsync(new HabitEditorModel { Title = "Read", Direction = HabitDirection.Positive, Difficulty = HabitDifficulty.Easy, ResetCounter = HabitResetCounter.Daily });

        Assert.Empty(toastService.Messages);
    }

    private static (DashboardState State, ToastService Toasts) CreateState(ISender sender)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        var provider = services.BuildServiceProvider();

        var toastService = new ToastService(provider.GetRequiredService<IStringLocalizer<SharedResources>>());
        var store = new BeeDayWebService(sender);
        var state = new DashboardState(store, toastService, provider.GetRequiredService<IStringLocalizer<DashboardResources>>());

        return (state, toastService);
    }

    private sealed class RecordingSender : ISender
    {
        public CancellationToken LastToken { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastToken = cancellationToken;
            return request switch
            {
                GetDashboardQuery => Task.FromResult((TResponse)(object)EmptyDashboard()),
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

        private static DashboardResponse EmptyDashboard() => new(
            new UserProfileSummary(Guid.Empty, "test", "Test", "", UserLanguage.English, UserTheme.Light, 0, 1, 0, 100),
            [], [], [], null);
    }

    private sealed class CancelingSender : ISender
    {
        public Action? CancelDuringSend { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            CancelDuringSend?.Invoke();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
