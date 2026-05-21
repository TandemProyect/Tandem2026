# Desing_2 — Catálogo de funciones del visor STL (Three.js)

Origen del código: `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`  
Vista de ejemplo: `Desing/Views/Desing_2/Viewer.cshtml` → `/Desing_2/Viewer`

Esta lista resume **todas las funciones con nombre** del módulo para cuando se abra el diseño de la oferta en este visor.

**Órbita / cubo 90° (Desing_2):** ver [`desing-2-orbit-pivot.md`](desing-2-orbit-pivot.md) — (1) `camera.up` siempre `(0,1,0)` en cubo; (2) orden `applyDirectionToOrthoCam` → `bindControls` → `maStlFinalizeViewCubePreset`; (3) cookie ignora `state.target`, pivote = `rulerAnchor`.

## Nivel módulo (export implícito vía ejecución)

| Función | Rol breve |
|--------|-----------|
| `createMasterArticleStlSkyBackgroundTexture()` | Textura degradado cielo → `scene.background` |
| `masterArticleStlWorldAxesLength(maxDim)` | Longitud ejes mundo según tamaño modelo |
| `applyMasterArticleStlAxesStyle(axesRoot)` | Opacidad / raycast off en ejes |
| `disposeObject3D(obj)` | Libera geometrías/materiales y quita de escena |
| `masterArticleStlTintColorFromDataHex(hexRaw)` | `THREE.Color` desde `data-ma-text-color1/2` |
| `masterArticleStlSecondaryUrlFromPrimary(primaryUrl)` | URL convención `nombre2.stl` |
| `bootMasterArticleDetailsStlViewer()` | Arranque completo: escena, luces, controles, UI, `loadStl` |

## Internas de `bootMasterArticleDetailsStlViewer`

| Función | Rol breve |
|--------|-----------|
| `syncClipToggleUi()` | Muestra/oculta sliders de recorte + clase en canvas |
| `syncCameraRadios()` | Sincroniza radios orto / iso |
| `viewDistanceFromModel()` | Distancia cámara según `lastMaxDim` |
| `syncViewCubesVisibility()` | Cubo navegación orto vs isométrico |
| `applyDirectionToOrthoCam(camera, dir)` | Orienta cámara ortográfica según vector |
| `applyOrthoDirection(dir)` | Aplica dirección predefinida orto |
| `applyOrthoDataView(viewKey)` | Vista desde clave `data-ortho-view` |
| `applyOrthoFaceToView(face)` | Click cara cubo orto |
| `applyIsoFaceToView(face)` | Click cara cubo iso |
| `syncGridToggleUi()` | Rejilla infinita on/off |
| `makeOrthoCamera()` | Construye `OrthographicCamera` |
| `vcEpsilon(value)` | Épsilon comparaciones cubo CSS |
| `getCameraCssMatrix3d(matrix)` | Matriz CSS para cubo vista |
| `setViewCubeCssFromCamera(cubeEl, camera)` | Rota cubo acorde a cámara |
| `activeCamera()` | Cámara según modo orto/iso |
| `applyFrustumToCamera(cam)` | Frustum ortográfico |
| `applyFrustumToBoth()` | Perspectiva + orto |
| `placeCamerasForModel(maxDim)` | Posición inicial cámaras |
| `bindControls(camera)` | `OrbitControls` + pivotes |
| `stlOrbitPointerDownWillRotate(ev)` | Heurística órbita vs cubo |
| `onCanvasPointerDownSetOrbitPivot(ev)` | Punto de pivote órbita |
| `onCanvasPointerDownRulerAnchorPick(ev)` | Clic: intersección rejilla → inserción → planta libre (prioridad en ese orden) |
| `onCanvasPointerMoveInsertionPick(ev)` | Snap intersección rejilla (recuadro cyan) + inserción (verde) + hover azul STL |
| `maStlSnapFloorToGridIntersectionMm(floorX, floorZ)` | Snap X/Z a rejilla minor 500 mm |
| `maStlProbeGridIntersectionPickNear(...)` | Umbral mm + px para activar snap en cruce de líneas |
| `maStlFormatRulerAnchorGridIntersectionToast(tpl, xMm, zMm)` | Toast CAD con coords en metros (`{0}`, `{1}`) |
| `onCanvasPointerLeaveInsertionPick()` | Limpia hover azul y recuadro al salir del canvas en modo pick |
| `maStlClientRayToWorkspaceFloor(...)` | Rayo cursor → plano suelo Y=floor (NDC desde rect canvas; persp+orto) |
| `maStlGetInsertionPointBottomCenterWorld(group)` | Bottom-center AABB en suelo (fórmula inserción `primary`) |
| `maStlInsertionPointProviders` | Catálogo extensible (`primary` = bottom-center, no centro 3D bbox) |
| `maStlCollectInsertionPointsWorld(group)` | Posiciones mundo de inserción tras apoyar STL |
| `maStlInsertionPickProximityThresholdMm(...)` | Umbral mm según modelo y cámara |
| `maStlBuildInsertionPickHighlightRect(...)` | Recuadro verde/cyan en suelo (modo pick) |
| `maStlClearStlPickHoverHighlight()` / `maStlApplyStlPickHoverHighlight(mesh)` | Hover azul temporal en malla STL (solo modo pick) |
| `maStlBuildRulerAnchorFloorMarker(...)` | Cruz cyan en suelo (anclaje) |
| `maStlResetOrbitTargetToRulerAnchor()` | Sincroniza `controls.target` con anclaje |
| `maStlSetRulerAnchorFromInsertionPoint(pos)` | Aplica anclaje en punto de inserción |
| `maStlRebuildRulerAnchorMarker()` | Reconstruye marca visual anclaje |
| `syncMaStlRulerAnchorPickBtnUi()` | Estado botón pick anclaje |
| `maStlExitRulerAnchorPickMode()` | Sale del modo pick |
| `applySceneBackgroundAndClearColor()` | Fondo escena / color limpieza |
| `syncSkyToggleUi()` | Cielo gradiente |
| `syncDarkBgToggleUi()` | Fondo casi negro |
| `syncGroundShadowToggleUi()` | Sombra en suelo + sombras en mallas |
| `setStatus(text)` | Pie de estado texto |
| `tick()` | Bucle `requestAnimationFrame` |
| `resizeRendererToHost()` | Tamaño renderer vs host |
| `getFullscreenElement()` | Elemento fullscreen actual |
| `requestFullscreenFor(el)` | Entrar fullscreen |
| `exitFullscreenDoc()` | Salir fullscreen |
| `syncFullscreenToggleUi()` | Botón fullscreen |
| `setCameraMode(mode)` | Cambio orto ↔ iso |
| `clipFractionFromSlider(inputEl)` | Fracción recorte 0–1 desde slider |
| `updateClipPlanes()` | Planos recorte Three.js |
| `masterArticleStlFitMaxDimFromWorldBox(box)` | Dimensión característica |
| `refitCamerasToObject(group)` | Encuadra modelo |
| `makeStlMeshStandardMaterial(tintColor)` | Material estándar + clipping |
| `tryLoadSecondaryStl(primaryUrl, group, myToken, loader)` | Carga `*2.stl` opcional |
| `loadStl(url, label)` | Carga STL principal + secundario |

