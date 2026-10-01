param(
    [string] $Root = "C:\00_Tandem2026\Desing\Content\DesignTools\DWG\AtkSystem60"
)
$ErrorActionPreference = "Stop"
$acc = "C:\Program Files\Autodesk\AutoCAD 2026\accoreconsole.exe"
$tdDir = Join-Path $Root "3D"
$snaps = Join-Path $Root "Snaps"
$master = Join-Path $tdDir "27904209.dwg"
$fnLsp = Join-Path $snaps "make-3d-from-279.lsp"
$hdr = Join-Path $snaps "_hdr3d.lsp"
$scr = Join-Path $snaps "_run3d.scr"

$panels = @(
    @{ Code = "27604207"; W = 0.60; H = 2.70; Lbl = "2,70x60" }
    @{ Code = "27454206"; W = 0.45; H = 2.70; Lbl = "2,70x45" }
    @{ Code = "27304205"; W = 0.30; H = 2.70; Lbl = "2,70x30" }
    @{ Code = "24904240"; W = 0.90; H = 2.40; Lbl = "2,40x90" }
    @{ Code = "24604242"; W = 0.60; H = 2.40; Lbl = "2,40x60" }
    @{ Code = "24454243"; W = 0.45; H = 2.40; Lbl = "2,40x45" }
    @{ Code = "24304244"; W = 0.30; H = 2.40; Lbl = "2,40x30" }
    @{ Code = "12904215"; W = 0.90; H = 1.20; Lbl = "1,20x90" }
    @{ Code = "12604213"; W = 0.60; H = 1.20; Lbl = "1,20x60" }
    @{ Code = "12454212"; W = 0.45; H = 1.20; Lbl = "1,20x45" }
    @{ Code = "12304211"; W = 0.30; H = 1.20; Lbl = "1,20x30" }
)

if (-not (Test-Path $master)) { throw "Falta maestro $master" }
Get-Process accoreconsole -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $tdDir "_stretch-test.dwg") -Force -ErrorAction SilentlyContinue

foreach ($p in $panels) {
    $dst = Join-Path $tdDir ($p.Code + ".dwg")
    try {
        Copy-Item -LiteralPath $master -Destination $dst -Force -ErrorAction Stop
    } catch {
        Write-Host "SKIP locked $($p.Code)"
        continue
    }
    $att = $p.Code.Substring($p.Code.Length - 4, 4)
    $w = $p.W.ToString("0.00", [cultureinfo]::InvariantCulture)
    $h = $p.H.ToString("0.00", [cultureinfo]::InvariantCulture)
    $hdrTxt = "(setq *W* $w)`r`n(setq *H* $h)`r`n(setq *LBL* `"$($p.Lbl)`")`r`n(setq *ATT* `"$att`")`r`n(setq *OLAP* 0.010)`r`n(load `"$($fnLsp.Replace('\','/'))`")`r`n"
    Set-Content -Path $hdr -Value $hdrTxt -Encoding ascii
    $scrTxt = "FILEDIA`r`n0`r`nSECURELOAD`r`n0`r`n(load `"$($hdr.Replace('\','/'))`")`r`nFILEDIA`r`n1`r`n_.QUIT`r`n_Y`r`n"
    Set-Content -Path $scr -Value $scrTxt -Encoding ascii
    $thisLog = Join-Path $snaps ("build-3d-" + $p.Code + ".log")
    & $acc /i $dst /s $scr /l en-US 2>&1 | Out-File -FilePath $thisLog -Encoding utf8
    $t = ([IO.File]::ReadAllText($thisLog) -replace "`0", "")
    $ok = $t -match ("OK-" + [regex]::Escape($p.Lbl))
    $err = $t -match "; error:"
    $len = (Get-Item $dst).Length
    Write-Host "$(if ($ok -and -not $err) {'OK'} else {'FAIL'}) $($p.Code) $($p.Lbl) bytes=$len"
    if ($err) {
        $t -split "`n" | Where-Object { $_ -match "error" } | Select-Object -First 2 | ForEach-Object { Write-Host "  $_" }
    }
}
Write-Host "==== DONE ===="
Get-ChildItem $tdDir -Filter "*.dwg" | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
