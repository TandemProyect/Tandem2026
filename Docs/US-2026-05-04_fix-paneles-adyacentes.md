# US: Fix paneles adyacentes + unificación flujos

**Fecha**: 2026-05-04 | **Estado**: ✅ Completada

## User Story

Al seleccionar líneas de un plano con muros de doble línea, detectar TODAS las esquinas en L. Mismo algoritmo para líneas ZWCAD e imágenes GPT-4o.

## Tareas

### ✅ T1: Diagnosticar bug
**Archivo**: `Desing/Services/LCornerDetector.cs`

`HashSet<int> lineasUsadas` impedía que paneles adyacentes compartieran líneas. En muros de doble línea, esquinas vecinas comparten las líneas de la pared común.

### ✅ T2: Eliminar lineasUsadas
Se eliminó `lineasUsadas`. `panelesProcesados` (HashSet<string>) ya evita duplicados exactos.

**Resultado**: 2 → 4 paneles procesados. 12 → 24 círculos.

### ✅ T3: Unificar flujos
Eliminado `DetectarEsquinasLDesdeImagen` y parámetro `desdeImagen`. Ambos flujos usan el mismo `DetectarEsquinasL`.

### ✅ T4: Endpoint imagen
Añadido `DetectarEsquinasImagen` en `DesignToolsAutocadController.cs` + `using System.Threading.Tasks`.

### ✅ T5: Comando ZWCAD
- `Commands.cs`: `TANDEM_ANALIZAR_IMAGEN`
- `MVCApiService.cs`: `AnalizarImagenAsync`
- Radio círculos: 200mm

## Arquitectura

```
ZWCAD Plugin
  TANDEM_SELECCIONAR_LINEAS → líneas del dibujo
  TANDEM_ANALIZAR_IMAGEN    → imagen (JPEG/PNG)
         │ HTTP POST
         ▼
ASP.NET MVC Controller
  ProcesarLineasSeleccionadas → LCornerDetector
  DetectarEsquinasImagen      → GPT-4o → LCornerDetector
         │
         ▼
LCornerDetector (código único)
  1. Conexiones individuales (puntos donde se tocan)
  2. Pares de líneas paralelas
  3. Paneles rectangulares (4 líneas, 2 grupos paralelos)
  4. Validación offset ≤ 1500mm
  5. Puntos interior/exterior/colores
  6. Polilíneas (capa ObjetoDB2d + extrusión ModelDesing 2700mm)
```

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Desing/Services/LCornerDetector.cs` | Eliminado `lineasUsadas`, unificado |
| `Desing/Controllers/DesignToolsAutocadController.cs` | +`DetectarEsquinasImagen`, +`using System.Threading.Tasks` |
| `TamdenZwcadPluging/ZwcadPlugin/Commands.cs` | +`TANDEM_ANALIZAR_IMAGEN`, radio 200 |
| `TamdenZwcadPluging/ZwcadPlugin/MVCApiService.cs` | +`AnalizarImagenAsync` |
| `TamdenZwcadPluging/ZwcadPlugin/MNU/Tandem2026.cui` | +entrada menú imagen |
| `TamdenZwcadPluging/ZwcadPlugin/CuixBuilder.cs` | +builder UI imagen |
