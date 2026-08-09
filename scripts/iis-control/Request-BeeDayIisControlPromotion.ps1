# =============================================================================
# BeeDay HMG - Request privileged promotion of Invoke-BeeDayIisControl.ps1
# =============================================================================
#
# Runs as LAB\svc_beeday_runner on the self-hosted deploy runner (SERV3WEB),
# invoked by deploy-hmg.yml BEFORE Deploy-BeeDay.ps1. This is the runner-side
# half of the updater boundary described in
# scripts/iis-control/Invoke-BeeDayIisControlUpdater.ps1: it never writes
# directly into C:\Ops\BeeDay\IisControl\ (the operational boundary) or even
# into C:\Ops\BeeDay\IisControlUpdater\Backups\ - it can only deposit files
# into a pre-created, narrowly-ACL'd staging area and trigger the
# \BeeDay\HMG-IisControl-Updater Scheduled Task (SYSTEM), then wait for a
# correlated result.
#
# Deliberately duplicates (rather than shares via a module with)
# Deploy-BeeDay.ps1's Invoke-BeeDayPrivilegedIisControl trigger/poll/
# correlate logic for the \BeeDay\HMG-IisControl task - this Sprint
# explicitly avoids refactoring that already-stabilized function to keep the
# change surface of production infrastructure small. Extracting a shared
# BeeDayPrivilegedTaskClient.psm1 (parameterized by TaskPath/TaskName/
# request+result paths/timeouts) is a tracked future opportunity, not done
# here.
#
# What this script does NOT do: it never reads, writes, or lists anything
# under C:\Ops\BeeDay\IisControl\ (the operational boundary - it doesn't need
# to and has no access there); it never runs Invoke-BeeDayIisControl.ps1 or
# any other privileged script directly; it never becomes an administrator.
#
# -ScriptPath defaults to the file sitting right next to this one in the
# checkout (scripts/iis-control/Invoke-BeeDayIisControl.ps1) - the exact
# source-of-truth copy deploy-hmg.yml already checked out at the commit BeeDay
# CI validated. -CommitSha is audit-only metadata (see manifest.commitSha in
# Invoke-BeeDayIisControlUpdater.ps1) - never a security check.

param(
    [string]$ScriptPath = (Join-Path $PSScriptRoot "Invoke-BeeDayIisControl.ps1"),
    [string]$CommitSha = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$updaterRoot = "C:\Ops\BeeDay\IisControlUpdater"
$stagingFolder = Join-Path $updaterRoot "Staging"
$requestsFolder = Join-Path $updaterRoot "Requests"
$resultsFolder = Join-Path $updaterRoot "Results"

$stagingScriptPath = Join-Path $stagingFolder "Invoke-BeeDayIisControl.ps1"
$stagingManifestPath = Join-Path $stagingFolder "manifest.json"
$requestFilePath = Join-Path $requestsFolder "promote-request.txt"
$resultFilePath = Join-Path $resultsFolder "result.json"
$installedManifestPath = Join-Path $updaterRoot "installed-manifest.json"

$taskPath = "\BeeDay\"
$taskName = "HMG-IisControl-Updater"
$fileName = "Invoke-BeeDayIisControl.ps1"

# Same timing/retry budget as Deploy-BeeDay.ps1's privileged IIS control client - see the comment
# there for why Ready/Run get separate timeouts and why the trigger is retried a bounded number of
# times rather than trusted on the first attempt.
$pollIntervalSeconds = 2
$readyTimeoutSeconds = 60
$runTimeoutSeconds = 60
$maxTriggerAttempts = 2

function Get-BeeDayFileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)

    $hasher = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.IO.File]::ReadAllBytes($Path)
        $hashBytes = $hasher.ComputeHash($bytes)
        return [System.BitConverter]::ToString($hashBytes).Replace("-", "").ToLowerInvariant()
    }
    finally {
        $hasher.Dispose()
    }
}

# svc_beeday_runner's ACE on Staging\*/Requests\promote-request.txt is (W,RC,RA) - Write Data +
# Read Control + Read Attributes, no Read Data - so Set-Content (which additionally needs Read Data
# for this account, confirmed on SERV3WEB for the operational boundary's identical grant shape)
# fails; a raw FileStream opened with exactly FileMode.Open (never creates - every target here is
# pre-provisioned) and FileAccess.Write matches the narrow grant.
function Write-BeeDayFileStreamContent {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content
    )

    $contentBytes = [System.Text.Encoding]::UTF8.GetBytes($Content)

    $fileStream = $null
    try {
        $fileStream = New-Object System.IO.FileStream(
            $Path,
            [System.IO.FileMode]::Open,
            [System.IO.FileAccess]::Write,
            [System.IO.FileShare]::Read
        )
        $fileStream.SetLength(0)
        $fileStream.Write($contentBytes, 0, $contentBytes.Length)
        $fileStream.Flush()
    }
    finally {
        if ($fileStream) {
            $fileStream.Dispose()
        }
    }
}

