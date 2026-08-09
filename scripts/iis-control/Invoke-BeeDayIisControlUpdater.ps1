# =============================================================================
# BeeDay HMG - Privileged IIS Control Updater (SOURCE / TEMPLATE - see note below)
# =============================================================================
#
# THIS FILE IN THE REPOSITORY IS A VERSIONED, AUDITABLE SOURCE COPY ONLY.
# It is NOT deployed automatically by CI/CD. The copy that actually runs lives
# at C:\Ops\BeeDay\IisControlUpdater\Invoke-BeeDayIisControlUpdater.ps1 on
# SERV3WEB, installed and owned exclusively by an administrator (SYSTEM /
# BUILTIN\Administrators = Full Control; LAB\svc_beeday_runner = no access at
# all). Installing or updating that real copy is a deliberate manual
# administrative action - see Provision-BeeDayHmgIisControlUpdater.ps1 in this
# same folder. This script itself is NEVER promoted through the pipeline it
# implements - it has no self-update path, on purpose (same reasoning as
# Invoke-BeeDayIisControl.ps1 never self-updating: a bug in a self-updating
# privileged script could permanently disable the only means of recovering
# it). The one and only thing this script's promotion pipeline is capable of
# updating is Invoke-BeeDayIisControl.ps1 - never itself, never
# Provision-*.ps1, never anything else.
#
# Runs as SYSTEM, triggered on demand by Scheduled Task
# \BeeDay\HMG-IisControl-Updater - a SEPARATE task from \BeeDay\HMG-IisControl
# (the operational STOP/START/CONFIGURE/RESTORE task), with its own ACE on
# its own security descriptor. This separation is deliberate: a bug in
# promotion logic must never be able to affect the operational task's
# availability during a live incident, and vice versa. Neither task, nor the
# directory tree either one owns, is touched by the other's script.
#
# Accepts NO command-line parameters, NO script block, and NO input from the
# caller other than the content of pre-provisioned files under
# C:\Ops\BeeDay\IisControlUpdater\. LAB\svc_beeday_runner can only trigger the
# task (Generic Read + Generic Execute on the task object) and overwrite three
# pre-existing files' content (Write Data granted directly on each -
# Staging\Invoke-BeeDayIisControl.ps1, Staging\manifest.json,
# Requests\promote-request.txt) - identical posture to the operational
# boundary. It has no Create/Delete/List rights anywhere in this tree.
#
# Validation pipeline (every stage is mandatory, in this order):
#   1. promote-request.txt: exactly 2 lines (operation, request id), operation
#      in a fixed allow-list (today: only PROMOTE), request id a well-formed
#      GUID.
#   2. Staging\manifest.json: valid JSON, requestId matches promote-request.txt
#      exactly (rejects a stale manifest left over from an earlier, failed
#      attempt).
#   3. manifest.fileName equals a single fixed literal
#      ("Invoke-BeeDayIisControl.ps1") by strict equality - never used to
#      build a filesystem path (the staged file is always read from a fixed
#      constant path). This is what makes path traversal structurally
#      impossible here, rather than merely filtered.
#   4. SHA-256 of the staged file content equals manifest.sha256.
#   5. The staged content parses as syntactically valid PowerShell
#      ([System.Management.Automation.Language.Parser]::ParseFile) - it is
#      NEVER executed, dot-sourced, or Invoke-Expression'd at any point in
#      this script.
#   6. Idempotency short-circuit: if manifest.sha256 already matches
#      installed-manifest.json's recorded sha256, promotion is a no-op
#      (status=UNCHANGED) - the installed file is never touched.
#   7. Otherwise: back up the currently-installed script (if one exists),
#      install the validated staged content, and verify the installed file's
#      own SHA-256 once more immediately after the write.
#
# Any failure from step 7 onward (i.e. after the live file may have been
# mutated) triggers an automatic restore from the backup just taken, keeping
# the operational boundary in a known-good state - see the outer catch below.
# Any failure at steps 1-6 never touches the live file at all: there is
# nothing to roll back in that case.
#
# Manifest and result files never carry secrets - the promoted content is
# versioned operations code, not a credential. Their purpose is integrity
# (SHA-256) and audit (commit SHA), never confidentiality. Commit SHA is
# recorded for audit only and is never a substitute for the cryptographic
# SHA-256 check above: whoever can merge to `hmg` could already edit this
# file directly - that authorization boundary is branch protection/code
# review on the repository, not this pipeline.
#
# Site/App Pool control (STOP/START/CONFIGURE/RESTORE), request.txt,
# env-config.secret, env-config-snapshot.secret, and the
# \BeeDay\HMG-IisControl task are entirely untouched by this script - they
# live under C:\Ops\BeeDay\IisControl\, a completely separate directory tree
# with its own separate ACL boundary that this script has no special access
# to beyond what SYSTEM already has everywhere.
#
# This script never administers IIS, never runs an arbitrary or
# caller-supplied command, never executes anything read from staging, never
# touches SQL Server, never runs a migration, and never deploys the
# application - its only privileged action is a validated file copy into one
# fixed, allow-listed destination.

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$updaterRoot = "C:\Ops\BeeDay\IisControlUpdater"
$stagingFolder = Join-Path $updaterRoot "Staging"
$requestsFolder = Join-Path $updaterRoot "Requests"
$resultsFolder = Join-Path $updaterRoot "Results"
$backupsFolder = Join-Path $updaterRoot "Backups"

