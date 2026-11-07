param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$PluginDllPath = "",
    [string]$BridgeSourceDir = ""
)

$ErrorActionPreference = "Stop"

function Write-Step([string]$message) {
    Write-Host "[TandemOpenCAD] $message"
}

function Ensure-CargoAvailable() {
    $cargo = Get-Command cargo -ErrorAction SilentlyContinue
    if ($null -eq $cargo) {
        throw "No se encontro 'cargo' en PATH. Instala Rust (rustup) y abre una terminal nueva. Ejemplo: winget install --id Rustlang.Rustup -e"
    }
}

function Build-PluginIfNeeded([string]$projectRoot, [string]$configuration) {
    Ensure-CargoAvailable
    $modeArg = if ($configuration -eq "Release") { "--release" } else { "" }
    Write-Step "Compilando plugin Rust ($configuration)..."

    Push-Location $projectRoot
    try {
        if ([string]::IsNullOrWhiteSpace($modeArg)) {
            & cargo build
        } else {
            & cargo build $modeArg
        }
        if (-not $?) {
            throw "cargo build fallo."
        }
    }
    finally {
        Pop-Location
    }
}

function Resolve-PluginDllPath([string]$projectRoot, [string]$configuration, [string]$providedPath) {
    if (-not [string]::IsNullOrWhiteSpace($providedPath)) {
        if (-not (Test-Path $providedPath)) {
            throw "No existe PluginDllPath: $providedPath"
        }
        return (Resolve-Path $providedPath).Path
    }

    $searchRoot = Join-Path $projectRoot ("target\" + $configuration.ToLowerInvariant())
    if (-not (Test-Path $searchRoot)) {
        Build-PluginIfNeeded -projectRoot $projectRoot -configuration $configuration
    }

    $dll = Get-ChildItem -Path $searchRoot -Filter "*.dll" -File |
        Where-Object { $_.Name -match "tandem.*opencad.*plugin|tandem_opencadstudio_plugin" } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $dll) {
        $dll = Get-ChildItem -Path $searchRoot -Filter "*.dll" -File |
        Where-Object { $_.Name -notmatch "^(std-|api-ms-win-|vcruntime|msvcp|ucrtbase)" } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    }

    if ($null -eq $dll) {
        Build-PluginIfNeeded -projectRoot $projectRoot -configuration $configuration
        $dll = Get-ChildItem -Path $searchRoot -Filter "*.dll" -File |
        Where-Object { $_.Name -notmatch "^(std-|api-ms-win-|vcruntime|msvcp|ucrtbase)" } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    }

    if ($null -eq $dll) {
        throw "No se encontro ninguna DLL en $searchRoot. Compila el plugin Rust primero."
    }

    return $dll.FullName
}

function Resolve-BridgeSourceDir([string]$repoRoot, [string]$providedPath) {
    if (-not [string]::IsNullOrWhiteSpace($providedPath)) {
        if (-not (Test-Path $providedPath)) {
            throw "No existe BridgeSourceDir: $providedPath"
        }
        return (Resolve-Path $providedPath).Path
    }

    $candidates = @(
        (Join-Path $repoRoot "OpenCadStudioPluging\bin\Release"),
        (Join-Path $repoRoot "OpenCadStudioPluging\bin\Debug")
    )

    foreach ($candidate in $candidates) {
        if ((Test-Path $candidate) -and (Test-Path (Join-Path $candidate "OpenCadStudioPluging.exe"))) {
            return $candidate
        }
    }

    return $null
}

function Ensure-OpenCadStudioPathEnv() {
    $current = [Environment]::GetEnvironmentVariable("TANDEM_OPENCADSTUDIO_PATH", "User")
    if (-not [string]::IsNullOrWhiteSpace($current) -and (Test-Path $current)) {
        return $current
    }

    $candidates = @(
        "C:\Program Files\Open CAD Studio\OpenCADStudio.exe",
        "C:\Program Files (x86)\Open CAD Studio\OpenCADStudio.exe"
    )
    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            [Environment]::SetEnvironmentVariable("TANDEM_OPENCADSTUDIO_PATH", $candidate, "User")
            return $candidate
        }
    }

    return $null
}

function Copy-BridgeHost([string]$sourceDir, [string]$targetDir) {
    if ([string]::IsNullOrWhiteSpace($sourceDir)) {
        Write-Warning "No se encontro build del bridge C#. El menu aparecera, pero los botones Tandem fallaran hasta copiar OpenCadStudioPluging.exe."
        return
    }

    $requiredFiles = @(
        "OpenCadStudioPluging.exe",
        "OpenCadStudioPluging.dll",
        "OpenCadStudioPluging.runtimeconfig.json"
    )

    foreach ($required in $requiredFiles) {
        $full = Join-Path $sourceDir $required
        if (-not (Test-Path $full)) {
            throw "Falta archivo requerido del bridge: $full"
        }
    }

    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null

    foreach ($required in $requiredFiles) {
        Copy-Item -Path (Join-Path $sourceDir $required) -Destination (Join-Path $targetDir $required) -Force
    }

    Get-ChildItem -Path $sourceDir -File |
        Where-Object { $requiredFiles -notcontains $_.Name } |
        ForEach-Object {
            try {
                Copy-Item -Path $_.FullName -Destination (Join-Path $targetDir $_.Name) -Force -ErrorAction Stop
            }
            catch {
                Write-Warning "No se pudo copiar archivo opcional '$($_.Name)': $($_.Exception.Message)"
            }
        }
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path $scriptDir
$repoRoot = Resolve-Path (Join-Path $projectRoot "..")

$pluginToml = Join-Path $projectRoot "plugin.toml"
if (-not (Test-Path $pluginToml)) {
    throw "No existe plugin.toml en $projectRoot"
}

$pluginDll = Resolve-PluginDllPath -projectRoot $projectRoot -configuration $Configuration -providedPath $PluginDllPath
Write-Step "DLL plugin detectada: $pluginDll"

$appData = [Environment]::GetFolderPath("ApplicationData")
if ([string]::IsNullOrWhiteSpace($appData)) {
    throw "No se pudo resolver %APPDATA%."
}

$pluginInstallDir = Join-Path $appData "OpenCADStudio\plugins\tandem.opencadstudio"
$bridgeInstallDir = Join-Path $appData "Tandem\OpenCadStudioPluging\Host"

New-Item -ItemType Directory -Path $pluginInstallDir -Force | Out-Null
Get-ChildItem -Path $pluginInstallDir -Filter "*.dll" -File -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
Copy-Item -Path $pluginToml -Destination (Join-Path $pluginInstallDir "plugin.toml") -Force
Copy-Item -Path $pluginDll -Destination (Join-Path $pluginInstallDir (Split-Path -Leaf $pluginDll)) -Force

$bridgeSource = Resolve-BridgeSourceDir -repoRoot $repoRoot -providedPath $BridgeSourceDir
Copy-BridgeHost -sourceDir $bridgeSource -targetDir $bridgeInstallDir

$openCadPath = Ensure-OpenCadStudioPathEnv

Write-Step "Plugin instalado en: $pluginInstallDir"
Write-Step "Bridge C# en: $bridgeInstallDir"
if (-not [string]::IsNullOrWhiteSpace($openCadPath)) {
    Write-Step "Variable TANDEM_OPENCADSTUDIO_PATH: $openCadPath"
} else {
    Write-Warning "No se pudo detectar OpenCADStudio.exe. Configura TANDEM_OPENCADSTUDIO_PATH manualmente."
}
Write-Step "Reinicia OpenCADStudio para ver la pestaña 'Tandem 2026'."
