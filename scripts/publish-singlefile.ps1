param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$SatelliteResourceLanguages = "zh-Hans;en",
    [ValidateSet("safe", "aggressive")]
    [string]$SizeMode = "safe"
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
$satelliteLanguagesArg = $SatelliteResourceLanguages -replace ";", "%3B"

$publishArgs = @(
    "publish", $projectFile,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:EnableCompressionInSingleFile=true",
    "-p:PublishTrimmed=false",
    "-p:DebugType=none",
    "-p:DebugSymbols=false",
    "-p:SatelliteResourceLanguages=$satelliteLanguagesArg",
    "-o", $outputDir
)

if ($SizeMode -eq "aggressive") {
    Write-Host "Mode: aggressive (trim + satellite resource pruning)." -ForegroundColor Yellow
    $publishArgs += @(
        "-p:PublishTrimmed=true",
        "-p:TrimMode=partial",
        "-p:_SuppressWinFormsTrimError=true"
    )
}
else {
    Write-Host "Mode: safe (no trim, satellite resource pruning only)." -ForegroundColor Green
}

dotnet @publishArgs

$exePath = Join-Path $outputDir "DesktopIconAutoHide.exe"
if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Publish finished but executable not found: $exePath"
}

Write-Host "Done: $exePath" -ForegroundColor Green
