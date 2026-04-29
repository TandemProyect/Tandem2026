$PAT  = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$h    = @{ Authorization = "Basic $auth" }
$hj   = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json; charset=utf-8" }
$boardId = "892fa957-9c33-4237-a99f-2660bd9ec80d"

$styleBody = @{
    rules = @{
        fill = @(
            @{ name="Bug";         filter="[System.WorkItemType] = 'Bug'";                    isEnabled=$true; settings=@{"background-color"="#CC293D"; "title-color"="#FFFFFF"} },
            @{ name="ZwcadPlugin"; filter="[System.Tags] Contains 'ZwcadPlugin'";             isEnabled=$true; settings=@{"background-color"="#339933"; "title-color"="#FFFFFF"} },
            @{ name="Desing";      filter="[System.Tags] Contains 'Desing'";                  isEnabled=$true; settings=@{"background-color"="#0078D4"; "title-color"="#FFFFFF"} },
            @{ name="DAL";         filter="[System.Tags] Contains 'DAL'";                     isEnabled=$true; settings=@{"background-color"="#E17D00"; "title-color"="#FFFFFF"} }
        )
    }
} | ConvertTo-Json -Depth 10

$endpoints = @(
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/$boardId/cardstylesettings?api-version=7.0",
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Stories/cardstylesettings?api-version=7.0",
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/$boardId/cardstylesettings?api-version=7.1-preview.1",
    "https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Stories/cardstylesettings?api-version=7.1-preview.1"
)

$ok = $false
foreach ($ep in $endpoints) {
    try {
        Invoke-RestMethod -Uri $ep -Headers $hj -Method Put -Body ([Text.Encoding]::UTF8.GetBytes($styleBody)) | Out-Null
        Write-Host ("OK: " + $ep) -ForegroundColor Green
        $ok = $true
        break
    } catch {
        Write-Host ("FAIL [" + $ep.Split("?")[1] + "]: " + $_.Exception.Message) -ForegroundColor Red
    }
}

if (-not $ok) {
    Write-Host "`nLa API cardstylesettings no esta disponible en este plan." -ForegroundColor Yellow
    Write-Host "Configura los colores manualmente en 2 minutos:" -ForegroundColor Cyan
    Write-Host "  1. Abre el panel: https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Stories" -ForegroundColor White
    Write-Host "  2. Clic en engranaje (arriba derecha) -> Styling" -ForegroundColor White
    Write-Host "  3. Add styling rule:" -ForegroundColor White
    Write-Host "     - Bug         | [System.WorkItemType] = 'Bug'             | fondo #CC293D (rojo)"   -ForegroundColor Red
    Write-Host "     - ZwcadPlugin | [System.Tags] Contains 'ZwcadPlugin'      | fondo #339933 (verde)"  -ForegroundColor Green
    Write-Host "     - Desing      | [System.Tags] Contains 'Desing'           | fondo #0078D4 (azul)"   -ForegroundColor Blue
    Write-Host "     - DAL         | [System.Tags] Contains 'DAL'              | fondo #E17D00 (naranja)" -ForegroundColor Yellow
}
