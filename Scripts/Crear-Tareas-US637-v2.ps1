# Script para crear tareas de la US-637 (sin estado inicial Done)
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

$ParentID = 637

$tasks = @(
	@{Titulo = "Implementar endpoint MVC para recibir geometria ZWCAD"; Descripcion = "Crear endpoint ProcesarLineasZwcad en DesignToolsAutocadController"},
	@{Titulo = "Crear DTOs para intercambio de datos ZWCAD-MVC"; Descripcion = "Implementar LineaDTO, PuntoDTO, SeleccionLineasDTO, EsquinaLDTO"},
	@{Titulo = "Desarrollar detector de esquinas en L (LCornerDetector)"; Descripcion = "Implementar logica de deteccion: agrupar lineas paralelas, validar perpendicularidad"},
	@{Titulo = "Implementar validacion de offset maximo de panel"; Descripcion = "Validar que lineas paralelas tengan separacion <= 1500 unidades"},
	@{Titulo = "Agregar soporte para geometria rotada"; Descripcion = "Implementar deteccion basada en relaciones geometricas"},
	@{Titulo = "Implementar exportacion de diagnosticos JSON"; Descripcion = "Crear sistema de telemetria en C:\\temp\\conexiones.json"},
	@{Titulo = "Actualizar plugin ZWCAD para dibujar marcadores visuales"; Descripcion = "Modificar comando para dibujar circulos rojos en puntos detectados"},
	@{Titulo = "Realizar pruebas con geometria compleja y rotada"; Descripcion = "Validar deteccion con estructuras rectangulares y multiples esquinas"}
)

$createdTasks = @()

foreach ($task in $tasks) {
	$payload = @(
		@{op = "add"; path = "/fields/System.Title"; value = $task.Titulo},
		@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
		@{op = "add"; path = "/fields/System.Description"; value = $task.Descripcion},
		@{op = "add"; path = "/relations/-"; value = @{
			rel = "System.LinkTypes.Hierarchy-Reverse"
			url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$ParentID"
		}}
	)

	$body = $payload | ConvertTo-Json -Depth 10
	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"

	try {
		$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body
		$taskId = $result.id
		Write-Host "Tarea #$taskId creada" -ForegroundColor Yellow

		# Ahora actualizar a Done
		$updateBody = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
		$updateUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$taskId?api-version=7.0"
		Invoke-RestMethod -Uri $updateUrl -Headers $headers -Method Patch -Body $updateBody | Out-Null

		Write-Host "✅ Task #$taskId Done: $($task.Titulo)" -ForegroundColor Green
		$createdTasks += $taskId
	} catch {
		Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
	}
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✅ RESUMEN US #$ParentID" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Tareas creadas y marcadas como Done: $($createdTasks.Count)" -ForegroundColor Cyan
Write-Host "IDs: $($createdTasks -join ', ')" -ForegroundColor Gray
