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
# caller other than the content of two permanently-existing files. LAB\
# svc_beeday_runner can only trigger the task (Generic Read + Generic Execute
# on the task object) and overwrite those two files' content (Write Data
# granted directly on each file - no rights on the Requests folder itself
# beyond traversing through it, so it cannot list, create, delete, or rename
# anything there) - it has no path into this script's logic beyond the
# literal values it validates against strict allow-lists before ever using
# them. On the caller's side (Deploy-BeeDay.ps1), those writes are done via a
# raw FileStream rather than Set-Content, and the result below is read via
# [System.IO.File]::ReadAllText rather than Get-Content - confirmed on
# SERV3WEB that those cmdlets need broader access than this account's narrow
# per-file grants allow (Set-Content additionally wants Read Data;
# Get-Content additionally wants Read Extended Attributes). This script
# itself runs as SYSTEM, which already has Full Control regardless, so
# Set-Content/Get-Content remain fine to use below.
#
# request.txt carries the operation (STOP/START/CONFIGURE) and a correlation
# GUID - never anything else. env-config.secret carries CONFIGURE's payload as
# {"requestId": "<same GUID as request.txt>", "variables": {...}} (App Pool
# environment variables, which may include
# BeeDay__Persistence__SqlServer__ConnectionString) - kept in its own file,
# never in request.txt or result.json, so a connection string is never
# adjacent to data this script ever echoes back. The requestId inside the
# payload must match request.txt's exactly before any variable is applied -
# this rejects a stale payload left over from an earlier CONFIGURE that
# failed before its own cleanup ran, or any other mismatch, without ever
# needing to inspect the variables themselves to make that call. Both
# request.txt and
# result.json (and env-config.secret) are permanent fixtures, pre-created
# once by Provision-BeeDayHmgIisControl.ps1, and are only ever overwritten in
# place - never deleted, never renamed. This is deliberate: a same-volume
# rename that replaces an existing file takes on the *source* file's security
# descriptor, not the destination's, so a temp-file-then-rename pattern would
# silently wipe out the narrow per-file ACL granted to svc_beeday_runner on
# every write. In-place overwrite (Set-Content -Force, which truncates the
# existing file rather than replacing it) preserves the file's own ACL
# indefinitely.
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
$envConfigFilePath = Join-Path $requestsFolder "env-config.secret"
$resultsFolder = "C:\Ops\BeeDay\IisControl\Results"
$resultFilePath = Join-Path $resultsFolder "result.json"

# CONFIGURE's payload (env-config.secret) can only ever set these exact names - anything else in
# the payload is rejected outright rather than silently ignored, even though the payload's shape
# is entirely under this repository's own control today. This is defense in depth: the payload
# reaching this point already went through svc_beeday_runner (which cannot read it back to verify
# what it wrote), so failing loudly on an unexpected key is preferable to trusting it blindly.
$allowedEnvironmentVariableNames = @(
    'ASPNETCORE_ENVIRONMENT',
    'DOTNET_ENVIRONMENT',
    'AllowedHosts',
    'BeeDay__IdentityEmail__PublicBaseUrl',
    'BeeDay__Persistence__SqlServer__ConnectionString',
    'BeeDay__Email__Resend__ApiKey',
    'BeeDay__Email__Resend__FromAddress',
    'BeeDay__Email__Resend__FromName'
)

function Assert-BeeDayNotReparsePoint {
    param([Parameter(Mandatory = $true)][string]$Path)

    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Refusing to process '$Path': it is a reparse point (symlink/junction), which is not permitted here."
    }
}