## Callbacks anónimos relevantes

- Loader STL: `function (geometry) { … }` y `function () { … }` en error dentro de `loadStl`.

## Carga desde oferta

1. Resolver ruta virtual segura del STL en servidor (`Desing_2Controller.ResolveSafeApplicationStlUrl`).
2. Renderizar botón oculto `#desing2-initial-stl-boot` con clase `master-article-stl-load` y `data-stl-url` / `data-slot-label`.
3. Tras el `type="module"` del viewer, un script clásico hace `.click()` para disparar `loadStl`.

Más contexto de IDs DOM y diagrama: `Scripts/ThreejsDesing/README.md`.

## Baseline cámara / rejilla (Desing_2, sin STL)

| Parámetro | Valor |
|-----------|--------|
| `MA_STL_DESING2_EMPTY_BASELINE_MM` | `12000` (12 m físicos en mm escena) |
| `frustumHalfY` inicial | `camFitDim × 0.55`, `camFitDim = max(12 000 × 1.22, 4000)` |
| Cámara orto | posición `(0, 0, 3 × lastMaxDim)`, `lookAt(0,0,0)` |
| Rejilla | visible al arrancar; celdas 250 / 1000 mm |
| Reglas UCS | visibles (`ma-stl-ucs-rulers-toggle` activo) |

Tras `loadStl`, `refitCamerasToObject` sustituye `lastMaxDim` y el frustum por el AABB del modelo.

## Anclaje de reglas

Punto clave del proyecto: el usuario elige el **punto de inserción** del objeto como origen de reglas (+X / −Z) y pivote de órbita.

