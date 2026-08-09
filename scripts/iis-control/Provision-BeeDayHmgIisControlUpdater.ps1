# =============================================================================
# BeeDay HMG - Privileged IIS Control Updater: administrative provisioning
# =============================================================================
#
# RUN THIS MANUALLY, AS ADMINISTRATOR, ON SERV3WEB, AFTER
# Provision-BeeDayHmgIisControl.ps1 HAS ALREADY BEEN RUN AT LEAST ONCE. NEVER
# FROM CI/CD, NEVER AS LAB\svc_beeday_runner. This script is versioned for
# auditability only - it is not wired into any workflow and must not be.
#
# Ordering requirement: this script hashes the ALREADY-INSTALLED
# C:\Ops\BeeDay\IisControl\Invoke-BeeDayIisControl.ps1 to seed
# installed-manifest.json with the real, currently-installed state - not an
# assumed or empty value. That file must exist before this script runs (it
# throws immediately if it does not) - see step 0 below.
#
# What it does:
#   0. Verifies C:\Ops\BeeDay\IisControl\Invoke-BeeDayIisControl.ps1 (the
#      operational boundary's installed script) already exists. This script
#      never installs or modifies anything under C:\Ops\BeeDay\IisControl\ -
#      that tree belongs exclusively to Provision-BeeDayHmgIisControl.ps1.
#   1. Ensures C:\Ops\BeeDay\IisControlUpdater exists with SYSTEM/
#      Administrators Full Control, plus a narrow, non-inherited grant for
#      svc_beeday_runner on the ROOT FOLDER OBJECT ITSELF: (RC,RA,X,S) - Read
#      Control, Read Attributes, Traverse Folder, Synchronize. Same rationale
#      as Provision-BeeDayHmgIisControl.ps1's root folder: FILE_TRAVERSE is
#      checked on every ancestor directory when opening a file by path,
#      without which no deeper grant (Staging\, Requests\, Results\) is
#      reachable at all.
#   2. Installs the updater script from this repo checkout into that
#      admin-only location. svc_beeday_runner has no ACE on the file itself.
#   3. Computes the SHA-256 of the ALREADY-INSTALLED
#      C:\Ops\BeeDay\IisControl\Invoke-BeeDayIisControl.ps1 directly (not the
#      repo checkout's copy - the two should be identical right after a fresh
#      Provision-BeeDayHmgIisControl.ps1 run, but the installed file is the
#      one source of truth this script trusts) and writes installed-manifest.json
#      with that real hash, the fixed file name, a best-effort commit SHA
#      (resolved via `git rev-parse HEAD` against this checkout - left null
#      if git is unavailable or this isn't a git working tree), and the
#      current timestamp. svc_beeday_runner gets a narrow read-only grant on
#      this one file - same shape as Results\result.json below - so
#      Request-BeeDayIisControlPromotion.ps1 can compare against it without
#      needing any access into the operational boundary at all.
#   4. Creates Staging\, Requests\, Results\ with inheritance broken from the
#      parent, each granting svc_beeday_runner the same narrow (RC,RA,X,S) on
#      the folder object - enough to traverse into it, not enough to list,
#      create, or delete anything.
#   5. Pre-creates Staging\Invoke-BeeDayIisControl.ps1 (placeholder content),
#      Staging\manifest.json (sentinel "{}"), and Requests\promote-request.txt
#      (sentinel "NONE"), then grants svc_beeday_runner (W,RC,RA) - Write Data
#      + Read Control + Read Attributes, no Read Data - on each. Same grant
#      shape, same reason, as request.txt/env-config.secret in
#      Provision-BeeDayHmgIisControl.ps1: Deploy-BeeDay.ps1-style raw
#      FileStream(FileMode.Open, FileAccess.Write) writes need the full
#      FILE_GENERIC_WRITE mapping (icacls "W"), not just Write Data.
#   6. Pre-creates Results\result.json (sentinel placeholder), grants
#      svc_beeday_runner (S,RC,RD,RA,REA) - Read Data + Read Extended
#      Attributes, no Write Data - same shape as the operational boundary's
#      result.json.
#   7. Creates Backups\ with inheritance broken from the parent, granting ONLY
#      SYSTEM/Administrators Full Control - NO ACE for svc_beeday_runner at
#      all, not even traverse. This script's promotion pipeline never asks the
#      runner to read or write a backup; only Invoke-BeeDayIisControlUpdater.ps1
#      (as SYSTEM) ever touches this folder.
#   8. Registers the \BeeDay\HMG-IisControl-Updater Scheduled Task: on-demand
#      only, no recurring trigger, runs as SYSTEM, RunLevel Highest, a single
#      fixed action, MultipleInstances=IgnoreNew - a SEPARATE task from
#      \BeeDay\HMG-IisControl, with its own independent security descriptor.
#   9. Grants LAB\svc_beeday_runner Generic Read + Generic Execute on THIS ONE
#      task's own security descriptor - enough to query and trigger it, not
#      enough to modify, delete, or view/change its action. This ACE is
#      entirely separate from the operational task's ACE granted by
#      Provision-BeeDayHmgIisControl.ps1.
#
# None of the above ever grants svc_beeday_runner FILE_LIST_DIRECTORY (folder
# enumeration), create/delete/rename rights, WRITE_DAC, or WRITE_OWNER -
# Modify and Full Control are never used for this account anywhere in this
# script. The runner also never receives any access whatsoever into
# C:\Ops\BeeDay\IisControl\ (the operational boundary) or into Backups\ under
# this tree.
#
# Re-run safely: steps are idempotent (Register-ScheduledTask -Force,
# icacls /inheritance:r + explicit grants, New-Item -Force leaves existing
# file content untouched, SDDL append is skipped if already present). Re-
# running this script does NOT refresh installed-manifest.json if it already
# exists - the manifest is meant to reflect the real installed state as
# tracked by the promotion pipeline from that point forward, not to be reset
# by a later provisioning re-run.

#Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

$runnerAccount = "LAB\svc_beeday_runner"
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

$updaterScriptDestination = Join-Path $updaterRoot "Invoke-BeeDayIisControlUpdater.ps1"
$updaterScriptSource = Join-Path $PSScriptRoot "Invoke-BeeDayIisControlUpdater.ps1"

# The operational boundary's installed script - owned exclusively by
# Provision-BeeDayHmgIisControl.ps1, never written by this script. Only read here, once, to seed
# installed-manifest.json with the real state.
$operationalInstalledScriptPath = "C:\Ops\BeeDay\IisControl\Invoke-BeeDayIisControl.ps1"

$stagingScriptSentinel = "# Staging placeholder - overwritten by Request-BeeDayIisControlPromotion.ps1 on every promotion attempt. Never installed as-is: a real promotion requires a correlated, hash-verified manifest.json alongside it."
$stagingManifestSentinel = "{}"
$requestSentinel = "NONE"
$resultSentinel = (
    [ordered]@{
        requestId      = "00000000-0000-0000-0000-000000000000"
        operation      = "NONE"
        exitCode       = 1
        status         = "NONE"
        sha256         = $null
        commitSha      = $null
        rollbackStatus = $null
        timestamp      = "1970-01-01T00:00:00.0000000Z"
    } | ConvertTo-Json -Compress
)

$taskPath = "\BeeDay\"
$taskName = "HMG-IisControl-Updater"

# Minimal, non-inherited folder-object grant used at every level of this path: lets the account
# traverse THROUGH the folder to reach a named child, and lets icacls/Get-Acl succeed when run as
# that account (READ_CONTROL) - never list-directory, never create/delete, never change ACLs.
$folderTraverseGrant = "${runnerAccount}:(RC,RA,X,S)"

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

Write-Host "=== 0. Verifying the operational boundary is already provisioned ==="
if (-not (Test-Path -LiteralPath $operationalInstalledScriptPath -PathType Leaf)) {
    throw "Operational script not found at '$operationalInstalledScriptPath'. Run Provision-BeeDayHmgIisControl.ps1 first - this updater bootstrap seeds installed-manifest.json from the real installed state and cannot proceed without it."
}
Write-Host "Found: $operationalInstalledScriptPath"

Write-Host "`n=== 1. Root folder and updater script (admin-only, runner gets traverse-through only) ==="
New-Item -ItemType Directory -Path $updaterRoot -Force | Out-Null
icacls $updaterRoot /inheritance:r | Out-Null
icacls $updaterRoot /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $updaterRoot /grant $folderTraverseGrant | Out-Null

if (-not (Test-Path -LiteralPath $updaterScriptSource)) {
    throw "Source script not found: $updaterScriptSource. Run this from a checkout of the BeeDay repository."
}
Copy-Item -LiteralPath $updaterScriptSource -Destination $updaterScriptDestination -Force
Write-Host "Installed: $updaterScriptDestination (no ACE for ${runnerAccount} - not readable by it)"

