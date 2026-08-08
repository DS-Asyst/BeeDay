# =============================================================================
# BeeDay HMG - Privileged IIS Control: administrative provisioning
# =============================================================================
#
# RUN THIS MANUALLY, AS ADMINISTRATOR, ON SERV3WEB. NEVER FROM CI/CD, NEVER AS
# LAB\svc_beeday_runner. This script is versioned for auditability only - it is
# not wired into any workflow and must not be.
#
# What it does:
#   1. Ensures C:\Ops\BeeDay\IisControl exists with SYSTEM/Administrators Full
#      Control, plus a narrow, non-inherited grant for svc_beeday_runner on
#      the ROOT FOLDER OBJECT ITSELF: (RC,RA,X,S) - Read Control, Read
#      Attributes, Traverse Folder, Synchronize. Windows checks FILE_TRAVERSE
#      on every ancestor directory when opening a file by path; without this,
#      no grant made deeper in the tree (Requests\, Results\) is reachable at
#      all, and icacls itself needs READ_CONTROL to even display an object's
#      ACL when run as that account. This grant is not inheritable, so it has
#      no effect on the script file living in this same folder or on any
#      other content placed here.
#   2. Installs the privileged control script from this repo checkout into
#      that admin-only location. svc_beeday_runner has no ACE on the file
#      itself, so it cannot read, modify, or replace it.
#   3. Creates Requests\ and Results\ with inheritance broken from the parent,
#      granting svc_beeday_runner the same narrow (RC,RA,X,S) on each folder
#      object - enough to traverse into it and have icacls/Get-Acl succeed,
#      not enough to list its contents, create, or delete anything.
#   4. Pre-creates request.txt (sentinel content "NONE") and result.json
#      (sentinel placeholder), then grants svc_beeday_runner a narrow,
#      file-level ACE directly on each: (S,RC,WD,RA) - Write Data only, no
#      Read Data - on request.txt; (S,RC,RD,RA,REA) - Read Data + Read
#      Extended Attributes, no Write Data - on result.json. REA was added
#      after confirming on SERV3WEB that [System.IO.File]::ReadAllText (used
#      by Deploy-BeeDay.ps1 instead of Get-Content, which needs even more
#      than RD/REA for this account) requires it in addition to RD. Both
#      files are meant to live forever and only ever be overwritten in place
#      (see the comments in Invoke-BeeDayIisControl.ps1 for why: a
#      rename-replace would silently drop these file-level grants).
#   5. Registers the \BeeDay\HMG-IisControl Scheduled Task: on-demand only, no
#      recurring trigger, runs as SYSTEM, RunLevel Highest, a single fixed
#      action, MultipleInstances=IgnoreNew so a second trigger while one run
#      is in progress is rejected outright instead of queuing or overlapping.
#   6. Grants LAB\svc_beeday_runner Generic Read + Generic Execute on that one
#      task's own security descriptor - enough to query and trigger it, not
#      enough to modify, delete, or view/change its action.
#
# None of the above ever grants svc_beeday_runner FILE_LIST_DIRECTORY (folder
# enumeration), create/delete/rename rights, WRITE_DAC, or WRITE_OWNER -
# Modify and Full Control are never used for this account anywhere in this
# script.
#
# Re-run safely: steps are idempotent (Register-ScheduledTask -Force,
# icacls /inheritance:r + explicit grants, New-Item -Force leaves existing
# file content untouched, SDDL append is skipped if already present).

#Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

$runnerAccount = "LAB\svc_beeday_runner"
$rootFolder = "C:\Ops\BeeDay\IisControl"
$requestsFolder = Join-Path $rootFolder "Requests"
$resultsFolder = Join-Path $rootFolder "Results"
$requestFilePath = Join-Path $requestsFolder "request.txt"
$resultFilePath = Join-Path $resultsFolder "result.json"
$scriptDestination = Join-Path $rootFolder "Invoke-BeeDayIisControl.ps1"
$scriptSource = Join-Path $PSScriptRoot "Invoke-BeeDayIisControl.ps1"

# Sentinel content: a single line never matches the STOP/START allow-list (which requires exactly 2
# lines), so a task run before any real request was ever issued fails closed instead of doing
# nothing silently or erroring in a confusing way.
$requestSentinel = "NONE"
$resultSentinel = (
    [ordered]@{
        requestId = "00000000-0000-0000-0000-000000000000"
        operation = "NONE"
        exitCode  = 1
        siteState = "Unknown"
        poolState = "Unknown"
        timestamp = "1970-01-01T00:00:00.0000000Z"
    } | ConvertTo-Json -Compress
)

$taskPath = "\BeeDay\"
$taskName = "HMG-IisControl"

# Minimal, non-inherited folder-object grant used at every level of this path: lets the account
# traverse THROUGH the folder to reach a named child, and lets icacls/Get-Acl succeed when run as
# that account (READ_CONTROL) - never list-directory, never create/delete, never change ACLs.
$folderTraverseGrant = "${runnerAccount}:(RC,RA,X,S)"

