# Creates artifacts/voice-model-{profileId}.zip for Novolis.Audio.Voice.SherpaOnnx nupkg packing.
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('en-us-piper-amy', 'en-us-piper-lessac-low', 'en-us-piper-kristin-medium')]
    [string] $ProfileId = 'en-us-piper-amy'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$source = Join-Path $repoRoot "models\$ProfileId"
$destDir = Join-Path $repoRoot 'artifacts'
$zip = Join-Path $destDir "voice-model-$ProfileId.zip"

if (-not (Test-Path (Join-Path $source 'tokens.txt'))) {
    throw "Missing $source — run: pwsh -File scripts/fetch-voice-model.ps1 -ProfileId $ProfileId"
}

New-Item -ItemType Directory -Force -Path $destDir | Out-Null
if (Test-Path $zip) { Remove-Item -Force $zip }
Compress-Archive -Path (Join-Path $source '*') -DestinationPath $zip -CompressionLevel Optimal
$mb = [math]::Round((Get-Item $zip).Length / 1MB, 2)
Write-Host "Wrote $zip ($mb MB)"
