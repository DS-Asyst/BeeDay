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
# request.txt carries the operation (STOP/START/CONFIGURE/RESTORE) and a correlation
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
# env-config-snapshot.secret exists purely so a failed CONFIGURE (partial or
# total) can be undone. Immediately before CONFIGURE changes anything, it
# durably records the PRE-CONFIGURE state of only the keys that CONFIGURE is
# about to touch ({"requestId": "<the SAME requestId as this CONFIGURE's own
# request.txt entry>", "variables": {"<key>": {"existed": true, "value": "..."}
# | {"existed": false}, ...}}) - if that write itself fails, CONFIGURE aborts
# before mutating anything, so a CONFIGURE that has progressed past this point
# always has a corresponding undo record on disk, tagged with its own requestId.
#
# RESTORE (triggered only by Deploy-BeeDay.ps1's own rollback path, never
# directly by a caller choice) is asked to undo a SPECIFIC requestId - not
# "whatever snapshot happens to be on disk". Deploy-BeeDay.ps1 deliberately
# reuses the CONFIGURE call's own requestId as RESTORE's request.txt requestId
# (instead of generating a fresh one, unlike every other operation), and this
# script only ever applies the snapshot if the snapshot's OWN stored requestId
# matches request.txt's exactly. This is what stops a stale snapshot from an
# EARLIER, already-successful deploy (never consumed, since that deploy never
# rolled back) from being silently reapplied by a LATER, unrelated deploy's
# rollback - e.g.: Deploy A's CONFIGURE snapshots V1 and applies V2, succeeds;
# the V1 snapshot is left on disk, still tagged with Deploy A's requestId;
# Deploy B fails before its own CONFIGURE ever runs (so V2 was never touched
# this attempt, nothing needs undoing); Deploy B's rollback must NOT restore
# V1 - and it does not, because it has no CONFIGURE requestId of its own to
# ask for (Deploy-BeeDay.ps1 skips triggering RESTORE entirely in that case),
# and even if it did, that id would never match Deploy A's leftover snapshot.
# A key that existed is set back to its exact prior value; a key that did not
# exist is removed outright, never left at an empty string. On a successful
# match, the snapshot is reset to "{}" afterwards so it can never be matched
# by requestId again (belt-and-suspenders on top of the requestId check, which
# alone already makes reuse practically impossible). Any mismatch - wrong
# requestId, no requestId, or no snapshot at all - is treated as "nothing to
# restore", not a failure: environment variables are left completely
# untouched in every one of those cases.
#
# Unlike request.txt/env-config.secret, svc_beeday_runner has NO access
# whatsoever to this file - not even Read Control - because it is written and
# read exclusively by this script itself (SYSTEM); it lives directly under
# the root folder and inherits that folder's admin-only ACL (see
# Provision-BeeDayHmgIisControl.ps1) rather than needing any per-file grant.
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

# Admin-only, no ACE for svc_beeday_runner at all (see the header comment above and
# Provision-BeeDayHmgIisControl.ps1) - lives directly under the root folder, not under Requests/
# Results, because unlike every other file here it is never touched by the runner in either
# direction.
$envConfigSnapshotFilePath = "C:\Ops\BeeDay\IisControl\env-config-snapshot.secret"

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

$environmentVariablesFilter = "system.applicationHost/applicationPools/add[@name='$AppPoolName']/environmentVariables"

# Shared by Set-BeeDayAppPoolEnvironmentVariables (CONFIGURE) and Restore-BeeDayAppPoolEnvironmentVariables
# (RESTORE) - both need to know, at the start of their own run, exactly what currently exists in the
# App Pool's environmentVariables collection and with which values.
#
# Get-WebConfigurationProperty -Name "." against a collection filter returns a single
# Microsoft.IIs.PowerShell.Framework.ConfigurationElement wrapping the whole collection, not one
# object per <add> in the pipeline - confirmed directly on SERV3WEB. The individual <add> elements
# live under its .Collection property, each exposing name/value via .Attributes["name"].Value/
# .Attributes["value"].Value; the wrapper itself has neither property directly. Under
# Set-StrictMode -Version Latest (script-wide), referencing $_.name directly on the wrapper threw a
# PropertyNotFoundStrict error on every call, which is why this once failed at
# READ_EXISTING_ENV/ENV_READ_FAILED before a single Remove/Add ever ran.
function Get-BeeDayAppPoolEnvironmentVariablesSnapshot {
    $existingEnvironmentVariables = Get-WebConfigurationProperty `
        -PSPath "MACHINE/WEBROOT/APPHOST" `
        -Filter $environmentVariablesFilter `
        -Name "." `
        -ErrorAction Stop

    $values = @{}
    foreach ($item in $existingEnvironmentVariables.Collection) {
        $values[$item.Attributes["name"].Value] = $item.Attributes["value"].Value
    }
    return $values
}

