# Script temporal para cerrar US-619
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

$body = '[{"op":"replace","path":"/fields/System.State","value":"Closed"}]'
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/619?api-version=7.0"

try {
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body
	Write-Host "✅ US #619 cerrada exitosamente" -ForegroundColor Green
	Write-Host "Estado: $($result.fields.'System.State')" -ForegroundColor Cyan
	Write-Host "Título: $($result.fields.'System.Title')" -ForegroundColor White
} catch {
	Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}
