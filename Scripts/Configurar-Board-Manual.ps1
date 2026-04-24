# ============================================================================
# Script: Configurar-Board-Manual.ps1
# Propósito: Guía paso a paso para configurar el board manualmente
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "GUÍA: CONFIGURAR BOARD TANDEM2026" -ForegroundColor Yellow
Write-Host "Copiando estructura de Athos_2025" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

Write-Host "`n⚠ La API de Azure DevOps tiene limitaciones para modificar" -ForegroundColor Yellow
Write-Host "  la configuración de boards existentes." -ForegroundColor Yellow
Write-Host "`nLa forma más confiable es hacerlo manualmente:" -ForegroundColor Cyan

# Abrir la configuración del board
Write-Host "`n[PASO 1] Abriendo configuración del board..." -ForegroundColor Cyan
Start-Process "https://dev.azure.com/VSCAD/tandem2026/_settings/board-team?teamId=tandem2026%20Team"
Start-Sleep -Seconds 2

Write-Host "`n[PASO 2] Sigue estos pasos EN LA PÁGINA WEB:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  1. Haz clic en 'Board' en el menú lateral izquierdo" -ForegroundColor White
Write-Host "  2. Haz clic en 'Columns'" -ForegroundColor White
Write-Host "  3. Haz clic en '+ New column' para cada columna nueva" -ForegroundColor White
Write-Host ""

Write-Host "`n[COLUMNAS A CREAR]" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Gray

$columns = @(
	@{
		Name = "Tareas a Analizar"
		State = "New"
		WIPLimit = $null
		Order = 1
	},
	@{
		Name = "Esperando documentación"
		State = "Active"
		WIPLimit = $null
		Order = 2
	},
	@{
		Name = "Preparado para Realizar"
		State = "Active"
		WIPLimit = 25
		Order = 3
	},
	@{
		Name = "Mal Testeado Volver a Realizar"
		State = "Active"
		WIPLimit = $null
		Order = 4
	},
	@{
		Name = "Realizando"
		State = "Active"
		WIPLimit = $null
		Order = 5
	},
	@{
		Name = "Preparando a testear"
		State = "Resolved"
		WIPLimit = 100
		Order = 6
	},
	@{
		Name = "Preparado para presentar"
		State = "Closed"
		WIPLimit = $null
		Order = 7
	}
)

foreach ($col in $columns) {
	Write-Host "`n$($col.Order). " -NoNewline -ForegroundColor Yellow
	Write-Host "$($col.Name)" -ForegroundColor White
	Write-Host "   Estado: " -NoNewline -ForegroundColor Gray
	Write-Host "$($col.State)" -ForegroundColor Cyan

	if ($col.WIPLimit) {
		Write-Host "   WIP Limit: " -NoNewline -ForegroundColor Gray
		Write-Host "$($col.WIPLimit)" -ForegroundColor Cyan
	} else {
		Write-Host "   WIP Limit: " -NoNewline -ForegroundColor Gray
		Write-Host "Sin límite" -ForegroundColor Gray
	}
}

Write-Host "`n══════════════════════════════════════════════════════" -ForegroundColor Gray

Write-Host "`n[INSTRUCCIONES DETALLADAS]" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para cada columna:" -ForegroundColor Yellow
Write-Host "  1. Clic en '+ New column'" -ForegroundColor White
Write-Host "  2. Escribe el nombre (ejemplo: 'Tareas a Analizar')" -ForegroundColor White
Write-Host "  3. Selecciona el estado mapeado (ejemplo: 'New')" -ForegroundColor White
Write-Host "  4. Si tiene WIP Limit, márcalo y pon el número" -ForegroundColor White
Write-Host "  5. Clic en 'Save'" -ForegroundColor White
Write-Host "  6. Repite para la siguiente columna" -ForegroundColor White

Write-Host "`n[ELIMINAR COLUMNAS ANTIGUAS]" -ForegroundColor Cyan
Write-Host "  - Haz clic en '...' (tres puntos) en cada columna vieja" -ForegroundColor White
Write-Host "  - Selecciona 'Delete column'" -ForegroundColor White
Write-Host "  - Confirma la eliminación" -ForegroundColor White

Write-Host "`n[REORDENAR COLUMNAS]" -ForegroundColor Cyan
Write-Host "  - Arrastra y suelta para cambiar el orden" -ForegroundColor White

Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "¿Necesitas ayuda? Presiona ENTER para continuar" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan

Read-Host "`nPresiona ENTER cuando hayas terminado"

# Abrir el board para verificar
Write-Host "`nAbriendo el board para verificar..." -ForegroundColor Cyan
Start-Process "https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues"

Write-Host "`n✓ ¡Listo! Verifica que las columnas estén correctas." -ForegroundColor Green
