# Script Final - Configurar Columnas Board Tandem 2026
# Ejecutar: .\Scripts\Configurar-Board-Final.ps1

$pat = $env:AZDO_PAT
if (-not $pat) { throw "AZDO_PAT no configurado en variables de entorno." }
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "",$pat)))
$headers = @{
	Authorization = ("Basic {0}" -f $base64AuthInfo)
	"Content-Type" = "application/json"
}

$teamId = "2ea0799c-57e5-48f6-87dd-f9eb6c232196"
$boardUrl = "https://dev.azure.com/VSCAD/tandem2026/$teamId/_apis/work/boards/Issues?api-version=7.1-preview.1"

Write-Host "=== Configurando Board Tandem 2026 ===" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Obtener configuracion actual
Write-Host "1. Obteniendo configuracion actual..." -ForegroundColor Yellow
try {
	$currentBoard = Invoke-RestMethod -Uri $boardUrl -Method Get -Headers $headers
	Write-Host "   Columnas actuales:" -ForegroundColor Green
	$currentBoard.columns | ForEach-Object {
		Write-Host "   - $($_.name)" -ForegroundColor White
	}
	Write-Host ""
} catch {
	Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# Paso 2: Definir nuevas columnas
Write-Host "2. Definiendo nuevas columnas..." -ForegroundColor Yellow

$newColumns = @(
	@{
		name = "New"
		stateMappings = @{ "Issue" = "New" }
		columnType = "incoming"
		isSplit = $false
		wipLimit = 50
	},
	@{
		name = "Tareas a Analizar"
		stateMappings = @{ "Issue" = "Active" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 10
	},
	@{
		name = "Esperando documentacion"
		stateMappings = @{ "Issue" = "Active" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 10
	},
	@{
		name = "Preparado para Realizar"
		stateMappings = @{ "Issue" = "Active" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 10
	},
	@{
		name = "Realizando"
		stateMappings = @{ "Issue" = "Active" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 5
	},
	@{
		name = "Mal Testeo Volver a Realizar"
		stateMappings = @{ "Issue" = "Active" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 5
	},
	@{
		name = "Preparando a testear"
		stateMappings = @{ "Issue" = "Active" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 5
	},
	@{
		name = "Preparado para presentar"
		stateMappings = @{ "Issue" = "Resolved" }
		columnType = "inProgress"
		isSplit = $false
		wipLimit = 10
	},
	@{
		name = "Closed"
		stateMappings = @{ "Issue" = "Closed" }
		columnType = "outgoing"
		isSplit = $false
		wipLimit = 300
	}
)

Write-Host "   Nuevas columnas:" -ForegroundColor Green
$newColumns | ForEach-Object {
	Write-Host "   - $($_.name) (WIP: $($_.wipLimit))" -ForegroundColor White
}
Write-Host ""

# Paso 3: Confirmar
$confirm = Read-Host "Aplicar cambios? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
	Write-Host "Cancelado" -ForegroundColor Yellow
	exit 0
}

# Paso 4: Aplicar cambios
Write-Host ""
Write-Host "3. Aplicando cambios al board..." -ForegroundColor Yellow

$payload = @{
	columns = $newColumns
} | ConvertTo-Json -Depth 10

try {
	$result = Invoke-RestMethod -Uri $boardUrl -Method Put -Headers $headers -Body $payload
	Write-Host "   Exito! Columnas configuradas" -ForegroundColor Green
	Write-Host ""
	Write-Host "Columnas finales:" -ForegroundColor Cyan
	$result.columns | ForEach-Object {
		Write-Host "  - $($_.name) (WIP: $($_.wipLimit))" -ForegroundColor White
	}
	Write-Host ""
	Write-Host "Ver board en:" -ForegroundColor Cyan
	Write-Host "https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues" -ForegroundColor White
} catch {
	Write-Host "   Error al aplicar: $($_.Exception.Message)" -ForegroundColor Red
	if ($_.Exception.Response) {
		$reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
		$responseBody = $reader.ReadToEnd()
		Write-Host "   Detalles: $responseBody" -ForegroundColor Red
	}
	exit 1
}
