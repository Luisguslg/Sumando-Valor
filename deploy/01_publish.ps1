param(
  [string]$ProjectRoot = (Resolve-Path "$PSScriptRoot\..").Path,
  [string]$OutDir = "$PSScriptRoot\..\artifacts",
  [string]$Configuration = "Release",
  [string]$PublishDirName = "publish",
  [string]$ZipName = "SumandoValor_publish.zip"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "== SumandoValor: Publish Release ==" -ForegroundColor Cyan

$publishDir = Join-Path $ProjectRoot $PublishDirName
$zipPath = Join-Path $OutDir $ZipName

if (Test-Path $publishDir) {
  Write-Host "Limpieza: $publishDir"
  Remove-Item -Recurse -Force $publishDir
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

Write-Host "dotnet publish -> $publishDir"
dotnet publish "$ProjectRoot\src\SumandoValor.Web\SumandoValor.Web.csproj" -c $Configuration -o $publishDir --nologo

if (Test-Path $zipPath) {
  Remove-Item -Force $zipPath
}

Write-Host "Creando ZIP -> $zipPath"
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

Write-Host "OK. Archivo listo: $zipPath" -ForegroundColor Green

