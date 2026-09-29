<#
.SYNOPSIS
  Compara esquema SQL Server local vs produccion (tablas, columnas, indices, FK).
  No compara datos. No guarda contrasenas.
#>
param(
    [string]$Database = 'db_a197cd_desingproducction',
    [string]$LocalServer = 'NUCBOXG5\SQLEXPRESS',
    [string]$RemoteServer = 'SQL5113.site4now.net',
    [string]$RemoteUser = 'db_a197cd_desingproducction_admin',
    [string]$RemotePassword = '',
    [string]$OutDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RemotePassword)) {
    $RemotePassword = $env:TANDEM_SQL_PROD_PASSWORD
}
if ([string]::IsNullOrWhiteSpace($RemotePassword)) {
    $sec = Read-Host "Password SQL produccion ($RemoteUser)" -AsSecureString
    $RemotePassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec))
}

if ([string]::IsNullOrWhiteSpace($OutDir)) {
    $OutDir = Join-Path $env:TEMP 'tandem-sql-schema-diff'
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$schemaSql = @'
SET NOCOUNT ON;
SELECT
    s.name + N'.' + t.name AS ObjectName,
    c.name AS ColumnName,
    ty.name AS TypeName,
    c.max_length AS MaxLength,
    c.precision AS Prec,
    c.scale AS Scale,
    CAST(c.is_nullable AS int) AS IsNullable,
    ISNULL(OBJECT_DEFINITION(c.default_object_id), N'') AS DefaultDef
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name, c.column_id;
'@

$indexSql = @'
SET NOCOUNT ON;
SELECT
    s.name + N'.' + t.name AS ObjectName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    CAST(i.is_unique AS int) AS IsUnique,
    CAST(i.is_primary_key AS int) AS IsPk,
    STUFF((
        SELECT N',' + c.name
        FROM sys.index_columns ic
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
        ORDER BY ic.key_ordinal
        FOR XML PATH(N''), TYPE
    ).value(N'.', N'nvarchar(max)'), 1, 1, N'') AS KeyCols
FROM sys.indexes i
JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.is_ms_shipped = 0 AND i.name IS NOT NULL
ORDER BY s.name, t.name, i.name;
'@

$fkSql = @'
SET NOCOUNT ON;
SELECT
    OBJECT_SCHEMA_NAME(fk.parent_object_id) + N'.' + OBJECT_NAME(fk.parent_object_id) AS FromTable,
    fk.name AS FkName,
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) + N'.' + OBJECT_NAME(fk.referenced_object_id) AS ToTable
FROM sys.foreign_keys fk
ORDER BY FromTable, FkName;
'@

function Get-SqlRows {
    param(
        [string]$ConnectionString,
        [string]$Query,
        [string]$Label
    )
    Write-Host "Consultando $Label..."
    $conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $Query
    $cmd.CommandTimeout = 120
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $table = New-Object System.Data.DataTable
    try {
        $conn.Open()
        [void]$adapter.Fill($table)
    }
    finally {
        $conn.Close()
    }
    return ,$table
}

function Convert-RowsToKeySet {
    param([System.Data.DataTable]$Table, [string[]]$KeyColumns)
    $set = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($row in $Table.Rows) {
        $parts = foreach ($col in $KeyColumns) { [string]$row[$col] }
        [void]$set.Add(($parts -join '|'))
    }
    return ,$set
}

$localCs = "Data Source=$LocalServer;Initial Catalog=$Database;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=15"
$remoteCs = "Data Source=$RemoteServer;Initial Catalog=$Database;User Id=$RemoteUser;Password=$RemotePassword;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30"

$localCols = Get-SqlRows $localCs $schemaSql 'local columnas'
$remoteCols = Get-SqlRows $remoteCs $schemaSql 'producción columnas'
$localIdx = Get-SqlRows $localCs $indexSql 'local índices'
$remoteIdx = Get-SqlRows $remoteCs $indexSql 'producción índices'
$localFk = Get-SqlRows $localCs $fkSql 'local FK'
$remoteFk = Get-SqlRows $remoteCs $fkSql 'producción FK'

$localTables = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($r in $localCols.Rows) { [void]$localTables.Add([string]$r['ObjectName']) }
$remoteTables = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($r in $remoteCols.Rows) { [void]$remoteTables.Add([string]$r['ObjectName']) }

$onlyLocalTables = @($localTables | Where-Object { -not $remoteTables.Contains($_) } | Sort-Object)
$onlyRemoteTables = @($remoteTables | Where-Object { -not $localTables.Contains($_) } | Sort-Object)

$localColKeys = Convert-RowsToKeySet $localCols @('ObjectName','ColumnName','TypeName','MaxLength','Prec','Scale','IsNullable','DefaultDef')
$remoteColKeys = Convert-RowsToKeySet $remoteCols @('ObjectName','ColumnName','TypeName','MaxLength','Prec','Scale','IsNullable','DefaultDef')

