using BeeDay.Application.Features.Dashboard.Queries;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Dashboard;
using BeeDay.Web.Components.Features.Dashboard.State;
using BeeDay.Web.Resources;
using BeeDay.Web.Services;
using BeeDay.Web.Tests.Localization;
using MediatR;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Components.Dashboard;

/// <summary>
/// Covers DashboardState's culture-aware feedback messages — the toast strings produced by the
/// presentation/state layer, not the Application-layer request handling itself.
/// </summary>
public sealed class DashboardStateTests
{
    [Fact]
    public async Task UnderEnglishUiCulture_LoadFailureShowsTheEnglishErrorToast()
    {
        var (state, toastService) = CreateState(new ThrowingSender());

        await BunitLocalizationSupport.WithUiCultureAsync("en-US", state.InitializeAsync);

        Assert.Equal("The dashboard data could not be loaded. Try refreshing the page.", toastService.Messages.Single().Message);
    }

    [Fact]
    public async Task UnderPortugueseUiCulture_LoadFailureShowsThePortugueseErrorToast()
    {
        var (state, toastService) = CreateState(new ThrowingSender());

        await BunitLocalizationSupport.WithUiCultureAsync("pt-BR", state.InitializeAsync);

        Assert.Equal("Não foi possível carregar os dados do painel. Tente atualizar a página.", toastService.Messages.Single().Message);
    }

    [Theory]
    [InlineData("en-US", "No repeat", "No due date", "In progress")]
    [InlineData("pt-BR", "Sem repetição", "Sem data de vencimento", "Em andamento")]
    public void FormattersRespectTheCurrentUiCulture(string culture, string noRepeat, string noDueDate, string inProgress)
    {
        var (state, _) = CreateState(new ThrowingSender());

        BunitLocalizationSupport.WithUiCulture(culture, () =>
        {
            Assert.Equal(noRepeat, state.FormatRepeat(TaskRepeat.None));
            Assert.Equal(noDueDate, state.FormatDueDate(null));
            Assert.Equal(inProgress, state.FormatProjectStatus(ProjectStatus.InProgress));
        });
    }

    [Theory]
    [InlineData("en-US", "Daily", "Weekly", "Monthly", "Planned", "Completed")]
    [InlineData("pt-BR", "Diariamente", "Semanalmente", "Mensalmente", "Planejado", "Concluído")]
    public void FormattersNeverFallBackToTheRawEnumNameForDefinedValues(
        string culture, string daily, string weekly, string monthly, string planned, string completed)
    {
        var (state, _) = CreateState(new ThrowingSender());

        BunitLocalizationSupport.WithUiCulture(culture, () =>
        {
            Assert.Equal(daily, state.FormatRepeat(TaskRepeat.Daily));
            Assert.Equal(weekly, state.FormatRepeat(TaskRepeat.Weekly));
            Assert.Equal(monthly, state.FormatRepeat(TaskRepeat.Monthly));
            Assert.Equal(planned, state.FormatProjectStatus(ProjectStatus.Planned));
            Assert.Equal(completed, state.FormatProjectStatus(ProjectStatus.Completed));
        });
    }

    [Theory]
    [InlineData("en-US", "3/5/2026")]
    [InlineData("pt-BR", "05/03/2026")]
    public void FormatDueDate_UsesTheStandardShortDatePatternForTheCurrentCulture(string culture, string expectedDisplayDate)
    {
        // DateOnly.ToString("d") — the standard short-date pattern — rather than a custom
        // "MMM dd, yyyy" pattern: a custom format string fixes day/month/year order regardless of
        // culture. "d" adapts the whole structure, so en-US (month/day/year) and pt-BR (day/month/
        // year) genuinely differ here, matching the same fix already applied to Wallet transaction
        // dates in Sprint 23.6.
        var (state, _) = CreateState(new ThrowingSender());

        BunitLocalizationSupport.WithUiCulture(culture, () =>
        {
            Assert.Equal(expectedDisplayDate, state.FormatDueDate(new DateOnly(2026, 3, 5)));
        });
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

    private sealed class ThrowingSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetDashboardQuery)
            {
                throw new InvalidOperationException("Simulated dashboard load failure.");
            }

            throw new NotSupportedException($"Unexpected request: {request.GetType().Name}");
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
