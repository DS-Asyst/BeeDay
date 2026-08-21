using BeeDay.Application.Features.Dashboard.Queries;
using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Behaviors.DragDrop;
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

    // EPIC 30 Sprint 30.14: exercises the acceptance criterion "no stale route or deleted-resource
    // crash remains unhandled" — an open workspace whose Project is deleted from elsewhere (another
    // tab/session) must not leave OpenProject/OpenProjectId dangling once the next reload happens.
    // Was previously true only by code reading (OpenProject re-derives every access); this makes it
    // a regression-proof fact, and also covers the OpenProjectId reset this Sprint added to
    // ReloadAsync for symmetry with the existing selectedProjectId reset.
    [Fact]
    public async Task OpenProjectWorkspace_WhenTheProjectDisappearsOnReload_ClosesWithoutCrashing()
    {
        var projectOne = new ProjectSummary(Guid.NewGuid(), "Project One", "", "#8056C7", false, null, null, false, ProjectStatus.Planned, 0, []);
        var projectTwo = new ProjectSummary(Guid.NewGuid(), "Project Two", "", "#8056C7", false, null, null, false, ProjectStatus.Planned, 0, []);
        var sender = new ReloadingSender([projectOne, projectTwo], [projectTwo]);
        var (state, _) = CreateState(sender);

        await state.InitializeAsync();
        state.OpenProjectWorkspace(projectOne);
        Assert.Equal(projectOne.Id, state.OpenProjectId);
        Assert.Equal(projectOne.Id, state.OpenProject?.Id);

        // Any successful mutation reaches ReloadAsync(); a reorder with 2 projects is the simplest
        // one to drive without touching unrelated command handling.
        await state.ReorderProjectsAsync(new SortableReorderEvent(projectOne.Id.ToString(), projectTwo.Id.ToString(), true));

        Assert.Null(state.OpenProjectId);
        Assert.Null(state.OpenProject);
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

    /// <summary>
    /// Returns <paramref name="first"/>'s Projects on the first GetDashboardQuery and
    /// <paramref name="second"/>'s on every call after — simulates a Project having been deleted
    /// elsewhere between the initial load and the next reload. Any other command is a no-op success.
    /// </summary>
    private sealed class ReloadingSender(IReadOnlyList<ProjectSummary> first, IReadOnlyList<ProjectSummary> second) : ISender
    {
        private int dashboardQueryCount;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetDashboardQuery)
            {
                dashboardQueryCount++;
                var projects = dashboardQueryCount == 1 ? first : second;
                var profile = new UserProfileSummary(Guid.NewGuid(), "tester", "Test User", "", UserLanguage.English, UserTheme.System, 0, 1, 0, 100);
                return Task.FromResult((TResponse)(object)new DashboardResponse(profile, [], [], projects, null));
            }

            throw new NotSupportedException($"Unexpected request: {request.GetType().Name}");
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            Task.CompletedTask;

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
