param(
	[string]$Titulo,
	[string]$Descripcion = "",
	[int]$StoryPoints = 0
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$headers = @{
	Authorization = "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT")))"
	"Content-Type" = "application/json-patch+json"
}

$ops = @(
	@{op="add"; path="/fields/System.Title"; value=$Titulo}
	@{op="add"; path="/fields/System.AreaPath"; value="tandem2026"}
)

if ($Descripcion) {
	$ops += @{op="add"; path="/fields/System.Description"; value=$Descripcion}
}

if ($StoryPoints -gt 0) {
	$ops += @{op="add"; path="/fields/Microsoft.VSTS.Scheduling.StoryPoints"; value=$StoryPoints}
}

$body = $ops | ConvertTo-Json -Depth 10
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Issue?api-version=7.0"

$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) -ContentType "application/json-patch+json; charset=utf-8"
Write-Host "US #$($result.id) creada con $StoryPoints puntos" -ForegroundColor Green
Write-Host $result._links.html.href -ForegroundColor Cyan
Start-Process $result._links.html.href
return $result.id
