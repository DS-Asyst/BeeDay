param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PublishPath,

    [Parameter(Mandatory = $true)]
    [ValidatePattern("^https://")]
    [string]$PublicBaseUrl,

    # Resend is optional: an environment that doesn't send transactional email through it (e.g.
    # Homologation today, which runs Resend.Enabled=false / Development.Enabled=true) simply
    # doesn't pass these. Leaving both ApiKey and FromAddress empty means Set-BeeDayEnvironmentVariables
    # skips the Resend variables entirely rather than overwriting the App Pool with blanks — the
    # existing IIS configuration for Resend is left exactly as it is. Passing them (as
    # deploy-prd.yml already does) enables Resend the same way it always has.
    [string]$ResendApiKey,
    [string]$ResendFromAddress,
    [string]$ResendFromName = "BeeDay",

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$AllowedHosts,

    # Environment-shaping parameters. Defaults reproduce the exact values this script hardcoded
    # before it became reusable, so deploy-prd.yml (unmodified, no new arguments) keeps behaving
    # byte-for-byte the same. deploy-hmg.yml overrides all of these explicitly.
    [ValidateNotNullOrEmpty()]
    [string]$SiteName = "BeeDay",

    [ValidateNotNullOrEmpty()]
    [string]$AppPoolName = "BeeDayPool",

    [ValidateNotNullOrEmpty()]
    [string]$DestinationPath = "C:\Apps\BeeDay",

    # ASPNETCORE_ENVIRONMENT/DOTNET_ENVIRONMENT for the App Pool. Default is "Homologation", not
    # "Production" — that mirrors what is actually committed and running today, because SERV3WEB
    # was, until deploy-hmg.yml existed, the only real deploy target this script had. Correcting
    # this default to a true production value is deliberately out of scope here (tracked
    # separately, together with the rest of deploy-prd.yml's eventual Azure rework).
    [ValidateNotNullOrEmpty()]
    [string]$Environment = "Homologation",

    [ValidateNotNullOrEmpty()]
    [string]$HealthCheckUrl = "http://127.0.0.1/health/ready",

    # Sent as the Host header on the health check request — lets the loopback URL above still
    # route to the right IIS site by host binding. Pass an empty string to disable the override,
    # e.g. when HealthCheckUrl already targets the real public domain (its own Host header is
    # then correct on its own).
    [string]$HealthCheckHostHeader = "beeday",

    # Runtime connection string for the application itself (e.g. the beeday_hmg SQL login) —
    # optional and empty by default so deploy-prd.yml (which doesn't pass it) is unaffected; when
    # provided, it is written as an App Pool environment variable and never touches
    # appsettings.*.json. Must never be the same value as MigrationConnectionString below.
    [string]$AppConnectionString,

    # Migrations. The application never runs them — only beeday_hmg_migrator (or the production
    # equivalent) does, via a connection string that is never the app's own. Applied by executing
    # an EF Core migration bundle (a self-sufficient executable produced by BeeDay CI, embedding
    # the compiled migrations and model) rather than `dotnet ef database update` - the bundle needs
    # no project restore, no NuGet access, and no .NET SDK on this machine, only the shared .NET
    # runtime. MigrationBundlePath is where deploy-hmg.yml downloads that artifact to.
    [switch]$RunMigrations,
    [string]$MigrationConnectionString,
    [string]$MigrationBundlePath,

    # Database backup. Implemented but intentionally not wired into any workflow yet — BACKUP
    # DATABASE runs on the SQL Server itself, so DatabaseBackupDirectory must be a path that
    # already exists on that server's disk, which isn't provisioned yet. Left disabled by default
    # so its absence never blocks an application deploy.
    [switch]$BackupDatabase,
    [string]$DatabaseBackupDirectory
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($RunMigrations -and [string]::IsNullOrWhiteSpace($MigrationConnectionString)) {
    throw "MigrationConnectionString is required when RunMigrations is set."
}

if ($RunMigrations -and [string]::IsNullOrWhiteSpace($MigrationBundlePath)) {
    throw "MigrationBundlePath is required when RunMigrations is set."
}

if ($BackupDatabase -and [string]::IsNullOrWhiteSpace($DatabaseBackupDirectory)) {
    throw "DatabaseBackupDirectory is required when BackupDatabase is set."
}

if (-not [string]::IsNullOrWhiteSpace($AppConnectionString) `
    -and -not [string]::IsNullOrWhiteSpace($MigrationConnectionString) `
    -and $AppConnectionString -eq $MigrationConnectionString) {
    throw "AppConnectionString and MigrationConnectionString must not be the same value - the application must never use the migrator credential."
}

# Exception messages can echo back raw parameter values verbatim (e.g. a malformed connection
# string thrown by SqlConnectionStringBuilder, or a driver error that embeds its input). GitHub
# Actions masks known secrets in the runner's own log capture, but that masking never reaches
# $deployLogsPath below - it's a plain file written directly to disk on the deploy target, outside
# any log pipeline GitHub controls. Every message that reaches Write-DeployMessage or Write-Error
# is scrubbed of these literal values first, so the real error text is preserved but a credential
# can never end up persisted on disk in the clear.
$script:secretValuesToRedact = @($MigrationConnectionString, $AppConnectionString, $ResendApiKey) |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

