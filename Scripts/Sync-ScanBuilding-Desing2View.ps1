# Descarga desde GitHub (feature/ScanBuilding) los archivos que arreglan CS0117 en Desing_2.
# Ejecutar en PowerShell desde C:\00_Tandem2026 (o cualquier cwd; usa rutas absolutas abajo).

$ErrorActionPreference = 'Stop'
$root = 'C:\00_Tandem2026'
$branch = 'feature/ScanBuilding'
$base = "https://raw.githubusercontent.com/TandemProyect/Tandem2026/$branch"

$files = @(
    'Desing/Views/Desing_2/_Desing2StlViewerWorkspace.cshtml',
    'Desing/Resources/MasterArticles.resx',
    'Desing/Resources/MasterArticles.en.resx',
    'Desing/Resources/MasterArticles.Designer.cs',
    'Desing/Views/Desing_2/Viewer.cshtml',
    'Desing/Scripts/Desing2/desing2-map-building-import.js',
    'Desing/assets/materio/css/site.css'
)

if (-not (Test-Path $root)) {
    throw "No existe $root. Ajusta `$root en este script a tu carpeta del repo."
}

foreach ($rel in $files) {
    $url = "$base/$($rel.Replace('\','/'))"
    $dest = Join-Path $root ($rel -replace '/', '\')
    $dir = Split-Path $dest -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Write-Host "Descargando $rel ..."
    Invoke-WebRequest -Uri $url -OutFile $dest -UseBasicParsing
}

Write-Host ""
Write-Host "OK. Archivos actualizados desde $branch."
Write-Host "Ahora en Visual Studio: Limpiar solucion -> Recompilar -> Ctrl+F5."
Write-Host "Comprueba que esta linea NO diga MasterArticles.StlPreview_ImageSketchErrorModalTitle:"
Write-Host "  Desing\Views\Desing_2\_Desing2StlViewerWorkspace.cshtml (busca imageSketchErrorModalTitle)"
