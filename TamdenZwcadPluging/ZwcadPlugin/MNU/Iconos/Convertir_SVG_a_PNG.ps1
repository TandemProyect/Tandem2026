# Script para convertir iconos SVG a PNG (16x16 y 32x32)
# Usa Inkscape (debe estar instalado) o alternativa online

param(
	[string]$InputFolder = "C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos\Bootstrap-Icons\icons"
)

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "CONVERTIR SVG A PNG" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# Carpetas de salida
$outputFolder16 = "C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos\png\16x16"
$outputFolder32 = "C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos\png\32x32"

# Verificar si Inkscape está instalado
$inkscapePaths = @(
	"C:\Program Files\Inkscape\bin\inkscape.exe",
	"C:\Program Files (x86)\Inkscape\bin\inkscape.exe",
	"$env:ProgramFiles\Inkscape\bin\inkscape.exe"
)

$inkscape = $null
foreach ($path in $inkscapePaths) {
	if (Test-Path $path) {
		$inkscape = $path
		break
	}
}

if ($inkscape) {
	Write-Host "✓ Inkscape encontrado: $inkscape" -ForegroundColor Green

	# Convertir cada SVG
	Get-ChildItem $InputFolder -Filter *.svg | ForEach-Object {
		$baseName = $_.BaseName
		$svgPath = $_.FullName

		Write-Host "`nConvirtiendo: $($_.Name)" -ForegroundColor Cyan

		# 16x16
		$png16 = Join-Path $outputFolder16 "$baseName.png"
		& $inkscape --export-type=png --export-width=16 --export-height=16 --export-filename=$png16 $svgPath 2>$null
		if (Test-Path $png16) {
			Write-Host "  ✓ $baseName.png (16x16)" -ForegroundColor Green
		}

		# 32x32
		$png32 = Join-Path $outputFolder32 "$baseName.png"
		& $inkscape --export-type=png --export-width=32 --export-height=32 --export-filename=$png32 $svgPath 2>$null
		if (Test-Path $png32) {
			Write-Host "  ✓ $baseName.png (32x32)" -ForegroundColor Green
		}
	}

	Write-Host "`n✓ Conversion completada!" -ForegroundColor Green
	Write-Host "`nIconos PNG generados en:" -ForegroundColor Cyan
	Write-Host "  16x16: $outputFolder16" -ForegroundColor White
	Write-Host "  32x32: $outputFolder32" -ForegroundColor White

} else {
	Write-Host "❌ Inkscape no esta instalado" -ForegroundColor Red
	Write-Host "`nOPCIONES:" -ForegroundColor Yellow
	Write-Host "`n1. Instalar Inkscape (Recomendado)" -ForegroundColor Cyan
	Write-Host "   https://inkscape.org/release/"
	Write-Host "   Ejecuta este script de nuevo despues de instalar"

	Write-Host "`n2. Convertir online (Manual)" -ForegroundColor Cyan
	Write-Host "   https://convertio.co/es/svg-png/"
	Write-Host "   Sube cada .svg y descarga en 16x16 y 32x32"

	Write-Host "`n3. Usar ImageMagick (Alternativa)" -ForegroundColor Cyan
	Write-Host "   https://imagemagick.org/script/download.php"

	Write-Host "`n¿Quieres abrir la pagina de descarga de Inkscape? (S/N)" -ForegroundColor Yellow
	$respuesta = Read-Host
	if ($respuesta -eq "S" -or $respuesta -eq "s") {
		Start-Process "https://inkscape.org/release/"
	}
}

Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
