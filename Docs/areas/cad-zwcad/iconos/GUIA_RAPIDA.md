# 🎨 Sistema de Iconos - Tandem 2026

## ✅ Estructura Creada

```
MNU\Iconos\
├── Bootstrap-Icons\         # 📚 Biblioteca completa (>2000 iconos)
│   └── icons\
│       ├── .gitkeep        # Mantiene carpeta en Git
│       └── [SVG files]     # Ignorados por .gitignore
│
├── png\                     # 🖼️ Iconos convertidos para ZWCAD
│   ├── 16x16\              # Pequeños (menús)
│   │   └── .gitkeep
│   └── 32x32\              # Grandes (ribbon)
│       └── .gitkeep
│
├── .gitignore              # Excluye biblioteca del repo
├── README_ICONOS.md        # Documentación completa
├── Organizar_Bootstrap_Icons.ps1   # Script 1: Descarga y organiza
└── Convertir_SVG_a_PNG.ps1         # Script 2: Convierte SVG→PNG
```

---

## 🚀 Próximos Pasos

### 1️⃣ Descargar Bootstrap Icons (TÚ)

**Ya está abierta la página en tu navegador:**
👉 https://github.com/twbs/icons/releases/latest

**Acción:**
- Descarga `bootstrap-icons-X.X.X.zip`
- Déjalo en tu carpeta **Descargas**
- **NO lo descomprimas**

---

### 2️⃣ Ejecutar Script de Organización (TÚ)

```powershell
cd C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos
.\Organizar_Bootstrap_Icons.ps1
```

**Qué hace:**
- ✅ Busca el ZIP automáticamente
- ✅ Extrae >2000 iconos a `Bootstrap-Icons\icons\`
- ✅ Copia los 7 iconos necesarios para Tandem 2026:
  - `speedometer2.svg` → `panel.svg`
  - `search.svg` → `detectar.svg`
  - `box.svg` → `generar3d.svg`
  - `arrow-clockwise.svg` → `regenerar.svg`
  - `gear.svg` → `encofrado.svg`
  - `download.svg` → `leer.svg`
  - `save.svg` → `guardar.svg`
- ✅ Crea índice de todos los iconos

---

### 3️⃣ Convertir SVG a PNG (TÚ)

**Necesitas Inkscape instalado:**
- Si no lo tienes: https://inkscape.org/release/

**Luego ejecuta:**
```powershell
.\Convertir_SVG_a_PNG.ps1
```

**Qué hace:**
- ✅ Convierte los 7 SVG seleccionados
- ✅ Genera versiones 16x16 → `png\16x16\`
- ✅ Genera versiones 32x32 → `png\32x32\`

---

### 4️⃣ Integrar en ZWCAD (COPILOT)

**Pendiente:**
- [ ] Actualizar `ZwcadPlugin.csproj` para copiar `png\` a output
- [ ] Editar `Tandem2026.cui` para referenciar los PNG:
  ```
  MNU\Iconos\png\16x16\panel.png
  MNU\Iconos\png\32x32\panel.png
  ```

---

## 📦 Qué NO se sube a Git

Gracias al `.gitignore`, **NO se subirán** al repositorio:
- ❌ Los >2000 iconos SVG originales (muy pesado)
- ❌ Los PNG generados localmente

**Solo se suben:**
- ✅ Scripts de automatización
- ✅ Documentación
- ✅ Estructura de carpetas (`.gitkeep`)

---

## 🎯 Ventajas de esta Estructura

### Para Tandem 2026:
- 7 iconos profesionales listos para el ribbon
- Conversión automatizada (no manual)
- Separación clara SVG vs PNG

### Para Futuros Proyectos:
- **Biblioteca completa disponible** (>2000 iconos)
- Reutilizable en AutoCAD, Revit, otros CADs
- Indexada y buscable
- Sin dependencia de servicios online

---

## 🔄 Workflow Completo

```
1. Descargar ZIP (manual, una vez)
		  ↓
2. Organizar_Bootstrap_Icons.ps1 (automático)
		  ↓
3. Convertir_SVG_a_PNG.ps1 (automático)
		  ↓
4. Actualizar .csproj y .cui (pendiente)
		  ↓
5. Compilar y cargar en ZWCAD
```

---

## 📝 Comandos Rápidos

```powershell
# Ver estructura
cd C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\MNU\Iconos
tree /F /A

# Ejecutar scripts
.\Organizar_Bootstrap_Icons.ps1
.\Convertir_SVG_a_PNG.ps1

# Ver estado Git
git status
```

---

## ⚠️ Notas Importantes

1. **El ZIP de Bootstrap Icons se descarga FUERA del repo**
   - No lo pongas en la carpeta del proyecto
   - El script lo busca en tu carpeta Descargas

2. **La biblioteca completa es local**
   - Cada desarrollador la descarga una vez
   - No se sincroniza por Git (demasiado grande)

3. **Los PNG finales SÍ se podrían subir a Git**
   - Solo 14 archivos (7 iconos × 2 tamaños)
   - Para eso, descomenta líneas en `.gitignore` después

---

## 🎉 Estado Actual

✅ Estructura de carpetas creada  
✅ Scripts de automatización listos  
✅ Documentación completa  
✅ `.gitignore` configurado  
✅ Commit realizado: `a5d5258`  
⏳ **ESPERANDO:** Descarga del ZIP de Bootstrap Icons  

---

**Siguiente acción:** Descargar el ZIP y ejecutar `Organizar_Bootstrap_Icons.ps1`
