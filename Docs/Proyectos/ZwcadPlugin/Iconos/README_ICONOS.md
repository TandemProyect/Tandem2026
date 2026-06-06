# Iconos para Menú Tandem 2026

**Ubicación:** `TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos\`

---

## 📁 Estructura de Carpetas

```
MNU\Iconos\
├── Bootstrap-Icons\     # Biblioteca completa de Bootstrap Icons
│   └── icons\           # Todos los SVG originales (2000+)
├── png\                 # Iconos convertidos para ZWCAD
│   ├── 16x16\          # Iconos pequeños
│   └── 32x32\          # Iconos grandes (ribbon)
├── README_ICONOS.md
├── Organizar_Bootstrap_Icons.ps1
└── Convertir_SVG_a_PNG.ps1
```

---

## 📋 Especificaciones de Iconos para ZWCAD

### Formatos Requeridos

ZWCAD soporta los siguientes formatos para iconos en el ribbon:

| Tamaño | Uso | Formato |
|--------|-----|---------|
| 16x16 px | Iconos pequeños (menús contextuales) | PNG, BMP |
| 32x32 px | Iconos grandes (ribbon buttons) | PNG, BMP |
| 64x64 px | Iconos extra grandes (opcional) | PNG, BMP |

**Recomendación:** Usar PNG con fondo transparente.

---

## 🎨 Iconos Necesarios

### Panel 1: Principal

| Comando | Nombre Archivo | Descripción | Palabras Clave para Buscar |
|---------|----------------|-------------|----------------------------|
| MVCCONEXION | `panel_16.png`<br>`panel_32.png` | Panel de control / Dashboard | dashboard, control panel, home |

**Sugerencia de color:** Azul (#1565C0)

---

### Panel 2: Modelo 3D

| Comando | Nombre Archivo | Descripción | Palabras Clave |
|---------|----------------|-------------|----------------|
| DETECTARMUROS | `detectar_16.png`<br>`detectar_32.png` | Lupa / Scanner / Radar | search, scan, detect, wall detection |
| GENERAR3D | `generar3d_16.png`<br>`generar3d_32.png` | Cubo 3D / Edificio 3D | 3d cube, building, model, generate |
| REGENERAR3D | `regenerar_16.png`<br>`regenerar_32.png` | Flechas circulares / Refresh | refresh, reload, regenerate, sync |
| CONFIGENCOFRADO | `encofrado_16.png`<br>`encofrado_32.png` | Engranaje / Ajustes / Molde | settings, gear, mold, formwork |

**Sugerencia de colores:**
- Detectar: Verde (#4CAF50)
- Generar: Azul (#2196F3)
- Regenerar: Naranja (#FF9800)
- Configurar: Gris (#757575)

---

### Panel 3: Datos MVC

| Comando | Nombre Archivo | Descripción | Palabras Clave |
|---------|----------------|-------------|----------------|
| LEERDISENOMVC | `leer_16.png`<br>`leer_32.png` | Flecha hacia abajo / Importar / Descargar | download, import, load, open folder |
| GUARDARDISENOMVC | `guardar_16.png`<br>`guardar_32.png` | Diskette / Flecha hacia arriba / Exportar | save, upload, export, floppy disk |

**Sugerencia de colores:**
- Leer: Verde (#4CAF50)
- Guardar: Azul (#2196F3)

---

## 🚀 Flujo de Trabajo

### 1. Descargar Bootstrap Icons (Una sola vez)

1. Ve a: https://github.com/twbs/icons/releases/latest
2. Descarga el archivo `bootstrap-icons-X.X.X.zip`
3. No lo descomprimas manualmente

### 2. Organizar la Biblioteca

```powershell
cd C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos
.\Organizar_Bootstrap_Icons.ps1
```

Este script:
- Busca el ZIP descargado
- Extrae todos los iconos a `Bootstrap-Icons\icons\`
- Copia los 7 iconos necesarios para Tandem 2026
- Crea un índice de todos los iconos disponibles

### 3. Convertir SVG a PNG

```powershell
.\Convertir_SVG_a_PNG.ps1
```

Este script:
- Requiere Inkscape instalado
- Convierte los SVG seleccionados a PNG
- Genera versiones 16x16 y 32x32
- Guarda en las carpetas `png\16x16\` y `png\32x32\`

### 4. Usar en ZWCAD

En `Tandem2026.cui`, referencia los iconos:
```
MNU\Iconos\png\16x16\panel.png
MNU\Iconos\png\32x32\panel.png
```

---

## 🔗 Enlaces Directos a Búsquedas (Flaticon)

### Panel Principal
- **Panel/Dashboard:** https://www.flaticon.com/search?word=dashboard
- **Control Panel:** https://www.flaticon.com/search?word=control%20panel

### Modelo 3D
- **Detectar/Lupa:** https://www.flaticon.com/search?word=search
- **Cubo 3D:** https://www.flaticon.com/search?word=3d%20cube
- **Regenerar:** https://www.flaticon.com/search?word=refresh
- **Engranaje:** https://www.flaticon.com/search?word=settings

### Datos MVC
- **Descargar:** https://www.flaticon.com/search?word=download
- **Guardar:** https://www.flaticon.com/search?word=save

---

## 📥 Cómo Descargar de Flaticon

1. Ve a https://www.flaticon.com
2. Busca el icono (ej: "3d cube")
3. Selecciona el icono que te guste
4. Haz clic en "Free download"
5. Selecciona tamaño: **32 px** (y después descarga también 16 px)
6. Formato: **PNG**
7. Descarga
8. Renombra según la tabla de arriba
9. Guarda en: `C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos\`

---

## 🎨 Alternativa: Crear Iconos Personalizados

Si quieres iconos personalizados con el mismo estilo:

### Herramientas Online Gratuitas:
- **Canva:** https://www.canva.com (plantillas de iconos)
- **Figma:** https://www.figma.com (diseño vectorial)
- **GIMP:** https://www.gimp.org (edición de imágenes)

### Especificaciones:
- Fondo transparente
- Líneas simples y claras
- Colores según la tabla de arriba
- Exportar en 16x16 y 32x32 px

---

## 📝 Checklist de Iconos

Una vez descargados, verifica:

- [ ] `panel_16.png` y `panel_32.png`
- [ ] `detectar_16.png` y `detectar_32.png`
- [ ] `generar3d_16.png` y `generar3d_32.png`
- [ ] `regenerar_16.png` y `regenerar_32.png`
- [ ] `encofrado_16.png` y `encofrado_32.png`
- [ ] `leer_16.png` y `leer_32.png`
- [ ] `guardar_16.png` y `guardar_32.png`

**Total:** 14 archivos (7 iconos × 2 tamaños)

---

## 🔧 Cómo Integrar en el Menú CUI

Una vez tengas los iconos, actualiza `Tandem2026.cui` para referenciarlos:

```xml
<MenuMacro UID="td_detect">
  <Macro>
	<Name>Detectar Muros</Name>
	<Command>^c^cDETECTARMUROS</Command>
	<SmallImage>Iconos\detectar_16.png</SmallImage>
	<LargeImage>Iconos\detectar_32.png</LargeImage>
  </Macro>
</MenuMacro>
```

**Nota:** Las rutas son relativas a la ubicación del archivo `.cui`.

---

## 📂 Estructura Final

```
MNU\
├── Tandem2026.cui
└── Iconos\
	├── panel_16.png
	├── panel_32.png
	├── detectar_16.png
	├── detectar_32.png
	├── generar3d_16.png
	├── generar3d_32.png
	├── regenerar_16.png
	├── regenerar_32.png
	├── encofrado_16.png
	├── encofrado_32.png
	├── leer_16.png
	├── leer_32.png
	├── guardar_16.png
	└── guardar_32.png
```

---

## 🎯 Próximos Pasos

1. **Descargar iconos** de Flaticon según la tabla
2. **Renombrar** según convención de nombres
3. **Guardar** en `MNU\Iconos\`
4. **Actualizar** `Tandem2026.cui` con referencias a los iconos
5. **Copiar** carpeta `Iconos` a `bin\Debug\MNU\Iconos\` (o configurar copia automática)
6. **Recargar** menú en ZWCAD para verificar

---

**Última actualización:** 24 de Abril de 2026
