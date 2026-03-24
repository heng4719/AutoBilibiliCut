param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$FrameworkDependent,
    [string]$IsccPath
)

$ErrorActionPreference = "Stop"

function Get-ProjectVersion {
    param([string]$ProjectFile)

    [xml]$projectXml = Get-Content $ProjectFile
    $versionNode = $projectXml.Project.PropertyGroup.Version | Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($versionNode)) {
        return "1.0.0"
    }

    return $versionNode
}

function Find-IsccPath {
    param([string]$PreferredPath)

    if (-not [string]::IsNullOrWhiteSpace($PreferredPath) -and (Test-Path $PreferredPath)) {
        return $PreferredPath
    }

    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $candidates = @(
        "E:\software\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

$projectRoot = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectRoot "videoCut.csproj"
$publishScript = Join-Path $PSScriptRoot "publish.ps1"
$installerScript = Join-Path $projectRoot "installer\videoCut.iss"
$publishDir = Join-Path $projectRoot "artifacts\publish\$RuntimeIdentifier"
$installerOutputDir = Join-Path $projectRoot "artifacts\installer"
$appVersion = Get-ProjectVersion -ProjectFile $projectFile
$isccPath = Find-IsccPath -PreferredPath $IsccPath

if (-not (Test-Path $publishDir)) {
    $publishArgs = @{
        Configuration = $Configuration
        RuntimeIdentifier = $RuntimeIdentifier
    }

    if ($FrameworkDependent.IsPresent) {
        $publishArgs.FrameworkDependent = $true
    }

    & $publishScript @publishArgs
}

if (-not (Test-Path $installerScript)) {
    throw "Installer script not found: $installerScript"
}

if (-not $isccPath) {
    throw "Inno Setup was not found. Install Inno Setup 6 first, then run this script again."
}

New-Item -ItemType Directory -Path $installerOutputDir -Force | Out-Null

$resolvedProjectRoot = (Resolve-Path $projectRoot).Path
$resolvedPublishDir = (Resolve-Path $publishDir).Path
$resolvedOutputDir = (Resolve-Path $installerOutputDir).Path

$compileArgs = @(
    "/DAppVersion=$appVersion",
    "/DProjectRoot=$resolvedProjectRoot",
    "/DPublishDir=$resolvedPublishDir",
    "/DOutputDir=$resolvedOutputDir",
    $installerScript
)

Write-Host "Building installer..."
& $isccPath @compileArgs

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compilation failed."
}

Write-Host ""
Write-Host "Installer completed."
Write-Host "Output: $installerOutputDir"
