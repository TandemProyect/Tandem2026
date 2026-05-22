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
| `maStlSqDistPointToSegment2dPx(px, py, ax, ay, bx, by)` | Cuadrado de distancia píxeles punto–segmento (pick pantalla líneas usuario en planta) |
| `maStlDesing2DimEditableMetersDisplayFromMm(lengthMm)` | Longitud física mm escena → cadena editable en **metros**, máximo **tres** decimales (`Intl`/fallback) |
| `maStlDesing2LengthMmRoundedEditableFromMm(lengthMm)` | Redondea mm escena desde metros físicos al máximo de decimales cotas editables (commit cota / distancia línea) |
| `maStlLineToolFloorDirLenFromDeltaMm(dxMm, dzMm, ortho15)` | Planta XZ: vector unitario y longitud desde delta; con `ortho15` acimut en pasos de 15° (0° = +X por `atan2(Z,X)`) |
| `maStlStlViewerIsLocalDevHost()` | `true` sólo localhost (parse fallido cotas ⇒ `console.error` opcional) |
| `maStlParseLengthInputValueToMm(text)` | Texto cotas línea usuario (`m` / `mm` opcional; coma o punto decimal; espacios y separadores de miles Intl) → longitud mm escena |
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
| `onCanvasPointerDownRulerAnchorPick(ev)` | Clic modo **rejilla** (`maStlRulerAnchorPickMode==='grid'`): snap activo en **P1/P2/mid** de línea usuario (`maStlFindFloorLineVertexSnapAtPointer`) o cruce rejilla connected; modo **objeto** (`object`): raycast STL → `maStlGetInsertionPointBottomLeftFootprintWorld(hitMesh)`. Sin salto de cámara (`maStlRulerAnchorMm` + reglas/overlays únicamente) |
| `onCanvasPointerMoveRulerAnchorPick(ev)` | `grid`: prioridad snap línea (esfera cian/verde) sobre snap rejilla HUD + cyan/connected; `object`: `maStlUpdateObjectInsertionPickHover` → hover STL + recuadro verde |
| `maStlRaycastClipStlMeshFirst(clientX, clientY)` | Puntero → primera intersección en `clipStlMeshes` (cada Mesh = objeto/parte) |
| `maStlUpdateObjectInsertionPickHover(clientX, clientY)` | Hover objeto: highlight STL + marca inserción en planta |
| `maStlSnapFloorToGridIntersectionMm(floorX, floorZ, minorMm)` | Snap X/Z a la rejilla menor (mm escena); `minorMm` opcional — default 500 mm |
| `maStlSnapFloorToGridFeatures(floorHit, proximity, gridSnapMm)` | Snap informativo: entre cruces de la rejilla menor, puntos medios de aristas y centros de celda; misma lógica `active` (proximidad + umbral en pantalla) que `maStlSnapFloorToGridIntersection` |
| `onCanvasPointerDownLineTool(ev)` / `onCanvasClickLineTool(ev)` / `onCanvasPointerMoveLineToolSync(ev)` | Herramienta línea Desing_2: **P1** + **P2** en planta (segundo punto por **clic** o **longitud + Intro** en `picking2`). Tras crear el segmento (`maStlLineToolResetForNextSegment`) vuelve a **`picking1`** para otro trazo sin reactivar el icono. Salida: **Escape**, clic en `#ma-stl-tool-line`, otra herramienta de barra, o **clic izquierdo en lienzo vacío** (sin snap/colocación) estando en `picking1`. Tras P1, con **orto 15°** activo (predeterminado): dirección hacia P2 en múltiplos de 15° en XZ (caucho, clic, distancia+Intro); **F8** o botón `#ma-stl-tool-ortho-15` alterna orto/off. Dirección libre igual que antes si orto está apagado. Longitud digitada/commit cota en metros: hasta **tres decimales** en visualización y al fijar (redondeo). Dirección al teclear: vector **P1→cursor** en suelo si el rayo es válido… |
| `onCanvasPointerMoveUserFloorLineHover(ev)` | Sin `picking*` ni modo pick rejilla/objeto: resalta línea usuario bajo cursor (proximidad pantalla ±`MA_STL_USER_FLOOR_LINE_SCREEN_PICK_PX` px sobre proyección 2D del segmento) |
| `onCanvasDblClickUserFloorLineDimension(ev)` | **Doble clic** en canvas: si el punto cae sobre una etiqueta HUD (ΔX/ΔZ/longitud por prioridad pantalla), abre **sólo esa** cota; si no (segmento naranja u halo unión), abre **las tres a la vez** (`editKind` **`all`**: inputs `#ma-stl-floor-dim-input`, `#ma-stl-floor-dim-input-dx`, `#ma-stl-floor-dim-input-dz` en las posiciones de cada readout). Tab entre campos; Enter en cualquiera o blur fuera del grupo confirman las tres (`maStlCommitUserFloorLineDimensionMulti`: ΔX → ΔZ → longitud). Los botones HUD usan **`dblclick`** (no clic simple). Formato **m**/ **mm**, **tres decimales** coherentes con `MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS` |
| `maStlCommitUserPlanLineSegmentMm(a, b)` | Crea `Line2` + `userData.maStlUserPlanLine`: `id`, `p1Mm`/`p2Mm` (punto inicial **fijo** al redimensionar); tras añadir llama **`maStlRefactorUserFloorLinesMergeCollinear`** |
| `maStlTryMergeUserFloorLineWithConnected(lnNew)` | Si el trazo comparte **un único** vértice (ε `maStlUserFloorLineMergeEndpointEpsMm`) con otro segmento, ambos son **colineales** en XZ (`|d1×d2| ≤ MA_STL_USER_FLOOR_LINE_MERGE_COLLINEAR_CROSS_MAX`) y **extensión de cadena** en la junta: `(p2,p1)`/`(p1,p2)` ⇒ `dot(d1,d2) > MA_STL_USER_FLOOR_LINE_MERGE_SAME_SENSE_DOT_MIN`; `(p1,p1)`/`(p2,p2)` ⇒ `dot < −umbral`. Fusiona en el segmento **ya existente** (extiende `p1` o `p2` hacia el vértice libre del otro), elimina el trazo sobrante y repite por **cadenas** colineales. Sentido opuesto o no colineal ⇒ dos segmentos |
| `maStlWeldAllUserFloorLineEndpointsMm()` / `maStlRefactorUserFloorLinesMergeCollinear()` | Refactor global: solda iterativamente todos los extremos XZ (≤ ε merge), luego repite `maStlTryMergeUserFloorLineWithConnected` sobre cada segmento hasta estabilizar. Disparadores: tras cada **`maStlCommitUserPlanLineSegmentMm`**, al salir del modo línea (**`maStlStopLineToolModesToolbar`**, p. ej. Escape), y **clic derecho corto** en canvas (`onCanvasPointerDownUserFloorLineRefactorRmb` + `pointerup` window: ≤ `MA_STL_USER_FLOOR_LINE_REFACTOR_RMB_CLICK_MAX_PX` px y sin pan de órbita > `MA_STL_DESING2_PICK_ORBIT_PAN_DETECTION_EPS_MM`; no en pick rejilla/objeto ni edición cotas). Durante herramienta línea el RMB sigue siendo pan si hubo arrastre |
| `maStlPickUserFloorLineNearScreenMm` / `maStlWorldMmToScreenPx` | Raycast overlays desactivado: pick por distancia pantalla segmento proyectado |
| `maStlResizeUserFloorLineToLengthMm` | `p2Mm = p1Mm + dirección_normalizada × longitud`; dirección desde `p1→p2` actual |
| `maStlRebuildUserFloorDimGuideGeometry` / `maStlEnsureUserFloorDimArrowMesh` / `maStlUpdateUserFloorLineDimHud` / `maStlSyncUserFloorDimHudScreenOnly` | **Tres cadenas** en planta (XZ): cota **longitud** paralela al segmento 3D (P1→P2); **ΔX** y **ΔZ** **desde `maStlRulerAnchorMm` (intersección reglas, suelo)** hasta **P1** (primer clic, punto inicial) en plano horizontal (extensiones en L cuando hace falta). `THREE.LineSegments` + **malla triangular** (`MeshBasicMaterial` `DoubleSide`) para **flechas CAD** en extremos (punta hacia el interior del tramo medido; tamaño vía `MA_STL_USER_FLOOR_LINE_DIM_ARROW_MESH_SCALE` ≈ 1/3). Overlay: `#ma-stl-floor-dim-readout`, `#ma-stl-floor-dim-readout-dx`, `#ma-stl-floor-dim-readout-dz` (`maStlPlaceAllFloorDimHudReadouts` / `maStlUserFloorDimProjectHudReadoutScreens`). Cambio de anclaje ⇒ `maStlInvalidateUserFloorDimGuideGeomCache` + resync HUD si hay hover. |
| `maStlResizeUserFloorLinePlanDeltaXMm` / `maStlResizeUserFloorLinePlanDeltaZMm` | **Traslación en planta** del segmento completo (P1 y P2 se desplazan igual en el eje editado): el valor es **delta firmado desde el anclaje de reglas (`maStlRulerAnchorMm`) hasta P1** en ese eje (`shift = (ref + Δ) − P1`; `P1/P2 += shift` en X o Z). Conserva longitud y dirección 3D; se rechaza el commit si la longitud residual quedaría `<` mínimo de segmento (`maStlUserFloorSegmentMinMm`). Redondeo vía **`maStlParseLengthInputValueToMm`** + **`maStlDesing2SignedDeltaMmRoundedEditableFromMm`** |
| `maStlDisposeUserFloorLineDimEdit` / `maStlBeginUserFloorLineDimensionEdit` | Modo **`length` / `deltaX` / `deltaZ`**: input `#ma-stl-floor-dim-input`. Modo **`all`**: tres inputs paralelos a los readouts. **`capt.dimKind`** en blur fotografía `{ line, inputEl, inputLenEl, inputDxEl, inputDzEl, detachBlur, dimKind }` (multi: sin `detachBlur` per-input; usa `maStlUserFloorLineDimEditDispose` global). Durante edición: asa DOM `#ma-stl-floor-line-drag-handle` (cuadrado azul 10×10 px en midpoint **geométrico** P1–P2) para arrastrar el segmento en XZ (`maStlTranslateUserFloorLineSegmentPlanMm` vía raycast suelo); órbita desactivada mientras se arrastra; oculta al cerrar edición (Escape / Enter / blur). |
| `maStlLineToolResetForNextSegment()` / `maStlStopLineToolModesToolbar()` / `maStlSyncLineToolHud()` | Estado `picking1`/`picking2`, HUD instrucción + coords (plantilla X/Z m); en `picking2` fila `#ma-stl-line-tool-hud-distance` + vista previa ≈ longitud; teclado global para dígitos/Intro salvo foco en otro input o edición de cotas. Tras P1 el foco pasa en microtarea al input distancia; **Intro en el botón línea (#ma-stl-tool-line) ya no ejecuta segundo “clic” que cerraba el modo sin segmento** (`keydown` capture + blur al activar). Tras cada segmento, `maStlLineToolResetForNextSegment` limpia caucho y distancia y permanece en `picking1`; `maStlStopLineToolModesToolbar` sale por completo y rehabilita órbita |
| `maStlLineToolComputeRubberBandEndMm` / `maStlLineToolUpdatePreviewDimHud` / `maStlLineToolSyncPreviewDimHudScreenOnly` / `maStlLineToolHidePreviewDimHud` | En **`picking2`**, cotas CAD en vivo (mismo `maStlRebuildUserFloorDimGuideGeometry` + overlay `#ma-stl-floor-dim-readout*`): **longitud** P1→cursor/caucho; **ΔX/ΔZ** ancla reglas→P1 (fijas mientras P1 no cambia). Se actualizan en cada `maStlLineToolRefreshPicking2RubberBand` (`pointermove` línea, orto F8, distancia tecleada). Clase overlay `desing2-stl-floor-dim-overlay--line-tool-preview` (sólo lectura). Se ocultan al confirmar segmento, Escape o salir de `picking2`. |
| `maStlFindLineToolVertexSnapCandidate` / `maStlSetLineToolVertexSnapHighlight` / `maStlResolveLineToolFloorPointMm` | Snap línea→línea en herramienta línea: candidatos P1/P2/medio de segmentos en `maStlUserLinesGroup`; esfera overlay cian→verde; prioridad sobre rejilla; clic/`resolve` devuelve `{ maStlLineVertexSnap: true }` y anula orto 15° en P2 |
| `maStlSyncLineToolOrtho15ToggleUi()` / `maStlToggleLineToolOrtho15FromUi()` / `maStlWireDesingV2F8OrthoKeyListener()` | Orto 15° en planta: botón `#ma-stl-tool-ortho-15` (estado `active` / `aria-pressed`) y tecla **F8** con visor activo; no dispara con foco en campos de texto/IME |
| `maStlDesing2CancelTransientToolsEscape()` | **Escape**: cancela edición cotas línea usuario si estaba abierta; modo línea (`picking*`), picks de anclaje rejilla/objeto, órbita bloqueada o desbloqueo diferido; limpia rubber-band, highlights rejilla/objeto STL/inserción, HUD cotas línea y HUD coords anclaje; quita clase `active` / `aria-pressed` en línea/rejilla/objeto; opcional toast vía `#ma-stl-viewer-shell` `data-ma-stl-escape-cancel-toast` (misma ruta Bootstrap que Guardar vista). Ignora Escape si el foco está en texto/`select`/contenido editable o dentro de `.modal.show` **salvo** el campo distancia `#ma-stl-line-tool-hud-distance` en `picking2` (ahí también cancela el modo línea) |
| `maStlFormatRulerAnchorGridIntersectionToast(tpl, xMm, zMm)` | Toast CAD con coords en metros (`{0}`, `{1}`) |
| `onCanvasPointerLeaveInsertionPick()` | Al salir del canvas: limpia highlights modo pick activo + hover/cota líneas usuario (`maStlClearUserFloorLineHover`) |
| `maStlClientRayToWorkspaceFloor(...)` | Rayo cursor → plano suelo Y=floor (NDC desde rect canvas; persp+orto) |
| `maStlGetInsertionPointBottomLeftFootprintWorld(group)` | **Punto de inserción (negocio / croquis CAD):** esquina **inferior izquierda** de la huella en planta (AABB mundo en X/Z), Y = suelo workspace: `(box.min.x, floorY, box.max.z)` según convención Desing_2 (−Z = arriba en planta). No usa centro X/Z ni metadata STL/DWG |
| `maStlInsertionPointProviders` | Catálogo extensible (`primary` = esa fórmula sobre un `THREE.Object3D`; modo objeto usa la Mesh clicada — p. ej. primario vs `*2.stl`) |
| `maStlCollectInsertionPointsWorld(group)` | Lista de puntos mundo por proveedor (útiles API / diagnóstico; no modo pick rejilla/objeto actual) |
| `maStlBuildInsertionPickHighlightRect(...)` | Recuadro verde sobre planta centrado en el punto de inserción (esquina huella inferior izquierda en modo objeto); rejilla modo grid usa otros meshes |
| `maStlClearStlPickHoverHighlight()` / `maStlApplyStlPickHoverHighlight(mesh)` | Hover azul temporal (modo objeto) |
| `maStlBuildRulerAnchorFloorMarker(...)` | Cruz cyan en suelo (anclaje; se oculta con `#ma-stl-ucs-rulers-toggle`) |
| `maStlBuildRulerAnchorIntersectBallMm(...)` | Esfera roja en el cruce de reglas; siempre visible mientras aplique modo reglas Desing_2 |
| `maStlApplyDesing2RulerLineMaterialTheme` / `maStlCreateDesing2RulerLineMaterial` | Líneas de reglas: gris ~70 % opacity con fondo claro; azul/cian anterior con `#ma-stl-dark-bg-toggle` activo |
| `maStlResetOrbitTargetToRulerAnchor()` | Sincroniza `controls.target` con anclaje |
| `maStlSetRulerAnchorFromInsertionPoint(pos)` | Aplica anclaje XY en planta (Y forzado a suelo) |
| `maStlRebuildRulerAnchorMarker()` | Reconstruye marca visual anclaje |
| `syncMaStlRulerPickToolbarUi()` | Estado botones rejilla/objeto (`#ma-stl-ruler-anchor-*-pick-toggle`), excluyentes |
| `maStlRebuildGridIntersectionPickHighlightMeshesForSnapChange()` | Limpia meshes de highlight cruce rejilla (cambio de snap / pestaña Entorno) |
| `applyDesing2EntornoLive()` | Aplica `#ma-stl-entorno-grid-snap-mm` y `#ma-stl-entorno-ruler-extent-m` a rejilla LOD, pick y cotas |
| `maStlExitRulerAnchorPickAfterPlacement()` | Tras clic válido canvas: teardown + diferir unlock órbita (ver orbit-pivot doc) |
| `maStlStopRulerAnchorPickModesToolbar()` | Toolbar off: cerrar modo pick sin defer |
| `maStlCancelAllViewerInteractionModes()` | Desing_2: Escape u otro uso — edición cotas línea (`maStlDisposeUserFloorLineDimEdit`), línea (`maStlStopLineToolModesToolbar`), pick rejilla/objeto (`maStlStopRulerAnchorPickModesToolbar`), órbita pick-lock rezagada (`maStlUnlockOrbitForRulerAnchorPick`), highlights/HUD/toolbar `#ma-stl-tool-line` / `#ma-stl-ruler-anchor-*` |
| `applySceneBackgroundAndClearColor()` | Fondo escena / color limpieza |
| `syncSkyToggleUi()` | Cielo gradiente |
| `syncDarkBgToggleUi()` | Fondo casi negro + color de trazos de reglas Desing_2 (cian/azul) |
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
| Rejilla | visible al arrancar; menor/mayor según Entorno (default paso menor 500 mm, mayor 5× antes de LOD) |
| Reglas UCS | visibles (`ma-stl-ucs-rulers-toggle` activo) |

Tras `loadStl`, `refitCamerasToObject` sustituye `lastMaxDim` y el frustum por el AABB del modelo.

## Anclaje de reglas

Punto clave del proyecto: el usuario elige el **punto de inserción** del objeto como origen de reglas (+X / −Z) y pivote de órbita.

| Elemento | Detalle |
|----------|---------|
| Estado | `maStlRulerAnchorMm` (`THREE.Vector3`), default `(0, 0, 0)` en suelo `MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM` |
| Modo rejilla — toolbar | `#ma-stl-ruler-anchor-pick-toggle` — `ri-crosshair-2-line`. Coloca reglas cuando el cruce al **incremental de rejilla** (Entorno, p. ej. 500 mm) está **connected** (verde) **o** cuando hay snap a **P1/P2/mid** de línea usuario (misma esfera cian/verde que herramienta línea). Clic fuera de snap muestra toast `StlPreview_RulerAnchorGridSnapRequiredToast` y **no** cambia anclaje |
| Modo objeto — toolbar | `#ma-stl-ruler-anchor-object-pick-toggle` — `ri-cube-line`. `pointermove`: raycast en `clipStlMeshes`; hover STL (azul) + recuadro verde en **punto de inserción** de esa malla — ver fila siguiente. Mutuamente excluyente con modo rejilla (`maStlRulerAnchorPickMode` `grid`/`object`/null). Toasts entran vía `_Desing2StlViewerWorkspace.cshtml` (`data-ma-stl-*`) |
| **Qué significa “punto de inserción”** | Origen tipo **croquis CAD** en la **esquina inferior izquierda de la huella** del objeto en planta: `(min.x, suelo Y, max.z)` del AABB mundo del mesh impactado (`maStlGetInsertionPointBottomLeftFootprintWorld`). Convención planta Desing_2: cotas −Z = «arriba» en papel ⇒ inferior = +Z; izquierda = menor X. No hay punto guardado en el STL; si existiera metadata de bloque DWG en el futuro podría añadirse otro proveedor en `maStlInsertionPointProviders`. Piezas separadas (`*2.stl`) siguen teniendo cada una su propio AABB |
| Modo rejilla — interacción | Crosshair + probe suelo → prioridad `maStlFindFloorLineVertexSnapAtPointer` (P1/P2/mid líneas usuario); si no, `maStlSnapFloorToGridIntersection`; HUD X/Z m; marcador cyan hasta “locked” green |
| Clic modo rejilla | Si snap línea o `snap.active` rejilla: `maStlSetRulerAnchorFromGridSnap` + toast `StlPreview_RulerAnchorGridIntersectionToast`; `maStlExitRulerAnchorPickAfterPlacement()` |
| Clic modo objeto | Si raycast STL: misma función de inserción + toast `StlPreview_RulerAnchorObjectInsertionToast`; sin impacto → `StlPreview_RulerAnchorObjectPickMissToast` |
| Snap rejilla | Paso configurable (**mm escena** = mm físicas con ×1000 de Desing_2): panel **Entorno** `#ma-stl-entorno-grid-snap-mm`; LOD de la rejilla multiplica este paso | 
| Marca | `maStlRulerAnchorMarkerGroup`: cruz cyan (toggle reglas) + esfera roja fija del cruce (no se oculta con `#ma-stl-ucs-rulers-toggle`) |
| Reglas | `maStlBuildPlanRulers(..., anchorXMm, anchorZMm)` — baselines desde anclaje; paso menor = incremental Entorno; paso mayor = **5×** ese valor; color trazos: gris translúcido en fondo claro, azul/cian si fondo negro |
| Órbita | `maStlResetOrbitTargetToRulerAnchor()` — `controls.target` = anclaje (sin raycast al rotar). Ver [`desing-2-orbit-pivot.md`](desing-2-orbit-pivot.md) |
| Salida pick | Segundo clic en el botón del modo activo, o clic canvas tras colocación exitosa |
| Cookie | `rulerAnchor: { x, y, z }` y `environment: { gridSnapMm, rulerExtentCapM }` en `ma_stl_desing2_viewer_state_global` (perspectiva al pulsar **Guardar vista**) |

La rejilla infinita sigue en el plano Y=0 global; solo reglas y pivote de órbita usan el anclaje.

### Herramienta línea (barra superior `#ma-stl-tool-line`)

| Elemento | Detalle |
|----------|---------|
| Flujo colocación | Clic icono → `picking1` → **primer clic** fija P1 → `picking2` + **caucho** (cursor o longitud tecleada con vista previa) + **cotas CAD en vivo** (longitud P1→cursor; ΔX/ΔZ ancla→P1) → **segundo clic** *o* **Intro** con distancia válida crea el segmento `Line2` en el suelo `Y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM` y **vuelve a `picking1`** para encadenar más trazos. Tras cada segmento, **`maStlRefactorUserFloorLinesMergeCollinear`** fusiona cadenas colineales mismo sentido (p. ej. snap en vértice existente ⇒ **un solo** trazo naranja). Salida del modo: **Escape** (refactor previo), clic otra vez en el icono, otra herramienta de barra, o **clic izquierdo en lienzo vacío** (sin punto de colocación) en `picking1` |
| Snap | Hover: mismas marcas cian/verde que el pick de rejilla (`maStlSnapFloorToGridFeatures`: cruces, medios de arista, centros de celda). **Prioridad** sobre rejilla: si el cursor está cerca de **P1**, **P2** o **punto medio** de otra línea usuario (`maStlFindLineToolVertexSnapCandidate` / `maStlFindFloorLineVertexSnapAtPointer`), esfera cian/verde (`maStlLineToolVertexSnapHighlightGroup`, radio **120 mm**, opacidad **50 %**) y clic fija el vértice exacto (sin orto 15°). Umbral: px pantalla = `maStlGridIntersectionPickScreenThresholdPx` + `MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST` (18 px), o proximidad XZ con paso Entorno × 0,52. En `picking2` se excluye el propio P1 del trazo en curso. Si no hay snap línea, impacto libre o snap rejilla como antes. Colocación en `click` (no `dblclick`); `pointerdown` en captura evita rotación Orbit |
| Hover / cotas tras colocación | Fuera de `picking*` y sin pick rejilla/objeto activo: hover sobre segmento ⇒ trazo más claro (`#ffaa33` vs `#ff6600`) más **tres cotas tipo CAD**: longitud paralela al segmento 3D sobre el suelo, **ΔX** y **ΔZ** **desde el anclaje de reglas (`maStlRulerAnchorMm`)** hasta **P1** (punto inicial del trazo) en el plano XZ (véase **`maStlRebuildUserFloorDimGuideGeometry`**). HUD: `#ma-stl-floor-dim-readout` (**longitud 3‑D**, `maStlDesing2DimEditableMetersDisplayFromMm`), **`#ma-stl-floor-dim-readout-dx` / `#ma-stl-floor-dim-readout-dz`** texto `ΔX`/`ΔZ` con **`maStlDesing2SignedDeltaMetersDisplayFromMm`** (metros, **tres decimales** máx.; reglas UCS siguen con `maStlRulerLabelMetersFromWorldM`). `pointermove` en host `#master-article-details-stl-viewer-canvas` mantiene lector y halo unión etiquetas (`maStlUserFloorDimLabelScreenHitIncludesPx`, BBoxes individuales `maStlUserFloorDimScrBox*`). **Nota roadmap:** mismo esquema de `userData.maStlUserPlanLine` permite extender la herramienta a otros objetos con cotas parametrizadas en capas siguientes |
| Edición cotas | **Sólo `dblclick`**: en cada botón readout (**longitud**, **ΔX**, **ΔZ**) abre **sólo esa** cota; **doble clic** en lienzo sobre el segmento o halo (`onCanvasDblClickUserFloorLineDimension`, captura **true**) abre **las tres** editables a la vez (Tab entre `#ma-stl-floor-dim-input`, `#ma-stl-floor-dim-input-dx`, `#ma-stl-floor-dim-input-dz`). Inputs compactos (`5.5ch` / `5ch`, max ~4.75 rem). **Asa azul** en midpoint del segmento (`#ma-stl-floor-line-drag-handle`): visible durante la sesión de edición; hover resalta; arrastre mueve P1+P2 en XZ (raycast plano suelo, órbita off). Hover resalta readout bajo cursor (`maStlUserFloorDimPickReadoutKindAtPx`); ya no elige cota CAD más cercana. `aria-label` vía `data-ma-stl-user-floor-line-dim-edit-*` y `data-ma-stl-user-floor-line-drag-handle-aria` en `#ma-stl-viewer-shell`. Longitud ⇒ `maStlResizeUserFloorLineToLengthMm`; ΔX/ΔZ ⇒ `maStlResizeUserFloorLinePlanDeltaXMm` / `…ZMm`. Enter/blur confirman; Escape cancela (**`dispose(true,true)`**) |
| HUD | `#ma-stl-line-tool-hud`: `StlPreview_LineToolInstructionFirst` / `StlPreview_LineToolInstructionSecond`, coordenadas (`StlPreview_RulerAnchorGridCoordsHud`) y en `picking2` campo `#ma-stl-line-tool-hud-distance` + prefijo vista previa `StlPreview_LineToolDistancePreviewApprox` vía `data-ma-stl-line-tool-distance-preview-prefix` |
| Persistencia escena | Segmentos hijos directos `maStlUserLinesGroup` con **material LineBasicMaterial clonado** por segmento (tema oscuro ajusta readout cotas/input y color de cotas paralelas según `#ma-stl-dark-bg-toggle`; tras `rebuildMaStlUcsOverlayDecor` cada línea recupera tono base naranja o highlight si seguía bajo hover) |
| Exclusión | Al activar pick rejilla/objeto de reglas u otra herramienta de la barra, se cancela el modo línea; al activar línea se llama `maStlStopRulerAnchorPickModesToolbar()`. Hover/cotas segmentos existentes están **suprimidas** mientras hay `picking*` o pick anclaje |
| Cámara | Bajo modo línea, `maStlLockOrbitForRulerAnchorPick()` deja pan (botón derecho) y zoom (rueda); rotación OFF. `onCanvasPointerDownSetOrbitPivot` ignora también el down si el modo línea está activo. `onCanvasPointerDownLineTool` corta la rotación en el down. Órbita permanece bloqueada mientras se encadenan segmentos; al salir (`maStlStopLineToolModesToolbar`) se rehabilita con `maStlUnlockOrbitForRulerAnchorPick`. **Clic derecho corto** sin pan (≤ 6 px, target sin mover > 0,5 mm) en canvas ejecuta refactor merge colineal manual (`maStlRefactorUserFloorLinesMergeCollinear`); arrastre RMB sigue siendo pan |

### Punto de inserción modo objeto — fórmula

1. Calcular `box = new THREE.Box3().setFromObject(mesh)` tras `mesh.updateMatrixWorld(true)` (AABB **alineado a ejes mundo**, no OBB local).
2. **Posición:** `(box.min.x, MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM, box.max.z)`.
3. **Justificación convención «inferior izquierda»:** en `maStlBuildPlanRulers` las marcas en dirección −Z se etiquetan como «arriba» en planta; por tanto la arista inferior del rectángulo de huella corresponde a **mayor Z**. La izquierda en planta (brazo +X hacia la derecha) es **menor X**. Es distinto de `(min.x, min.z)`, que sería la esquina superior-izquierda en esa misma convención.
4. **STL:** el formato triangular no incluye punto de inserción DWG; en el flujo maestro artículos / Desing_2 no hay otro campo persistido para este origen — todo deriva del envolvente. Un proveedor extra en `maStlInsertionPointProviders` podría leer metadata futura si existiera.

## Pivote de órbita y cubo 90° (Desing_2)

**Guía definitiva (obligatoria antes de editar órbita/cubo):** [`desing-2-orbit-pivot.md`](desing-2-orbit-pivot.md)

Resumen — no regredir:

- **Cubo 90°:** `applyDirectionToOrthoCam` mantiene `camera.up = (0,1,0)`; cadena `applyDirectionToOrthoCam` → `bindControls` → `maStlFinalizeViewCubePreset` (`update` + `saveState`).
- **Pivote fijo:** en Desing_2 (`maStlUsesFixedOrbitPivotAtOrigin`) `onCanvasPointerDownSetOrbitPivot` resetea al anclaje y **no** raycastea el STL al rotar.
- **Cookie:** restaurar `rulerAnchor` y cámaras; **ignorar** `state.target` — pivote = anclaje actual.

Funciones clave: `maStlUsesFixedOrbitPivotAtOrigin`, `maStlResetOrbitTargetToRulerAnchor`, `onCanvasPointerDownSetOrbitPivot`, `bindControls`, `applyDirectionToOrthoCam`, `maStlFinalizeViewCubePreset`.

## Pestaña Entorno (panel lateral)

| ID | Rol |
|--------|-----|
| `#ma-stl-entorno-grid-snap-mm` | Select: paso menor rejilla + snap pick (50–2000 mm). Major visual = **×5** para LOD base. |
| `#ma-stl-entorno-ruler-extent-m` | Select: tope de alcance físico de cotas (5–80 m); el algoritmo `maStlDesing2RulerExtentMm` puede quedarse por debajo según tamaño de pieza. |

Cambiar un select aplica al alza con `applyDesing2EntornoLive()`. La persistencia está en el JSON de la cookie **`ma_stl_desing2_viewer_state_global`**, clave interna `environment` (véase tabla siguiente), al pulsar `#ma-stl-save-viewer-state`.

### JSON `environment` (misma cookie que el resto del estado)

| Campo | Unidad | Rol |
|--------|--------|-----|
| `gridSnapMm` | mm escena (= mm físicas con convención Desing_2) | Paso menor de rejilla y de snap en modo cruce; línea mayor dibujada ×5 a este valor (LOD multiplica ambos). |
| `rulerExtentCapM` | metros físicos | Tope máximo de los brazos de regla; coincide con la opción elegida en el select Entorno. |

## Persistencia en cookie (solo Desing_2)

Guardado **explícito** con el botón de barra `#ma-stl-save-viewer-state` (icono `ri-save-3-line`, tooltip i18n «Guardar vista»). No hay auto-guardado al salir ni al mover la cámara.

| Clave cookie | `ma_stl_desing2_viewer_state_global` (misma vista para cualquier oferta/diseño) |
| Contenido | `activeCamera` (`ortho`/`iso`), `cameraOrtho` / `cameraIso` (`position`, `up`, `zoom` si orto), `target` (órbita), `rulerAnchor` (`{x,y,z}`), `environment` (`gridSnapMm`, `rulerExtentCapM` en **metros físicos**), `toggles` (rejilla, cielo, sombra, fondo oscuro, cortes UI, ejes XYZ, reglas UCS), sliders de corte X/Y |
| Restaurar | Cookie leída al arranque en `pendingDesing2Restore`. **Una vez** tras el **primer** `refitCamerasToObject` (STL cargado) o tras el arranque sin auto-carga STL. Orden en refit: frustum/rejilla → **omitir** `placeCamerasForModel` si hay restore pendiente o ya aplicado → `bindControls` → `maStlApplyDesing2ViewerStateFromCookie`. Refits posteriores (p. ej. `*2.stl`) **no** resetean cámara (`maStlDesing2StateRestored`). **`state.target` no se restaura** — pivote = `rulerAnchor`. |
| Lectura legado | Si no hay cookie global, se intenta la cookie antigua por `offerId`/`designId` o `ma_stl_desing2_viewer_state` |

Helpers módulo: `maStlDesing2ReadViewerStateFromCookie`, `maStlReadCookie`, `maStlWriteCookie`, `maStlDesing2BuildViewerStateSnapshot`, `maStlApplyDesing2ViewerStateFromCookie`, `maStlDesing2SaveViewerStateToCookie`, `maStlDesing2TryRestoreViewerStateFromCookie`. Arranque STL: evento `ma-stl-desing2-viewer-ready` (evita carrera módulo vs `#desing2-initial-stl-boot`). No aplica al visor de artículos maestro (sin `data-ma-stl-show-rulers-toggle`).
