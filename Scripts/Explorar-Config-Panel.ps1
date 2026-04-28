$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }

# 1. Proceso del proyecto
Write-Host "=== PROCESO DEL PROYECTO ===" -ForegroundColor Cyan
$proj = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/tandem2026?includeCapabilities=true&api-version=7.0" -Headers $h
Write-Host "Proceso: $($proj.capabilities.processTemplate.templateName)" -ForegroundColor Yellow

# 2. Work Item Types disponibles
Write-Host "`n=== WORK ITEM TYPES DISPONIBLES ===" -ForegroundColor Cyan
$wits = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitemtypes?api-version=7.0" -Headers $h
$wits.value | ForEach-Object { Write-Host "  - $($_.name)" -ForegroundColor Green }

# 3. Card settings actuales
Write-Host "`n=== CARD SETTINGS ACTUALES ===" -ForegroundColor Cyan
try {
    $cs = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Issues/cardsettings?api-version=7.0" -Headers $h
    Write-Host "Card fields: $($cs.cards | ConvertTo-Json -Depth 3)" -ForegroundColor Gray
} catch {
    Write-Host "Error obteniendo card settings: $($_.Exception.Message)" -ForegroundColor Red
}

# 4. Card styles/rules actuales
Write-Host "`n=== CARD STYLES ACTUALES ===" -ForegroundColor Cyan
try {
    $styles = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Issues/cardstylesettings?api-version=7.0" -Headers $h
    Write-Host ($styles | ConvertTo-Json -Depth 5) -ForegroundColor Gray
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