# Converges a single App Pool environment variable to the desired state - the one place both
# CONFIGURE and RESTORE actually touch applicationHost.config, so a fix to this mechanic (like the
# -AtElement one above) only ever needs to happen once. Removal identifies the element via
# -AtElement (never a collection-item filter + bare -Name ".", which silently no-ops against this
# WebAdministration provider instead of removing anything - confirmed directly on SERV3WEB; leaving
# the old entry in place made the following Add-WebConfigurationProperty fail with 0x800700B7
# "Cannot add duplicate collection entry"). $Value $null means "remove only, do not re-add" - used by
# RESTORE for a key that did not exist before the CONFIGURE being undone; it must end up absent
# again, never set back to an empty string, which would be a different value.
function Set-BeeDayAppPoolEnvironmentVariable {
    param(
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][bool]$KeyCurrentlyExists,
        [AllowNull()]$Value
    )

    if ($KeyCurrentlyExists) {
        $script:currentStage = 'REMOVE_EXISTING_ENV'
        $script:currentErrorCode = 'ENV_REMOVE_FAILED'
        Remove-WebConfigurationProperty `
            -PSPath "MACHINE/WEBROOT/APPHOST" `
            -Filter $environmentVariablesFilter `
            -Name "." `
            -AtElement @{ name = $Key } `
            -ErrorAction Stop
    }

    if ($null -ne $Value) {
        $script:currentStage = 'ADD_ENV_VARIABLE'
        $script:currentErrorCode = 'ENV_ADD_FAILED'
        Add-WebConfigurationProperty `
            -PSPath "MACHINE/WEBROOT/APPHOST" `
            -Filter $environmentVariablesFilter `
            -Name "." `
            -Value @{ name = $Key; value = $Value } `
            -ErrorAction Stop
    }
}

# Durably records, BEFORE any mutation, exactly what Set-BeeDayAppPoolEnvironmentVariables is about
# to change - see the header comment above for the file/format. Never logs $Variables - same
# discipline as the rest of this file. Admin-only file (see $envConfigSnapshotFilePath) - SYSTEM has
# Full Control regardless of prior state, so a plain Set-Content (unlike request.txt/env-config.secret,
# which need the FileStream workaround for svc_beeday_runner's narrow grant) is fine here.
function Write-BeeDayEnvironmentVariableSnapshot {
    param(
        [Parameter(Mandatory = $true)][string]$RequestId,
        [Parameter(Mandatory = $true)][System.Collections.Specialized.OrderedDictionary]$Variables
    )

    $snapshot = [ordered]@{
        requestId = $RequestId
        variables = $Variables
    }
    $snapshot | ConvertTo-Json -Compress -Depth 5 |
        Set-Content -LiteralPath $envConfigSnapshotFilePath -Encoding utf8 -NoNewline -Force
}

