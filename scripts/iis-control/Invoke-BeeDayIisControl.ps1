# =============================================================================
# BeeDay HMG - Privileged IIS Control (SOURCE / TEMPLATE - see note below)
# =============================================================================
#
# THIS FILE IN THE REPOSITORY IS A VERSIONED, AUDITABLE SOURCE COPY ONLY.
# It is NOT deployed automatically by CI/CD. The copy that actually runs lives
# at C:\Ops\BeeDay\IisControl\Invoke-BeeDayIisControl.ps1 on SERV3WEB, installed
# and owned exclusively by an administrator (SYSTEM / BUILTIN\Administrators =
# Full Control; LAB\svc_beeday_runner = no access at all). Installing or
# updating that real copy is a deliberate manual administrative action - see
# Provision-BeeDayHmgIisControl.ps1 in this same folder. The GitHub Actions
# runner and the deploy artifact must never be able to write to, or influence
# the content of, the copy that actually executes.
#
# Runs as SYSTEM, triggered on demand by Scheduled Task \BeeDay\HMG-IisControl.
# Accepts NO command-line parameters, NO script block, and NO input from the
# caller other than the content of a single, permanently-existing request
# file. LAB\svc_beeday_runner can only trigger the task (Generic Read +
# Generic Execute on the task object) and overwrite that one file's content
# (Write Data granted directly on the file - no rights on the Requests folder
# itself beyond traversing through it, so it cannot list, create, delete, or
# rename anything there) - it has no path into this script's logic beyond the
# two literal values below, which this script validates against a strict
# allow-list before ever using them. On the caller's side (Deploy-BeeDay.ps1),
# that write is done via a raw FileStream rather than Set-Content, and the
# result below is read via [System.IO.File]::ReadAllText rather than
# Get-Content - confirmed on SERV3WEB that those cmdlets need broader access
# than this account's narrow per-file grants allow (Set-Content additionally
# wants Read Data; Get-Content additionally wants Read Extended Attributes).
# This script itself runs as SYSTEM, which already has Full Control
# regardless, so Set-Content/Get-Content remain fine to use below.
#
# request.txt and result.json are both permanent fixtures, pre-created once by
# Provision-BeeDayHmgIisControl.ps1, and are only ever overwritten in place -
# never deleted, never renamed. This is deliberate: a same-volume rename that
# replaces an existing file takes on the *source* file's security descriptor,
# not the destination's, so a temp-file-then-rename pattern would silently
# wipe out the narrow per-file ACL granted to svc_beeday_runner on every
# write. In-place overwrite (Set-Content -Force, which truncates the existing
# file rather than replacing it) preserves the file's own ACL indefinitely.
#
# Site and App Pool names are hardcoded here on purpose - never a parameter,
# never an environment variable, never taken from the request file. This is
# what makes it structurally impossible for a caller of this script to target
# any resource other than these two.

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$SiteName = "BeeDay-HMG"
$AppPoolName = "BeeDay-Web-AppPool"

$requestsFolder = "C:\Ops\BeeDay\IisControl\Requests"
$requestFilePath = Join-Path $requestsFolder "request.txt"
$resultsFolder = "C:\Ops\BeeDay\IisControl\Results"
$resultFilePath = Join-Path $resultsFolder "result.json"

function Assert-BeeDayNotReparsePoint {
    param([Parameter(Mandatory = $true)][string]$Path)

    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Refusing to process '$Path': it is a reparse point (symlink/junction), which is not permitted here."
    }
}

function Write-BeeDayIisControlResult {
    param(
        [Parameter(Mandatory = $true)][string]$RequestId,
        [Parameter(Mandatory = $true)][string]$Operation,
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][string]$SiteState,
        [Parameter(Mandatory = $true)][string]$PoolState
    )

    $result = [ordered]@{
        requestId = $RequestId
        operation = $Operation
        exitCode  = $ExitCode
        siteState = $SiteState
        poolState = $PoolState
        timestamp = (Get-Date -Format "o")
    }

    # In-place overwrite (truncate the existing file, same file object) - never
    # delete/rename/recreate result.json, which would replace its security
    # descriptor with an inherited one and silently drop the narrow read-only
    # grant given to svc_beeday_runner. SYSTEM has Full Control regardless, so
    # this write is not itself access-restricted; it's written this way purely
    # to keep the file's own ACL stable across runs.
    $result | ConvertTo-Json -Compress | Set-Content -LiteralPath $resultFilePath -Encoding utf8 -NoNewline -Force
}

$operation = $null
$requestId = $null

