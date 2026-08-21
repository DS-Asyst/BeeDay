using BeeDay.Application.Features.Dashboard.Contracts;
using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Application.Features.Wallets.Responses;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer;

/// <summary>
/// SQL Server adapter for <see cref="IDashboardReadService"/> — mirrors
/// <c>JsonDashboardReadService</c> exactly (same projections, same shape), against
/// <see cref="BeeDayDbContext"/> instead of the whole-document JSON store. Every read is
/// <c>AsNoTracking()</c>; ordering of Habits/Tasks/Projects/Todos uses the same <c>Position</c> shadow
/// property the repositories already order by — the equivalent of the JSON document's own list order,
/// since SQL Server has no implicit row order.
/// </summary>
internal sealed class EfDashboardReadService(IDbContextFactory<BeeDayDbContext> contextFactory) : IDashboardReadService
{
    public async Task<DashboardResponse> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(existing => existing.Id == userId, cancellationToken)
            ?? throw new InvalidDomainStateException($"User '{userId}' was not found.");

        var habits = await context.Habits.AsNoTracking()
            .Where(habit => habit.UserId == userId)
            .OrderBy(habit => EF.Property<int>(habit, "Position"))
            .ToListAsync(cancellationToken);

        var tasks = await context.RecurringTasks.AsNoTracking()
            .Where(task => task.UserId == userId)
            .OrderBy(task => EF.Property<int>(task, "Position"))
            .ToListAsync(cancellationToken);

        var projects = await context.Projects.AsNoTracking()
            .Include(project => project.Todos.OrderBy(todo => EF.Property<int>(todo, "Position")))
            .Where(project => project.UserId == userId)
            .OrderBy(project => EF.Property<int>(project, "Position"))
            .ToListAsync(cancellationToken);

        var wallet = await context.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.UserId == userId, cancellationToken);

        WalletSummaryResponse? walletSummary = null;
        if (wallet is not null)
        {
            var walletTransactions = context.Transactions.AsNoTracking()
                .Where(transaction => transaction.WalletId == wallet.Id);

            // Aggregated in SQL (SUM/COUNT) instead of loading every transaction row over the network
            // just to sum them in memory — this query runs on every dashboard load and after every
            // unrelated Habit/Task/Todo/Project mutation (DashboardState.ReloadAsync always reloads
            // the wallet summary too), so the full-row-transfer cost was paid far more often than the
            // wallet page itself is visited.
            var totalIncome = await walletTransactions
                .Where(transaction => transaction.Type == TransactionType.Income)
                .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0m;
            var totalExpenses = await walletTransactions
                .Where(transaction => transaction.Type == TransactionType.Expense)
                .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0m;
            var transactionCount = await walletTransactions.CountAsync(cancellationToken);

            walletSummary = new WalletSummaryResponse(
                wallet.Id,
                totalIncome - totalExpenses,
                totalIncome,
                totalExpenses,
                transactionCount,
                wallet.UpdatedAtUtc);
        }

        return new DashboardResponse(
            MapProfile(user),
            habits.Select(MapHabit).ToList(),
            tasks.Select(MapTask).ToList(),
            projects.Select(MapProject).ToList(),
            walletSummary);
    }

    private static UserProfileSummary MapProfile(User user) => new(
        user.Id,
        user.Nickname,
        user.Name,
        user.Avatar,
        user.Language,
        user.Theme,
        user.Experience.TotalExperience,
        user.Experience.CurrentLevel,
        user.Experience.CurrentLevelExperience,
        user.Experience.ExperienceRequiredForCurrentLevel);

    private static HabitSummary MapHabit(Habit habit) => new(
        habit.Id,
        habit.Title,
        habit.Description,
        habit.Featured,
        habit.Attribute,
        habit.Direction,
        habit.Difficulty,
        habit.ResetCounter,
        habit.PositiveCount,
        habit.NegativeCount,
        habit.CreatedAtUtc,
        habit.UpdatedAtUtc);

    private static TaskSummary MapTask(RecurringTask task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Featured,
        task.Attribute,
        task.Repeat,
        task.Completed,
        task.CreatedAtUtc,
        task.UpdatedAtUtc);

    private static ProjectSummary MapProject(Project project) => new(
        project.Id,
        project.Name,
        project.Description,
        project.Color,
        project.Featured,
        project.Attribute,
        project.ExpectedDate,
        project.Archived,
        project.Status,
        project.ProgressPercentage,
        project.Todos.Select(MapTodo).ToList());

    private static TodoSummary MapTodo(Todo todo) => new(
        todo.Id,
        todo.Title,
        todo.Description,
        todo.ProjectId,
        todo.Featured,
        todo.DueDate,
        todo.Attribute,
        todo.Completed,
        todo.CreatedAtUtc,
        todo.UpdatedAtUtc);
}