$stagingScriptPath = Join-Path $stagingFolder "Invoke-BeeDayIisControl.ps1"
$stagingManifestPath = Join-Path $stagingFolder "manifest.json"
$requestFilePath = Join-Path $requestsFolder "promote-request.txt"
$resultFilePath = Join-Path $resultsFolder "result.json"
$installedManifestPath = Join-Path $updaterRoot "installed-manifest.json"

# The ONLY file this script is ever capable of promoting - never a parameter, never derived from
# the manifest's own fileName field beyond an equality check against this one literal.
$installedScriptPath = "C:\Ops\BeeDay\IisControl\Invoke-BeeDayIisControl.ps1"
$allowedFileNames = @('Invoke-BeeDayIisControl.ps1')

function Assert-BeeDayNotReparsePoint {
    param([Parameter(Mandatory = $true)][string]$Path)

    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Refusing to process '$Path': it is a reparse point (symlink/junction), which is not permitted here."
    }
}

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

# Structured, fixed-vocabulary result - never $_.Exception.Message. rollbackStatus is only
# meaningful when status=FAILED: NOT_APPLICABLE (nothing was ever mutated), ROLLED_BACK (backup
# restored and re-verified), RESTORE_VERIFICATION_FAILED (restore ran but the post-restore hash
# didn't match the backup's own hash), RESTORE_FAILED (the restore write itself threw) - the last
# two are the one truly bad outcome this script can produce: the boundary may be left without a
# known-good script, and that state must be visible to whoever reads result.json, not swallowed.
function Write-BeeDayIisControlUpdaterResult {
    param(
        [Parameter(Mandatory = $true)][string]$RequestId,
        [Parameter(Mandatory = $true)][string]$Operation,
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][string]$Status,
        [AllowNull()]$Sha256 = $null,
        [AllowNull()]$CommitSha = $null,
        [AllowNull()]$RollbackStatus = $null,
        $ErrorStage = $null,
        $ErrorCode = $null
    )

    $result = [ordered]@{
        requestId      = $RequestId
        operation      = $Operation
        exitCode       = $ExitCode
        status         = $Status
        sha256         = $Sha256
        commitSha      = $CommitSha
        rollbackStatus = $RollbackStatus
        errorStage     = $ErrorStage
        errorCode      = $ErrorCode
        timestamp      = (Get-Date -Format "o")
    }

    # In-place overwrite (truncate the existing file, same file object) - never delete/rename/
    # recreate result.json, for the same reason documented in Invoke-BeeDayIisControl.ps1: that
    # would replace its security descriptor with an inherited one and silently drop the narrow
    # read-only grant given to svc_beeday_runner. SYSTEM has Full Control regardless, so this write
    # is not itself access-restricted; it's written this way purely to keep the file's own ACL
    # stable across runs.
    $result | ConvertTo-Json -Compress | Set-Content -LiteralPath $resultFilePath -Encoding utf8 -NoNewline -Force
}

$script:currentStage = $null
$script:currentErrorCode = $null
$script:backupPath = $null
$script:backupSha256 = $null

$operation = $null
$requestId = $null

