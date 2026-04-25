# 🔍 Investigación: Problema con Iconos en ZWCAD Plugin

**US:** #619 - Insertar Img en Command Seleccionar Muro  
**Fecha:** 2026-04-25  
**Estado:** ⚠️ PENDIENTE - Funcionalidad implementada pero icono no visible  

---

## 📋 Resumen Ejecutivo

El comando `TANDEM_SELECCIONAR_LINEAS` funciona correctamente, pero **el icono no aparece en el ribbon de ZWCAD**. 

**Botón visible:** ✅ SÍ (panel "Seleccion" en ribbon "Tandem 2026")  
**Icono visible:** ❌ NO (solo aparece el texto "Seleccionar")  
**Comando funcional:** ✅ SÍ (ejecuta correctamente al hacer clic)

---

## 🧪 Pruebas Realizadas

### ✅ Prueba 1: Menú de Test Manual en ZWCAD
Se creó un CUI de prueba mínimo (`TestIconoV2.cui`) con un icono PNG simple.

**Resultado:** ✅ **FUNCIONA** cuando:
- El icono PNG está en: `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\`
- El CUI usa solo el nombre: `<LargeImage>simple_square.png</LargeImage>`

**Archivo de prueba:**
```
C:\00_Tandem2026\test_icons\TestIconoV2.cui
```

**Icono de prueba:**
```
C:\Program Files\ZWSOFT\ZWCAD 2026\Support\simple_square.png
```

---

## 🔍 Diagnóstico del Problema

### Causas Identificadas

1. **Rutas Absolutas No Funcionan**
   - ❌ `<LargeImage>C:\00_Tandem2026\...\img\SelectLines.png</LargeImage>`
   - ❌ `<LargeImage>img\SelectLines.png</LargeImage>` (ruta relativa desde CUI)
   - ✅ `<LargeImage>SelectLines.png</LargeImage>` (solo nombre, buscando en Support)

2. **Ubicación del Icono**
   - ❌ Iconos en `bin\Debug\MNU\img\` no son accesibles
   - ❌ Iconos en carpetas del proyecto no son accesibles
   - ✅ Iconos en `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\` **SÍ funcionan**

3. **Formato del Icono**
   - ✅ PNG funciona correctamente
   - ⚠️ BMP generado con PowerShell no funciona
   - ⚠️ SVG no probado definitivamente

---

## 📂 Estado Actual de Archivos

### Archivos del Plugin

```
TamdenZwcadPluging/ZwcadPlugin/
├── MNU/
│   ├── img/
│   │   ├── SelectLines.png (243 bytes) ⚠️ Demasiado pequeño
│   │   └── SelectLines.svg (1597 bytes)
│   └── Tandem2026.cui
├── bin/Debug/
│   ├── MNU/
│   │   ├── img/
│   │   │   └── SelectLines.png (copiado por compilación)
│   │   └── Tandem2026.cui (generado por CuixBuilder)
│   └── ZwcadPlugin.dll
├── CuixBuilder.cs (define el XML del CUI)
└── ZwcadPlugin.csproj
```

### Configuración Actual en CuixBuilder.cs

```csharp
<MenuMacro UID="td_seleccionar">
  <Macro>
	<Name>Seleccionar Lineas</Name>
	<Command>^c^cTANDEM_SELECCIONAR_LINEAS</Command>
	<HelpString>Permite seleccionar lineas y polilineas en el dibujo</HelpString>
	<LargeImage>img\SelectLines.png</LargeImage>  ⚠️ NO FUNCIONA
	<SmallImage>img\SelectLines.png</SmallImage>  ⚠️ NO FUNCIONA
  </Macro>
</MenuMacro>
```

### Configuración en ZwcadPlugin.csproj

```xml
<ItemGroup>
  <Content Include="MNU\img\SelectLines.png">
	<CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>

<Target Name="CopiarMNU" AfterTargets="Build">
  <Copy SourceFiles="MNU\Tandem2026.cui" 
		DestinationFolder="$(OutputPath)MNU\" 
		SkipUnchangedFiles="true" />
  <!-- ⚠️ FALTA: Copiar img\ a bin\Debug\MNU\img\ -->
