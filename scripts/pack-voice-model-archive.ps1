# Creates artifacts/voice-model-en-us-piper-amy.zip for Novolis.Audio.Voice.SherpaOnnx nupkg packing.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$source = Join-Path $repoRoot 'models\en-us-piper-amy'
$destDir = Join-Path $repoRoot 'artifacts'
$zip = Join-Path $destDir 'voice-model-en-us-piper-amy.zip'

if (-not (Test-Path (Join-Path $source 'tokens.txt'))) {
    throw "Missing $source — run scripts/fetch-piper-model.ps1 and git lfs pull first."
}

New-Item -ItemType Directory -Force -Path $destDir | Out-Null
if (Test-Path $zip) { Remove-Item -Force $zip }
Compress-Archive -Path (Join-Path $source '*') -DestinationPath $zip -CompressionLevel Optimal
$mb = [math]::Round((Get-Item $zip).Length / 1MB, 2)
Write-Host "Wrote $zip ($mb MB)"
