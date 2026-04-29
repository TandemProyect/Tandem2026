$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$org = 'VSCAD'; $project = 'tandem2026'
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $PAT))

$usHeaders = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }
$usBody = '[
  {"op":"add","path":"/fields/System.Title","value":"Extrude Create Corner Type1"},
  {"op":"add","path":"/fields/System.State","value":"Active"},
  {"op":"add","path":"/fields/System.Description","value":"Extrude the Corner Type1 lines to create 3D geometry."}
]'
$usUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/`$User%20Story?api-version=7.0"
$us = Invoke-RestMethod -Uri $usUrl -Headers $usHeaders -Method Post -Body $usBody
$usId = $us.id
$usApiUrl = $us.url
Write-Host "US creada: #$usId - $($us.fields.'System.Title')" -ForegroundColor Green

$taskUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/`$Task?api-version=7.0"
foreach ($title in @(
    "Develop: Extrude Create Corner Type1",
    "CR: Extrude Create Corner Type1",
    "Test: Extrude Create Corner Type1"
)) {
    $taskBody = '[
      {"op":"add","path":"/fields/System.Title","value":"' + $title + '"},
      {"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"' + $usApiUrl + '","attributes":{"comment":""}}}
    ]'
    $t = Invoke-RestMethod -Uri $taskUrl -Headers $usHeaders -Method Post -Body $taskBody
    Write-Host "Task creada: #$($t.id) - $($t.fields.'System.Title')" -ForegroundColor Cyan
}
