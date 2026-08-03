using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Used by tests that need to observe the real content of an outgoing identity email (password
/// reset, email confirmation) — e.g. to recover the raw token from the same link a real recipient
/// would click. Enables the app's own <c>DevelopmentEmailSender</c> (the production capture-to-disk
/// implementation, not a test double) with a unique, disposable subdirectory per factory instance,
/// so parallel tests never see each other's captured emails.
/// </summary>
/// <remarks>
/// <c>DevelopmentEmailSender</c> refuses to write outside the app's content root, so the capture
/// directory must be a relative subpath under it rather than a %TEMP% path — cleaned up on Dispose.
/// </remarks>
public class EmailCaptureWebApplicationFactory : BeeDayWebApplicationFactory
{
    private static readonly Regex TokenPattern = new("[?&]token=([^&\"]+)", RegexOptions.Compiled);

    private readonly string emailDirectoryRelativePath =
        Path.Combine("App_Data", "test-email-capture", Guid.NewGuid().ToString("N"));

    protected override IReadOnlyDictionary<string, string?> AdditionalConfiguration { get; }

    public EmailCaptureWebApplicationFactory()
    {
        AdditionalConfiguration = new Dictionary<string, string?>
        {
            ["BeeDay:Email:Development:Enabled"] = "true",
            ["BeeDay:Email:Development:Directory"] = emailDirectoryRelativePath
        };
    }

    // Cached on first access: by the time Dispose runs (via WebApplicationFactory's DisposeAsync),
    // the underlying host's Services is already torn down, so ContentRootPath can no longer be
    // resolved from DI. Every test using this factory touches Services well before disposal
    // (seeding a user, sending a command), so the cache is always populated in time.
    private string? cachedCaptureDirectory;

    private string CaptureDirectory => cachedCaptureDirectory ??=
        Path.Combine(Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath, emailDirectoryRelativePath);

    /// <summary>
    /// Waits (briefly; capture is fire-and-forget from the handler's perspective but is awaited by
    /// the caller in these tests before this is invoked) then returns the raw token embedded in the
    /// most recently captured email's action link, or null if no email was captured.
    /// </summary>
    public async Task<string?> TryGetLatestCapturedTokenAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(CaptureDirectory))
        {
            return null;
        }

        var latest = new DirectoryInfo(CaptureDirectory)
            .GetFiles("*.html")
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();
        if (latest is null)
        {
            return null;
        }

        var html = await File.ReadAllTextAsync(latest.FullName, cancellationToken);
        var match = TokenPattern.Match(html);
        return match.Success ? Uri.UnescapeDataString(match.Groups[1].Value) : null;
    }

    public int CountCapturedEmails() =>
        Directory.Exists(CaptureDirectory) ? new DirectoryInfo(CaptureDirectory).GetFiles("*.html").Length : 0;

    protected override void Dispose(bool disposing)
    {
        var captureDirectory = cachedCaptureDirectory;
        base.Dispose(disposing);

        if (disposing && captureDirectory is not null)
        {
            DeleteBestEffort(captureDirectory);
        }
    }

    // The capture files were just written moments earlier in the same process; on Windows the
    // handle can still be settling, which surfaces as a transient IOException/UnauthorizedAccessException
    // even though nothing is genuinely holding the directory open. A handful of immediate retries
    // clears that up reliably — this is teardown hygiene only, never something a test asserts on.
    private static void DeleteBestEffort(string directory)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            try
            {
                Directory.Delete(directory, recursive: true);
                return;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Thread.Sleep(50);
            }
        }
    }
}
