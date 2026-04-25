# Ver estructura completa del board
$pat = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "",$pat)))
$headers = @{
	Authorization = ("Basic {0}" -f $base64AuthInfo)
}

$teamId = "2ea0799c-57e5-48f6-87dd-f9eb6c232196"
$boardUrl = "https://dev.azure.com/VSCAD/tandem2026/$teamId/_apis/work/boards/Issues?api-version=7.1-preview.1"

$board = Invoke-RestMethod -Uri $boardUrl -Method Get -Headers $headers
$board | ConvertTo-Json -Depth 10 | Out-File "C:\00_Tandem2026\board-structure.json"
Write-Host "Estructura guardada en board-structure.json" -ForegroundColor Green
