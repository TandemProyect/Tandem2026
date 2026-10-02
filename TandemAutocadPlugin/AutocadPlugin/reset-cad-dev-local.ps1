# Developer reset: local AtDesing as if never used.
# Close AutoCAD first if UnAtdesing left files locked.
$ErrorActionPreference = "Continue"

$targets = @(
    (Join-Path $env:LOCALAPPDATA "AtDesing"),
    (Join-Path $env:TEMP "TandemAutocadWebView2"),
    (Join-Path $env:APPDATA "Tandem\AutocadPlugin\connect-eta-local.txt"),
    (Join-Path $env:APPDATA "Tandem\AutocadPlugin\connect-eta-prod.txt"),
    (Join-Path $env:APPDATA "Tandem\AutocadPlugin\plantilla-theme.txt")
)

$acad = Get-Process -Name "acad" -ErrorAction SilentlyContinue
if ($acad) {
    Write-Host "AutoCAD is still open. Close it and rerun this script if something stays locked."
}

foreach ($path in $targets) {
    if (Test-Path $path) {
        try {
            Remove-Item -LiteralPath $path -Recurse -Force
            Write-Host "deleted  $path"
        }
        catch {
            Write-Host "LOCKED   $path - $($_.Exception.Message)"
        }
    }
    else {
        Write-Host "missing  $path"
    }
}

Write-Host "Done. In AutoCAD: NETLOAD and TANDEM to test first start."