function Protect-DeploySecret {
    param([AllowEmptyString()][Parameter(Mandatory = $true)][string]$Message)

    $sanitized = $Message
    foreach ($secret in $script:secretValuesToRedact) {
        $sanitized = $sanitized.Replace($secret, "[REDACTED]")
    }

    return $sanitized
}

$currentIdentityName = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name

# SERV3WEB restricts applicationHost.config/redirection.config to TrustedInstaller/SYSTEM/
# Administrators only - this account cannot be added to Administrators or granted any access there,
# so it cannot control IIS directly (Stop-Website/Start-WebAppPool both need to read and commit
# changes through that config store). Controlling BeeDay-HMG / BeeDay-Web-AppPool instead goes
# through a single privileged Scheduled Task (\BeeDay\HMG-IisControl, runs as SYSTEM) that this
# account can only trigger and query - never modify - via a Generic Read + Generic Execute grant on
# that one task's own security descriptor (a completely separate ACL namespace from IIS config).
# These constants are used only when $SiteName/$AppPoolName are exactly these two literals (see
# Stop-BeeDayIis/Start-BeeDayIis below); any other target keeps using direct WebAdministration calls
# exactly as before, so deploy-prd.yml's current behavior is unaffected.
$privilegedIisSiteName = "BeeDay-HMG"
$privilegedIisAppPoolName = "BeeDay-Web-AppPool"
$privilegedIisTaskPath = "\BeeDay\"
$privilegedIisTaskName = "HMG-IisControl"
$privilegedIisRequestPath = "C:\Ops\BeeDay\IisControl\Requests\request.txt"
$privilegedIisResultPath = "C:\Ops\BeeDay\IisControl\Results\result.json"

# Carries the CONFIGURE operation's payload (App Pool environment variables, including
# BeeDay__Persistence__SqlServer__ConnectionString when set) - never request.txt/result.json, and
# never logged. Same trust model as request.txt: svc_beeday_runner can only overwrite its content
# (Write Data, no Read Data), so it can never read back what it wrote; SYSTEM reads it, applies it,
# and immediately overwrites it with an empty placeholder afterwards.
$privilegedIisEnvConfigPath = "C:\Ops\BeeDay\IisControl\Requests\env-config.secret"

# Timing/retry knobs for Invoke-BeeDayPrivilegedIisControl below. Split into two distinct budgets
# per a real SERV3WEB race: (1) time allowed waiting for a PREVIOUS run to leave State=Running
# before we ever trigger, and (2) time allowed waiting for OUR OWN triggered run to go from
# Start-ScheduledTask back to State=Ready. Confounding these into one timeout would make the error
# messages useless for telling "the last operation is stuck" apart from "this operation is stuck".
$privilegedIisPollIntervalSeconds = 2
$privilegedIisReadyTimeoutSeconds = 60
$privilegedIisRunTimeoutSeconds = 60

# MultipleInstances=IgnoreNew can still silently swallow a trigger even after waiting for
# State=Ready (a Start-ScheduledTask call can lose a narrow race against the task engine's own
# internal state settling) - bounded retry lets that specific case self-heal instead of failing
# the whole deploy, without ever allowing two executions to run concurrently (each retry only
# fires after Ready is re-confirmed).
$privilegedIisMaxTriggerAttempts = 2

$backupRoot = "C:\Apps\BeeDay-Backups"
$externalRoot = "C:\Apps\BeeDay-Data"
$dataPath = Join-Path $externalRoot "Data"
$dataBackupRoot = Join-Path $backupRoot "Data"
$applicationBackupRoot = Join-Path $backupRoot "Application"
$deployLogsPath = Join-Path $externalRoot "DeployLogs"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$applicationBackupPath = Join-Path $applicationBackupRoot "BeeDay-$timestamp"
$dataBackupPath = Join-Path $dataBackupRoot "BeeDay-Data-$timestamp"
$deployStartedAt = Get-Date
$deploymentSucceeded = $false
$script:logFilePath = $null

$externalDirectories = @(
    $dataPath,
    (Join-Path $dataPath "Backups"),
    (Join-Path $externalRoot "DataProtection-Keys"),
    (Join-Path $externalRoot "Emails"),
    (Join-Path $externalRoot "Logs"),
    $deployLogsPath
)

function Start-DeployLog {
    param([Parameter(Mandatory = $true)][string]$Directory)

    New-Item -ItemType Directory -Path $Directory -Force | Out-Null
    $script:logFilePath = Join-Path $Directory "Deploy-$timestamp.log"
}

function Write-DeployMessage {
    param([Parameter(Mandatory = $true)][string]$Message)

    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $(Protect-DeploySecret $Message)"
    Write-Host ""
    Write-Host $line

    if ($script:logFilePath) {
        Add-Content -LiteralPath $script:logFilePath -Value $line
    }
}

function Test-BeeDayUsesPrivilegedIisControl {
    return ($SiteName -eq $privilegedIisSiteName) -and ($AppPoolName -eq $privilegedIisAppPoolName)
}

