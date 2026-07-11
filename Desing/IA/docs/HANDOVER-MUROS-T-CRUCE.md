# Handover — Muros 3D: uniones en T y en cruce (+)

Última actualización: 2026-07-10

Documento de continuidad para ampliar la extrusión 3D de muros en **Desing_2** más allá de **muro recto** (4 vértices) y **esquina L** (6 vértices). Incluye lo que ya está implementado, la lógica 2D reutilizable y las secciones que el equipo debe completar con reglas de negocio, diagramas y casos de prueba.

> Contexto producto y catálogo paneles **AT-60** (paso 0,15 m, esquina 0,30): [`ENCOFRADO-AT60-BASE.md`](ENCOFRADO-AT60-BASE.md)

---

## 1. Estado actual (resumen)

| Pieza | Muro recto (4v) | Esquina L (6v) | Unión T (8v) | Cruce + |
|-------|-----------------|----------------|--------------|---------|
| Dibujo 2D (ejes + caras) | ✅ | ✅ | ✅ caras recortadas | ✅ caras recortadas |
| `WallConnections.json` | ✅ | ✅ `junctionType: L` | ✅ `T` + `tCandidates` | ✅ `Cross` |
| API `ProcesarLineasZwcad` / `LCornerDetector` | ✅ rectos | ✅ (omitida en cliente AT-60) | ❌ | ❌ |
| Extrusión 3D rectos (API) | ✅ | — | — | — |
| Huella AT-60 cliente (`ma-stl-atk60-formwork.js`) | — | ✅ | ✅ | ❌ pendiente |
| ATK60 encofrado STL (`ModuloCruceT`, etc.) | — | — | referencia legacy | referencia legacy |

**Clasificación en cliente:**

- API: `Vertices.length >= 6` → omitir (esquina la resuelve AT-60 con E por eje).
- AT-60: 6v → `cornerL`, 8v → `cornerT` (`maAtk60ClassifyFootprintKind`).

**Documento de implementación:** [`ENCOFRADO-AT60-IMPLEMENTACION.md`](ENCOFRADO-AT60-IMPLEMENTACION.md)

---

## 2. Documentación y archivos relacionados

| Recurso | Contenido |
|---------|-----------|
| `HANDOVER-US-IMG-MUROS.md` | Tipificación muros rectos (tipos 1–4), flujo imagen/CAD, `LCornerDetector` |
| `HANDOVER-DESING2-MUROS-ARRANQUE.md` | Visor Desing_2, Ortho/F8, conexiones T al dibujar, `WallConnections.json` |
| `HANDOVER-LCornerDetector.md` | Detección esquina L en ZWCAD, puntos interior/exterior |
| `desing-2-stl-viewer-funciones.md` | Índice de funciones del visor (snap T-junction, marquee, etc.) |
| `IA/Communication/WallConnections.json` | Snapshot de depuración (líneas, nodos, `junctionType`, `tCandidates`) |
| `IA/Examples/README.md` | Casos de prueba — **Caso 03 (T)** y cruce aún sin ejemplo |

### Código principal

| Capa | Archivo | Rol |
|------|---------|-----|
| Cliente 3D | `Scripts/MasterArticles/master-article-details-stl-viewer.js` | Modos, API, bucket nodos, render AT-60 |
| Cliente AT-60 | `Scripts/MasterArticles/ma-stl-atk60-formwork.js` | Huellas L/T, `selectPanel`, meta encofrado |
| Servidor | `Services/LCornerDetector.cs` | Detección L + muros rectos → `PolilineasADibujar` |
| API | `Controllers/DesignToolsAutocadController.cs` → `ProcesarLineasZwcad` | Entrada líneas/caras, altura, espesor |
| ATK60 legacy | `Repositories/Atk60/Wall/ModuloCruceT.cs` | Piezas STL encofrado T (no conectado al visor nuevo) |

---

## 3. Lo ya resuelto: muro recto y esquina L (3D)

### 3.1 Muro recto