Write-Host "`n=== 2. installed-manifest.json - seeded from the REAL installed state, runner: Read Data only ==="
if (-not (Test-Path -LiteralPath $installedManifestPath)) {
    $installedSha256 = Get-BeeDayFileSha256 -Path $operationalInstalledScriptPath

    $commitSha = $null
    try {
        $gitOutput = & git -C $PSScriptRoot rev-parse HEAD 2>$null
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($gitOutput)) {
            $commitSha = $gitOutput.Trim()
        }
    }
    catch {
        $commitSha = $null
    }

    if ($commitSha) {
        Write-Host "Resolved commit SHA from this checkout: $commitSha"
    }
    else {
        Write-Host "Could not resolve a commit SHA from this checkout (git unavailable or not a working tree) - installed-manifest.json will record commitSha=null. This is expected/acceptable per design (commit SHA is audit-only, best-effort)."
    }

    $installedManifestContent = [ordered]@{
        fileName    = "Invoke-BeeDayIisControl.ps1"
        sha256      = $installedSha256
        commitSha   = $commitSha
        requestId   = $null
        installedAt = (Get-Date -Format "o")
    }
    $installedManifestContent | ConvertTo-Json -Compress |
        Set-Content -LiteralPath $installedManifestPath -Encoding utf8 -NoNewline -Force

    Write-Host "installed-manifest.json seeded with sha256=$installedSha256 (hashed directly from $operationalInstalledScriptPath)."
}
else {
    Write-Host "installed-manifest.json already exists - left untouched (re-run of this script does not reset promotion-tracked state)."
}
icacls $installedManifestPath /grant "${runnerAccount}:(S,RC,RD,RA,REA)" | Out-Null

Write-Host "`n=== 3. Staging folder - svc_beeday_runner: traverse-through only (no list/create/delete) ==="
New-Item -ItemType Directory -Path $stagingFolder -Force | Out-Null
icacls $stagingFolder /inheritance:r | Out-Null
icacls $stagingFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $stagingFolder /grant $folderTraverseGrant | Out-Null

Write-Host "`n=== 3b. Staging\Invoke-BeeDayIisControl.ps1 - pre-created, svc_beeday_runner: Write only (no read/delete) ==="
if (-not (Test-Path -LiteralPath $stagingScriptPath)) {
    Set-Content -LiteralPath $stagingScriptPath -Value $stagingScriptSentinel -Encoding utf8 -NoNewline
}
# (S,RC,WD,RA) alone was confirmed on SERV3WEB to produce Access Denied for the FileStream(Open,
# Write) pattern used against request.txt/env-config.secret in the operational boundary - the same
# reasoning applies here since Request-BeeDayIisControlPromotion.ps1 uses the identical technique.
# (W,RC,RA) is the from-scratch grant confirmed to work.
icacls $stagingScriptPath /grant "${runnerAccount}:(W,RC,RA)" | Out-Null

Write-Host "`n=== 3c. Staging\manifest.json - pre-created, svc_beeday_runner: Write only, same grant as above ==="
if (-not (Test-Path -LiteralPath $stagingManifestPath)) {
    Set-Content -LiteralPath $stagingManifestPath -Value $stagingManifestSentinel -Encoding utf8 -NoNewline
}
icacls $stagingManifestPath /grant "${runnerAccount}:(W,RC,RA)" | Out-Null

Write-Host "`n=== 4. Requests folder - svc_beeday_runner: traverse-through only (no list/create/delete) ==="
New-Item -ItemType Directory -Path $requestsFolder -Force | Out-Null
icacls $requestsFolder /inheritance:r | Out-Null
icacls $requestsFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $requestsFolder /grant $folderTraverseGrant | Out-Null

Write-Host "`n=== 4b. promote-request.txt - pre-created, svc_beeday_runner: Write only (no read/delete) ==="
if (-not (Test-Path -LiteralPath $requestFilePath)) {
    Set-Content -LiteralPath $requestFilePath -Value $requestSentinel -Encoding ascii -NoNewline
}
icacls $requestFilePath /grant "${runnerAccount}:(W,RC,RA)" | Out-Null

Write-Host "`n=== 5. Results folder - svc_beeday_runner: traverse-through only (no list/create/delete) ==="
New-Item -ItemType Directory -Path $resultsFolder -Force | Out-Null
icacls $resultsFolder /inheritance:r | Out-Null
icacls $resultsFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $resultsFolder /grant $folderTraverseGrant | Out-Null

Write-Host "`n=== 5b. result.json - pre-created, svc_beeday_runner: Read Data only (no write/delete) ==="
if (-not (Test-Path -LiteralPath $resultFilePath)) {
    Set-Content -LiteralPath $resultFilePath -Value $resultSentinel -Encoding utf8 -NoNewline
}
icacls $resultFilePath /grant "${runnerAccount}:(S,RC,RD,RA,REA)" | Out-Null