# Requests a STOP or START from the privileged Scheduled Task and blocks until it either confirms
# success or the timeout elapses - the task itself is asynchronous (Start-ScheduledTask returns
# immediately), so this polls Task Scheduler's own state/LastRunTime/LastTaskResult (native
# mechanisms) plus a small result file the task writes, rather than inventing a custom IPC protocol.
#
# Never passes $SiteName/$AppPoolName (or anything else) to the task: the only data that crosses
# this boundary is the fixed operation string and a correlation GUID, written into request.txt's
# content, which the task then validates against its own hardcoded STOP/START allow-list. There is
# no argument, script block, or path handed to the task from here - Start-ScheduledTask takes only
# the task's identity.
#
# request.txt is a permanent fixture (pre-created by provisioning), overwritten in place rather than
# written-then-renamed: svc_beeday_runner's ACL on it is Write Data only (no Delete/Create anywhere
# in that folder), and a same-volume rename-replace would need Delete on the file being replaced -
# which this account deliberately does not have.
#
# The request write itself uses a raw FileStream instead of Set-Content: confirmed on SERV3WEB that
# Set-Content fails for this account even with Write Data granted (the cmdlet opens the destination
# in a way that also needs Read Data, most likely for encoding/BOM detection - access this account
# deliberately does not have, since request.txt must never be readable by the runner). A FileStream
# opened with exactly FileMode.Open (never creates - the file must already be pre-provisioned) and
# FileAccess.Write matches the narrow grant exactly and was confirmed working. The result read below
# uses [System.IO.File]::ReadAllText for the same reason: Get-Content needed Read Extended
# Attributes in addition to Read Data to succeed for this account.
function Write-BeeDayFileStreamContent {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content
    )

    $contentBytes = [System.Text.Encoding]::UTF8.GetBytes($Content)

    # SetLength(0) truncates before writing so a shorter value never leaves trailing bytes from a
    # longer previous one. Dispose runs in finally so the handle is always released, success or
    # failure. FileMode.Open never creates - the target must already be pre-provisioned.
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

