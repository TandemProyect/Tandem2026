# Script para crear tareas de la US-638 (ATK60)
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

$ParentID = 638

# Definir las tareas
$tasks = @(
	@{
		Titulo = "Analizar especificaciones técnicas del sistema ATK60"
		Descripcion = "Revisar documentación técnica del sistema ATK60 y definir requisitos exactos de posicionamiento de puntos de anclaje"
	},
	@{
		Titulo = "Diseñar algoritmo de cálculo de puntos ATK60"
		Descripcion = "Diseñar e implementar el algoritmo que calcula las posiciones óptimas de los puntos de instalación basándose en las esquinas detectadas"
	},
	@{
		Titulo = "Implementar validación de compatibilidad ATK60"
		Descripcion = "Crear lógica de validación que verifique que los puntos detectados cumplen con las especificaciones del sistema ATK60"
	},
	@{
		Titulo = "Implementar exportación de datos de puntos"
		Descripcion = "Desarrollar funcionalidad para exportar los puntos detectados en formatos JSON y/o CSV compatibles con el sistema de instalación"
	},
	@{
		Titulo = "Crear generador de reporte de puntos ATK60"
		Descripcion = "Implementar generación de reportes detallados con coordenadas, especificaciones y validaciones de cada punto"
	},
	@{
		Titulo = "Actualizar marcado visual en ZWCAD"
		Descripcion = "Modificar el comando de ZWCAD para diferenciar visualmente los puntos ATK60 (colores, símbolos, etiquetas)"
	},
	@{
		Titulo = "Crear pruebas con geometría compleja"
		Descripcion = "Diseñar y ejecutar casos de prueba con estructuras complejas, múltiples esquinas y orientaciones variadas"
	},
	@{
		Titulo = "Documentar funcionalidad ATK60"
		Descripcion = "Actualizar documentación técnica con la funcionalidad ATK60, incluyendo ejemplos de uso y casos de prueba"
	}
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
		Write-Host "✅ Task #$($result.id) creada: $($task.Titulo)" -ForegroundColor Green
		$createdTasks += $result.id
	} catch {
		Write-Host "❌ Error creando tarea '$($task.Titulo)': $($_.Exception.Message)" -ForegroundColor Red
	}
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✅ RESUMEN" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "US #$ParentID - Detectar puntos para sistema ATK 60" -ForegroundColor White
Write-Host "Tareas creadas: $($createdTasks.Count)" -ForegroundColor Cyan
Write-Host "IDs: $($createdTasks -join ', ')" -ForegroundColor Gray
Write-Host "`nURL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/$ParentID" -ForegroundColor Gray
