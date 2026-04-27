# Script simplificado para adjuntar documentación a US-619
param(
	[int]$WorkItemId = 619,
	[string]$FilePath = "C:\00_Tandem2026\AGENTE-US619-INFO.md"
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$org = "VSCAD"
$project = "tandem2026"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))

if (-not (Test-Path $FilePath)) {
	Write-Host "Error: Archivo no encontrado: $FilePath" -ForegroundColor Red
	exit 1
}

$fileName = Split-Path $FilePath -Leaf
$fileBytes = [System.IO.File]::ReadAllBytes($FilePath)

Write-Host "Subiendo archivo..." -ForegroundColor Yellow
$uploadUrl = "https://dev.azure.com/$org/$project/_apis/wit/attachments?fileName=$fileName&api-version=7.0"
$uploadHeaders = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/octet-stream"
}

try {
	$uploadResult = Invoke-RestMethod -Uri $uploadUrl -Headers $uploadHeaders -Method Post -Body $fileBytes
	Write-Host "Archivo subido" -ForegroundColor Green
} catch {
	Write-Host "Error subiendo archivo: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

Write-Host "Adjuntando a Work Item #$WorkItemId..." -ForegroundColor Yellow
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
				comment = "Documentacion completa de la US-619 y sistema ATK60"
			}
		}
	}
) | ConvertTo-Json -Depth 10

try {
	$result = Invoke-RestMethod -Uri $attachUrl -Headers $attachHeaders -Method Patch -Body $attachPayload
	Write-Host "Documento adjuntado exitosamente" -ForegroundColor Green
	$attachments = $result.relations | Where-Object { $_.rel -eq "AttachedFile" }
	Write-Host "Total de archivos adjuntos: $($attachments.Count)" -ForegroundColor Cyan
} catch {
	Write-Host "Error adjuntando archivo: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}
