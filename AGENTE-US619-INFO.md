# 📋 INFORMACIÓN PARA AGENTE - US-619 (COMPLETADA)

## 🎯 Work Item Azure DevOps

- **ID:** 619
- **Título:** Insertar Img en Command Seleccionar Muro
- **Estado:** Done ✅
- **Tipo:** Issue
- **Story Points:** 1
- **URL:** https://dev.azure.com/VSCAD/213253e7-f177-4e2d-bdf3-410b97f6883d/_workitems/edit/619

**Descripción:**
> Agregar un icono/imagen visual al comando de seleccion de muros en el ribbon y menu de ZWCAD. El icono debe ser claro y representativo de la accion de seleccionar muros. Effort estimado: 1 punto.

**Objetivo del Sistema:**
> Este comando forma parte del sistema de detección de esquinas para el **Sistema ATK60**, un sistema de encofrado que requiere la identificación precisa de puntos en las esquinas de estructuras rectangulares para su posterior encofrado.

---

## 🏗️ Sistema ATK60

### ¿Qué es el Sistema ATK60?
El **ATK60** es un sistema de encofrado modular utilizado en construcción que requiere:
- Detección precisa de esquinas en estructuras rectangulares
- Identificación de puntos de conexión entre paredes perpendiculares
- Validación de dimensiones y offsets entre elementos paralelos
- Preparación de puntos de anclaje para el sistema de encofrado

### Requisitos del Sistema ATK60
1. **Detección de Esquinas en L**: Identificar intersecciones perpendiculares entre muros
2. **Validación de Offset**: Las líneas paralelas deben tener una separación ≤ 1500mm (ancho máximo de panel)
3. **Marcado Visual**: Dibujar círculos o marcas en los puntos críticos para instalación
4. **Soporte Multi-orientación**: Detectar esquinas independientemente de la rotación del diseño

### Implementación Actual
La **US-619** (completada) implementó la **fase inicial** del sistema ATK60:
- ✅ Detección automática de esquinas en L
- ✅ Validación de offset máximo (1500 unidades)
- ✅ Marcado visual con círculos rojos en puntos de conexión
- ✅ Exportación de datos diagnósticos en JSON

