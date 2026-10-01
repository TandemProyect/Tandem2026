param(
    [string] $Root = "C:\00_Tandem2026\Desing\Content\DesignTools\DWG\AtkSystem60"
)

$ErrorActionPreference = "Stop"
$snapsDir = Join-Path $Root "Snaps"
$refDir = Join-Path $Root "3DRef"
$tdDir = Join-Path $Root "3D"
New-Item -ItemType Directory -Path $snapsDir, $refDir, $tdDir -Force | Out-Null

# Paneles rectos ATK-60 = gama NEVI 2.7 / 2.4 / 1.2 x 0.90 / 0.60 / 0.45 / 0.30
# CodeName = fichero STL Atk60Element. Grapas: 550 mm en 2.7/2.4, 350 mm en 1.2 (manual NEVI).
$panels = @(
    @{ Code = "27904209"; Article = "3120270090"; H = 2.70; W = 0.90; SkipJson = $true; SkipDxf = $true }
    @{ Code = "27604207"; Article = "3120270060"; H = 2.70; W = 0.60 }
    @{ Code = "27454206"; Article = "3120270045"; H = 2.70; W = 0.45 }
    @{ Code = "27304205"; Article = "3120270030"; H = 2.70; W = 0.30 }
    @{ Code = "24904240"; Article = "3120240090"; H = 2.40; W = 0.90 }
    @{ Code = "24604242"; Article = "3120240060"; H = 2.40; W = 0.60 }
    @{ Code = "24454243"; Article = "3120240045"; H = 2.40; W = 0.45 }
    @{ Code = "24304244"; Article = "3120240030"; H = 2.40; W = 0.30 }
    @{ Code = "12904215"; Article = "3120120090"; H = 1.20; W = 0.90 }
    @{ Code = "12604213"; Article = "3120120060"; H = 1.20; W = 0.60 }
    @{ Code = "12454212"; Article = "3120120045"; H = 1.20; W = 0.45 }
    @{ Code = "12304211"; Article = "3120120030"; H = 1.20; W = 0.30 }
)

$insetX = 0.0437138
$thk = 0.12
$yGrapa = -0.119128
$rMark = 0.04

function Get-GrapaZ([double]$h) {
    if ($h -ge 2.0) { return 0.55 }
    return 0.35
}

