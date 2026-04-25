$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "",$PAT)))
$headers = @{Authorization = ("Basic {0}" -f $base64AuthInfo)}
$teamId = "2ea0799c-57e5-48f6-87dd-f9eb6c232196"
$boardUrl = "https://dev.azure.com/VSCAD/tandem2026/$teamId/_apis/work/boards/Issues?api-version=7.1-preview.1"
Write-Host "Verificacion del Board Tandem 2026" -ForegroundColor Cyan
Write-Host ""
$board = Invoke-RestMethod -Uri $boardUrl -Method Get -Headers $headers
Write-Host "Board: $($board.name)" -ForegroundColor Green
Write-Host "Columnas actuales:" -ForegroundColor Yellow
$board.columns | ForEach-Object {Write-Host "  - $($_.name) ($($_.columnType)) -> Estado: $($_.stateMappings.Issue) | WIP: $($_.itemLimit)" -ForegroundColor White}
Write-Host ""
Write-Host "Ver en: https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues" -ForegroundColor Cyan
