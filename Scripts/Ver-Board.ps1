# Ver estado actual del board
$pat = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "",$pat)))
$headers = @{
	Authorization = ("Basic {0}" -f $base64AuthInfo)
	"Content-Type" = "application/json"
}

$teamId = "2ea0799c-57e5-48f6-87dd-f9eb6c232196"
$boardUrl = "https://dev.azure.com/VSCAD/tandem2026/$teamId/_apis/work/boards/Issues?api-version=7.1-preview.1"

Write-Host "Estado actual del board:" -ForegroundColor Cyan
$board = Invoke-RestMethod -Uri $boardUrl -Method Get -Headers $headers
$board.columns | ForEach-Object {
	Write-Host "  - $($_.name) | Tipo: $($_.columnType) | Estado: $($_.stateMappings.Issue)" -ForegroundColor White
}

Write-Host ""
Write-Host "Creando nuevas columnas..." -ForegroundColor Yellow

$json = @"
{
  "columns": [
	{
	  "id": "new-col-1",
	  "name": "New",
	  "itemLimit": 50,
	  "stateMappings": {"Issue": "To Do"},
	  "columnType": "incoming",
	  "description": ""
	},
	{
	  "id": "new-col-2",
	  "name": "Tareas a Analizar",
	  "itemLimit": 10,
	  "stateMappings": {"Issue": "To Do"},
	  "isSplit": false,
	  "description": "",
	  "columnType": "inProgress"
	},
	{
	  "id": "new-col-3",
	  "name": "Esperando documentacion",
	  "itemLimit": 10,
	  "stateMappings": {"Issue": "To Do"},
	  "isSplit": false,
	  "description": "",
	  "columnType": "inProgress"
	},
	{
	  "id": "new-col-4",
	  "name": "Preparado para Realizar",
	  "itemLimit": 10,
	  "stateMappings": {"Issue": "Doing"},
	  "isSplit": false,
	  "description": "",
	  "columnType": "inProgress"
	},
	{
	  "id": "new-col-5",
	  "name": "Realizando",
	  "itemLimit": 5,
	  "stateMappings": {"Issue": "Doing"},
	  "isSplit": false,
	  "description": "",
	  "columnType": "inProgress"
	},
	{
	  "id": "new-col-6",
	  "name": "Mal Testeo Volver a Realizar",
	  "itemLimit": 5,
	  "stateMappings": {"Issue": "Doing"},
	  "isSplit": false,
	  "description": "",
	  "columnType": "inProgress"
	},
	{
	  "id": "new-col-7",
	  "name": "Preparando a testear",
	  "itemLimit": 5,
	  "stateMappings": {"Issue": "Doing"},
	  "isSplit": false,
	  "description": "",
	  "columnType": "inProgress"
	},
	{
	  "id": "new-col-8",
	  "name": "Preparado para presentar",
	  "itemLimit": 10,
	  "stateMappings": {"Issue": "Done"},
	  "isSplit": false,
	  "description": "",
	  "columnType": "inProgress"
	},
	{
	  "id": "new-col-9",
	  "name": "Closed",
	  "itemLimit": 300,
	  "stateMappings": {"Issue": "Done"},
	  "columnType": "outgoing",
	  "description": ""
	}
  ]
}
"@

try {
	$result = Invoke-RestMethod -Uri $boardUrl -Method Put -Headers $headers -Body $json
	Write-Host ""
	Write-Host "Exito! Columnas creadas:" -ForegroundColor Green
	$result.columns | ForEach-Object {
		Write-Host "  - $($_.name)" -ForegroundColor White
	}
} catch {
	Write-Host ""
	Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
