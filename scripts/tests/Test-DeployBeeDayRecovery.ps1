# Hotfix 26.9.1 regression coverage for scripts/Deploy-BeeDay.ps1 (GitHub Actions run 31986772973).
#
# The repository has no PowerShell test framework (no Pester, no PSScriptAnalyzer, no *.Tests.ps1
# convention - confirmed by searching the repo before this file was added), so this is a small,
# framework-free assertion script rather than a new dependency. Both defects below are exercised
# against the REAL text of scripts/Deploy-BeeDay.ps1 - extracted via the PowerShell AST (function)
# and a literal line-boundary slice (the main try/catch/finally) - never a hand-copied duplicate of
# the logic, so a future regression in the real file cannot silently pass here.
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
# Defect A - an empty/whitespace HmgAllowedRecipients must never crash under
# Set-StrictMode -Version Latest, and must always produce a real collection,
# for both the parsing function and the exact call-site pattern that uses it.
# ===========================================================================

$functionAst = $ast.Find(
    { param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'ConvertTo-BeeDayRecipientList' },
    $true
)
if (-not $functionAst) {
    throw "ConvertTo-BeeDayRecipientList not found in scripts/Deploy-BeeDay.ps1 - has hotfix 26.9.1 been reverted?"
}

$assignmentLine = (Select-String -LiteralPath $deployScriptPath -Pattern '^\$script:hmgAllowedRecipients = ' | Select-Object -First 1)
if (-not $assignmentLine) {
    throw "Could not find the `$script:hmgAllowedRecipients assignment line in scripts/Deploy-BeeDay.ps1 - has it been restructured?"
}

# Defines the REAL function and executes the REAL call-site assignment (both extracted verbatim
# from the production script) in this scope, under the same Set-StrictMode used in production,
# without executing the rest of Deploy-BeeDay.ps1 (which requires mandatory deployment parameters
# and performs real IIS/filesystem actions).
. ([scriptblock]::Create($functionAst.Extent.Text))
Set-StrictMode -Version Latest

function Invoke-BeeDayRecipientAssignment {
    param([AllowEmptyString()][Parameter(Mandatory = $true)][string]$HmgAllowedRecipients)

    . ([scriptblock]::Create($assignmentLine.Line))
    # A bare "return $script:hmgAllowedRecipients" would re-collapse a zero-element array to $null
    # across this helper's own return boundary - the same trap this test exists to catch - so the
    # call site below must independently wrap this call in @(), exactly as production does.
    return , $script:hmgAllowedRecipients
}

$emptyResult = Invoke-BeeDayRecipientAssignment -HmgAllowedRecipients ""
Assert-True ($null -ne $emptyResult) "Empty allowlist: `$script:hmgAllowedRecipients is not `$null"
Assert-True ($emptyResult.Count -eq 0) "Empty allowlist: `$script:hmgAllowedRecipients has zero elements (no throw on .Count)"

$whitespaceResult = Invoke-BeeDayRecipientAssignment -HmgAllowedRecipients "   "
Assert-True ($null -ne $whitespaceResult -and $whitespaceResult.Count -eq 0) "Whitespace-only allowlist: zero elements, not `$null"

$oneResult = Invoke-BeeDayRecipientAssignment -HmgAllowedRecipients "reviewer1@example.invalid"
Assert-True ($oneResult.Count -eq 1) "Single recipient: exactly one element"
Assert-True ($oneResult[0] -eq "reviewer1@example.invalid") "Single recipient: value preserved"

$multiResult = Invoke-BeeDayRecipientAssignment -HmgAllowedRecipients " reviewer1@example.invalid ; reviewer2@example.invalid ;;  "
Assert-True ($multiResult.Count -eq 2) "Multiple recipients: blank/whitespace entries dropped"
Assert-True (
    $multiResult[0] -eq "reviewer1@example.invalid" -and $multiResult[1] -eq "reviewer2@example.invalid"
) "Multiple recipients: order and trimming preserved"

# The exact App Pool env-var loop from Set-BeeDayEnvironmentVariables - proves an empty allowlist
# emits no BeeDay__Email__HmgRecipientGuard__AllowedRecipients__N variables.
$variables = @{}
for ($i = 0; $i -lt $emptyResult.Count; $i++) {
    $variables["BeeDay__Email__HmgRecipientGuard__AllowedRecipients__$i"] = $emptyResult[$i]
}
Assert-True ($variables.Count -eq 0) "Empty allowlist: no AllowedRecipients App Pool variables emitted"

# ===========================================================================
# Defect B - a failure inside the main deployment operation must not be able
# to skip the rollback attempt, and the script must still end non-zero.
# ===========================================================================

$scriptLines = Get-Content -LiteralPath $deployScriptPath
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