function Wait-BeeDayUpdaterTaskState {
    param(
        [Parameter(Mandatory = $true)][string]$TargetState,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds,
        [Parameter(Mandatory = $true)][string]$TimeoutMessage
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ($true) {
        $observedState = (Get-ScheduledTask -TaskPath $taskPath -TaskName $taskName -ErrorAction Stop).State
        if ($observedState -eq $TargetState) {
            return
        }
        if ($observedState -eq 'Disabled') {
            throw "IIS control updater task '$taskPath$taskName' is Disabled - cannot proceed."
        }
        if ((Get-Date) -ge $deadline) {
            throw "$TimeoutMessage (last observed state: $observedState, waited ${TimeoutSeconds}s)."
        }
        Start-Sleep -Seconds $pollIntervalSeconds
    }
}

Write-Host "========================================"
Write-Host "BEEDAY - PRIVILEGED IIS CONTROL SCRIPT PROMOTION"
Write-Host "========================================"

if (-not (Test-Path -LiteralPath $ScriptPath -PathType Leaf)) {
    throw "Source script not found: $ScriptPath"
}

Write-Host "Computing SHA-256 of '$ScriptPath'..."
$candidateSha256 = Get-BeeDayFileSha256 -Path $ScriptPath
Write-Host "Candidate SHA-256: $candidateSha256"

# Read-only access to installed-manifest.json is the one narrow grant this account has into the
# updater boundary that reveals any state at all - fileName/sha256/commitSha/installedAt, never a
# secret. Reading it lets promotion be skipped entirely (never even triggering the Scheduled Task)
# when nothing changed, without ever needing access into the operational boundary itself.
$installedSha256 = $null
if (Test-Path -LiteralPath $installedManifestPath -PathType Leaf) {
    try {
        $installedManifestRaw = [System.IO.File]::ReadAllText($installedManifestPath)
        $installedManifest = $installedManifestRaw | ConvertFrom-Json
        $installedSha256Property = $installedManifest.PSObject.Properties['sha256']
        if ($installedSha256Property -and -not [string]::IsNullOrWhiteSpace([string]$installedSha256Property.Value)) {
            $installedSha256 = ([string]$installedSha256Property.Value).ToLowerInvariant()
        }
    }
    catch {
        Write-Host "installed-manifest.json could not be read/parsed - treating installed state as unknown (will request promotion)."
    }
}
else {
    Write-Host "installed-manifest.json not found - treating installed state as unknown (will request promotion)."
}

if ($installedSha256 -and $installedSha256 -eq $candidateSha256) {
    Write-Host "Privileged IIS control script unchanged (sha256=$candidateSha256) - skipping promotion."
    exit 0
}

Write-Host "Privileged IIS control script changed (or installed state unknown) - requesting promotion..."

Wait-BeeDayUpdaterTaskState -TargetState 'Ready' -TimeoutSeconds $readyTimeoutSeconds `
    -TimeoutMessage "IIS control updater task could not be started because a previous run never became Ready"

$requestId = [guid]::NewGuid().ToString()

$scriptContent = [System.IO.File]::ReadAllText($ScriptPath)
Write-BeeDayFileStreamContent -Path $stagingScriptPath -Content $scriptContent

$manifestObject = [ordered]@{
    requestId = $requestId
    fileName  = $fileName
    sha256    = $candidateSha256
    commitSha = $CommitSha
}
$manifestJson = $manifestObject | ConvertTo-Json -Compress
Write-BeeDayFileStreamContent -Path $stagingManifestPath -Content $manifestJson

Write-BeeDayFileStreamContent -Path $requestFilePath -Content "PROMOTE`n$requestId"

# Bounded retry: same MultipleInstances=IgnoreNew race the operational boundary already guards
# against - each iteration triggers the task, waits for Ready, then checks whether result.json
# actually correlates with THIS invocation before trusting it.
$result = $null
$attempt = 0
while ($true) {
    $attempt++

    try {
        Start-ScheduledTask -TaskPath $taskPath -TaskName $taskName -ErrorAction Stop
    }
    catch {
        throw "IIS control updater task could not be triggered (attempt $attempt): $($_.Exception.Message)"
    }

    Wait-BeeDayUpdaterTaskState -TargetState 'Ready' -TimeoutSeconds $runTimeoutSeconds `
        -TimeoutMessage "IIS control updater task exceeded its execution timeout (attempt $attempt)"

    if (-not (Test-Path -LiteralPath $resultFilePath)) {
        throw "IIS control updater task finished but produced no result file (attempt $attempt)."
    }

    $candidateResult = [System.IO.File]::ReadAllText($resultFilePath) | ConvertFrom-Json

    if ($candidateResult.requestId -eq $requestId -and $candidateResult.operation -eq 'PROMOTE') {
        $result = $candidateResult
        break
    }

    Write-Host "Attempt $attempt produced no correlated result (result.json has requestId=$($candidateResult.requestId), operation=$($candidateResult.operation); expected requestId=$requestId) - the trigger was likely swallowed by MultipleInstances=IgnoreNew."

    if ($attempt -ge $maxTriggerAttempts) {
        throw "IIS control updater task never actually started a new run after $attempt attempt(s) - no result.json update correlates with requestId $requestId. Refusing to trust an unrelated result."
    }

    Wait-BeeDayUpdaterTaskState -TargetState 'Ready' -TimeoutSeconds $readyTimeoutSeconds `
        -TimeoutMessage "IIS control updater task did not return to Ready before retrying the trigger (after attempt $attempt)"
}

$taskInfo = Get-ScheduledTaskInfo -TaskPath $taskPath -TaskName $taskName

Write-Host "Privileged IIS control promotion result: status=$($result.status) exitCode=$($result.exitCode) sha256=$($result.sha256) rollbackStatus=$($result.rollbackStatus)"

if ($taskInfo.LastTaskResult -ne 0 -or $result.exitCode -ne 0) {
    throw "Privileged IIS control script promotion failed (status=$($result.status), errorStage=$($result.errorStage), errorCode=$($result.errorCode), rollbackStatus=$($result.rollbackStatus)). The deploy must not proceed with a possibly-stale or possibly-inconsistent privileged control script."
}

if ($result.status -notin @('INSTALLED', 'UNCHANGED')) {
    throw "Privileged IIS control script promotion returned an unexpected status: $($result.status)."
}

Write-Host "Privileged IIS control script promotion completed: $($result.status) (sha256=$($result.sha256))."
