# Script: US.ps1 (User Story rapida)
param(
	[Parameter(Mandatory=$true, Position=0)]
	[string]$Titulo,

	[Parameter(Mandatory=$false, Position=1)]
	[string]$Descripcion = ""
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

$payload = @(
	@{op = "add"; path = "/fields/System.Title"; value = $Titulo},
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Issue"},
	@{op = "add"; path = "/fields/System.AreaPath"; value = "tandem2026"},
	@{op = "add"; path = "/fields/System.IterationPath"; value = "tandem2026\\Sprint 1"},
	@{op = "add"; path = "/fields/Microsoft.VSTS.Common.Priority"; value = 2}
) | ConvertTo-Json -Depth 10

if ($Descripcion) {
	$payload = ($payload | ConvertFrom-Json) + @{op = "add"; path = "/fields/System.Description"; value = $Descripcion} | ConvertTo-Json -Depth 10
}

$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Issue?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload

Write-Host "✓ #$($result.id) creada" -ForegroundColor Green
Start-Process $result._links.html.href
return $result.id
