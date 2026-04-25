# Script automatico sin confirmacion - Crear columnas board
$pat = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "",$pat)))
$headers = @{
	Authorization = ("Basic {0}" -f $base64AuthInfo)
	"Content-Type" = "application/json"
}

$teamId = "2ea0799c-57e5-48f6-87dd-f9eb6c232196"
$boardUrl = "https://dev.azure.com/VSCAD/tandem2026/$teamId/_apis/work/boards/Issues?api-version=7.1-preview.1"

Write-Host "Creando columnas en el panel..." -ForegroundColor Cyan

$newColumns = @(
	@{
		name = "New"
		stateMappings = @{ "Issue" = "To Do" }
		columnType = "incoming"
		isSplit = $false
		wipLimit = 50
	},
	@{
		name = "Tareas a Analizar"
		stateMappings = @{ "Issue" = "To Do" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 10
	},
	@{
		name = "Esperando documentacion"
		stateMappings = @{ "Issue" = "To Do" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 10
	},
	@{
		name = "Preparado para Realizar"
		stateMappings = @{ "Issue" = "Doing" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 10
	},
	@{
		name = "Realizando"
		stateMappings = @{ "Issue" = "Doing" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 5
	},
	@{
		name = "Mal Testeo Volver a Realizar"
		stateMappings = @{ "Issue" = "Doing" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 5
	},
	@{
		name = "Preparando a testear"
		stateMappings = @{ "Issue" = "Doing" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 5
	},
	@{
		name = "Preparado para presentar"
		stateMappings = @{ "Issue" = "Done" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 10
	},
	@{
		name = "Closed"
		stateMappings = @{ "Issue" = "Done" }
		columnType = "outgoing"
		isSplit = $false
		wipLimit = 300
	}
)

$payload = @{
	columns = $newColumns
} | ConvertTo-Json -Depth 10

try {
	$result = Invoke-RestMethod -Uri $boardUrl -Method Put -Headers $headers -Body $payload
	Write-Host "Exito! Columnas creadas:" -ForegroundColor Green
	$result.columns | ForEach-Object {
		Write-Host "  - $($_.name) (WIP: $($_.wipLimit))" -ForegroundColor White
	}
} catch {
	Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
