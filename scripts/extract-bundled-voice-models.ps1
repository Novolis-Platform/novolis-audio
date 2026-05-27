# Extracts Novolis voice model zips under {OutputDir}/models/{profileId}/ when not already materialized.
param(
    [Parameter(Mandatory = $true)]
    [string] $OutputDir
)

$ErrorActionPreference = 'Stop'
$modelsRoot = Join-Path $OutputDir 'models'
if (-not (Test-Path $modelsRoot)) { return }

Get-ChildItem $modelsRoot -Filter '*.zip' -File | ForEach-Object {
    $profileId = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
    $dest = Join-Path $modelsRoot $profileId
    $tokens = Join-Path $dest 'tokens.txt'
    $phontab = Join-Path $dest 'espeak-ng-data\phontab'

    if ((Test-Path $tokens) -and (Test-Path $phontab)) { return }

    if (Test-Path $dest) { Remove-Item -Recurse -Force $dest }
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    Expand-Archive -LiteralPath $_.FullName -DestinationPath $dest -Force
}
