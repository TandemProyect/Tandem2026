$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }

# 1. WITs con nombre correcto
Write-Host "=== WORK ITEM TYPES ===" -ForegroundColor Cyan
$wits = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitemtypes?api-version=7.0" -Headers $h
$wits.value | ForEach-Object { Write-Host "  - $($_.name) [color: $($_.color)]" -ForegroundColor Green }

# 2. Procesos disponibles en la organización
Write-Host "`n=== PROCESOS EN LA ORGANIZACION ===" -ForegroundColor Cyan
$procs = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/process/processes?api-version=7.0" -Headers $h
$procs.value | ForEach-Object { Write-Host "  - $($_.name) [ID: $($_.id), Tipo: $($_.type)]" -ForegroundColor Yellow }

# 3. ID del proceso actual del proyecto
Write-Host "`n=== PROCESO DEL PROYECTO ===" -ForegroundColor Cyan
$proj = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/tandem2026?includeCapabilities=true&api-version=7.0" -Headers $h
Write-Host "Proceso: $($proj.capabilities.processTemplate.templateName)" -ForegroundColor Yellow
Write-Host "TypeId: $($proj.capabilities.processTemplate.templateTypeId)" -ForegroundColor Gray

# 4. Card style settings (probar distintas versiones API)
Write-Host "`n=== CARD STYLE SETTINGS ===" -ForegroundColor Cyan
$styleEndpoints = @(
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Issues/cardstylesettings?api-version=7.1-preview.1",
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Issues/cardstylesettings?api-version=6.0-preview.1"
)
foreach ($ep in $styleEndpoints) {
    try {
        $s = Invoke-RestMethod -Uri $ep -Headers $h
        Write-Host "OK con: $ep" -ForegroundColor Green
        Write-Host ($s | ConvertTo-Json -Depth 4 | Select-Object -First 50) -ForegroundColor Gray
        break
    } catch {
        Write-Host "FAIL [$($ep.Split('=')[-1])]: $($_.Exception.Message)" -ForegroundColor Red
    }
}
