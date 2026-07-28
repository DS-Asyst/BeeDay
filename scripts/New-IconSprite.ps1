<#
.SYNOPSIS
    Regenerates LevelUp's official Streamline-derived icon library and sprite.

.DESCRIPTION
    Reads design/icons/catalog/icon-mapping.csv (PixelIconName, SymbolId, Folder,
    Category, SourceFile) and, for each row:
      1. Reads the matching immutable source SVG from
         design/icons/source/streamline-pixel/ (never modified, never written to).
      2. Extracts its inner path content and viewBox.
      3. Writes a standalone icon file to
         src/LevelUp.Web/wwwroot/icons/streamline/{Folder}/{SymbolId}.svg.
      4. Adds a <symbol id="{SymbolId}"> entry to the combined sprite at
         src/LevelUp.Web/wwwroot/icons/streamline/sprite.svg.

    This script only touches src/LevelUp.Web/wwwroot/icons/streamline/. It never
    writes to design/icons/source/streamline-pixel/.

    To add a new icon: add a PixelIconName entry to PixelIconName.cs, add a
    matching row to design/icons/catalog/icon-mapping.csv referencing an existing
    file under design/icons/source/streamline-pixel/, add the corresponding
    Define(...) line to PixelIconRegistry.cs, then re-run this script.

    To replace an existing icon's artwork: change its SourceFile in
    icon-mapping.csv to a different file from the source library, then re-run
    this script. PixelIconName/PixelIconRegistry do not need to change.

.EXAMPLE
    pwsh scripts/New-IconSprite.ps1
#>

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceDir = Join-Path $repoRoot 'design/icons/source/streamline-pixel'
$mappingPath = Join-Path $repoRoot 'design/icons/catalog/icon-mapping.csv'
$destRoot = Join-Path $repoRoot 'src/LevelUp.Web/wwwroot/icons/streamline'

if (-not (Test-Path $mappingPath)) {
    throw "Mapping file not found: $mappingPath"
}

$rows = Import-Csv -Path $mappingPath
Write-Host "Loaded $($rows.Count) icon mappings from $mappingPath"

$svgPattern = '(?s)<svg[^>]*viewBox="([^"]+)"[^>]*>(.*)</svg>'
$symbolBlocks = New-Object System.Collections.Generic.List[string]
$seenSymbolIds = @{}

foreach ($row in $rows) {
    $sourcePath = Join-Path $sourceDir $row.SourceFile
    if (-not (Test-Path $sourcePath)) {
        throw "Missing source SVG for $($row.PixelIconName): $sourcePath"
    }

    if ($seenSymbolIds.ContainsKey($row.SymbolId)) {
        throw "Duplicate SymbolId '$($row.SymbolId)' in mapping file (rows for $($seenSymbolIds[$row.SymbolId]) and $($row.PixelIconName))"
    }
    $seenSymbolIds[$row.SymbolId] = $row.PixelIconName

    $content = Get-Content -Path $sourcePath -Raw
    $match = [regex]::Match($content, $svgPattern)
    if (-not $match.Success) {
        throw "Could not parse SVG structure for $sourcePath"
    }

    $viewBox = $match.Groups[1].Value
    $innerLines = $match.Groups[2].Value -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }
    $inner = $innerLines -join "`n    "

    $destDir = Join-Path $destRoot $row.Folder
    New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    $destFile = Join-Path $destDir "$($row.SymbolId).svg"

    $iconSvg = "<svg xmlns=`"http://www.w3.org/2000/svg`" viewBox=`"$viewBox`">`n    $inner`n</svg>`n"
    [System.IO.File]::WriteAllText($destFile, $iconSvg)

    $symbolBlocks.Add("  <symbol id=`"$($row.SymbolId)`" viewBox=`"$viewBox`">`n    $inner`n  </symbol>")
}

$spriteHeader = @'
<?xml version='1.0' encoding='utf-8'?>
<!--
  LevelUp Pixel Icon Sprite
  Generated from the Streamline Pixel icon collection.
  Source: https://icon-sets.iconify.design/streamline-pixel/
  Author: Streamline (https://streamlinehq.com)
  License: CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/)
  Do not edit by hand. Regenerate via scripts/New-IconSprite.ps1.
-->
<svg xmlns="http://www.w3.org/2000/svg" aria-hidden="true" style="display:none">
'@

$spriteContent = $spriteHeader + "`n" + ($symbolBlocks -join "`n") + "`n</svg>`n"
$spritePath = Join-Path $destRoot 'sprite.svg'
[System.IO.File]::WriteAllText($spritePath, $spriteContent)

Write-Host "Wrote $($rows.Count) individual icon files and sprite.svg ($($symbolBlocks.Count) symbols) to $destRoot"
