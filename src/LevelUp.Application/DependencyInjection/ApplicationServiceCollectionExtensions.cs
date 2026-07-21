using LevelUp.Application.Features.Dashboard.Contracts;
using LevelUp.Application.Features.Dashboard.Services;
using LevelUp.Application.Features.Habits.Contracts;
using LevelUp.Application.Features.Habits.Services;
using LevelUp.Application.Features.Profiles.Contracts;
using LevelUp.Application.Features.Profiles.Services;
using LevelUp.Application.Features.Projects.Contracts;
using LevelUp.Application.Features.Projects.Services;
using LevelUp.Application.Features.Tasks.Contracts;
using LevelUp.Application.Features.Tasks.Services;
using LevelUp.Application.Features.Todos.Contracts;
using LevelUp.Application.Features.Todos.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LevelUp.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddLevelUpApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ILevelUpQueryService, LevelUpQueryService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IProjectService, ProjectService>();

        return services;
    }
}
