$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$org = 'VSCAD'; $project = 'tandem2026'
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $PAT))

# 1. Crear US
$usHeaders = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }
$usBody = '[
  {"op":"add","path":"/fields/System.Title","value":"Create Corner Type1 Lines DB"},
  {"op":"add","path":"/fields/System.State","value":"Active"},
  {"op":"add","path":"/fields/System.Description","value":"Create the corner type 1 lines and persist them in the database."}
]'
$usUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/`$User%20Story?api-version=7.0"
$us = Invoke-RestMethod -Uri $usUrl -Headers $usHeaders -Method Post -Body $usBody
$usId = $us.id
$usApiUrl = $us.url
Write-Host "US creada: #$usId - $($us.fields.'System.Title')" -ForegroundColor Green

# 2. Crear tareas hijas
$taskHeaders = @{ Authorization = "Basic $auth"; 'Content-Type' = 'application/json-patch+json' }
$taskUrl = "https://dev.azure.com/$org/$project/_apis/wit/workitems/`$Task?api-version=7.0"

foreach ($task in @(
    @{ Title = "Develop: Create Corner Type1 Lines DB" },
    @{ Title = "CR: Create Corner Type1 Lines DB" },
    @{ Title = "Test: Create Corner Type1 Lines DB" }
)) {
    $taskBody = '[
      {"op":"add","path":"/fields/System.Title","value":"' + $task.Title + '"},
      {"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"' + $usApiUrl + '","attributes":{"comment":""}}}
    ]'
    $t = Invoke-RestMethod -Uri $taskUrl -Headers $taskHeaders -Method Post -Body $taskBody
    Write-Host "Task creada: #$($t.id) - $($t.fields.'System.Title')" -ForegroundColor Cyan
}
