# =============================================================================
# BeeDay HMG - Privileged IIS Control: administrative provisioning
# =============================================================================
#
# RUN THIS MANUALLY, AS ADMINISTRATOR, ON SERV3WEB. NEVER FROM CI/CD, NEVER AS
# LAB\svc_beeday_runner. This script is versioned for auditability only - it is
# not wired into any workflow and must not be.
#
# What it does:
#   1. Ensures C:\Ops\BeeDay\IisControl exists with SYSTEM/Administrators-only
#      access (matches the ACL already confirmed in place).
#   2. Installs the privileged control script from this repo checkout into
#      that admin-only location.
#   3. Creates Requests\ and Results\ with inheritance broken from the parent,
#      granting svc_beeday_runner ONLY (X) Traverse Folder on each - enough to
#      reach a named file, not enough to list, create, or delete anything.
#   4. Pre-creates request.txt (sentinel content "NONE") and result.json
#      (sentinel placeholder), then grants svc_beeday_runner a narrow,
#      file-level ACE directly on each: (S,WD,RA) - Write Data only - on
#      request.txt, (S,RD,RA) - Read Data only - on result.json. Both files
#      are meant to live forever and only ever be overwritten in place (see
#      the comments in Invoke-BeeDayIisControl.ps1 for why: a rename-replace
#      would silently drop these file-level grants).
#   5. Registers the \BeeDay\HMG-IisControl Scheduled Task: on-demand only, no
#      recurring trigger, runs as SYSTEM, RunLevel Highest, a single fixed
#      action, MultipleInstances=IgnoreNew so a second trigger while one run
#      is in progress is rejected outright instead of queuing or overlapping.
#   6. Grants LAB\svc_beeday_runner Generic Read + Generic Execute on that one
#      task's own security descriptor - enough to query and trigger it, not
#      enough to modify, delete, or view/change its action.
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

Write-Host "=== 1. Root folder and script (admin-only) ==="
New-Item -ItemType Directory -Path $rootFolder -Force | Out-Null
icacls $rootFolder /inheritance:r | Out-Null
icacls $rootFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null

if (-not (Test-Path -LiteralPath $scriptSource)) {
    throw "Source script not found: $scriptSource. Run this from a checkout of the BeeDay repository."
}
Copy-Item -LiteralPath $scriptSource -Destination $scriptDestination -Force
Write-Host "Installed: $scriptDestination"

Write-Host "`n=== 2. Requests folder - svc_beeday_runner: Traverse only (no list/create/delete) ==="
New-Item -ItemType Directory -Path $requestsFolder -Force | Out-Null
icacls $requestsFolder /inheritance:r | Out-Null
icacls $requestsFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $requestsFolder /grant "${runnerAccount}:(X)" | Out-Null

Write-Host "`n=== 3. request.txt - pre-created, svc_beeday_runner: Write Data only (no read/delete) ==="
if (-not (Test-Path -LiteralPath $requestFilePath)) {
    Set-Content -LiteralPath $requestFilePath -Value $requestSentinel -Encoding ascii -NoNewline
}
icacls $requestFilePath /grant "${runnerAccount}:(S,WD,RA)" | Out-Null

Write-Host "`n=== 4. Results folder - svc_beeday_runner: Traverse only (no list/create/delete) ==="
New-Item -ItemType Directory -Path $resultsFolder -Force | Out-Null
icacls $resultsFolder /inheritance:r | Out-Null
icacls $resultsFolder /grant:r "SYSTEM:(OI)(CI)F" "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
icacls $resultsFolder /grant "${runnerAccount}:(X)" | Out-Null

Write-Host "`n=== 5. result.json - pre-created, svc_beeday_runner: Read Data only (no write/delete) ==="
if (-not (Test-Path -LiteralPath $resultFilePath)) {
    Set-Content -LiteralPath $resultFilePath -Value $resultSentinel -Encoding utf8 -NoNewline
}
icacls $resultFilePath /grant "${runnerAccount}:(S,RD,RA)" | Out-Null

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

$service = New-Object -ComObject "Schedule.Service"
$service.Connect()
$folder = $service.GetFolder($taskPath)
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
Write-Host "  icacls '$requestsFolder'"
Write-Host "  icacls '$requestFilePath'"
Write-Host "  icacls '$resultsFolder'"
Write-Host "  icacls '$resultFilePath'"
Write-Host "  Get-ScheduledTask -TaskPath '$taskPath' -TaskName '$taskName'"
Write-Host "  (as LAB\svc_beeday_runner) Start-ScheduledTask -TaskPath '$taskPath' -TaskName '$taskName'"
Write-Host "  (as LAB\svc_beeday_runner, must fail) Get-Content '$requestFilePath'"
Write-Host "  (as LAB\svc_beeday_runner, must fail) Set-Content '$resultFilePath' -Value 'forged'"
Write-Host "  (as LAB\svc_beeday_runner, must fail) Get-Content '$scriptDestination'"
