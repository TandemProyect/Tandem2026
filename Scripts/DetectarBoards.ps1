$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }

Write-Host "=== PROCESO ACTUAL ===" -ForegroundColor Cyan
$proj = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/tandem2026?includeCapabilities=true&api-version=7.0" -Headers $h
Write-Host ("Proceso: " + $proj.capabilities.processTemplate.templateName)

Write-Host "`n=== BOARDS DISPONIBLES ===" -ForegroundColor Cyan
$boards = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards?api-version=7.0" -Headers $h
$boards.value | ForEach-Object { Write-Host ("  Board: '" + $_.name + "' ID: " + $_.id) -ForegroundColor Green }

Write-Host "`n=== WITs DEL PROYECTO ===" -ForegroundColor Cyan
$wits = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitemtypes?api-version=7.0" -Headers $h
$wits.value | ForEach-Object { Write-Host ("  - " + $_.name) -ForegroundColor Yellow }