# Blocks until the privileged task's State (Get-ScheduledTask, NOT Get-ScheduledTaskInfo) reaches
# $TargetState - almost always 'Ready' here. Deliberately state-based rather than
# LastRunTime-based: a real SERV3WEB incident showed Get-ScheduledTaskInfo's LastRunTime failing
# to advance for a run that had, in fact, genuinely executed and produced a correctly-correlated
# result.json, so LastRunTime is treated as diagnostic-only everywhere in this file - never as a
# gate. 'Disabled' fails immediately (waiting out the timeout would never help); any other
# non-target state (Running/Queued/Unknown) is polled until $TargetState or the timeout.
function Wait-BeeDayPrivilegedIisTaskState {
    param(
        [Parameter(Mandatory = $true)][string]$TargetState,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds,
        [Parameter(Mandatory = $true)][string]$TimeoutMessage
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ($true) {
        $observedState = (Get-ScheduledTask -TaskPath $privilegedIisTaskPath -TaskName $privilegedIisTaskName -ErrorAction Stop).State
        if ($observedState -eq $TargetState) {
            return
        }
        if ($observedState -eq 'Disabled') {
            throw "Privileged IIS control task '$privilegedIisTaskPath$privilegedIisTaskName' is Disabled - cannot proceed."
        }
        if ((Get-Date) -ge $deadline) {
            throw "$TimeoutMessage (last observed state: $observedState, waited ${TimeoutSeconds}s)."
        }
        Start-Sleep -Seconds $privilegedIisPollIntervalSeconds
    }
}

function Invoke-BeeDayPrivilegedIisControl {
    param(
        [Parameter(Mandatory = $true)][ValidateSet('STOP', 'START', 'CONFIGURE')][string]$Operation,

        # Only used for CONFIGURE. Written to env-config.secret (never request.txt/result.json,
        # never logged) before request.txt is written, so it's already in place by the time the
        # privileged task reads the operation and acts on it.
        [hashtable]$EnvironmentVariables
    )

    if ($Operation -eq 'CONFIGURE' -and -not $EnvironmentVariables) {
        throw "EnvironmentVariables is required when Operation is CONFIGURE."
    }

    Write-DeployMessage "Requesting privileged IIS control: $Operation (site=$privilegedIisSiteName, pool=$privilegedIisAppPoolName)..."

    # Wait for any previous invocation (ours or a stray manual trigger) to fully leave Running -
    # a one-shot point-in-time check here is exactly the race that hit SERV3WEB in practice: STOP's
    # result.json was already valid and had been read successfully, but the Scheduled Task's own
    # State had not yet settled back to Ready, so the immediately-following CONFIGURE's
    # Start-ScheduledTask call was silently swallowed by MultipleInstances=IgnoreNew.
    Wait-BeeDayPrivilegedIisTaskState -TargetState 'Ready' -TimeoutSeconds $privilegedIisReadyTimeoutSeconds `
        -TimeoutMessage "Privileged IIS control task ($Operation) could not be started because a previous run never became Ready"

    # Generated once, shared by both files below - this is what lets the privileged script prove
    # the env-config.secret payload it's about to apply is the one written for THIS invocation, not
    # a stale leftover from an earlier CONFIGURE that failed before being invalidated. It is also
    # now the PRIMARY (not merely supporting) proof that a given result.json belongs to this
    # invocation - see the correlation loop below. LastRunTime/LastTaskResult are still read for
    # diagnostics and for the final exit-code check, but a real SERV3WEB run demonstrated
    # Get-ScheduledTaskInfo.LastRunTime failing to advance for a run that had, in fact, genuinely
    # executed and produced a correctly-correlated result.json - so LastRunTime alone must never be
    # able to reject a result whose requestId matches.
    $requestId = [guid]::NewGuid().ToString()

    if ($Operation -eq 'CONFIGURE') {
        # Written BEFORE request.txt, and never through Write-DeployMessage/Write-Host - only the
        # variable count is logged, never names or values. requestId travels inside the payload
        # (not just in request.txt) so the privileged script can reject a stale/mismatched payload
        # before ever reading $variables out of it.
        Write-DeployMessage "Writing App Pool environment configuration payload ($($EnvironmentVariables.Count) variable(s))..."
        $payload = [ordered]@{
            requestId = $requestId
            variables = $EnvironmentVariables
        }
        $payloadJson = $payload | ConvertTo-Json -Compress
        Write-BeeDayFileStreamContent -Path $privilegedIisEnvConfigPath -Content $payloadJson
    }

    Write-BeeDayFileStreamContent -Path $privilegedIisRequestPath -Content "$Operation`n$requestId"

    # Bounded retry: each iteration triggers the task, waits for it to return to Ready, and then
    # checks whether result.json actually correlates with THIS invocation (requestId + operation).
    # A mismatch after Ready means the trigger was swallowed (Start-ScheduledTask lost a narrow
    # race against the task engine's own state settling, even after the wait above) - Ready is
    # re-confirmed before every retry, so a retry can never overlap a still-running execution and
    # MultipleInstances=IgnoreNew is never relied upon to protect against a double-fire from here.
    $result = $null
    $attempt = 0
    while ($true) {
        $attempt++

        # Diagnostic only (see comment on $requestId above) - never used to accept or reject a
        # result below.
        $preTriggerLastRunTime = (Get-ScheduledTaskInfo -TaskPath $privilegedIisTaskPath -TaskName $privilegedIisTaskName).LastRunTime

        try {
            Start-ScheduledTask -TaskPath $privilegedIisTaskPath -TaskName $privilegedIisTaskName -ErrorAction Stop
        }
        catch {
            throw "Privileged IIS control task ($Operation) could not be triggered (attempt $attempt): $($_.Exception.Message)"
        }

        # This intentionally does not distinguish "never started" from "ran and finished" - State
        # alone cannot make that distinction reliably (a fast run can already be back to Ready by
        # the first poll). That distinction is made below, from result.json's requestId, which is
        # the one signal proven trustworthy for it.
        Wait-BeeDayPrivilegedIisTaskState -TargetState 'Ready' -TimeoutSeconds $privilegedIisRunTimeoutSeconds `
            -TimeoutMessage "Privileged IIS control task ($Operation) exceeded its execution timeout (attempt $attempt)"

        if (-not (Test-Path -LiteralPath $privilegedIisResultPath)) {
            throw "Privileged IIS control task ($Operation) finished but produced no result file (attempt $attempt)."
        }

        # Get-Content also failed for this account on SERV3WEB even with Read Data granted (needed
        # Read Extended Attributes too - now granted by provisioning); ReadAllText matches the
        # narrow ACL and was confirmed working.
        $candidateResult = [System.IO.File]::ReadAllText($privilegedIisResultPath) | ConvertFrom-Json

        if ($candidateResult.requestId -eq $requestId -and $candidateResult.operation -eq $Operation) {
            $result = $candidateResult
            break
        }

        $postAttemptLastRunTime = (Get-ScheduledTaskInfo -TaskPath $privilegedIisTaskPath -TaskName $privilegedIisTaskName).LastRunTime
        Write-DeployMessage "Privileged IIS control task ($Operation) attempt $attempt produced no correlated result (result.json has requestId=$($candidateResult.requestId), operation=$($candidateResult.operation); expected requestId=$requestId; LastRunTime before=$preTriggerLastRunTime after=$postAttemptLastRunTime, diagnostic only) - the trigger was likely swallowed by MultipleInstances=IgnoreNew."

        if ($attempt -ge $privilegedIisMaxTriggerAttempts) {
            throw "Privileged IIS control task ($Operation) never actually started a new run after $attempt attempt(s) - no result.json update correlates with requestId $requestId. Refusing to trust an unrelated result."
        }

        # Re-confirm Ready before retrying - the same guard as the very first wait above, applied
        # again defensively so the retry's own trigger cannot overlap a run that is somehow still
        # in flight.
        Wait-BeeDayPrivilegedIisTaskState -TargetState 'Ready' -TimeoutSeconds $privilegedIisReadyTimeoutSeconds `
            -TimeoutMessage "Privileged IIS control task ($Operation) did not return to Ready before retrying the trigger (after attempt $attempt)"
    }

    # Read only after Ready + requestId correlation are both established above - LastTaskResult is
    # a secondary check alongside result.exitCode, never the sole basis for trusting the outcome.
    $taskInfo = Get-ScheduledTaskInfo -TaskPath $privilegedIisTaskPath -TaskName $privilegedIisTaskName

    Write-DeployMessage "Privileged IIS control result: operation=$($result.operation) exitCode=$($result.exitCode) siteState=$($result.siteState) poolState=$($result.poolState)"

    if ($taskInfo.LastTaskResult -ne 0 -or $result.exitCode -ne 0) {
        throw "Privileged IIS control task ($Operation) failed (LastTaskResult=$($taskInfo.LastTaskResult), reported exitCode=$($result.exitCode)). Site state: $($result.siteState), Pool state: $($result.poolState)."
    }
}

function Stop-BeeDayIis {
    if (Test-BeeDayUsesPrivilegedIisControl) {
        Invoke-BeeDayPrivilegedIisControl -Operation "STOP"
        return
    }

    Write-DeployMessage "Stopping IIS site '$SiteName'..."
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue

    Write-DeployMessage "Stopping application pool '$AppPoolName'..."
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue

    Start-Sleep -Seconds 3
}

function Start-BeeDayIis {
    if (Test-BeeDayUsesPrivilegedIisControl) {
        Invoke-BeeDayPrivilegedIisControl -Operation "START"
        return
    }

    $appPoolState = Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue
    if ($null -eq $appPoolState) {
        throw "Application pool was not found: $AppPoolName"
    }

    if ($appPoolState.Value -ne "Started") {
        Write-DeployMessage "Starting application pool '$AppPoolName'..."
        Start-WebAppPool -Name $AppPoolName
    }

    $siteState = Get-WebsiteState -Name $SiteName -ErrorAction SilentlyContinue
    if ($null -eq $siteState) {
        throw "IIS site was not found: $SiteName"
    }

    if ($siteState.Value -ne "Started") {
        Write-DeployMessage "Starting IIS site '$SiteName'..."
        Start-Website -Name $SiteName
    }
}

function Copy-DirectoryContents {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )

    New-Item -ItemType Directory -Path $Destination -Force | Out-Null

    Get-ChildItem -LiteralPath $Source -Force -ErrorAction SilentlyContinue |
        ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $Destination -Recurse -Force
        }
}

