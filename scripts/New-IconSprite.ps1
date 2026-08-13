<#
.SYNOPSIS
    Regenerates BeeDay's official multi-provider icon library and sprite.

.DESCRIPTION
    Reads design/icons/catalog/icon-mapping.csv (BeeDayIconName, SymbolId, Provider,
    SourceName, Folder, Category, Variant, License) and, for each row:
      1. Reads the matching immutable source SVG from the provider's source folder
         under design/icons/source/ (never modified, never written to).
      2. Extracts its inner path content and viewBox.
      3. Preserves Lucide's outline attributes and currentColor stroke. For brand
         providers (Devicon, OfficialBrand), the artwork's own colors are
         preserved untouched — brand marks are not recolored.
      4. Writes a standalone icon file to
         src/BeeDay.Web/wwwroot/icons/{provider-slug}/{Folder}/{SymbolId}.svg.
      5. Adds a <symbol id="{SymbolId}"> entry to the combined sprite at
         src/BeeDay.Web/wwwroot/icons/sprite.svg.

    This script only touches src/BeeDay.Web/wwwroot/icons/. It never writes to
    design/icons/source/.

    Supported providers and their source folders:
      Lucide          -> design/icons/source/lucide/lucide--{SourceName}.svg
      Devicon         -> design/icons/source/devicon/devicon--{SourceName}.svg
      OfficialBrand   -> design/icons/source/official-brand/official-brand--{SourceName}.svg
      BeeDayCustom   -> design/icons/source/beeday-custom/beeday-custom--{SourceName}.svg

    To add a new icon: add a BeeDayIconName entry to BeeDayIconName.cs, add a
    matching row to design/icons/catalog/icon-mapping.csv referencing an existing
    file under the appropriate design/icons/source/{provider}/ folder, add the
    corresponding Define(...) line to BeeDayIconRegistry.cs, then re-run this script.

    To replace an existing icon's artwork: change its Provider/SourceName in
    icon-mapping.csv, then re-run this script. BeeDayIconName/BeeDayIconRegistry do
    not need to change.

.EXAMPLE
    pwsh scripts/New-IconSprite.ps1
#>

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $repoRoot 'design/icons/source'
$mappingPath = Join-Path $repoRoot 'design/icons/catalog/icon-mapping.csv'
$destRoot = Join-Path $repoRoot 'src/BeeDay.Web/wwwroot/icons'

$monochromeProviders = @('Lucide', 'BeeDayCustom')
$brandProviders = @('Devicon', 'OfficialBrand')

$providerSlugs = @{
    Lucide          = 'lucide'
    Devicon         = 'devicon'
    OfficialBrand   = 'official-brand'
    BeeDayCustom   = 'beeday-custom'
}

$providerSourcePrefix = @{
    Lucide          = 'lucide--'
    Devicon         = 'devicon--'
    OfficialBrand   = 'official-brand--'
    BeeDayCustom   = 'beeday-custom--'
}

if (-not (Test-Path $mappingPath)) {
    throw "Mapping file not found: $mappingPath"
}

function Assert-SafeToken {
    param([string]$Value, [string]$FieldName, [string]$RowContext)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "Empty $FieldName for $RowContext"
    }
    if ($Value -match '\.\.' -or $Value -match '[\\/]') {
        throw "Unsafe $FieldName '$Value' for $RowContext (path traversal or path separator not allowed)"
    }
    if ($Value -notmatch '^[A-Za-z0-9_-]+$') {
        throw "Unsafe $FieldName '$Value' for $RowContext (only letters, digits, '-', '_' are allowed)"
    }
}

$rows = Import-Csv -Path $mappingPath
Write-Host "Loaded $($rows.Count) icon mappings from $mappingPath"

# This directory contains generated output only. Clear provider folders so a source migration does
# not leave stale assets behind; immutable sources live under design/icons/source.
if (Test-Path $destRoot) {
    Get-ChildItem -LiteralPath $destRoot -Directory | Remove-Item -Recurse -Force
}

$svgPattern = '(?s)<svg([^>]*)>(.*)</svg>'
$symbolBlocks = New-Object System.Collections.Generic.List[string]
$seenSymbolIds = @{}