</Target>
```

---

## ✅ Solución Propuesta

### Opción A: Copiar Iconos a Support de ZWCAD (Recomendada)

**Ventajas:**
- ✅ Funciona garantizado (probado con TestIconoV2.cui)
- ✅ No requiere rutas complejas
- ✅ ZWCAD encuentra los iconos automáticamente

**Desventajas:**
- ⚠️ Requiere permisos de administrador para copiar a `C:\Program Files\`
- ⚠️ Los iconos no están autocontenidos en el plugin

**Pasos:**

1. **Crear/Obtener un icono PNG válido** (32x32 o 64x64 píxeles):
   ```powershell
   # Opción 1: Copiar de Bootstrap Icons y convertir SVG a PNG
   # Opción 2: Usar un icono PNG ya existente
   # Opción 3: Crear con herramienta gráfica (GIMP, Photoshop, Paint.NET)
   ```

2. **Copiar el icono a ZWCAD Support:**
   ```powershell
   Copy-Item "TamdenZwcadPluging\ZwcadPlugin\MNU\img\SelectLines.png" `
			 "C:\Program Files\ZWSOFT\ZWCAD 2026\Support\SelectLines.png" -Force
   ```

3. **Actualizar CuixBuilder.cs:**
   ```csharp
   <LargeImage>SelectLines.png</LargeImage>
   <SmallImage>SelectLines.png</SmallImage>
   ```

4. **Recompilar y recargar en ZWCAD:**
   ```
   CUIUNLOAD (descargar Tandem 2026)
   MENULOAD (cargar bin\Debug\MNU\Tandem2026.cui)
   ```

---

### Opción B: Script de Post-Instalación

Crear un script PowerShell que copie los iconos a Support durante la instalación/compilación:

```powershell
# Scripts/Install-Icons.ps1
$zwcadSupport = "C:\Program Files\ZWSOFT\ZWCAD 2026\Support"
$iconos = Get-ChildItem "TamdenZwcadPluging\ZwcadPlugin\MNU\img\*.png"

foreach ($icono in $iconos) {
	Copy-Item $icono.FullName "$zwcadSupport\$($icono.Name)" -Force
	Write-Host "✓ Copiado: $($icono.Name)"
}
```

Añadir al `.csproj`:
```xml
<Target Name="InstalarIconos" AfterTargets="Build">
  <Exec Command="powershell -ExecutionPolicy Bypass -File $(ProjectDir)..\..\Scripts\Install-Icons.ps1" />
</Target>
```

---

### Opción C: Investigar Rutas Relativas Correctas (Pendiente)

ZWCAD puede aceptar rutas relativas si se configuran correctamente. Requiere más investigación sobre:
- Variable `SUPPORTPATH` de ZWCAD
- Configuración de "Trusted Locations"
- Uso de `%APPDATA%` o variables de entorno

---

## 🐛 Problemas Conocidos

### 1. SelectLines.png de 243 bytes

El archivo actual es demasiado pequeño y probablemente corrupto.

**Verificar:**
```powershell
Get-Item "TamdenZwcadPluging\ZwcadPlugin\MNU\img\SelectLines.png"
# Length debe ser > 1000 bytes para un PNG de 32x32
```

**Solución:**
Generar un PNG válido:
```powershell
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap(32, 32)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 0, 120, 215))
$g.FillEllipse($brush, 4, 4, 24, 24)
$brush.Dispose()
$g.Dispose()
$bmp.Save("TamdenZwcadPluging\ZwcadPlugin\MNU\img\SelectLines.png", [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
```

### 2. Target CopiarMNU Incompleto

