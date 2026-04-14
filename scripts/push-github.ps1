param(
    [string]$Message = "",
    [string]$Remote = "origin",
    [string]$Branch = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptDir "..")
Set-Location $projectRoot

git --version | Out-Null

if (-not (Test-Path -LiteralPath (Join-Path $projectRoot ".git"))) {
    throw "Current directory is not a git repository: $projectRoot"
}

$remoteUrl = ""
try {
    $remoteUrl = (git remote get-url $Remote).Trim()
}
catch {
    throw "Git remote '$Remote' not found."
}

if ([string]::IsNullOrWhiteSpace($remoteUrl)) {
    throw "Git remote '$Remote' has no URL."
}

if ([string]::IsNullOrWhiteSpace($Branch)) {
    $Branch = (git branch --show-current).Trim()
}

if ([string]::IsNullOrWhiteSpace($Branch)) {
    throw "Cannot detect current branch. Please pass -Branch explicitly."
}

Write-Host "Repository: $projectRoot" -ForegroundColor Cyan
Write-Host "Remote: $Remote ($remoteUrl)" -ForegroundColor Cyan
Write-Host "Branch: $Branch" -ForegroundColor Cyan

git add -A

git diff --cached --quiet
$hasStagedChanges = ($LASTEXITCODE -ne 0)

if ($hasStagedChanges) {
    if ([string]::IsNullOrWhiteSpace($Message)) {
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $Message = "chore: update ($timestamp)"
    }

    git commit -m $Message
}
else {
    Write-Host "No staged changes to commit." -ForegroundColor Yellow
}

git push $Remote $Branch

Write-Host "Done." -ForegroundColor Green
