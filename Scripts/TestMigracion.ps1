$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }
$hj   = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json; charset=utf-8" }
$newP = "c36c639d-de0d-456d-bc7e-fa8384e4f950"
$oldP = "b8a3a935-7e91-48b8-a94c-606d37c3e9f2"
$proj = "213253e7-f177-4e2d-bdf3-410b97f6883d"
$brd  = "892fa957-9c33-4237-a99f-2660bd9ec80d"

# WITs en proceso Tandem2026
Write-Host "WITs en Tandem2026:" -ForegroundColor Cyan
$r = Invoke-RestMethod -Uri ("https://dev.azure.com/VSCAD/_apis/work/processes/" + $newP + "/workItemTypes?api-version=7.1-preview.2") -Headers $h
$r.value | ForEach-Object { Write-Host ("  " + $_.name + " [" + $_.color + "]") }

# Proceso actual
Write-Host "Proceso actual del proyecto:" -ForegroundColor Cyan
$pj = Invoke-RestMethod -Uri ("https://dev.azure.com/VSCAD/_apis/projects/" + $proj + "?includeCapabilities=true&api-version=7.0") -Headers $h
Write-Host ("  " + $pj.capabilities.processTemplate.templateName)

# Intentar migracion
Write-Host "Intentando migracion:" -ForegroundColor Cyan
$bodyMig = '{"name":"tandem2026","sourceProcessTypeId":"' + $oldP + '","targetProcessTypeId":"' + $newP + '","projectId":"' + $proj + '"}'
try {
    Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/work/processes/migrations?api-version=7.1-preview.1" -Headers $hj -Method Post -Body ([Text.Encoding]::UTF8.GetBytes($bodyMig)) | Out-Null
    Write-Host "  OK migracion!" -ForegroundColor Green
} catch {
    Write-Host ("  FAIL: " + $_.ErrorDetails.Message) -ForegroundColor Red
}

# Card styles
Write-Host "Card styles GET:" -ForegroundColor Cyan
$url1 = "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/" + $brd + "/cardstylesettings?api-version=7.0"
$url2 = "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Issues/cardstylesettings?api-version=7.0"
foreach ($u in @($url1, $url2)) {
    try {
        Invoke-RestMethod -Uri $u -Headers $h | Out-Null
        Write-Host ("  OK: " + $u) -ForegroundColor Green
    } catch {
        Write-Host ("  FAIL: " + $_.Exception.Message) -ForegroundColor Red
    }
}
