param(
    [Parameter(Mandatory = $true)]
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
$out = ($OutputPath -replace '[\s\\/"]+$', '')
$leaf = Split-Path $out -Leaf
if ($leaf -match "^Debug\d+$") {
    Write-Host "NETLOAD slot already in use: $leaf"
    exit 0
}

$binRoot = Split-Path $out -Parent
if (-not (Test-Path $out)) {
    throw "Output path not found: $out"
}

$nums = @(Get-ChildItem -Path $binRoot -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "^Debug(\d+)$" } |
    ForEach-Object { [int]([regex]::Match($_.Name, "^Debug(\d+)$").Groups[1].Value) })

$next = 2
if ($nums.Count -gt 0) {
    $next = ($nums | Measure-Object -Maximum).Maximum + 1
}

$dest = Join-Path $binRoot ("Debug" + $next)
New-Item -ItemType Directory -Path $dest -Force | Out-Null
robocopy $out $dest /E /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null
if ($LASTEXITCODE -ge 8) {
    throw "robocopy failed ($LASTEXITCODE) $out -> $dest"
}

Write-Host "NETLOAD -> $dest\ZwcadPlugin.dll"
