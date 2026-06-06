# Health Check - Verificacion rapida del proyecto
# Uso: .\Scripts\HealthCheck.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " TANDEM 2026 - HEALTH CHECK" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# 1. Verificar repositorio Git
Write-Host "[1/5] Verificando repositorio Git..." -ForegroundColor Yellow
try {
	$gitStatus = git status 2>&1
	if ($LASTEXITCODE -eq 0) {
		$branch = git branch --show-current
		Write-Host "  OK - Branch: $branch" -ForegroundColor Green
	} else {
		Write-Host "  ERROR - No es un repositorio Git" -ForegroundColor Red
		$allGood = $false
	}
} catch {
	Write-Host "  ERROR - Git no disponible" -ForegroundColor Red
	$allGood = $false
}
Write-Host ""

# 2. Verificar documentacion critica
Write-Host "[2/5] Verificando documentacion..." -ForegroundColor Yellow
$docFolder = Get-ChildItem -Directory -ErrorAction SilentlyContinue |
	Where-Object { $_.Name -like 'documentaci*' -and $_.Name -ne 'Docs' } |
	Select-Object -First 1
$continuityDoc = if ($docFolder) {
	Join-Path $docFolder.Name 'CONTINUITY.md'
} else {
	'documentacion\CONTINUITY.md'
}
$criticalDocs = @(
	$continuityDoc,
	'README.md',
	'Docs\README.md',
	'Docs\General\Azure-DevOps.md'
)
$docsOK = $true
foreach ($doc in $criticalDocs) {
	if (Test-Path $doc) {
		Write-Host "  OK - $doc" -ForegroundColor Green
	} else {
		Write-Host "  MISSING - $doc" -ForegroundColor Red
		$docsOK = $false
		$allGood = $false
	}
}
if ($docsOK) {
	Write-Host "  Documentacion completa" -ForegroundColor Green
}
Write-Host ""

# 3. Verificar scripts
Write-Host "[3/5] Verificando scripts..." -ForegroundColor Yellow
$criticalScripts = @(
	"Scripts\US.ps1",
	"Scripts\Edit-US.ps1",
	"Scripts\Verificar-Board.ps1"
)
$scriptsOK = $true
foreach ($script in $criticalScripts) {
	if (Test-Path $script) {
		Write-Host "  OK - $script" -ForegroundColor Green
	} else {
		Write-Host "  MISSING - $script" -ForegroundColor Red
		$scriptsOK = $false
		$allGood = $false
	}
}
if ($scriptsOK) {
	Write-Host "  Scripts criticos presentes" -ForegroundColor Green
}
Write-Host ""

# 4. Verificar Azure DevOps (si PAT disponible)
Write-Host "[4/5] Verificando Azure DevOps..." -ForegroundColor Yellow
if (Test-Path "Scripts\US.ps1") {
	$patLine = Get-Content "Scripts\US.ps1" | Select-String '$PAT = ' | Select-Object -First 1
	if ($patLine) {
		Write-Host "  OK - PAT encontrado en US.ps1" -ForegroundColor Green
		try {
			$PAT = ($patLine -split '"')[1]
			$base64 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
			$headers = @{Authorization = "Basic $base64"}
			$result = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/_apis/projects/tandem2026?api-version=7.1" -Method Get -Headers $headers -TimeoutSec 5
			Write-Host "  OK - Conexion a Azure DevOps exitosa" -ForegroundColor Green
		} catch {
			Write-Host "  WARNING - No se pudo conectar a Azure DevOps" -ForegroundColor Yellow
			Write-Host "  (Puede ser normal si no hay internet)" -ForegroundColor Gray
		}
	} else {
		Write-Host "  WARNING - PAT no encontrado" -ForegroundColor Yellow
		$allGood = $false
	}
} else {
	Write-Host "  SKIP - Scripts no disponibles" -ForegroundColor Gray
}
Write-Host ""

# 5. Verificar solucion Visual Studio
Write-Host "[5/5] Verificando solucion..." -ForegroundColor Yellow
$slnFiles = Get-ChildItem -Filter *.sln -File
if ($slnFiles.Count -gt 0) {
	Write-Host "  OK - Solucion encontrada: $($slnFiles[0].Name)" -ForegroundColor Green
} else {
	Write-Host "  WARNING - No se encontro archivo .sln" -ForegroundColor Yellow
}
Write-Host ""

# Resumen final
Write-Host "========================================" -ForegroundColor Cyan
if ($allGood) {
	Write-Host " RESULTADO: TODO OK" -ForegroundColor Green
	Write-Host ""
	Write-Host "El proyecto esta listo para trabajar." -ForegroundColor Green
	Write-Host "Lee $continuityDoc para entender el contexto." -ForegroundColor Cyan
} else {
	Write-Host " RESULTADO: REVISAR WARNINGS/ERRORES" -ForegroundColor Yellow
	Write-Host ""
	Write-Host "Algunos componentes no estan disponibles." -ForegroundColor Yellow
	Write-Host "Revisa los mensajes arriba para detalles." -ForegroundColor Yellow
}
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Informacion adicional
Write-Host "Proximos pasos sugeridos:" -ForegroundColor Cyan
Write-Host "  1. Leer $continuityDoc" -ForegroundColor White
Write-Host "  2. Ejecutar .\Scripts\Verificar-Board.ps1" -ForegroundColor White
Write-Host "  3. Revisar git log --oneline -10" -ForegroundColor White
Write-Host ""
