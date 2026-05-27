# Creates artifacts/speech-model-{profileId}.zip for Novolis.Audio.Voice.SherpaOnnx nupkg packing.
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('silero-vad', 'en-whisper-tiny')]
    [string] $ProfileId = 'silero-vad'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$source = Join-Path $repoRoot "models\$ProfileId"
$destDir = Join-Path $repoRoot 'artifacts'
$zip = Join-Path $destDir "speech-model-$ProfileId.zip"

if (-not (Test-Path (Join-Path $source '*'))) {
    throw "Missing $source — run: pwsh -File scripts/fetch-speech-model.ps1 -ProfileId $ProfileId"
}

New-Item -ItemType Directory -Force -Path $destDir | Out-Null
if (Test-Path $zip) { Remove-Item -Force $zip }
Compress-Archive -Path (Join-Path $source '*') -DestinationPath $zip -CompressionLevel Optimal
$mb = [math]::Round((Get-Item $zip).Length / 1MB, 2)
Write-Host "Wrote $zip ($mb MB)"
