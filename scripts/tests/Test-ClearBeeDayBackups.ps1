# Regression coverage for scripts/Clear-BeeDayBackups.ps1 (EPIC 30, Sprint 30.25, BD30-F017).
#
# Framework-free, exit-code-driven, matching the existing convention in this folder (no Pester in
# this repository - see Test-DeployBeeDayRecovery.ps1's own note on that). Uses only a real
# temporary directory this script creates and deletes itself - never touches any path under
# C:\Apps or any other real BeeDay location.
#
# Run: powershell -File scripts/tests/Test-ClearBeeDayBackups.ps1
# Exits 0 when every assertion passes, non-zero otherwise.

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$scriptPath = Join-Path $repoRoot "scripts\Clear-BeeDayBackups.ps1"

if (-not (Test-Path -LiteralPath $scriptPath)) {
    throw "Cannot find scripts/Clear-BeeDayBackups.ps1 at expected path: $scriptPath"
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
    throw "scripts/Clear-BeeDayBackups.ps1 has parse errors: $($parseErrors -join '; ')"
}
Assert-True -Condition $true -Message "Script parses without syntax errors"

function New-TestRoot {
    $dir = Join-Path ([System.IO.Path]::GetTempPath()) ("beeday-backups-retention-tests-" + [guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path (Join-Path $dir "Application") -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $dir "Data") -Force | Out-Null
    return $dir
}

function New-BackupPair {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][datetime]$Timestamp
    )

    $suffix = $Timestamp.ToString("yyyyMMdd-HHmmss")
    $applicationDir = Join-Path $Root "Application\BeeDay-$suffix"
    $dataDir = Join-Path $Root "Data\BeeDay-Data-$suffix"
    New-Item -ItemType Directory -Path $applicationDir -Force | Out-Null
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $applicationDir "placeholder.txt") -Value "placeholder" -NoNewline
    return [PSCustomObject]@{ Application = $applicationDir; Data = $dataDir }
}

# ===========================================================================
# Backup root does not exist yet - must not throw, nothing to clean up.
# ===========================================================================
$missingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("beeday-backups-retention-missing-" + [guid]::NewGuid().ToString("N"))
$missingRootThrew = $false
try {
    & $scriptPath -BackupRoot $missingRoot -RetentionDays 30 -MinimumToKeep 3 | Out-Null
}
catch {
    $missingRootThrew = $true
}
Assert-True -Condition (-not $missingRootThrew) -Message "A non-existent backup root does not throw"

# ===========================================================================
# Stale pairs beyond -MinimumToKeep are removed; recent pairs and the
# -MinimumToKeep most recent stale pairs are always preserved.
# ===========================================================================
$testRoot = New-TestRoot
try {
    $stale1 = New-BackupPair -Root $testRoot -Timestamp (Get-Date).AddDays(-45)
    $stale2 = New-BackupPair -Root $testRoot -Timestamp (Get-Date).AddDays(-44)
    $recent1 = New-BackupPair -Root $testRoot -Timestamp (Get-Date).AddDays(-1)
    $recent2 = New-BackupPair -Root $testRoot -Timestamp (Get-Date).AddHours(-1)

    # MinimumToKeep=3 protects the 3 most recent pairs overall by recency (recent2, recent1,
    # stale2) even though stale2 is itself past -RetentionDays - only the 4th-most-recent (stale1)
    # falls outside that window and is removed.
    & $scriptPath -BackupRoot $testRoot -RetentionDays 30 -MinimumToKeep 3 | Out-Null

    Assert-True -Condition (-not (Test-Path -LiteralPath $stale1.Application)) -Message "The oldest stale Application backup beyond MinimumToKeep is removed"
    Assert-True -Condition (-not (Test-Path -LiteralPath $stale1.Data)) -Message "The oldest stale Data backup beyond MinimumToKeep is removed"
    Assert-True -Condition (Test-Path -LiteralPath $stale2.Application) -Message "A stale Application backup within the MinimumToKeep recency window is preserved"
    Assert-True -Condition (Test-Path -LiteralPath $stale2.Data) -Message "A stale Data backup within the MinimumToKeep recency window is preserved"
    Assert-True -Condition (Test-Path -LiteralPath $recent1.Application) -Message "A recent Application backup is always preserved"
    Assert-True -Condition (Test-Path -LiteralPath $recent2.Data) -Message "A recent Data backup is always preserved"

    # Idempotency: running again with nothing left to remove must not throw.
    $secondRunThrew = $false
    try {
        & $scriptPath -BackupRoot $testRoot -RetentionDays 30 -MinimumToKeep 3 | Out-Null
    }
    catch {
        $secondRunThrew = $true
    }
    Assert-True -Condition (-not $secondRunThrew) -Message "Running again with nothing stale left does not throw (idempotent)"
    Assert-True -Condition (Test-Path -LiteralPath $recent1.Application) -Message "The recent backup still exists after the idempotent second run"
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}

