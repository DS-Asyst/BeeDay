# Regression coverage for Hotfix 26.9.3 (GitHub Actions run 32009214798): CONFIGURE's real
# environment-variable-name allow-list in scripts/iis-control/Invoke-BeeDayIisControl.ps1.
#
# Run 32009214798 proved a contract mismatch between the two sides of the privileged IIS boundary:
# Deploy-BeeDay.ps1 emits one BeeDay__Email__HmgRecipientGuard__AllowedRecipients__N variable per
# configured HMG recipient (Hotfix 26.9.2), but Invoke-BeeDayIisControl.ps1's CONFIGURE validation
# only accepted a fixed list of exact names and rejected every recipient variable outright
# (VALIDATE_VARIABLES/VARIABLE_NOT_ALLOWED) - the deployment failed, and the already
# Environment-Validated rollback correctly restored the previous version.
#
# This suite exercises the REAL allow-list and the REAL validator function, extracted verbatim
# from Invoke-BeeDayIisControl.ps1 by text boundary (never a hand-copied duplicate), so a future
# regression in that file cannot silently pass here. It never touches IIS, SQL, or any file under
# C:\Ops\BeeDay - the rest of that script (which does) is never sourced or executed.
#
# The repository has no PowerShell test framework (no Pester, no PSScriptAnalyzer, no
# *.Tests.ps1 convention), so this stays framework-free and exit-code-driven, matching
# scripts/tests/Test-DeployBeeDayRecovery.ps1.
#
# Run: powershell -File scripts/tests/Test-InvokeBeeDayIisControlContract.ps1
# Exits 0 when every assertion passes, non-zero otherwise.

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$iisControlScriptPath = Join-Path $repoRoot "scripts\iis-control\Invoke-BeeDayIisControl.ps1"

if (-not (Test-Path -LiteralPath $iisControlScriptPath)) {
    throw "Cannot find scripts/iis-control/Invoke-BeeDayIisControl.ps1 at expected path: $iisControlScriptPath"
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
[void][System.Management.Automation.Language.Parser]::ParseFile($iisControlScriptPath, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -gt 0) {
    throw "scripts/iis-control/Invoke-BeeDayIisControl.ps1 has parse errors: $($parseErrors -join '; ')"
}

# Extracted by text boundary - the allow-list array, the recipient-guard regex, and the validator
# function are self-contained top-level statements with no dependency on the rest of the script
# (no Import-Module, no file/IIS access), so a line-boundary slice is sufficient and does not need
# AST function extraction the way a nested function definition would.
$scriptLines = Get-Content -LiteralPath $iisControlScriptPath

# Each Select-String result is captured into its own variable and null-checked BEFORE touching
# .LineNumber - a zero-match Select-String emits nothing, so Select-Object -First 1 on it is $null,
# and .LineNumber on $null throws under Set-StrictMode -Version Latest (the exact class of bug this
# whole hotfix chain has been about) rather than failing with a clear, actionable message.
$startMatch = $scriptLines | Select-String -Pattern '^\$allowedEnvironmentVariableNames = @\($' | Select-Object -First 1
if ($null -eq $startMatch) {
    throw "Could not locate '`$allowedEnvironmentVariableNames = @(' in Invoke-BeeDayIisControl.ps1 - has the contract been restructured, or is Hotfix 26.9.3 not present on this branch? Update this test's extraction boundary."
}
$startLineIndex = $startMatch.LineNumber

$functionMatch = $scriptLines | Select-String -Pattern '^function Test-BeeDayAllowedEnvironmentVariableName \{' | Select-Object -First 1
if ($null -eq $functionMatch) {
    throw "Could not locate 'function Test-BeeDayAllowedEnvironmentVariableName' in Invoke-BeeDayIisControl.ps1 - has it been renamed or removed, or is Hotfix 26.9.3 not present on this branch? Update this test's extraction boundary."
}
$endLineIndex = $functionMatch.LineNumber

# The function's own closing brace is the next '^}' after its declaration.
$closingBraceMatch = $scriptLines[($endLineIndex - 1)..($scriptLines.Count - 1)] | Select-String -Pattern '^\}$' | Select-Object -First 1
if ($null -eq $closingBraceMatch) {
    throw "Could not locate the closing brace of Test-BeeDayAllowedEnvironmentVariableName - update this test's extraction boundary."
}
$endLineIndexInclusive = $endLineIndex + $closingBraceMatch.LineNumber - 1

$contractText = ($scriptLines[($startLineIndex - 1)..($endLineIndexInclusive - 1)]) -join "`n"

. ([scriptblock]::Create($contractText))
Set-StrictMode -Version Latest

if (-not (Get-Command Test-BeeDayAllowedEnvironmentVariableName -ErrorAction SilentlyContinue)) {
    throw "Test-BeeDayAllowedEnvironmentVariableName was not defined by the extracted text - extraction boundary is likely wrong."
}

# ===========================================================================
# All eight existing fixed environment-variable names remain accepted.
# ===========================================================================

$fixedNames = @(
    'ASPNETCORE_ENVIRONMENT',
    'DOTNET_ENVIRONMENT',
    'AllowedHosts',
    'BeeDay__IdentityEmail__PublicBaseUrl',
    'BeeDay__Persistence__SqlServer__ConnectionString',
    'BeeDay__Email__Resend__ApiKey',
    'BeeDay__Email__Resend__FromAddress',
    'BeeDay__Email__Resend__FromName'
)
foreach ($name in $fixedNames) {
    Assert-True (Test-BeeDayAllowedEnvironmentVariableName -Name $name) "Fixed name accepted: $name"
}

# ===========================================================================
# HMG recipient-guard indexed variables - the exact contract this hotfix adds.
# ===========================================================================

Assert-True (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__0") "AllowedRecipients__0 accepted"
Assert-True (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__1") "AllowedRecipients__1 accepted"
Assert-True (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__25") "Higher numeric index (25) accepted"

# ===========================================================================
# Must remain rejected - malformed indexes, wrong section, unrelated names.
# ===========================================================================

Assert-True (-not (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__")) "Empty index rejected"
Assert-True (-not (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__abc")) "Non-numeric index rejected"
Assert-True (-not (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__01")) "Non-canonical (leading-zero) index rejected"
Assert-True (-not (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__-1")) "Negative index rejected"
Assert-True (-not (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__Anything__0")) "Unrelated key under the HmgRecipientGuard section rejected"
Assert-True (-not (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__Resend__UnexpectedSetting")) "Unrelated BeeDay variable outside the fixed list rejected"
Assert-True (-not (Test-BeeDayAllowedEnvironmentVariableName -Name "RANDOM_VAR")) "Arbitrary environment variable rejected"
Assert-True (-not (Test-BeeDayAllowedEnvironmentVariableName -Name "BeeDay__Email__HmgRecipientGuard__AllowedRecipients__0__Extra")) "Trailing characters after a valid index rejected"

# ===========================================================================
Write-Host ""
Write-Host "$($script:testCount - $script:failureCount)/$($script:testCount) assertions passed."

if ($script:failureCount -gt 0) {
    throw "$($script:failureCount) regression assertion(s) failed."
}
