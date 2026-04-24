# Script: Copiar-Board-Simple.ps1
param([Parameter(Mandatory=$true)][string]$PAT)

$ErrorActionPreference = "Stop"
$Org = "VSCAD"
$SourceProj = "Athos_2025"
$SourceTeam = "Athos_2025 Team"
$TargetProj = "tandem2026"
$TargetTeam = "tandem2026 Team"

$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json"}

Write-Host "`n=== COPIANDO CONFIGURACION DE BOARD ===" -ForegroundColor Cyan
Write-Host "Origen: $SourceProj" -ForegroundColor White
Write-Host "Destino: $TargetProj" -ForegroundColor White

# Obtener board de origen
Write-Host "`n[1/3] Obteniendo configuracion de origen..." -ForegroundColor Cyan
$sourceUrl = "https://dev.azure.com/$Org/$SourceProj/$SourceTeam/_apis/work/boards/Stories?api-version=7.1-preview.1"
$sourceBoard = Invoke-RestMethod -Uri $sourceUrl -Headers $headers -Method Get

Write-Host "OK - Board: $($sourceBoard.name)" -ForegroundColor Green
Write-Host "     Columnas: $($sourceBoard.columns.Count)" -ForegroundColor Gray

foreach ($col in $sourceBoard.columns) {
	$wip = if ($col.itemLimit -gt 0) { " [WIP: $($col.itemLimit)]" } else { "" }
	Write-Host "     - $($col.name)$wip" -ForegroundColor Gray
}

# Adaptar columnas para Issues
Write-Host "`n[2/3] Adaptando para work items tipo 'Issue'..." -ForegroundColor Cyan
$adaptedColumns = @()

foreach ($col in $sourceBoard.columns) {
	$newCol = @{
		id = $col.id
		name = $col.name
		itemLimit = $col.itemLimit
		stateMappings = @{ "Issue" = "Active" }
		columnType = $col.columnType
		isSplit = $col.isSplit
		description = $col.description
	}

	# Mapear estados correctamente
	foreach ($key in $col.stateMappings.PSObject.Properties.Name) {
		$state = $col.stateMappings.$key
		$newCol.stateMappings["Issue"] = $state
		break
	}

	$adaptedColumns += $newCol
}

Write-Host "OK - $($adaptedColumns.Count) columnas adaptadas" -ForegroundColor Green

# Aplicar al board destino
Write-Host "`n[3/3] Aplicando al board de destino..." -ForegroundColor Cyan
$targetUrl = "https://dev.azure.com/$Org/$TargetProj/$TargetTeam/_apis/work/boards/Issues?api-version=7.1-preview.1"
$payload = @{ columns = $adaptedColumns } | ConvertTo-Json -Depth 10

try {
	$result = Invoke-RestMethod -Uri $targetUrl -Headers $headers -Method Put -Body $payload

	Write-Host "OK - Configuracion aplicada" -ForegroundColor Green
	Write-Host "`nNuevas columnas:" -ForegroundColor Cyan

	foreach ($col in $result.columns) {
		$wip = if ($col.itemLimit -gt 0) { " [WIP: $($col.itemLimit)]" } else { "" }
		Write-Host "  OK $($col.name)$wip" -ForegroundColor Green
	}

	Write-Host "`n=== COMPLETADO ===" -ForegroundColor Green
	Start-Process "https://dev.azure.com/$Org/$TargetProj/_boards/board/t/$TargetTeam/Issues"

} catch {
	Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
	if ($_.ErrorDetails.Message) {
		Write-Host "Detalles: $($_.ErrorDetails.Message)" -ForegroundColor Red
	}
	exit 1
}