- **Entrada:** par de líneas paralelas (caras interior/exterior) o eje + offset.
- **Salida:** polígono cerrado de **4 vértices** en capa `ModelDesing`, `AlturaExtrusion = altura muro` (default 2700 mm).
- **Servidor:** `AgregarMuroRecto` en `LCornerDetector.cs` (tipos 1–4 según conectividad de extremos).
- **Cliente:** `maStlWall3dCreateMeshFromPolylineDto` → `THREE.ExtrudeGeometry`.

### 3.2 Esquina L

- **Entrada:** panel rectangular de 4 líneas (2 pares paralelos perpendiculares).
- **Salida:** polígono cerrado de **6 vértices**, orden US-675/679:  
  `Verde → Interior → Amarillo → Cian → Exterior → Blanco`
- **Servidor:** bloque en `DetectarEsquinasL` (~líneas 481–507 `LCornerDetector.cs`).
- **Cliente fallback 2D:** `maStlWall2dModelAddLCorner` (misma idea geométrica con `halfT`).
- **Regla cota:** perímetro exterior = referencia; espesor hacia interior (`HANDOVER-US-IMG-MUROS.md`).

---

## 4. Lógica 2D ya implementada (base para T y +)

El refactor `maStlWall2dToolRefactorAllWallJunctionsMm` se ejecuta tras dibujar/editar muros y hace dos fases:

1. **Ingletes L** entre cada par de ejes que comparten vértice (`maStlWall2dToolMiterFacePairAtVertexMm`).
2. **Uniones T y cruce** (`maStlWall2dToolRefactorTJunctionsFromBucket`).

### 4.1 Clasificación de nodo (`maStlWallConnectionsClassifyNode`)

Contando **ejes** (`wallRole === 'axis'`) incidentes en un vértice:

| `axisCount` | `uniqueDirCount` | `junctionType` |
|-------------|------------------|----------------|
| 0–1 | — | `Free` o `Connection` (solo caras) |
| 2 | 1 (colineales) | `Collinear` |
| 2 | 2 | `L` |
| 3 | — | `T` |
| 4 | — | `Cross` |
| >4 | — | `Multi` |

Además, `maStlWallConnectionsBuildJunctionDiagnostics` rellena `tCandidates` cuando hay 3+ ejes: par colineal opuesto (`through`) + rama (`branch`).

### 4.2 Unión en T (2D) — caras + huella AT-60

Tras el refactor 2D de caras, el **encofrado** en modo Muro 2D/3D usa `maAtk60BuildTFootprintMm` (no la API). Cotas:

| Elemento | Necesidad |
|----------|-----------|
| Ancho atravesado | `2 × E_atravesado` → panel |
| Vástago (altura) | `E_atravesado + 0,30` → panel |
| Luz interior | `2×0,30 + E_rama` → panel |
| Ancho rama | `E_rama` → panel |

**Función 2D caras:** `maStlWall2dToolApplyTJunctionMm(throughA, throughB, branch, vertexMm)`

**Precondiciones:**

- Dos ejes colineales `throughA` + `throughB` (mismo muro atravesado, segmentos opuestos desde el vértice).
- Un eje `branch` perpendicular que llega al vértice.
- Espesores coherentes en los tres `wallGroupId` (pueden diferir si la config lo permite).

**Geometría (resumen):**

1. Identificar la cara del muro atravesado **hacia la rama** (`nearSign`).
2. Intersectar las dos caras de la rama con la cara near del atravesado → dos puntos `nearBranchJoints`.
3. En el muro atravesado: la cara near recibe el joint más alejado en dirección de salida; la cara opuesta queda en offset simétrico al vértice.
4. Repetir en `throughA` y `throughB`.

**Al dibujar (snap):** con Ortho (F8) activo, el extremo se proyecta perpendicular al eje receptor (`maStlLineToolTryOrthoTPerpendicularEndMm`). Snap sobre **cara** se normaliza al **eje** (`maStlLineToolResolveWallAxisLineFromSnapLine`).

**Partición previa:** si un extremo cae en el **cuerpo** de otro eje, `maStlWall2dToolSplitAxisAtInteriorPointMm` parte el eje para que el vértice exista antes del refactor T.

