using System.Diagnostics;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Proves the Program.cs startup guard that rejects a non-absolute/non-HTTPS
/// <c>BeeDay:IdentityEmail:PublicBaseUrl</c> outside Development (Epic 26, Sprint 26.5 — audits
/// <c>PublicBaseUrl</c> as the security-sensitive origin contract for every Identity email callback
/// link). Uninspected before this sprint: no test exercised this guard, the AllowedHosts guard, or
/// the DataProtectionKeysDirectory guard at all.
/// </summary>
/// <remarks>
/// Launches the real, already-built <c>BeeDay.Web.dll</c> as a genuinely separate OS process instead
/// of an in-process <c>WebApplicationFactory</c>. <c>WebApplicationFactory</c> would require mutating
/// process-wide environment variables (the only way to feed a value to Program.cs's guard, which
/// reads <c>builder.Configuration</c> synchronously before <c>Build()</c> — earlier than
/// <c>WebApplicationFactory</c>'s own config overlay takes effect, per the same reasoning documented
/// on <see cref="ProductionLikeWebApplicationFactory"/>) shared with every other test running in this
/// same process. Two different in-process locking designs were tried and both produced intermittent
/// cross-test failures under real xUnit parallel execution that did not reproduce in isolation — see
/// <see cref="ProductionEnvironmentVariableTestLock"/>'s remarks. A genuinely separate child process
/// has its own isolated environment and cannot race with anything in this process, eliminating the
/// problem rather than attempting to synchronize around it.
/// </remarks>
public sealed class ProductionOriginGuardTests
{
    [Fact]
    public Task ProductionEnvironment_WithNonHttpsPublicBaseUrl_FailsToStart() =>
        AssertStartupFailsAsync("http://h-beeday.com.br");

    [Fact]
    public Task ProductionEnvironment_WithRelativePublicBaseUrl_FailsToStart() =>
        AssertStartupFailsAsync("/account/confirm-email");

    [Fact]
    public Task ProductionEnvironment_WithMissingPublicBaseUrl_FailsToStart() =>
        AssertStartupFailsAsync(publicBaseUrl: null);

    private static async Task AssertStartupFailsAsync(string? publicBaseUrl)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(GetWebAssemblyPath())
        };
        startInfo.ArgumentList.Add(GetWebAssemblyPath());

        // A separate OS process gets its own copy of the environment — these overrides cannot leak
        // into, or race with, any other test running in this test process.
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Production";
        if (publicBaseUrl is null)
        {
            startInfo.Environment.Remove("BeeDay__IdentityEmail__PublicBaseUrl");
        }
        else
        {
            startInfo.Environment["BeeDay__IdentityEmail__PublicBaseUrl"] = publicBaseUrl;
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start the BeeDay.Web process under test.");

        var stdErrTask = process.StandardError.ReadToEndAsync();
        var stdOutTask = process.StandardOutput.ReadToEndAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail(
                "BeeDay.Web did not exit within 30s — the PublicBaseUrl guard did not fail startup as expected " +
                "(or the process is hanging on an unrelated concern). This guard is expected to throw " +
                "synchronously, before builder.Build(), before any SQL/network access.");
        }

        var stdErr = await stdErrTask;
        var stdOut = await stdOutTask;

        Assert.NotEqual(0, process.ExitCode);
        Assert.Contains(
            "PublicBaseUrl must be an absolute HTTPS URL",
            stdErr + stdOut,
            StringComparison.Ordinal);
    }

    private static string GetWebAssemblyPath()
    {
        var repositoryRoot = GetRepositoryRoot();
        var configuration = AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";
        var path = Path.Combine(repositoryRoot, "src", "BeeDay.Web", "bin", configuration, "net10.0", "BeeDay.Web.dll");

        return File.Exists(path)
            ? path
            : throw new InvalidOperationException(
                $"Could not find the built BeeDay.Web.dll at '{path}'. Build the solution before running this test.");
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root (BeeDay.slnx) from the test output directory.");
    }
}
