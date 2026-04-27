# Marcar US-637 como Done
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

$body = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/637?api-version=7.0"

try {
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body
	Write-Host "OK US #637 marcada como Done" -ForegroundColor Green
	Write-Host "Estado: $($result.fields.'System.State')" -ForegroundColor Cyan
	Write-Host "Titulo: $($result.fields.'System.Title')" -ForegroundColor White
} catch {
	Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
