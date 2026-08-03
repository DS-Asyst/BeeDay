namespace BeeDay.Infrastructure.Configuration;

public sealed class ResendOptions
{
    public const string SectionName = "BeeDay:Email:Resend";

    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string FromName { get; set; } = "BeeDay";
    public string FromAddress { get; set; } = string.Empty;
}
