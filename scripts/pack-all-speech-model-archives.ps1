# Packs all bundled speech models declared in the speech models manifest.
$ErrorActionPreference = 'Stop'
$ids = @('silero-vad', 'en-whisper-tiny')
foreach ($id in $ids) {
  $source = Join-Path (Split-Path -Parent $PSScriptRoot) "models\$id"
  if (Test-Path $source) {
    & "$PSScriptRoot\pack-speech-model-archive.ps1" -ProfileId $id
  } else {
    Write-Host "SKIP $id (missing $source)"
  }
}