try {
    # Neither the Requests folder nor promote-request.txt itself must ever be a reparse point -
    # that would let a lower-privileged writer redirect this SYSTEM-run script at an arbitrary
    # location on disk. svc_beeday_runner cannot create either (no Delete/Create rights anywhere in
    # this path), so this is a sanity check against administrative error, not an attack this
    # account could carry out on its own.
    Assert-BeeDayNotReparsePoint -Path $requestsFolder

    if (-not (Test-Path -LiteralPath $requestFilePath -PathType Leaf)) {
        throw "Request file not found at '$requestFilePath'. It must be pre-provisioned - see Provision-BeeDayHmgIisControlUpdater.ps1."
    }

    Assert-BeeDayNotReparsePoint -Path $requestFilePath

    # Strict two-line format, strict allow-list - same discipline as Invoke-BeeDayIisControl.ps1.
    $requestLines = @(Get-Content -LiteralPath $requestFilePath)
    if ($requestLines.Count -ne 2) {
        throw "No valid pending request (expected exactly 2 lines: operation, request id - got $($requestLines.Count))."
    }

    $operation = $requestLines[0].Trim()
    $requestId = $requestLines[1].Trim()

    if ($operation -notin @('PROMOTE')) {
        throw "Rejected request: '$operation' is not an allowed operation. Only PROMOTE is accepted."
    }

    $script:currentStage = 'VALIDATE_REQUEST_ID'
    $script:currentErrorCode = 'REQUEST_ID_INVALID'
    $parsedGuid = [guid]::Empty
    if (-not [guid]::TryParse($requestId, [ref]$parsedGuid)) {
        throw "Rejected request: request id is not a well-formed GUID."
    }

    # Invalidate immediately after validating, before touching staging - a crash or unexpected exit
    # mid-promotion must never leave a promote-request.txt that a later manual (or accidental) task
    # trigger could re-process.
    Set-Content -LiteralPath $requestFilePath -Value "NONE" -Encoding ascii -NoNewline -Force

    # --- Staging manifest ---
    $script:currentStage = 'READ_MANIFEST'
    $script:currentErrorCode = 'MANIFEST_NOT_FOUND'
    Assert-BeeDayNotReparsePoint -Path $stagingFolder
    if (-not (Test-Path -LiteralPath $stagingManifestPath -PathType Leaf)) {
        throw "Staging manifest not found at '$stagingManifestPath'. It must be pre-provisioned - see Provision-BeeDayHmgIisControlUpdater.ps1."
    }
    Assert-BeeDayNotReparsePoint -Path $stagingManifestPath
    $manifestRaw = Get-Content -LiteralPath $stagingManifestPath -Raw

    $script:currentStage = 'PARSE_MANIFEST'
    $script:currentErrorCode = 'MANIFEST_INVALID'
    try {
        $manifest = $manifestRaw | ConvertFrom-Json
    }
    catch {
        throw "Staging manifest could not be parsed as valid JSON."
    }

    $script:currentStage = 'VALIDATE_MANIFEST_REQUEST_ID'
    $script:currentErrorCode = 'REQUEST_ID_MISMATCH'
    $manifestRequestIdProperty = $manifest.PSObject.Properties['requestId']
    if (-not $manifestRequestIdProperty -or [string]::IsNullOrWhiteSpace([string]$manifestRequestIdProperty.Value)) {
        throw "Staging manifest is missing requestId."
    }
    $manifestRequestGuid = [guid]::Empty
    if (-not [guid]::TryParse([string]$manifestRequestIdProperty.Value, [ref]$manifestRequestGuid)) {
        throw "Staging manifest requestId is not a well-formed GUID."
    }
    if ($manifestRequestGuid -ne $parsedGuid) {
        throw "Staging manifest requestId does not match the request just read from promote-request.txt - rejecting a stale or mismatched manifest."
    }

    $script:currentStage = 'VALIDATE_FILENAME'
    $script:currentErrorCode = 'FILENAME_NOT_ALLOWED'
    $fileNameProperty = $manifest.PSObject.Properties['fileName']
    if (-not $fileNameProperty -or [string]$fileNameProperty.Value -notin $allowedFileNames) {
        throw "Rejected manifest: fileName is not in the allowed list."
    }
    # Fixed constant from here on - $fileName is used only for the audit trail written into
    # installed-manifest.json/result.json below, NEVER to build a filesystem path. Every path this
    # script reads from or writes to is one of the module-level constants declared at the top.
    $fileName = [string]$fileNameProperty.Value

    $script:currentStage = 'VALIDATE_MANIFEST_SHA256'
    $script:currentErrorCode = 'MANIFEST_INVALID'
    $sha256Property = $manifest.PSObject.Properties['sha256']
    if (-not $sha256Property -or [string]::IsNullOrWhiteSpace([string]$sha256Property.Value)) {
        throw "Staging manifest is missing sha256."
    }
    $expectedSha256 = ([string]$sha256Property.Value).ToLowerInvariant()

    $commitShaProperty = $manifest.PSObject.Properties['commitSha']
    $commitSha = if ($commitShaProperty -and -not [string]::IsNullOrWhiteSpace([string]$commitShaProperty.Value)) {
        [string]$commitShaProperty.Value
    }
    else {
        $null
    }

    # --- Staged file ---
    $script:currentStage = 'READ_STAGED_FILE'
    $script:currentErrorCode = 'STAGED_FILE_NOT_FOUND'
    if (-not (Test-Path -LiteralPath $stagingScriptPath -PathType Leaf)) {
        throw "Staged file not found at '$stagingScriptPath'. It must be pre-provisioned - see Provision-BeeDayHmgIisControlUpdater.ps1."
    }
    Assert-BeeDayNotReparsePoint -Path $stagingScriptPath

    $script:currentStage = 'VALIDATE_HASH'
    $script:currentErrorCode = 'HASH_MISMATCH'
    $actualSha256 = Get-BeeDayFileSha256 -Path $stagingScriptPath
    if ($actualSha256 -ne $expectedSha256) {
        throw "Staged file SHA-256 does not match the manifest - refusing to promote unverified content."
    }

    $script:currentStage = 'VALIDATE_SYNTAX'
    $script:currentErrorCode = 'SYNTAX_INVALID'
    $parseTokens = $null
    $parseErrors = $null
    [void][System.Management.Automation.Language.Parser]::ParseFile($stagingScriptPath, [ref]$parseTokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw "Staged file failed PowerShell syntax validation ($($parseErrors.Count) error(s)) - refusing to promote content that does not parse. This is a static syntax check only (the staged content is never executed, dot-sourced, or Invoke-Expression'd)."
    }

    # --- Idempotency short-circuit ---
    # This is a secondary check: Request-BeeDayIisControlPromotion.ps1 already compares against
    # installed-manifest.json before ever triggering this task, so the common case never reaches
    # here at all. This second check is defense in depth against a stale caller-side read or a race
    # between two concurrent deploy attempts - it must never be relied upon as the only place this
    # decision is made, but it must always be correct when it IS reached.
    $script:currentStage = 'READ_INSTALLED_MANIFEST'
    $script:currentErrorCode = 'INSTALLED_MANIFEST_READ_FAILED'
    $installedSha256 = $null
    if (Test-Path -LiteralPath $installedManifestPath -PathType Leaf) {
        Assert-BeeDayNotReparsePoint -Path $installedManifestPath
        try {
            $installedManifestRaw = Get-Content -LiteralPath $installedManifestPath -Raw
            $installedManifest = $installedManifestRaw | ConvertFrom-Json
            $installedSha256Property = $installedManifest.PSObject.Properties['sha256']
            if ($installedSha256Property -and -not [string]::IsNullOrWhiteSpace([string]$installedSha256Property.Value)) {
                $installedSha256 = ([string]$installedSha256Property.Value).ToLowerInvariant()
            }
        }
        catch {
            # A corrupted/unreadable installed-manifest.json must never block a legitimate
            # promotion - fail-safe toward "treat as changed" (promote) rather than trusting an
            # unreadable state, same posture $installedSha256 already has when the file is absent.
            $installedSha256 = $null
        }
    }

    if ($installedSha256 -and $installedSha256 -eq $expectedSha256) {
        $script:currentStage = $null
        $script:currentErrorCode = $null
        Write-BeeDayIisControlUpdaterResult -RequestId $requestId -Operation $operation `
            -ExitCode 0 -Status "UNCHANGED" -Sha256 $expectedSha256 -CommitSha $commitSha
        exit 0
    }

    # --- Backup (only if a version is currently installed) ---
    $script:currentStage = 'ASSERT_BACKUP_WRITABLE'
    $script:currentErrorCode = 'BACKUP_DIRECTORY_UNAVAILABLE'
    if (-not (Test-Path -LiteralPath $backupsFolder -PathType Container)) {
        throw "Backup directory not found: $backupsFolder. It must be pre-provisioned - see Provision-BeeDayHmgIisControlUpdater.ps1."
    }

    # A missing installed script is treated as "nothing to back up", not a hard failure - it can
    # legitimately happen the very first time this pipeline runs after the operational boundary was
    # provisioned in a way that hasn't installed a script yet. Installing is strictly better than
    # leaving the boundary with nothing, so this proceeds rather than refusing.
    if (Test-Path -LiteralPath $installedScriptPath -PathType Leaf) {
        Assert-BeeDayNotReparsePoint -Path $installedScriptPath
        $script:currentStage = 'CREATE_BACKUP'
        $script:currentErrorCode = 'BACKUP_FAILED'
        $backupTimestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $candidateBackupPath = Join-Path $backupsFolder "Invoke-BeeDayIisControl-$backupTimestamp.ps1.bak"
        Copy-Item -LiteralPath $installedScriptPath -Destination $candidateBackupPath -Force
        # Only set once the backup file demonstrably exists on disk - this is the flag the outer
        # catch below uses to decide whether a restore is even possible/needed.
        $script:backupPath = $candidateBackupPath
        $script:backupSha256 = Get-BeeDayFileSha256 -Path $candidateBackupPath
    }

    # --- Install ---
    $script:currentStage = 'INSTALL'
    $script:currentErrorCode = 'INSTALL_FAILED'
    $stagedBytes = [System.IO.File]::ReadAllBytes($stagingScriptPath)
    [System.IO.File]::WriteAllBytes($installedScriptPath, $stagedBytes)

    $script:currentStage = 'VERIFY_INSTALLED_HASH'
    $script:currentErrorCode = 'POST_INSTALL_HASH_MISMATCH'
    $installedSha256AfterWrite = Get-BeeDayFileSha256 -Path $installedScriptPath
    if ($installedSha256AfterWrite -ne $expectedSha256) {
        throw "Installed file SHA-256 does not match the manifest immediately after writing."
    }

    # --- Update installed manifest (real, verified state - not the staging manifest's claim) ---
    $script:currentStage = 'UPDATE_INSTALLED_MANIFEST'
    $script:currentErrorCode = 'INSTALLED_MANIFEST_WRITE_FAILED'
    $installedManifestContent = [ordered]@{
        fileName    = $fileName
        sha256      = $installedSha256AfterWrite
        commitSha   = $commitSha
        requestId   = $requestId
        installedAt = (Get-Date -Format "o")
    }
    $installedManifestContent | ConvertTo-Json -Compress |
        Set-Content -LiteralPath $installedManifestPath -Encoding utf8 -NoNewline -Force

    # Consume the staging manifest so a stray re-trigger of this task (outside a real
    # Request-BeeDayIisControlPromotion.ps1 run) can never reprocess stale content - best-effort
    # hygiene, same discipline Invoke-BeeDayIisControl.ps1 applies to env-config.secret. Not itself
    # a security boundary: promote-request.txt was already invalidated above, and the requestId
    # correlation checked earlier would reject stale manifest content on its own regardless.
    try {
        Set-Content -LiteralPath $stagingManifestPath -Value "{}" -Encoding utf8 -NoNewline -Force
    }
    catch {
        # Intentionally swallowed - hygiene only, not a failure condition.
    }

    $script:currentStage = $null
    $script:currentErrorCode = $null

    Write-BeeDayIisControlUpdaterResult -RequestId $requestId -Operation $operation `
        -ExitCode 0 -Status "INSTALLED" -Sha256 $installedSha256AfterWrite -CommitSha $commitSha

    exit 0
}
catch {
    $failedRequestId = if ($requestId) { $requestId } else { "unknown" }
    $failedOperation = if ($operation) { $operation } else { "unknown" }
    $failedErrorCode = if ($script:currentErrorCode) { $script:currentErrorCode } else { 'UNKNOWN_FAILURE' }
    $failedStage = $script:currentStage

    # If a backup was taken, the live file may already have been mutated (or partially mutated) -
    # restore it unconditionally, regardless of which stage failed from CREATE_BACKUP onward. This
    # never runs for a failure at any earlier validation stage ($script:backupPath stays $null in
    # that case), because nothing was ever touched - there is nothing to roll back.
    $rollbackStatus = "NOT_APPLICABLE"
    if ($script:backupPath) {
        try {
            $restoredBytes = [System.IO.File]::ReadAllBytes($script:backupPath)
            [System.IO.File]::WriteAllBytes($installedScriptPath, $restoredBytes)

            $restoredSha256 = Get-BeeDayFileSha256 -Path $installedScriptPath
            if ($restoredSha256 -eq $script:backupSha256) {
                $rollbackStatus = "ROLLED_BACK"
            }
            else {
                $rollbackStatus = "RESTORE_VERIFICATION_FAILED"
            }
        }
        catch {
            $rollbackStatus = "RESTORE_FAILED"
        }
    }

    try {
        Write-BeeDayIisControlUpdaterResult -RequestId $failedRequestId -Operation $failedOperation `
            -ExitCode 1 -Status "FAILED" -Sha256 $null -CommitSha $null -RollbackStatus $rollbackStatus `
            -ErrorStage $failedStage -ErrorCode $failedErrorCode
    }
    catch {
        # Best-effort - a failure writing result.json must never mask the original error below.
    }

    Write-Error "Privileged IIS control updater failed (stage=$failedStage, rollback=$rollbackStatus): $($_.Exception.Message)"
    exit 1
}
