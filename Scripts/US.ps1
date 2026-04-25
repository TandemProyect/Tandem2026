# Script: US.ps1 (User Story rapida)
param(
	[Parameter(Mandatory=$true, Position=0)]
	[string]$Titulo,

	[Parameter(Mandatory=$false, Position=1)]
	[string]$Descripcion = "",

	[Parameter(Mandatory=$false)]
	[int]$StoryPoints = 0
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

$payloadArray = @(
	@{op = "add"; path = "/fields/System.Title"; value = $Titulo},
	@{op = "add"; path = "/fields/System.AreaPath"; value = "tandem2026"},
	@{op = "add"; path = "/fields/System.IterationPath"; value = "tandem2026\\Sprint 1"},
	@{op = "add"; path = "/fields/Microsoft.VSTS.Common.Priority"; value = 2}
)

if ($Descripcion) {
	$payloadArray += @{op = "add"; path = "/fields/System.Description"; value = $Descripcion}
}

if ($StoryPoints -gt 0) {
	$payloadArray += @{op = "add"; path = "/fields/Microsoft.VSTS.Scheduling.StoryPoints"; value = $StoryPoints}
}

if ($payloadArray.Count -eq 1) {
	$payload = "[$($payloadArray | ConvertTo-Json -Depth 10)]"
} else {
	$payload = $payloadArray | ConvertTo-Json -Depth 10
}

$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Issue?api-version=7.0"

try {
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload
	Write-Host "✓ US #$($result.id) creada" -ForegroundColor Green
} catch {
	Write-Host "✗ Error al crear US: $_" -ForegroundColor Red
	throw
}

# Crear Tasks automaticamente (Develop, Test, CR)
Write-Host "Creando Tasks automaticas..." -ForegroundColor Cyan
$tasks = @("Develop", "Test", "CR")
foreach ($taskType in $tasks) {
	$taskTitle = "$taskType - $Titulo"
	$taskPayloadArray = @(
		@{op = "add"; path = "/fields/System.Title"; value = $taskTitle}
	)
	$taskPayloadArray += @{
		op = "add"
		path = "/relations/-"
		value = @{
			rel = "System.LinkTypes.Hierarchy-Reverse"
			url = $result.url
		}
	}
	$taskPayload = $taskPayloadArray | ConvertTo-Json -Depth 10
	if ($taskPayloadArray.Count -eq 1) {
		$taskPayload = "[$taskPayload]"
	}
	$taskUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"

	try {
		$taskResult = Invoke-RestMethod -Uri $taskUrl -Headers $headers -Method Post -Body $taskPayload
		Write-Host "  ✓ Task $taskType creada: #$($taskResult.id)" -ForegroundColor Green
	} catch {
		Write-Host "  ✗ Error al crear Task $taskType : $_" -ForegroundColor Yellow
	}
}

if ($result._links.html.href) {
	Start-Process $result._links.html.href
}
return $result.id
