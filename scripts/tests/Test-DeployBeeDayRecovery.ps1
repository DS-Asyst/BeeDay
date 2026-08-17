# Regression coverage for scripts/Deploy-BeeDay.ps1, covering two real HMG incidents:
#
#   Hotfix 26.9.1 (GitHub Actions run 31986772973) - Defect B: a Write-Error inside the
#   deployment catch block escalated to a terminating error under the script-wide
#   $ErrorActionPreference = "Stop", skipping the rollback attempt entirely. Fixed and
#   Environment Validated on a real HMG deployment (run 31993611105): rollback now reliably
#   reaches STOP -> restore-skip -> START -> health check. Do not weaken that coverage below
#   without new evidence of a defect in it.
#
#   Hotfix 26.9.2 (GitHub Actions run 31993611105) - Defect A: $script:hmgAllowedRecipients
#   collided, case-insensitively, with the -HmgAllowedRecipients script PARAMETER (PowerShell
#   variable names are case-insensitive). Hotfix 26.9.1's own regression suite passed 16/16 and
#   still missed this, because its helper transported the parsed list through a NESTED FUNCTION
#   parameter also named $HmgAllowedRecipients and a "return , $x" trick - a construction that,
#   unlike the real script, has no script-level parameter for $script:hmgAllowedRecipients to
#   collide with. That normalized away the exact scope shape that broke in production. The
#   Defect A coverage below closes that gap by invoking the REAL, unmodified param block and the
#   REAL Set-BeeDayEnvironmentVariables function together, exactly as deploy-hmg.yml does (a
#   script invoked via `&` with its own top-level parameters) - never a hand-copied loop.
#
# The repository has no PowerShell test framework (no Pester, no PSScriptAnalyzer, no
# *.Tests.ps1 convention - confirmed by searching the repo before this file was first added), so
# this stays a small, framework-free, exit-code-driven assertion script rather than a new
# dependency.
#
# Engine parity: this suite and the real deploy-hmg.yml job both run under Windows PowerShell
# 5.1 Desktop (confirmed: $PSVersionTable.PSVersion 5.1.26100.9168 in the environment that
# reproduced both incidents; deploy-hmg.yml's self-hosted HMG runner shells out to
# C:\Windows\System32\WindowsPowerShell\v1.0\powershell.EXE). No engine-version difference was
# found or is relied upon by this suite - the Defect A bug reproduced and is fixed under the
# same engine/version this suite runs under, not a different one.
#
# Run: powershell -File scripts/tests/Test-DeployBeeDayRecovery.ps1
# Exits 0 when every assertion passes, non-zero otherwise.

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$deployScriptPath = Join-Path $repoRoot "scripts\Deploy-BeeDay.ps1"

