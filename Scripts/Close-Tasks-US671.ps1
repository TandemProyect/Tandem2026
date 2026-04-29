$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$org = 'VSCAD'; $project = 'tandem2026'
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $PAT))
$headers = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }

# Cerrar tareas
$closedBody = '[{"op":"add","path":"/fields/System.State","value":"Closed"}]'
foreach ($taskId in @(672, 673, 674)) {
    $url = "https://dev.azure.com/$org/$project/_apis/wit/workitems/$taskId`?api-version=7.0"
    try {
        $r = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $closedBody -ErrorAction Stop
        Write-Host "Task #$taskId → Closed OK" -ForegroundColor Green
    } catch {
        $msg = $_.Exception.Message
        Write-Host "Task #$taskId ERROR: $msg" -ForegroundColor Red
    }
}

# Resolver US-671
$resolvedBody = '[{"op":"add","path":"/fields/System.State","value":"Resolved"}]'
$usUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/671`?api-version=7.0"
try {
    $r = Invoke-RestMethod -Uri $usUrl -Headers $headers -Method Patch -Body $resolvedBody -ErrorAction Stop
    Write-Host "US-671 → Resolved OK" -ForegroundColor Green
} catch {
    $msg = $_.Exception.Message
    Write-Host "US-671 ERROR: $msg" -ForegroundColor Red
}
