# EPIC 28, Sprint 28.7 (Observability Operationalization): docs/deployment/03-observability.md §7
# has documented, since before this Epic, that no rotation/retention policy exists for the IIS
# stdout log directory (web.config's stdoutLogFile, C:\Apps\BeeDay-Data\Logs on HMG) or for the
# Event Journal. ASP.NET Core Module (ANCM) creates a fresh timestamped file per process start
# (e.g. stdout_20260817053012_9948.log) but never deletes old ones - left alone, they accumulate
# indefinitely.
#
# This script is a small, standalone, idempotent retention tool - deliberately NOT wired into
# scripts/Deploy-BeeDay.ps1's own critical deploy/rollback path, so a failure here can never affect
# a deployment. It only ever deletes files matching ANCM's own "stdout_*.log" naming convention,
# only inside the one directory it is given, and only those older than -RetentionDays. Run it
# manually, or schedule it (e.g. a Windows Scheduled Task) - this repository does not provision
# that scheduling itself; see docs/deployment/03-observability.md §7 for the operational procedure.
#
# Usage:
#   powershell -File scripts/Clear-BeeDayStdoutLogs.ps1 -Directory "C:\Apps\BeeDay-Data\Logs"
#   powershell -File scripts/Clear-BeeDayStdoutLogs.ps1 -Directory "C:\Apps\BeeDay-Data\Logs" -RetentionDays 14 -WhatIf

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Directory,

    [ValidateRange(1, 3650)]
    [int]$RetentionDays = 30
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $Directory -PathType Container)) {
    Write-Host "Stdout log directory '$Directory' does not exist yet - nothing to clean up."
    return
}

$cutoff = (Get-Date).AddDays(-$RetentionDays)
$staleFiles = @(Get-ChildItem -LiteralPath $Directory -Filter "stdout_*.log" -File -ErrorAction SilentlyContinue |
    Where-Object { $_.LastWriteTimeUtc -lt $cutoff.ToUniversalTime() })

if ($staleFiles.Count -eq 0) {
    Write-Host "No stdout log files older than $RetentionDays day(s) found in '$Directory'."
    return
}

$removed = 0
foreach ($file in $staleFiles) {
    if ($PSCmdlet.ShouldProcess($file.FullName, "Remove stale stdout log file")) {
        Remove-Item -LiteralPath $file.FullName -Force
        $removed++
    }
}

Write-Host "Removed $removed stale stdout log file(s) older than $RetentionDays day(s) from '$Directory'."
