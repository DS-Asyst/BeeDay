# Regression coverage for scripts/Clear-BeeDayStdoutLogs.ps1 (EPIC 28, Sprint 28.7).
#
# Framework-free, exit-code-driven, matching the existing convention in this folder (no Pester in
# this repository - see Test-DeployBeeDayRecovery.ps1's own note on that). Uses only a real
# temporary directory this script creates and deletes itself - never touches any path under
# C:\Apps or any other real BeeDay location.
#
# Run: powershell -File scripts/tests/Test-ClearBeeDayStdoutLogs.ps1
# Exits 0 when every assertion passes, non-zero otherwise.

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$scriptPath = Join-Path $repoRoot "scripts\Clear-BeeDayStdoutLogs.ps1"

if (-not (Test-Path -LiteralPath $scriptPath)) {
    throw "Cannot find scripts/Clear-BeeDayStdoutLogs.ps1 at expected path: $scriptPath"
}

$script:testCount = 0
$script:failureCount = 0

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    $script:testCount++
    if (-not $Condition) {
        $script:failureCount++
        Write-Host "FAIL: $Message"
    }
    else {
        Write-Host "PASS: $Message"
    }
}

$parseErrors = $null
$tokens = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -gt 0) {
    throw "scripts/Clear-BeeDayStdoutLogs.ps1 has parse errors: $($parseErrors -join '; ')"
}
Assert-True -Condition $true -Message "Script parses without syntax errors"

function New-TestDirectory {
    $dir = Join-Path ([System.IO.Path]::GetTempPath()) ("beeday-stdout-retention-tests-" + [guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    return $dir
}

function New-TestFile {
    param(
        [Parameter(Mandatory = $true)][string]$Directory,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][int]$AgeDays
    )

    $path = Join-Path $Directory $Name
    Set-Content -LiteralPath $path -Value "placeholder" -NoNewline
    $timestamp = (Get-Date).AddDays(-$AgeDays)
    (Get-Item -LiteralPath $path).LastWriteTime = $timestamp
    return $path
}

# ===========================================================================
# Directory does not exist yet - must not throw, nothing to clean up.
# ===========================================================================
$missingDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("beeday-stdout-retention-missing-" + [guid]::NewGuid().ToString("N"))
$missingDirectoryThrew = $false
try {
    & $scriptPath -Directory $missingDirectory -RetentionDays 30 | Out-Null
}
catch {
    $missingDirectoryThrew = $true
}
Assert-True -Condition (-not $missingDirectoryThrew) -Message "A non-existent directory does not throw"

# ===========================================================================
# Only stale stdout_*.log files are removed - recent ones and non-matching
# filenames of any age are always preserved.
# ===========================================================================
$testDir = New-TestDirectory
try {
    $staleStdout = New-TestFile -Directory $testDir -Name "stdout_20260101000000_1111.log" -AgeDays 45
    $recentStdout = New-TestFile -Directory $testDir -Name "stdout_20260817000000_2222.log" -AgeDays 1
    $staleOtherFile = New-TestFile -Directory $testDir -Name "notes.txt" -AgeDays 45

    & $scriptPath -Directory $testDir -RetentionDays 30 | Out-Null

    Assert-True -Condition (-not (Test-Path -LiteralPath $staleStdout)) -Message "A stale stdout_*.log file older than the retention window is removed"
    Assert-True -Condition (Test-Path -LiteralPath $recentStdout) -Message "A recent stdout_*.log file is preserved"
    Assert-True -Condition (Test-Path -LiteralPath $staleOtherFile) -Message "A non-matching filename is never touched, regardless of age"

    # Idempotency: running again with nothing left to remove must not throw.
    $secondRunThrew = $false
    try {
        & $scriptPath -Directory $testDir -RetentionDays 30 | Out-Null
    }
    catch {
        $secondRunThrew = $true
    }
    Assert-True -Condition (-not $secondRunThrew) -Message "Running again with nothing stale left does not throw (idempotent)"
    Assert-True -Condition (Test-Path -LiteralPath $recentStdout) -Message "The recent file still exists after the idempotent second run"
}
finally {
    Remove-Item -LiteralPath $testDir -Recurse -Force -ErrorAction SilentlyContinue
}

# ===========================================================================
# -WhatIf must not remove anything.
# ===========================================================================
$whatIfDir = New-TestDirectory
try {
    $staleStdout = New-TestFile -Directory $whatIfDir -Name "stdout_20260101000000_3333.log" -AgeDays 45

    & $scriptPath -Directory $whatIfDir -RetentionDays 30 -WhatIf | Out-Null

    Assert-True -Condition (Test-Path -LiteralPath $staleStdout) -Message "-WhatIf does not remove a stale file"
}
finally {
    Remove-Item -LiteralPath $whatIfDir -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "Ran $script:testCount assertion(s), $script:failureCount failure(s)."

if ($script:failureCount -gt 0) {
    exit 1
}

exit 0
