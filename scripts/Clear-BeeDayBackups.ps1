# EPIC 30, Sprint 30.25 (BD30-F017): docs/deployment/04-operations.md has documented, since Sprint
# 30.3, that Deploy-BeeDay.ps1 creates a full application + data backup on every deploy
# (C:\Apps\BeeDay-Backups\Application\BeeDay-<timestamp>, ...\Data\BeeDay-Data-<timestamp>) but never
# deletes old ones - left alone, they accumulate indefinitely, one pair per deploy, forever.
#
# This script is a small, standalone, idempotent retention tool - same pattern as the existing
# Clear-BeeDayStdoutLogs.ps1, deliberately NOT wired into Deploy-BeeDay.ps1's own critical deploy/
# rollback path, so a failure here can never affect a deployment. Unlike the log cleaner, each backup
# here is a full directory tree (Copy-DirectoryContents), not a single file, and the timestamp is
# parsed from the directory's own name (matching Deploy-BeeDay.ps1's `Get-Date -Format
# "yyyyMMdd-HHmmss"` exactly) rather than relied on from filesystem metadata, which can be ambiguous
# for a directory whose files were all copied at slightly different instants during the backup.
#
# -MinimumToKeep is a deliberate safety net beyond what Clear-BeeDayStdoutLogs.ps1 needs: a stale log
# file is disposable, but a backup pair is the only rollback material this repository's deploy
# process has (see BD30-F016 - there is no independent SQL Server backup yet). Even if every backup
# pair is older than -RetentionDays (e.g. after a long idle period with no deploys), the N most
# recent pairs are always kept, never zero.
#
# Run it manually, or schedule it (e.g. a Windows Scheduled Task) - this repository does not
# provision that scheduling itself, matching Clear-BeeDayStdoutLogs.ps1's own operational model.
#
# Usage:
#   powershell -File scripts/Clear-BeeDayBackups.ps1 -BackupRoot "C:\Apps\BeeDay-Backups"
#   powershell -File scripts/Clear-BeeDayBackups.ps1 -BackupRoot "C:\Apps\BeeDay-Backups" -RetentionDays 14 -MinimumToKeep 5 -WhatIf

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$BackupRoot,

    [ValidateRange(1, 3650)]
    [int]$RetentionDays = 30,

    [ValidateRange(0, 1000)]
    [int]$MinimumToKeep = 3
)

$ErrorActionPreference = "Stop"

function Get-StaleBackupDirectories {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$Filter,

        [Parameter(Mandatory = $true)]
        [string]$TimestampPrefix,

        [Parameter(Mandatory = $true)]
        [datetime]$Cutoff,

        [Parameter(Mandatory = $true)]
        [int]$MinimumToKeep
    )

    if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
        return @()
    }

    $candidates = @(Get-ChildItem -LiteralPath $Root -Filter $Filter -Directory -ErrorAction SilentlyContinue |
        ForEach-Object {
            $timestampText = $_.Name.Substring($TimestampPrefix.Length)
            $parsed = [datetime]::MinValue
            if ([datetime]::TryParseExact(
                    $timestampText, "yyyyMMdd-HHmmss", $null,
                    [System.Globalization.DateTimeStyles]::None, [ref]$parsed)) {
                [PSCustomObject]@{ Directory = $_; Timestamp = $parsed }
            }
            # A directory that doesn't match Deploy-BeeDay.ps1's own naming convention is left alone
            # entirely - never guessed at, never deleted based on filesystem metadata instead.
        } | Sort-Object -Property Timestamp -Descending)

    if ($candidates.Count -le $MinimumToKeep) {
        return @()
    }

    return @($candidates | Select-Object -Skip $MinimumToKeep | Where-Object { $_.Timestamp -lt $Cutoff })
}

$cutoff = (Get-Date).AddDays(-$RetentionDays)
$applicationRoot = Join-Path $BackupRoot "Application"
$dataRoot = Join-Path $BackupRoot "Data"

$staleApplicationBackups = Get-StaleBackupDirectories -Root $applicationRoot -Filter "BeeDay-*" `
    -TimestampPrefix "BeeDay-" -Cutoff $cutoff -MinimumToKeep $MinimumToKeep
$staleDataBackups = Get-StaleBackupDirectories -Root $dataRoot -Filter "BeeDay-Data-*" `
    -TimestampPrefix "BeeDay-Data-" -Cutoff $cutoff -MinimumToKeep $MinimumToKeep

$staleBackups = @($staleApplicationBackups) + @($staleDataBackups)

if ($staleBackups.Count -eq 0) {
    Write-Host "No backup directories older than $RetentionDays day(s) found beyond the $MinimumToKeep most recent in '$BackupRoot'."
    return
}

$removed = 0
foreach ($backup in $staleBackups) {
    if ($PSCmdlet.ShouldProcess($backup.Directory.FullName, "Remove stale backup directory")) {
        Remove-Item -LiteralPath $backup.Directory.FullName -Recurse -Force
        $removed++
    }
}

Write-Host "Removed $removed stale backup director(y/ies) older than $RetentionDays day(s) from '$BackupRoot' (keeping at least $MinimumToKeep most recent of each kind)."
