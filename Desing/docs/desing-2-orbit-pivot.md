# Desing_2 — Pivote de órbita y cubo de vistas (guía definitiva)

**Código:** `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`  
**Vista:** `/Desing_2/Viewer` (`#ma-stl-viewer-shell` con `data-ma-stl-show-rulers-toggle="true"`)

> **Para agentes y revisores:** cualquier cambio en cubo de vistas, `OrbitControls`, cookie Desing_2 o `onCanvasPointerDownSetOrbitPivot` **debe leer esta guía** y ejecutar la prueba manual de regresión al final. Regla Cursor: `.cursor/rules/desing-2-view-cube-orbit.mdc`.

---

## Historia (no volver a romper)

Este comportamiento se ha **corregido varias veces en mayo 2026** (~4× pivote raycast + ~10× cubo 90°). Las causas son distintas pero el síntoma es similar: tras TOP/zoom/rotar la órbita “se pierde” o el primer arrastre no parte de un polo cardinal.

**Última corrección confirmada:** 2026-05-20 (usuario validó cubo 90° + órbita estable).

---

## Regla maestra — anclaje de reglas

En **Desing_2**, `OrbitControls.target` debe permanecer en el **anclaje de reglas** (`maStlRulerAnchorMm`, por defecto `(0, 0, 0)` en suelo Y=0). **No** raycastear el STL para mover el pivote al rotar.

El usuario coloca el anclaje con `#ma-stl-ruler-anchor-pick-toggle` (modo pick → acercar al **punto de inserción** del STL → recuadro activo → clic). Tras el pick, reglas, marca en suelo y órbita comparten ese punto.

En el **visor maestro de artículos** (shell **sin** `data-ma-stl-show-rulers-toggle="true"`), el raycast bajo el cursor sigue siendo el comportamiento CAD deseado.

### Por qué no raycast al rotar

Al alejar la cámara (zoom out), un raycast desde el cursor al mesh STL devuelve un punto lejos del anclaje. `controls.target` se desplaza, la órbita deja de girar alrededor del UCS/reglas y el usuario **pierde la orientación**.

### Detección Desing_2

| Función | Condición |
|---------|-----------|
| `maStlUsesFixedOrbitPivotAtOrigin()` | `#ma-stl-viewer-shell` tiene `data-ma-stl-show-rulers-toggle="true"` (`maStlDesingV2Viewer`) |

**No** usar `maStlRulersGate` para esta decisión: exige que exista el botón `#ma-stl-ucs-rulers-toggle` en el DOM. El atributo del shell es la fuente de verdad.

### `onCanvasPointerDownSetOrbitPivot` (Desing_2)

En pointerdown de rotación (capture, **antes** de OrbitControls):

1. Si `maStlUsesFixedOrbitPivotAtOrigin()` → `maStlResetOrbitTargetToRulerAnchor()` → **return** (sin raycast).
2. Else (maestro artículos) → raycast STL bajo cursor como CAD.

---

## View cube 90° — **DO NOT REGRESS**

Marcadores en código: bloques `/* VIEW CUBE 90° — DO NOT REGRESS: see desing-2-orbit-pivot.md */` sobre `applyDirectionToOrthoCam`, `maStlFinalizeViewCubePreset`, rama reuse de `bindControls`, y la puerta Desing_2 en `onCanvasPointerDownSetOrbitPivot`.

### Causa raíz

`applyDirectionToOrthoCam` (versión rota) ponía `camera.up` en `(0, 0, ±1)` para vistas TOP/BOTTOM. **OrbitControls** convierte `position − target` a coordenadas esféricas usando `camera.up` como eje de referencia. Con `up` en Z, una vista TOP real (`position` en +Y mirando al anclaje) se interpreta como órbita horizontal (`phi ≈ π/2`). El **primer arrastre** tras pulsar el cubo rota en azimuth/polar “libre” en lugar de partir de un polo cardinal ±90°.

Desing_2 usa `PerspectiveCamera` para la rejilla cuando `maStlRulersGate`; el cubo sigue la misma API (`cameraOrtho` / `cameraIso`).