El target de compilación no copia la carpeta `img\`:

**Actualizar en ZwcadPlugin.csproj:**
```xml
<Target Name="CopiarMNU" AfterTargets="Build">
  <Copy SourceFiles="MNU\Tandem2026.cui" 
		DestinationFolder="$(OutputPath)MNU\" 
		SkipUnchangedFiles="true" />

  <!-- Copiar carpeta img con todos los iconos -->
  <ItemGroup>
	<ImgFiles Include="MNU\img\**\*.*" />
  </ItemGroup>
  <Copy SourceFiles="@(ImgFiles)" 
		DestinationFolder="$(OutputPath)MNU\img\%(RecursiveDir)" 
		SkipUnchangedFiles="true" />
</Target>
```

---

## 📚 Documentación Relacionada

### Archivos Creados Durante la Investigación

1. **`TamdenZwcadPluging\ZwcadPlugin\INSTRUCCIONES_AGREGAR_ICONOS.md`**
   - Guía completa para añadir iconos a nuevos comandos
   - Checklist paso a paso
   - Problemas comunes y soluciones

2. **`test_icons\TestIconoV2.cui`**
   - CUI de prueba que **SÍ funciona** con iconos
   - Referencia para estructura correcta

3. **`test_icons\simple_square.png`**
   - Icono de prueba funcional
   - Copiado a `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\`

### Commits Relacionados

```bash
# Último commit antes de pausar
git log --oneline -5

# Cambios pendientes (no commiteados)
git status
```

---

## 🎯 Próximos Pasos para Resolver

### Paso 1: Crear Icono PNG Válido
```powershell
cd C:\00_Tandem2026
# Ejecutar script de generación de PNG o copiar uno existente
```

### Paso 2: Copiar a ZWCAD Support
```powershell
Copy-Item "TamdenZwcadPluging\ZwcadPlugin\MNU\img\SelectLines.png" `
		  "C:\Program Files\ZWSOFT\ZWCAD 2026\Support\SelectLines.png" -Force
```

### Paso 3: Actualizar CuixBuilder.cs
```csharp
// Líneas 87-88
<LargeImage>SelectLines.png</LargeImage>
<SmallImage>SelectLines.png</SmallImage>
```

### Paso 4: Compilar y Probar
```
1. Recompilar proyecto (Ctrl+Shift+B)
2. En ZWCAD: CUIUNLOAD -> Tandem 2026
3. En ZWCAD: MENULOAD -> bin\Debug\MNU\Tandem2026.cui
4. Verificar que el icono aparece
```

### Paso 5: Commit
```bash
git add .
git commit -m "Fix: Iconos funcionando en ZWCAD usando Support folder AB#619"
git push
```

---

## 📞 Comandos Rápidos de Recuperación

Para retomar el trabajo en otro chat:

```powershell
# Verificar estado del icono actual
Get-Item "TamdenZwcadPluging\ZwcadPlugin\MNU\img\SelectLines.png"

# Ver iconos en ZWCAD Support
Get-ChildItem "C:\Program Files\ZWSOFT\ZWCAD 2026\Support\*.png"

# Ver configuración actual en CUI
Select-String -Path "TamdenZwcadPluging\ZwcadPlugin\CuixBuilder.cs" -Pattern "SelectLines" -Context 2

# Abrir documentación
code TamdenZwcadPluging\ZwcadPlugin\INSTRUCCIONES_AGREGAR_ICONOS.md
code C:\00_Tandem2026\test_icons\TestIconoV2.cui
```

---

## ✅ Criterio de Aceptación

La US #619 estará **completa** cuando:

1. ✅ El comando `TANDEM_SELECCIONAR_LINEAS` existe y funciona
2. ❌ **PENDIENTE:** El icono aparece en el ribbon "Tandem 2026" > panel "Seleccion"
3. ✅ El tooltip muestra "Seleccionar Lineas - Permite seleccionar..."
4. ❌ **PENDIENTE:** El icono es visualmente distinguible (no solo texto)

---

## 🔗 Referencias

- **Bootstrap Icons:** `TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos\bootstrap-icons-1.13.1\`
- **Guía de Iconos:** `TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos\README_ICONOS.md`
- **Documentación ZWCAD CUI:** (buscar en documentación oficial de ZWSOFT)

---

**📌 Nota Final:** La solución está clara y probada. Solo falta ejecutar los pasos 1-5 para completar la implementación.