# Moved verbatim from Deploy-BeeDay.ps1's Set-BeeDayEnvironmentVariables: this is the actual write
# to applicationHost.config (via the App Pool's environmentVariables collection), which is why it
# now only ever runs here, as SYSTEM. Never logs $Variables - callers must not either. Any failure
# is re-thrown as a fixed, generic message: a WebAdministration exception here could plausibly
# embed a value (e.g. an invalid-value error echoing the offending string), and
# BeeDay__Persistence__SqlServer__ConnectionString may be one of those values.
function Set-BeeDayAppPoolEnvironmentVariables {
    param([Parameter(Mandatory = $true)][hashtable]$Variables)

    foreach ($key in $Variables.Keys) {
        if ($key -notin $allowedEnvironmentVariableNames) {
            throw "Rejected environment variable name: '$key' is not in the allowed list."
        }
    }

    try {
        foreach ($entry in $Variables.GetEnumerator()) {
            $filter = "system.applicationHost/applicationPools/add[@name='$AppPoolName']/environmentVariables/add[@name='$($entry.Key)']"

            Remove-WebConfigurationProperty `
                -PSPath "MACHINE/WEBROOT/APPHOST" `
                -Filter $filter `
                -Name "." `
                -ErrorAction SilentlyContinue

            Add-WebConfigurationProperty `
                -PSPath "MACHINE/WEBROOT/APPHOST" `
                -Filter "system.applicationHost/applicationPools/add[@name='$AppPoolName']/environmentVariables" `
                -Name "." `
                -Value @{ name = $entry.Key; value = $entry.Value }
        }
    }
    catch {
        throw "Failed to configure App Pool environment variables (details withheld - the underlying error may reference configuration values)."
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

    if ($operation -notin @('STOP', 'START', 'CONFIGURE')) {
        throw "Rejected request: '$operation' is not an allowed operation. Only STOP, START, and CONFIGURE are accepted."
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
    elseif ($operation -eq 'START') {
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
    else {
        # CONFIGURE: applies App Pool environment variables (env-config.secret's payload) via the
        # same applicationHost.config write Deploy-BeeDay.ps1 used to attempt directly as
        # svc_beeday_runner - moved here because that account has no access to it at all.
        if (-not (Test-Path -LiteralPath $envConfigFilePath -PathType Leaf)) {
            throw "Environment configuration payload not found at '$envConfigFilePath'. It must be pre-provisioned - see Provision-BeeDayHmgIisControl.ps1."
        }

        # Past this point the path is confirmed to be a real file, not a reparse point - safe to
        # unconditionally clear in finally below regardless of what happens while consuming it.
        # Every failure mode from here on (malformed JSON, empty payload, a rejected key, a
        # WebAdministration failure, or anything else) must still result in env-config.secret
        # ending up wiped - this is the whole point of the try/finally: cleanup must not depend on
        # reaching a specific line of "happy path" code.
        Assert-BeeDayNotReparsePoint -Path $envConfigFilePath

        $cleanupFailed = $false
        try {
            $payloadRaw = Get-Content -LiteralPath $envConfigFilePath -Raw

            # A parse failure here must never echo $payloadRaw back - it may contain a connection
            # string mid-corruption - so the caught exception is replaced with a fixed message,
            # the same discipline Deploy-BeeDay.ps1 already applies to SqlConnectionStringBuilder
            # parse failures.
            try {
                $parsedPayload = $payloadRaw | ConvertFrom-Json
            }
            catch {
                throw "Environment configuration payload could not be parsed as valid JSON."
            }

            # Correlation: the payload must carry the SAME request id request.txt carried ($parsedGuid,
            # already validated above). This proves the payload being applied right now is the one
            # Deploy-BeeDay.ps1 wrote for THIS invocation, not a stale leftover from an earlier
            # CONFIGURE that failed before being invalidated (or anything else that might have
            # ended up in the file). Property existence is checked via .PSObject.Properties rather
            # than direct member access - Set-StrictMode -Version Latest throws on referencing a
            # property that doesn't exist on a PSCustomObject, so a payload missing requestId
            # entirely must not itself become an unhandled/uninformative error.
            $requestIdProperty = $parsedPayload.PSObject.Properties['requestId']
            if (-not $requestIdProperty -or [string]::IsNullOrWhiteSpace([string]$requestIdProperty.Value)) {
                throw "Environment configuration payload is missing requestId."
            }

            $payloadRequestGuid = [guid]::Empty
            if (-not [guid]::TryParse([string]$requestIdProperty.Value, [ref]$payloadRequestGuid)) {
                throw "Environment configuration payload requestId is not a well-formed GUID."
            }

            if ($payloadRequestGuid -ne $parsedGuid) {
                throw "Environment configuration payload requestId does not match the request just read from request.txt - rejecting a stale or mismatched payload."
            }

            $variablesProperty = $parsedPayload.PSObject.Properties['variables']
            if (-not $variablesProperty) {
                throw "Environment configuration payload is missing the variables object."
            }

            # ConvertFrom-Json returns a PSCustomObject, not a hashtable, on this PowerShell
            # version - converting it explicitly is what lets
            # Set-BeeDayAppPoolEnvironmentVariables's allow-list check enumerate keys the same way
            # regardless of how the payload arrived.
            $variablesToApply = @{}
            foreach ($property in $variablesProperty.Value.PSObject.Properties) {
                $variablesToApply[$property.Name] = $property.Value
            }

            if ($variablesToApply.Count -eq 0) {
                throw "Environment configuration payload was empty - nothing to configure."
            }

            Set-BeeDayAppPoolEnvironmentVariables -Variables $variablesToApply
        }
        finally {
            # Runs on every path out of the try block above: success, a thrown exception from
            # parsing/validation/allow-list/WebAdministration, or anything else. Never logs
            # $payloadRaw or any prior content - only whether the overwrite itself succeeded.
            # $ErrorActionPreference is "Stop" script-wide, so Write-Error here would itself become
            # a terminating error and could mask whatever exception is already propagating out of
            # the try block - the failure is recorded in a flag instead and only acted on below,
            # after the finally block has fully completed.
            try {
                Set-Content -LiteralPath $envConfigFilePath -Value "{}" -Encoding utf8 -NoNewline -Force
            }
            catch {
                $cleanupFailed = $true
            }
        }

        if ($cleanupFailed) {
            # Only reached when the try block above succeeded but the cleanup write itself failed
            # (if the try block had thrown, that exception already propagated past this point) -
            # still fails closed, per requirement, even though the App Pool configuration was
            # already applied successfully.
            throw "Environment configuration was applied, but clearing the payload file afterwards failed - failing closed (details withheld)."
        }
    }

    Start-Sleep -Seconds 2
    $siteState = (Get-WebsiteState -Name $SiteName -ErrorAction Stop).Value
    $poolState = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop).Value

    if ($operation -eq 'CONFIGURE') {
        # No target site/pool state to compare against - reaching this point without an exception
        # is success. States are still reported, purely for visibility, to keep the result schema
        # uniform with STOP/START.
        $success = $true
    }
    else {
        $expectedState = if ($operation -eq 'STOP') { 'Stopped' } else { 'Started' }
        $success = ($siteState -eq $expectedState) -and ($poolState -eq $expectedState)
    }

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
