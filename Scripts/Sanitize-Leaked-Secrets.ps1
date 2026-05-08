Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

param(
    [Parameter(Mandatory = $true)]
    [string]$LeakedOpenAiKey,

    [Parameter(Mandatory = $true)]
    [string]$LeakedAzdoPat,

    [string]$RootPath = "C:\00_Tandem2026"
)

if (!(Test-Path $RootPath)) {
    throw "RootPath no existe: $RootPath"
}

$include = @("*.cs", "*.config", "*.ps1", "*.md", "*.json", "*.yml", "*.yaml", "*.txt")
$files = Get-ChildItem -Path $RootPath -Recurse -File -Include $include |
    Where-Object { $_.FullName -notmatch "\\.git\\" }

$openAiReplacements = 0
$azdoReplacements = 0

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content

    if ($content.Contains($LeakedOpenAiKey)) {
        $content = $content.Replace($LeakedOpenAiKey, "REDACTED_OPENAI_KEY")
    }

    if ($content.Contains($LeakedAzdoPat)) {
        $content = $content.Replace($LeakedAzdoPat, "REDACTED_AZDO_PAT")
    }

    if ($content -ne $original) {
        $openAiReplacements += ([regex]::Matches($original, [regex]::Escape($LeakedOpenAiKey))).Count
        $azdoReplacements += ([regex]::Matches($original, [regex]::Escape($LeakedAzdoPat))).Count
        [System.IO.File]::WriteAllText($file.FullName, $content)
        Write-Host "Sanitized: $($file.FullName)"
    }
}

Write-Host ""
Write-Host "OpenAI replacements: $openAiReplacements"
Write-Host "AZDO replacements: $azdoReplacements"
Write-Host "Listo. Revisa cambios con: git status --short"
