# Script: Reconfigurar-Board-Completo.ps1
param([Parameter(Mandatory=$true)][string]$PAT)

$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json"}
$Org = "VSCAD"
$Proj = "tandem2026"
$Team = "tandem2026 Team"

Write-Host "`n=== RECONFIGURANDO BOARD ===" -ForegroundColor Cyan

# Configuracion deseada (copiada de Athos_2025)
$newColumns = @(
	@{ name = "Tareas a Analizar"; stateMappings = @{Issue = "To Do"}; itemLimit = 0; columnType = "incoming" },
	@{ name = "Esperando documentacion"; stateMappings = @{Issue = "Doing"}; itemLimit = 5; columnType = "inProgress" },
	@{ name = "Preparado para Realizar"; stateMappings = @{Issue = "To Do"}; itemLimit = 25; columnType = "inProgress" },
	@{ name = "Mal Testeo Volver a Realizar"; stateMappings = @{Issue = "Doing"}; itemLimit = 5; columnType = "inProgress" },
	@{ name = "Realizando"; stateMappings = @{Issue = "Doing"}; itemLimit = 5; columnType = "inProgress" },
	@{ name = "Preparando a testear"; stateMappings = @{Issue = "Done"}; itemLimit = 100; columnType = "inProgress" },
	@{ name = "Preparado para presentar"; stateMappings = @{Issue = "Done"}; itemLimit = 0; columnType = "outgoing" }
)

# Obtener board actual
$boardUrl = "https://dev.azure.com/$Org/$Proj/$Team/_apis/work/boards/Issues?api-version=7.1-preview.1"
$board = Invoke-RestMethod -Uri $boardUrl -Headers $headers

Write-Host "Board actual: $($board.name)" -ForegroundColor White
Write-Host "Columnas actuales: $($board.columns.Count)" -ForegroundColor Gray

# Generar IDs para nuevas columnas
$columnsWithIds = @()
foreach ($col in $newColumns) {
	$columnsWithIds += @{
		id = [guid]::NewGuid().ToString()
		name = $col.name
		itemLimit = $col.itemLimit
		stateMappings = $col.stateMappings
		columnType = $col.columnType
		isSplit = $false
		description = ""
	}
}

# Actualizar board
Write-Host "`nAplicando nueva configuracion..." -ForegroundColor Cyan
$payload = @{ columns = $columnsWithIds } | ConvertTo-Json -Depth 10

try {
	$result = Invoke-RestMethod -Uri $boardUrl -Headers $headers -Method Put -Body $payload

	Write-Host "OK - Board reconfigurado" -ForegroundColor Green
	Write-Host "`nNuevas columnas:" -ForegroundColor Cyan
	foreach ($col in $result.columns) {
		$wip = if ($col.itemLimit -gt 0) { " [WIP: $($col.itemLimit)]" } else { "" }
		Write-Host "  $($col.name)$wip" -ForegroundColor White
	}

	Write-Host "`n=== COMPLETADO ===" -ForegroundColor Green
	Start-Process "https://dev.azure.com/$Org/$Proj/_boards/board/t/$Team/Issues"

} catch {
	Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
	if ($_.ErrorDetails.Message) {
		Write-Host "$($_.ErrorDetails.Message)" -ForegroundColor Yellow
	}

	Write-Host "`n💡 Si el error persiste, hazlo manualmente:" -ForegroundColor Cyan
	Write-Host "https://dev.azure.com/$Org/$Proj/_settings/board-team" -ForegroundColor White
	exit 1
}
