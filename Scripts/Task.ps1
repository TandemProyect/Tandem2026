# Script: Task.ps1 (Crear Task vinculada a User Story)
param(
	[Parameter(Mandatory=$true, Position=0)]
	[int]$ParentID,

	[Parameter(Mandatory=$true, Position=1)]
	[string]$Titulo,

	[Parameter(Mandatory=$false, Position=2)]
	[string]$Descripcion = "",

	[Parameter(Mandatory=$false)]
	[ValidateSet("To Do", "Doing", "Done")]
	[string]$Estado = "To Do"
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

# Crear payload base
$payload = @(
	@{op = "add"; path = "/fields/System.Title"; value = $Titulo},
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
	@{op = "add"; path = "/relations/-"; value = @{
		rel = "System.LinkTypes.Hierarchy-Reverse"
		url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$ParentID"
	}}
)

# Agregar descripción si se proporciona
if ($Descripcion) {
	$payload += @{op = "add"; path = "/fields/System.Description"; value = $Descripcion}
}

$body = $payload | ConvertTo-Json -Depth 10

# Crear Task
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body

Write-Host "✓ Task #$($result.id) creada y vinculada a US #$ParentID" -ForegroundColor Green

# Si el estado no es "To Do", actualizarlo
if ($Estado -ne "To Do") {
	$updateBody = '[{"op":"replace","path":"/fields/System.State","value":"' + $Estado + '"}]'
	$updateUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$($result.id)?api-version=7.0"
	$updateHeaders = @{
		Authorization = "Basic $auth"
		"Content-Type" = "application/json-patch+json; charset=utf-8"
	}

	Invoke-RestMethod -Uri $updateUrl -Headers $updateHeaders -Method Patch -Body $updateBody | Out-Null
	Write-Host "✓ Estado actualizado a '$Estado'" -ForegroundColor Green
}

# Abrir en navegador
Start-Process $result._links.html.href

return $result.id
