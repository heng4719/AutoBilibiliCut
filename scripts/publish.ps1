param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$FrameworkDependent
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectRoot "videoCut.csproj"
$publishDir = Join-Path $projectRoot "artifacts\publish\$RuntimeIdentifier"

$selfContained = if ($FrameworkDependent.IsPresent) { "false" } else { "true" }

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

$arguments = @(
    "publish",
    $projectFile,
    "-c", $Configuration,
    "-r", $RuntimeIdentifier,
    "--self-contained", $selfContained,
    "-o", $publishDir,
    "/p:PublishSingleFile=false",
    "/p:PublishReadyToRun=false",
    "/p:DebugType=None",
    "/p:DebugSymbols=false"
)

Write-Host "Publishing to $publishDir ..."
& dotnet @arguments

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

Write-Host ""
Write-Host "Publish completed."
Write-Host "Output: $publishDir"
