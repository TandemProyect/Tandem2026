# 📝 Instrucciones para Agregar Iconos a Nuevos Comandos

## ✅ Lista de Verificación (Checklist)

Cuando agregues un **nuevo comando con icono** al plugin ZWCAD, sigue estos pasos:

### 1. **Crear/Copiar el Icono**
```powershell
# Copiar un icono existente de Bootstrap Icons
Copy-Item "MNU\Iconos\bootstrap-icons-1.13.1\<nombre>.svg" "MNU\img\<NombreComando>.png"

# O crear uno manualmente en MNU\img\
```

**Requisitos del icono:**
- ✅ Formato: **PNG** (ZWCAD no soporta SVG bien)
- ✅ Tamaño: **32x32 píxeles** mínimo
- ✅ Ubicación: `TamdenZwcadPluging\ZwcadPlugin\MNU\img\`

---

### 2. **Incluir el Icono en el Proyecto (.csproj)**

**Opción A: Desde Visual Studio (Recomendado)**

1. En el **Explorador de Soluciones**, haz clic derecho en el proyecto `ZwcadPlugin`
2. **Add > Existing Item...**
3. Navega a `MNU\img\<NombreComando>.png` y selecciónalo
4. Haz clic derecho en el archivo añadido > **Properties**
5. Cambia **"Copy to Output Directory"** a **"Copy always"** o **"Copy if newer"**

**Opción B: Editar .csproj manualmente**

Cierra Visual Studio y añade dentro de `<ItemGroup>`:

```xml
<Content Include="MNU\img\<NombreComando>.png">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</Content>
```

---

### 3. **Actualizar el Target de Compilación (Opcional pero Recomendado)**

Si quieres que **todos los archivos** de `MNU\img\` se copien automáticamente sin tener que añadirlos uno por uno:

1. **Cierra la solución** en Visual Studio
2. Abre `ZwcadPlugin.csproj` con un editor de texto
3. Busca la sección `<Target Name="CopiarMNU"` (cerca del final)
4. Reemplázala con:

```xml
<Target Name="CopiarMNU" AfterTargets="Build">
  <Copy SourceFiles="MNU\Tandem2026.cui" DestinationFolder="$(OutputPath)MNU\" SkipUnchangedFiles="true" />
  <!-- Copia la carpeta img con todos los iconos -->
  <ItemGroup>
	<ImgFiles Include="MNU\img\**\*.*" />
  </ItemGroup>
  <Copy SourceFiles="@(ImgFiles)" DestinationFolder="$(OutputPath)MNU\img\%(RecursiveDir)" SkipUnchangedFiles="true" />
</Target>
```

5. Guarda y **reabre la solución**

---

### 4. **Actualizar CuixBuilder.cs**

Añade el nuevo `MenuMacro` en `CuixBuilder.cs` dentro de `<MacroGroup Name="TD-Main">`:

```csharp
<MenuMacro UID="td_nuevo_comando">
  <Macro>
	<Name>Nombre del Comando</Name>
	<Command>^c^cNOMBRE_COMANDO</Command>
	<HelpString>Descripcion del comando</HelpString>
	<LargeImage>img\NombreComando.png</LargeImage>
	<SmallImage>img\NombreComando.png</SmallImage>
  </Macro>
</MenuMacro>
```

**⚠️ Importante:** 
- La ruta del icono es **relativa** al archivo `.cui`: `img\NombreComando.png`
- **NO uses** `MNU\img\...` dentro del XML

---

### 5. **Añadir el Botón al Ribbon**

En la sección `<RibbonPanelSourceCollection>`, añade el botón al panel correspondiente:

```csharp
<RibbonCommandButton ButtonStyle="LargeWithText" MenuMacroID="td_nuevo_comando" Text="Texto Boton"/>
```

**Ejemplo:**

```csharp
<RibbonPanelSource Text="Seleccion" UID="td_ribbon_panel4">
  <Name>Herramientas</Name>
  <RibbonRow>
	<RibbonCommandButton ButtonStyle="LargeWithText" MenuMacroID="td_seleccionar" Text="Seleccionar"/>
	<RibbonCommandButton ButtonStyle="LargeWithText" MenuMacroID="td_nuevo_comando" Text="Nuevo"/>
  </RibbonRow>
