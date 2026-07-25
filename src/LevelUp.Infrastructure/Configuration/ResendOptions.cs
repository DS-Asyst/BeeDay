namespace LevelUp.Infrastructure.Configuration;

public sealed class ResendOptions
{
    public const string SectionName = "LevelUp:Email:Resend";

    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string FromName { get; set; } = "LevelUp";
    public string FromAddress { get; set; } = string.Empty;
}
