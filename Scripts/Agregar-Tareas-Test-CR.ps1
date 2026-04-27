# Agregar tareas de Test y CR a US-637 y US-638
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

# US-637 (ya completada, tareas en Done)
Write-Host "`n=== US-637 ===" -ForegroundColor Cyan
$tasks637 = @(
	@{ParentID = 637; Titulo = "Realizar pruebas de integracion y validacion"; Descripcion = "Ejecutar suite completa de tests para validar deteccion de esquinas"; Estado = "Done"},
	@{ParentID = 637; Titulo = "Code Review del detector de esquinas"; Descripcion = "Revisar codigo de LCornerDetector, DTOs y endpoint MVC"; Estado = "Done"}
)

foreach ($task in $tasks637) {
	$payload = @(
		@{op = "add"; path = "/fields/System.Title"; value = $task.Titulo},
		@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
		@{op = "add"; path = "/fields/System.Description"; value = $task.Descripcion},
		@{op = "add"; path = "/relations/-"; value = @{
			rel = "System.LinkTypes.Hierarchy-Reverse"
			url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$($task.ParentID)"
		}}
	)

	$body = $payload | ConvertTo-Json -Depth 10
	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"

	try {
		$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body
		$taskId = $result.id
		Write-Host "Tarea creada #$taskId" -ForegroundColor Yellow

		# Marcar como Done si ya está completada (US-637)
		if ($task.Estado -eq "Done") {
			Start-Sleep -Milliseconds 300
			$updateBody = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
			$updateUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$taskId?api-version=7.0"
			Invoke-RestMethod -Uri $updateUrl -Headers $headers -Method Patch -Body $updateBody | Out-Null
			Write-Host "OK Task #$taskId (Done): $($task.Titulo)" -ForegroundColor Green
		}
	} catch {
		Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
	}
}

# US-638 (nueva, tareas en To Do)
Write-Host "`n=== US-638 ===" -ForegroundColor Cyan
$tasks638 = @(
	@{ParentID = 638; Titulo = "Realizar pruebas con casos de prueba ATK60"; Descripcion = "Ejecutar suite de tests para validar deteccion de puntos ATK60"},
	@{ParentID = 638; Titulo = "Code Review de implementacion ATK60"; Descripcion = "Revisar codigo de calculo de puntos, validaciones y exportacion"}
)

foreach ($task in $tasks638) {
	$payload = @(
		@{op = "add"; path = "/fields/System.Title"; value = $task.Titulo},
		@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
		@{op = "add"; path = "/fields/System.Description"; value = $task.Descripcion},
		@{op = "add"; path = "/relations/-"; value = @{
			rel = "System.LinkTypes.Hierarchy-Reverse"
			url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$($task.ParentID)"
		}}
	)

	$body = $payload | ConvertTo-Json -Depth 10
	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"

	try {
		$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body
		Write-Host "OK Task #$($result.id): $($task.Titulo)" -ForegroundColor Green
	} catch {
		Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
	}
}

Write-Host "`n=== RESUMEN ===" -ForegroundColor Cyan
Write-Host "US-637: 2 tareas agregadas (Test + CR) - marcadas como Done" -ForegroundColor Green
Write-Host "US-638: 2 tareas agregadas (Test + CR) - estado To Do" -ForegroundColor Yellow
