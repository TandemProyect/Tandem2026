# ⚡ Script Rápido: Crear US con Tasks Estándar
# Uso: .\Quick-US.ps1 "Título de la US" ["Descripción opcional"]

param(
	[Parameter(Mandatory=$true, Position=0)]
	[string]$Titulo,

	[Parameter(Mandatory=$false, Position=1)]
	[string]$Descripcion = ""
)

# ✅ CONFIGURACIÓN CORRECTA (NO MODIFICAR)
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$org = "VSCAD"
$project = "tandem2026"

$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "🎯 Creando User Story Completa" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ========================================
# 1. CREAR USER STORY
# ========================================
Write-Host "`n[1/2] Creando User Story..." -ForegroundColor Yellow
Write-Host "Título: $Titulo" -ForegroundColor Gray

$usPayload = @(
	@{op = "add"; path = "/fields/System.Title"; value = $Titulo}
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Issue"}
)

if ($Descripcion) {
	Write-Host "Descripción: $Descripcion" -ForegroundColor Gray
	$usPayload += @{op = "add"; path = "/fields/System.Description"; value = $Descripcion}
}

$usBody = $usPayload | ConvertTo-Json -Depth 10
$usUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/`$Issue?api-version=7.0"

try {
	$us = Invoke-RestMethod -Uri $usUrl -Headers $headers -Method Post -Body $usBody
	$usId = $us.id

	Write-Host "✓ US #$usId creada exitosamente" -ForegroundColor Green
} catch {
	Write-Host "✗ Error al crear US: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# ========================================
# 2. CREAR TASKS ESTÁNDAR
# ========================================
Write-Host "`n[2/2] Creando Tasks estándar (Develop, Test, CR)..." -ForegroundColor Yellow

$tasks = @("Develop", "Test", "CR")
$createdTasks = @()

foreach ($taskType in $tasks) {
	$taskPayload = '[{"op":"add","path":"/fields/System.Title","value":"' + $taskType + '"},{"op":"add","path":"/fields/System.WorkItemType","value":"Task"},{"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"https://dev.azure.com/' + $org + '/' + $project + '/_apis/wit/workitems/' + $usId + '"}}]'

	$taskUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/`$Task?api-version=7.0"

	try {
		$task = Invoke-RestMethod -Uri $taskUrl -Headers $headers -Method Post -Body $taskPayload
		$createdTasks += @{
			id = $task.id
			type = $taskType
		}
		Write-Host "  ✓ Task $taskType creada: #$($task.id)" -ForegroundColor Green
	} catch {
		Write-Host "  ✗ Error creando task $taskType : $($_.Exception.Message)" -ForegroundColor Red
	}
}

# ========================================
# 3. RESUMEN FINAL
# ========================================
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ USER STORY COMPLETA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "`n📋 User Story #$usId" -ForegroundColor Cyan
Write-Host "   Título: $Titulo" -ForegroundColor White
if ($Descripcion) {
	Write-Host "   Descripción: $Descripcion" -ForegroundColor Gray
}

Write-Host "`n📌 Tasks creadas:" -ForegroundColor Cyan
foreach ($task in $createdTasks) {
	Write-Host "   #$($task.id) - $($task.type)" -ForegroundColor White
}

Write-Host "`n🔗 Enlaces:" -ForegroundColor Cyan
Write-Host "   US:    https://dev.azure.com/$org/$project/_workitems/edit/$usId" -ForegroundColor White
Write-Host "   Board: https://dev.azure.com/$org/$project/_boards/board/t/$project%20Team/Issues" -ForegroundColor White

Write-Host "`n💡 Próximos pasos:" -ForegroundColor Cyan
Write-Host "   1. Revisar la US en Azure DevOps" -ForegroundColor White
Write-Host "   2. Ajustar prioridad si es necesario:" -ForegroundColor White
Write-Host "      .\Edit-US.ps1 $usId -Prioridad 1" -ForegroundColor Gray
Write-Host "   3. Vincular commits con: AB#$usId" -ForegroundColor White
Write-Host "      git commit -m `"feat: descripción AB#$usId`"" -ForegroundColor Gray

Write-Host "`n========================================`n" -ForegroundColor Green
