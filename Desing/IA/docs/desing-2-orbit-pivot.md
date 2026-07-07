# Desing_2 — Pivote de órbita y cubo de vistas (guía definitiva)

**Código:** `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`  
**Vista:** `/Desing_2/Viewer` (`#ma-stl-viewer-shell` con `data-ma-stl-show-rulers-toggle="true"`)

> **Para agentes y revisores:** cualquier cambio en cubo de vistas, `OrbitControls`, cookie Desing_2 o `onCanvasPointerDownSetOrbitPivot` **debe leer esta guía** y ejecutar la prueba manual de regresión al final. Regla Cursor: `.cursor/rules/desing-2-view-cube-orbit.mdc`.

---

## Historia (no volver a romper)

Este comportamiento se ha **corregido varias veces en mayo 2026** (~4× pivote raycast + ~10× cubo 90°). Las causas son distintas pero el síntoma es similar: tras TOP/zoom/rotar la órbita “se pierde” o el primer arrastre no parte de un polo cardinal.

**Última corrección confirmada:** 2026-05-22 (herramienta línea — al terminar segmento **no** `preserveView` hacia ruler; evita mismo salto que pan→orbit target desfasado).

---

## Regla maestra — anclaje de reglas

En **Desing_2**, tras **colocación de anclaje** (rejilla/objeto), el primer arrastre de **rotación** debe alinear `OrbitControls.target` con **`maStlRulerAnchorMm`** mediante `maStlApplyRulerAnchorOrbitPivotPreserveView` para no «teletransportar» el encuadre. **No obstante**, si el usuario **panea libremente** (órbita con botón PAN / equivalente fuera del pick-lock), el pivote de órbita **permanece donde quedó** el último paneo hasta que cambie el anclaje con pick, preset del cubo, `bindControls`, refit, o cookie restore: **no** forzar ese snap en cada `pointerdown` de rotate (causaba saltos «como si el pan nunca hubiera ocurrido»). **No** raycastear el STL para mover el pivote al rotar.

El usuario coloca el anclaje con `#ma-stl-ruler-anchor-pick-toggle` (**rejilla 500 mm**) o `#ma-stl-ruler-anchor-object-pick-toggle` (**punto de inserción** de la pieza STL clicada — esquina inferior izquierda de la huella en planta). Tras el pick, reglas, marca en suelo y órbita comparten ese punto.

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

1. Si `maStlUsesFixedOrbitPivotAtOrigin()`:
   - si **`maStlDesing2OrbitDeferRulerPivotPreserveOnNextSync`** (paneo durante pick-lock): **omitir** `preserveView` — **`update`** + **`saveState`** + limpiar también el flag opcional **`maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown`** — **return** sin raycast. Persiste hasta **`maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock`** (u otros resets).
   - **cuando **`maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown`** (tras colocación reciente del anclaje en Desing_2, antes del primer LMB-rotate) → **`maStlApplyRulerAnchorOrbitPivotPreserveView()`**: alinea `controls.target` con `maStlRulerAnchorMm` compensando la cámara; el flag **se consume ahí**.
   - en caso contrario (p. ej. el usuario paneó antes con órbita normal) → sólo **`controls.update()`** + **`saveState`** — sin reaplicar anclaje; **return** sin raycast.
2. Else (maestro artículos) → raycast STL bajo cursor como CAD.

### Pick de anclaje de reglas — orden de rehabilitación `OrbitControls`

Los listeners de modo pick van en **capture** sobre el canvas (`onCanvasPointerDownRulerAnchorPick`). `OrbitControls` registra `pointerdown` en **bubble** (orden por defecto). Tras colocar el anclaje, `maStlExitRulerAnchorPickAfterPlacement` **no debe** rehabilitar Orbit hasta **después del `pointerup`/`pointercancel`** del mismo gesto más **unos frames** (`requestAnimationFrame` doble): un `queueMicrotask` llega **demasiado pronto** (antes de cerrar el gesto pointer) y Orbit puede quedar fuera de fase con la cámara. En Desing_2 el clic de colocación **no mueve `camera` ni `controls.target`**: sólo actualiza `maStlRulerAnchorMm` y geometría de reglas.