# Moved verbatim from Deploy-BeeDay.ps1's Set-BeeDayEnvironmentVariables: this is the actual write
# to applicationHost.config (via the App Pool's environmentVariables collection), which is why it
# now only ever runs here, as SYSTEM. Never logs $Variables - callers must not either. Any failure
# is re-thrown as a fixed, generic message: a WebAdministration exception here could plausibly
# embed a value (e.g. an invalid-value error echoing the offending string), and
# BeeDay__Persistence__SqlServer__ConnectionString may be one of those values.
function Set-BeeDayAppPoolEnvironmentVariables {
    param(
        [Parameter(Mandatory = $true)][hashtable]$Variables,
        [Parameter(Mandatory = $true)][string]$RequestId
    )

    $script:currentStage = 'VALIDATE_VARIABLES'
    $script:currentErrorCode = 'VARIABLE_NOT_ALLOWED'
    foreach ($key in $Variables.Keys) {
        if ($key -notin $allowedEnvironmentVariableNames) {
            throw "Rejected environment variable name: '$key' is not in the allowed list."
        }
    }

    try {
        # Read-only canary before any mutation: confirms WebAdministration can actually enumerate
        # the App Pool's environmentVariables collection right now, tagged READ_EXISTING_ENV instead
        # of failing more ambiguously a few lines later inside the Remove/Add loop. Its result is
        # also reused below to decide, per variable, whether a removal is even needed - this is what
        # keeps CONFIGURE idempotent (an entry that doesn't exist yet must never be treated as a
        # removal failure) without resorting to -ErrorAction SilentlyContinue, which would just as
        # readily swallow a genuine WebAdministration failure on an entry that DOES exist.
        $script:currentStage = 'READ_EXISTING_ENV'
        $script:currentErrorCode = 'ENV_READ_FAILED'
        $existingVariableValues = Get-BeeDayAppPoolEnvironmentVariablesSnapshot

        # Snapshot BEFORE any mutation, and only for the keys this call is about to touch - exactly
        # what a later RESTORE would need to undo it, no more. If this write itself fails, the
        # thrown exception (ENV_SNAPSHOT_FAILED) aborts before the Remove/Add loop below ever runs -
        # this CONFIGURE never mutates anything it hasn't first durably recorded how to undo.
        $script:currentStage = 'SNAPSHOT_EXISTING_ENV'
        $script:currentErrorCode = 'ENV_SNAPSHOT_FAILED'
        $snapshotVariables = [ordered]@{}
        foreach ($key in $Variables.Keys) {
            if ($existingVariableValues.ContainsKey($key)) {
                $snapshotVariables[$key] = [ordered]@{ existed = $true; value = $existingVariableValues[$key] }
            }
            else {
                $snapshotVariables[$key] = [ordered]@{ existed = $false }
            }
        }
        Write-BeeDayEnvironmentVariableSnapshot -RequestId $RequestId -Variables $snapshotVariables

        foreach ($entry in $Variables.GetEnumerator()) {
            Set-BeeDayAppPoolEnvironmentVariable -Key $entry.Key `
                -KeyCurrentlyExists $existingVariableValues.ContainsKey($entry.Key) -Value $entry.Value
        }
    }
    catch {
        throw "Failed to configure App Pool environment variables (details withheld - the underlying error may reference configuration values)."
    }
}

# RESTORE: reapplies the pre-CONFIGURE snapshot (env-config-snapshot.secret) - only ever triggered
# by Deploy-BeeDay.ps1's own rollback path, after CONFIGURE has run (fully or partially) and a later
# step failed. $RequestId is request.txt's own requestId, which Deploy-BeeDay.ps1 deliberately sets
# to the SAME id the CONFIGURE it wants undone used (see $script:lastConfigureRequestId in
# Deploy-BeeDay.ps1) - never a freshly generated one, unlike every other operation. The snapshot is
# only ever applied if its OWN stored requestId matches this exactly; any mismatch - a snapshot from
# an earlier, unrelated deploy that was never consumed, a snapshot that predates this deploy's
# CONFIGURE ever running, or a caller asking to restore an id with no corresponding snapshot at all -
# is treated exactly like an absent snapshot: nothing to restore, success, no variable touched. This
# is what stops a stale snapshot left over from a PREVIOUS successful deploy from being silently
# reapplied by a LATER deploy's rollback (see the header comment for the concrete scenario this
# closes). On a successful match, the snapshot is reset to "{}" afterwards (best-effort - see below)
# so it can never be matched again, even in principle. Never logs variable values - same discipline
# as Set-BeeDayAppPoolEnvironmentVariables, and any WebAdministration failure is re-thrown as a
# fixed, generic message for the same reason (a raw error could echo back a value being restored,
# e.g. a connection string).
function Restore-BeeDayAppPoolEnvironmentVariables {
    param([Parameter(Mandatory = $true)][string]$RequestId)

    $script:currentStage = 'READ_SNAPSHOT'
    $script:currentErrorCode = 'SNAPSHOT_READ_FAILED'

    if (-not (Test-Path -LiteralPath $envConfigSnapshotFilePath -PathType Leaf)) {
        return
    }

    Assert-BeeDayNotReparsePoint -Path $envConfigSnapshotFilePath
    $snapshotRaw = Get-Content -LiteralPath $envConfigSnapshotFilePath -Raw

    $script:currentErrorCode = 'SNAPSHOT_INVALID'
    try {
        $parsedSnapshot = $snapshotRaw | ConvertFrom-Json
    }
    catch {
        throw "Environment configuration snapshot could not be parsed as valid JSON."
    }

    # Correlation gate: see the function comment above. Deliberately compared BEFORE looking at
    # $variablesProperty at all - a snapshot belonging to a different requestId must never influence
    # anything below this point, not even to decide whether it "looks" empty or populated.
    $script:currentStage = 'VALIDATE_SNAPSHOT_REQUEST_ID'
    $snapshotRequestIdProperty = $parsedSnapshot.PSObject.Properties['requestId']
    if (-not $snapshotRequestIdProperty -or [string]$snapshotRequestIdProperty.Value -ne $RequestId) {
        return
    }

    $script:currentErrorCode = 'SNAPSHOT_INVALID'
    $variablesProperty = $parsedSnapshot.PSObject.Properties['variables']
    if (-not $variablesProperty) {
        return
    }

    $snapshotEntries = @($variablesProperty.Value.PSObject.Properties)
    if ($snapshotEntries.Count -eq 0) {
        return
    }

    try {
        $script:currentStage = 'READ_EXISTING_ENV'
        $script:currentErrorCode = 'ENV_READ_FAILED'
        $existingVariableValues = Get-BeeDayAppPoolEnvironmentVariablesSnapshot

        foreach ($property in $snapshotEntries) {
            $key = $property.Name

            $existedProperty = $property.Value.PSObject.Properties['existed']
            $existedBeforeConfigure = [bool]($existedProperty -and $existedProperty.Value)

            $restoredValue = $null
            if ($existedBeforeConfigure) {
                $valueProperty = $property.Value.PSObject.Properties['value']
                if ($valueProperty) {
                    $restoredValue = [string]$valueProperty.Value
                }
            }

            Set-BeeDayAppPoolEnvironmentVariable -Key $key `
                -KeyCurrentlyExists $existingVariableValues.ContainsKey($key) -Value $restoredValue
        }
    }
    catch {
        throw "Failed to restore App Pool environment variables (details withheld - the underlying error may reference configuration values)."
    }

    # Consume the snapshot now that it has been successfully applied - it must never be eligible to
    # match a future RequestId comparison again (the id is now already reflected in live IIS state,
    # not just on disk). Best-effort and never fatal: unlike env-config.secret's cleanup (which
    # CONFIGURE fails closed on), the restore above already fully succeeded and is not undone by a
    # failure here - the correlation check already makes any leftover snapshot permanently
    # unmatchable by construction (every future CONFIGURE generates a fresh GUID), so this is
    # hygiene on top of that guarantee, not something a later RESTORE could be tricked by.
    try {
        Set-Content -LiteralPath $envConfigSnapshotFilePath -Value "{}" -Encoding utf8 -NoNewline -Force
    }
    catch {
        # Intentionally swallowed - see comment above.
    }
}

