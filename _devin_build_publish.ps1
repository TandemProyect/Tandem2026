$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) { throw 'vswhere no encontrado' }
$installPath = & $vswhere -latest -products * -property installationPath | Select-Object -First 1
if (-not $installPath) { throw 'Visual Studio no encontrado' }
Write-Output "VS=$installPath"
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { $msbuild = Join-Path $installPath 'MSBuild\Current\Bin\MSBuild.exe' }
Write-Output "MSBuild=$msbuild"
$toolRoot = Join-Path $installPath 'MSBuild\Microsoft\VisualStudio'
$vstools = $null
Get-ChildItem $toolRoot -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^v\d' } | Sort-Object Name -Descending | ForEach-Object {
    if (-not $vstools -and (Test-Path (Join-Path $_.FullName 'WebApplications\Microsoft.WebApplication.targets'))) { $vstools = $_.FullName }
}
if (-not $vstools) { throw 'VSToolsPath no encontrado' }
Write-Output "VSToolsPath=$vstools"
$pub = 'C:\00_Tandem2026\artifacts\design_site4now'
if (Test-Path $pub) { Remove-Item $pub -Recurse -Force }
New-Item -ItemType Directory -Force -Path $pub | Out-Null
$proj = 'C:\00_Tandem2026\Desing\Design.csproj'
& $msbuild $proj /m /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=Site4Now-FileSystem "/p:VSToolsPath=$vstools" /v:m /nologo
if ($LASTEXITCODE -ne 0) { throw "MSBuild salio con $LASTEXITCODE" }
if (-not (Test-Path (Join-Path $pub 'Web.config'))) { throw 'La publicacion no dejo Web.config' }
Write-Output 'BUILD_OK'
