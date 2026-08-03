using BeeDay.Application.Common.Behaviors;
using BeeDay.Application.Common.Experience;
using BeeDay.Application.Common.Identity;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BeeDay.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddLevelUpApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IEmailConfirmationIssuer, EmailConfirmationIssuer>();
        services.AddSingleton<IExperienceRewardPolicy, ExperienceRewardPolicy>();
        services.AddScoped<IExperienceRewardService, ExperienceRewardService>();

        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>());

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainEventBehavior<,>));

        return services;
    }
}
