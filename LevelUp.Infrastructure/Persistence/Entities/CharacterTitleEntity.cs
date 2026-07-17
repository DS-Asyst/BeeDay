namespace LevelUp.Infrastructure.Persistence.Entities;

public sealed class CharacterTitleEntity
{
    public int Id { get; set; }
    public int CharacterId { get; set; } = 1;
    public string Title { get; set; } = string.Empty;
}
