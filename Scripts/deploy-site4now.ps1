#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compila Design (MVC 4.8) en Release y publica una carpeta lista para FTP a Site4Now.

.DESCRIPTION
    No sube nada ni almacena contraseñas. Tras ejecutar, sube el contenido generado con
    FileZilla / WinSCP el panel FTP.

    Guía completa: Docs/General/Deploy-Site4Now.md

.NOTES
    - Requiere Visual Studio Build Tools (o VS) con carga ASP.NET / Web.MSBuild y nuget CLI en PATH (después del setup de NuGet en VS también vale).
    - La salida coincide con el ejemplo de artefactos del repo (.gitignored).
#>

param(
    [string]$PublishRoot = "",
    [switch]$SkipNuGetRestore
)

$ErrorActionPreference = 'Stop'

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$DesignProj = Join-Path $RepoRoot 'Desing\Design.csproj'

if (-not (Test-Path $DesignProj)) {
    throw "No se encuentra $DesignProj (¿estás ejecutando desde el repo?)."
}

if ([string]::IsNullOrWhiteSpace($PublishRoot)) {
    $PublishRoot = Join-Path $RepoRoot 'artifacts\design_site4now_manual'
}

$VsWhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $VsWhere)) {
    throw @"
No encontrado vswhere.exe. Instala Visual Studio / Build Tools con la carga ASP.NET (Web.MSBuild targets).
"@

}

$installationPath = & $VsWhere `
    -latest `
    -products * `
    -requires Microsoft.VisualStudio.Component.Web.MSBuild `
    -property installationPath `
    | Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($installationPath)) {
    $installationPath = & $VsWhere `
        -latest `
        -products * `
        -requires Microsoft.VisualStudio.Workload.MSBuildTools `
        -property installationPath `
        | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($installationPath)) {
    throw 'No hay instalación de Visual Studio compatible con compilación ASP.NET.'
}

$toolRoot = Join-Path $installationPath 'MSBuild\Microsoft\VisualStudio'
$ordered =
    @( Get-ChildItem -Path $toolRoot -Directory -ErrorAction Stop |
           Where-Object { $_.Name -match '^v\d' } |
           Sort-Object @{ Expression = {
                                    try {
                                        $suffix = ($_.Name -replace '^v', '').Split('.')[0..3] -join '.'
                                        [version]::Parse($suffix)
                                    }
                                    catch { [version]::Parse('0.0') }
                                }; Ascending = $false })

$env:VSToolsPath = $null
foreach ($dir in $ordered) {
    $targets = Join-Path $dir.FullName 'WebApplications\Microsoft.WebApplication.targets'
    if (Test-Path $targets) {
        $env:VSToolsPath = $dir.FullName
        Write-Host "VSToolsPath = $($env:VSToolsPath)"
        break
    }
}

if ([string]::IsNullOrWhiteSpace($env:VSToolsPath)) {
    throw "Falta Microsoft.WebApplication.targets bajo '$toolRoot'."
}

if (-not (Get-Command msbuild.exe -ErrorAction SilentlyContinue)) {
    Write-Warning "msbuild no está en PATH. Abre 'Developer PowerShell for VS', o ejecuta desde x64 VS prompt."
}

$cmdMsBuild = Get-Command msbuild.exe -ErrorAction SilentlyContinue
$MsBuildExe = $(if ($cmdMsBuild) { $cmdMsBuild.Source })
if ([string]::IsNullOrWhiteSpace($MsBuildExe)) {
    $candidates =
        @( Join-Path $installationPath 'MSBuild\Current\Bin\amd64\MSBuild.exe'),
           (Join-Path $installationPath 'MSBuild\Current\Bin\MSBuild.exe')

    foreach ($c in $candidates) {
        if (Test-Path $c) { $MsBuildExe = $c; break }
    }
}

if ([string]::IsNullOrWhiteSpace($MsBuildExe)) {
    throw "No se encuentra MSBuild.exe. Instala/compila con Visual Studio 2022+ (carga ASP.NET)."
}

$Sln = Join-Path $RepoRoot 'Design.sln'
if (-not $SkipNuGetRestore) {
    if (-not (Get-Command nuget -ErrorAction SilentlyContinue)) {
        Write-Warning "Comando 'nuget' ausente — restaura manualmente antes de ejecutar este script:"
        Write-Warning "    nuget restore `"$Sln`""
        Write-Warning "O ejecuta desde 'Developer Command Prompt'. Saltando Restore."
    }
    else {
        & nuget restore $Sln -NonInteractive | Write-Host
    }
}

if (Test-Path $PublishRoot) {
    Remove-Item $PublishRoot -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $PublishRoot | Out-Null

Write-Host "`nPublishUrl = $PublishRoot`n"

& $MsBuildExe $DesignProj `
    /m `
    /p:Configuration=Release `
    /p:DeployOnBuild=true `
    /p:WebPublishMethod=FileSystem `
    /p:"PublishUrl=$PublishRoot" `
    /p:DeleteExistingFiles=true

if (-not (Test-Path (Join-Path $PublishRoot 'Web.config'))) {
    throw "Fallo al publicar: no aparece Web.config en $PublishRoot"
}

Write-Host @"


OK — Artefactos en:
  $PublishRoot

Siguientes pasos:
  1) Credenciales FTP (Site4Now / panel): servidor, usuario, contraseña, carpeta remota correcta (ver Docs/General/Deploy-Site4Now.md).
  2) En el servidor, mantén Web.GoogleMaps.config fuera de Git pero presente físicamente.
  3) Para despliegue automático tras push a master mira el workflow .github/workflows/deploy-site4now.yml

"@