function Clear-DirectoryContents {
    param([Parameter(Mandatory = $true)][string]$Path)

    Get-ChildItem -LiteralPath $Path -Force -ErrorAction SilentlyContinue |
        Remove-Item -Recurse -Force
}

# NTFS ACLs on the IIS-facing directories are infrastructure and are provisioned administratively,
# once, ahead of time on the deploy target - never by the deploy itself. LAB\svc_beeday_runner
# (the account this script runs as) is deliberately low-privilege and must never be granted
# WRITE_DAC or Full Control just so the deploy can call Set-Acl. These two functions only verify
# that the access the deploy actually needs is already in place; Assert-BeeDayRequiredAccess below
# fails loudly - naming the directory, the expected identity, and the missing right - if it isn't.
#
# Test-BeeDayWriteAccess probes the *effective* access of whichever account is currently running
# (a real create+delete of a throwaway file) rather than pattern-matching a hardcoded account name
# in the ACL - that stays correct even if the runner account is renamed or differs between
# environments, and it also naturally accounts for group membership and inheritance instead of
# requiring an exact identity-string match.
#
# Test-BeeDayAppPoolAccess cannot use the same probe: this script never runs as the IIS AppPool
# identity, so it inspects the target directory's ACL (including inherited entries, which is what
# `.Access` returns) for an explicit Allow entry granting at least the required rights.
function Test-BeeDayWriteAccess {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return $false
    }

    $probeFile = Join-Path $Path ".beeday-acl-probe-$([guid]::NewGuid().ToString('N')).tmp"
    try {
        [System.IO.File]::WriteAllText($probeFile, "")
        return $true
    }
    catch {
        return $false
    }
    finally {
        Remove-Item -LiteralPath $probeFile -Force -ErrorAction SilentlyContinue
    }
}

