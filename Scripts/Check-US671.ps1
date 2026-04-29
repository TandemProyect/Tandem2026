$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $PAT))
$headers = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }

foreach ($id in @(671, 672, 673, 674)) {
    try {
        $r = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$id?api-version=7.0" -Headers $headers -ErrorAction Stop
        Write-Host "#$id OK: $($r.fields.'System.Title') [$($r.fields.'System.State')]" -ForegroundColor Green
    } catch {
        Write-Host "#$id ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
}