Write-Host "`n=== 6. Backups folder - admin-only, NO ACE for ${runnerAccount} at all ==="
New-Item -ItemType Directory -Path $backupsFolder -Force | Out-Null
icacls $backupsFolder /inheritance:r | Out-Null
icacls $backupsFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null

Write-Host "`n=== 7. Scheduled Task: $taskPath$taskName (SYSTEM, on-demand only, independent of $taskPath''HMG-IisControl) ==="
$action = New-ScheduledTaskAction -Execute "powershell.exe" `
    -Argument "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$updaterScriptDestination`""
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -ExecutionTimeLimit (New-TimeSpan -Minutes 5)

Register-ScheduledTask -TaskPath $taskPath -TaskName $taskName `
    -Action $action -Principal $principal -Settings $settings -Force `
    -Description "Validates and promotes a new version of Invoke-BeeDayIisControl.ps1 from Staging\ into C:\Ops\BeeDay\IisControl\. Independent of \BeeDay\HMG-IisControl. Action and principal are not modifiable by LAB\svc_beeday_runner." |
    Out-Null

Write-Host "`n=== 8. Restricting who can trigger this ONE task ==="
$sid = (New-Object System.Security.Principal.NTAccount($runnerAccount)).Translate([System.Security.Principal.SecurityIdentifier]).Value

$comTaskPath = $taskPath.TrimEnd('\')
if ([string]::IsNullOrWhiteSpace($comTaskPath)) {
    $comTaskPath = "\"
}

$service = New-Object -ComObject "Schedule.Service"
$service.Connect()
$folder = $service.GetFolder($comTaskPath)
$task = $folder.GetTask($taskName)
$currentSd = $task.GetSecurityDescriptor(0xF)

if ($currentSd -notmatch [regex]::Escape($sid)) {
    $newSd = $currentSd + "(A;;GRGX;;;$sid)"
    $task.SetSecurityDescriptor($newSd, 0)
    Write-Host "Granted Generic Read + Generic Execute on '$taskPath$taskName' to $runnerAccount."
}
else {
    Write-Host "$runnerAccount already has an ACE on '$taskPath$taskName' - left unchanged."
}

Write-Host "`n=== Done. Verify with: ==="
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$updaterRoot'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$installedManifestPath'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$stagingFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$stagingScriptPath'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$stagingManifestPath'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$requestsFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$requestFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$resultsFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$resultFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) icacls '$backupsFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Get-Content '$backupsFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) [System.IO.File]::ReadAllText('$installedManifestPath')"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) `$s = New-Object System.IO.FileStream('$stagingScriptPath',[System.IO.FileMode]::Open,[System.IO.FileAccess]::Write,[System.IO.FileShare]::Read); `$s.Dispose()"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) `$s = New-Object System.IO.FileStream('$stagingManifestPath',[System.IO.FileMode]::Open,[System.IO.FileAccess]::Write,[System.IO.FileShare]::Read); `$s.Dispose()"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) `$s = New-Object System.IO.FileStream('$requestFilePath',[System.IO.FileMode]::Open,[System.IO.FileAccess]::Write,[System.IO.FileShare]::Read); `$s.Dispose()"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) [System.IO.File]::ReadAllText('$resultFilePath')"
Write-Host "  (as LAB\svc_beeday_runner, EXPECTED to fail) Get-Content '$stagingScriptPath'"
Write-Host "  (as LAB\svc_beeday_runner, EXPECTED to fail) Get-Content '$stagingManifestPath'"
Write-Host "  (as LAB\svc_beeday_runner, EXPECTED to fail) Get-Content '$requestFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, EXPECTED to fail) Set-Content '$resultFilePath' -Value 'forged'"
Write-Host "  (as LAB\svc_beeday_runner, EXPECTED to fail) Get-Content '$updaterScriptDestination'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) Get-ScheduledTask -TaskPath '$taskPath' -TaskName '$taskName'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) Start-ScheduledTask -TaskPath '$taskPath' -TaskName '$taskName'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Get-ChildItem '$updaterRoot'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Get-ChildItem '$stagingFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) New-Item '$stagingFolder\other.ps1'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Remove-Item '$stagingScriptPath'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Remove-Item '$requestFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) any access at all into '$operationalInstalledScriptPath' beyond what Provision-BeeDayHmgIisControl.ps1 already granted (unchanged by this script)"