**Línea (dos `click` simples tras `pointerdown` capturado)** y **pick rejilla/objeto**, con **`enableRotate=false`** pero **pan/zoom activos**, pueden dejar `controls.target` **desfasado** respecto a `maStlRulerAnchorMm`:

- **Herramienta línea** no modifica **`maStlRulerAnchorMm`**. Tras colocar P2 (`maStlStopLineToolModesToolbar`), **`maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock(true)`**: **no** ejecuta **`preserveView`** hacia el ruler — solo **`controls.update()`** + **`saveState()`** tras unlock; igual filosofía que no forzar pivote cuando el **`target`** quedó a propósito desalineado por paneo (**incluye** paneo **antes** de activar línea). Rehabilitación diferida: `queueMicrotask` + **2 RAF** (el **`click`** de P2 se entrega después de **`pointerup`**).

- **Pick de anclaje (rejilla/objeto)** tras colocar: suele hacer falta **`maStlApplyRulerAnchorOrbitPivotPreserveView()`** tras **`maStlUnlockOrbitForRulerAnchorPickInner`**, antes de **`saveState()`** (`maStlSync*` sin skip; rutas `pointerup` + 2 RAF).

- **El usuario panea** durante el lock sin mover el anclaje como dato (`maStlRulerAnchorMm` igual al inicio del lock): forzar **`preserveView`** en unlock sería un salto. **Mitigación:** baseline de **`controls.target`** al entrar en lock + listener **`change`**; si **`target`** se mueve más de **`MA_STL_DESING2_PICK_ORBIT_PAN_DETECTION_EPS_MM`** ⇒ **`maStlDesing2OrbitDeferRulerPivotPreserveOnNextSync`**. En **`maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock`** (rama pick de anclaje): omitir **`preserveView`** si defer **y** anclaje **no** cambió; si **sí** cambió (colocación pick) → **`preserveView`** + limpiar defer. **`onCanvasPointerDownSetOrbitPivot`** omite **`preserveView`** mientras defer siga activo. **Solo zoom** no desplaza `target`; no marca defer.

La primera rotación sin defer puede ejecutar **`preserveView`** sólo cuando el flag de «anclaje recién colocado» lo pide (`maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown`).

### Causa raíz (2026-05-21 — pan → rotate saltaba atrás)

`onCanvasPointerDownSetOrbitPivot` ejecutaba **`maStlApplyRulerAnchorOrbitPivotPreserveView()` en todo LMB-rotate** cuando no había defer de pick-lock. Eso **reasignaba** `controls.target` al anclaje de reglas antes de cada nuevo gesto de rotación. Para **zoom out + orbitar desde el anclaje** ese snap es estable; tras un **paneo libre**, el modelo correcto es orbitar sobre el **`target` actual** hasta cambiar anclaje o preset. Forzar repetidamente el realineado al anclaje (y la compensación interna que implica «vista preservada») producía una **fuerte sensación de salto**: el encuadre volvía a parecer anterior al último paneo (**mitigación:** gate con `maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown` marcado sólo tras `maStlSetRulerAnchor*` en Desing_2).

### Causa raíz (2026-05-22 — segundo clic línea saltaba la cámara)

