# Script para corregir las referencias de ZWCAD en el archivo .csproj
# Ejecuta este script después de cerrar Visual Studio

$projectPath = "ZwcadPlugin\ZwcadPlugin.csproj"

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Corrigiendo referencias de ZWCAD" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que el archivo existe
if (-not (Test-Path $projectPath)) {
    Write-Host "ERROR: No se encontró el archivo $projectPath" -ForegroundColor Red
    Write-Host "Asegúrate de ejecutar este script desde la carpeta del repositorio (C:\Users\jag\source\repos\)" -ForegroundColor Yellow
    exit 1
}

# Leer el contenido del archivo
$content = Get-Content $projectPath -Raw

# Verificar si ya está corregido
if ($content -match 'C:\\Program Files\\ZWSOFT\\ZWCAD 2026\\ZwManaged\.dll') {
    Write-Host "✓ Las referencias ya están correctas" -ForegroundColor Green
    exit 0
}

# Hacer respaldo
$backupPath = "$projectPath.backup"
Copy-Item $projectPath $backupPath -Force
Write-Host "✓ Respaldo creado: $backupPath" -ForegroundColor Green

# Reemplazar las rutas incorrectas
$content = $content -replace '<HintPath>lib\\ZwManaged\.dll</HintPath>', '<HintPath>C:\Program Files\ZWSOFT\ZWCAD 2026\ZwManaged.dll</HintPath>'
$content = $content -replace '<HintPath>lib\\ZwDatabaseMgd\.dll</HintPath>', '<HintPath>C:\Program Files\ZWSOFT\ZWCAD 2026\ZwDatabaseMgd.dll</HintPath>'

# Agregar SpecificVersion=False si no existe
if ($content -notmatch '<SpecificVersion>False</SpecificVersion>') {
    $content = $content -replace '(<Reference Include="ZwManaged">.*?<Private>False</Private>)', '$1`n      <SpecificVersion>False</SpecificVersion>'
    $content = $content -replace '(<Reference Include="ZwDatabaseMgd">.*?<Private>False</Private>)', '$1`n      <SpecificVersion>False</SpecificVersion>'
}

# Guardar el archivo
Set-Content $projectPath $content -NoNewline

Write-Host "✓ Referencias corregidas exitosamente" -ForegroundColor Green
Write-Host ""
Write-Host "Cambios realizados:" -ForegroundColor Yellow
Write-Host "  - ZwManaged.dll: lib\ → C:\Program Files\ZWSOFT\ZWCAD 2026\" -ForegroundColor White
Write-Host "  - ZwDatabaseMgd.dll: lib\ → C:\Program Files\ZWSOFT\ZWCAD 2026\" -ForegroundColor White
Write-Host "  - Agregado: <SpecificVersion>False</SpecificVersion>" -ForegroundColor White
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Cyan
Write-Host "1. Abre Visual Studio" -ForegroundColor White
Write-Host "2. Abre el proyecto ZwcadPlugin" -ForegroundColor White
Write-Host "3. Compila el proyecto (Ctrl+Shift+B o F6)" -ForegroundColor White
Write-Host "4. Si hay errores, cierra y vuelve a abrir Visual Studio" -ForegroundColor White
Write-Host ""
Write-Host "¡Listo!" -ForegroundColor Green
