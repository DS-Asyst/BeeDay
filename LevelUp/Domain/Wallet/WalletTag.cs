
namespace LevelUp.Domain.Wallet;

public sealed class WalletTag
{
    public int Id { get; set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime? UpdatedAt { get; private set; }

    public void Configure(string name)
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("A tag já foi configurada.");
        }

        SetName(name);
    }

    public void UpdateName(string name)
    {
        SetName(name);
        UpdatedAt = DateTime.Now;
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }
}
