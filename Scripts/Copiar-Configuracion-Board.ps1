# ============================================================================
# Script: Copiar-Configuracion-Board.ps1
# Propósito: Copiar configuración completa del board de Athos_2025 a tandem2026
# ============================================================================

param(
	[Parameter(Mandatory=$true)]
	[string]$PersonalAccessToken
)

$ErrorActionPreference = "Stop"

$Organization = "VSCAD"
$SourceProject = "Athos_2025"
$SourceTeam = "Athos_2025 Team"
$TargetProject = "tandem2026"
$TargetTeam = "tandem2026 Team"

# Autenticación
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PersonalAccessToken"))
$headers = @{
	Authorization = "Basic $base64AuthInfo"
	"Content-Type" = "application/json"
}

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "COPIANDO CONFIGURACION DE BOARD" -ForegroundColor Yellow
Write-Host "Origen: $SourceProject" -ForegroundColor White
Write-Host "Destino: $TargetProject" -ForegroundColor White
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# ──────────────────────────────────────────────────────────────────────────
# PASO 1: Obtener configuración del board de origen
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[1/3] Obteniendo configuración de $SourceProject..." -ForegroundColor Cyan

$sourceUrl = "https://dev.azure.com/$Organization/$SourceProject/$SourceTeam/_apis/work/boards/Stories?api-version=7.1-preview.1"

try {
	$sourceBoard = Invoke-RestMethod -Uri $sourceUrl -Headers $headers -Method Get

	Write-Host "✓ Configuración obtenida:" -ForegroundColor Green
	Write-Host "  Board: $($sourceBoard.name)" -ForegroundColor Gray
	Write-Host "  Columnas: $($sourceBoard.columns.Count)" -ForegroundColor Gray

	Write-Host "`n  Columnas encontradas:" -ForegroundColor Cyan
	foreach ($col in $sourceBoard.columns) {
		$wipText = if ($col.itemLimit -gt 0) { " [WIP: $($col.itemLimit)]" } else { "" }
		Write-Host "    - $($col.name)$wipText" -ForegroundColor White
	}

} catch {
	Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# ──────────────────────────────────────────────────────────────────────────
# PASO 2: Adaptar configuración para Issues (en lugar de Stories)
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[2/3] Adaptando configuración para work items tipo 'Issue'..." -ForegroundColor Cyan

$adaptedColumns = @()

foreach ($col in $sourceBoard.columns) {
	# Mapear estados de "User Story" a "Issue"
	$newStateMappings = @{}

	foreach ($key in $col.stateMappings.PSObject.Properties.Name) {
		$state = $col.stateMappings.$key

		# Convertir estados de User Story a Issue
		$issueState = switch ($state) {
			"New" { "New" }
			"Active" { "Active" }
			"Resolved" { "Resolved" }
			"Closed" { "Closed" }
			default { "Active" } # Por defecto mapear a Active
		}

		$newStateMappings["Issue"] = $issueState
	}

	$adaptedCol = @{
		id = $col.id
		name = $col.name
		itemLimit = $col.itemLimit
		stateMappings = $newStateMappings
		columnType = $col.columnType
		isSplit = $col.isSplit
		description = $col.description
	}

	$adaptedColumns += $adaptedCol
}

Write-Host "✓ $($adaptedColumns.Count) columnas adaptadas" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────
# PASO 3: Aplicar configuración al board de destino
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[3/3] Aplicando configuración a $TargetProject..." -ForegroundColor Cyan

$targetUrl = "https://dev.azure.com/$Organization/$TargetProject/$TargetTeam/_apis/work/boards/Issues?api-version=7.1-preview.1"

$payload = @{
	columns = $adaptedColumns
} | ConvertTo-Json -Depth 10

try {
	# Intentar actualizar con PUT
	$result = Invoke-RestMethod -Uri $targetUrl -Headers $headers -Method Put -Body $payload

	Write-Host "✓ Configuracion aplicada correctamente" -ForegroundColor Green
	Write-Host "`n  Nuevas columnas en $TargetProject`:" -ForegroundColor Cyan

	foreach ($col in $result.columns) {
		$wipText = if ($col.itemLimit -gt 0) { " [WIP: $($col.itemLimit)]" } else { "" }
		Write-Host "    ✓ $($col.name)$wipText" -ForegroundColor Green
	}

} catch {
	Write-Host "✗ Error al aplicar configuracion:" -ForegroundColor Red
	Write-Host "  $($_.Exception.Message)" -ForegroundColor Red

	if ($_.ErrorDetails.Message) {
		Write-Host "`n  Detalles:" -ForegroundColor Yellow
		Write-Host "  $($_.ErrorDetails.Message)" -ForegroundColor Red
	}

	Write-Host "`n⚠ Razones posibles del fallo:" -ForegroundColor Yellow
	Write-Host "  1. Azure DevOps puede requerir configuracion manual" -ForegroundColor Gray
	Write-Host "  2. Los estados de 'Issue' y 'User Story' son incompatibles" -ForegroundColor Gray
	Write-Host "  3. El proceso del proyecto no permite cambios automaticos" -ForegroundColor Gray

	Write-Host "`n💡 Solucion alternativa:" -ForegroundColor Cyan
	Write-Host "  Cambia el proceso del proyecto a uno personalizado" -ForegroundColor White
	Write-Host "  desde Project Settings → Overview → Process" -ForegroundColor White

	exit 1
}

# ──────────────────────────────────────────────────────────────────────────
# VERIFICACION
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "CONFIGURACION COMPLETADA" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

Write-Host "`n✓ Board configurado exitosamente" -ForegroundColor Green
Write-Host "`nVerifica el resultado en:" -ForegroundColor Cyan
Start-Process "https://dev.azure.com/$Organization/$TargetProject/_boards/board/t/$TargetTeam/Issues"

Write-Host "`n⚠ Los work items existentes se reorganizaran automaticamente" -ForegroundColor Yellow
Write-Host "  segun su estado actual." -ForegroundColor Gray