function Write-BeeDayIisControlResult {
    param(
        [Parameter(Mandatory = $true)][string]$RequestId,
        [Parameter(Mandatory = $true)][string]$Operation,
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][string]$SiteState,
        [Parameter(Mandatory = $true)][string]$PoolState,

        # Diagnostic only - always one of the fixed constants in $script:currentStage/
        # $script:currentErrorCode below, NEVER derived from an exception's own message. Absent/
        # $null on every successful run (rendered as JSON null - deliberately untyped so a $null
        # argument survives as $null instead of being coerced to "" by a [string] parameter type);
        # only ever set from the outer catch. Never pass $_.Exception.Message (or anything built
        # from it) here - see the constant lists next to $script:currentStage's declaration for why.
        $ErrorStage = $null,
        $ErrorCode = $null
    )

    $result = [ordered]@{
        requestId  = $RequestId
        operation  = $Operation
        exitCode   = $ExitCode
        siteState  = $SiteState
        poolState  = $PoolState
        errorStage = $ErrorStage
        errorCode  = $ErrorCode
        timestamp  = (Get-Date -Format "o")
    }

    # In-place overwrite (truncate the existing file, same file object) - never
    # delete/rename/recreate result.json, which would replace its security
    # descriptor with an inherited one and silently drop the narrow read-only
    # grant given to svc_beeday_runner. SYSTEM has Full Control regardless, so
    # this write is not itself access-restricted; it's written this way purely
    # to keep the file's own ACL stable across runs.
    $result | ConvertTo-Json -Compress | Set-Content -LiteralPath $resultFilePath -Encoding utf8 -NoNewline -Force
}

