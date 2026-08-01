using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;

namespace LevelUp.Domain.Entities;

/// <summary>
/// Presentation-facing view of a <see cref="User"/>: nickname, display name, avatar,
/// preferences, and progress. Carries no authentication or security state.
/// </summary>
/// <remarks>
/// Physically still backed by fields stored on <see cref="User"/> for JSON persistence
/// compatibility — splitting Profile into a separately-persisted aggregate would change the
/// current JSON document shape, which is out of scope while JSON remains the temporary
/// adapter. That physical split becomes natural once relational persistence introduces
/// separate tables for Identity and Profile.
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