function Add-Line([System.Text.StringBuilder]$sb, [double]$x1, [double]$y1, [double]$z1, [double]$x2, [double]$y2, [double]$z2) {
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("LINE")
    [void]$sb.AppendLine("8")
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("10"); [void]$sb.AppendLine($x1.ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("20"); [void]$sb.AppendLine($y1.ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("30"); [void]$sb.AppendLine($z1.ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("11"); [void]$sb.AppendLine($x2.ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("21"); [void]$sb.AppendLine($y2.ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("31"); [void]$sb.AppendLine($z2.ToString("0.######", [cultureinfo]::InvariantCulture))
}

function Add-Circle([System.Text.StringBuilder]$sb, [double]$x, [double]$y, [double]$z, [double]$r) {
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("CIRCLE")
    [void]$sb.AppendLine("8")
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("10"); [void]$sb.AppendLine($x.ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("20"); [void]$sb.AppendLine($y.ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("30"); [void]$sb.AppendLine($z.ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("40"); [void]$sb.AppendLine($r.ToString("0.######", [cultureinfo]::InvariantCulture))
}

function Write-Json($p, $zG) {
    $w = $p.W; $h = $p.H
    $json = @"
{
  "codeName": "$($p.Code)",
  "articleCode": "$($p.Article)",
  "family": "panel-rect",
  "units": "m",
  "origin": "insbase-0-0-0",
  "comment": "Caja X=0..$w Y=-$thk..0 Z=0..$h m. PANEL = cara Y=0. GRAPA_V = $([int]($zG*1000)) mm desde pie y cabeza (NEVI/ATK-60).",
  "box": {
    "width": $w,
    "height": $h,
    "thickness": $thk,
    "axisWidth": "X",
    "axisHeight": "Z",
    "axisThickness": "Y"
  },
  "insbase": [0, 0, 0],
  "snaps": [
    { "id": "V_BL", "role": "PANEL", "x": 0, "y": 0, "z": 0, "rzDeg": 0 },
    { "id": "V_BR", "role": "PANEL", "x": $w, "y": 0, "z": 0, "rzDeg": 0 },
    { "id": "V_TL", "role": "PANEL", "x": 0, "y": 0, "z": $h, "rzDeg": 0 },
    { "id": "V_TR", "role": "PANEL", "x": $w, "y": 0, "z": $h, "rzDeg": 0 },
    { "id": "L$('{0:000}' -f [int]($zG*100))", "role": "GRAPA_V", "x": $insetX, "y": $yGrapa, "z": $zG, "rzDeg": 180 },
    { "id": "L$('{0:000}' -f [int](($h-$zG)*100))", "role": "GRAPA_V", "x": $insetX, "y": $yGrapa, "z": $($h-$zG), "rzDeg": 180 },
    { "id": "R$('{0:000}' -f [int]($zG*100))", "role": "GRAPA_V", "x": $($w-$insetX), "y": $yGrapa, "z": $zG, "rzDeg": 0 },
    { "id": "R$('{0:000}' -f [int](($h-$zG)*100))", "role": "GRAPA_V", "x": $($w-$insetX), "y": $yGrapa, "z": $($h-$zG), "rzDeg": 0 }
  ]
}
"@
    $path = Join-Path $snapsDir ($p.Code + ".json")
    Set-Content -Path $path -Value $json.Replace("`r`n", "`n") -Encoding utf8
}

function Write-Dxf($p, $zG) {
    $w = $p.W; $h = $p.H; $t = -$thk
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("SECTION")
    [void]$sb.AppendLine("2")
    [void]$sb.AppendLine("HEADER")
    [void]$sb.AppendLine("9")
    [void]$sb.AppendLine("`$INSUNITS")
    [void]$sb.AppendLine("70")
    [void]$sb.AppendLine("6")
    [void]$sb.AppendLine("9")
    [void]$sb.AppendLine("`$INSBASE")
    [void]$sb.AppendLine("10")
    [void]$sb.AppendLine("0.0")
    [void]$sb.AppendLine("20")
    [void]$sb.AppendLine("0.0")
    [void]$sb.AppendLine("30")
    [void]$sb.AppendLine("0.0")
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("ENDSEC")
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("SECTION")
    [void]$sb.AppendLine("2")
    [void]$sb.AppendLine("ENTITIES")

    $edges = @(
        @(0,0,0, $w,0,0), @( $w,0,0, $w,0,$h), @($w,0,$h, 0,0,$h), @(0,0,$h, 0,0,0),
        @(0,$t,0, $w,$t,0), @($w,$t,0, $w,$t,$h), @($w,$t,$h, 0,$t,$h), @(0,$t,$h, 0,$t,0),
        @(0,0,0, 0,$t,0), @($w,0,0, $w,$t,0), @(0,0,$h, 0,$t,$h), @($w,0,$h, $w,$t,$h)
    )
    foreach ($e in $edges) {
        Add-Line $sb $e[0] $e[1] $e[2] $e[3] $e[4] $e[5]
    }

    $zs = @($zG, ($h - $zG))
    $xs = @($insetX, ($w - $insetX))
    foreach ($x in $xs) {
        foreach ($z in $zs) {
            Add-Circle $sb $x 0 $z $rMark
        }
    }
    foreach ($c in @(@(0,0,0), @($w,0,0), @(0,0,$h), @($w,0,$h))) {
        Add-Circle $sb $c[0] $c[1] $c[2] ($rMark * 1.3)
    }

    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("TEXT")
    [void]$sb.AppendLine("8")
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("10")
    [void]$sb.AppendLine("0.02")
    [void]$sb.AppendLine("20")
    [void]$sb.AppendLine("0.0")
    [void]$sb.AppendLine("30")
    [void]$sb.AppendLine(($h + 0.04).ToString("0.######", [cultureinfo]::InvariantCulture))
    [void]$sb.AppendLine("40")
    [void]$sb.AppendLine("0.08")
    [void]$sb.AppendLine("1")
    [void]$sb.AppendLine("$($p.Code) $($w)x$h")
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("ENDSEC")
    [void]$sb.AppendLine("0")
    [void]$sb.AppendLine("EOF")

    $path = Join-Path $refDir ($p.Code + "R.dxf")
    [IO.File]::WriteAllText($path, $sb.ToString().Replace("`r`n", "`n"))
}

$jsonN = 0
$dxfN = 0
foreach ($p in $panels) {
    $zG = Get-GrapaZ $p.H
    if (-not $p.SkipJson) {
        Write-Json $p $zG
        $jsonN++
    }
    if (-not $p.SkipDxf) {
        Write-Dxf $p $zG
        $dxfN++
    }
}

Write-Host "JSON nuevos: $jsonN"
Write-Host "DXF 3DRef: $dxfN"
Write-Host "OK - paneles rectos ATK-60 / NEVI. 3D mesh: node stl-to-3d-dxf.js"
