# Script para crear tareas de la US-637
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

$ParentID = 637

# Definir las tareas para detección de geometría y esquinas en L
$tasks = @(
	@{
		Titulo = "Implementar endpoint MVC para recibir geometría ZWCAD"
		Descripcion = "Crear endpoint ProcesarLineasZwcad en DesignToolsAutocadController para recibir selección de líneas desde el plugin"
	},
	@{
		Titulo = "Crear DTOs para intercambio de datos ZWCAD-MVC"
		Descripcion = "Implementar LineaDTO, PuntoDTO, SeleccionLineasDTO, EsquinaLDTO, DeteccionEsquinasLDTO y ApiResponse en ambos proyectos"
	},
	@{
		Titulo = "Desarrollar detector de esquinas en L (LCornerDetector)"
		Descripcion = "Implementar lógica de detección: agrupar líneas paralelas, validar perpendicularidad, calcular offsets"
	},
	@{
		Titulo = "Implementar validación de offset máximo de panel"
		Descripcion = "Validar que líneas paralelas tengan separación <= 1500 unidades (ancho máximo de panel)"
	},
	@{
		Titulo = "Agregar soporte para geometría rotada"
		Descripcion = "Implementar detección basada en relaciones geométricas (perpendicular/paralelo) en lugar de orientación absoluta"
	},
	@{
		Titulo = "Implementar exportación de diagnósticos JSON"
		Descripcion = "Crear sistema de telemetría que guarde resultados en C:\\temp\\conexiones.json con resumen ejecutivo"
	},
	@{
		Titulo = "Actualizar plugin ZWCAD para dibujar marcadores visuales"
		Descripcion = "Modificar comando TANDEM_SELECCIONAR_LINEAS para dibujar círculos rojos en puntos de conexión detectados"
	},
	@{
		Titulo = "Realizar pruebas con geometría compleja y rotada"
		Descripcion = "Validar detección con estructuras rectangulares, múltiples esquinas y diferentes orientaciones"
	}
)

$createdTasks = @()

foreach ($task in $tasks) {
	$payload = @(
		@{op = "add"; path = "/fields/System.Title"; value = $task.Titulo},
		@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
		@{op = "add"; path = "/fields/System.Description"; value = $task.Descripcion},
		@{op = "add"; path = "/fields/System.State"; value = "Done"},
		@{op = "add"; path = "/relations/-"; value = @{
			rel = "System.LinkTypes.Hierarchy-Reverse"
			url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$ParentID"
		}}
	)

	$body = $payload | ConvertTo-Json -Depth 10
	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"

	try {
		$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body
		Write-Host "✅ Task #$($result.id) creada como Done: $($task.Titulo)" -ForegroundColor Green
		$createdTasks += $result.id
	} catch {
		Write-Host "❌ Error creando tarea '$($task.Titulo)': $($_.Exception.Message)" -ForegroundColor Red
	}
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✅ RESUMEN" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "US #$ParentID - Detección de esquinas en L" -ForegroundColor White
Write-Host "Tareas creadas: $($createdTasks.Count)" -ForegroundColor Cyan
Write-Host "IDs: $($createdTasks -join ', ')" -ForegroundColor Gray
Write-Host "Estado: Todas marcadas como Done (trabajo ya completado)" -ForegroundColor Green
Write-Host "`nURL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/$ParentID" -ForegroundColor Gray