Al completar el segmento, **`maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock`** (sin modo línea) ejecutaba **`preserveView`** salvo la rama «defer + anclaje sin cambiar». Eso **re-alineaba** `controls.target` con **`maStlRulerAnchorMm`** tras **cualquier** sesión pick-lock incluida la **línea**, aunque la línea **no** actualice el anclaje. Si el usuario había paneado antes o durante la colocación (target intencionalmente lejos del ruler en datos), el encuadre **saltaba**. **Mitigación:** rama explícita **`skipRulerAnchorPreserveViewOnUnlock`** al salir solo de la herramienta línea (y `queueMicrotask` + 2 RAF para el unlock diferido tras `click`).

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
| `maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock` | Tras `unlock inner`: con **`skipRulerAnchorPreserveViewOnUnlock`** (solo salida herramienta **línea**) → limpiar defer + `controls.update()`; sin skip: si defer por pan **y** anclaje sin cambiar → noop; si anchor cambió (pick rejilla/objeto) → `preserveView` + limpiar defer; si sin defer → `preserveView`. Antes de `saveState` en rutas diferidas. |
| `maStlWireDesing2OrbitPickLockListener` | Tras **cada** `bindControls`; detecta paneo durante pick-lock (**`controls.change`** + baseline). |
| `bindControls` | Al reutilizar o crear `OrbitControls` (siempre **después** de mover cámara en presets del cubo) |
| `maStlFinalizeViewCubePreset` | Tras cada preset del cubo |
| `placeCamerasForModel` | Tras colocar cámaras en refit de modelo |
| `maStlSetRulerAnchorFromGridSnap` / `maStlSetRulerAnchorFromInsertionPoint` | Colocar datos de anclaje (pick Desing_2: **no** toca `controls`/`camera`); en Desing_2 marca `maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown`; `maStlSync*` usa cambio vs `_maStlPickLockRulerAnchorStartMm` para decidir preserve tras pan. |
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
- Rehabilitar `OrbitControls` con sólo `queueMicrotask` al salir del pick **colocado en pointerdown** (`pointerup`/Orbit pueden quedar desfasados): usar fin de gesto (`pointerup`/`pointercancel` en capture) + doble RAF (`maStlScheduleDeferRulerPickOrbitUnlockAfterPointerEnd`). **Línea** (colocación en `click`): `queueMicrotask` + doble RAF en `maStlSchedulePickOrbitUnlockAfterPlacement(false, true)` — no reutilizar el listener `pointerup` global (ya ocurrió antes del `click`).
- Tras herramienta **línea**, llamar **`maStlApplyRulerAnchorOrbitPivotPreserveView`** en unlock «para encajar STL» → regresión (salto si `target` ≠ ruler tras pan).
- Ejecutar `maStlApplyRulerAnchorOrbitPivotPreserveView()` en el **mismo** `pointerdown` que **coloca** el anclaje en Desing_2 (preferir mover sólo `maStlRulerAnchorMm` + geometría; el siguiente `pointerdown` de rotación aplica `preserveView` desde `onCanvasPointerDownSetOrbitPivot`).
- Llamar `controls.saveState()` al cerrar modo pick/regla/línea **antes** de ejecutar **`maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock`** (tras `unlock inner`, antes de `saveState`; salvo regress intencional de orden).
- Quitar **`maStlWireDesing2OrbitPickLockListener()`** tras crear `OrbitControls` → regresión: salto al rotar después de paneo en herramienta línea/regla.

**Do not remove the `maStlUsesFixedOrbitPivotAtOrigin` early return without testing zoom-out rotate.**

---

## Prueba manual — pivote fijo (anclaje)

1. Ctrl+F5 en `/Desing_2/Viewer`.
2. Activar pick de anclaje (`ri-crosshair-2-line`); clic en una esquina del panel: reglas, cruz cyan y esfera roja del cruce siguen el anclaje (proyectado a Y=suelo). Con `#ma-stl-ucs-rulers-toggle` se ocultan trazos y etiquetas y la cruz cyan, **no** la esfera roja.
3. Alejar zoom al mínimo permitido.
4. Arrastrar con botón izquierdo para rotar: el modelo debe orbitar estable alrededor del **punto focal actual** cuando procede desde el último navegador (según último paneo/target), manteniendo anclaje de reglas en el suelo sólo como referencia de datos.
5. **Pan libre:** panel derecho para paneo (o combinación habitual de OrbitControls); mover la vista notablemente → **primer arrastre de rotación (LMB)** sin salto que deshaga el último paneo.
6. Guardar vista → recargar: anclaje y órbita restaurados desde cookie `rulerAnchor` (no `target`).

Para cubo 90°, usar la **Prueba manual — cubo 90° + órbita** arriba.

### Prueba manual — línea + paneo + rotar (anti-regresión 2026-05-21)

1. Activar herramienta **línea**; hacer **primer clic** (pasa a `picking2` / línea de caucho).
2. Paneer la vista (**no** debe haber sido paneo sólo-zoom con `target` inmóvil en casos donde el problema se reprodujo con pan real).
3. **Segundo clic** para terminar segmento — u operación equivalente que desbloquee órbita.
4. **Primer clic** de rotar (LMB drag): sin salto perceptible respecto del encuadre previo.

Reglas y líneas siguen en **espacio mundo** sobre `maStlRulerAnchorMm`; el pivote de órbita puede estar desalineado con el anclaje hasta que el usuario mueva el anclaje o use **cubo**/`bindControls`.
