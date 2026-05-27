# Downloads and extracts a Sherpa Piper model into models/{repoFolder}/
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('en-us-piper-amy', 'en-us-piper-lessac-low', 'en-us-piper-kristin-medium')]
    [string] $ProfileId
)

$ErrorActionPreference = 'Stop'

$downloads = @{
    'en-us-piper-amy' = @{
        Archive = 'vits-piper-en_US-amy-low.tar.bz2'
        Url = 'https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-amy-low.tar.bz2'
    }
    'en-us-piper-lessac-low' = @{
        Archive = 'vits-piper-en_US-lessac-low.tar.bz2'
        Url = 'https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-lessac-low.tar.bz2'
    }
    'en-us-piper-kristin-medium' = @{
        Archive = 'vits-piper-en_US-kristin-medium.tar.bz2'
        Url = 'https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-kristin-medium.tar.bz2'
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$modelsDir = Join-Path $repoRoot 'models'
$entry = $downloads[$ProfileId]
$archive = Join-Path $modelsDir $entry.Archive
$extractRoot = Join-Path $modelsDir $ProfileId

New-Item -ItemType Directory -Force -Path $modelsDir | Out-Null

if (-not (Test-Path $archive)) {
    Write-Host "Downloading $($entry.Archive)..."
    curl.exe -SL -o $archive $entry.Url
}

$tempExtract = Join-Path $modelsDir "_extract-$ProfileId"
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
