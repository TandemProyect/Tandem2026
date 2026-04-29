$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }

Write-Host "WITs en RequirementBacklog:" -ForegroundColor Cyan
$bc = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/backlogconfiguration?api-version=7.0" -Headers $h
$bc.requirementBacklog.workItemTypes | ForEach-Object { Write-Host ("  - " + $_.name) -ForegroundColor Yellow }

Write-Host "`nEstados de Bug:" -ForegroundColor Cyan
$bugStates = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitemtypes/Bug/states?api-version=7.0" -Headers $h
$bugStates.value | ForEach-Object { Write-Host ("  - " + $_.name + " [" + $_.stateCategory + "]") -ForegroundColor Green }