### 4.3 Cruce + (2D)

**Funciones:**

- `maStlWall2dToolSplitAllAxisInteriorCrossingsMm` — parte ambos ejes en el punto de intersección interior (no en extremos).
- `maStlWall2dToolTryApplyCrossJunctionMm` — detecta dos pares colineales opuestos perpendiculares (4 ejes).
- `maStlWall2dToolApplyCrossJunctionMm` — recorta caras: cada eje se corta contra el bloqueador perpendicular (`maStlWall2dToolCutAxisFacesAgainstBlockingWallMm`).

**Precondición crítica:** tras el split, los **cuatro** brazos deben tener **extremo en el cruce** para que `junctionType` sea `Cross` y el refactor aplique.

### 4.4 Esquema `WallConnections.json` (uniones)

```json
{
  "nodeId": "N…",
  "junctionType": "T",
  "axisEndpoints": [
    { "lineId": 1, "endpoint": "p2", "directionFromNode": { "x": -1, "z": 0 } },
    { "lineId": 4, "endpoint": "p1", "directionFromNode": { "x": 0, "z": -1 } }
  ],
  "tCandidates": [
    {
      "throughAxisSegmentIds": [1, 2],
      "branchAxisSegmentId": 4,
      "throughWallGroupIds": [1, 1],
      "branchWallGroupId": 1
    }
  ]
}
```

Usar este JSON tras dibujar una T o un + real para validar clasificación antes de implementar 3D.

---

## 5. Hueco a cubrir: extrusión 3D en T y +

Hoy el flujo 3D es:

```
Caras seleccionadas / todas
  → POST ProcesarLineasZwcad (Lineas + AlturaMuroMm)
  → LCornerDetector (solo L + rectos)
  → PolilineasADibujar (ModelDesing)
  → maStlWall3dAddMeshesFromDetection
```

**No hay** generación de polígonos de unión T ni + en servidor ni en cliente 3D. Las caras 2D ya están bien recortadas; falta emitir **una o más polilíneas cerradas** por unión (como la L de 6 vértices) y clasificarlas en el visor.

### 5.1 Preguntas de negocio a cerrar (rellenar por el equipo)

Marcar cada ítem cuando esté definido con diagrama o imagen en `IA/Examples/`.

#### Unión en T

- [ ] **¿Una sola pieza 3D en el nodo T** o **solo muros rectos recortados** sin pieza de relleno?
- [ ] **Número de vértices** del polígono extruido (¿8, 10, 12…?).
- [ ] **Orden de vértices** (CW/CCW en planta, mismo criterio que L: exterior primero o interior primero).
- [ ] **Cota exterior:** ¿la rama conserva cara exterior alineada con el atravesado o hay regla distinta?
- [ ] **Espesores distintos** (rama E₁, atravesado E₂): ¿cómo se resuelve el nudo? (diagrama obligatorio)
- [ ] **Rama en T oblicua** (Ortho off): ¿válida para 3D o solo ortogonal?
- [ ] **Pieza ATK60:** ¿equivalente a `ModuloCruceT` / paneles `*T.stl` o geometría genérica de hormigón?

#### Cruce +

- [ ] **¿Pieza central** (cuadrado/diamante) + 4 rectos, o **4 rectos** que se solapan sin pieza?
- [ ] **Número de vértices** del polígono central (si aplica).
- [ ] **Orden de vértices** y referencia de cotas en las 4 direcciones.
- [ ] **Espesores iguales** vs mixtos en los cuatro brazos.
- [ ] **Ejes no ortogonales** (cruce oblicuo): ¿alcance v1?

### 5.2 Plantilla — definición unión T (completar)

```
Caso: T-90-esperando-documentacion
Espesor atravesado E_through: ___ mm
Espesor rama E_branch: ___ mm
Altura H: ___ mm (default 2700)

Vértices en planta (mm, XZ escena Desing_2):
  T1: ( ___, ___ )  — rol: ___
  T2: ( ___, ___ )  — rol: ___
  …

Orden de cierre: T1 → T2 → … → T1

Reglas:
  - Cara exterior atravesada: ___
  - Cara exterior rama: ___
  - Intersección con muros rectos adyacentes: ___

Imagen/DWG: IA/Examples/____/planta.dwg + resultado-esperado.png
```

