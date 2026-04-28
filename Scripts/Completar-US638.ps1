$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
    Authorization = "Basic $auth"
    "Content-Type" = "application/json-patch+json; charset=utf-8"
}
$usId = 638
$urlTask = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
$urlUS   = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$usId`?api-version=7.0"

# 1. Mover US #638 a Done
$bodyDone = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
Invoke-RestMethod -Uri $urlUS -Headers $headers -Method Patch -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyDone)) -ContentType "application/json-patch+json; charset=utf-8" | Out-Null
Write-Host "✅ US #638 movida a Done" -ForegroundColor Green

# 2. Task Test - crear primero sin estado
$payloadTest = @(
    @{op="add"; path="/fields/System.Title"; value="Test"},
    @{op="add"; path="/fields/System.WorkItemType"; value="Task"},
    @{op="add"; path="/fields/System.Description"; value="Verificacion en ZWCAD: seleccion de 4 lineas en esquina L. Resultado: circulo azul en punto interior correcto, circulo rojo en punto exterior correcto."},
    @{op="add"; path="/relations/-"; value=@{rel="System.LinkTypes.Hierarchy-Reverse"; url="https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$usId"}}
) | ConvertTo-Json -Depth 10
$resTest = Invoke-RestMethod -Uri $urlTask -Headers $headers -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($payloadTest)) -ContentType "application/json-patch+json; charset=utf-8"
Write-Host "✅ Task Test creada: #$($resTest.id)" -ForegroundColor Green

# Mover Task Test a Done
$urlTestPatch = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$($resTest.id)?api-version=7.0"
$bodyTaskDone = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
Invoke-RestMethod -Uri $urlTestPatch -Headers $headers -Method Patch -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyTaskDone)) -ContentType "application/json-patch+json; charset=utf-8" | Out-Null
Write-Host "✅ Task Test #$($resTest.id) marcada Done" -ForegroundColor Green

# 3. Task CR - crear primero sin estado
$payloadCR = @(
    @{op="add"; path="/fields/System.Title"; value="CR"},
    @{op="add"; path="/fields/System.WorkItemType"; value="Task"},
    @{op="add"; path="/fields/System.Description"; value="Code Review: LCornerDetector.cs - metodo CalcularPuntosEsquinaL. Logica correcta de interseccion geometrica para punto interior/exterior de esquina L."},
    @{op="add"; path="/relations/-"; value=@{rel="System.LinkTypes.Hierarchy-Reverse"; url="https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$usId"}}
) | ConvertTo-Json -Depth 10
$resCR = Invoke-RestMethod -Uri $urlTask -Headers $headers -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($payloadCR)) -ContentType "application/json-patch+json; charset=utf-8"
Write-Host "✅ Task CR creada: #$($resCR.id)" -ForegroundColor Green

# Mover Task CR a Done
$urlCRPatch = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$($resCR.id)?api-version=7.0"
Invoke-RestMethod -Uri $urlCRPatch -Headers $headers -Method Patch -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyTaskDone)) -ContentType "application/json-patch+json; charset=utf-8" | Out-Null
Write-Host "✅ Task CR #$($resCR.id) marcada Done" -ForegroundColor Green

# 4. Adjuntar documentacion
$docPath = "C:\00_Tandem2026\Docs\US\US-638-CORRECCION-PUNTO-INTERIOR-ESQUINA-L.md"
$uploadUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/attachments?fileName=US-638-CORRECCION-PUNTO-INTERIOR-ESQUINA-L.md&api-version=7.0"
$headersUpload = @{ Authorization = "Basic $auth" }
$uploadResult = Invoke-RestMethod -Uri $uploadUrl -Headers $headersUpload -Method Post -InFile $docPath -ContentType "application/octet-stream"

$bodyAttach = ConvertTo-Json -Depth 10 @(@{op="add"; path="/relations/-"; value=@{rel="AttachedFile"; url=$uploadResult.url; attributes=@{comment="Documentacion tecnica US-638: correccion deteccion punto interior esquina L"}}})
Invoke-RestMethod -Uri $urlUS -Headers $headers -Method Patch -Body ([System.Text.Encoding]::UTF8.GetBytes($bodyAttach)) -ContentType "application/json-patch+json; charset=utf-8" | Out-Null
Write-Host "✅ Documentacion adjuntada a US #638" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "US #638 completada" -ForegroundColor Green
Write-Host "Tasks: Test #$($resTest.id) (Done) | CR #$($resCR.id) (Done)" -ForegroundColor Cyan
Write-Host "URL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/$usId" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan
