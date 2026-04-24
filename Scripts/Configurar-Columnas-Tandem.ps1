# Script para configurar columnas personalizadas en Azure DevOps Board
# Proyecto: tandem2026
# Team: tandem2026 Team
# Board: Issues

# IMPORTANTE: Actualizar el PAT antes de ejecutar
$pat = "TU_PAT_AQUI"  # Reemplazar con tu Personal Access Token

# Configuracion
$organization = "VSCAD"
$project = "tandem2026"
$team = "tandem2026 Team"
$boardName = "Issues"

# Headers para autenticacion
$token = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
$headers = @{
	Authorization = "Basic $token"
	"Content-Type" = "application/json"
}

# URL del board
$teamEncoded = [System.Web.HttpUtility]::UrlEncode($team)
$boardUrl = "https://dev.azure.com/$organization/$project/$teamEncoded/_apis/work/boards/$boardName`?api-version=7.1-preview.1"

Write-Host "=== Configurando Columnas del Board ===" -ForegroundColor Cyan
Write-Host "Proyecto: $project" -ForegroundColor Yellow
Write-Host "Team: $team" -ForegroundColor Yellow
Write-Host "Board: $boardName" -ForegroundColor Yellow
Write-Host ""

# Obtener configuracion actual
Write-Host "Obteniendo configuracion actual..." -ForegroundColor Green
try {
	$currentBoard = Invoke-RestMethod -Uri $boardUrl -Headers $headers -Method Get
	Write-Host "Configuracion obtenida exitosamente" -ForegroundColor Green
	Write-Host ""
	Write-Host "Columnas actuales:" -ForegroundColor Yellow
	$currentBoard.columns | ForEach-Object {
		Write-Host "  - $($_.name)" -ForegroundColor White
	}
	Write-Host ""
}
catch {
	Write-Host "Error al obtener configuracion: $($_.Exception.Message)" -ForegroundColor Red
	Write-Host "Verifica que el PAT sea valido y tenga permisos de 'Work Items (Read & Write)'" -ForegroundColor Yellow
	exit 1
}

# Definir nuevas columnas
$newColumns = @(
	@{
		name = "New"
		stateMappings = @{
			"Issue" = "New"
		}
		columnType = "incoming"
		isSplit = $false
		description = "Nuevas tareas"
		wipLimit = 50
	},
	@{
		name = "Tareas a Analizar"
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
		isSplit = $false
		description = "Tareas pendientes de analisis"
		wipLimit = 10
	},
	@{
		name = "Esperando documentacion"
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
		isSplit = $false
		description = "Tareas esperando documentacion"
		wipLimit = 10
	},
	@{
		name = "Preparado para Realizar"
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
		isSplit = $false
		description = "Listo para comenzar desarrollo"
		wipLimit = 10
	},
	@{
		name = "Realizando"
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
		isSplit = $false
		description = "En desarrollo activo"
		wipLimit = 5
	},
	@{
		name = "Mal Testeo Volver a Realizar"
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
		isSplit = $false
		description = "Requiere correccion despues de testing"
		wipLimit = 5
	},
	@{
		name = "Preparando a testear"
		stateMappings = @{
			"Issue" = "Active"
		}
		columnType = "inProgress"
		isSplit = $false
		description = "Preparando para pruebas"
		wipLimit = 5
	},
	@{
		name = "Preparado para presentar"
		stateMappings = @{
			"Issue" = "Resolved"
		}
		columnType = "inProgress"
		isSplit = $false
		description = "Listo para presentar"
		wipLimit = 10
	},
	@{
		name = "Closed"
		stateMappings = @{
			"Issue" = "Closed"
		}
		columnType = "outgoing"
		isSplit = $false
		description = "Tareas completadas"
		wipLimit = 300
	}
)

# Crear payload para actualizacion
$payload = @{
	columns = $newColumns
} | ConvertTo-Json -Depth 10

Write-Host "=== Nueva Configuracion de Columnas ===" -ForegroundColor Cyan
$newColumns | ForEach-Object {
	Write-Host "  - $($_.name) (WIP: $($_.wipLimit))" -ForegroundColor White
}
Write-Host ""

# Confirmar cambios
$confirmation = Read-Host "Deseas aplicar estos cambios? (S/N)"
if ($confirmation -ne "S" -and $confirmation -ne "s") {
	Write-Host "Operacion cancelada" -ForegroundColor Yellow
	exit 0
}

# Aplicar cambios
Write-Host ""
Write-Host "Aplicando cambios..." -ForegroundColor Green
try {
	$result = Invoke-RestMethod -Uri $boardUrl -Headers $headers -Method Put -Body $payload
	Write-Host "Columnas configuradas exitosamente!" -ForegroundColor Green
	Write-Host ""
	Write-Host "Columnas actualizadas:" -ForegroundColor Yellow
	$result.columns | ForEach-Object {
		Write-Host "  - $($_.name)" -ForegroundColor White
	}
	Write-Host ""
	Write-Host "Abre el board para ver los cambios:" -ForegroundColor Cyan
	Write-Host "https://dev.azure.com/$organization/$project/_boards/board/t/$team/Issues" -ForegroundColor Cyan
}
catch {
	Write-Host "Error al actualizar columnas: $($_.Exception.Message)" -ForegroundColor Red
	Write-Host ""
	Write-Host "Detalles del error:" -ForegroundColor Yellow
	Write-Host $_.Exception.Response.StatusCode -ForegroundColor Red

	if ($_.Exception.Response) {
		$reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
		$responseBody = $reader.ReadToEnd()
		Write-Host $responseBody -ForegroundColor Red
	}

	Write-Host ""
	Write-Host "Posibles soluciones:" -ForegroundColor Yellow
	Write-Host "1. Verifica que el PAT sea valido y no haya expirado" -ForegroundColor White
	Write-Host "2. Asegurate de que el PAT tenga permisos de 'Work Items (Read & Write)'" -ForegroundColor White
	Write-Host "3. Configura las columnas manualmente en:" -ForegroundColor White
	Write-Host "   https://dev.azure.com/$organization/$project/_settings/board-team" -ForegroundColor Cyan
	exit 1
}
