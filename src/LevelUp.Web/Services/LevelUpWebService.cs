using LevelUp.Application.Features.Dashboard.Queries;
using LevelUp.Application.Features.Dashboard.Responses;
using LevelUp.Application.Features.Habits.Commands;
using LevelUp.Application.Features.Ordering.Commands;
using LevelUp.Application.Features.Ordering.Requests;
using LevelUp.Application.Features.Projects.Commands;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Application.Features.Todos.Commands;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Queries;
using LevelUp.Application.Features.Users.Responses;
using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Habits.Models;
using LevelUp.Web.Components.Features.Projects.Models;
using LevelUp.Web.Components.Features.Tasks.Models;
using LevelUp.Web.Components.Features.Todos.Models;
using MediatR;
namespace LevelUp.Web.Services;

public sealed class LevelUpWebService(ISender sender)
{
    public Task<CurrentUserResponse?> GetCurrentUserAsync() => sender.Send(new GetCurrentUserQuery());
    public Task<DashboardResponse> LoadDashboardAsync() => sender.Send(new GetDashboardQuery());
    public Task<Guid> CreateAccountAsync(string name, string email, string password, string nickname, string? avatar = null) =>
        sender.Send(new CreateAccountCommand(new(name, email, password, nickname, avatar)));
    public Task<Guid> CreateUserAsync(string name, string email, string password) => sender.Send(new CreateUserCommand(new(name, email, password)));
    public Task CompleteUserProfileAsync(string fullName, string nickname, string? avatar = null) => sender.Send(new CompleteUserProfileCommand(new(fullName, nickname, avatar)));
    public Task UpdateUserAsync(string name, string email) => sender.Send(new UpdateCurrentUserAccountCommand(new(name, email)));
    public Task UpdatePreferencesAsync(UserLanguage language, UserTheme theme) => sender.Send(new UpdateCurrentUserPreferencesCommand(new(language, theme)));
    public Task CompleteOnboardingAsync() => sender.Send(new CompleteCurrentUserOnboardingCommand());
    public Task ChangePasswordAsync(string currentPassword, string newPassword, string confirmNewPassword) => sender.Send(new ChangeCurrentUserPasswordCommand(new(currentPassword, newPassword, confirmNewPassword)));
    public Task AddHabitAsync(HabitEditorModel m) => sender.Send(new CreateHabitCommand(new(m.Title, m.Description, m.Direction, m.Difficulty, m.ResetCounter, m.Attribute)));
    public Task UpdateHabitAsync(Guid id, HabitEditorModel m) => sender.Send(new UpdateHabitCommand(id, new(m.Title, m.Description, m.Direction, m.Difficulty, m.ResetCounter, m.Attribute)));
    public Task AddTaskAsync(TaskEditorModel m) => sender.Send(new CreateTaskCommand(new(m.Title, m.Description, m.Repeat, m.Attribute)));
    public Task UpdateTaskAsync(Guid id, TaskEditorModel m) => sender.Send(new UpdateTaskCommand(id, new(m.Title, m.Description, m.Repeat, m.Attribute)));
    public Task AddTodoAsync(TodoEditorModel m) => sender.Send(new CreateTodoCommand(new(m.ProjectId ?? Guid.Empty, m.Title, m.Description, ToDateOnly(m.DueDate), m.Attribute)));
    public Task UpdateTodoAsync(Guid id, TodoEditorModel m) => sender.Send(new UpdateTodoCommand(id, new(m.ProjectId ?? Guid.Empty, m.Title, m.Description, ToDateOnly(m.DueDate), m.Attribute)));
    public Task AddProjectAsync(ProjectEditorModel m) => sender.Send(new CreateProjectCommand(new(m.Title, m.Description, m.Color, ToDateOnly(m.ExpectedDate), m.Archived, m.Attribute)));
    public Task UpdateProjectAsync(Guid id, ProjectEditorModel m) => sender.Send(new UpdateProjectCommand(id, new(m.Title, m.Description, m.Color, ToDateOnly(m.ExpectedDate), m.Archived, m.Attribute)));
    public Task RegisterHabitPositiveAsync(Guid id) => sender.Send(new RegisterHabitPositiveCommand(id)); public Task RegisterHabitNegativeAsync(Guid id) => sender.Send(new RegisterHabitNegativeCommand(id));
    public Task ToggleTaskAsync(Guid id) => sender.Send(new ToggleTaskCommand(id)); public Task ToggleTodoAsync(Guid id) => sender.Send(new ToggleTodoCommand(id));
    public Task DeleteHabitAsync(Guid id) => sender.Send(new DeleteHabitCommand(id)); public Task DeleteTaskAsync(Guid id) => sender.Send(new DeleteTaskCommand(id)); public Task DeleteTodoAsync(Guid id) => sender.Send(new DeleteTodoCommand(id)); public Task DeleteProjectAsync(Guid id) => sender.Send(new DeleteProjectCommand(id));
    public Task ReorderAsync(ActivityCollection c, IReadOnlyList<Guid> ids) => sender.Send(new ReorderActivitiesCommand(new(c, ids)));
    private static DateOnly? ToDateOnly(DateTime? value) => value is null ? null : DateOnly.FromDateTime(value.Value);
}
