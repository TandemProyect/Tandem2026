# Script: Create-StandardTasks.ps1
# Crea las 3 Tasks estándar (Develop, Test, CR) para una User Story

param(
	[Parameter(Mandatory=$true, Position=0)]
	[int]$US_ID,

	[Parameter(Mandatory=$false)]
	[string]$Feature = "funcionalidad"
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOot1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

Write-Host "`n=== Creando Tasks estándar para US #$US_ID ===" -ForegroundColor Cyan
Write-Host ""

# Tasks a crear
$tasks = @(
	@{Title = "Develop - Implementar $Feature"; Type = "Develop"},
	@{Title = "Test - Probar $Feature"; Type = "Test"},
	@{Title = "CR - Code Review $Feature"; Type = "CR"}
)

$createdTasks = @()

foreach ($task in $tasks) {
	$payload = @(
		@{op = "add"; path = "/fields/System.Title"; value = $task.Title},
		@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
		@{op = "add"; path = "/relations/-"; value = @{
			rel = "System.LinkTypes.Hierarchy-Reverse"
			url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$US_ID"
		}}
	) | ConvertTo-Json -Depth 10

	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload

	$createdTasks += @{Id = $result.id; Title = $task.Title; Type = $task.Type}
	Write-Host "✓ Task #$($result.id) creada - $($task.Type)" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Resumen ===" -ForegroundColor Cyan
Write-Host "US #$US_ID ahora tiene 3 Tasks:" -ForegroundColor Yellow
foreach ($task in $createdTasks) {
	Write-Host "  - Task #$($task.Id): $($task.Type)" -ForegroundColor White
}

Write-Host "`n✓ Todas las Tasks creadas exitosamente" -ForegroundColor Green
Write-Host "Abre Azure DevOps para verlas: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/$US_ID`n" -ForegroundColor Cyan

return $createdTasks
