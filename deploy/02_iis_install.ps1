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
  & icacls $Path /grant "$acct:(OI)(CI)M" /T | Out-Null
}

Assert-Admin
Ensure-Module

Write-Host "== SumandoValor: IIS Install/Update ==" -ForegroundColor Cyan

if (-not (Test-Path $ZipPath)) {
  throw "No existe el ZIP: $ZipPath"
}

if ([string]::IsNullOrWhiteSpace($SqlUser) -or [string]::IsNullOrWhiteSpace($SqlPassword)) {
  throw "Debes pasar SqlUser y SqlPassword (SQL Auth) para configurar la conexión remota."
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
Get-WebBinding -Name $SiteName -Protocol "http" -ErrorAction SilentlyContinue | Remove-WebBinding -ErrorAction SilentlyContinue
New-WebBinding -Name $SiteName -Protocol "http" -Port $HttpPort -HostHeader $HostName

Write-Host "Variables de entorno (IIS) para el sitio"
$conn = "Server=$SqlServer;Database=$SqlDatabase;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;MultipleActiveResultSets=True;"

$appcmd = "$env:windir\System32\inetsrv\appcmd.exe"
& $appcmd set config "$SiteName" -section:system.webServer/aspNetCore /+"environmentVariables.[name='ASPNETCORE_ENVIRONMENT',value='Production']" /commit:apphost | Out-Null
& $appcmd set config "$SiteName" -section:system.webServer/aspNetCore /+"environmentVariables.[name='ConnectionStrings__DefaultConnection',value='$conn']" /commit:apphost | Out-Null

& $appcmd set config "$SiteName" -section:system.webServer/aspNetCore /+"environmentVariables.[name='Email__Smtp__Enabled',value='true']" /commit:apphost | Out-Null
& $appcmd set config "$SiteName" -section:system.webServer/aspNetCore /+"environmentVariables.[name='Email__Smtp__Host',value='$SmtpHost']" /commit:apphost | Out-Null
& $appcmd set config "$SiteName" -section:system.webServer/aspNetCore /+"environmentVariables.[name='Email__Smtp__Port',value='$SmtpPort']" /commit:apphost | Out-Null
& $appcmd set config "$SiteName" -section:system.webServer/aspNetCore /+"environmentVariables.[name='Email__Smtp__EnableSsl',value='$($SmtpEnableSsl.ToString().ToLower())']" /commit:apphost | Out-Null

Write-Host "Reiniciando IIS"
iisreset | Out-Null

Write-Host "OK. Sitio: http://localhost:$HttpPort/" -ForegroundColor Green

