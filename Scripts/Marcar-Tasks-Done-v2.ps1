$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

647..654 | ForEach-Object {
	$taskId = $_

	# Primero a Doing
	$bodyDoing = '[{"op":"replace","path":"/fields/System.State","value":"Doing"}]'
	try {
		Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$taskId?api-version=7.0" -Headers $headers -Method Patch -Body $bodyDoing | Out-Null
		Write-Host "Task #$taskId -> Doing" -ForegroundColor Yellow
	} catch {
		Write-Host "Error moviendo #$taskId a Doing" -ForegroundColor Red
		return
	}

	# Luego a Done
	Start-Sleep -Milliseconds 500
	$bodyDone = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
	try {
		Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$taskId?api-version=7.0" -Headers $headers -Method Patch -Body $bodyDone | Out-Null
		Write-Host "Task #$taskId -> Done OK" -ForegroundColor Green
	} catch {
		Write-Host "Error moviendo #$taskId a Done" -ForegroundColor Red
	}
}
