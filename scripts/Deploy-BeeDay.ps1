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
    # equivalent) does, via a connection string that is never the app's own.
    [switch]$RunMigrations,
    [string]$MigrationConnectionString,

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

$repoRoot = Split-Path -Parent $PSScriptRoot
$migrationsProjectPath = Join-Path $repoRoot "src\BeeDay.Infrastructure"
$toolManifestPath = Join-Path $repoRoot "dotnet-tools.json"

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

function Stop-BeeDayIis {
    Write-DeployMessage "Stopping IIS site '$SiteName'..."
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue

    Write-DeployMessage "Stopping application pool '$AppPoolName'..."
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue

    Start-Sleep -Seconds 3
}

function Start-BeeDayIis {
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

# Applies pending EF Core migrations using $ConnectionString exclusively — never the application's
# own connection string. The design-time factory (BeeDayDbContextFactory) still builds the model as
# usual; --connection overrides which database the migration actually runs against, so no C# code
# needs to know about the migrator credential.
#
# Never assumes dotnet-ef is installed globally on the runner: `dotnet tool restore
# --tool-manifest` resolves it as a LOCAL tool from this repo's dotnet-tools.json, which pins the
# exact EF Core CLI version — the version the repo declares is the version that runs, regardless
# of whatever else may or may not be installed machine-wide. Running `dotnet ef` from $repoRoot
# afterwards (via Push-Location) lets the dotnet muxer resolve it as that restored local tool.
function Invoke-BeeDayMigration {
    param([Parameter(Mandatory = $true)][string]$ConnectionString)

    Push-Location $repoRoot
    try {
        Write-DeployMessage "Restoring EF Core tool manifest (local tool, pinned version)..."
        dotnet tool restore --tool-manifest $toolManifestPath
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet tool restore failed with exit code $LASTEXITCODE."
        }

        Write-DeployMessage "Applying EF Core migrations via the migrator connection..."
        dotnet ef database update `
            --project $migrationsProjectPath `
            --startup-project $migrationsProjectPath `
            --connection $ConnectionString
        if ($LASTEXITCODE -ne 0) {
            throw "EF Core migration failed with exit code $LASTEXITCODE."
        }

        Write-DeployMessage "Migrations applied successfully."
    }
    finally {
        Pop-Location
    }
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

$appPoolIdentity = "IIS AppPool\$AppPoolName"
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
Copy-DirectoryContents -Source $DestinationPath -Destination $applicationBackupPath

Write-DeployMessage "Backing up persistent data to '$dataBackupPath'..."
Copy-DirectoryContents -Source $dataPath -Destination $dataBackupPath

try {
    if ($BackupDatabase) {
        Backup-BeeDayDatabase -ConnectionString $MigrationConnectionString -BackupDirectory $DatabaseBackupDirectory
    }

    if ($RunMigrations) {
        Invoke-BeeDayMigration -ConnectionString $MigrationConnectionString
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
