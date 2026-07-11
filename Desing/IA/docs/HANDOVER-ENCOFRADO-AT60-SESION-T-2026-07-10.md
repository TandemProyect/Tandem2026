# Handover sesión — Encofrado AT-60, unión T (2026-07-10)

Documento para **continuidad entre agentes**. Resume el trabajo realizado en el visor Desing_2 sobre muros rectos + unión T, errores que NO repetir, estado actual del código y próximos pasos.

> Contexto general: [ENCOFRADO-AT60-BASE.md](ENCOFRADO-AT60-BASE.md)  
> Código integrado: [ENCOFRADO-AT60-IMPLEMENTACION.md](ENCOFRADO-AT60-IMPLEMENTACION.md)  
> Topología T/+: [HANDOVER-MUROS-T-CRUCE.md](HANDOVER-MUROS-T-CRUCE.md)

---

## 1. Objetivo del producto (recordatorio)

| Capa | Qué es | Estado |
|------|--------|--------|
| **Muro arquitectónico** (gris) | Extrusión desde caras fuente (`facePos` / `faceNeg` por eje) | ✅ Rectos en prueba usuario |
| **Forma blanca AT-60** (amarillo) | Huella en planta → extruir; cotas cian = panel catálogo | 🟡 T en **modo guía** (sin extruir) |
| **Líneas fuente** | Ejes/caras persisten; mallas se regeneran al cambiar modo | ✅ |

**Regla de negocio:** en unión T el muro atravesado se **parte en dos tramos rectos**; en el hueco va la pieza de esquina T. Los extremos de los rectos deben ser **planos ⊥ al eje**, no oblicuos (ingletes 2D de caras no deben deformar el sólido 3D).

---

## 2. Caso de prueba del usuario

**JSON:** `Desing/IA/Communication/WallConnections.json` (exportado desde visor).

**Topología en N2** (≈ 9944, 0, 0):

| Eje | Rol | Dirección |
|-----|-----|-----------|
| **10** | Atravesado (izq.) | X, termina en N2 (`p2`) |
| **13** | Atravesado (der.) | +X, empieza en N2 (`p1`) |
| **16** | Rama | −Z desde N2 (`p1` → `p2`) |

`junctionType: "T"` en nodo N2.

**Espesor típico en prueba:** `numberOffsetMm: 150` → **E = 300 mm** por eje.

**Boceto de referencia (AutoCAD):** polígono blanco **P-1…P-8** en el hueco entre los tres rectos azules; puntos en **caras** de muro, no en el eje. Guardar copias en `Desing/IA/Examples/` cuando se valide.

---

## 3. Qué está hecho y funciona

### 3.1 Muros rectos desde caras fuente

- `maStlWall2dModelCollectWallFaceStrips()` — agrupa eje + `facePos` + `faceNeg`.
- `maStlDesing2BuildWallModelFromFaceStrips(mode)` — prioridad sobre API `LCornerDetector`.
- 3D: `maStlWall3dAddMeshesFromFaceStrips(faceStrips, axisEndpointTrimMap)`.

### 3.2 Recorte en uniones (hueco para pieza T/L)

- `maAtk60GetTJunctionTrimMm` / `maAtk60GetLJunctionTrimMm` en `ma-stl-atk60-formwork.js`.
- `maStlAtk60BuildAxisEndpointTrimMap()` — `Map` clave `axisId|p1` o `axisId|p2` → mm a recortar.

| Unión | Eje | Recorte en vértice |
|-------|-----|------------------|
| **T** | cada pata atravesado | `throughHalfMm = panel(2×E_atr) / 2` |
| **T** | rama | `branchStemMm = panel(E_atr + 0,30)` |
| **L** | cada pata | `leg1Mm` / `leg2Mm` = panel(E + 0,30) |

### 3.3 Tapas rectas en extremos recortados (fix caras oblicuas)

**Problema:** al recortar siguiendo extremos de caras 2D (ingletes T), los rectos terminaban en **cara oblicua**.

**Solución:** `maStlWall2dModelStripSquareCapAtAxisMm` — corte **⊥ al eje** en el punto del eje + offset a caras con `maStlWall2dToolFaceOffsetPointAtAxisPointMm`.

Usado en `maStlWall2dModelBuildStripQuadXzMm` cuando `trimP1 > 0` o `trimP2 > 0`.

### 3.4 Esquina L AT-60

- `maAtk60BuildLFootprintMm` — 6 vértices; render + extrusión OK.
- Sin cambios relevantes en esta sesión.

