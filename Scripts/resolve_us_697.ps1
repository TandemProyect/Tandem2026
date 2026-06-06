$pat = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$h = @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $pat)); 'Content-Type' = 'application/json-patch+json' }
$baseUri = 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems'
$patch = @( @{ op = 'add'; path = '/fields/System.State'; value = 'Resolved' } )
$json = ConvertTo-Json $patch -Depth 10
$resp = Invoke-RestMethod -Uri "$baseUri/697?api-version=7.1" -Method Patch -Headers $h -Body $json
Write-Output ("US #697 -> {0}" -f $resp.fields.'System.State')
