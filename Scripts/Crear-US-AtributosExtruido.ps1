$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$org = 'VSCAD'; $project = 'tandem2026'
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $PAT))
$headers = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }

# 1. Crear US
$usBody = '[
  {"op":"add","path":"/fields/System.Title","value":"Add Attributes to Extruded Corner Type1 Object"},
  {"op":"add","path":"/fields/System.State","value":"Active"},
  {"op":"add","path":"/fields/System.Description","value":"Add metadata attributes (XData or Block attributes) to the extruded corner L object in layer ModelDesing. Decide between XData (internal) or Block with AttributeDefinition (visible in ZWCAD). See attached documentation for options and next steps."}
]'
$usUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/`$User%20Story?api-version=7.0"
$us = Invoke-RestMethod -Uri $usUrl -Headers $headers -Method Post -Body $usBody
$usId = $us.id
$usApiUrl = $us.url
Write-Host "US creada: #$usId - $($us.fields.'System.Title')" -ForegroundColor Green

# 2. Crear tareas
$taskUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/`$Task?api-version=7.0"
foreach ($title in @(
    "Develop: Add Attributes to Extruded Corner Type1 Object",
    "CR: Add Attributes to Extruded Corner Type1 Object",
    "Test: Add Attributes to Extruded Corner Type1 Object"
)) {
    $taskBody = '[
      {"op":"add","path":"/fields/System.Title","value":"' + $title + '"},
      {"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"' + $usApiUrl + '","attributes":{"comment":""}}}
    ]'
    $t = Invoke-RestMethod -Uri $taskUrl -Headers $headers -Method Post -Body $taskBody
    Write-Host "Task creada: #$($t.id) - $($t.fields.'System.Title')" -ForegroundColor Cyan
}

# 3. Adjuntar documentación
$filePath = 'C:\00_Tandem2026\Docs\Proyectos\Desing\NEXT-Atributos-Objeto-Extruido.md'
$fileName = 'NEXT-Atributos-Objeto-Extruido.md'
$uploadUrl = "https://dev.azure.com/$org/$project/_apis/wit/attachments?fileName=$fileName&api-version=7.0"
$uploadHeaders = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/octet-stream' }
$fileContent = [System.IO.File]::ReadAllBytes($filePath)
$uploadResult = Invoke-RestMethod -Uri $uploadUrl -Headers $uploadHeaders -Method Post -Body $fileContent
Write-Host "Archivo subido: $($uploadResult.url)" -ForegroundColor Green

$attachUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/$($usId)?api-version=7.0"
$attachBody = '[{"op":"add","path":"/relations/-","value":{"rel":"AttachedFile","url":"' + $uploadResult.url + '","attributes":{"comment":"Documentacion proximo paso - atributos objeto extruido"}}}]'
Invoke-RestMethod -Uri $attachUrl -Headers $headers -Method Patch -Body $attachBody | Out-Null
Write-Host "Documento adjuntado a US-$usId" -ForegroundColor Green
