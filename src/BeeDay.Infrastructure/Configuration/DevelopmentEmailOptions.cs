namespace BeeDay.Infrastructure.Configuration;

public sealed class DevelopmentEmailOptions
{
    public const string SectionName = "BeeDay:Email:Development";

    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "Data/Emails";
}
