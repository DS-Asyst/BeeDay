using AngleSharp.Html.Parser;

namespace LevelUp.Web.Tests.Integration;

/// <summary>Shared helper for scraping a real antiforgery token out of a server-rendered page.</summary>
internal static class AntiforgeryTestHelper
{
    public static async Task<string> GetTokenAsync(HttpClient client, string path, CancellationToken cancellationToken)
    {
        var html = await client.GetStringAsync(path, cancellationToken);
        var document = await new HtmlParser().ParseDocumentAsync(html, cancellationToken);
        var input = document.QuerySelector("input[name='__RequestVerificationToken']")
            ?? throw new InvalidOperationException($"No antiforgery token field found on '{path}'.");

        return input.GetAttribute("value")
            ?? throw new InvalidOperationException($"Antiforgery token field on '{path}' had no value.");
    }
}
