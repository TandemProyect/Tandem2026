$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$hj   = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json; charset=utf-8" }
$base = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems"

# 1. Crear US
Write-Host "Creando US..." -ForegroundColor Cyan
$usPayload = '[{"op":"add","path":"/fields/System.Title","value":"Create Faces on L-corner type"},{"op":"add","path":"/fields/System.State","value":"Active"}]'
$us = Invoke-RestMethod -Uri ("$base/`$User%20Story?api-version=7.0") -Headers $hj -Method Post -Body ([Text.Encoding]::UTF8.GetBytes($usPayload))
$usId = $us.id
Write-Host ("  US #" + $usId + " creada: " + $us.fields.'System.Title') -ForegroundColor Green
Write-Host ("  Estado: " + $us.fields.'System.State') -ForegroundColor Yellow

# 2. Crear tareas CR y Test vinculadas
foreach ($taskName in @("CR", "Test")) {
    Write-Host ("Creando Task '" + $taskName + "'...") -ForegroundColor Cyan
    $taskPayload = '[{"op":"add","path":"/fields/System.Title","value":"' + $taskName + '"},{"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/' + $usId + '"}}]'
    $task = Invoke-RestMethod -Uri ("$base/`$Task?api-version=7.0") -Headers $hj -Method Post -Body ([Text.Encoding]::UTF8.GetBytes($taskPayload))
    Write-Host ("  Task #" + $task.id + " '" + $taskName + "' creada y vinculada") -ForegroundColor Green
}

Write-Host "`nUS #$usId en columna 'Tasks to Analyze'" -ForegroundColor Green
Write-Host ("URL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/" + $usId) -ForegroundColor Gray
