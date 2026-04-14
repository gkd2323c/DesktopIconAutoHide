param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptDir "..")
$projectFile = Join-Path $projectRoot "DesktopIconAutoHide.csproj"
$outputDir = Join-Path $projectRoot ("artifacts\\singlefile\\" + $Runtime)

if (-not (Test-Path -LiteralPath $projectFile)) {
    throw "Project file not found: $projectFile"
}

if (Test-Path -LiteralPath $outputDir) {
    Remove-Item -LiteralPath $outputDir -Recurse -Force
}

Write-Host "Publishing single-file executable..." -ForegroundColor Cyan

dotnet publish $projectFile `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:PublishTrimmed=false `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -o $outputDir

$exePath = Join-Path $outputDir "DesktopIconAutoHide.exe"
if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Publish finished but executable not found: $exePath"
}

Write-Host "Done: $exePath" -ForegroundColor Green
