$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }
$hJson = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json; charset=utf-8" }

$newProcessId = "c36c639d-de0d-456d-bc7e-fa8384e4f950"  # Tandem2026 process

# Obtener el GUID del proyecto
Write-Host "Obteniendo GUID del proyecto..." -ForegroundColor Cyan
$proj = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/tandem2026?api-version=7.0" -Headers $h
$projectId = $proj.id
Write-Host "  Project ID: $projectId" -ForegroundColor Yellow

# Intentar migración con la API dedicada
Write-Host "`nIntentando migración via processes/migrations..." -ForegroundColor Cyan
$bodyMig = @{
    targetProcessTypeId = $newProcessId
    projectId           = $projectId
} | ConvertTo-Json
try {
    $mig = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/work/processes/migrations?api-version=7.1-preview.1" -Headers $hJson -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyMig))
    Write-Host "  ✅ Migración iniciada: $($mig | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "  FAIL migrations API: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# Intentar PATCH con project ID (GUID) en lugar de nombre
Write-Host "`nIntentando PATCH con project GUID..." -ForegroundColor Cyan
$bodyPatch = @{
    capabilities = @{
        processTemplate = @{ templateTypeId = $newProcessId }
    }
} | ConvertTo-Json -Depth 5
try {
    Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/$projectId`?api-version=7.0" -Headers $hJson -Method Patch -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyPatch)) | Out-Null
    Write-Host "  ✅ Proyecto migrado al proceso Tandem2026" -ForegroundColor Green
} catch {
    Write-Host "  FAIL PATCH: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# Intentar card styles con board ID
Write-Host "`nIntentando card styles con board ID..." -ForegroundColor Cyan
$boardId = "892fa957-9c33-4237-a99f-2660bd9ec80d"
$styleEndpoints = @(
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/$boardId/cardstylesettings?api-version=7.0",
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/$boardId/cardstylesettings?api-version=6.1-preview.1",
    "https://dev.azure.com/VSCAD/tandem2026/$projectId/_apis/work/boards/$boardId/cardstylesettings?api-version=7.0"
)
$styleBody = @{
    rules = @{
        fill = @(
            @{ name="Bug"; filter="[System.WorkItemType] = 'Bug'"; isEnabled=$true; settings=@{"background-color"="#CC293D";"title-color"="#FFFFFF"} },
            @{ name="ZwcadPlugin"; filter="[System.Tags] Contains 'ZwcadPlugin'"; isEnabled=$true; settings=@{"background-color"="#339933";"title-color"="#FFFFFF"} },
            @{ name="Desing"; filter="[System.Tags] Contains 'Desing'"; isEnabled=$true; settings=@{"background-color"="#0078D4";"title-color"="#FFFFFF"} },
            @{ name="DAL"; filter="[System.Tags] Contains 'DAL'"; isEnabled=$true; settings=@{"background-color"="#E17D00";"title-color"="#FFFFFF"} }
        )
    }
} | ConvertTo-Json -Depth 10
foreach ($ep in $styleEndpoints) {
    try {
        Invoke-RestMethod -Uri $ep -Headers $hJson -Method Put -Body ([System.Text.Encoding]::UTF8.GetBytes($styleBody)) | Out-Null
        Write-Host "  ✅ Card styles OK: $ep" -ForegroundColor Green
        break
    } catch {
        Write-Host "  FAIL [$($ep.Split('?')[1])]: $($_.Exception.Message.Substring(0,60))" -ForegroundColor Red
    }
}

# Verificar WITs actuales del proyecto
Write-Host "`nWITs disponibles ahora en el proyecto:" -ForegroundColor Cyan
$wits = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitemtypes?api-version=7.0" -Headers $h
$wits.value | ForEach-Object { Write-Host "  - '$($_.name)' [color: $($_.color)]" -ForegroundColor Green }
