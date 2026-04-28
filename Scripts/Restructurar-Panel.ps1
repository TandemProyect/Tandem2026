$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{ Authorization = "Basic $auth" }
$org     = "VSCAD"
$project = "tandem2026"
$team    = "tandem2026 Team"
$board   = "Stories"
$teamEncoded = [Uri]::EscapeDataString($team)

# ─────────────────────────────────────────
# PASO 1: Leer columnas actuales y sus IDs
# ─────────────────────────────────────────
$getUrl = "https://dev.azure.com/$org/$project/$teamEncoded/_apis/work/boards/$board/columns?api-version=7.0"
Write-Host "Leyendo columnas actuales..." -ForegroundColor Cyan
$current = Invoke-RestMethod -Uri $getUrl -Headers $headers -Method Get
$current.value | ForEach-Object { Write-Host "  [$($_.columnType)] '$($_.name)' -> ID: $($_.id)" -ForegroundColor Yellow }

# Guardar IDs de incoming y outgoing (son fijas, no se pueden eliminar)
$idIncoming = ($current.value | Where-Object { $_.columnType -eq "incoming" })[0].id
$idOutgoing = ($current.value | Where-Object { $_.columnType -eq "outgoing" })[0].id
Write-Host "`nIncoming ID: $idIncoming" -ForegroundColor Gray
Write-Host "Outgoing ID: $idOutgoing" -ForegroundColor Gray

# ─────────────────────────────────────────
# PASO 2: Definir nueva estructura
# ─────────────────────────────────────────
$nuevasColumnas = @(
    @{
        id           = $idIncoming
        name         = "New"
        itemLimit    = 50
        stateMappings = @{ "User Story" = "New" }
        isSplit      = $false
        description  = ""
        columnType   = "incoming"
    },
    @{
        name         = "Tareas a Analizar"
        itemLimit    = 10
        stateMappings = @{ "User Story" = "Active" }
        isSplit      = $false
        description  = ""
        columnType   = "inProgress"
    },
    @{
        name         = "Esperando documentacion"
        itemLimit    = 10
        stateMappings = @{ "User Story" = "Active" }
        isSplit      = $false
        description  = ""
        columnType   = "inProgress"
    },
    @{
        name         = "Preparado para Realizar"
        itemLimit    = 10
        stateMappings = @{ "User Story" = "Active" }
        isSplit      = $false
        description  = ""
        columnType   = "inProgress"
    },
    @{
        name         = "Realizando"
        itemLimit    = 5
        stateMappings = @{ "User Story" = "Active" }
        isSplit      = $false
        description  = ""
        columnType   = "inProgress"
    },
    @{
        name         = "Mal Testeo Volver a Realizar"
        itemLimit    = 5
        stateMappings = @{ "User Story" = "Active" }
        isSplit      = $false
        description  = ""
        columnType   = "inProgress"
    },
    @{
        name         = "Preparando a testear"
        itemLimit    = 5
        stateMappings = @{ "User Story" = "Active" }
        isSplit      = $false
        description  = ""
        columnType   = "inProgress"
    },
    @{
        name         = "Preparado para presentar"
        itemLimit    = 10
        stateMappings = @{ "User Story" = "Resolved" }
        isSplit      = $false
        description  = ""
        columnType   = "inProgress"
    },
    @{
        id           = $idOutgoing
        name         = "Closed"
        itemLimit    = 300
        stateMappings = @{ "User Story" = "Closed" }
        isSplit      = $false
        description  = ""
        columnType   = "outgoing"
    }
)

# ─────────────────────────────────────────
# PASO 3: PUT con la nueva estructura
# ─────────────────────────────────────────
$putUrl = "https://dev.azure.com/$org/$project/$teamEncoded/_apis/work/boards/$board/columns?api-version=7.0"
$body   = $nuevasColumnas | ConvertTo-Json -Depth 10

Write-Host "`nAplicando nueva estructura de columnas..." -ForegroundColor Cyan
try {
    $resultado = Invoke-RestMethod -Uri $putUrl -Headers ($headers + @{"Content-Type"="application/json"}) -Method Put -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) -ContentType "application/json; charset=utf-8"
    Write-Host "`n✅ Panel reestructurado correctamente!" -ForegroundColor Green
    Write-Host "Columnas aplicadas:" -ForegroundColor Cyan
    $resultado | ForEach-Object { Write-Host "  [$($_.columnType)] '$($_.name)' (WIP: $($_.itemLimit))" -ForegroundColor Green }
} catch {
    Write-Host "`n❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Respuesta: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

Write-Host "`nPanel: https://dev.azure.com/$org/$project/_boards/board/t/$teamEncoded/$board" -ForegroundColor Gray