# ===========================================================================
# -MinimumToKeep is a floor even when every pair is stale - never delete
# down to zero.
# ===========================================================================
$floorRoot = New-TestRoot
try {
    1..5 | ForEach-Object {
        New-BackupPair -Root $floorRoot -Timestamp (Get-Date).AddDays(-90 - $_) | Out-Null
    }

    & $scriptPath -BackupRoot $floorRoot -RetentionDays 30 -MinimumToKeep 3 | Out-Null

    $remainingApplication = @(Get-ChildItem (Join-Path $floorRoot "Application") -Directory)
    $remainingData = @(Get-ChildItem (Join-Path $floorRoot "Data") -Directory)
    Assert-True -Condition ($remainingApplication.Count -eq 3) -Message "Exactly MinimumToKeep Application backups survive when every backup is stale"
    Assert-True -Condition ($remainingData.Count -eq 3) -Message "Exactly MinimumToKeep Data backups survive when every backup is stale"
}
finally {
    Remove-Item -LiteralPath $floorRoot -Recurse -Force -ErrorAction SilentlyContinue
}

# ===========================================================================
# -WhatIf must not remove anything.
# ===========================================================================
$whatIfRoot = New-TestRoot
try {
    $stale = New-BackupPair -Root $whatIfRoot -Timestamp (Get-Date).AddDays(-90)
    New-BackupPair -Root $whatIfRoot -Timestamp (Get-Date).AddDays(-1) | Out-Null

    & $scriptPath -BackupRoot $whatIfRoot -RetentionDays 30 -MinimumToKeep 1 -WhatIf | Out-Null

    Assert-True -Condition (Test-Path -LiteralPath $stale.Application) -Message "-WhatIf does not remove a stale Application backup"
    Assert-True -Condition (Test-Path -LiteralPath $stale.Data) -Message "-WhatIf does not remove a stale Data backup"
}
finally {
    Remove-Item -LiteralPath $whatIfRoot -Recurse -Force -ErrorAction SilentlyContinue
}

# ===========================================================================
# A directory that doesn't match Deploy-BeeDay.ps1's own naming convention
# is left alone entirely, regardless of age.
# ===========================================================================
$nonMatchingRoot = New-TestRoot
try {
    $nonMatching = Join-Path $nonMatchingRoot "Application\SomethingElse-20200101-000000"
    New-Item -ItemType Directory -Path $nonMatching -Force | Out-Null
    New-BackupPair -Root $nonMatchingRoot -Timestamp (Get-Date).AddDays(-1) | Out-Null

    & $scriptPath -BackupRoot $nonMatchingRoot -RetentionDays 30 -MinimumToKeep 0 | Out-Null

    Assert-True -Condition (Test-Path -LiteralPath $nonMatching) -Message "A non-matching directory name is never touched, regardless of age"
}
finally {
    Remove-Item -LiteralPath $nonMatchingRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "Ran $script:testCount assertion(s), $script:failureCount failure(s)."

if ($script:failureCount -gt 0) {
    exit 1
}

exit 0
