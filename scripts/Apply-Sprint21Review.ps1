$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$obsoletePaths = @(
    "src/LevelUp.Application/Features/Profiles",
    "src/LevelUp.Domain/Entities/Profile.cs",
    "src/LevelUp.Domain/ValueObjects/ProfileName.cs",
    "src/LevelUp.Domain/ValueObjects/ProfileNickname.cs",
    "src/LevelUp.Web/Components/Features/Profile"
)

foreach ($relativePath in $obsoletePaths) {
    $path = Join-Path $root $relativePath
    if (Test-Path $path) {
        Remove-Item $path -Recurse -Force
        Write-Host "Removed obsolete path: $relativePath"
    }
}

Write-Host "Sprint 2.1 legacy Profile cleanup completed."