| Elemento | Detalle |
|----------|---------|
| Estado | `maStlRulerAnchorMm` (`THREE.Vector3`), default `(0, 0, 0)` en suelo `MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM` |
| Toolbar | `#ma-stl-ruler-anchor-pick-toggle` — icono `ri-crosshair-2-line`, toggle modo pick |
| Punto de inserción | Tras apoyar STL: `primary` = **bottom-center** AABB mundo: `x=(min.x+max.x)/2`, `y=floorY`, `z=(min.z+max.z)/2` (≠ centro 3D del bbox). Ver `maStlGetInsertionPointBottomCenterWorld`. Extensible vía `maStlInsertionPointProviders[]` |
| Modo pick | Cursor crosshair; `pointermove`: rayo a suelo → snap a intersección rejilla **500 mm** → recuadro **cyan** si cercano; si no, hover **azul** STL + proximidad al **punto de inserción** → recuadro **verde** |
| Clic | 1) Intersección activa → anclaje en snap + toast `StlPreview_RulerAnchorGridIntersectionToast` (`Reglas en (X, Z) m`); 2) recuadro verde → inserción; 3) planta libre X/Z del cursor (`StlPreview_RulerAnchorPickFloorToast`); sale del modo pick |
| Rejilla snap | `MA_STL_DESING2_GRID_MINOR_MM` (500); umbral XZ mm + px pantalla (misma familia que pick inserción); base fija 500 mm aunque LOD aleje celdas |
| Marca | Cruz cyan en suelo (`maStlRulerAnchorMarkerGroup`) |
| Reglas | `maStlBuildPlanRulers(..., anchorXMm, anchorZMm)` — baselines desde anclaje; marcas 500 mm / 2500 mm |
| Órbita | `maStlResetOrbitTargetToRulerAnchor()` — `controls.target` = anclaje (sin raycast al rotar). Ver [`desing-2-orbit-pivot.md`](desing-2-orbit-pivot.md) |
| Salida pick | Tras clic con recuadro activo, o segundo clic en el botón toolbar |
| Cookie | `rulerAnchor: { x, y, z }` en `ma_stl_desing2_viewer_state_global` (sin cambios) |

La rejilla infinita sigue en el plano Y=0 global; solo reglas y pivote de órbita usan el anclaje.

## Pivote de órbita y cubo 90° (Desing_2)

**Guía definitiva (obligatoria antes de editar órbita/cubo):** [`desing-2-orbit-pivot.md`](desing-2-orbit-pivot.md)

Resumen — no regredir:

- **Cubo 90°:** `applyDirectionToOrthoCam` mantiene `camera.up = (0,1,0)`; cadena `applyDirectionToOrthoCam` → `bindControls` → `maStlFinalizeViewCubePreset` (`update` + `saveState`).
- **Pivote fijo:** en Desing_2 (`maStlUsesFixedOrbitPivotAtOrigin`) `onCanvasPointerDownSetOrbitPivot` resetea al anclaje y **no** raycastea el STL al rotar.
- **Cookie:** restaurar `rulerAnchor` y cámaras; **ignorar** `state.target` — pivote = anclaje actual.

Funciones clave: `maStlUsesFixedOrbitPivotAtOrigin`, `maStlResetOrbitTargetToRulerAnchor`, `onCanvasPointerDownSetOrbitPivot`, `bindControls`, `applyDirectionToOrthoCam`, `maStlFinalizeViewCubePreset`.

## Persistencia en cookie (solo Desing_2)

Guardado **explícito** con el botón de barra `#ma-stl-save-viewer-state` (icono `ri-save-3-line`, tooltip i18n «Guardar vista»). No hay auto-guardado al salir ni al mover la cámara.

| Clave cookie | `ma_stl_desing2_viewer_state_global` (misma vista para cualquier oferta/diseño) |
| Contenido | `activeCamera` (`ortho`/`iso`), `cameraOrtho` / `cameraIso` (`position`, `up`, `zoom` si orto), `target` (órbita), `rulerAnchor` (`{x,y,z}`), `toggles` (rejilla, cielo, sombra, fondo oscuro, cortes UI, ejes XYZ, reglas UCS), sliders de corte X/Y |
| Restaurar | Cookie leída al arranque en `pendingDesing2Restore`. **Una vez** tras el **primer** `refitCamerasToObject` (STL cargado) o tras el arranque sin auto-carga STL. Orden en refit: frustum/rejilla → **omitir** `placeCamerasForModel` si hay restore pendiente o ya aplicado → `bindControls` → `maStlApplyDesing2ViewerStateFromCookie`. Refits posteriores (p. ej. `*2.stl`) **no** resetean cámara (`maStlDesing2StateRestored`). **`state.target` no se restaura** — pivote = `rulerAnchor`. |
| Lectura legado | Si no hay cookie global, se intenta la cookie antigua por `offerId`/`designId` o `ma_stl_desing2_viewer_state` |

Helpers módulo: `maStlDesing2ReadViewerStateFromCookie`, `maStlReadCookie`, `maStlWriteCookie`, `maStlDesing2BuildViewerStateSnapshot`, `maStlApplyDesing2ViewerStateFromCookie`, `maStlDesing2SaveViewerStateToCookie`, `maStlDesing2TryRestoreViewerStateFromCookie`. Arranque STL: evento `ma-stl-desing2-viewer-ready` (evita carrera módulo vs `#desing2-initial-stl-boot`). No aplica al visor de artículos maestro (sin `data-ma-stl-show-rulers-toggle`).
