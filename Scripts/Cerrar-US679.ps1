$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$org = 'VSCAD'; $project = 'tandem2026'; $usId = 679
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $PAT))

# 1. Subir y adjuntar doc
$filePath = 'C:\00_Tandem2026\Docs\Proyectos\Desing\US679-Extrude-Create-Corner-Type1.md'
$fileName = 'US679-Extrude-Create-Corner-Type1.md'
$uploadUrl = "https://dev.azure.com/$org/$project/_apis/wit/attachments?fileName=$fileName&api-version=7.0"
$uploadHeaders = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/octet-stream' }
$fileContent = [System.IO.File]::ReadAllBytes($filePath)
$uploadResult = Invoke-RestMethod -Uri $uploadUrl -Headers $uploadHeaders -Method Post -Body $fileContent
Write-Host "Archivo subido: $($uploadResult.url)" -ForegroundColor Green

$attachUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/$($usId)?api-version=7.0"
$attachHeaders = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }
$attachBody = '[{"op":"add","path":"/relations/-","value":{"rel":"AttachedFile","url":"' + $uploadResult.url + '","attributes":{"comment":"Documentacion US-679"}}}]'
Invoke-RestMethod -Uri $attachUrl -Headers $attachHeaders -Method Patch -Body $attachBody | Out-Null
Write-Host "Documento adjuntado a US-679" -ForegroundColor Green

# 2. Cerrar tareas
$headers = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }
$closedBody = '[{"op":"add","path":"/fields/System.State","value":"Closed"}]'
foreach ($taskId in @(680, 681, 682)) {
    $url = "https://dev.azure.com/$org/$project/_apis/wit/workitems/$taskId`?api-version=7.0"
    try {
        Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $closedBody -ErrorAction Stop | Out-Null
        Write-Host "Task #$taskId → Closed" -ForegroundColor Green
    } catch {
        Write-Host "Error Task #${taskId}: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 3. Resolver US
$resolvedBody = '[{"op":"add","path":"/fields/System.State","value":"Resolved"}]'
$usUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/$usId`?api-version=7.0"
try {
    Invoke-RestMethod -Uri $usUrl -Headers $headers -Method Patch -Body $resolvedBody -ErrorAction Stop | Out-Null
    Write-Host "US-679 → Resolved" -ForegroundColor Green
} catch {
    Write-Host "Error US-679: $($_.Exception.Message)" -ForegroundColor Red
}