### `camera.up` — incorrecto vs correcto

```javascript
// ❌ INCORRECTO — rompe OrbitControls tras TOP/BOTTOM
if (dir.y > 0.9) camera.up.set(0, 0, 1);   // TOP
else if (dir.y < -0.9) camera.up.set(0, 0, -1); // BOTTOM
else camera.up.set(0, 1, 0);

// ✅ CORRECTO — siempre Y-up de escena (Design-3d / Three.js)
camera.up.set(0, 1, 0);
camera.position.copy(anchor).add(dir.clone().normalize().multiplyScalar(d));
camera.lookAt(anchor);
// TOP = anchor + (0, dist, 0); no cambiar up por cara
```

**Nunca** asignar `camera.up` a `(0, 0, ±1)` en presets del cubo.

### Orden obligatorio al pulsar cubo (cara, arista o esquina)

| Paso | Función | Qué hace |
|------|---------|----------|
| 1 | `applyDirectionToOrthoCam(camera, dir)` | `position`, `up = (0,1,0)`, `lookAt(maStlRulerAnchorMm)`. **No** tocar `controls` ni `controls.target`. |
| 2 | `maStlResetOrbitTargetToRulerAnchor()` | *(implícito al inicio de paso 3)* — pivote = anclaje. |
| 3 | `bindControls(camera)` | Si `controls.object === camera`: solo reset target + `update()` (reutilizar instancia). Si cambia cámara (orto↔iso): `dispose` + `OrbitControls` nuevo **después** del paso 1. |
| 4 | `maStlFinalizeViewCubePreset()` | `maStlResetOrbitTargetToRulerAnchor()` → `controls.object.updateMatrixWorld(true)` → `controls.update()` → `controls.saveState()`. |

Cadena en handlers:

| UI | Handler | Cadena |
|----|---------|--------|
| Cara / arista / esquina orto | `applyOrthoDataView` / `applyOrthoFaceToView` | pasos 1 → 3 → 4 con `cameraOrtho` |
| Cara iso | `applyIsoFaceToView` | pasos 1 → 3 → 4 con `cameraIso` |

Caras (`front`, `top`, `right`, …) = ejes ±90°. Aristas/esquinas = direcciones diagonales normalizadas (45°), no sustituyen caras.

### Anti-patrones — cubo 90° (checklist)

- [ ] `bindControls` **antes** de `applyDirectionToOrthoCam` → esféricas desincronizadas.
- [ ] `maStlResetOrbitTargetToRulerAnchor` **dentro** de `applyDirectionToOrthoCam` → usar solo en pasos 3–4 (controls ya enlazado a la cámara correcta).
- [ ] `camera.up.set(0, 0, ±1)` para TOP/BOTTOM → rompe OrbitControls.
- [ ] Omitir `maStlFinalizeViewCubePreset` o `saveState()` tras cubo.
- [ ] Aplicar `state.target` de cookie tras cubo → pivote debe ser `rulerAnchor`.
- [ ] Raycast STL al rotar en Desing_2 → ver sección anclaje.

### Prueba manual — cubo 90° + órbita (5 pasos)

1. **Ctrl+F5** en `/Desing_2/Viewer`; cargar STL.
2. Clic **TOP** — planta estable (eje Y escena); repetir **FRONT** (+Z) y **RIGHT** (+X). Sin “salto” en el primer arrastre.
3. Alejar zoom al mínimo; arrastrar rotar — órbita estable alrededor del **anclaje de reglas**, sin saltos diagonales.
4. *(Opcional)* Mover anclaje con pick (`ri-crosshair-2-line`); repetir pasos 2–3 en el nuevo punto.
5. Guardar vista (`#ma-stl-save-viewer-state`) → recargar — cámara desde cookie; pivote sigue `rulerAnchor`, **no** `target` legado.

---

## Funciones que deben resetear el target

