# Script para obtener estados válidos del work item 619
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"}

# Obtener el work item completo
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/619?`$expand=all&api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Get

Write-Host "Tipo de work item: $($result.fields.'System.WorkItemType')" -ForegroundColor Cyan
Write-Host "Estado actual: $($result.fields.'System.State')" -ForegroundColor Yellow
Write-Host "Razón: $($result.fields.'System.Reason')" -ForegroundColor White

# Intentar obtener transiciones permitidas
$url2 = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitemtypes/Issue/states?api-version=7.0"
try {
	$states = Invoke-RestMethod -Uri $url2 -Headers $headers -Method Get
	Write-Host "`nEstados posibles:" -ForegroundColor Green
	$states.value | ForEach-Object { Write-Host "  - $($_.name)" }
} catch {
	Write-Host "No se pudieron obtener los estados posibles" -ForegroundColor Red
}
