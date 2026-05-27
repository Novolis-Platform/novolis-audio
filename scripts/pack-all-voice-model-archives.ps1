# Packs all bundled voice models declared in the voice models manifest.
$ErrorActionPreference = 'Stop'
$ids = @('en-us-piper-amy', 'en-us-piper-lessac-low', 'en-us-piper-kristin-medium')
foreach ($id in $ids) {
    & "$PSScriptRoot\pack-voice-model-archive.ps1" -ProfileId $id
}