# Structured, secret-free failure diagnostics for the outer catch below. Both are updated
# immediately BEFORE each sensitive operation (never derived from the exception afterwards) so
# that whatever the outer catch sees reflects exactly which step was in progress when something
# threw - never the exception's own text, which is the only thing on this script's boundary that
# could ever carry a fragment of a secret (e.g. a WebAdministration error echoing an invalid
# value). $currentErrorCode is a best-effort default for the active stage, overridden right at a
# specific throw site when one stage can fail for more than one reason (e.g. VALIDATE_REQUEST_ID
# covers both a malformed GUID and a requestId that doesn't match).
$script:currentStage = $null
$script:currentErrorCode = $null

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

    if ($operation -notin @('STOP', 'START', 'CONFIGURE', 'RESTORE')) {
        throw "Rejected request: '$operation' is not an allowed operation. Only STOP, START, CONFIGURE, and RESTORE are accepted."
    }

    $script:currentStage = 'VALIDATE_REQUEST_ID'
    $script:currentErrorCode = 'REQUEST_ID_INVALID'
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

    if ($operation -eq 'STOP' -or $operation -eq 'START') {
        # STOP/START never touch env-config.secret, so their own Import-Module has no cleanup to
        # protect - it stays outside any try/finally, tagged only for the duration of the call
        # itself. A failure here surfaces with errorStage=IMPORT_WEBADMINISTRATION/
        # errorCode=IIS_IMPORT_FAILED, same as CONFIGURE's below.
        $script:currentStage = 'IMPORT_WEBADMINISTRATION'
        $script:currentErrorCode = 'IIS_IMPORT_FAILED'
        Import-Module WebAdministration -ErrorAction Stop

        # Idempotent AND resilient to a transient WebAdministration error: every read/transition
        # step below is wrapped in its own try/catch that swallows whatever it throws and moves on
        # - a real SERV3WEB run showed Get-WebsiteState/Get-WebAppPoolState/Stop-Website/
        # Stop-WebAppPool occasionally throwing even though the underlying state change had, in
        # fact, already taken effect (confirmed immediately after the "failed" run by querying
        # state directly - both Site and Pool were already Stopped). Swallowing here does NOT mask
        # a real failure: the shared FINALIZE block below (Get-WebsiteState/Get-WebAppPoolState
        # with -ErrorAction Stop on every poll iteration - see Sprint 17.18) is the one authoritative,
        # un-swallowed check that actually decides $success - if the site/pool genuinely never reach
        # the target state within FINALIZE's own poll timeout, or a FINALIZE read itself fails, this
        # operation still fails. What changes here is that a transient hiccup in one of these
        # individual steps no longer aborts the whole operation before FINALIZE ever gets a chance to
        # prove the transition actually worked.
        # Each step is tagged with its own fixed stage/errorCode immediately before running, purely
        # for diagnostics should something outside this try/catch pattern ever throw here.
        if ($operation -eq 'STOP') {
            # Mandatory order: Site before Pool.
            $script:currentStage = 'READ_SITE_STATE'
            $script:currentErrorCode = 'SITE_READ_FAILED'
            $currentSiteState = $null
            try {
                $currentSiteState = (Get-WebsiteState -Name $SiteName -ErrorAction Stop).Value
            }
            catch {
                # Transient - FINALIZE below is the authoritative check.
            }

            if ($currentSiteState -ne 'Stopped') {
                $script:currentStage = 'STOP_SITE'
                $script:currentErrorCode = 'SITE_STOP_FAILED'
                try {
                    Stop-Website -Name $SiteName -ErrorAction Stop
                }
                catch {
                    # Transient (e.g. "already stopped", or a provider hiccup) - FINALIZE below is
                    # the authoritative check.
                }
            }

            $script:currentStage = 'READ_POOL_STATE'
            $script:currentErrorCode = 'POOL_READ_FAILED'
            $currentPoolState = $null
            try {
                $currentPoolState = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop).Value
            }
            catch {
                # Transient - FINALIZE below is the authoritative check.
            }

            if ($currentPoolState -ne 'Stopped') {
                $script:currentStage = 'STOP_POOL'
                $script:currentErrorCode = 'POOL_STOP_FAILED'
                try {
                    Stop-WebAppPool -Name $AppPoolName -ErrorAction Stop
                }
                catch {
                    # Transient - FINALIZE below is the authoritative check.
                }
            }
        }
        else {
            # Mandatory order: Pool before Site.
            $script:currentStage = 'READ_POOL_STATE'
            $script:currentErrorCode = 'POOL_READ_FAILED'
            $currentPoolState = $null
            try {
                $currentPoolState = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop).Value
            }
            catch {
                # Transient - FINALIZE below is the authoritative check.
            }

            if ($currentPoolState -ne 'Started') {
                $script:currentStage = 'START_POOL'
                $script:currentErrorCode = 'POOL_START_FAILED'
                try {
                    Start-WebAppPool -Name $AppPoolName -ErrorAction Stop
                }
                catch {
                    # Transient - FINALIZE below is the authoritative check.
                }
            }

            $script:currentStage = 'READ_SITE_STATE'
            $script:currentErrorCode = 'SITE_READ_FAILED'
            $currentSiteState = $null
            try {
                $currentSiteState = (Get-WebsiteState -Name $SiteName -ErrorAction Stop).Value
            }
            catch {
                # Transient - FINALIZE below is the authoritative check.
            }

            if ($currentSiteState -ne 'Started') {
                $script:currentStage = 'START_SITE'
                $script:currentErrorCode = 'SITE_START_FAILED'
                try {
                    Start-Website -Name $SiteName -ErrorAction Stop
                }
                catch {
                    # Transient (e.g. "already started", or a provider hiccup) - FINALIZE below is
                    # the authoritative check.
                }
            }
        }

        $script:currentStage = $null
        $script:currentErrorCode = $null
    }
    elseif ($operation -eq 'CONFIGURE') {
        # CONFIGURE: applies App Pool environment variables (env-config.secret's payload) via the
        # same applicationHost.config write Deploy-BeeDay.ps1 used to attempt directly as
        # svc_beeday_runner - moved here because that account has no access to it at all.
        if (-not (Test-Path -LiteralPath $envConfigFilePath -PathType Leaf)) {
            throw "Environment configuration payload not found at '$envConfigFilePath'. It must be pre-provisioned - see Provision-BeeDayHmgIisControl.ps1."
        }

        # Validated (not a reparse point) BEFORE the try/finally below is ever entered - the
        # finally's unconditional cleanup write must only ever go through a path already proven
        # safe, never through one that hasn't been checked yet.
        Assert-BeeDayNotReparsePoint -Path $envConfigFilePath

        $cleanupFailed = $false

        # Set only on the last line of the try block below (nothing throws after it) - lets the
        # finally block tell "the try block itself failed" apart from "the try block succeeded and
        # only the cleanup write below failed", so a cleanup that succeeds never overwrites the
        # true failing stage/code of an earlier exception with SECRET_CLEANUP, and a cleanup that
        # fails is never misattributed to whatever stage happened to run last on the happy path.
        $tryBlockSucceeded = $false
        try {
            # Import-Module now runs INSIDE this try/finally (unlike STOP/START above) precisely so
            # its failure is covered by the same unconditional cleanup as every other CONFIGURE
            # failure mode below - env-config.secret already carries this invocation's real payload
            # (the runner wrote it before the Scheduled Task even started) by the time this line
            # runs, so a WebAdministration import failure must not be able to leave it behind.
            $script:currentStage = 'IMPORT_WEBADMINISTRATION'
            $script:currentErrorCode = 'IIS_IMPORT_FAILED'
            Import-Module WebAdministration -ErrorAction Stop

            $script:currentStage = 'READ_PAYLOAD'
            $script:currentErrorCode = 'PAYLOAD_READ_FAILED'
            $payloadRaw = Get-Content -LiteralPath $envConfigFilePath -Raw

            # A parse failure here must never echo $payloadRaw back - it may contain a connection
            # string mid-corruption - so the caught exception is replaced with a fixed message,
            # the same discipline Deploy-BeeDay.ps1 already applies to SqlConnectionStringBuilder
            # parse failures.
            $script:currentStage = 'PARSE_PAYLOAD'
            $script:currentErrorCode = 'PAYLOAD_INVALID'
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
            $script:currentStage = 'VALIDATE_REQUEST_ID'
            $script:currentErrorCode = 'REQUEST_ID_INVALID'
            $requestIdProperty = $parsedPayload.PSObject.Properties['requestId']
            if (-not $requestIdProperty -or [string]::IsNullOrWhiteSpace([string]$requestIdProperty.Value)) {
                throw "Environment configuration payload is missing requestId."
            }

            $payloadRequestGuid = [guid]::Empty
            if (-not [guid]::TryParse([string]$requestIdProperty.Value, [ref]$payloadRequestGuid)) {
                throw "Environment configuration payload requestId is not a well-formed GUID."
            }

            if ($payloadRequestGuid -ne $parsedGuid) {
                $script:currentErrorCode = 'REQUEST_ID_MISMATCH'
                throw "Environment configuration payload requestId does not match the request just read from request.txt - rejecting a stale or mismatched payload."
            }

            $script:currentStage = 'VALIDATE_VARIABLES'
            $script:currentErrorCode = 'PAYLOAD_INVALID'
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

            # Set-BeeDayAppPoolEnvironmentVariables advances $script:currentStage/$currentErrorCode
            # itself through VALIDATE_VARIABLES (allow-list) / READ_EXISTING_ENV /
            # SNAPSHOT_EXISTING_ENV / REMOVE_EXISTING_ENV / ADD_ENV_VARIABLE as it goes.
            Set-BeeDayAppPoolEnvironmentVariables -Variables $variablesToApply -RequestId $requestId
            $tryBlockSucceeded = $true
        }
        finally {
            # Runs on every path out of the try block above: success, a thrown exception from
            # parsing/validation/allow-list/WebAdministration, or anything else. Never logs
            # $payloadRaw or any prior content - only whether the overwrite itself succeeded.
            # $ErrorActionPreference is "Stop" script-wide, so Write-Error here would itself become
            # a terminating error and could mask whatever exception is already propagating out of
            # the try block - the failure is recorded in a flag instead and only acted on below,
            # after the finally block has fully completed.
            if ($tryBlockSucceeded) {
                $script:currentStage = 'SECRET_CLEANUP'
                $script:currentErrorCode = 'SECRET_CLEANUP_FAILED'
            }
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

        $script:currentStage = $null
        $script:currentErrorCode = $null
    }
    else {
        # RESTORE: never touches env-config.secret/request.txt beyond the correlation already
        # handled above - it reads only env-config-snapshot.secret (see the header comment and
        # Restore-BeeDayAppPoolEnvironmentVariables), which the caller cannot write, read, or
        # otherwise influence. Import-Module failure here is tagged the same way STOP/START tag it
        # above, for the same reason (RESTORE has no secret payload of its own to protect via a
        # finally block).
        $script:currentStage = 'IMPORT_WEBADMINISTRATION'
        $script:currentErrorCode = 'IIS_IMPORT_FAILED'
        Import-Module WebAdministration -ErrorAction Stop
        $script:currentStage = $null
        $script:currentErrorCode = $null

        Restore-BeeDayAppPoolEnvironmentVariables -RequestId $requestId
    }

    # Common to STOP/START/CONFIGURE/RESTORE alike - a failure here (state query) is tagged FINALIZE
    # rather than left attributed to whichever operation-specific stage happened to run last.
    $script:currentStage = 'FINALIZE'
    $script:currentErrorCode = $null

    if ($operation -eq 'CONFIGURE' -or $operation -eq 'RESTORE') {
        # No target site/pool state to compare against - reaching this point without an exception
        # is success. The settle delay and single read are kept exactly as before - only for
        # visibility, to keep the result schema uniform with STOP/START - and never used to decide
        # $success.
        Start-Sleep -Seconds 2
        $siteState = (Get-WebsiteState -Name $SiteName -ErrorAction Stop).Value
        $poolState = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop).Value
        $success = $true
    }
    else {
        # STOP/START converge to Site=$expectedState and Pool=$expectedState asynchronously - IIS
        # can still report a transitional state (e.g. Pool=Stopping right after Stop-WebAppPool
        # returns) for a few seconds. A single read after a fixed delay (the previous behavior)
        # could observe that transitional state and report STATE_MISMATCH even though the site/pool
        # would have converged moments later - confirmed on SERV3WEB (Site=Stopped/Pool=Stopping on
        # the first deploy attempt, both Stopped on an immediate retry). Poll instead, up to
        # $finalizeTimeoutSeconds, so real-but-slow convergence is not misreported as a failure;
        # only a genuine non-convergence within that window still fails.
        $expectedState = if ($operation -eq 'STOP') { 'Stopped' } else { 'Started' }
        $finalizeTimeoutSeconds = 30
        $finalizePollIntervalSeconds = 1
        $finalizeDeadline = (Get-Date).AddSeconds($finalizeTimeoutSeconds)

        while ($true) {
            $siteState = (Get-WebsiteState -Name $SiteName -ErrorAction Stop).Value
            $poolState = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop).Value
            $success = ($siteState -eq $expectedState) -and ($poolState -eq $expectedState)

            if ($success -or (Get-Date) -ge $finalizeDeadline) {
                break
            }

            Start-Sleep -Seconds $finalizePollIntervalSeconds
        }
    }

    $script:currentStage = $null
    $script:currentErrorCode = $null

    if (-not $success) {
        # STOP/START only - CONFIGURE/RESTORE always have $success=$true above. The transitional
        # steps for STOP/START now tolerate transient errors and treat this FINALIZE read as the
        # sole authoritative check (see the comment above the STOP/START block); if the site/pool
        # genuinely never converged to the target state, that is a real failure and is now tagged
        # distinctly, rather than the errorStage=null/errorCode=null this path used to report (this
        # branch never went through the outer catch below, so nothing used to set them here).
        Write-BeeDayIisControlResult -RequestId $requestId -Operation $operation `
            -ExitCode 1 -SiteState $siteState -PoolState $poolState `
            -ErrorStage 'FINALIZE' -ErrorCode 'STATE_MISMATCH'
        # -ErrorAction Continue overrides the script-wide $ErrorActionPreference = "Stop" for this
        # one call - without it, Write-Error itself becomes a terminating error (same hazard already
        # documented above CONFIGURE's finally block) that would skip exit 1 below and fall into the
        # outer catch, which would then overwrite the accurate result.json just written above
        # (siteState/poolState=real values, errorStage=FINALIZE, errorCode=STATE_MISMATCH) with its
        # generic fallback (siteState/poolState="Unknown", errorStage=$null, errorCode=UNKNOWN_FAILURE)
        # - this is exactly the failure mode observed on SERV3WEB.
        Write-Error "Operation '$operation' completed but final state was not fully '$expectedState' (site=$siteState, pool=$poolState)." -ErrorAction Continue
        exit 1
    }

    Write-BeeDayIisControlResult -RequestId $requestId -Operation $operation `
        -ExitCode 0 -SiteState $siteState -PoolState $poolState

    exit 0
}
catch {
    $failedRequestId = if ($requestId) { $requestId } else { "unknown" }
    $failedOperation = if ($operation) { $operation } else { "unknown" }

    # $script:currentStage/$currentErrorCode were set BEFORE the operation that actually threw -
    # never derived from $_ here. $currentErrorCode falls back to the fixed constant
    # UNKNOWN_FAILURE (never to anything built from $_.Exception.Message) for any failure point
    # that predates the first stage marker, or that isn't one of the stages this diagnostic feature
    # covers (e.g. an unexpected STOP/START failure).
    $failedErrorCode = if ($script:currentErrorCode) { $script:currentErrorCode } else { 'UNKNOWN_FAILURE' }

    Write-BeeDayIisControlResult -RequestId $failedRequestId -Operation $failedOperation `
        -ExitCode 1 -SiteState "Unknown" -PoolState "Unknown" `
        -ErrorStage $script:currentStage -ErrorCode $failedErrorCode

    # -ErrorAction Continue for the same reason as the STATE_MISMATCH branch above: result.json was
    # already written once, correctly, immediately above - a terminating Write-Error here has no
    # further catch to fall into, but would still skip exit 1 below and let PowerShell's own uncaught-
    # exception unwind decide the process exit code instead of the explicit one below.
    Write-Error "Privileged IIS control failed: $($_.Exception.Message)" -ErrorAction Continue
    exit 1
}
