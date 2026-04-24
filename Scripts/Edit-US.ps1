# Script: Edit-US.ps1 (Editar User Story rapida)
param(
	[Parameter(Mandatory=$true, Position=0)]
	[int]$ID,

	[Parameter(Mandatory=$false)]
	[string]$Titulo,

	[Parameter(Mandatory=$false)]
	[string]$Descripcion,

	[Parameter(Mandatory=$false)]
	[ValidateSet("To Do", "Doing", "Done")]
	[string]$Estado,

	[Parameter(Mandatory=$false)]
	[ValidateSet("1", "2", "3", "4")]
	[string]$Prioridad,

	[Parameter(Mandatory=$false)]
	[string]$AsignadoA
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

$changes = @()

if ($Titulo) { $changes += @{op = "replace"; path = "/fields/System.Title"; value = $Titulo} }
if ($Descripcion) { $changes += @{op = "replace"; path = "/fields/System.Description"; value = $Descripcion} }
if ($Estado) { $changes += @{op = "replace"; path = "/fields/System.State"; value = $Estado} }
if ($Prioridad) { $changes += @{op = "replace"; path = "/fields/Microsoft.VSTS.Common.Priority"; value = [int]$Prioridad} }
if ($AsignadoA) { $changes += @{op = "replace"; path = "/fields/System.AssignedTo"; value = $AsignadoA} }

if ($changes.Count -eq 0) {
	Write-Host "⚠ No se especificaron cambios" -ForegroundColor Yellow
	exit
}

$payload = $changes | ConvertTo-Json -Depth 10
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$ID?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $payload

Write-Host "✓ #$ID actualizada" -ForegroundColor Green
Start-Process $result._links.html.href
