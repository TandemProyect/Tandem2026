param(
    [string] $Root = "C:\00_Tandem2026\Desing\Content\DesignTools\DWG\AtkSystem60"
)
$ErrorActionPreference = "Stop"
$acc = "C:\Program Files\Autodesk\AutoCAD 2026\accoreconsole.exe"
$refDir = Join-Path $Root "3DRef"
$snaps = Join-Path $Root "Snaps"
$master = Join-Path $refDir "27904209R.dwg"
$fnLsp = Join-Path $snaps "make-from-279.lsp"
$hdr = Join-Path $snaps "_hdr.lsp"
$scr = Join-Path $snaps "_run.scr"
$log = Join-Path $snaps "build-3dref-panels.log"

$panels = @(
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
    @{ Code = "27104219"; W = 0.75; H = 2.70; Lbl = "2,70x75" }
    @{ Code = "24104224"; W = 0.75; H = 2.40; Lbl = "2,40x75" }
    @{ Code = "12104120"; W = 0.75; H = 1.20; Lbl = "1,20x75" }
)

if (-not (Test-Path $master)) { throw "Falta maestro $master" }
Get-Process accoreconsole -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

$logLines = @()
foreach ($p in $panels) {
    $dst = Join-Path $refDir ($p.Code + "R.dwg")
    Copy-Item -LiteralPath $master -Destination $dst -Force
    $w = $p.W.ToString("0.00", [cultureinfo]::InvariantCulture)
    $h = $p.H.ToString("0.00", [cultureinfo]::InvariantCulture)
    $hdrTxt = "(setq *W* $w)`r`n(setq *H* $h)`r`n(setq *LBL* `"$($p.Lbl)`")`r`n(load `"$($fnLsp.Replace('\','/'))`")`r`n"
    Set-Content -Path $hdr -Value $hdrTxt -Encoding ascii
    $scrTxt = "FILEDIA`r`n0`r`nSECURELOAD`r`n0`r`n(load `"$($hdr.Replace('\','/'))`")`r`nFILEDIA`r`n1`r`n_.QUIT`r`n_Y`r`n"
    Set-Content -Path $scr -Value $scrTxt -Encoding ascii
    $thisLog = Join-Path $snaps ("build-" + $p.Code + ".log")
    & $acc /i $dst /s $scr /l en-US 2>&1 | Out-File -FilePath $thisLog -Encoding utf8
    $t = ([IO.File]::ReadAllText($thisLog) -replace "`0", "")
    $ok = $t -match ("OK-" + [regex]::Escape($p.Lbl))
    $err = $t -match "; error:"
    $len = (Get-Item $dst).Length
    $line = "$(if ($ok -and -not $err) {'OK'} else {'FAIL'}) $($p.Code) $($p.Lbl) bytes=$len"
    Write-Host $line
    $logLines += $line
    if ($err) {
        ($t -split "`n" | Where-Object { $_ -match "error" } | Select-Object -First 3) | ForEach-Object { Write-Host "  $_"; $logLines += "  $_" }
    }
}
Set-Content -Path $log -Value $logLines -Encoding utf8
Write-Host "==== DONE ===="
Get-ChildItem $refDir -Filter "*R.dwg" | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
