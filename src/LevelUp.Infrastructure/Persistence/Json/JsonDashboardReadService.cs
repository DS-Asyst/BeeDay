using LevelUp.Application.Features.Dashboard.Contracts;
using LevelUp.Application.Features.Dashboard.Responses;
using LevelUp.Application.Features.Wallets.Responses;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Infrastructure.Persistence.Json;

/// <summary>
/// Temporary JSON adapter for <see cref="IDashboardReadService"/>. Reproduces the per-user
/// projection that <c>LevelUpData.CreateUserSnapshot</c> used to build for
/// <c>GetLevelUpResponse</c>, now shaped into <see cref="DashboardResponse"/> instead of exposing
/// <c>LevelUpData</c>.
/// </summary>
internal sealed class JsonDashboardReadService(JsonLevelUpDocumentStore store) : IDashboardReadService
{
    public async Task<DashboardResponse> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var data = await store.LoadAsync(cancellationToken);
        var user = data.Users.FirstOrDefault(candidate => candidate.Id == userId)
            ?? throw new InvalidDomainStateException($"User '{userId}' was not found.");

        var habits = data.Habits.Where(habit => habit.UserId == userId).Select(MapHabit).ToList();
        var tasks = data.Tasks.Where(task => task.UserId == userId).Select(MapTask).ToList();
        var projects = data.Projects.Where(project => project.UserId == userId).Select(MapProject).ToList();
        var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == userId);

        return new DashboardResponse(
            MapProfile(user),
            habits,
            tasks,
            projects,
            wallet is null ? null : MapWalletSummary(wallet, data));
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

    private static WalletSummaryResponse MapWalletSummary(Wallet wallet, LevelUpData data)
    {
        var transactions = data.Transactions.Where(transaction => transaction.WalletId == wallet.Id).ToList();
        return new WalletSummaryResponse(
            wallet.Id,
            wallet.CalculateBalance(transactions),
            wallet.CalculateTotalIncome(transactions),
            wallet.CalculateTotalExpenses(transactions),
            transactions.Count,
            wallet.UpdatedAtUtc);
    }
}
