# Script para organizar Bootstrap Icons después de descargar
# Ejecutar después de descargar el ZIP

param(
	[string]$ZipPath = "$env:USERPROFILE\Downloads\bootstrap-icons-*.zip"
)

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "ORGANIZANDO BOOTSTRAP ICONS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# 1. Buscar el archivo ZIP descargado
$zipFile = Get-ChildItem $ZipPath | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $zipFile) {
	Write-Host "❌ No se encontró el archivo ZIP en Descargas" -ForegroundColor Red
	Write-Host "Por favor, especifica la ruta manualmente:" -ForegroundColor Yellow
	Write-Host '.\Organizar_Bootstrap_Icons.ps1 -ZipPath "C:\ruta\al\archivo.zip"'
	exit
}

Write-Host "`n✓ Archivo encontrado: $($zipFile.Name)" -ForegroundColor Green

# 2. Crear carpeta de destino
$destinoBase = "C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos"
$destinoBiblioteca = Join-Path $destinoBase "Bootstrap-Icons\icons"

New-Item -ItemType Directory -Path $destinoBiblioteca -Force | Out-Null
Write-Host "✓ Carpeta biblioteca creada: $destinoBiblioteca" -ForegroundColor Green

# 3. Extraer ZIP
Write-Host "`nExtrayendo archivos..." -ForegroundColor Cyan
Expand-Archive -Path $zipFile.FullName -DestinationPath $destinoBiblioteca -Force
Write-Host "✓ Archivos extraídos" -ForegroundColor Green

# 4. Buscar carpeta de SVGs
$svgFolder = Get-ChildItem -Path $destinoBiblioteca -Recurse -Directory | 
			 Where-Object { $_.Name -eq "icons" } | 
			 Select-Object -First 1

if ($svgFolder) {
	Write-Host "✓ Encontrados $(Get-ChildItem $svgFolder.FullName -Filter *.svg | Measure-Object | Select-Object -ExpandProperty Count) iconos SVG" -ForegroundColor Green
}

# 5. Copiar iconos específicos necesarios para Tandem 2026
Write-Host "`nCopiando iconos necesarios para el proyecto..." -ForegroundColor Cyan

$iconosNecesarios = @{
	"speedometer2.svg" = "panel"
	"search.svg" = "detectar"
	"box.svg" = "generar3d"
	"arrow-clockwise.svg" = "regenerar"
	"gear.svg" = "encofrado"
	"download.svg" = "leer"
	"save.svg" = "guardar"
}

$carpetaSvgProyecto = Join-Path $destinoBase "Bootstrap-Icons\icons"

foreach ($original in $iconosNecesarios.Keys) {
	$nuevoNombre = $iconosNecesarios[$original]
	$origen = Join-Path $svgFolder.FullName $original

	if (Test-Path $origen) {
		Copy-Item $origen -Destination "$carpetaSvgProyecto\$nuevoNombre.svg"
		Write-Host "  ✓ $original → $nuevoNombre.svg" -ForegroundColor Green
	} else {
		Write-Host "  ⚠ No se encontró: $original" -ForegroundColor Yellow
	}
}

# 6. Crear archivo de índice
Write-Host "`nCreando indice de iconos..." -ForegroundColor Cyan
$indexPath = Join-Path $destinoBiblioteca "INDICE_ICONOS.txt"
Get-ChildItem $svgFolder.FullName -Filter *.svg | 
	Select-Object Name | 
	Out-File $indexPath -Encoding UTF8

Write-Host "✓ Indice creado: $indexPath" -ForegroundColor Green

# 7. Resumen
Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "`nBiblioteca completa:" -ForegroundColor Green
Write-Host "  $destinoBiblioteca"
Write-Host "`nIconos seleccionados para Tandem 2026:" -ForegroundColor Green
Write-Host "  $carpetaSvgProyecto"
Write-Host "`nIconos copiados:" -ForegroundColor Cyan
Get-ChildItem $carpetaSvgProyecto -Filter *.svg | ForEach-Object { Write-Host "  - $($_.Name)" }

Write-Host "`n⚠ NOTA: Los iconos estan en formato SVG." -ForegroundColor Yellow
Write-Host "Necesitas convertirlos a PNG 16x16 y 32x32 para ZWCAD." -ForegroundColor Yellow
Write-Host "`nEjecuta el siguiente script para convertir:" -ForegroundColor Cyan
Write-Host "  .\Convertir_SVG_a_PNG.ps1" -ForegroundColor White

Write-Host "`n✓ Organizacion completada!" -ForegroundColor Green
