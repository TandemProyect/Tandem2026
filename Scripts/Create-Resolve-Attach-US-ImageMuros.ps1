Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Set-Location "C:\00_Tandem2026\Scripts"

$id = .\Create-US-Fast.ps1 `
  -Titulo "Imagen a muros: E/H y estabilidad de cotas" `
  -Descripcion "Ajuste de flujo imagen->muros para espesor/altura y geometria estable." `
  -StoryPoints 5

if (-not $id) {
  throw "No se devolvio ID al crear la US"
}

.\Edit-US.ps1 -ID $id -Estado "Resolved"

.\Attach-Document.ps1 `
  -WorkItemId $id `
  -FilePath "C:\00_Tandem2026\documentación\HANDOVER-US-IMG-MUROS.md" `
  -Comment "Handover tecnico completo para continuidad de agentes"

Write-Output "US_ID=$id"
