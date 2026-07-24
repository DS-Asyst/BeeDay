$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$dataPath = Join-Path $projectRoot 'src/LevelUp.Web/Data'

if (Test-Path $dataPath) {
    Get-ChildItem -LiteralPath $dataPath -Force | Remove-Item -Recurse -Force
}
else {
    New-Item -ItemType Directory -Path $dataPath | Out-Null
}

Write-Host "LevelUp test data reset: $dataPath"
