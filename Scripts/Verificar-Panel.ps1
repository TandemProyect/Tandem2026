$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{ Authorization = "Basic $auth" }
$url = "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Issues/columns?api-version=7.0"

$r = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
Write-Host "Columnas actuales del panel:" -ForegroundColor Cyan
$r.value | ForEach-Object {
    $tipo = $_.columnType
    $nombre = $_.name
    $wip = $_.itemLimit
    Write-Host "  [$tipo] '$nombre' (WIP: $wip)" -ForegroundColor Green
}
Write-Host "Total: $($r.count) columnas" -ForegroundColor Yellow
