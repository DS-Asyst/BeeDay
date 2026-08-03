using Microsoft.AspNetCore.HttpOverrides;

namespace BeeDay.Web.Configuration;

public sealed class ProductionHostingOptions
{
    public const string SectionName = "LevelUp:Hosting";

    public string DataProtectionKeysDirectory { get; set; } = "DataProtection-Keys";
    public ForwardedHeadersConfiguration ForwardedHeaders { get; set; } = new();
}

public sealed class ForwardedHeadersConfiguration
{
    public bool Enabled { get; set; }
    public int ForwardLimit { get; set; } = 1;
    public string[] KnownProxies { get; set; } = [];
    public string[] KnownNetworks { get; set; } = [];

    public ForwardedHeaders Headers => Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
        | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
        | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost;
}