### 3.5 Unión T — modo guía (estado actual)

**NO extruir** hasta validar boceto con el usuario.

| Flag | Valor | Ubicación |
|------|-------|-----------|
| `MA_STL_ATK60_T_EXTRUDE_3D` | `false` | `master-article-details-stl-viewer.js` (~5242) |

**Render T:** `maStlAtk60AddTFootprintPointGuide` dibuja:

- `LineLoop` blanco cerrado P-1→…→P-2
- Esfera + aro blanco por vértice (radio 60 mm)
- Etiquetas `P-1`…`P-8` (`maStlMakeTextSprite`, font 8px, escala 85 mm)
- Colores por punto (índice en array): rojo, naranja, amarillo, verde, cian, azul, violeta, rosa

**Huella:** `maAtk60BuildTFootprintMm` en `ma-stl-atk60-formwork.js`.

---

## 4. Geometría T — P-1…P-8 (boceto)

### 4.1 Sistema local en el vértice

| Vector | Origen |
|--------|--------|
| **u** | Eje atravesado `p1→p2` (`throughAxisDir`) |
| **v** | Hacia cuerpo de la rama (`branchInto`) |
| **nuNear / nuFar** | Normal al atravesado; **near** = cara hacia la rama (`nearSign` igual que `maStlWall2dToolApplyTJunctionMm`) |

**Importante (fix 2026-07-10 tarde):** los puntos van en **caras** del atravesado (`±E/2` en `nu`), no en el eje (`v=0` sin offset).

### 4.2 Tabla de puntos (orden del bucle CCW)

Recorrido: **P-1 → P-8 → P-7 → P-6 → P-5 → P-4 → P-3 → P-2 → P-1**

| Etiqueta | Índice array | Superficie | Coordenadas locales (u, v) |
|----------|--------------|------------|----------------------------|
| **P-1** | 0 | cara **near** | `(−innerHalf, stemLen)` |
| **P-8** | 1 | cara **far** | `(−innerHalf, vShoulder)` |
| **P-7** | 2 | cara **far** | `(−throughHalf, vShoulder)` |
| **P-6** | 3 | cara **far** | `(−throughHalf, 0)` |
| **P-5** | 4 | cara **far** | `(+throughHalf, 0)` |
| **P-4** | 5 | cara **far** | `(+throughHalf, vShoulder)` |
| **P-3** | 6 | cara **far** | `(+innerHalf, vShoulder)` |
| **P-2** | 7 | cara **near** | `(+innerHalf, stemLen)` |

### 4.3 Cotas panel (caso fácil actual)

```text
throughHalf  = panel(2 × E_atr) / 2     // = recorte recto en vértice
stemLen      = panel(E_atr + 0,30)
innerHalf    = panel(2×0,30 + E_rama) / 2
vShoulder    = E_atr                    // ⚠️ provisional; ajustar con boceto
```

Ejemplo **E_atr = E_rama = 300 mm:**

| Cota | Panel |
|------|-------|
| 2×E | 900 → throughHalf = 450 |
| E+0,30 | 750 → stemLen |
| 2×0,30+E | 900 → innerHalf = 450 |

Si `innerHalf === throughHalf`, hombros P-8—P-7 y P-3—P-4 tienen longitud 0 → silueta rectangular (degeneración esperada).

### 4.4 Esquema ASCII (vista en planta, rama hacia arriba)

```text
        P-1 ============ P-2     ← cara near, v = stemLen
         |                |
        P-8              P-3     ← hombro, v = vShoulder (cara far)
         |                |
        P-7              P-4
         |                |
        P-6 ============= P-5     ← base, v = 0 (cara far)
```

---

## 5. Intentos fallidos — NO repetir sin revisar

| Intento | Por qué falló |
|---------|----------------|
| Polígono 8v solo en cara **near** (`anNear`) | Espesor visual incorrecto; pieza “fina” |
| Dos sólidos separados (barra + vástago) | Usuario: peor que boceto; tres piezas confusas |
| Recorte desplazando extremos de caras **oblicuas** | Rectos con tapa oblicua |
| Puntos solo en ejes **u,v** sin `nu` | Contorno pasa por **centro** del muro, no por caras (AutoCAD) |
| Extruir antes de validar contorno | Bloquea iteración con usuario |

---

## 6. Archivos tocados en esta sesión