### Próxima Fase - US-638
La **US-638** "Detectar puntos para sistema ATK 60" (8 story points) continuará el desarrollo:
- 🎯 **Objetivo:** Implementar detección de puntos críticos de instalación para ATK60
- 🔗 **URL:** https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/638
- 📋 **Tareas:** 8 tareas creadas (#639-#646)
- 📚 **Documentación:** Ver `SISTEMA-ATK60.md` para detalles completos

---

## 📊 Contexto del Proyecto

### Organización Azure DevOps
- **Organización:** VSCAD
- **Proyecto:** tandem2026
- **Panel:** https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
- **PAT:** Está en los scripts en `C:\00_Tandem2026\Scripts\` (no necesitas configurarlo)

### Repositorio GitHub
- **Repo:** https://github.com/JuanGodoyLopez/Tandem-2026
- **Branch:** master
- **Usuario Git:** Juan Andrés Godoy López <jag@vscad.com>
- **Workspace:** C:\00_Tandem2026\

---

## 🔍 Problema Actual

### ¿Qué funciona?
- ✅ El comando `TANDEM_SELECCIONAR_LINEAS` funciona correctamente
- ✅ El botón aparece en el ribbon "Tandem 2026" → panel "Seleccion"
- ✅ Al hacer clic, ejecuta el comando sin problemas

### ¿Qué NO funciona?
- ❌ **El icono no se muestra en el ribbon** (solo aparece el texto "Seleccionar")
- ❌ El archivo PNG actual está corrupto (solo 243 bytes)
- ❌ Las rutas relativas (`img\SelectLines.png`) no funcionan en ZWCAD

---

## 🔍 Diagnóstico Técnico

### Causa Raíz Identificada

**ZWCAD solo muestra iconos cuando:**
1. Están en: `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\`
2. El CUI usa solo el nombre: `<LargeImage>SelectLines.png</LargeImage>`

**NO funciona con:**
- ❌ Rutas relativas: `img\SelectLines.png`
- ❌ Rutas absolutas: `C:\00_Tandem2026\...\SelectLines.png`
- ❌ Iconos en carpetas del proyecto

### Validación Realizada

Se creó un CUI de prueba (`TestIconoV2.cui`) que **SÍ funciona:**
- **Archivo:** `C:\00_Tandem2026\test_icons\TestIconoV2.cui`
- **Icono:** `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\simple_square.png`
- **Resultado:** ✅ Icono visible en ribbon

---

## 📂 Archivos Relevantes

### Archivos del Plugin ZWCAD

```
C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\
├── MNU\
│   ├── img\
│   │   ├── SelectLines.png      ⚠️ 243 bytes - CORRUPTO
│   │   └── SelectLines.svg      ℹ️ 1597 bytes
│   └── Tandem2026.cui           ℹ️ CUI fuente
│
├── CuixBuilder.cs               🔧 Genera el CUI dinámicamente
│
├── Commands.cs                  ✅ Comando TANDEM_SELECCIONAR_LINEAS
│
└── ZwcadPlugin.csproj           📦 Configuración del proyecto
```

### Código Actual - CuixBuilder.cs

**Ubicación:** `TamdenZwcadPluging/ZwcadPlugin/CuixBuilder.cs`

**Sección del icono (líneas ~100-110):**
```csharp
<MenuMacro UID=\"td_seleccionar\">
  <Macro>
	<Name>Seleccionar Lineas</Name>
	<Command>^c^cTANDEM_SELECCIONAR_LINEAS</Command>
	<HelpString>Permite seleccionar lineas y polilineas en el dibujo</HelpString>
	<LargeImage>img\\SelectLines.png</LargeImage>  ⚠️ NO FUNCIONA
	<SmallImage>img\\SelectLines.png</SmallImage>  ⚠️ NO FUNCIONA
  </Macro>
</MenuMacro>
```

---

## 🎯 Solución a Implementar

### Pasos Requeridos

#### 1. Generar PNG Válido
- **Tamaño:** 32x32 píxeles (para LargeImage)
- **Formato:** PNG con fondo transparente
- **Diseño:** Icono representativo de "seleccionar líneas/muros"
- **Guardar en:** `C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\img\SelectLines.png`

#### 2. Copiar Icono a ZWCAD Support
```powershell
Copy-Item "C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\img\SelectLines.png" `
		  -Destination "C:\Program Files\ZWSOFT\ZWCAD 2026\Support\SelectLines.png"
```

#### 3. Actualizar CuixBuilder.cs
```csharp
// Cambiar de:
<LargeImage>img\\SelectLines.png</LargeImage>
<SmallImage>img\\SelectLines.png</SmallImage>

// A:
<LargeImage>SelectLines.png</LargeImage>
<SmallImage>SelectLines.png</SmallImage>
```

#### 4. Recompilar Plugin
```powershell
cd C:\00_Tandem2026
# Build del proyecto en Visual Studio o con MSBuild
```

#### 5. Probar en ZWCAD
- Cargar plugin actualizado
- Verificar que el icono aparece en el ribbon

---

## 📚 Documentación Existente

### Documentos de Investigación
1. **`Docs/Proyectos/ZwcadPlugin/INVESTIGACION_ICONOS_US619.md`** (348 líneas)
   - Investigación completa del problema
   - Pruebas realizadas
   - Diagnóstico técnico detallado
   - Ejemplos de código

2. **`US619_RESUMEN_ICONOS.md`** (57 líneas)
   - Resumen ejecutivo del estado
   - Solución validada
   - Pasos pendientes

### Documentación de Gestión de Panel

**Ubicación:** `C:\00_Tandem2026\Docs\General\GESTION-PANEL-AZURE-DEVOPS.md`

**Contenido clave:**
- Configuración correcta de Azure DevOps (org: VSCAD, proyecto: tandem2026)
- Scripts disponibles para gestionar Work Items
- Cómo editar US, cambiar estados, adjuntar documentación
- Workflow completo de desarrollo

---

## 🔧 Scripts Disponibles

### Ubicación: `C:\00_Tandem2026\Scripts\`

#### Create-US-Fast.ps1 ⭐
```powershell
# Crear nueva US con Story Points
.\Create-US-Fast.ps1 -Titulo "Título" -Descripcion "Desc" -StoryPoints 8
```

#### Edit-US.ps1
```powershell
# Cambiar estado de US
.\Edit-US.ps1 -ID 619 -Estado "Done"

# Cambiar título
.\Edit-US.ps1 -ID 619 -Titulo "Nuevo título"
```

#### Attach-Document.ps1
```powershell
# Adjuntar documentación a US
.\Attach-Document.ps1 -WorkItemId 619 -FilePath "archivo.md" -Comment "Documentación"
```

---

## ✅ Criterios de Completitud

### Cuando marcar US-619 como "Done"

La US-619 estará completa cuando:

1. ✅ Icono PNG válido generado (32x32 px)
2. ✅ Icono copiado a `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\`
3. ✅ CuixBuilder.cs actualizado (sin ruta `img\`)
4. ✅ Plugin recompilado sin errores
5. ✅ **Icono visible en ribbon de ZWCAD**
6. ✅ Comando sigue funcionando correctamente
7. ✅ Commit realizado con mensaje: `"feat(US-619): Agregar icono SelectLines a ribbon ZWCAD AB#619"`
8. ✅ Push a GitHub completado
9. ✅ Documentación técnica adjunta a US-619 en Azure DevOps
10. ✅ US movida a estado "Done"

---

## 📝 Workflow Recomendado

```powershell
# 1. Implementar la solución (generar PNG, actualizar código, etc.)

# 2. Build y prueba en ZWCAD
# (verificar que el icono aparece)

# 3. Commit con vínculo a US
git add .
git commit -m "feat(US-619): Agregar icono SelectLines a ribbon ZWCAD AB#619"
git push origin master

# 4. Crear documentación técnica (ej: US-619-IMPLEMENTACION.md)

# 5. Mover US a Done
cd C:\00_Tandem2026\Scripts
.\Edit-US.ps1 -ID 619 -Estado "Done"

# 6. Adjuntar documentación
.\Attach-Document.ps1 -WorkItemId 619 -FilePath "US-619-IMPLEMENTACION.md" -Comment "Documentación implementación icono ribbon"
```

---

## 🔗 Enlaces Rápidos

- **US-619 en Azure DevOps:** https://dev.azure.com/VSCAD/213253e7-f177-4e2d-bdf3-410b97f6883d/_workitems/edit/619
- **Panel del Proyecto:** https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
- **Repo GitHub:** https://github.com/JuanGodoyLopez/Tandem-2026
- **Doc Principal Azure DevOps:** `C:\00_Tandem2026\Docs\General\GESTION-PANEL-AZURE-DEVOPS.md`
- **Doc Create-US-Fast:** `C:\00_Tandem2026\Docs\Scripts\CREATE-US-FAST.md`

---

## ⚠️ Notas Importantes

1. **No perderás acceso a agentes:** Los scripts usan PAT hardcodeado, independiente de tu sesión Git
2. **Usuario Git actual:** jag@vscad.com (Juan Andrés Godoy López)
3. **Para commits manuales:** Usa Git Changes en Visual Studio (NO conectes Team Explorer a Azure DevOps)
4. **Formato de commit:** Usar prefijo convencional: `feat(US-619): mensaje AB#619`

---

**Fecha:** 2026-04-25  
**Preparado para:** Nuevo agente especializado en US-619  
**Usuario:** jag@vscad.com
