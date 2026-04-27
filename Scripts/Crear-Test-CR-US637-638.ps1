# Crear tareas Test y CR para US-637 y US-638
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

# US-637 - Tareas marcadas como Done (ya completada)
Write-Host "Creando tareas para US-637..." -ForegroundColor Cyan

# Tarea Test US-637
$payload = @(
	@{op = "add"; path = "/fields/System.Title"; value = "Realizar pruebas de deteccion de esquinas"},
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
	@{op = "add"; path = "/fields/System.Description"; value = "Ejecutar suite completa de tests para validar deteccion de esquinas en L"},
	@{op = "add"; path = "/relations/-"; value = @{rel = "System.LinkTypes.Hierarchy-Reverse"; url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/637"}}
) | ConvertTo-Json -Depth 10
$result = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0" -Headers $headers -Method Post -Body $payload
Write-Host "Test Task created: #$($result.id)" -ForegroundColor Green

# Tarea CR US-637
$payload = @(
	@{op = "add"; path = "/fields/System.Title"; value = "Code Review del detector de esquinas"},
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
	@{op = "add"; path = "/fields/System.Description"; value = "Revisar codigo de LCornerDetector, DTOs y endpoint MVC"},
	@{op = "add"; path = "/relations/-"; value = @{rel = "System.LinkTypes.Hierarchy-Reverse"; url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/637"}}
) | ConvertTo-Json -Depth 10
$result = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0" -Headers $headers -Method Post -Body $payload
Write-Host "CR Task created: #$($result.id)" -ForegroundColor Green

# US-638 - Tareas en To Do (nueva US)
Write-Host "`nCreando tareas para US-638..." -ForegroundColor Cyan

# Tarea Test US-638
$payload = @(
	@{op = "add"; path = "/fields/System.Title"; value = "Realizar pruebas con casos ATK60"},
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
	@{op = "add"; path = "/fields/System.Description"; value = "Ejecutar suite de tests para validar deteccion de puntos ATK60"},
	@{op = "add"; path = "/relations/-"; value = @{rel = "System.LinkTypes.Hierarchy-Reverse"; url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/638"}}
) | ConvertTo-Json -Depth 10
$result = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0" -Headers $headers -Method Post -Body $payload
Write-Host "Test Task created: #$($result.id)" -ForegroundColor Green

# Tarea CR US-638
$payload = @(
	@{op = "add"; path = "/fields/System.Title"; value = "Code Review de implementacion ATK60"},
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
	@{op = "add"; path = "/fields/System.Description"; value = "Revisar codigo de calculo de puntos, validaciones y exportacion ATK60"},
	@{op = "add"; path = "/relations/-"; value = @{rel = "System.LinkTypes.Hierarchy-Reverse"; url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/638"}}
) | ConvertTo-Json -Depth 10
$result = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0" -Headers $headers -Method Post -Body $payload
Write-Host "CR Task created: #$($result.id)" -ForegroundColor Green

Write-Host "`nOK - Todas las tareas Test y CR creadas" -ForegroundColor Green
