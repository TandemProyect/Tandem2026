# Crear columnas preservando estructura existente
$pat = $env:AZDO_PAT
if (-not $pat) { throw "AZDO_PAT no configurado en variables de entorno." }
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "",$pat)))
$headers = @{
	Authorization = ("Basic {0}" -f $base64AuthInfo)
	"Content-Type" = "application/json"
}

$teamId = "2ea0799c-57e5-48f6-87dd-f9eb6c232196"
$boardUrl = "https://dev.azure.com/VSCAD/tandem2026/$teamId/_apis/work/boards/Issues?api-version=7.1-preview.1"

Write-Host "Obteniendo board actual..." -ForegroundColor Cyan
$currentBoard = Invoke-RestMethod -Uri $boardUrl -Method Get -Headers $headers

Write-Host "Board obtenido. ID: $($currentBoard.id)" -ForegroundColor Green
Write-Host "Revision: $($currentBoard.revision)" -ForegroundColor Green

# Construir payload basado en estructura actual
$newColumns = @(
	@{
		id = [guid]::NewGuid().ToString()
		name = "New"
		itemLimit = 50
		stateMappings = @{
			"Issue" = "To Do"
		}
		columnType = "incoming"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Tareas a Analizar"
		itemLimit = 10
		stateMappings = @{
			"Issue" = "To Do"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Esperando documentacion"
		itemLimit = 10
		stateMappings = @{
			"Issue" = "To Do"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Preparado para Realizar"
		itemLimit = 10
		stateMappings = @{
			"Issue" = "Doing"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Realizando"
		itemLimit = 5
		stateMappings = @{
			"Issue" = "Doing"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Mal Testeo Volver a Realizar"
		itemLimit = 5
		stateMappings = @{
			"Issue" = "Doing"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Preparando a testear"
		itemLimit = 5
		stateMappings = @{
			"Issue" = "Doing"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Preparado para presentar"
		itemLimit = 10
		stateMappings = @{
			"Issue" = "Done"
		}
		columnType = "inProgress"
	},
	@{
		id = [guid]::NewGuid().ToString()
		name = "Closed"
		itemLimit = 300
		stateMappings = @{
			"Issue" = "Done"
		}
		columnType = "outgoing"
	}
)

$payload = @{
	columns = $newColumns
} | ConvertTo-Json -Depth 10

Write-Host "Aplicando cambios..." -ForegroundColor Yellow
Write-Host "Payload:" -ForegroundColor Gray
Write-Host $payload -ForegroundColor DarkGray

try {
	$result = Invoke-RestMethod -Uri $boardUrl -Method Put -Headers $headers -Body $payload
	Write-Host "`nExito! Columnas creadas:" -ForegroundColor Green
	$result.columns | ForEach-Object {
		Write-Host "  - $($_.name)" -ForegroundColor White
	}
} catch {
	Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
	if ($_.ErrorDetails) {
		Write-Host "Detalles: $($_.ErrorDetails.Message)" -ForegroundColor Red
	}
}
