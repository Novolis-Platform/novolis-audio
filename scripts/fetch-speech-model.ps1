# Downloads bundled speech models into models/{profileId}/
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('silero-vad', 'en-whisper-tiny', 'all')]
    [string] $ProfileId = 'all'
)

$ErrorActionPreference = 'Stop'

$downloads = @{
    'silero-vad' = @{
        Files = @(
            @{
                Name = 'silero_vad.onnx'
                Url = 'https://github.com/k2-fsa/sherpa-onnx/releases/download/vad-models/silero_vad.onnx'
            }
        )
    }
    'en-whisper-tiny' = @{
        Archive = 'sherpa-onnx-whisper-tiny.en.tar.bz2'
        Url = 'https://github.com/k2-fsa/sherpa-onnx/releases/download/asr-models/sherpa-onnx-whisper-tiny.en.tar.bz2'
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$modelsDir = Join-Path $repoRoot 'models'

function Ensure-SileroVad {
    $dest = Join-Path $modelsDir 'silero-vad'
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    foreach ($file in $downloads['silero-vad'].Files) {
        $out = Join-Path $dest $file.Name
        if (-not (Test-Path $out)) {
            Write-Host "Downloading $($file.Name)..."
            curl.exe -SL -o $out $file.Url
        }
    }
    Write-Host "Model ready at $dest"
}

function Ensure-WhisperTiny {
    $entry = $downloads['en-whisper-tiny']
    $archive = Join-Path $modelsDir $entry.Archive
    $extractRoot = Join-Path $modelsDir 'en-whisper-tiny'

    New-Item -ItemType Directory -Force -Path $modelsDir | Out-Null
    if (-not (Test-Path $archive)) {
        Write-Host "Downloading $($entry.Archive)..."
        curl.exe -SL -o $archive $entry.Url
    }

    $tempExtract = Join-Path $modelsDir '_extract-en-whisper-tiny'
    if (Test-Path $tempExtract) { Remove-Item -Recurse -Force $tempExtract }
    New-Item -ItemType Directory -Force -Path $tempExtract | Out-Null
    tar -xf $archive -C $tempExtract

    $source = Get-ChildItem $tempExtract -Directory | Select-Object -First 1
    if (-not $source) { throw "Expected extracted folder in $tempExtract" }

    if (Test-Path $extractRoot) { Remove-Item -Recurse -Force $extractRoot }
    New-Item -ItemType Directory -Force -Path $extractRoot | Out-Null
    Copy-Item -Path (Join-Path $source.FullName '*') -Destination $extractRoot -Recurse -Force
    Remove-Item -Recurse -Force $tempExtract

    $sizeMb = [math]::Round((Get-ChildItem $extractRoot -Recurse -File | Measure-Object Length -Sum).Sum / 1MB, 2)
    Write-Host "Model ready at $extractRoot ($sizeMb MB)"
}

switch ($ProfileId) {
    'silero-vad' { Ensure-SileroVad }
    'en-whisper-tiny' { Ensure-WhisperTiny }
    'all' {
        Ensure-SileroVad
        Ensure-WhisperTiny
    }
}
