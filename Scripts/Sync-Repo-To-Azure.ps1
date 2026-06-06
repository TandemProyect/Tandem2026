<#
.SYNOPSIS
    Sincroniza el repositorio completo de GitHub hacia Azure DevOps Repos.

.DESCRIPTION
    El proyecto tandem2026 en Azure DevOps Repos suele contener solo ZwcadPlugin
    (repo antiguo). El monorepo completo vive en GitHub (Design.sln, Desing, DAL, ...).
    Este script empuja la rama master de GitHub al repo de Azure DevOps.

    Requiere PAT con permisos Code (Read & write). Usar variable de entorno:
      $env:AZDO_PAT = "<tu-pat>"
    o
      $env:AZURE_DEVOPS_PAT = "<tu-pat>"

.PARAMETER Branch
    Rama a publicar (por defecto: master).

.PARAMETER Force
    Fuerza el push si el historial de Azure DevOps es distinto (repo solo-plugin).

.PARAMETER DryRun
    Solo muestra lo que haría, sin ejecutar git push.

.EXAMPLE
    $env:AZDO_PAT = "xxxxx"
    .\Scripts\Sync-Repo-To-Azure.ps1

.EXAMPLE
    .\Scripts\Sync-Repo-To-Azure.ps1 -Force -DryRun
#>
[CmdletBinding()]
param(
    [string]$Branch = "master",
    [switch]$Force,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$org     = "VSCAD"
$project = "tandem2026"
$repo    = "tandem2026"
$remote  = "azure"

$pat = $env:AZDO_PAT
if ([string]::IsNullOrWhiteSpace($pat)) {
    $pat = $env:AZURE_DEVOPS_PAT
}
if ([string]::IsNullOrWhiteSpace($pat)) {
    throw "Define AZDO_PAT o AZURE_DEVOPS_PAT con un PAT de Azure DevOps (scope Code: Read & write)."
}

$authHeader = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
$headers = @{ Authorization = "Basic $authHeader" }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " SYNC REPO COMPLETO -> AZURE DEVOPS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Validar PAT
Write-Host "[1/4] Validando PAT..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod `
        -Uri "https://dev.azure.com/$org/$project/_apis/git/repositories?api-version=7.1" `
        -Headers $headers `
        -Method Get
    Write-Host "  OK - PAT valido" -ForegroundColor Green
}
catch {
    throw "PAT invalido o sin permisos (HTTP $($_.Exception.Response.StatusCode.value__)). Regenera el PAT en Azure DevOps."
}

# 2. Comprobar que estamos en el monorepo completo
Write-Host "[2/4] Comprobando estructura local..." -ForegroundColor Yellow
$expected = @("Design.sln", "Desing", "DAL", "TamdenZwcadPluging", "Scripts")
$missing = $expected | Where-Object { -not (Test-Path $_) }
if ($missing.Count -gt 0) {
    throw "Faltan carpetas del monorepo: $($missing -join ', '). Ejecuta desde la raiz del repo GitHub."
}
Write-Host "  OK - Monorepo completo detectado" -ForegroundColor Green

# 3. Configurar remote azure
Write-Host "[3/4] Configurando remote '$remote'..." -ForegroundColor Yellow
$azureUrl = "https://dev.azure.com/$org/$project/_git/$repo"
$existing = git remote get-url $remote 2>$null
if ($LASTEXITCODE -ne 0) {
    git remote add $remote $azureUrl
    Write-Host "  OK - Remote '$remote' anadido" -ForegroundColor Green
}
else {
    if ($existing -ne $azureUrl) {
        git remote set-url $remote $azureUrl
        Write-Host "  OK - URL de '$remote' actualizada" -ForegroundColor Green
    }
    else {
        Write-Host "  OK - Remote '$remote' ya configurado" -ForegroundColor Green
    }
}

# 4. Push
Write-Host "[4/4] Publicando rama '$Branch'..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Destino: $azureUrl" -ForegroundColor Cyan
Write-Host "Rama:    $Branch" -ForegroundColor Cyan
if ($Force) {
    Write-Host "Modo:    FORCE (reemplaza historial del repo solo-plugin en Azure)" -ForegroundColor Yellow
}
Write-Host ""

if ($DryRun) {
    $dryCmd = "git push"
    if ($Force) { $dryCmd += " --force" }
    $dryCmd += " $azureUrl ${Branch}:${Branch}"
    Write-Host "DRY RUN - Comando que se ejecutaria:" -ForegroundColor Magenta
    Write-Host "  $dryCmd" -ForegroundColor Magenta
    Write-Host ""
    Write-Host "Tras el push, en Azure DevOps > Repos > Files deberias ver:" -ForegroundColor Green
    foreach ($item in $expected) { Write-Host "  - $item" -ForegroundColor Green }
    exit 0
}

if (-not $Force) {
    Write-Host "Si el push falla por historiales distintos, repite con -Force:" -ForegroundColor Yellow
    Write-Host "  .\Scripts\Sync-Repo-To-Azure.ps1 -Force" -ForegroundColor Yellow
    Write-Host ""
}

# Git pide credenciales: inyectar PAT via URL (no se guarda en remote)
$pushUrl = "https://:$pat@dev.azure.com/$org/$project/_git/$repo"
$refspec = "${Branch}:${Branch}"
$gitArgs = @("-c", "credential.helper=", "push")
if ($Force) { $gitArgs += "--force" }
$gitArgs += @($pushUrl, $refspec)
& git @gitArgs

if ($LASTEXITCODE -ne 0) {
    throw "git push fallo. Prueba con -Force si Azure solo tenia ZwcadPlugin."
}

Write-Host ""
Write-Host "OK - Repositorio publicado en Azure DevOps" -ForegroundColor Green
Write-Host "URL: https://dev.azure.com/$org/$project/_git/$repo" -ForegroundColor Cyan
Write-Host "Files: https://dev.azure.com/$org/$project/_git/$repo?path=/&version=GB$Branch" -ForegroundColor Cyan