Write-Host "=== 1. Root folder and script (admin-only, runner gets traverse-through only) ==="
New-Item -ItemType Directory -Path $rootFolder -Force | Out-Null
icacls $rootFolder /inheritance:r | Out-Null
icacls $rootFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $rootFolder /grant $folderTraverseGrant | Out-Null

if (-not (Test-Path -LiteralPath $scriptSource)) {
    throw "Source script not found: $scriptSource. Run this from a checkout of the BeeDay repository."
}
Copy-Item -LiteralPath $scriptSource -Destination $scriptDestination -Force
Write-Host "Installed: $scriptDestination (no ACE for ${runnerAccount} - not readable by it)"

Write-Host "`n=== 2. Requests folder - svc_beeday_runner: traverse-through only (no list/create/delete) ==="
New-Item -ItemType Directory -Path $requestsFolder -Force | Out-Null
icacls $requestsFolder /inheritance:r | Out-Null
icacls $requestsFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $requestsFolder /grant $folderTraverseGrant | Out-Null

Write-Host "`n=== 3. request.txt - pre-created, svc_beeday_runner: Write Data only (no read/delete) ==="
if (-not (Test-Path -LiteralPath $requestFilePath)) {
    Set-Content -LiteralPath $requestFilePath -Value $requestSentinel -Encoding ascii -NoNewline
}
icacls $requestFilePath /grant "${runnerAccount}:(S,RC,WD,RA)" | Out-Null

Write-Host "`n=== 4. Results folder - svc_beeday_runner: traverse-through only (no list/create/delete) ==="
New-Item -ItemType Directory -Path $resultsFolder -Force | Out-Null
icacls $resultsFolder /inheritance:r | Out-Null
icacls $resultsFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $resultsFolder /grant $folderTraverseGrant | Out-Null

Write-Host "`n=== 5. result.json - pre-created, svc_beeday_runner: Read Data only (no write/delete) ==="
if (-not (Test-Path -LiteralPath $resultFilePath)) {
    Set-Content -LiteralPath $resultFilePath -Value $resultSentinel -Encoding utf8 -NoNewline
}
icacls $resultFilePath /grant "${runnerAccount}:(S,RC,RD,RA,REA)" | Out-Null

Write-Host "`n=== 6. Scheduled Task: $taskPath$taskName (SYSTEM, on-demand only) ==="
$action = New-ScheduledTaskAction -Execute "powershell.exe" `
    -Argument "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$scriptDestination`""
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -ExecutionTimeLimit (New-TimeSpan -Minutes 5)

Register-ScheduledTask -TaskPath $taskPath -TaskName $taskName `
    -Action $action -Principal $principal -Settings $settings -Force `
    -Description "Fixed, parameterless STOP/START of site BeeDay-HMG / pool BeeDay-Web-AppPool only. Action and principal are not modifiable by LAB\svc_beeday_runner." |
    Out-Null

Write-Host "`n=== 7. Restricting who can trigger this ONE task ==="
$sid = (New-Object System.Security.Principal.NTAccount($runnerAccount)).Translate([System.Security.Principal.SecurityIdentifier]).Value

# Schedule.Service's COM GetFolder() rejects a trailing backslash (HRESULT 0x8007007B - "The
# filename, directory name, or volume label syntax is incorrect"), unlike the ScheduledTasks
# PowerShell module's -TaskPath, which expects one. $taskPath stays "\BeeDay\" for
# Get-ScheduledTask/Register-ScheduledTask above; only the COM call gets a trimmed copy.
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
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$rootFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$requestsFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$requestFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$resultsFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) icacls '$resultFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) `$s = New-Object System.IO.FileStream('$requestFilePath',[System.IO.FileMode]::Open,[System.IO.FileAccess]::Write,[System.IO.FileShare]::Read); `$s.Dispose()"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) [System.IO.File]::ReadAllText('$resultFilePath')"
Write-Host "  (as LAB\svc_beeday_runner, EXPECTED to fail - Deploy-BeeDay.ps1 does not use these cmdlets for this reason) Set-Content '$requestFilePath' -Value 'STOP'"
Write-Host "  (as LAB\svc_beeday_runner, EXPECTED to fail - Deploy-BeeDay.ps1 does not use these cmdlets for this reason) Get-Content '$resultFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) Get-ScheduledTask -TaskPath '$taskPath' -TaskName '$taskName'"
Write-Host "  (as LAB\svc_beeday_runner, must succeed) Start-ScheduledTask -TaskPath '$taskPath' -TaskName '$taskName'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Get-Content '$requestFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Set-Content '$resultFilePath' -Value 'forged'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Get-Content '$scriptDestination'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Get-ChildItem '$rootFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Get-ChildItem '$requestsFolder'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) New-Item '$requestsFolder\other.txt'"
Write-Host "  (as LAB\svc_beeday_runner, must FAIL) Remove-Item '$requestFilePath'"