| Función | Momento |
|---------|---------|
| `onCanvasPointerDownSetOrbitPivot` | Pointerdown rotación: reset + early return Desing_2 — sin raycast |
| `bindControls` | Al reutilizar o crear `OrbitControls` (siempre **después** de mover cámara en presets del cubo) |
| `maStlFinalizeViewCubePreset` | Tras cada preset del cubo |
| `placeCamerasForModel` | Tras colocar cámaras en refit de modelo |
| `maStlSetRulerAnchorFromInsertionPoint` | Tras colocar anclaje en punto de inserción (pick) |
| `maStlApplyDesing2ViewerStateFromCookie` | Tras restaurar cámara/toggles — **no** copiar `state.target` |

`maStlResetOrbitTargetToRulerAnchor()` hace `controls.target.copy(maStlRulerAnchorMm)` en Desing_2; en maestro de artículos sigue `(0, 0, 0)`.

**Nota:** `applyDirectionToOrthoCam` **no** resetea target; el llamador encadena `bindControls` → `maStlFinalizeViewCubePreset`.

---

## Cookie — restaurar vista

Cookie: `ma_stl_desing2_viewer_state_global` (ver [`desing-2-stl-viewer-funciones.md`](desing-2-stl-viewer-funciones.md)).

| Campo | ¿Restaurar en Desing_2? | Notas |
|-------|-------------------------|-------|
| `rulerAnchor` | **Sí** | → `maStlRulerAnchorMm`, reglas, marca suelo |
| `target` | **No** | Se serializa por compatibilidad; **nunca aplicar** — pivote = `rulerAnchor` |
| `cameraOrtho` / `cameraIso` | **Sí** | Con `{ skipLookAt: true }` |
| `activeCamera`, `toggles`, clips | **Sí** | Según snapshot |

Flujo en `maStlApplyDesing2ViewerStateFromCookie`:

1. Restaurar toggles, clips, `rulerAnchor`, estados de cámara (sin `lookAt` forzado desde target).
2. **Ignorar** `state.target` (comentario en código ~L2497).
3. Cerrar siempre con `maStlResetOrbitTargetToRulerAnchor()` → `controls.update()` → `controls.saveState()`.

Tras restaurar cookie **o** preset del cubo, la órbita debe seguir el **anclaje actual**, no un `target` guardado ni `(0,0,0)` si el anclaje fue movido.

---

## Anti-patrones globales (provocan regresión)

- Volver a poner `orbitPivotRaycaster.intersectObject(currentRoot)` en Desing_2 al rotar.
- Cambiar `maStlUsesFixedOrbitPivotAtOrigin()` para usar `maStlRulersGate` en lugar de `maStlDesingV2Viewer`.
- Quitar el early return en `onCanvasPointerDownSetOrbitPivot` “para probar CAD” sin validar zoom out + rotate.
- En presets del cubo: `bindControls` **antes** de `applyDirectionToOrthoCam`.
- Reset de target **dentro** de `applyDirectionToOrthoCam`.
- `camera.up` en `(0,0,±1)` para TOP/BOTTOM en presets del cubo.
- Omitir `maStlFinalizeViewCubePreset` o `saveState()` tras cubo.
- Aplicar `state.target` de cookie en Desing_2.
- Forzar `controls.target.set(0,0,0)` en Desing_2 tras un anclaje distinto del origen.

**Do not remove the `maStlUsesFixedOrbitPivotAtOrigin` early return without testing zoom-out rotate.**

---

## Prueba manual — pivote fijo (anclaje)

1. Ctrl+F5 en `/Desing_2/Viewer`.
2. Activar pick de anclaje (`ri-crosshair-2-line`); clic en una esquina del panel: reglas y cruz cyan se mueven al punto (proyectado a Y=suelo).
3. Alejar zoom al mínimo permitido.
4. Arrastrar con botón izquierdo para rotar: el modelo debe orbitar estable alrededor del **anclaje**, no del origen de escena.
5. Guardar vista → recargar: anclaje y órbita restaurados desde cookie `rulerAnchor` (no `target`).

Para cubo 90°, usar la **Prueba manual — cubo 90° + órbita** arriba.
