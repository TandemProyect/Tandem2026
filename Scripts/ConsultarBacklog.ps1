$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }

Write-Host "=== BACKLOG CONFIG ===" -ForegroundColor Cyan
$bc = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/backlogconfiguration?api-version=7.0" -Headers $h

Write-Host "`nRequirementCategory (Stories board):" -ForegroundColor Yellow
$req = $bc.portfolioBacklogs | Where-Object { $_.id -eq "Microsoft.RequirementCategory" }
if (-not $req) {
    $req2 = $bc.requirementBacklog
    $req2.workItemTypes | ForEach-Object { Write-Host ("  WIT: " + $_.name) }
} else {
    $req.workItemTypes | ForEach-Object { Write-Host ("  WIT: " + $_.name) }
}

Write-Host "`nRequirementBacklog direct:" -ForegroundColor Yellow
$bc.requirementBacklog.workItemTypes | ForEach-Object { Write-Host ("  WIT: " + $_.name) -ForegroundColor Green }

Write-Host "`nEstados validos por WIT:" -ForegroundColor Cyan
$bc.requirementBacklog.workItemTypes | ForEach-Object {
    $witName = $_.name
    Write-Host ("`n  " + $witName + ":") -ForegroundColor Yellow
    $witStates = Invoke-RestMethod -Uri ("https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitemtypes/" + [Uri]::EscapeDataString($witName) + "/states?api-version=7.0") -Headers $h
    $witStates.value | ForEach-Object { Write-Host ("    - " + $_.name + " [" + $_.stateCategory + "]") }
}