### 5.3 Plantilla — definición cruce + (completar)

```
Caso: CROSS-90-esperando-documentacion
Espesor E: ___ mm (¿único para los 4 brazos?)
Altura H: ___ mm

Vértices pieza central (si aplica):
  C1: ( ___, ___ )  — rol: ___
  …

Orden de cierre: C1 → … → C1

Reglas:
  - ¿Los 4 brazos se recortan hasta el interior del cruce o hasta el eje?
  - Simetría: ___

Imagen/DWG: IA/Examples/____/
```

---

## 6. Propuesta técnica de implementación

### 6.1 Hecho (2026-07-10) — encofrado AT-60 en cliente

Ver [`ENCOFRADO-AT60-IMPLEMENTACION.md`](ENCOFRADO-AT60-IMPLEMENTACION.md).

- Módulo `ma-stl-atk60-formwork.js`: L (6v) y T (8v) con `selectPanel` y tacos.
- Integración en visor: bucket de nodos, render 2D/3D, omisión esquinas API ≥6v.
- Espesor E por eje vía `numberOffsetMm` en caras.
- `maStlWall2dModelRegenerateFromSourceLines` llama huellas AT-60 tras tiras/segmentos.

### 6.2 Pendiente

Orden sugerido:

1. **Ejemplos** en `IA/Examples/` (Caso 03 T, Caso 05 cruce +) con `maStlAtk60FormworkMeta` esperado.
2. **Cruce +** — `maAtk60BuildCrossFootprintMm` (cliente) y/o servidor genérico.
3. **Servidor opcional** — `LCornerDetector` T/+ genérico si se quiere extrusión sin AT-60.
4. **Rectos** — descomposición P30…P90 y STL catálogo.
5. **Validación visual** — distintos E (0,2345, 0,45+0,25 T, E asimétrico en L).

### Criterios de aceptación (borrador)

- T ortogonal 90°, mismo espesor: sin huecos ni solapes visibles en planta; extrusión altura H; cotas exteriores coherentes con 2D.
- + ortogonal 90°, mismo espesor: cuatro brazos encuentran el nudo; `junctionType: Cross` en JSON.
- Regenerar modo Muro 3D tras editar espesor en Configuración Desing_2.
- Diagnóstico explica cada pieza T/+ generada o descartada (motivo).

---

## 7. Referencia rápida — funciones cliente

| Función | Uso |
|---------|-----|
| `maStlWall2dToolApplyTJunctionMm` | Cierre 2D T |
| `maStlWall2dToolApplyCrossJunctionMm` | Cierre 2D + |
| `maStlWall2dToolSplitAllAxisInteriorCrossingsMm` | Split ejes en cruce |
| `maStlWallConnectionsClassifyNode` | Tipo de nodo en JSON |
| `maStlWall3dClassifyPolyline` | Tipo pieza 3D (ampliar) |
| `maStlDesing2RequestWallModelDetection` | Regenerar todo el modelo 3D |

---

## 8. Próximo paso para el usuario / equipo

1. Completar las secciones **§5.1–5.3** con diagramas (papel o CAD) de al menos:
   - T 90° mismo espesor.
   - T 90° espesor rama ≠ atravesado (si aplica en producción).
   - + 90° mismo espesor.
2. Subir capturas a `IA/Examples/2026-07-10-caso-03-t/` y `…-caso-05-cross/`.
3. Indicar **orden exacto de vértices** y si la pieza T/+ es una sola extrusión o varias.
4. Con eso, el agente puede implementar en `LCornerDetector` + clasificación 3D sin reescribir el flujo 2D.

---

## Changelog

| Fecha | Cambio |
|-------|--------|
| 2026-07-10 | Creación del documento: inventario recto/L, lógica 2D T/+, plantillas pendientes para 3D |
