using BeeDay.Domain.Enums;
using BeeDay.Domain.Experience;

namespace BeeDay.Domain.Entities;

/// <summary>
/// Presentation-facing view of a <see cref="User"/>: nickname, display name, avatar,
/// preferences, and progress. Carries no authentication or security state.
/// </summary>
/// <remarks>
/// This projection remains backed by fields stored on <see cref="User"/> so profile-facing code
/// cannot acquire or mutate authentication and security state through a separate aggregate.
/// </remarks>
public sealed class Profile
{
    public string Nickname { get; }
    public string Name { get; }
    public string Avatar { get; }
    public UserLanguage Language { get; }
    public UserTheme Theme { get; }
    public UserExperience Experience { get; }

    public bool IsComplete => !string.IsNullOrEmpty(Nickname);

    internal Profile(string nickname, string name, string avatar, UserLanguage language, UserTheme theme, UserExperience experience)
    {
        Nickname = nickname;
        Name = name;
        Avatar = avatar;
        Language = language;
        Theme = theme;
        Experience = experience;
    }
}
