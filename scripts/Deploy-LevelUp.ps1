param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PublishPath
)

$ErrorActionPreference = "Stop"

$siteName = "LevelUp"
$appPoolName = "LevelUpPool"

$destinationPath = "C:\Apps\LevelUp"
$backupRoot = "C:\Apps\LevelUp-Backups"
$externalRoot = "C:\Apps\LevelUp-Data"
$externalDirectories = @(
    (Join-Path $externalRoot "Data"),
    (Join-Path $externalRoot "DataProtection-Keys"),
    (Join-Path $externalRoot "Emails"),
    (Join-Path $externalRoot "Logs")
)

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupPath = Join-Path $backupRoot "LevelUp-$timestamp"

function Write-DeployMessage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    Write-Host ""
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
}

function Start-LevelUpIis {
    Write-DeployMessage "Iniciando Application Pool '$appPoolName'..."

    $appPoolState = Get-WebAppPoolState `
        -Name $appPoolName `
        -ErrorAction SilentlyContinue

    if ($null -eq $appPoolState) {
        throw "Application Pool nao encontrado: $appPoolName"
    }

    if ($appPoolState.Value -ne "Started") {
        Start-WebAppPool -Name $appPoolName
    }

    Write-DeployMessage "Iniciando site '$siteName'..."

    $siteState = Get-WebsiteState `
        -Name $siteName `
        -ErrorAction SilentlyContinue

    if ($null -eq $siteState) {
        throw "Site IIS nao encontrado: $siteName"
    }

    if ($siteState.Value -ne "Started") {
        Start-Website -Name $siteName
    }
}

Write-Host "========================================"
Write-Host "LEVELUP - DEPLOY PARA IIS"
Write-Host "========================================"

Import-Module WebAdministration

$resolvedPublishPath = Resolve-Path `
    -LiteralPath $PublishPath `
    -ErrorAction Stop

$PublishPath = $resolvedPublishPath.Path

if (-not (Test-Path -LiteralPath $PublishPath -PathType Container)) {
    throw "Diretorio de publicacao nao encontrado: $PublishPath"
}

$requiredFiles = @(
    "LevelUp.Web.dll",
    "web.config"
)

foreach ($requiredFile in $requiredFiles) {
    $requiredFilePath = Join-Path $PublishPath $requiredFile

    if (-not (Test-Path -LiteralPath $requiredFilePath -PathType Leaf)) {
        throw "Arquivo obrigatorio nao encontrado: $requiredFilePath"
    }
}

New-Item `
    -ItemType Directory `
    -Path $destinationPath `
    -Force |
    Out-Null

New-Item `
    -ItemType Directory `
    -Path $backupRoot `
    -Force |
    Out-Null

foreach ($directory in $externalDirectories) {
    New-Item `
        -ItemType Directory `
        -Path $directory `
        -Force |
        Out-Null
}

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

Write-DeployMessage "Criando backup em '$backupPath'..."

New-Item `
    -ItemType Directory `
    -Path $backupPath `
    -Force |
    Out-Null

$existingItems = Get-ChildItem `
    -LiteralPath $destinationPath `
    -Force `
    -ErrorAction SilentlyContinue

foreach ($item in $existingItems) {
    Copy-Item `
        -LiteralPath $item.FullName `
        -Destination $backupPath `
        -Recurse `
        -Force
}

try {
    Write-DeployMessage "Parando site '$siteName'..."

    Stop-Website `
        -Name $siteName `
        -ErrorAction SilentlyContinue

    Write-DeployMessage "Parando Application Pool '$appPoolName'..."

    Stop-WebAppPool `
        -Name $appPoolName `
        -ErrorAction SilentlyContinue

    Start-Sleep -Seconds 3

    Write-DeployMessage "Removendo arquivos da versao anterior..."

    Get-ChildItem `
        -LiteralPath $destinationPath `
        -Force |
        Remove-Item `
            -Recurse `
            -Force

    Write-DeployMessage "Copiando nova versao..."

    Get-ChildItem `
        -LiteralPath $PublishPath `
        -Force |
        ForEach-Object {
            Copy-Item `
                -LiteralPath $_.FullName `
                -Destination $destinationPath `
                -Recurse `
                -Force
        }

    Start-LevelUpIis

    Start-Sleep -Seconds 8

    Write-DeployMessage "Executando health check..."

    $response = Invoke-WebRequest `
        -Uri "http://127.0.0.1/health/ready" `
        -Headers @{
            Host = "levelup"
        } `
        -UseBasicParsing `
        -TimeoutSec 30

    if ($response.StatusCode -ne 200) {
        throw "Health check falhou. HTTP $($response.StatusCode)."
    }

    Write-DeployMessage "Deploy concluido com sucesso."
    Write-Host "HTTP Status: $($response.StatusCode)"
    Write-Host "Backup: $backupPath"
}
catch {
    Write-Host ""
    Write-Error "Falha durante o deploy: $($_.Exception.Message)"

    try {
        Start-LevelUpIis
    }
    catch {
        Write-Error "Tambem nao foi possivel reiniciar o IIS: $($_.Exception.Message)"
    }

    throw
}
