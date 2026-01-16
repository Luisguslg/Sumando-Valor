param(
  [string]$SiteName = "SumandoValor",
  [string]$AppPoolName = "SumandoValorAppPool",
  [string]$ZipPath = "C:\Temp\SumandoValor_publish.zip",
  [string]$InstallDir = "C:\inetpub\sumandovalor\app",
  [int]$HttpPort = 80,
  [string]$HostName = "",

  # DB (SQL Auth)
  [string]$SqlServer = "VECCSAPP10\KPMGDV",
  [string]$SqlDatabase = "SumandoValorDb",
  [string]$SqlUser = "",
  [string]$SqlPassword = "",

  # SMTP (sin usuario/clave)
  [string]$SmtpHost = "mail.ve.kworld.kpmg.com",
  [int]$SmtpPort = 25,
  [bool]$SmtpEnableSsl = $false
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-Admin {
  $id = [Security.Principal.WindowsIdentity]::GetCurrent()
  $p = New-Object Security.Principal.WindowsPrincipal($id)
  if (-not $p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Ejecuta este script como Administrador."
  }
}

function Ensure-Module {
  Import-Module WebAdministration -ErrorAction Stop
}

function Ensure-Dir([string]$Path) {
  New-Item -ItemType Directory -Force -Path $Path | Out-Null
}

function Grant-AppPoolModify([string]$Path, [string]$Pool) {
  $acct = "IIS AppPool\$Pool"
  # NOTE: PowerShell treats "$var:" as a scoped variable reference; use ${} to delimit.
  & icacls "$Path" /grant "${acct}:(OI)(CI)M" /T | Out-Null
}

function Set-IisEnvVar([string]$Site, [string]$Name, [string]$Value) {
  $psPath = "IIS:\Sites\$Site"
  $filter = "system.webServer/aspNetCore/environmentVariables"

  # Remove existing with same name (avoid duplicates / invalid config)
  $existing = Get-WebConfigurationProperty -PSPath $psPath -Filter $filter -Name "." -ErrorAction SilentlyContinue
  if ($existing) {
    $toRemove = @()
    foreach ($item in $existing.Collection) {
      if ($item["name"] -eq $Name) { $toRemove += $item }
    }
    foreach ($item in $toRemove) {
      Remove-WebConfigurationProperty -PSPath $psPath -Filter $filter -Name "." -AtElement @{ name = $Name } -ErrorAction SilentlyContinue
    }
  }

  Add-WebConfigurationProperty -PSPath $psPath -Filter $filter -Name "." -Value @{ name = $Name; value = $Value } | Out-Null
}

Assert-Admin
Ensure-Module

Write-Host "== SumandoValor: IIS Install/Update ==" -ForegroundColor Cyan

if (-not (Test-Path $ZipPath)) {
  throw "No existe el ZIP: $ZipPath"
}

if ([string]::IsNullOrWhiteSpace($SqlUser)) {
  $SqlUser = Read-Host "SQL User (SQL Auth)"
}
if ([string]::IsNullOrWhiteSpace($SqlPassword)) {
  $sec = Read-Host "SQL Password (SQL Auth)" -AsSecureString
  $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec)
  try { $SqlPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}

Ensure-Dir $InstallDir

Write-Host "Descomprimiendo $ZipPath -> $InstallDir"
Expand-Archive -Path $ZipPath -DestinationPath $InstallDir -Force

Ensure-Dir (Join-Path $InstallDir "App_Data\Certificates")
Ensure-Dir (Join-Path $InstallDir "App_Data\DataProtection-Keys")
Ensure-Dir (Join-Path $InstallDir "logs")

Write-Host "Permisos (Modify) para AppPool en App_Data y logs"
Grant-AppPoolModify (Join-Path $InstallDir "App_Data") $AppPoolName
Grant-AppPoolModify (Join-Path $InstallDir "logs") $AppPoolName

if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
  Write-Host "Creando AppPool: $AppPoolName"
  New-WebAppPool -Name $AppPoolName | Out-Null
}

Write-Host "Configurando AppPool: No Managed Code"
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""

if (-not (Test-Path "IIS:\Sites\$SiteName")) {
  Write-Host "Creando Sitio: $SiteName -> $InstallDir"
  New-Website -Name $SiteName -PhysicalPath $InstallDir -Port $HttpPort -ApplicationPool $AppPoolName -Force | Out-Null
} else {
  Write-Host "Sitio existe: $SiteName (no se recrea)"
}

Write-Host "Bindings (HTTP)"
Get-WebBinding -Name $SiteName -Protocol "http" -ErrorAction SilentlyContinue | ForEach-Object {
  try { Remove-WebBinding -Name $SiteName -Protocol "http" -Port $_.bindingInformation.Split(':')[1] -HostHeader $_.Host -ErrorAction SilentlyContinue } catch {}
}
if ([string]::IsNullOrWhiteSpace($HostName)) {
  New-WebBinding -Name $SiteName -Protocol "http" -Port $HttpPort | Out-Null
} else {
  New-WebBinding -Name $SiteName -Protocol "http" -Port $HttpPort -HostHeader $HostName | Out-Null
}

Write-Host "Variables de entorno (IIS) para el sitio"
$conn = "Server=$SqlServer;Database=$SqlDatabase;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;MultipleActiveResultSets=True;"
Set-IisEnvVar -Site $SiteName -Name "ASPNETCORE_ENVIRONMENT" -Value "Production"
Set-IisEnvVar -Site $SiteName -Name "ConnectionStrings__DefaultConnection" -Value $conn

Set-IisEnvVar -Site $SiteName -Name "Email__Smtp__Enabled" -Value "true"
Set-IisEnvVar -Site $SiteName -Name "Email__Smtp__Host" -Value $SmtpHost
Set-IisEnvVar -Site $SiteName -Name "Email__Smtp__Port" -Value "$SmtpPort"
Set-IisEnvVar -Site $SiteName -Name "Email__Smtp__EnableSsl" -Value "$($SmtpEnableSsl.ToString().ToLower())"

Write-Host "Reiniciando IIS"
iisreset | Out-Null

Write-Host "OK. Sitio: http://localhost:$HttpPort/" -ForegroundColor Green