foreach ($row in $rows) {
    $rowContext = "$($row.BeeDayIconName) ($($row.SymbolId))"

    Assert-SafeToken -Value $row.SymbolId -FieldName 'SymbolId' -RowContext $rowContext
    Assert-SafeToken -Value $row.SourceName -FieldName 'SourceName' -RowContext $rowContext
    Assert-SafeToken -Value $row.Folder -FieldName 'Folder' -RowContext $rowContext

    if (-not $providerSlugs.ContainsKey($row.Provider)) {
        throw "Unsupported Provider '$($row.Provider)' for $rowContext. Expected one of: $($providerSlugs.Keys -join ', ')"
    }

    if ($seenSymbolIds.ContainsKey($row.SymbolId)) {
        throw "Duplicate SymbolId '$($row.SymbolId)' in mapping file (rows for $($seenSymbolIds[$row.SymbolId]) and $($row.BeeDayIconName))"
    }
    $seenSymbolIds[$row.SymbolId] = $row.BeeDayIconName

    $providerSlug = $providerSlugs[$row.Provider]
    $sourceFileName = "$($providerSourcePrefix[$row.Provider])$($row.SourceName).svg"
    $sourcePath = Join-Path (Join-Path $sourceRoot $providerSlug) $sourceFileName
    $resolvedSourceRoot = (Resolve-Path (Join-Path $sourceRoot $providerSlug)).ProviderPath

    if (-not (Test-Path $sourcePath)) {
        throw "Missing source SVG for $rowContext`: $sourcePath"
    }

    $resolvedSourcePath = (Resolve-Path $sourcePath).ProviderPath
    if (-not $resolvedSourcePath.StartsWith($resolvedSourceRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path traversal detected resolving source for $rowContext`: $resolvedSourcePath"
    }

    $content = Get-Content -Path $sourcePath -Raw
    $match = [regex]::Match($content, $svgPattern)
    if (-not $match.Success) {
        throw "Could not parse SVG structure for $sourcePath (malformed or unsupported SVG)"
    }

    $rootAttributes = $match.Groups[1].Value
    $viewBoxMatch = [regex]::Match($rootAttributes, 'viewBox="([^"]+)"')
    if ($viewBoxMatch.Success) {
        $viewBox = $viewBoxMatch.Groups[1].Value
    }
    else {
        $widthMatch = [regex]::Match($rootAttributes, 'width="([0-9.]+)"')
        $heightMatch = [regex]::Match($rootAttributes, 'height="([0-9.]+)"')
        if (-not ($widthMatch.Success -and $heightMatch.Success)) {
            throw "Could not determine a viewBox for $sourcePath (no viewBox, width, or height attribute)"
        }
        $viewBox = "0 0 $($widthMatch.Groups[1].Value) $($heightMatch.Groups[1].Value)"
    }

    $innerLines = $match.Groups[2].Value -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }
    $inner = $innerLines -join "`n    "

    $presentationAttributes = ''
    if ($row.Provider -eq 'Lucide') {
        $presentationAttributes = ' fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"'
    }
    elseif ($monochromeProviders -contains $row.Provider) {
        # Monochrome providers must resolve to currentColor regardless of whether
        # the source path already declares a fill attribute.
        $inner = [regex]::Replace($inner, 'fill="(?!none")[^"]*"', 'fill="currentColor"')
        if ($inner -notmatch 'fill="currentColor"') {
            $inner = [regex]::Replace($inner, '<path ', '<path fill="currentColor" ', 1)
        }
    }
    elseif ($brandProviders -notcontains $row.Provider) {
        throw "Provider '$($row.Provider)' is not classified as monochrome or brand for $rowContext"
    }
    # Brand providers (Devicon, OfficialBrand): colors are preserved as authored.

    $destDir = Join-Path (Join-Path $destRoot $providerSlug) $row.Folder
    New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    $destFile = Join-Path $destDir "$($row.SymbolId).svg"

    $iconSvg = "<svg xmlns=`"http://www.w3.org/2000/svg`" viewBox=`"$viewBox`"$presentationAttributes>`n    $inner`n</svg>`n"
    [System.IO.File]::WriteAllText($destFile, $iconSvg)

    $symbolBlocks.Add("  <symbol id=`"$($row.SymbolId)`" viewBox=`"$viewBox`"$presentationAttributes>`n    $inner`n  </symbol>")
}

$spriteHeader = @'
<?xml version='1.0' encoding='utf-8'?>
<!--
  BeeDay Icon Sprite
  Generated from multiple icon providers. Do not edit by hand.
  Regenerate via scripts/New-IconSprite.ps1.

  Providers:
    Lucide           — https://lucide.dev/             — ISC License
    Devicon          — https://devicon.dev/            — MIT License
    Official Brand   — https://simpleicons.org/         — CC0 1.0 (markup only; brand marks remain trademarks of their owners)

  See design/icons/source/{provider}/ATTRIBUTION.md for full attribution and
  design/icons/catalog/icon-mapping.csv for the per-icon provider/source record.
-->
<svg xmlns="http://www.w3.org/2000/svg" aria-hidden="true" style="display:none">
'@

$spriteContent = $spriteHeader + "`n" + ($symbolBlocks -join "`n") + "`n</svg>`n"
$spritePath = Join-Path $destRoot 'sprite.svg'
[System.IO.File]::WriteAllText($spritePath, $spriteContent)

Write-Host "Wrote $($rows.Count) individual icon files and sprite.svg ($($symbolBlocks.Count) symbols) to $destRoot"
