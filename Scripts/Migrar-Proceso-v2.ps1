$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }
$hJson = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json; charset=utf-8" }

$newProcessId = "c36c639d-de0d-456d-bc7e-fa8384e4f950"   # Tandem2026
$oldProcessId = "b8a3a935-7e91-48b8-a94c-606d37c3e9f2"   # Basic
$projectId    = "213253e7-f177-4e2d-bdf3-410b97f6883d"    # tandem2026

# Verificar proceso actual del proyecto
Write-Host "=== ESTADO ACTUAL ===" -ForegroundColor Cyan
$proc = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/$projectId`?includeCapabilities=true&api-version=7.0" -Headers $h
Write-Host "Proceso actual: $($proc.capabilities.processTemplate.templateName) [$($proc.capabilities.processTemplate.templateTypeId)]" -ForegroundColor Yellow

# Ver si el proceso Tandem2026 ya tiene Bug en los WITs del proceso
Write-Host "`n=== WITs EN PROCESO Tandem2026 ===" -ForegroundColor Cyan
$procWits = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/work/processes/$newProcessId/workItemTypes?api-version=7.1-preview.2" -Headers $h
$procWits.value | ForEach-Object { Write-Host "  - $($_.name) [ref: $($_.referenceName), color: $($_.color)]" -ForegroundColor Green }

# Intentar migración con todos los campos posibles
Write-Host "`n=== INTENTANDO MIGRACIÓN ===" -ForegroundColor Cyan
$attempts = @(
    @{
        desc = "Con sourceProcessTypeId+targetProcessTypeId+projectId"
        body = @{ sourceProcessTypeId=$oldProcessId; targetProcessTypeId=$newProcessId; projectId=$projectId }
    },
    @{
        desc = "Solo targetProcessTypeId+projectId"
        body = @{ targetProcessTypeId=$newProcessId; projectId=$projectId }
    },
    @{
        desc = "Con name+targetProcessTypeId+projectId"
        body = @{ name="Tandem2026"; targetProcessTypeId=$newProcessId; projectId=$projectId }
    }
)

foreach ($attempt in $attempts) {
    Write-Host "`n  Intento: $($attempt.desc)" -ForegroundColor Cyan
    try {
        $b = $attempt.body | ConvertTo-Json
        $r = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/work/processes/migrations?api-version=7.1-preview.1" -Headers $hJson -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($b))
        Write-Host "  ✅ Éxito: $($r | ConvertTo-Json)" -ForegroundColor Green
        break
    } catch {
        $msg = $_.ErrorDetails.Message
        Write-Host "  FAIL: $msg" -ForegroundColor Red
    }
}

# Intentar rutas alternativas para card styles
Write-Host "`n=== CARD STYLES - RUTAS ALTERNATIVAS ===" -ForegroundColor Cyan
$boardId = "892fa957-9c33-4237-a99f-2660bd9ec80d"
$teamId  = $null
# Obtener team ID
$teams = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/$projectId/teams?api-version=7.0" -Headers $h
$teams.value | ForEach-Object { Write-Host "  Team: '$($_.name)' ID: $($_.id)" -ForegroundColor Gray }
$teamId = ($teams.value | Where-Object { $_.name -eq "tandem2026 Team" })[0].id

$styleRoutes = @(
    "https://dev.azure.com/VSCAD/tandem2026/$teamId/_apis/work/boards/$boardId/cardstylesettings?api-version=7.0",
    "https://dev.azure.com/VSCAD/$projectId/$teamId/_apis/work/boards/$boardId/cardstylesettings?api-version=7.0",
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/$boardId/cardstylesettings?api-version=7.0"
)
foreach ($ep in $styleRoutes) {
    try {
        $r = Invoke-RestMethod -Uri $ep -Headers $h -Method Get
        Write-Host "  ✅ GET OK: $ep" -ForegroundColor Green
        Write-Host "  Contenido: $($r | ConvertTo-Json -Depth 3 | Select-Object -First 20)" -ForegroundColor Gray
        break
    } catch {
        $errMsg = $_.Exception.Message; if ($errMsg.Length -gt 70) { $errMsg = $errMsg.Substring(0,70) }
        Write-Host "  FAIL GET: $errMsg" -ForegroundColor Red
    }
}