</RibbonPanelSource>
```

---

### 6. **Compilar y Probar**

1. **Compila el proyecto** (Ctrl+Shift+B)
2. Verifica que el archivo se copió:
   ```powershell
   Get-Item "TamdenZwcadPluging\ZwcadPlugin\bin\Debug\MNU\img\NombreComando.png"
   ```
3. **En ZWCAD:**
   - Ejecuta `CUIUNLOAD` y descarga "Tandem 2026"
   - Ejecuta `MENULOAD` y carga `bin\Debug\MNU\Tandem2026.cui`
   - Verifica que el icono aparece en el ribbon

---

## 🎯 Ejemplo Completo: Comando "Seleccionar Líneas"

### Estructura de Archivos
```
TamdenZwcadPluging/ZwcadPlugin/
├── MNU/
│   ├── img/
│   │   ├── SelectLines.png      ← Icono 32x32 PNG
│   │   └── SelectLines.svg      ← Opcional (no usado)
│   └── Tandem2026.cui           ← Generado por CuixBuilder
├── CuixBuilder.cs               ← Define el XML del CUI
└── ZwcadPlugin.csproj           ← Incluye el icono con CopyToOutputDirectory
```

### En CuixBuilder.cs

```csharp
<MenuMacro UID="td_seleccionar">
  <Macro>
	<Name>Seleccionar Lineas</Name>
	<Command>^c^cTANDEM_SELECCIONAR_LINEAS</Command>
	<HelpString>Permite seleccionar lineas y polilineas en el dibujo</HelpString>
	<LargeImage>img\SelectLines.png</LargeImage>
	<SmallImage>img\SelectLines.png</SmallImage>
  </Macro>
</MenuMacro>
```

### En ZwcadPlugin.csproj

```xml
<ItemGroup>
  <Content Include="MNU\img\SelectLines.png">
	<CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

---

## 🚨 Problemas Comunes

### ❌ El icono no aparece en ZWCAD

**Causa:** El archivo no se copió a `bin\Debug\MNU\img\`

**Solución:**
1. Verifica que el icono está incluido en el `.csproj` con `<CopyToOutputDirectory>Always</CopyToOutputDirectory>`
2. Recompila el proyecto
3. Verifica manualmente que el archivo existe en `bin\Debug\MNU\img\`

---

### ❌ Solo aparece el texto del botón, sin icono

**Causa:** ZWCAD no encontró el archivo o la ruta es incorrecta

**Solución:**
1. La ruta en `CuixBuilder.cs` debe ser relativa al `.cui`: `img\NombreComando.png`
2. **NO uses** rutas absolutas ni `MNU\img\...`
3. Usa **PNG**, no SVG

---

### ❌ El icono es muy pequeño o borroso

**Causa:** El archivo PNG es menor de 32x32 píxeles

**Solución:**
- Crea un icono de **al menos 32x32 píxeles**
- Para mejor calidad en pantallas de alta resolución, usa **64x64 píxeles**

---

## 📚 Referencias

- **Iconos disponibles:** `MNU\Iconos\bootstrap-icons-1.13.1\`
- **Documentación de iconos:** `Docs\Proyectos\ZwcadPlugin\Iconos\README_ICONOS.md`
- **Guía de integración:** `Docs\Proyectos\ZwcadPlugin\Iconos\INSTRUCCIONES_CSPROJ.md`

---

## ✅ Resumen Rápido

```powershell
# 1. Copiar icono
Copy-Item "MNU\Iconos\bootstrap-icons-1.13.1\line.svg" "MNU\img\MiComando.png"

# 2. Añadir al proyecto (desde VS: Add > Existing Item > Properties > Copy always)

# 3. Actualizar CuixBuilder.cs (añadir MenuMacro y RibbonCommandButton)

# 4. Compilar
dotnet build

# 5. Verificar
Get-Item "bin\Debug\MNU\img\MiComando.png"

# 6. Recargar en ZWCAD
# CUIUNLOAD > Tandem 2026
# MENULOAD > bin\Debug\MNU\Tandem2026.cui
```

---

**Última actualización:** 2026-04-25  
**US asociada:** #619 - Insertar Img en Command Seleccionar Muro
