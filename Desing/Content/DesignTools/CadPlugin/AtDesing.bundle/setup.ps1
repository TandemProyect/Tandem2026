$src = Split-Path -Parent $MyInvocation.MyCommand.Path
$dest = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\AtDesing.bundle"
New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item -Path (Join-Path $src "*") -Destination $dest -Recurse -Force
Write-Host "AtDesing instalado en $dest"
Write-Host "Cierra AutoCAD y vuelve a abrirlo."