try {
    # Neither the Requests folder nor request.txt itself must ever be a
    # reparse point - that would let a lower-privileged writer redirect this
    # SYSTEM-run script at an arbitrary location on disk. svc_beeday_runner
    # cannot create either (no Delete/Create rights anywhere in this path), so
    # this is a sanity check against administrative error, not an attack this
    # account could carry out on its own.
    Assert-BeeDayNotReparsePoint -Path $requestsFolder

    if (-not (Test-Path -LiteralPath $requestFilePath -PathType Leaf)) {
        throw "Request file not found at '$requestFilePath'. It must be pre-provisioned - see Provision-BeeDayHmgIisControl.ps1."
    }

    Assert-BeeDayNotReparsePoint -Path $requestFilePath

    # Strict two-line format, strict allow-list. Nothing here is ever
    # interpreted as a command, echoed as one, or used to build a path -
    # $operation is compared by exact equality against two literals and
    # $requestId is used only for correlation in the result file.
    $requestLines = @(Get-Content -LiteralPath $requestFilePath)
    if ($requestLines.Count -ne 2) {
        throw "No valid pending request (expected exactly 2 lines: operation, request id - got $($requestLines.Count))."
    }

    $operation = $requestLines[0].Trim()
    $requestId = $requestLines[1].Trim()

    if ($operation -notin @('STOP', 'START')) {
        throw "Rejected request: '$operation' is not an allowed operation. Only STOP and START are accepted."
    }

    $parsedGuid = [guid]::Empty
    if (-not [guid]::TryParse($requestId, [ref]$parsedGuid)) {
        throw "Rejected request: request id is not a well-formed GUID."
    }

    # Invalidate immediately after validating, before touching IIS - a crash or
    # unexpected exit mid-operation must never leave a request.txt that a
    # later manual (or accidental) task trigger could re-process. NONE is a
    # single line, so it always fails the exactly-2-lines check above on the
    # next read, the same way an empty/never-provisioned file would.
    Set-Content -LiteralPath $requestFilePath -Value "NONE" -Encoding ascii -NoNewline -Force

    Import-Module WebAdministration -ErrorAction Stop

    if ($operation -eq 'STOP') {
        # Idempotent: WebAdministration's Stop-Website/Stop-WebAppPool throw ("Object on target
        # path is already stopped.") when asked to stop something already stopped, which would
        # otherwise turn a no-op into a false failure. Querying current state first and only
        # calling Stop-* when it isn't already the target state is what makes "already stopped"
        # a success, not an error. Mandatory order: Site before Pool.
        $currentSiteState = (Get-WebsiteState -Name $SiteName -ErrorAction Stop).Value
        if ($currentSiteState -ne 'Stopped') {
            Stop-Website -Name $SiteName -ErrorAction Stop
        }

        $currentPoolState = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop).Value
        if ($currentPoolState -ne 'Stopped') {
            Stop-WebAppPool -Name $AppPoolName -ErrorAction Stop
        }
    }
    else {
        # Same idempotency reasoning as STOP, in reverse. Mandatory order: Pool before Site.
        $currentPoolState = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop).Value
        if ($currentPoolState -ne 'Started') {
            Start-WebAppPool -Name $AppPoolName -ErrorAction Stop
        }

        $currentSiteState = (Get-WebsiteState -Name $SiteName -ErrorAction Stop).Value
        if ($currentSiteState -ne 'Started') {
            Start-Website -Name $SiteName -ErrorAction Stop
        }
    }

    Start-Sleep -Seconds 2
    $siteState = (Get-WebsiteState -Name $SiteName -ErrorAction Stop).Value
    $poolState = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop).Value

    $expectedState = if ($operation -eq 'STOP') { 'Stopped' } else { 'Started' }
    $success = ($siteState -eq $expectedState) -and ($poolState -eq $expectedState)

    Write-BeeDayIisControlResult -RequestId $requestId -Operation $operation `
        -ExitCode ([int](-not $success)) -SiteState $siteState -PoolState $poolState

    if (-not $success) {
        Write-Error "Operation '$operation' completed but final state was not fully '$expectedState' (site=$siteState, pool=$poolState)."
        exit 1
    }

    exit 0
}
catch {
    $failedRequestId = if ($requestId) { $requestId } else { "unknown" }
    $failedOperation = if ($operation) { $operation } else { "unknown" }

    Write-BeeDayIisControlResult -RequestId $failedRequestId -Operation $failedOperation `
        -ExitCode 1 -SiteState "Unknown" -PoolState "Unknown"

    Write-Error "Privileged IIS control failed: $($_.Exception.Message)"
    exit 1
}
