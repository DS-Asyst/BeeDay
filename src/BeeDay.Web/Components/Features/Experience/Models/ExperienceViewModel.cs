using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Experience;

namespace BeeDay.Web.Components.Features.Experience.Models;

public sealed record ExperienceViewModel(
    int Level,
    long CurrentExperience,
    long RequiredExperience,
    long RemainingExperience)
{
    public double ProgressPercentage => RequiredExperience <= 0
        ? 100d
        : Math.Clamp(CurrentExperience * 100d / RequiredExperience, 0d, 100d);

    public static ExperienceViewModel From(UserExperience experience)
    {
        ArgumentNullException.ThrowIfNull(experience);

        return new ExperienceViewModel(
            experience.CurrentLevel,
            experience.CurrentLevelExperience,
            experience.ExperienceRequiredForCurrentLevel,
            experience.ExperienceForNextLevel);
    }

    public static ExperienceViewModel From(UserProfileSummary profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new ExperienceViewModel(
            profile.CurrentLevel,
            profile.CurrentLevelExperience,
            profile.ExperienceRequiredForCurrentLevel,
            profile.ExperienceRequiredForCurrentLevel - profile.CurrentLevelExperience);
    }
}
