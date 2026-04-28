$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }
$hJson = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json; charset=utf-8" }
$basicProcessId = "b8a3a935-7e91-48b8-a94c-606d37c3e9f2"

# ─────────────────────────────────────────────────────────────────
# PASO 1: Crear proceso heredado "Tandem2026" basado en Basic
# ─────────────────────────────────────────────────────────────────
Write-Host "PASO 1: Creando proceso heredado Tandem2026..." -ForegroundColor Cyan

# Verificar si ya existe
$procesos = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/work/processes?api-version=7.1-preview.2" -Headers $h
$existente = $procesos.value | Where-Object { $_.name -eq "Tandem2026" }

if ($existente) {
    $processId = $existente.typeId
    Write-Host "  Proceso Tandem2026 ya existe. ID: $processId" -ForegroundColor Yellow
} else {
    $bodyProceso = @{
        name               = "Tandem2026"
        description        = "Proceso personalizado Tandem 2026 - Basic + Bug"
        parentProcessTypeId = $basicProcessId
        referenceName      = "Tandem2026.Process"
    } | ConvertTo-Json
    $nuevoProceso = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/work/processes?api-version=7.1-preview.2" -Headers $hJson -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyProceso))
    $processId = $nuevoProceso.typeId
    Write-Host "  ✅ Proceso creado. ID: $processId" -ForegroundColor Green
}

# ─────────────────────────────────────────────────────────────────
# PASO 2: Añadir tipo "Bug" al proceso Tandem2026
# ─────────────────────────────────────────────────────────────────
Write-Host "`nPASO 2: Añadiendo WIT Bug al proceso..." -ForegroundColor Cyan

# Verificar si Bug ya existe en el proceso
$wits = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/work/processes/$processId/workItemTypes?api-version=7.1-preview.2" -Headers $h
$bugExistente = $wits.value | Where-Object { $_.name -eq "Bug" }

if ($bugExistente) {
    Write-Host "  Bug WIT ya existe. Ref: $($bugExistente.referenceName)" -ForegroundColor Yellow
    $bugRef = $bugExistente.referenceName
} else {
    $bodyBug = @{
        name        = "Bug"
        description = "Defecto o error encontrado en el sistema"
        color       = "CC293D"
        icon        = "icon_insect"
        isDisabled  = $false
    } | ConvertTo-Json
    $bugWit = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/work/processes/$processId/workItemTypes?api-version=7.1-preview.2" -Headers $hJson -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyBug))
    $bugRef = $bugWit.referenceName
    Write-Host "  ✅ Bug WIT creado. Ref: $bugRef" -ForegroundColor Green
}

# ─────────────────────────────────────────────────────────────────
# PASO 3: Cambiar el proyecto tandem2026 al nuevo proceso
# ─────────────────────────────────────────────────────────────────
Write-Host "`nPASO 3: Actualizando proyecto al proceso Tandem2026..." -ForegroundColor Cyan

$bodyProj = @{
    capabilities = @{
        processTemplate = @{
            templateTypeId = $processId
        }
    }
} | ConvertTo-Json -Depth 5

try {
    Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/tandem2026?api-version=7.0" -Headers $hJson -Method Patch -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyProj)) | Out-Null
    Write-Host "  ✅ Proyecto actualizado al proceso Tandem2026" -ForegroundColor Green
} catch {
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Detalle: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

# ─────────────────────────────────────────────────────────────────
# PASO 4: Obtener board ID e intentar card styles
# ─────────────────────────────────────────────────────────────────
Write-Host "`nPASO 4: Obteniendo board ID para configurar colores..." -ForegroundColor Cyan

$boards = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards?api-version=7.0" -Headers $h
$boards.value | ForEach-Object { Write-Host "  Board: '$($_.name)' ID: $($_.id)" -ForegroundColor Yellow }

$boardId = ($boards.value | Where-Object { $_.name -eq "Issues" })[0].id
Write-Host "  Board ID: $boardId" -ForegroundColor Gray

# Intentar card style con ID del board
Write-Host "`nPASO 5: Configurando colores por tipo (Bug=rojo, Issue=azul)..." -ForegroundColor Cyan
$styleUrls = @(
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/$boardId/cardstylesettings?api-version=7.1-preview.1",
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Issues/cardstylesettings?api-version=7.1-preview.1"
)

$styleBody = @{
    rules = @{
        fill = @(
            @{
                name          = "Bug - Rojo"
                filter        = "[System.WorkItemType] = 'Bug'"
                isEnabled     = $true
                settings      = @{ "background-color" = "#CC293D"; "title-color" = "#FFFFFF" }
            },
            @{
                name          = "ZwcadPlugin - Verde"
                filter        = "[System.Tags] Contains 'ZwcadPlugin'"
                isEnabled     = $true
                settings      = @{ "background-color" = "#339933"; "title-color" = "#FFFFFF" }
            },
            @{
                name          = "Desing - Azul"
                filter        = "[System.Tags] Contains 'Desing'"
                isEnabled     = $true
                settings      = @{ "background-color" = "#0078D4"; "title-color" = "#FFFFFF" }
            },
            @{
                name          = "DAL - Naranja"
                filter        = "[System.Tags] Contains 'DAL'"
                isEnabled     = $true
                settings      = @{ "background-color" = "#E17D00"; "title-color" = "#FFFFFF" }
            }
        )
    }
} | ConvertTo-Json -Depth 10

$styleOk = $false
foreach ($url in $styleUrls) {
    try {
        Invoke-RestMethod -Uri $url -Headers $hJson -Method Put -Body ([System.Text.Encoding]::UTF8.GetBytes($styleBody)) | Out-Null
        Write-Host "  ✅ Colores configurados via: $url" -ForegroundColor Green
        $styleOk = $true
        break
    } catch {
        Write-Host "  FAIL: $($_.Exception.Message.Substring(0,[Math]::Min(80,$_.Exception.Message.Length)))" -ForegroundColor Red
    }
}

if (-not $styleOk) {
    Write-Host "`n  ⚠️  Los colores deben configurarse manualmente:" -ForegroundColor Yellow
    Write-Host "  1. Abre el panel: https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues" -ForegroundColor White
    Write-Host "  2. Clic en ⚙️ (arriba derecha) → 'Styling'" -ForegroundColor White
    Write-Host "  3. 'Add styling rule' y configura:" -ForegroundColor White
    Write-Host "     - Bug: Work Item Type = Bug → color rojo (#CC293D)" -ForegroundColor White
    Write-Host "     - ZwcadPlugin: Tags Contains ZwcadPlugin → verde (#339933)" -ForegroundColor White
    Write-Host "     - Desing: Tags Contains Desing → azul (#0078D4)" -ForegroundColor White
    Write-Host "     - DAL: Tags Contains DAL → naranja (#E17D00)" -ForegroundColor White
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Proceso Tandem2026 ID: $processId" -ForegroundColor White
Write-Host "Bug WIT ref: $bugRef" -ForegroundColor White
Write-Host "Para crear un Bug usa el tipo 'Bug' (rojo) en lugar de 'Issue'" -ForegroundColor White
Write-Host "Para identificar proyecto usa Tags: ZwcadPlugin / Desing / DAL" -ForegroundColor White
Write-Host "Panel: https://dev.azure.com/VSCAD/tandem2026/_workitems/create/Bug" -ForegroundColor Gray
