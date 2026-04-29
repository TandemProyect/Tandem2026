$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$org = 'VSCAD'; $project = 'tandem2026'
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $PAT))
$headers = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }

$closedBody   = '[{"op":"add","path":"/fields/System.State","value":"Closed"}]'
$resolvedBody = '[{"op":"add","path":"/fields/System.State","value":"Resolved"}]'

foreach ($taskId in @(676, 677, 678)) {
    $url = "https://dev.azure.com/$org/$project/_apis/wit/workitems/$taskId`?api-version=7.0"
    try {
        Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $closedBody -ErrorAction Stop | Out-Null
        Write-Host "Task #$taskId → Closed" -ForegroundColor Green
    } catch {
        Write-Host "Error Task #${taskId}: $($_.Exception.Message)" -ForegroundColor Red
    }
}

$usUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/675`?api-version=7.0"
try {
    Invoke-RestMethod -Uri $usUrl -Headers $headers -Method Patch -Body $resolvedBody -ErrorAction Stop | Out-Null
    Write-Host "US-675 → Resolved" -ForegroundColor Green
} catch {
    Write-Host "Error US-675: $($_.Exception.Message)" -ForegroundColor Red
}
