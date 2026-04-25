# Script: Attach-Document.ps1
# Adjunta un documento a un Work Item en Azure DevOps

param(
	[Parameter(Mandatory=$true)]
	[int]$WorkItemId,

	[Parameter(Mandatory=$true)]
	[string]$FilePath,

	[Parameter(Mandatory=$false)]
	[string]$Comment = "Documentación adjunta"
)

# Configuración
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$org = "VSCAD"
$project = "tandem2026"

$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))

# Verificar que el archivo existe
if (-not (Test-Path $FilePath)) {
	Write-Host "❌ Error: El archivo '$FilePath' no existe." -ForegroundColor Red
	exit 1
}

$fileName = Split-Path $FilePath -Leaf
$fileBytes = [System.IO.File]::ReadAllBytes($FilePath)
$fileSize = (Get-Item $FilePath).Length

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "📎 Adjuntando Documento a Work Item" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Work Item: #$WorkItemId" -ForegroundColor White
Write-Host "Archivo: $fileName" -ForegroundColor White
Write-Host "Tamaño: $([math]::Round($fileSize/1KB, 2)) KB" -ForegroundColor White

# ========================================
# PASO 1: Subir archivo al área temporal
# ========================================
Write-Host "`n[1/2] Subiendo archivo..." -ForegroundColor Yellow

$uploadUrl = "https://dev.azure.com/$org/$project/_apis/wit/attachments?fileName=$fileName&api-version=7.0"
$uploadHeaders = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/octet-stream"
}

try {
	$uploadResult = Invoke-RestMethod -Uri $uploadUrl -Headers $uploadHeaders -Method Post -Body $fileBytes
	Write-Host "✅ Archivo subido exitosamente" -ForegroundColor Green
	Write-Host "   URL: $($uploadResult.url)" -ForegroundColor Gray
} catch {
	Write-Host "❌ Error subiendo archivo: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# ========================================
# PASO 2: Adjuntar archivo al Work Item
# ========================================
Write-Host "`n[2/2] Adjuntando al Work Item #$WorkItemId..." -ForegroundColor Yellow

$attachUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/$WorkItemId?api-version=7.0"
$attachHeaders = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

$attachPayload = @(
	@{
		op = "add"
		path = "/relations/-"
		value = @{
			rel = "AttachedFile"
			url = $uploadResult.url
			attributes = @{
				comment = $Comment
			}
		}
	}
) | ConvertTo-Json -Depth 10

try {
	$result = Invoke-RestMethod -Uri $attachUrl -Headers $attachHeaders -Method Patch -Body $attachPayload
	Write-Host "✅ Documento adjuntado exitosamente" -ForegroundColor Green

	# Contar attachments
	$attachments = $result.relations | Where-Object { $_.rel -eq "AttachedFile" }
	Write-Host "   Total de archivos adjuntos: $($attachments.Count)" -ForegroundColor Cyan

} catch {
	Write-Host "❌ Error adjuntando archivo: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# ========================================
# RESUMEN FINAL
# ========================================
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ PROCESO COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "`n📎 Archivo adjuntado:" -ForegroundColor Cyan
Write-Host "   Nombre: $fileName" -ForegroundColor White
Write-Host "   Tamaño: $([math]::Round($fileSize/1KB, 2)) KB" -ForegroundColor White
Write-Host "   Work Item: #$WorkItemId" -ForegroundColor White
Write-Host "   Comentario: $Comment" -ForegroundColor White

Write-Host "`n🔗 Ver en Azure DevOps:" -ForegroundColor Cyan
$wiUrl = "https://dev.azure.com/$org/$project/_workitems/edit/$WorkItemId"
Write-Host "   $wiUrl" -ForegroundColor Blue

Write-Host ""  # Línea en blanco final
