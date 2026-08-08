param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PublishPath,

    [Parameter(Mandatory = $true)]
    [ValidatePattern("^https://")]
    [string]$PublicBaseUrl,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ResendApiKey,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ResendFromAddress,

    [string]$ResendFromName = "BeeDay",

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$AllowedHosts
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$siteName = "BeeDay"
$appPoolName = "BeeDayPool"
$destinationPath = "C:\Apps\BeeDay"
$backupRoot = "C:\Apps\BeeDay-Backups"
$externalRoot = "C:\Apps\BeeDay-Data"
$dataPath = Join-Path $externalRoot "Data"
$dataBackupRoot = Join-Path $backupRoot "Data"
$applicationBackupRoot = Join-Path $backupRoot "Application"
$healthCheckUri = "http://127.0.0.1/health/ready"
$healthCheckHost = "beeday"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$applicationBackupPath = Join-Path $applicationBackupRoot "BeeDay-$timestamp"
$dataBackupPath = Join-Path $dataBackupRoot "BeeDay-Data-$timestamp"
$deploymentSucceeded = $false

$externalDirectories = @(
    $dataPath,
    (Join-Path $dataPath "Backups"),
    (Join-Path $externalRoot "DataProtection-Keys"),
    (Join-Path $externalRoot "Emails"),
    (Join-Path $externalRoot "Logs")
)

function Write-DeployMessage {
    param([Parameter(Mandatory = $true)][string]$Message)

    Write-Host ""
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
}

function Stop-BeeDayIis {
    Write-DeployMessage "Stopping IIS site '$siteName'..."
    Stop-Website -Name $siteName -ErrorAction SilentlyContinue

    Write-DeployMessage "Stopping application pool '$appPoolName'..."
    Stop-WebAppPool -Name $appPoolName -ErrorAction SilentlyContinue

    Start-Sleep -Seconds 3
}

function Start-BeeDayIis {
    $appPoolState = Get-WebAppPoolState -Name $appPoolName -ErrorAction SilentlyContinue
    if ($null -eq $appPoolState) {
        throw "Application pool was not found: $appPoolName"
    }

    if ($appPoolState.Value -ne "Started") {
        Write-DeployMessage "Starting application pool '$appPoolName'..."
        Start-WebAppPool -Name $appPoolName
    }

    $siteState = Get-WebsiteState -Name $siteName -ErrorAction SilentlyContinue
    if ($null -eq $siteState) {
        throw "IIS site was not found: $siteName"
    }

    if ($siteState.Value -ne "Started") {
        Write-DeployMessage "Starting IIS site '$siteName'..."
        Start-Website -Name $siteName
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

function Invoke-BeeDayHealthCheck {
    $lastError = $null

    for ($attempt = 1; $attempt -le 6; $attempt++) {
        try {
            Write-DeployMessage "Running readiness health check (attempt $attempt of 6)..."

            $response = Invoke-WebRequest `
                -Uri $healthCheckUri `
                -Headers @{ Host = $healthCheckHost } `
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

function Set-BeeDayEnvironmentVariables {
    Write-DeployMessage "Configuring IIS application-pool environment variables..."

    $variables = @{
        ASPNETCORE_ENVIRONMENT = "Homologation"
        DOTNET_ENVIRONMENT = "Homologation"
        AllowedHosts = $AllowedHosts
        BeeDay__IdentityEmail__PublicBaseUrl = $PublicBaseUrl
        BeeDay__Email__Resend__ApiKey = $ResendApiKey
        BeeDay__Email__Resend__FromAddress = $ResendFromAddress
        BeeDay__Email__Resend__FromName = $ResendFromName
    }

    foreach ($entry in $variables.GetEnumerator()) {
        $filter = "system.applicationHost/applicationPools/add[@name='$appPoolName']/environmentVariables/add[@name='$($entry.Key)']"

        Remove-WebConfigurationProperty `
            -PSPath "MACHINE/WEBROOT/APPHOST" `
            -Filter $filter `
            -Name "." `
            -ErrorAction SilentlyContinue

        Add-WebConfigurationProperty `
            -PSPath "MACHINE/WEBROOT/APPHOST" `
            -Filter "system.applicationHost/applicationPools/add[@name='$appPoolName']/environmentVariables" `
            -Name "." `
            -Value @{ name = $entry.Key; value = $entry.Value }
    }
}

Write-Host "========================================"
Write-Host "BEEDAY - HARDENED IIS DEPLOYMENT"
Write-Host "========================================"

Import-Module WebAdministration

$PublishPath = (Resolve-Path -LiteralPath $PublishPath -ErrorAction Stop).Path

$requiredFiles = @("BeeDay.Web.dll", "web.config")
foreach ($requiredFile in $requiredFiles) {
    $requiredFilePath = Join-Path $PublishPath $requiredFile
    if (-not (Test-Path -LiteralPath $requiredFilePath -PathType Leaf)) {
        throw "Required published file was not found: $requiredFilePath"
    }
}

@($destinationPath, $backupRoot, $applicationBackupRoot, $dataBackupRoot) + $externalDirectories |
    ForEach-Object { New-Item -ItemType Directory -Path $_ -Force | Out-Null }

$appPoolIdentity = "IIS AppPool\$appPoolName"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $appPoolIdentity,
    "Modify",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)

foreach ($directory in $externalDirectories) {
    $acl = Get-Acl -LiteralPath $directory
    $acl.SetAccessRule($accessRule)
    Set-Acl -LiteralPath $directory -AclObject $acl
}

Write-DeployMessage "Backing up current application to '$applicationBackupPath'..."
Copy-DirectoryContents -Source $destinationPath -Destination $applicationBackupPath

Write-DeployMessage "Backing up persistent data to '$dataBackupPath'..."
Copy-DirectoryContents -Source $dataPath -Destination $dataBackupPath

try {
    Stop-BeeDayIis
    Set-BeeDayEnvironmentVariables

    Write-DeployMessage "Replacing application files while preserving external data..."
    Clear-DirectoryContents -Path $destinationPath
    Copy-DirectoryContents -Source $PublishPath -Destination $destinationPath

    Start-BeeDayIis
    Invoke-BeeDayHealthCheck

    $deploymentSucceeded = $true
    Write-DeployMessage "Deployment completed successfully."
    Write-Host "Application backup: $applicationBackupPath"
    Write-Host "Data backup: $dataBackupPath"
}
catch {
    $deploymentError = $_.Exception.Message
    Write-Error "Deployment failed: $deploymentError"

    Write-DeployMessage "Starting rollback to the previous application version..."

    try {
        Stop-BeeDayIis
        Clear-DirectoryContents -Path $destinationPath
        Copy-DirectoryContents -Source $applicationBackupPath -Destination $destinationPath
        Start-BeeDayIis
        Invoke-BeeDayHealthCheck
        Write-DeployMessage "Rollback completed and previous version is healthy."
    }
    catch {
        Write-Error "Rollback also failed: $($_.Exception.Message)"
    }

    throw "Deployment failed and rollback was attempted. Original error: $deploymentError"
}
finally {
    if (-not $deploymentSucceeded) {
        Write-Host "Persistent data was not replaced. Backup available at: $dataBackupPath"
    }
}
