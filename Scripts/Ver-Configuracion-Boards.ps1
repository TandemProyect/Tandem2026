# Script: Ver-Configuracion-Boards.ps1
param([Parameter(Mandatory=$true)][string]$PAT)

$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"}
$Org = "VSCAD"

Write-Host "`n=== COMPARANDO CONFIGURACIONES ===" -ForegroundColor Cyan

# Board Athos_2025
Write-Host "`n[ATHOS_2025]" -ForegroundColor Yellow
$url1 = "https://dev.azure.com/$Org/Athos_2025/Athos_2025%20Team/_apis/work/boards/Stories?api-version=7.1-preview.1"
$board1 = Invoke-RestMethod -Uri $url1 -Headers $headers
Write-Host "Process: $($board1.process)" -ForegroundColor Gray
foreach ($col in $board1.columns) {
	Write-Host "  - $($col.name) | Estado: $($col.stateMappings.PSObject.Properties.Value) | WIP: $($col.itemLimit)" -ForegroundColor White
}

# Board tandem2026
Write-Host "`n[TANDEM2026]" -ForegroundColor Yellow
$url2 = "https://dev.azure.com/$Org/tandem2026/tandem2026%20Team/_apis/work/boards/Issues?api-version=7.1-preview.1"
$board2 = Invoke-RestMethod -Uri $url2 -Headers $headers
Write-Host "Process: $($board2.process)" -ForegroundColor Gray
foreach ($col in $board2.columns) {
	Write-Host "  - $($col.name) | Estado: $($col.stateMappings.PSObject.Properties.Value) | WIP: $($col.itemLimit)" -ForegroundColor White
}

Write-Host "`n=== PROCESOS DE LOS PROYECTOS ===" -ForegroundColor Cyan
$proj1 = Invoke-RestMethod -Uri "https://dev.azure.com/$Org/_apis/projects/Athos_2025?api-version=7.0" -Headers $headers
$proj2 = Invoke-RestMethod -Uri "https://dev.azure.com/$Org/_apis/projects/tandem2026?api-version=7.0" -Headers $headers

Write-Host "`nAthos_2025 usa: $($proj1.capabilities.processTemplate.templateName)" -ForegroundColor Cyan
Write-Host "tandem2026 usa: $($proj2.capabilities.processTemplate.templateName)" -ForegroundColor Cyan

if ($proj1.capabilities.processTemplate.templateName -ne $proj2.capabilities.processTemplate.templateName) {
	Write-Host "`n⚠ PROBLEMA DETECTADO:" -ForegroundColor Red
	Write-Host "  Los proyectos usan procesos diferentes." -ForegroundColor Yellow
	Write-Host "  Para copiar la configuracion exacta, ambos deben usar el mismo proceso." -ForegroundColor Yellow
	Write-Host "`nSOLUCION:" -ForegroundColor Cyan
	Write-Host "  1. Ve a: https://dev.azure.com/$Org/tandem2026/_settings/settings" -ForegroundColor White
	Write-Host "  2. Haz clic en 'Overview'" -ForegroundColor White
	Write-Host "  3. En 'Process', haz clic en 'Change'" -ForegroundColor White
	Write-Host "  4. Selecciona: $($proj1.capabilities.processTemplate.templateName)" -ForegroundColor White
	Write-Host "  5. Guarda los cambios" -ForegroundColor White
	Write-Host "`nDespues de cambiar el proceso, las columnas se copian automaticamente." -ForegroundColor Green
} else {
	Write-Host "`nOK - Ambos usan el mismo proceso" -ForegroundColor Green
}
