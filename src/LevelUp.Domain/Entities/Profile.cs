using System.Text.Json.Serialization;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Domain.Entities;

public sealed class Profile
{
    [JsonInclude]
    public string Name { get; private set; } = string.Empty;

    [JsonInclude]
    public string Nickname { get; private set; } = string.Empty;

    [JsonInclude]
    public CharacterClass Class { get; private set; } = CharacterClass.Warrior;

    public static Profile Create(string name, string? nickname, CharacterClass characterClass)
    {
        var profile = new Profile();
        profile.Update(name, nickname, characterClass);
        return profile;
    }

    public void Update(string name, string? nickname, CharacterClass characterClass)
    {
        Name = ProfileName.Create(name).Value;
        Nickname = ProfileNickname.Create(nickname).Value;
        Class = EnumValidation.Defined(characterClass, nameof(characterClass));
    }
}