| Archivo | Cambios |
|---------|---------|
| `Desing/Scripts/MasterArticles/ma-stl-atk60-formwork.js` | `maAtk60BuildTFootprintMm` (P-1…P-8 + caras), `maAtk60GetTJunctionTrimMm`, `maAtk60GetLJunctionTrimMm` |
| `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js` | Trim map, square cap, guía T, flags extrusión |
| `Desing/IA/Communication/WallConnections.json` | Caso prueba usuario (no commitear STL/bin) |
| `Desing/IA/docs/ENCOFRADO-AT60-IMPLEMENTACION.md` | Parcialmente desactualizado — usar **este** handover para T |

### 6.1 Funciones clave (grep)

```text
maStlDesing2BuildWallModelFromFaceStrips
maStlWall2dModelBuildStripQuadXzMm
maStlWall2dModelStripSquareCapAtAxisMm
maStlAtk60BuildAxisEndpointTrimMap
maAtk60BuildTFootprintMm
maStlAtk60AddTFootprintPointGuide
maStlAtk60TryRenderJunctionAtBucket
```

### 6.2 Detección T en bucket

`maStlAtk60TryRenderJunctionAtBucket`: 3 ejes → par colineal opuesto (atravesado) + rama; `throughAxisDir` = `maStlUserFloorPlanLineDirUnitXz(a)` (p1→p2, no `intoA`).

---

## 7. Cómo probar (checklist agente)

1. Abrir visor Desing_2 con dibujo del JSON o mismo layout (recinto + rama).
2. **Muro 3D** — regenerar.
3. **Rectos gris:** dos tramos horizontales + rama; extremos **planos** en N2; hueco central.
4. **Guía T:** contorno blanco + puntos P-1…P-8 coloreados; **sin** sólido amarillo extruido (`MA_STL_ATK60_T_EXTRUDE_3D === false`).
5. Comparar con boceto AutoCAD: puntos en **borde exterior** del hueco (cara far) y boca vástago (cara near).
6. Pulsar **Acotar muro** — cotas AT-60 en nodos (puede no coincidir hasta cerrar geometría T).

---

## 8. Pendiente para el siguiente agente

### 8.1 Prioridad 1 — Cerrar geometría P-1…P-8 con usuario

- [ ] Validar visualmente cada punto vs boceto AutoCAD (capturas en `Desing/IA/Examples/`).
- [ ] Ajustar `vShoulder` si hombro no coincide (hoy `= E_atr`).
- [ ] **P-1 / P-2:** ¿solo offset cara atravesado near o también cara rama (`E_rama` en normal de rama)?
- [ ] Confirmar signo de **v** (rama −Z en JSON vs boceto “rama arriba” en pantalla).

**Método acordado:** iterar **solo coordenadas** en `maAtk60BuildTFootprintMm`; no tocar lógica de rectos salvo bug explícito.

### 8.2 Prioridad 2 — Extrusión

- [ ] Cuando el usuario confirme contorno: `MA_STL_ATK60_T_EXTRUDE_3D = true`.
- [ ] Revisar `maAtk60BuildTFootprintDimPlacements` (índices pts cambiaron con nuevo orden P-1…P-8).

### 8.3 Prioridad 3 — Otros

- [ ] Unión **Cross** (+): `maAtk60BuildCrossFootprintMm` — sin implementar.
- [ ] Descomposición rectos en paneles P30…P90 + STL catálogo.
- [ ] Eje degenerado `length 0` en algunos JSON (artefacto dibujo).

---

## 9. Activar extrusión T (cuando proceda)

En `master-article-details-stl-viewer.js`:

```javascript
const MA_STL_ATK60_T_EXTRUDE_3D = true;
```

Flujo: `maAtk60BuildTFootprintMm` → `maStlWall3dAddMeshFromAtk60Footprint` (ExtrudeGeometry, H = `MA_STL_WALL3D_DEFAULT_HEIGHT_MM`).

---

## 10. Historial de mensajes usuario (resumen)

1. Reset tras fallo PC — retomar T en N2, L en N7/N8.
2. Dividir atravesado en dos rectos restando esquina; T invertida.
3. Rectos OK; T mal (3 sólidos, espesores pequeños) — no tocar rectos.
4. Caras oblicuas en conexión — fix square cap.
5. Solo dibujar línea P-1…P-8 + etiquetas; sin extruir.
6. Textos más pequeños; puntos de colores.
7. Puntos en **caras**, no centros — offset `nuNear`/`nuFar`.
8. **Este handover** — documentar para siguiente agente.

---

*Última actualización: 2026-07-11*
