# ============================================================================
# Script: Configurar-Board-Automatico-v2.ps1
# Propósito: Configurar board usando Team Settings API
# ============================================================================

param(
	[Parameter(Mandatory=$true)]
	[string]$PersonalAccessToken
)

$ErrorActionPreference = "Stop"

$Organization = "VSCAD"
$Project = "tandem2026"
$Team = "tandem2026 Team"

# Autenticación
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PersonalAccessToken"))
$headers = @{
	Authorization = "Basic $base64AuthInfo"
	"Content-Type" = "application/json"
}

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "CONFIGURANDO BOARD AUTOMÁTICAMENTE" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# ──────────────────────────────────────────────────────────────────────────
# PASO 1: Obtener el ID del team
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[1/5] Obteniendo información del team..." -ForegroundColor Cyan

$teamUrl = "https://dev.azure.com/$Organization/_apis/projects/$Project/teams?api-version=7.0"

try {
	$teams = Invoke-RestMethod -Uri $teamUrl -Headers $headers -Method Get
	$targetTeam = $teams.value | Where-Object { $_.name -eq $Team }

	if (-not $targetTeam) {
		Write-Host "✗ Team no encontrado: $Team" -ForegroundColor Red
		exit 1
	}

	$teamId = $targetTeam.id
	Write-Host "✓ Team encontrado: $($targetTeam.name)" -ForegroundColor Green
	Write-Host "  ID: $teamId" -ForegroundColor Gray
} catch {
	Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# ──────────────────────────────────────────────────────────────────────────
# PASO 2: Obtener la configuración actual del board
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[2/5] Obteniendo configuración actual del board..." -ForegroundColor Cyan

$boardUrl = "https://dev.azure.com/$Organization/$Project/$teamId/_apis/work/boards/Issues?api-version=7.1-preview.1"

try {
	$currentBoard = Invoke-RestMethod -Uri $boardUrl -Headers $headers -Method Get
	Write-Host "✓ Board actual: $($currentBoard.name)" -ForegroundColor Green
	Write-Host "  Columnas actuales: $($currentBoard.columns.Count)" -ForegroundColor Gray

	foreach ($col in $currentBoard.columns) {
		Write-Host "    - $($col.name)" -ForegroundColor Gray
	}
} catch {
	Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# ──────────────────────────────────────────────────────────────────────────
# PASO 3: Definir nueva configuración de columnas
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[3/5] Preparando nueva configuración..." -ForegroundColor Cyan

$newColumns = @(
	@{
		id = [guid]::NewGuid().ToString()
		name = "Tareas a Analizar"
		itemLimit = 0
		stateMappings = @{
			"Issue" = "New"
		}
		columnType = "incoming"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Esperando documentación"
		itemLimit = 0
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Preparado para Realizar"
		itemLimit = 25
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Mal Testeado Volver a Realizar"
		itemLimit = 0
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Realizando"
		itemLimit = 0
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Preparando a testear"
		itemLimit = 100
		stateMappings = @{
			"Issue" = "Resolved"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Preparado para presentar"
		itemLimit = 0
		stateMappings = @{
			"Issue" = "Closed"
		}
		columnType = "outgoing"
	}
)

Write-Host "✓ $($newColumns.Count) columnas preparadas" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────
# PASO 4: Actualizar el board (método PUT completo)
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[4/5] Aplicando configuración..." -ForegroundColor Cyan

$updatePayload = @{
	columns = $newColumns
} | ConvertTo-Json -Depth 10

$updateUrl = "https://dev.azure.com/$Organization/$Project/$teamId/_apis/work/boards/Issues?api-version=7.1-preview.1"

try {
	Write-Host "  URL: $updateUrl" -ForegroundColor Gray
	Write-Host "  Método: PUT" -ForegroundColor Gray

	$response = Invoke-RestMethod -Uri $updateUrl -Headers $headers -Method Put -Body $updatePayload

	Write-Host "✓ Board actualizado correctamente" -ForegroundColor Green

} catch {
	Write-Host "✗ Error al actualizar: $($_.Exception.Message)" -ForegroundColor Red

	if ($_.ErrorDetails.Message) {
		$errorObj = $_.ErrorDetails.Message | ConvertFrom-Json
		Write-Host "`nDetalles del error:" -ForegroundColor Yellow
		Write-Host "  Mensaje: $($errorObj.message)" -ForegroundColor Red
	}

	Write-Host "`n⚠ La API puede no permitir cambios automáticos." -ForegroundColor Yellow
	Write-Host "  Usa el script manual: .\Configurar-Board-Manual.ps1" -ForegroundColor Cyan
	exit 1
}

# ──────────────────────────────────────────────────────────────────────────
# PASO 5: Verificar cambios
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n[5/5] Verificando cambios..." -ForegroundColor Cyan

try {
	$updatedBoard = Invoke-RestMethod -Uri $boardUrl -Headers $headers -Method Get

	Write-Host "✓ Columnas actualizadas:" -ForegroundColor Green
	foreach ($col in $updatedBoard.columns) {
		$limitText = if ($col.itemLimit -gt 0) { " (WIP: $($col.itemLimit))" } else { "" }
		Write-Host "  ✓ $($col.name)$limitText" -ForegroundColor Green
	}
} catch {
	Write-Host "⚠ No se pudo verificar. Revisa manualmente." -ForegroundColor Yellow
}

# ──────────────────────────────────────────────────────────────────────────
# RESUMEN
# ──────────────────────────────────────────────────────────────────────────

Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "CONFIGURACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

Write-Host "`nVerifica el board en:" -ForegroundColor Cyan
Start-Process "https://dev.azure.com/$Organization/$Project/_boards/board/t/$Team/Issues"

Write-Host "`n✓ Listo!" -ForegroundColor Green
