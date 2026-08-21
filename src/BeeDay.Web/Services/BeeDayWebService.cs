using BeeDay.Application.Features.Dashboard.Queries;
using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Application.Features.Habits.Commands;
using BeeDay.Application.Features.Ordering.Commands;
using BeeDay.Application.Features.Ordering.Requests;
using BeeDay.Application.Features.Projects.Commands;
using BeeDay.Application.Features.Tasks.Commands;
using BeeDay.Application.Features.Todos.Commands;
using BeeDay.Application.Features.Users.Commands;
using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Habits.Models;
using BeeDay.Web.Components.Features.Projects.Models;
using BeeDay.Web.Components.Features.Tasks.Models;
using BeeDay.Web.Components.Features.Todos.Models;
using MediatR;
namespace BeeDay.Web.Services;

public sealed class BeeDayWebService(ISender sender)
{
    public Task<CurrentUserResponse?> GetCurrentUserAsync(CancellationToken cancellationToken = default) => sender.Send(new GetCurrentUserQuery(), cancellationToken);
    public Task<DashboardResponse> LoadDashboardAsync(CancellationToken cancellationToken = default) => sender.Send(new GetDashboardQuery(), cancellationToken);
    public Task<Guid> CreateAccountAsync(string name, string email, string password, string nickname, string? avatar = null, CancellationToken cancellationToken = default) =>
        sender.Send(new CreateAccountCommand(new(name, email, password, nickname, avatar)), cancellationToken);
    public Task<Guid> CreateUserAsync(string name, string email, string password, CancellationToken cancellationToken = default) => sender.Send(new CreateUserCommand(new(name, email, password)), cancellationToken);
    public Task CompleteUserProfileAsync(string fullName, string nickname, string? avatar = null, CancellationToken cancellationToken = default) => sender.Send(new CompleteUserProfileCommand(new(fullName, nickname, avatar)), cancellationToken);
    public Task UpdateUserAsync(string name, string email, string currentPassword = "", CancellationToken cancellationToken = default) => sender.Send(new UpdateCurrentUserAccountCommand(new(name, email, currentPassword)), cancellationToken);
    public Task UpdatePreferencesAsync(UserLanguage language, UserTheme theme, CancellationToken cancellationToken = default) => sender.Send(new UpdateCurrentUserPreferencesCommand(new(language, theme)), cancellationToken);
    public Task CompleteOnboardingAsync(CancellationToken cancellationToken = default) => sender.Send(new CompleteCurrentUserOnboardingCommand(), cancellationToken);
    public Task ChangePasswordAsync(string currentPassword, string newPassword, string confirmNewPassword, CancellationToken cancellationToken = default) => sender.Send(new ChangeCurrentUserPasswordCommand(new(currentPassword, newPassword, confirmNewPassword)), cancellationToken);
    public Task AddHabitAsync(HabitEditorModel m, CancellationToken cancellationToken = default) => sender.Send(new CreateHabitCommand(new(m.Title, m.Description, m.Direction, m.Difficulty, m.ResetCounter, m.Attribute)), cancellationToken);
    public Task UpdateHabitAsync(Guid id, HabitEditorModel m, CancellationToken cancellationToken = default) => sender.Send(new UpdateHabitCommand(id, new(m.Title, m.Description, m.Direction, m.Difficulty, m.ResetCounter, m.Attribute)), cancellationToken);
    public Task AddTaskAsync(TaskEditorModel m, CancellationToken cancellationToken = default) => sender.Send(new CreateTaskCommand(new(m.Title, m.Description, m.Repeat, m.Attribute)), cancellationToken);
    public Task UpdateTaskAsync(Guid id, TaskEditorModel m, CancellationToken cancellationToken = default) => sender.Send(new UpdateTaskCommand(id, new(m.Title, m.Description, m.Repeat, m.Attribute)), cancellationToken);
    public Task AddTodoAsync(TodoEditorModel m, CancellationToken cancellationToken = default) => sender.Send(new CreateTodoCommand(new(m.ProjectId ?? Guid.Empty, m.Title, m.Description, ToDateOnly(m.DueDate), m.Attribute)), cancellationToken);
    public Task UpdateTodoAsync(Guid id, TodoEditorModel m, CancellationToken cancellationToken = default) => sender.Send(new UpdateTodoCommand(id, new(m.ProjectId ?? Guid.Empty, m.Title, m.Description, ToDateOnly(m.DueDate), m.Attribute)), cancellationToken);
    public Task AddProjectAsync(ProjectEditorModel m, CancellationToken cancellationToken = default) => sender.Send(new CreateProjectCommand(new(m.Title, m.Description, m.Color, ToDateOnly(m.ExpectedDate), m.Archived, m.Attribute)), cancellationToken);
    public Task UpdateProjectAsync(Guid id, ProjectEditorModel m, CancellationToken cancellationToken = default) => sender.Send(new UpdateProjectCommand(id, new(m.Title, m.Description, m.Color, ToDateOnly(m.ExpectedDate), m.Archived, m.Attribute)), cancellationToken);
    public Task RegisterHabitPositiveAsync(Guid id, CancellationToken cancellationToken = default) => sender.Send(new RegisterHabitPositiveCommand(id), cancellationToken);
    public Task RegisterHabitNegativeAsync(Guid id, CancellationToken cancellationToken = default) => sender.Send(new RegisterHabitNegativeCommand(id), cancellationToken);
    public Task ToggleTaskAsync(Guid id, CancellationToken cancellationToken = default) => sender.Send(new ToggleTaskCommand(id), cancellationToken);
    public Task ToggleTodoAsync(Guid id, CancellationToken cancellationToken = default) => sender.Send(new ToggleTodoCommand(id), cancellationToken);
    public Task DeleteHabitAsync(Guid id, CancellationToken cancellationToken = default) => sender.Send(new DeleteHabitCommand(id), cancellationToken);
    public Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default) => sender.Send(new DeleteTaskCommand(id), cancellationToken);
    public Task DeleteTodoAsync(Guid id, CancellationToken cancellationToken = default) => sender.Send(new DeleteTodoCommand(id), cancellationToken);
    public Task DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default) => sender.Send(new DeleteProjectCommand(id), cancellationToken);
    public Task ReorderAsync(ActivityCollection c, IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default) => sender.Send(new ReorderActivitiesCommand(new(c, ids)), cancellationToken);
    private static DateOnly? ToDateOnly(DateTime? value) => value is null ? null : DateOnly.FromDateTime(value.Value);
}
