Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$id = 703
$pat = $env:AZDO_PAT
if (-not $pat) { throw "AZDO_PAT no configurado en variables de entorno." }
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))

$headers = @{
  Authorization = "Basic $auth"
  "Content-Type" = "application/json-patch+json; charset=utf-8"
}

$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/${id}?api-version=7.0"
$stateBody = @(
  @{ op = "replace"; path = "/fields/System.State"; value = "Resolved" }
) | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body ([System.Text.Encoding]::UTF8.GetBytes($stateBody)) -ContentType "application/json-patch+json; charset=utf-8" | Out-Null

$filePath = "C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\UI\HANDOVER-US-IMG-MUROS.md"
if (!(Test-Path $filePath)) {
  throw "No existe el archivo handover: $filePath"
}

$uploadHeaders = @{
  Authorization = "Basic $auth"
  "Content-Type" = "application/octet-stream"
}

$uploadUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/attachments?fileName=HANDOVER-US-IMG-MUROS.md&api-version=7.0"
$bytes = [System.IO.File]::ReadAllBytes($filePath)
$upload = Invoke-RestMethod -Uri $uploadUrl -Headers $uploadHeaders -Method Post -Body $bytes

$attachBody = @(
  @{
    op = "add"
    path = "/relations/-"
    value = @{
      rel = "AttachedFile"
      url = $upload.url
      attributes = @{ comment = "Handover tecnico completo para continuidad de agentes" }
    }
  }
) | ConvertTo-Json -Depth 20

Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body ([System.Text.Encoding]::UTF8.GetBytes($attachBody)) -ContentType "application/json-patch+json; charset=utf-8" | Out-Null
Write-Output "OK_US_703"