$localColNames = Convert-RowsToKeySet $localCols @('ObjectName','ColumnName')
$remoteColNames = Convert-RowsToKeySet $remoteCols @('ObjectName','ColumnName')

$colsOnlyLocal = New-Object System.Collections.Generic.List[string]
$colsOnlyRemote = New-Object System.Collections.Generic.List[string]
foreach ($k in $localColNames) {
    if (-not $remoteColNames.Contains($k)) { $colsOnlyLocal.Add($k) }
}
foreach ($k in $remoteColNames) {
    if (-not $localColNames.Contains($k)) { $colsOnlyRemote.Add($k) }
}

$localIdxKeys = Convert-RowsToKeySet $localIdx @('ObjectName','IndexName','IndexType','IsUnique','IsPk','KeyCols')
$remoteIdxKeys = Convert-RowsToKeySet $remoteIdx @('ObjectName','IndexName','IndexType','IsUnique','IsPk','KeyCols')
$idxOnlyLocal = @($localIdxKeys | Where-Object { -not $remoteIdxKeys.Contains($_) } | Sort-Object)
$idxOnlyRemote = @($remoteIdxKeys | Where-Object { -not $localIdxKeys.Contains($_) } | Sort-Object)

$localFkKeys = Convert-RowsToKeySet $localFk @('FromTable','FkName','ToTable')
$remoteFkKeys = Convert-RowsToKeySet $remoteFk @('FromTable','FkName','ToTable')
$fkOnlyLocal = @($localFkKeys | Where-Object { -not $remoteFkKeys.Contains($_) } | Sort-Object)
$fkOnlyRemote = @($remoteFkKeys | Where-Object { -not $localFkKeys.Contains($_) } | Sort-Object)

$report = Join-Path $OutDir 'schema-diff.txt'
$lines = @()
$lines += "Comparacion de esquema  $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$lines += "Local:  $LocalServer / $Database  (tablas $($localTables.Count), columnas $($localCols.Rows.Count))"
$lines += "Remote: $RemoteServer / $Database  (tablas $($remoteTables.Count), columnas $($remoteCols.Rows.Count))"
$lines += ""
$lines += "=== Tablas SOLO en LOCAL (subir a produccion) ==="
if ($onlyLocalTables) { $lines += $onlyLocalTables } else { $lines += '(ninguna)' }
$lines += ""
$lines += "=== Tablas SOLO en PRODUCCION (bajar a local) ==="
if ($onlyRemoteTables) { $lines += $onlyRemoteTables } else { $lines += '(ninguna)' }
$lines += ""
$lines += "=== Columnas SOLO en LOCAL ==="
if ($colsOnlyLocal.Count -gt 0) { $lines += ($colsOnlyLocal | Sort-Object) } else { $lines += '(ninguna)' }
$lines += ""
$lines += "=== Columnas SOLO en PRODUCCION ==="
if ($colsOnlyRemote.Count -gt 0) { $lines += ($colsOnlyRemote | Sort-Object) } else { $lines += '(ninguna)' }
$lines += ""
$lines += "=== Columnas con tipo/null/default distinto (mismo nombre) ==="
$mismatchNames = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($k in $localColKeys) {
    $parts = $k.Split('|')
    if ($parts.Length -lt 2) { continue }
    $nameKey = $parts[0] + '|' + $parts[1]
    if ($remoteColNames.Contains($nameKey) -and -not $remoteColKeys.Contains($k)) {
        [void]$mismatchNames.Add($nameKey)
    }
}
if ($mismatchNames.Count -gt 0) { $lines += ($mismatchNames | Sort-Object) } else { $lines += '(ninguna)' }
$lines += ""
$lines += "=== Indices distintos (solo local / solo prod, primeras 80) ==="
$lines += "LOCAL: $($idxOnlyLocal.Count)"
$lines += ($idxOnlyLocal | Select-Object -First 80)
$lines += "PROD: $($idxOnlyRemote.Count)"
$lines += ($idxOnlyRemote | Select-Object -First 80)
$lines += ""
$lines += "=== FK solo local ==="
if ($fkOnlyLocal) { $lines += $fkOnlyLocal } else { $lines += '(ninguna)' }
$lines += "=== FK solo prod ==="
if ($fkOnlyRemote) { $lines += $fkOnlyRemote } else { $lines += '(ninguna)' }

$lines | Set-Content -Path $report -Encoding UTF8
Write-Host ""
Write-Host "Informe: $report"
Write-Host "Tablas local=$($localTables.Count) prod=$($remoteTables.Count)  | solo-local=$($onlyLocalTables.Count) solo-prod=$($onlyRemoteTables.Count)"
Write-Host "Columnas extra local=$($colsOnlyLocal.Count) extra prod=$($colsOnlyRemote.Count) tipo distinto=$($mismatchNames.Count)"

$lines | ForEach-Object { Write-Host $_ }
