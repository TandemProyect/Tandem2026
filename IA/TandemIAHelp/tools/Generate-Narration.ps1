param(
    [Parameter(Mandatory = $true)]
    [string]$HelpId,

    [int]$DurationSeconds = 60
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($DurationSeconds -le 0) {
    throw 'La duracion debe ser un numero positivo.'
}

$RootDir = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$HelpItemsDir = Join-Path $RootDir 'help-items'
$OutputDir = Join-Path $RootDir 'output'
$HelpItemPath = Join-Path $HelpItemsDir "$HelpId.json"

if (-not (Test-Path $HelpItemPath)) {
    throw "No existe la ficha: $HelpItemPath"
}

$HelpItem = Get-Content $HelpItemPath -Raw | ConvertFrom-Json
$Steps = @($HelpItem.expectedUserFlow)
$Hints = @($HelpItem.videoNarrationHints)
$EdgeCases = @($HelpItem.edgeCases)
$SecondsPerStep = [Math]::Max(5, [Math]::Round($DurationSeconds / [Math]::Max($Steps.Count, 1)))

function Format-HelpTime {
    param([int]$TotalSeconds)
    $Minutes = [Math]::Floor($TotalSeconds / 60)
    $Seconds = $TotalSeconds % 60
    return ('{0:00}:{1:00}' -f $Minutes, $Seconds)
}

$NarrationLines = @(
    "En este video vamos a ver como usar $($HelpItem.title).",
    $HelpItem.purpose
)

for ($Index = 0; $Index -lt $Steps.Count; $Index++) {
    $NarrationLines += "Paso $($Index + 1): $($Steps[$Index])"
}

$NarrationLines += 'Al terminar, el plano queda preparado con el muro dibujado para continuar el trabajo.'

$SubtitleRows = @()
for ($Index = 0; $Index -lt $Steps.Count; $Index++) {
    $Start = $Index * $SecondsPerStep
    $End = [Math]::Min($DurationSeconds, $Start + $SecondsPerStep)
    $SubtitleRows += "| $(Format-HelpTime $Start) - $(Format-HelpTime $End) | $($Steps[$Index]) |"
}

$CommandAutocad = if ($HelpItem.commands.autocad) { $HelpItem.commands.autocad } else { 'pendiente' }
$Builder = [System.Text.StringBuilder]::new()

function Add-Line {
    param([string]$Text = '')
    [void]$Builder.AppendLine($Text)
}

Add-Line "# Narracion base - $($HelpItem.title)"
Add-Line
Add-Line "- Help id: ``$($HelpItem.id)``"
Add-Line "- Funcion: $($HelpItem.feature)"
Add-Line "- Duracion objetivo: $DurationSeconds segundos"
Add-Line "- Comando AutoCAD: ``$CommandAutocad``"
Add-Line
Add-Line '## Voz en off base'
Add-Line
foreach ($Line in ($NarrationLines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })) {
    Add-Line $Line
    Add-Line
}
Add-Line '## Subtitulos sugeridos'
Add-Line
Add-Line '| Tiempo | Texto |'
Add-Line '| --- | --- |'
foreach ($Row in $SubtitleRows) {
    Add-Line $Row
}
Add-Line
Add-Line '## Pistas para mejorar con IA'
Add-Line
foreach ($Hint in $Hints) {
    Add-Line "- $Hint"
}
Add-Line
Add-Line '## Casos borde a evitar en el video'
Add-Line
foreach ($EdgeCase in $EdgeCases) {
    Add-Line "- $EdgeCase"
}
Add-Line
Add-Line '## Checklist de montaje'
Add-Line
Add-Line '- Confirmar que el video muestra el boton marcado.'
Add-Line '- Ajustar la narracion a lo que se ve en pantalla.'
Add-Line '- Generar voz a partir de la seccion "Voz en off base".'
Add-Line '- Sincronizar subtitulos con los clics principales.'
Add-Line '- En una fase posterior, insertar avatar y voz definitiva.'

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$OutputPath = Join-Path $OutputDir "$HelpId.narration.md"
$Builder.ToString() | Set-Content -Path $OutputPath -Encoding UTF8
Write-Output $OutputPath