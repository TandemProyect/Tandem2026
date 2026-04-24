# Script: Crear-UserStory.ps1
# Crea una nueva User Story en Azure DevOps

param(
	[Parameter(Mandatory=$true)]
	[string]$PAT,

	[Parameter(Mandatory=$true)]
	[string]$Titulo,

	[Parameter(Mandatory=$false)]
	[string]$Descripcion = "",

	[Parameter(Mandatory=$false)]
	[string]$Prioridad = "2",

	[Parameter(Mandatory=$false)]
	[string]$Esfuerzo = "",

	[Parameter(Mandatory=$false)]
	[string]$AsignadoA = "",

	[Parameter(Mandatory=$false)]
	[string]$Tags = "",

	[Parameter(Mandatory=$false)]
	[string]$AreaPath = "tandem2026",

	[Parameter(Mandatory=$false)]
	[string]$IterationPath = "tandem2026\\Sprint 1"
)

$ErrorActionPreference = "Stop"

$Org = "VSCAD"
$Project = "tandem2026"

# Autenticacion
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

Write-Host "`n=== CREANDO USER STORY ===" -ForegroundColor Cyan
Write-Host "Proyecto: $Project" -ForegroundColor White
Write-Host "Titulo: $Titulo" -ForegroundColor White

# Construir payload
$fields = @(
	@{
		op = "add"
		path = "/fields/System.Title"
		value = $Titulo
	},
	@{
		op = "add"
		path = "/fields/System.WorkItemType"
		value = "Issue"
	},
	@{
		op = "add"
		path = "/fields/System.AreaPath"
		value = $AreaPath
	},
	@{
		op = "add"
		path = "/fields/System.IterationPath"
		value = $IterationPath
	},
	@{
		op = "add"
		path = "/fields/Microsoft.VSTS.Common.Priority"
		value = [int]$Prioridad
	}
)

# Agregar descripcion si existe
if ($Descripcion) {
	$fields += @{
		op = "add"
		path = "/fields/System.Description"
		value = $Descripcion
	}
}

# Agregar esfuerzo si existe
if ($Esfuerzo) {
	$fields += @{
		op = "add"
		path = "/fields/Microsoft.VSTS.Scheduling.Effort"
		value = $Esfuerzo
	}
}

# Agregar asignado si existe
if ($AsignadoA) {
	$fields += @{
		op = "add"
		path = "/fields/System.AssignedTo"
		value = $AsignadoA
	}
}

# Agregar tags si existen
if ($Tags) {
	$fields += @{
		op = "add"
		path = "/fields/System.Tags"
		value = $Tags
	}
}

$payload = $fields | ConvertTo-Json -Depth 10

# Crear work item
$url = "https://dev.azure.com/$Org/$Project/_apis/wit/workitems/`$Issue?api-version=7.0"

try {
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload

	Write-Host "`n✓ User Story creada exitosamente!" -ForegroundColor Green
	Write-Host "`nID: #$($result.id)" -ForegroundColor Cyan
	Write-Host "Titulo: $($result.fields.'System.Title')" -ForegroundColor White
	Write-Host "Estado: $($result.fields.'System.State')" -ForegroundColor Gray
	Write-Host "Prioridad: $($result.fields.'Microsoft.VSTS.Common.Priority')" -ForegroundColor Gray

	if ($result.fields.'System.AssignedTo') {
		Write-Host "Asignado a: $($result.fields.'System.AssignedTo'.displayName)" -ForegroundColor Gray
	}

	Write-Host "`nURL: $($result._links.html.href)" -ForegroundColor Cyan

	# Abrir en el navegador
	Start-Process $result._links.html.href

	return $result.id

} catch {
	Write-Host "`n✗ Error al crear User Story:" -ForegroundColor Red
	Write-Host $_.Exception.Message -ForegroundColor Red

	if ($_.ErrorDetails.Message) {
		Write-Host $_.ErrorDetails.Message -ForegroundColor Yellow
	}

	exit 1
}
