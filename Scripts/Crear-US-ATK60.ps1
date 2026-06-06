# Script para crear nueva US de ATK60
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$headers = @{
	Authorization = "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT")))"
	"Content-Type" = "application/json-patch+json"
}

$Titulo = "Detectar puntos para sistema ATK 60"
$Descripcion = @"
<h2>Objetivo</h2>
<p>Implementar la detección automática de puntos críticos en esquinas para el <strong>Sistema de Encofrado ATK60</strong>.</p>

<h2>Contexto</h2>
<p>El sistema ATK60 es un sistema de encofrado modular que requiere la identificación precisa de puntos de anclaje en las esquinas de estructuras rectangulares.</p>

<h2>Requisitos</h2>
<ul>
<li>Detectar puntos de instalación para el sistema ATK60 en las esquinas identificadas</li>
<li>Calcular posiciones óptimas para los elementos de encofrado</li>
<li>Validar compatibilidad con las especificaciones técnicas del sistema ATK60</li>
<li>Exportar datos de puntos en formato compatible con el sistema de instalación</li>
<li>Generar reporte de puntos detectados con coordenadas y especificaciones</li>
</ul>

<h2>Criterios de Aceptación</h2>
<ul>
<li>Los puntos detectados cumplen con las especificaciones del sistema ATK60</li>
<li>Se genera un reporte detallado de cada punto de instalación</li>
<li>Los datos son exportables en formato JSON y/o CSV</li>
<li>El sistema maneja correctamente estructuras complejas y múltiples esquinas</li>
</ul>

<h2>Referencias</h2>
<ul>
<li>US-619: Detección de esquinas en L (completada)</li>
<li>Documentación: SISTEMA-ATK60.md</li>
<li>Documentación: documentación/AGENTE-US619-INFO.md</li>
</ul>
"@
$StoryPoints = 8

$ops = @(
	@{op="add"; path="/fields/System.Title"; value=$Titulo}
	@{op="add"; path="/fields/System.AreaPath"; value="tandem2026"}
	@{op="add"; path="/fields/System.Description"; value=$Descripcion}
	@{op="add"; path="/fields/Microsoft.VSTS.Scheduling.StoryPoints"; value=$StoryPoints}
)

$body = $ops | ConvertTo-Json -Depth 10
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Issue?api-version=7.0"

try {
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) -ContentType "application/json-patch+json; charset=utf-8"
	Write-Host "✅ US #$($result.id) creada exitosamente con $StoryPoints story points" -ForegroundColor Green
	Write-Host "Título: $Titulo" -ForegroundColor Cyan
	Write-Host "URL: $($result._links.html.href)" -ForegroundColor Gray

	# Guardar el ID para crear las tareas
	$result.id | Out-File "C:\temp\nueva_us_id.txt" -Encoding UTF8

	return $result.id
} catch {
	Write-Host "❌ Error creando US: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}
