$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$org = 'VSCAD'; $project = 'tandem2026'; $usId = 671
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $PAT))

# 1. Subir el archivo
$filePath = 'C:\00_Tandem2026\Docs\Proyectos\Desing\US671-Locate-Termination-Points.md'
$fileName = 'US671-Locate-Termination-Points.md'
$uploadUrl = "https://dev.azure.com/$org/$project/_apis/wit/attachments?fileName=$fileName&api-version=7.0"
$uploadHeaders = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/octet-stream' }
$fileContent = [System.IO.File]::ReadAllBytes($filePath)
$uploadResult = Invoke-RestMethod -Uri $uploadUrl -Headers $uploadHeaders -Method Post -Body $fileContent
Write-Host "Archivo subido: $($uploadResult.url)" -ForegroundColor Green

# 2. Adjuntar al work item
$attachUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/$($usId)?api-version=7.0"
$attachHeaders = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }
$attachBody = '[{"op":"add","path":"/relations/-","value":{"rel":"AttachedFile","url":"' + $uploadResult.url + '","attributes":{"comment":"Documentacion implementacion US-671"}}}]'
$attachResult = Invoke-RestMethod -Uri $attachUrl -Headers $attachHeaders -Method Patch -Body $attachBody
Write-Host "Documento adjuntado a US-671" -ForegroundColor Green

# 3. Cerrar tareas #672, #673, #674
$headers = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }
$closedBody = '[{"op":"add","path":"/fields/System.State","value":"Closed"}]'
foreach ($taskId in @(672, 673, 674)) {
    $taskUrl = "https://dev.azure.com/$org/_apis/wit/workitems/$taskId?api-version=7.0"
    try {
        Invoke-RestMethod -Uri $taskUrl -Headers $headers -Method Patch -Body $closedBody -ErrorAction Stop | Out-Null
        Write-Host "Task #$taskId cerrada (Closed)" -ForegroundColor Green
    } catch {
        Write-Host "Error Task #${taskId}: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 4. Mover US-671 a Resolved
$usUrl = "https://dev.azure.com/$org/_apis/wit/workitems/$usId?api-version=7.0"
$resolvedBody = '[{"op":"add","path":"/fields/System.State","value":"Resolved"}]'
try {
    Invoke-RestMethod -Uri $usUrl -Headers $headers -Method Patch -Body $resolvedBody -ErrorAction Stop | Out-Null
    Write-Host "US-671 → Resolved (Ready to Present)" -ForegroundColor Green
} catch {
    Write-Host "Error US-671: $($_.Exception.Message)" -ForegroundColor Red
}
