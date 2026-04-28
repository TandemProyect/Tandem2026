$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$hj   = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json; charset=utf-8" }

# 1. Crear US "Project Start"
Write-Host "Creando US 'Project Start'..." -ForegroundColor Cyan
$payload = '[{"op":"add","path":"/fields/System.Title","value":"Project Start"},{"op":"add","path":"/fields/System.Description","value":"Initial project setup and kickoff for Tandem 2026."}]'
$result  = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$User%20Story?api-version=7.0" -Headers $hj -Method Post -Body ([Text.Encoding]::UTF8.GetBytes($payload))
$usId    = $result.id
Write-Host ("  US #" + $usId + " creada: " + $result.fields.'System.Title') -ForegroundColor Green

# 2. Mover a Closed
Write-Host "Moviendo US #$usId a Closed..." -ForegroundColor Cyan
$close = '[{"op":"replace","path":"/fields/System.State","value":"Closed"}]'
Invoke-RestMethod -Uri ("https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/" + $usId + "?api-version=7.0") -Headers $hj -Method Patch -Body ([Text.Encoding]::UTF8.GetBytes($close)) | Out-Null
Write-Host "  US #$usId marcada como Closed" -ForegroundColor Green

Write-Host "`nUS ID: $usId" -ForegroundColor Yellow
Write-Host ("URL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/" + $usId) -ForegroundColor Gray