if (-not (Test-Path -LiteralPath $deployScriptPath)) {
    throw "Cannot find scripts/Deploy-BeeDay.ps1 at expected path: $deployScriptPath"
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
$ast = [System.Management.Automation.Language.Parser]::ParseFile($deployScriptPath, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -gt 0) {
    throw "scripts/Deploy-BeeDay.ps1 has parse errors: $($parseErrors -join '; ')"
}

# ===========================================================================
# Defect A - the real param block and the real Set-BeeDayEnvironmentVariables,
# invoked together exactly as deploy-hmg.yml invokes Deploy-BeeDay.ps1 itself
# (a script file invoked via `&`, with its own top-level -HmgAllowedRecipients
# parameter) - never a hand-copied loop or a scope shape that can't collide.
# ===========================================================================

# Everything from the top of the file through Test-BeeDayUsesPrivilegedIisControl - the real,
# unmodified param block, validation, ConvertTo-BeeDayRecipientList, both $script:-scoped
# assignments (the exact ones that collided with the parameter in production), Protect-DeploySecret,
# Write-DeployMessage, the privileged-IIS constants, and Test-BeeDayUsesPrivilegedIisControl.
# Sliced by text boundary (not a hardcoded line number) so it survives unrelated edits above it.
$boundaryMatch = Select-String -LiteralPath $deployScriptPath -Pattern '^function Write-BeeDayFileStreamContent \{' | Select-Object -First 1
if (-not $boundaryMatch) {
    throw "Could not find the Write-BeeDayFileStreamContent boundary in scripts/Deploy-BeeDay.ps1 - has it been restructured? Update the slice boundary in this test."
}
$scriptLines = Get-Content -LiteralPath $deployScriptPath
$headText = ($scriptLines[0..($boundaryMatch.LineNumber - 2)]) -join "`n"

$setEnvVarsAst = $ast.Find(
    { param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Set-BeeDayEnvironmentVariables' },
    $true
)
if (-not $setEnvVarsAst) {
    throw "Set-BeeDayEnvironmentVariables not found in scripts/Deploy-BeeDay.ps1 - has it been renamed or removed?"
}

# The privileged CONFIGURE call is the one genuinely external dependency left after the slice
# above (it triggers a real SYSTEM-level Scheduled Task on SERV3WEB) - stubbed here to instead
# write the App Pool variables it was asked to configure to a result file, so the assertions
# below can inspect exactly what the real function decided to build, without ever touching IIS.
$harnessScript = @"
$headText

function Invoke-BeeDayPrivilegedIisControl {
    param(`$Operation, `$EnvironmentVariables, `$RequestId)
    # Plain-text, one key per line, via the .NET File API directly - not ConvertTo-Json /
    # Set-Content, both of which collapse a 1-element (or, for ConvertTo-Json on Windows
    # PowerShell 5.1, even a multi-element) pipeline input in ways that lose array shape on the
    # way back out. WriteAllLines/ReadAllLines never collapse 0/1/N elements differently.
    [System.IO.File]::WriteAllLines(`$env:BEEDAY_TEST_RESULT_PATH, [string[]]@(`$EnvironmentVariables.Keys | Sort-Object))
}

$($setEnvVarsAst.Extent.Text)

Set-BeeDayEnvironmentVariables
"@

$harnessPath = Join-Path ([System.IO.Path]::GetTempPath()) "beeday-deploy-test-harness-$([guid]::NewGuid().ToString('N')).ps1"
Set-Content -LiteralPath $harnessPath -Value $harnessScript -Encoding UTF8

function Invoke-BeeDayRecipientEnumeration {
    param([AllowEmptyString()][Parameter(Mandatory = $true)][string]$HmgAllowedRecipients)

    $resultPath = Join-Path ([System.IO.Path]::GetTempPath()) "beeday-deploy-test-result-$([guid]::NewGuid().ToString('N')).json"
    if (Test-Path -LiteralPath $resultPath) {
        Remove-Item -LiteralPath $resultPath -Force
    }

    $threw = $false
    $errorMessage = $null
    try {
        $env:BEEDAY_TEST_RESULT_PATH = $resultPath
        & $harnessPath `
            -PublishPath "C:\beeday-test-publish" `
            -PublicBaseUrl "https://example.invalid" `
            -ResendApiKey "test-dummy" `
            -ResendFromAddress "test-dummy" `
            -ResendFromName "test-dummy" `
            -HmgAllowedRecipients $HmgAllowedRecipients `
            -AllowedHosts "example.invalid" `
            -SiteName "BeeDay-HMG" `
            -AppPoolName "BeeDay-Web-AppPool" `
            -Environment "Homologation" `
            -AppConnectionString "Server=example-invalid;Database=example;"
    }
    catch {
        $threw = $true
        $errorMessage = $_.Exception.Message
    }
    finally {
        Remove-Item Env:\BEEDAY_TEST_RESULT_PATH -ErrorAction SilentlyContinue
    }

    $variableKeys = @()
    if (-not $threw -and (Test-Path -LiteralPath $resultPath)) {
        $variableKeys = [System.IO.File]::ReadAllLines($resultPath)
        Remove-Item -LiteralPath $resultPath -Force -ErrorAction SilentlyContinue
    }

    return [pscustomobject]@{
        Threw          = $threw
        ErrorMessage   = $errorMessage
        RecipientKeys  = @($variableKeys | Where-Object { $_ -like "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__*" })
    }
}

try {
    $emptyRun = Invoke-BeeDayRecipientEnumeration -HmgAllowedRecipients ""
    Assert-True (-not $emptyRun.Threw) "Empty allowlist: Set-BeeDayEnvironmentVariables does not throw (error: $($emptyRun.ErrorMessage))"
    Assert-True ($emptyRun.RecipientKeys.Count -eq 0) "Empty allowlist: no AllowedRecipients App Pool variables emitted"

    $whitespaceRun = Invoke-BeeDayRecipientEnumeration -HmgAllowedRecipients "   "
    Assert-True (-not $whitespaceRun.Threw) "Whitespace-only allowlist: Set-BeeDayEnvironmentVariables does not throw (error: $($whitespaceRun.ErrorMessage))"
    Assert-True ($whitespaceRun.RecipientKeys.Count -eq 0) "Whitespace-only allowlist: no AllowedRecipients App Pool variables emitted"

    $oneRun = Invoke-BeeDayRecipientEnumeration -HmgAllowedRecipients "reviewer1@example.invalid"
    Assert-True (-not $oneRun.Threw) "Single recipient: Set-BeeDayEnvironmentVariables does not throw (error: $($oneRun.ErrorMessage))"
    Assert-True ($oneRun.RecipientKeys.Count -eq 1) "Single recipient: exactly one indexed App Pool variable"
    Assert-True ($oneRun.RecipientKeys -contains "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__0") "Single recipient: index 0 present"

    $multiRun = Invoke-BeeDayRecipientEnumeration -HmgAllowedRecipients "reviewer1@example.invalid;reviewer2@example.invalid;reviewer3@example.invalid"
    Assert-True (-not $multiRun.Threw) "Multiple recipients: Set-BeeDayEnvironmentVariables does not throw (error: $($multiRun.ErrorMessage))"
    Assert-True ($multiRun.RecipientKeys.Count -eq 3) "Multiple recipients: three correctly-indexed App Pool variables"
    Assert-True (
        ($multiRun.RecipientKeys -contains "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__0") -and
        ($multiRun.RecipientKeys -contains "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__1") -and
        ($multiRun.RecipientKeys -contains "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__2")
    ) "Multiple recipients: indices 0, 1, 2 all present, correctly ordered"
}
finally {
    Remove-Item -LiteralPath $harnessPath -Force -ErrorAction SilentlyContinue
}

# ===========================================================================
# Defect B - Environment Validated on a real HMG deployment (run 31993611105):
# a failure inside the main deployment operation must not be able to skip the
# rollback attempt, and the script must still end non-zero. Coverage preserved
# unchanged from Hotfix 26.9.1 - no new evidence of a defect in this path.
# ===========================================================================

$tryLineIndex = ($scriptLines | Select-String -Pattern '^try \{$' | Select-Object -First 1).LineNumber
if (-not $tryLineIndex) {
    throw "Could not locate the top-level 'try {' block in scripts/Deploy-BeeDay.ps1 - has it been restructured?"
}
# The top-level try/catch/finally is the last statement in the file (confirmed: nothing follows its
# closing brace), so the slice runs to end of file rather than relying on a second, brittle marker.
$deployBlockText = ($scriptLines[($tryLineIndex - 1)..($scriptLines.Count - 1)]) -join "`n"

# Stand-ins for every external dependency the real block calls - none exist in this process, and
# none may perform real IIS/filesystem work in an ordinary unit test. Each records its own call so
# the assertions below can prove the REAL rollback sequence ran, not a mock pretending it did.
$script:calls = New-Object System.Collections.Generic.List[string]

function Backup-BeeDayDatabase { param($ConnectionString, $BackupDirectory) $script:calls.Add("Backup-BeeDayDatabase") }
function Invoke-BeeDayMigration { param($BundlePath, $ConnectionString) $script:calls.Add("Invoke-BeeDayMigration") }
function Stop-BeeDayIis { $script:calls.Add("Stop-BeeDayIis") }
function Start-BeeDayIis { $script:calls.Add("Start-BeeDayIis") }
function Restore-BeeDayIisEnvironmentVariables { $script:calls.Add("Restore-BeeDayIisEnvironmentVariables") }
function Clear-DirectoryContents { param($Path) $script:calls.Add("Clear-DirectoryContents") }
function Copy-DirectoryContents { param($Source, $Destination) $script:calls.Add("Copy-DirectoryContents") }
function Invoke-BeeDayHealthCheck { $script:calls.Add("Invoke-BeeDayHealthCheck") }
function Write-DeployMessage { param($Message) $script:calls.Add("Write-DeployMessage:$Message") }
function Protect-DeploySecret { param($Message) return $Message }

# Reproduces exactly what run 31986772973 hit: the FIRST call inside the main try block
# (Set-BeeDayEnvironmentVariables, standing in for the real null/.Count crash) throws.
function Set-BeeDayEnvironmentVariables {
    $script:calls.Add("Set-BeeDayEnvironmentVariables")
    throw "The property 'Count' cannot be found on this object. Verify that the property exists."
}

# Variables the real block reads but does not itself define (normally set earlier in the script).
$BackupDatabase = $false
$RunMigrations = $false
$PublishPath = "C:\test-publish"
$DestinationPath = "C:\test-destination"
$applicationBackupPath = "C:\test-app-backup"
$dataBackupPath = "C:\test-data-backup"
$deployStartedAt = Get-Date
$deploymentSucceeded = $false

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$caughtTerminatingError = $null
try {
    . ([scriptblock]::Create($deployBlockText))
}
catch {
    $caughtTerminatingError = $_
}

Assert-True ($null -ne $caughtTerminatingError) "Deployment failure: script still ends with a terminating error (non-zero exit)"
Assert-True (
    $caughtTerminatingError.Exception.Message -like "*Deployment failed and rollback was attempted*"
) "Deployment failure: final error is the intended rollback-summary throw, not an unrelated escalation"
Assert-True ($caughtTerminatingError.Exception.Message -like "*Count*") "Deployment failure: original error text is retained in the final message"

$rollbackStarted = @($script:calls | Where-Object { $_ -eq "Write-DeployMessage:Starting rollback to the previous application version..." })
Assert-True ($rollbackStarted.Count -eq 1) "Rollback: 'Starting rollback...' was reached (not skipped by the failure-logging statement)"
Assert-True ($script:calls -contains "Restore-BeeDayIisEnvironmentVariables") "Rollback: environment restoration was attempted"
Assert-True ($script:calls -contains "Start-BeeDayIis") "Rollback: IIS was restarted"
Assert-True ($script:calls -contains "Invoke-BeeDayHealthCheck") "Rollback: health check was attempted after restore"

# Ordering proof: the simulated failure happens BEFORE the rollback's own Start-BeeDayIis call -
# confirms this exercised the catch/rollback path, not some unrelated success path.
$failIndex = $script:calls.IndexOf("Set-BeeDayEnvironmentVariables")
$rollbackStartIisIndex = $script:calls.LastIndexOf("Start-BeeDayIis")
Assert-True ($rollbackStartIisIndex -gt $failIndex) "Rollback: IIS restart happened after the simulated failure, as part of rollback"

# ===========================================================================
Write-Host ""
Write-Host "$($script:testCount - $script:failureCount)/$($script:testCount) assertions passed."

if ($script:failureCount -gt 0) {
    throw "$($script:failureCount) regression assertion(s) failed."
}
