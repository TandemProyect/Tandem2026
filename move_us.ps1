$pat = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $pat))
$headers = @{
    Authorization = 'Basic ' + $base64Auth
    'Content-Type' = 'application/json-patch+json'
}

$moveBody = @(
    @{
        op = 'add'
        path = '/fields/System.State'
        value = 'Active'
    }
)
$moveJson = ConvertTo-Json $moveBody -Depth 10
Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/688?api-version=7.1" -Method Patch -Headers $headers -Body $moveJson
Write-Output "US 688 moved to Active"
