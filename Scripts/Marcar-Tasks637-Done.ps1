# Marcar Tasks 647-654 como Done
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

$body = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'

647..654 | ForEach-Object {
	$taskId = $_
	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$taskId?api-version=7.0"

	try {
		$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body
		Write-Host "OK Task #$taskId marcada como Done" -ForegroundColor Green
	} catch {
		Write-Host "Error Task #$taskId" -ForegroundColor Red
	}
}

Write-Host "`nTodas las tareas de US-637 han sido marcadas como Done" -ForegroundColor Cyan