function Test-BeeDayAppPoolAccess {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$AppPoolIdentity,
        [Parameter(Mandatory = $true)][System.Security.AccessControl.FileSystemRights]$RequiredRights
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return $false
    }

    # Simulates the real NTFS access-check algorithm instead of just looking for "an Allow ACE with
    # the right bits" - a Deny ACE for this identity must be able to veto an Allow that would
    # otherwise satisfy the check, and precedence is explicit deny > explicit allow > inherited deny
    # > inherited allow, not "any Deny beats any Allow". .Access returns ACEs in actual DACL order,
    # which for any canonically-formed ACL (the normal state after icacls/Set-Acl/the ACL editor UI)
    # already reflects that precedence - explicit before inherited - so walking it in order while
    # tracking which requested bits are still pending, exactly like Windows does, reproduces the
    # real evaluation: a Deny ACE that still covers a pending bit fails the whole check immediately;
    # an Allow ACE grants its bits toward the pending set; success once every requested bit has been
    # granted before any covering Deny is reached.
    #
    # Known limitation: this only recognizes the exact identity string passed in, not group
    # membership the identity might inherit rights or denials through (e.g. IIS_IUSRS, which every
    # IIS AppPool virtual identity implicitly belongs to). A Deny scoped to such a group would not
    # be detected here. Resolving full group membership for a virtual AppPool identity from a
    # differently-privileged script is not practical without new tooling/dependencies, so
    # provisioning must not rely on group-scoped Deny ACEs for these directories.
    $acl = Get-Acl -LiteralPath $Path
    $grantedRights = [System.Security.AccessControl.FileSystemRights]0

    foreach ($rule in $acl.Access) {
        if (-not $rule.IdentityReference.Value.Equals($AppPoolIdentity, [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        $pendingRights = $RequiredRights -band (-bnot $grantedRights)

        if ($rule.AccessControlType -eq [System.Security.AccessControl.AccessControlType]::Deny) {
            if (($rule.FileSystemRights -band $pendingRights) -ne 0) {
                return $false
            }
            continue
        }

        $grantedRights = $grantedRights -bor $rule.FileSystemRights

        if (($grantedRights -band $RequiredRights) -eq $RequiredRights) {
            return $true
        }
    }

    return $false
}

# Collects every missing grant before throwing (rather than stopping at the first one) so whoever
# provisions the server on SERV3WEB gets the complete list in a single failed run instead of
# discovering them one deploy at a time.
function Assert-BeeDayRequiredAccess {
    param([Parameter(Mandatory = $true)][string]$AppPoolName)

    $appPoolIdentity = "IIS AppPool\$AppPoolName"

    $writeChecks = @($DestinationPath, $backupRoot, $externalRoot)

    $appPoolChecks = @(
        @{ Path = $DestinationPath; Rights = [System.Security.AccessControl.FileSystemRights]::ReadAndExecute; Description = "Read & Execute" }
        @{ Path = $dataPath; Rights = [System.Security.AccessControl.FileSystemRights]::Modify; Description = "Modify" }
        @{ Path = (Join-Path $externalRoot "DataProtection-Keys"); Rights = [System.Security.AccessControl.FileSystemRights]::Modify; Description = "Modify" }
        @{ Path = (Join-Path $externalRoot "Emails"); Rights = [System.Security.AccessControl.FileSystemRights]::Modify; Description = "Modify" }
        @{ Path = (Join-Path $externalRoot "Logs"); Rights = [System.Security.AccessControl.FileSystemRights]::Modify; Description = "Modify" }
    )

    $missing = @()

    foreach ($path in $writeChecks) {
        if (-not (Test-BeeDayWriteAccess -Path $path)) {
            $missing += "Directory: '$path' | Expected identity: $currentIdentityName (the account running this deploy) | Required right: Modify (write)"
        }
    }

    foreach ($check in $appPoolChecks) {
        if (-not (Test-BeeDayAppPoolAccess -Path $check.Path -AppPoolIdentity $appPoolIdentity -RequiredRights $check.Rights)) {
            $missing += "Directory: '$($check.Path)' | Expected identity: $appPoolIdentity | Required right: $($check.Description)"
        }
    }

    if ($missing.Count -gt 0) {
        $details = $missing -join "`n  "
        throw "Required NTFS permissions are missing. ACLs are provisioned administratively on the deploy target and are never modified by this script. Missing:`n  $details"
    }
}

function Invoke-BeeDayHealthCheck {
    $lastError = $null
    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($HealthCheckHostHeader)) {
        $headers["Host"] = $HealthCheckHostHeader
    }

    for ($attempt = 1; $attempt -le 6; $attempt++) {
        try {
            Write-DeployMessage "Running readiness health check (attempt $attempt of 6): $HealthCheckUrl"

            $response = Invoke-WebRequest `
                -Uri $HealthCheckUrl `
                -Headers $headers `
                -UseBasicParsing `
                -TimeoutSec 20

            if ($response.StatusCode -eq 200) {
                return
            }

            $lastError = "HTTP $($response.StatusCode)"
        }
        catch {
            $lastError = $_.Exception.Message
        }

        Start-Sleep -Seconds 5
    }

    throw "Readiness health check failed after 6 attempts. Last error: $lastError"
}

# Building $variables never itself touches applicationHost.config - it only decides WHAT should be
# set, exactly as before. Applying it does: for BeeDay-HMG that now goes through the same privileged
# Scheduled Task as Stop-BeeDayIis/Start-BeeDayIis (svc_beeday_runner has no access to
# applicationHost.config, confirmed on SERV3WEB - Add-WebConfigurationProperty/
# Remove-WebConfigurationProperty failed with "insufficient permissions" on redirection.config);
# any other target keeps using the direct WebAdministration calls exactly as before.
function Set-BeeDayEnvironmentVariables {
    Write-DeployMessage "Configuring IIS application-pool environment variables (Environment=$Environment)..."

    $variables = @{
        ASPNETCORE_ENVIRONMENT = $Environment
        DOTNET_ENVIRONMENT = $Environment
        AllowedHosts = $AllowedHosts
        BeeDay__IdentityEmail__PublicBaseUrl = $PublicBaseUrl
    }

    # Only set when provided — deploy-prd.yml doesn't pass -AppConnectionString, so this stays
    # absent there and appsettings.Production.json's own resolution is untouched. When present, it
    # overrides appsettings.Homologation.json's committed ConnectionString for this App Pool only,
    # without editing that file.
    if (-not [string]::IsNullOrWhiteSpace($AppConnectionString)) {
        $variables["BeeDay__Persistence__SqlServer__ConnectionString"] = $AppConnectionString
    }

    # Resend stays fully out of the App Pool config when either value is absent, instead of
    # writing blanks over whatever is already configured there — that's what lets Homologation
    # (Resend.Enabled=false / Development.Enabled=true, committed in appsettings.Homologation.json)
    # go untouched today, and lets the exact same parameters turn Resend on later without any
    # script change.
    if (-not [string]::IsNullOrWhiteSpace($ResendApiKey) -and -not [string]::IsNullOrWhiteSpace($ResendFromAddress)) {
        $variables["BeeDay__Email__Resend__ApiKey"] = $ResendApiKey
        $variables["BeeDay__Email__Resend__FromAddress"] = $ResendFromAddress
        $variables["BeeDay__Email__Resend__FromName"] = $ResendFromName
    }

    if (Test-BeeDayUsesPrivilegedIisControl) {
        Invoke-BeeDayPrivilegedIisControl -Operation "CONFIGURE" -EnvironmentVariables $variables
        return
    }

    foreach ($entry in $variables.GetEnumerator()) {
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

# Diagnoses SQL Server error 18456 ("Login failed") without ever touching the credential itself.
# Parses $ConnectionString with SqlConnectionStringBuilder and logs only a fixed allow-list of
# non-secret fields - Server, Database, User, whether a password is present (boolean, never the
# value), Encrypt, TrustServerCertificate - each on its own Write-DeployMessage call rather than
# concatenated into one string, so a single log line can never end up looking like (or containing)
# a real connection string. A wrong Server, Database, or User Id is the far more common cause of a
# "right password, still 18456" report than the password itself, and this is exactly what surfaces
# that without ever printing the value that would prove it.
#
# Never lets the raw exception from a failed parse propagate: some malformed-string parse failures
# can embed fragments of the offending input in their Message, so the catch block below always
# throws a fixed, generic message instead of forwarding $_.Exception.Message.
function Assert-BeeDayMigrationConnection {
    param([Parameter(Mandatory = $true)][string]$ConnectionString)

    try {
        $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($ConnectionString)
    }
    catch {
        throw "MigrationConnectionString could not be parsed as a valid SQL Server connection string."
    }

    $hasPassword = -not [string]::IsNullOrEmpty($builder.Password)

    Write-DeployMessage "Migration connection metadata:"
    Write-DeployMessage "  Server=$($builder.DataSource)"
    Write-DeployMessage "  Database=$($builder.InitialCatalog)"
    Write-DeployMessage "  User=$($builder.UserID)"
    Write-DeployMessage "  PasswordPresent=$hasPassword"
    Write-DeployMessage "  Encrypt=$($builder.Encrypt)"
    Write-DeployMessage "  TrustServerCertificate=$($builder.TrustServerCertificate)"

    $missingFields = @()
    if ([string]::IsNullOrWhiteSpace($builder.DataSource)) { $missingFields += "Server/Data Source" }
    if ([string]::IsNullOrWhiteSpace($builder.InitialCatalog)) { $missingFields += "Database/Initial Catalog" }
    if ([string]::IsNullOrWhiteSpace($builder.UserID)) { $missingFields += "User ID" }
    if (-not $hasPassword) { $missingFields += "Password" }

    if ($missingFields.Count -gt 0) {
        throw "MigrationConnectionString is missing required field(s): $($missingFields -join ', ')."
    }
}

# Applies pending EF Core migrations by running the migration bundle BeeDay CI produced alongside
# this exact publish artifact - never `dotnet ef database update`. The bundle already embeds the
# compiled migrations and model (built and validated in CI, where a full restore is normal and
# cheap), so running it here needs no MSBuild project load, no project.assets.json, and no NuGet
# access at all on this machine - only the shared .NET runtime. $ConnectionString is the migrator
# credential exclusively, passed only at the moment of execution (never written to disk, never
# baked into the bundle) - never the application's own connection string.
#
# The bundle's own stdout/stderr is captured rather than left to stream straight to the console:
# a bad connection string can surface inside the bundle's own error text, and routing every line
# through Write-DeployMessage (which already redacts known secret values) is what keeps it out of
# both the console and $deployLogsPath, exactly like every other message in this script.
function Invoke-BeeDayMigration {
    param(
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [Parameter(Mandatory = $true)][string]$ConnectionString
    )

    if (-not (Test-Path -LiteralPath $BundlePath -PathType Leaf)) {
        throw "Migration bundle was not found: $BundlePath"
    }

    Assert-BeeDayMigrationConnection -ConnectionString $ConnectionString

    Write-DeployMessage "Applying EF Core migrations via the migration bundle..."

    $bundleOutput = & $BundlePath --connection $ConnectionString 2>&1
    $bundleExitCode = $LASTEXITCODE

    foreach ($line in $bundleOutput) {
        Write-DeployMessage "[migration-bundle] $line"
    }

    if ($bundleExitCode -ne 0) {
        throw "Migration bundle failed with exit code $bundleExitCode."
    }

    Write-DeployMessage "Migrations applied successfully."
}

# BACKUP DATABASE runs on the SQL Server itself, not on this machine — BackupDirectory must be a
# path that already exists on that server's disk. Reuses $ConnectionString (the migrator
# credential) for now; a dedicated backup login/permission may replace this once the backup
# directory and retention policy are provisioned on SERV4SQL (tracked separately, not enabled by
# any workflow yet).
function Backup-BeeDayDatabase {
    param(
        [Parameter(Mandatory = $true)][string]$ConnectionString,
        [Parameter(Mandatory = $true)][string]$BackupDirectory
    )

    Import-Module SqlServer -ErrorAction Stop

    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($ConnectionString)
    $serverInstance = $builder.DataSource
    $databaseName = $builder.InitialCatalog
    $backupFile = Join-Path $BackupDirectory "$databaseName-$timestamp.bak"

    Write-DeployMessage "Backing up database '$databaseName' on '$serverInstance' to '$backupFile'..."

    Invoke-Sqlcmd `
        -ServerInstance $serverInstance `
        -Database "master" `
        -Query "BACKUP DATABASE [$databaseName] TO DISK = N'$backupFile' WITH INIT, COMPRESSION, STATS = 10;" `
        -QueryTimeout 0

    Write-DeployMessage "Database backup completed: $backupFile"
}

Start-DeployLog -Directory $deployLogsPath

Write-Host "========================================"
Write-Host "BEEDAY - HARDENED IIS DEPLOYMENT"
Write-Host "========================================"

Write-DeployMessage "Starting deployment (Site=$SiteName, Environment=$Environment)..."

Import-Module WebAdministration

$PublishPath = (Resolve-Path -LiteralPath $PublishPath -ErrorAction Stop).Path

$requiredFiles = @("BeeDay.Web.dll", "web.config")
foreach ($requiredFile in $requiredFiles) {
    $requiredFilePath = Join-Path $PublishPath $requiredFile
    if (-not (Test-Path -LiteralPath $requiredFilePath -PathType Leaf)) {
        throw "Required published file was not found: $requiredFilePath"
    }
}

@($DestinationPath, $backupRoot, $applicationBackupRoot, $dataBackupRoot) + $externalDirectories |
    ForEach-Object { New-Item -ItemType Directory -Path $_ -Force | Out-Null }

Write-DeployMessage "Validating required NTFS permissions (ACLs are provisioned administratively - this deploy never modifies them)..."
Assert-BeeDayRequiredAccess -AppPoolName $AppPoolName

Write-DeployMessage "Backing up current application to '$applicationBackupPath'..."
Copy-DirectoryContents -Source $DestinationPath -Destination $applicationBackupPath

Write-DeployMessage "Backing up persistent data to '$dataBackupPath'..."
Copy-DirectoryContents -Source $dataPath -Destination $dataBackupPath

try {
    if ($BackupDatabase) {
        Backup-BeeDayDatabase -ConnectionString $MigrationConnectionString -BackupDirectory $DatabaseBackupDirectory
    }

    if ($RunMigrations) {
        Invoke-BeeDayMigration -BundlePath $MigrationBundlePath -ConnectionString $MigrationConnectionString
    }

    Stop-BeeDayIis
    Set-BeeDayEnvironmentVariables

    Write-DeployMessage "Replacing application files while preserving external data..."
    Clear-DirectoryContents -Path $DestinationPath
    Copy-DirectoryContents -Source $PublishPath -Destination $DestinationPath

    Start-BeeDayIis
    Invoke-BeeDayHealthCheck

    $deploymentSucceeded = $true
    Write-DeployMessage "Deployment completed successfully."
    Write-Host "Application backup: $applicationBackupPath"
    Write-Host "Data backup: $dataBackupPath"
}
catch {
    $deploymentError = Protect-DeploySecret $_.Exception.Message
    Write-Error "Deployment failed: $deploymentError"

    Write-DeployMessage "Starting rollback to the previous application version..."

    try {
        Stop-BeeDayIis
        Clear-DirectoryContents -Path $DestinationPath
        Copy-DirectoryContents -Source $applicationBackupPath -Destination $DestinationPath
        Start-BeeDayIis
        Invoke-BeeDayHealthCheck
        Write-DeployMessage "Rollback completed and previous version is healthy."
    }
    catch {
        Write-Error "Rollback also failed: $(Protect-DeploySecret $_.Exception.Message)"
    }

    throw "Deployment failed and rollback was attempted. Original error: $deploymentError"
}
finally {
    if (-not $deploymentSucceeded) {
        Write-Host "Persistent data was not replaced. Backup available at: $dataBackupPath"
    }

    $elapsed = (Get-Date) - $deployStartedAt
    Write-DeployMessage "Total deployment time: $($elapsed.ToString())"
}
