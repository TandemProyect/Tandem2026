# ============================================================================
# Script: Configurar-Board-Tandem2026.ps1
# Propósito: Copiar la estructura del board de Athos_2025 a tandem2026
# ============================================================================

param(
	[string]$Organization = "VSCAD",
	[string]$SourceProject = "Athos_2025",
	[string]$TargetProject = "tandem2026",
	[string]$SourceTeam = "Athos_2025 Team",
	[string]$TargetTeam = "tandem2026 Team",
	[Parameter(Mandatory=$true)]
	[string]$PersonalAccessToken
)

$ErrorActionPreference = "Stop"

# Convertir PAT a Base64 para autenticación
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PersonalAccessToken"))
$headers = @{
	Authorization = "Basic $base64AuthInfo"
	"Content-Type" = "application/json"
}

$baseUrl = "https://dev.azure.com/$Organization"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "CONFIGURANDO BOARD: $TargetProject" -ForegroundColor Yellow
Write-Host "Copiando estructura de: $SourceProject" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# ──────────────────────────────────────────────────────────────────────────
# PASO 1: Obtener configuración del board de origen (Athos_2025)
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[1/4] Obteniendo configuración del board de origen..." -ForegroundColor Cyan

$sourceBoardUrl = "$baseUrl/$SourceProject/$SourceTeam/_apis/work/boards/Stories?api-version=7.0"

try {
	$sourceBoard = Invoke-RestMethod -Uri $sourceBoardUrl -Headers $headers -Method Get
	Write-Host "✓ Board de origen obtenido: $($sourceBoard.name)" -ForegroundColor Green
	Write-Host "  Columnas: $($sourceBoard.columns.Count)" -ForegroundColor Gray
} catch {
	Write-Host "✗ Error al obtener board de origen: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# ──────────────────────────────────────────────────────────────────────────
# PASO 2: Obtener el board de destino (tandem2026)
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[2/4] Obteniendo board de destino..." -ForegroundColor Cyan

$targetBoardUrl = "$baseUrl/$TargetProject/$TargetTeam/_apis/work/boards/Issues?api-version=7.0"

try {
	$targetBoard = Invoke-RestMethod -Uri $targetBoardUrl -Headers $headers -Method Get
	Write-Host "✓ Board de destino obtenido: $($targetBoard.name)" -ForegroundColor Green
} catch {
	Write-Host "✗ Error al obtener board de destino: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# ──────────────────────────────────────────────────────────────────────────
# PASO 3: Mapear columnas de Stories a Issues
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[3/4] Preparando nueva configuración de columnas..." -ForegroundColor Cyan

# Estructura de columnas observada en Athos_2025:
$newColumns = @(
	@{
		name = "Tareas a Analizar"
		stateMappings = @{
			"Issue" = "New"
		}
		isSplit = $false
		description = "Nuevas tareas pendientes de análisis"
		columnType = "incoming"
	},
	@{
		name = "Esperando documentación"
		stateMappings = @{
			"Issue" = "Active"
		}
		isSplit = $false
		description = "Tareas bloqueadas esperando documentación"
		columnType = "inProgress"
	},
	@{
		name = "Preparado para Realizar"
		stateMappings = @{
			"Issue" = "Active"
		}
		isSplit = $false
		description = "Tareas listas para comenzar"
		columnType = "inProgress"
		itemLimit = 25
	},
	@{
		name = "Mal Testeado Volver a Realizar"
		stateMappings = @{
			"Issue" = "Active"
		}
		isSplit = $false
		description = "Tareas que fallaron testing y deben rehacerse"
		columnType = "inProgress"
	},
	@{
		name = "Realizando"
		stateMappings = @{
			"Issue" = "Active"
		}
		isSplit = $false
		description = "Tareas en desarrollo activo"
		columnType = "inProgress"
	},
	@{
		name = "Preparando a testear"
		stateMappings = @{
			"Issue" = "Resolved"
		}
		isSplit = $false
		description = "Desarrollo completado, pendiente de testing"
		columnType = "inProgress"
		itemLimit = 100
	},
	@{
		name = "Preparado para presentar"
		stateMappings = @{
			"Issue" = "Closed"
		}
		isSplit = $false
		description = "Tareas completadas y testeadas"
		columnType = "outgoing"
	}
)

Write-Host "✓ $($newColumns.Count) columnas preparadas" -ForegroundColor Green

foreach ($col in $newColumns) {
	$limitText = if ($col.itemLimit) { " (límite: $($col.itemLimit))" } else { "" }
	Write-Host "  - $($col.name)$limitText" -ForegroundColor Gray
}

# ──────────────────────────────────────────────────────────────────────────
# PASO 4: Actualizar el board de destino
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[4/4] Aplicando nueva configuración al board..." -ForegroundColor Cyan

$updatePayload = @{
	columns = $newColumns
} | ConvertTo-Json -Depth 10

try {
	$updateUrl = "$baseUrl/$TargetProject/$TargetTeam/_apis/work/boards/Issues/columns?api-version=7.0"

	# Azure DevOps requiere PATCH para actualizar columnas
	$response = Invoke-RestMethod -Uri $updateUrl -Headers $headers -Method Patch -Body $updatePayload

	Write-Host "✓ Board actualizado correctamente" -ForegroundColor Green
	Write-Host "`nNuevas columnas configuradas:" -ForegroundColor Cyan

	foreach ($col in $response.columns) {
		Write-Host "  ✓ $($col.name)" -ForegroundColor Green
	}

} catch {
	Write-Host "✗ Error al actualizar board: $($_.Exception.Message)" -ForegroundColor Red
	Write-Host "`nDetalles del error:" -ForegroundColor Yellow
	Write-Host $_.Exception.Response.StatusCode -ForegroundColor Red
	Write-Host $_.ErrorDetails.Message -ForegroundColor Red
	exit 1
}

# ──────────────────────────────────────────────────────────────────────────
# RESUMEN
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "CONFIGURACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

Write-Host "`nVerifica el board en:" -ForegroundColor Cyan
Write-Host "  https://dev.azure.com/$Organization/$TargetProject/_boards/board/t/$TargetTeam/Issues" -ForegroundColor White

Write-Host "`n⚠ NOTA: Los work items existentes se moverán automáticamente" -ForegroundColor Yellow
Write-Host "   a las columnas según su estado actual." -ForegroundColor Yellow
