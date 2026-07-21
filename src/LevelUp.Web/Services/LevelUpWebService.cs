using LevelUp.Application.Features.Dashboard.Contracts;
using LevelUp.Application.Features.Habits.Contracts;
using LevelUp.Application.Features.Habits.Requests;
using LevelUp.Application.Features.Profiles.Contracts;
using LevelUp.Application.Features.Profiles.Requests;
using LevelUp.Application.Features.Projects.Contracts;
using LevelUp.Application.Features.Projects.Requests;
using LevelUp.Application.Features.Tasks.Contracts;
using LevelUp.Application.Features.Tasks.Requests;
using LevelUp.Application.Features.Todos.Contracts;
using LevelUp.Application.Features.Todos.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Web.Models;

namespace LevelUp.Web.Services;

public sealed class LevelUpWebService(
    ILevelUpQueryService queryService,
    IProfileService profileService,
    IHabitService habitService,
    ITaskService taskService,
    ITodoService todoService,
    IProjectService projectService)
{
    public async Task<LevelUpData> LoadAsync()
    {
        var response = await queryService.GetAsync();
        return response.Data;
    }

    public Task CreateProfileAsync(string name, string nickname, CharacterClass characterClass) =>
        profileService.SaveAsync(new SaveProfileRequest(name, nickname, characterClass));

    public Task AddHabitAsync(HabitEditorModel model) =>
        habitService.AddAsync(new SaveHabitRequest(
            model.Title,
            model.Description,
            model.Direction,
            model.Difficulty,
            model.ResetCounter));

    public Task UpdateHabitAsync(Guid id, HabitEditorModel model) =>
        habitService.UpdateAsync(id, new SaveHabitRequest(
            model.Title,
            model.Description,
            model.Direction,
            model.Difficulty,
            model.ResetCounter));

    public Task AddTaskAsync(ActivityEditorModel model) =>
        taskService.AddAsync(new SaveTaskRequest(model.Title, model.Description, model.Repeat));

    public Task UpdateTaskAsync(Guid id, ActivityEditorModel model) =>
        taskService.UpdateAsync(id, new SaveTaskRequest(model.Title, model.Description, model.Repeat));

    public Task AddTodoAsync(ActivityEditorModel model) =>
        todoService.AddAsync(new SaveTodoRequest(model.Title, model.Description, ToDateOnly(model.DueDate)));

    public Task UpdateTodoAsync(Guid id, ActivityEditorModel model) =>
        todoService.UpdateAsync(id, new SaveTodoRequest(model.Title, model.Description, ToDateOnly(model.DueDate)));

    public Task AddProjectAsync(ActivityEditorModel model) =>
        projectService.AddAsync(new SaveProjectRequest(model.Title, model.Description, model.ProjectStatus));

    public Task UpdateProjectAsync(Guid id, ActivityEditorModel model) =>
        projectService.UpdateAsync(id, new SaveProjectRequest(model.Title, model.Description, model.ProjectStatus));

    public Task RegisterHabitPositiveAsync(Guid id) => habitService.RegisterPositiveAsync(id);
    public Task RegisterHabitNegativeAsync(Guid id) => habitService.RegisterNegativeAsync(id);
    public Task ToggleTaskAsync(Guid id) => taskService.ToggleAsync(id);
    public Task ToggleTodoAsync(Guid id) => todoService.ToggleAsync(id);
    public Task ToggleProjectAsync(Guid id) => projectService.ToggleAsync(id);
    public Task DeleteHabitAsync(Guid id) => habitService.DeleteAsync(id);
    public Task DeleteTaskAsync(Guid id) => taskService.DeleteAsync(id);
    public Task DeleteTodoAsync(Guid id) => todoService.DeleteAsync(id);
    public Task DeleteProjectAsync(Guid id) => projectService.DeleteAsync(id);

    private static DateOnly? ToDateOnly(DateTime? value) =>
        value is null ? null : DateOnly.FromDateTime(value.Value);
}
