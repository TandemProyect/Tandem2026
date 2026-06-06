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
| `onCanvasPointerDownRulerAnchorPick(ev)` | Clic modo **rejilla** (`maStlRulerAnchorPickMode==='grid'`): snap activo en **P1/P2/mid/cuerpo** de línea usuario (`maStlFindFloorLineVertexSnapAtPointer` → `maStlFindNearestUserFloorLineSegmentSnapMm`) o cruce rejilla connected; modo **objeto** (`object`): raycast STL → `maStlGetInsertionPointBottomLeftFootprintWorld(hitMesh)`. Sin salto de cámara (`maStlRulerAnchorMm` + reglas/overlays únicamente) |
| `onCanvasPointerMoveRulerAnchorPick(ev)` | `grid`: prioridad snap línea (esfera cian/verde) sobre snap rejilla HUD + cyan/connected; `object`: `maStlUpdateObjectInsertionPickHover` → hover STL + recuadro verde |
| `maStlRaycastClipStlMeshFirst(clientX, clientY)` | Puntero → primera intersección en `clipStlMeshes` (cada Mesh = objeto/parte) |
| `maStlUpdateObjectInsertionPickHover(clientX, clientY)` | Hover objeto: highlight STL + marca inserción en planta |
| `maStlSnapFloorToGridIntersectionMm(floorX, floorZ, minorMm)` | Snap X/Z a la rejilla menor (mm escena); `minorMm` opcional — default 500 mm |
| `maStlSnapFloorToGridFeatures(floorHit, proximity, gridSnapMm)` | Snap informativo: entre cruces de la rejilla menor, puntos medios de aristas y centros de celda; misma lógica `active` (proximidad + umbral en pantalla) que `maStlSnapFloorToGridIntersection` |
| `onCanvasPointerDownLineTool(ev)` / `onCanvasClickLineTool(ev)` / `onCanvasPointerMoveLineToolSync(ev)` | Herramienta **polilínea** Desing_2 (`#ma-stl-tool-polyline`): **P1** + tramos en planta (segundo punto por **clic** o **longitud + Intro** en `picking2`). Tras cada clic sin snap de conexión encadena (`maStlPolylineToolAdvanceAfterSegmentMm`); si el clic usa snap línea→línea (`maStlApplyLineToolVertexSnapOnClickMm`, vértice o cuerpo ≤ 100 mm XZ) **`maStlStopLineToolModesToolbar`** termina el comando (como Escape). **Enter** con herramienta apagada repite el **último comando de barra** (`maStlDesing2ApplyWindowEnterToRepeatLastToolbarCommand`; predeterminado polilínea → `picking1`). Salida: **Escape**, clic en `#ma-stl-tool-polyline`, otra herramienta de barra, **snap connect en clic**, o **clic izquierdo en lienzo vacío** (sin snap/colocación) estando en `picking1`. Tras P1, con **orto 15°** activo (predeterminado): dirección hacia P2 en múltiplos de 15° en XZ (caucho, clic, distancia+Intro); **F8** o botón `#ma-stl-tool-ortho-15` alterna orto/off. Longitud digitada/commit cota en metros: hasta **tres decimales**… |
| `onCanvasPointerMoveUserFloorLineHover(ev)` | Sin `picking*` ni modo pick rejilla/objeto: resalta trazo bajo cursor (`#ffaa33`) y muestra **sólo cota longitud** (`maStlUpdateUserFloorLineDimHud(true)` — guías longitud + `#ma-stl-floor-dim-readout`; **sin** ΔX/ΔZ ni asas). Si hay `maStlUserFloorLineSelected`, prevalece sobre hover y mantiene las **tres cotas** + asas aunque el ratón salga del trazo |
| `onCanvasClickUserFloorLineSelect(ev)` | **Clic simple** en canvas (captura, tras retardo `MA_STL_USER_FLOOR_LINE_CLICK_SELECT_DELAY_MS` ≈ 260 ms para no competir con doble clic): selecciona segmento (`maStlUserFloorLineSelected`), muestra **longitud + ΔX + ΔZ** + guías CAD + asas P1/P2 libres. **No** abre inputs. Clic en lienzo vacío deselecciona. Escape / `maStlClearUserFloorLineSelection` limpian selección |
| `onCanvasDblClickUserFloorLineDimension(ev)` | **Doble clic** en canvas: cancela el retardo del clic simple; selecciona línea; mantiene **las tres cotas** visibles y abre edición (`maStlBeginUserFloorLineDimensionEdit`). Si el punto cae sobre una etiqueta HUD (ΔX/ΔZ/longitud por prioridad pantalla), abre **sólo esa** cota; si no (segmento naranja u halo unión), abre **las tres a la vez** (`editKind` **`all`**: inputs `#ma-stl-floor-dim-input`, `#ma-stl-floor-dim-input-dx`, `#ma-stl-floor-dim-input-dz`). Tab entre campos; Enter en cualquiera o blur fuera del grupo confirman las tres (`maStlCommitUserFloorLineDimensionMulti`: ΔX → ΔZ → longitud). Los botones HUD usan **`dblclick`** (no clic simple). Formato **m**/ **mm**, **tres decimales** coherentes con `MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS` |
| `maStlCommitUserPlanLineSegmentMm(a, b)` | Crea `Line2` + `userData.maStlUserPlanLine`: `id`, `p1Mm`/`p2Mm` (punto inicial **fijo** al redimensionar); tras añadir llama **`maStlRefactorUserFloorLinesMergeCollinear`** |
| `maStlTryMergeUserFloorLineWithConnected(lnNew)` | Si el trazo comparte **un único** vértice (ε `maStlUserFloorLineMergeEndpointEpsMm`) con otro segmento, ambos son **colineales** en XZ (`|d1×d2| ≤ MA_STL_USER_FLOOR_LINE_MERGE_COLLINEAR_CROSS_MAX`) y **extensión de cadena** en la junta: `(p2,p1)`/`(p1,p2)` ⇒ `dot(d1,d2) > MA_STL_USER_FLOOR_LINE_MERGE_SAME_SENSE_DOT_MIN`; `(p1,p1)`/`(p2,p2)` ⇒ `dot < −umbral`. Fusiona en el segmento **ya existente** (extiende `p1` o `p2` hacia el vértice libre del otro), elimina el trazo sobrante y repite por **cadenas** colineales. Sentido opuesto o no colineal ⇒ dos segmentos |
| `maStlWeldAllUserFloorLineEndpointsMm()` / `maStlRefactorUserFloorLinesMergeCollinear()` | Refactor global: solda iterativamente todos los extremos XZ (≤ ε merge), luego repite `maStlTryMergeUserFloorLineWithConnected` sobre cada segmento hasta estabilizar. Disparadores: tras cada **`maStlCommitUserPlanLineSegmentMm`**, al salir del modo polilínea (**`maStlStopLineToolModesToolbar`**, p. ej. Escape o snap connect), y **clic derecho corto** en canvas… Durante herramienta polilínea el RMB sigue siendo pan si hubo arrastre |
| `maStlPickUserFloorLineNearScreenMm` / `maStlWorldMmToScreenPx` | Raycast overlays desactivado: pick por distancia pantalla segmento proyectado |
| `maStlResizeUserFloorLineToLengthMm` | `p2Mm = p1Mm + dirección_normalizada × longitud`; dirección desde `p1→p2` actual |
| `maStlRebuildUserFloorDimGuideGeometry` / `maStlEnsureUserFloorDimArrowMesh` / `maStlUpdateUserFloorLineDimHud` / `maStlSyncUserFloorDimHudScreenOnly` / `maStlUserFloorLineDimHudLengthOnlyMode` | **Tres cadenas** en planta (XZ): cota **longitud** paralela al segmento 3D (P1→P2); **ΔX** y **ΔZ** **desde `maStlRulerAnchorMm` (intersección reglas, suelo)** hasta **P1** (primer clic, punto inicial) en plano horizontal (extensiones en L cuando hace falta). `maStlUserFloorLineDimHudLengthOnlyMode`: **hover** ⇒ `true` (sólo longitud); **selección** o **edición** ⇒ `false` (tres cotas); **arrastre asa P1/P2** ⇒ `true`. `THREE.LineSegments` + **malla triangular** (`MeshBasicMaterial` `DoubleSide`) para **flechas CAD** en extremos (punta hacia el interior del tramo medido; tamaño vía `MA_STL_USER_FLOOR_LINE_DIM_ARROW_MESH_SCALE` ≈ 1/3). Overlay: `#ma-stl-floor-dim-readout`, `#ma-stl-floor-dim-readout-dx`, `#ma-stl-floor-dim-readout-dz` (`maStlPlaceAllFloorDimHudReadouts` / `maStlUserFloorDimProjectHudReadoutScreens`). Cambio de anclaje ⇒ `maStlInvalidateUserFloorDimGuideGeomCache` + resync HUD si hay hover/selección. |
| `maStlResizeUserFloorLinePlanDeltaXMm` / `maStlResizeUserFloorLinePlanDeltaZMm` | **Traslación en planta** del segmento completo (P1 y P2 se desplazan igual en el eje editado): el valor es **delta firmado desde el anclaje de reglas (`maStlRulerAnchorMm`) hasta P1** en ese eje (`shift = (ref + Δ) − P1`; `P1/P2 += shift` en X o Z). Conserva longitud y dirección 3D; se rechaza el commit si la longitud residual quedaría `<` mínimo de segmento (`maStlUserFloorSegmentMinMm`). Redondeo vía **`maStlParseLengthInputValueToMm`** + **`maStlDesing2SignedDeltaMmRoundedEditableFromMm`** |
| `maStlDisposeUserFloorLineDimEdit` / `maStlBeginUserFloorLineDimensionEdit` | Modo **`length` / `deltaX` / `deltaZ`**: input `#ma-stl-floor-dim-input`. Modo **`all`**: tres inputs paralelos a los readouts. **`capt.dimKind`** en blur fotografía `{ line, inputEl, inputLenEl, inputDxEl, inputDzEl, detachBlur, dimKind }` (multi: sin `detachBlur` per-input; usa `maStlUserFloorLineDimEditDispose` global). Durante edición: asa DOM `#ma-stl-floor-line-drag-handle` (cuadrado azul 10×10 px en midpoint **geométrico** P1–P2) para arrastrar el segmento en XZ (`maStlTranslateUserFloorLineSegmentPlanMm` vía raycast suelo); órbita desactivada mientras se arrastra; oculta al cerrar edición (Escape / Enter / blur). |
| `maStlUserFloorLineEndpointIsConnected` / `maStlUserFloorLineP1IsConnected` / `maStlUserFloorLineP2IsConnected` / `maStlSyncUserFloorLineEndpointHandles` / `maStlSetUserFloorLineP1PlanMm` / `maStlSetUserFloorLineP2PlanMm` / `maStlProjectPointOntoSegmentAxisMm` / `maStlProjectPointOntoUserFloorSegmentBodyXzMm` / `maStlFindNearestUserFloorLineSegmentSnapMm` / `maStlFindUserFloorEndpointDragSnapCandidate` / `maStlApplyUserFloorEndpointStretchSnapConnect` | **Selección por clic** (`maStlUserFloorLineSelected`, no sólo hover): asas azules circulares **11 px** visuales, **24 px** área de captura (`#ma-stl-floor-line-p1-handle`, `#ma-stl-floor-line-p2-handle`, clase `.desing2-stl-floor-line-endpoint-handle`) en **P1** y **P2** si el extremo no está en junta (`maStlUserFloorPlanEndpointIncidentCountMm` ≥ 2 en ε merge — antes “otra línea comparte vértice”). P1 en anclaje reglas recibe **nudge** en pantalla (`maStlNudgeUserFloorLineP1HandleScrFromAnchor`) para no quedar oculto bajo la esfera roja. Ocultas durante herramienta línea, pick anclaje, edición cotas (conflicto con asa midpoint). Soldadura en hover eliminada (sólo commit/refactor). Arrastre **restringido al eje** del segmento: P1 se mueve sobre la recta P2→P1 (`maStlProjectPointOntoSegmentAxisMm`, P2 fijo); P2 sobre P1→P2 (P1 fijo); `t ≥ maStlUserFloorSegmentMinMm()`. **Preview snap** (esfera cian/verde) durante arrastre si extremo en eje o cursor ≤ **100 mm** XZ de P1/P2/mid/**cuerpo** vecino (`maStlFindUserFloorEndpointDragSnapCandidate`); al **soltar** (pointerup) con snap activo y extremo ≤ **100 mm** XZ (`maStlApplyUserFloorEndpointStretchSnapConnect`) conecta de inmediato al punto exacto (`kind`: `p1`|`p2`|`mid`|`segment`) y refactoriza — **sin clic extra** ni doble clic. Pointer capture en el botón asa. `aria-label` vía `data-ma-stl-user-floor-line-p1-handle-aria` / `data-ma-stl-user-floor-line-p2-handle-aria`. |
| `maStlStartPolylineToolModesToolbar()` / `maStlStopLineToolModesToolbar()` / `maStlSyncLineToolHud()` / `maStlDesing2SetLastToolbarCommandId()` / `maStlDesing2ActivateToolbarCommandById()` / `maStlDesing2ApplyWindowEnterToRepeatLastToolbarCommand()` / `maStlLineToolApplyWindowEnterToActivate()` / `maStlWireDesingV2LineToolEnterActivateKeyListener()` | Estado `picking1`/`picking2` (`maStlLineToolKind === 'polyline'`), HUD instrucción + coords (plantilla X/Z m); en `picking2` fila `#ma-stl-line-tool-hud-distance` + vista previa ≈ longitud; teclado global para dígitos/Intro salvo foco en otro input o edición de cotas. Tras P1 el foco pasa en microtarea al input distancia; **Intro en el botón polilínea (#ma-stl-tool-polyline)** no ejecuta segundo “clic” que cerrara el modo sin confirmar segmento (`keydown` capture + blur al activar). Tras segmento con **snap connect** en clic, `maStlStopLineToolModesToolbar` sale por completo; sin connect encadena otro tramo. **Enter** con modo apagado repite `maStlDesing2LastToolbarCommandId` (predeterminado `ma-stl-tool-polyline`) vía `maStlDesing2ActivateToolbarCommandById` |
| `maStlStartOffsetToolModesToolbar()` / `maStlStopOffsetToolModesToolbar()` / `maStlSyncOffsetToolHud()` / `maStlOffsetToolSideSignFromFloorPointMm()` / `maStlOffsetToolParallelEndpointsMm()` / `maStlOffsetToolParallelEndpointsWithMitersMm()` / `maStlOffsetToolFindCommittedOffsetOfNeighborMm()` / `maStlOffsetToolAcceptMiterEndpointMm()` / `maStlOffsetToolCommitParallelCopyMm()` | Estado `pickLine` → `pickDirection`; HUD `#ma-stl-offset-tool-hud`; preview discontinuo `maStlOffsetToolPreviewLine`; distancia `maStlOffsetToolDistanceMm` (default `maStlDesing2EnvOffsetDefaultMm()`); doble clic abre `#ma-stl-offset-tool-hud-distance`; clic dirección confirma copia paralela (miter interior en ambos extremos de cadenas cerradas) y sale del modo |
| `maStlDesing2BeginWindowSelection(options)` / `maStlDesing2EndWindowSelection()` / `maStlDesing2IsWindowSelectionActive()` / `maStlDesing2IsWindowSelectionBusy()` / `maStlDesing2WindowSelectionPointerDown(ev)` / `maStlDesing2ComputeWindowSelectionHits(rect, pickOptions)` / `maStlDesing2EnsureWindowSelectionMarqueeEl()` / `maStlDesing2MeshScreenBoundsPx()` / `maStlRectPxFromDrag()` / `maStlSegmentIntersectsRectPx()` | **Selección por ventana/cruce (marquee genérico)** — sesión opt-in por herramienta; DOM `#ma-stl-window-selection-marquee`; umbral `MA_STL_WINDOW_SELECTION_DRAG_THRESHOLD_PX` (4 px); L→R **ventana** (contención), R→L **cruce** (intersección); callback `onSelectionComplete(lines, meshes, mode)`; opciones `enabled`, `additive`, `blocksMarqueeAt`, `filterLine`, `filterMesh`, `onMarqueeDragStart`; pointer capture durante arrastre (sin conflicto órbita si la herramienta bloquea órbita); expuesto en `window` |
| `maStlStartDeleteToolModesToolbar()` / `maStlStopDeleteToolModesToolbar()` / `maStlSyncDeleteToolHud()` / `maStlDeleteToolToggleLineSelection()` / `maStlDeleteToolAddLineSelection()` / `maStlDeleteToolAddMeshSelection()` / `maStlDeleteToolToggleMeshSelection()` / `maStlDeleteToolCommitSelection()` / `maStlDeleteToolApplyWindowKeydownConfirm()` / `maStlDeleteToolWireWindowSelection()` / `maStlDeleteToolUnwireWindowSelection()` / `maStlApplyUserFloorLineDeletePickMaterial()` / `maStlDeleteToolApplyStlMeshDeletePickMaterial()` / `maStlIsDeletableStlMesh()` / `maStlDetachStlMeshToGraveyard()` / `maStlRestoreStlMeshFromGraveyard()` / `maStlDesing2ApplyStlMeshSnapshotFromUuids()` | Modo borrar: `maStlDeleteToolActive`; multi-selección mixta `maStlDeleteToolSelectedLines` (`Set<Line2>`) + `maStlDeleteToolSelectedMeshes` (`Set<Mesh>`); **clic** alterna línea/malla (50 % opacity); **marquee** vía API genérica (`maStlDeleteToolWireWindowSelection`) en lienzo vacío — unión aditiva; hover no seleccionado = brillo habitual; **Enter/Espacio/Supr** confirma (`deleteUserFloorLinesAndStlMeshes` undo snapshot) y sale; **Escape** cancela; HUD `StlPreview_DeleteToolInstruction`. Pick clic: línea prioriza sobre malla |
| `maStlStartWallDimToolModesToolbar()` / `maStlStopWallDimToolModesToolbar()` / `maStlWallDimDetectWallPairsMm()` / `maStlWallDimSelectRepresentativePairs()` / `maStlWallDimBuildPlacementFromPair()` / `maStlWallDimRefreshScanAndDraw()` / `maStlSyncWallDimReadoutScreens()` / `maStlSyncWallDimToolToggleBtnUi()` | Cota espesor muro Desing_2 (`#ma-stl-tool-wall-dim`): escaneo pares paralelos usuario, dedup por espesor ±5 mm, guías CAD + overlay `#ma-stl-wall-dim-overlay`; Escape / segundo clic apaga |
| `maStlLineToolComputeRubberBandEndMm` / `maStlLineToolUpdatePreviewDimHud` / `maStlLineToolSyncPreviewDimHudScreenOnly` / `maStlLineToolHidePreviewDimHud` | En **`picking2`**, cotas CAD en vivo (`maStlRebuildUserFloorDimGuideGeometry(ud, true)` — **sólo longitud**; sin ΔX/ΔZ ni guías delta): longitud P1→cursor/caucho. Overlay `#ma-stl-floor-dim-readout` (sin `#ma-stl-floor-dim-readout-dx` / `-dz`). Se actualizan en cada `maStlLineToolRefreshPicking2RubberBand`. Clase overlay `desing2-stl-floor-dim-overlay--line-tool-preview` (sólo lectura). Se ocultan al confirmar segmento, Escape o salir de `picking2`. |
| `maStlFindNearestUserFloorLineSegmentSnapMm` / `maStlFindLineToolVertexSnapCandidate` / `maStlSetLineToolVertexSnapHighlight` / `maStlApplyLineToolVertexSnapOnClickMm` / `maStlResolveLineToolFloorPointMm` / `maStlSyncLineSnapAngleHud` | Snap línea→línea unificado: candidatos **P1/P2/mid** y **punto sobre el cuerpo** del segmento (proyección XZ, t∈[0,1]) en `maStlUserLinesGroup`; tolerancia estricta **100 mm** XZ (`MA_STL_USER_FLOOR_LINE_VERTEX_SNAP_MM`, sin boost px); esfera overlay cian→verde (**200 mm** visual, `MA_STL_LINE_TOOL_VERTEX_SNAP_BALL_VISUAL_RADIUS_MM`); prioridad sobre rejilla; clic (`maStlApplyLineToolVertexSnapOnClickMm`) fija `{x,y,z}` exacto con `kind` y anula orto 15° en P2. **T-junction:** extremo perpendicular puede anclarse en el trazo de otra línea aunque sus vértices estén lejos. **HUD ángulo** `#ma-stl-line-snap-angle-hud`: en preview snap (`picking2` o estirar asa) muestra acimut original (caucho P1→cursor o eje línea estirada) vs. eje segmento destino (`atan2(dZ,dX)`, 0°=+X, 90°=+Z); colineal ≤1° → `45° (colineal)`; oculto con la esfera |
| `maStlSyncLineToolOrtho15ToggleUi()` / `maStlToggleLineToolOrtho15FromUi()` / `maStlWireDesingV2F8OrthoKeyListener()` | Orto 15° en planta: botón `#ma-stl-tool-ortho-15` (barra inferior), estado `active` / `aria-pressed` y tecla **F8** con visor activo; no dispara con foco en campos de texto/IME |
| `maStlDesing2SerializeEditSnapshot()` / `maStlDesing2ApplyEditSnapshot(snapshot)` | Snapshot undo: líneas usuario (`id`, `p1Mm`, `p2Mm`) + `nextSegId` + `rulerAnchor` mm + **`stlMeshUuids`** (uuids de mallas visibles en `clipStlMeshes`); restauración STL vía `maStlStlMeshGraveyard` (mallas retiradas conservadas en memoria); snapshots antiguos sin `stlMeshUuids` no alteran mallas; **no** incluye cámara/órbita |
| `maStlDesing2PushUndoAction({ label, undo, redo })` | API pública (también `window.maStlDesing2PushUndoAction`) para registrar acciones undo/redo genéricas |
| `maStlDesing2PushEditSnapshotUndo(label, before, after)` | Atajo snapshot: empuja par before/after al stack undo |
| `maStlDesing2Undo()` / `maStlDesing2Redo()` | Deshacer / rehacer; pilas `maStlDesing2UndoStack` / `maStlDesing2RedoStack` (expuestas en `window`) |
| `maStlDesing2SyncUndoRedoToolbarUi()` / `maStlWireDesingV2UndoRedoKeyListener()` | Botones `#ma-stl-tool-undo` / `#ma-stl-tool-redo` (disabled si pila vacía); **Ctrl+Z** undo, **Ctrl+Y** y **Ctrl+Shift+Z** redo |
| `maStlDesing2CancelTransientToolsEscape()` | **Escape**: cancela edición cotas línea usuario si estaba abierta; modo línea (`picking*`), modo offset (`pickLine`/`pickDirection`), **modo borrar** (`maStlDeleteToolActive`), picks de anclaje rejilla/objeto, órbita bloqueada o desbloqueo diferido; limpia rubber-band, preview offset, highlights rejilla/objeto STL/inserción, HUD cotas línea y HUD coords anclaje; quita clase `active` / `aria-pressed` en línea/rejilla/objeto/offset/**delete**; opcional toast vía `#ma-stl-viewer-shell` `data-ma-stl-escape-cancel-toast` (misma ruta Bootstrap que Guardar vista). Ignora Escape si el foco está en texto/`select`/contenido editable o dentro de `.modal.show` **salvo** `#ma-stl-line-tool-hud-distance` en `picking2` o `#ma-stl-offset-tool-hud-distance` con offset activo |
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
| `maStlApplyDesing2RulerLineMaterialTheme` / `maStlCreateDesing2RulerLineMaterial` | Líneas de reglas: gris ~70 % opacity con fondo claro; azul/cian anterior con `#ma-stl-dark-bg-toggle` activo. Marcas en múltiplos de 5 (5, 10, 15…) y sus etiquetas: azul `#2563eb` fijo |
| `maStlSyncEdgeRulersOverlay` / `maStlSyncEdgeRulersOverlayVisibility` | **Reglas borde visor** (`#ma-stl-edge-rulers-overlay`): bandas canvas superior (+X) y derecha (+Z) dentro del lienzo; toggle `#ma-stl-edge-rulers-toggle` (`maStlEdgeRulersManualOn`); redibujado en `tick`, resize, encuadre, anclaje, tema |
| `maStlEdgeRulerSampleViewportFloorExtents` / `maStlEdgeRulerBuildAxisTicks` / `maStlEdgeRulerNiceLabelStepMm` | Extensión visible en suelo (raycast esquinas/bordes canvas); marcas alineadas al anclaje; LOD menor = Entorno × `maStlDesing2GridLodCellSizesMm`; etiquetas enteras cada 1–5–10… m según ~42–78 px entre marcas |
| `maStlResetOrbitTargetToRulerAnchor()` | Sincroniza `controls.target` con anclaje |
| `maStlSetRulerAnchorFromInsertionPoint(pos)` | Aplica anclaje XY en planta (Y forzado a suelo) |
| `maStlRebuildRulerAnchorMarker()` | Reconstruye marca visual anclaje |
| `syncMaStlRulerPickToolbarUi()` | Estado botones rejilla/objeto (`#ma-stl-ruler-anchor-*-pick-toggle`), excluyentes |
| `maStlRebuildGridIntersectionPickHighlightMeshesForSnapChange()` | Limpia meshes de highlight cruce rejilla (cambio de snap / pestaña Entorno) |
| `applyDesing2EntornoLive()` | Aplica `#ma-stl-entorno-grid-snap-mm` y `#ma-stl-entorno-ruler-extent-m` a rejilla LOD, pick y cotas |
| `maStlExitRulerAnchorPickAfterPlacement()` | Tras clic válido canvas: teardown + diferir unlock órbita (ver orbit-pivot doc) |
| `maStlStopRulerAnchorPickModesToolbar()` | Toolbar off: cerrar modo pick sin defer |
| `maStlCancelAllViewerInteractionModes()` | Desing_2: Escape u otro uso — edición cotas línea (`maStlDisposeUserFloorLineDimEdit`), polilínea (`maStlStopLineToolModesToolbar`), pick rejilla/objeto (`maStlStopRulerAnchorPickModesToolbar`), órbita pick-lock rezagada (`maStlUnlockOrbitForRulerAnchorPick`), highlights/HUD/toolbar `#ma-stl-tool-polyline` / `#ma-stl-ruler-anchor-*` |
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
| Reglas UCS (suelo) | visibles (`#ma-stl-ucs-rulers-toggle` activo; `maStlUcsRulersManualOn`) |
| Reglas borde visor | visibles (`#ma-stl-edge-rulers-toggle` activo; `maStlEdgeRulersManualOn`); canvas top/right inset 26 px (`site.css` `--ma-stl-edge-ruler-thickness`) |

Tras `loadStl`, `refitCamerasToObject` sustituye `lastMaxDim` y el frustum por el AABB del modelo.

## Reglas borde visor (canvas DOM)

Cuando `#ma-stl-edge-rulers-toggle` está activo (`maStlEdgeRulersManualOn`), el visor muestra **dos reglas tipo CAD** pegadas al borde del lienzo (no a las barras flotantes). Independiente de `#ma-stl-ucs-rulers-toggle` (reglas UCS en suelo, `maStlBuildPlanRulers`):

| Elemento | Detalle |
|----------|---------|
| DOM | `#ma-stl-edge-rulers-overlay` en `_Desing2StlViewerWorkspace.cshtml`; `#ma-stl-edge-ruler-top` (horizontal) y `#ma-stl-edge-ruler-right` (vertical); esquina superior-derecha libre (hueco 26×26 px) |
| Ejes | Superior: **+X** en planta (cotas firmadas desde `maStlRulerAnchorMm.x`). Derecha: **+Z** en planta (cotas firmadas desde `maStlRulerAnchorMm.z`) |
| Etiquetas | Metros enteros (`1`, `2`, `3`…); medios metros = marcas cortas sin número; múltiplos de 5 m en azul `#2563eb`; anclaje = marca roja |
| Adaptación zoom | `maStlWorldMmPerPixel` + LOD rejilla (`maStlDesing2GridLodCellSizesMm` con paso Entorno) para marcas menores; `maStlEdgeRulerNiceLabelStepMm` elige 1/2/5/10… m entre etiquetas (~42–78 px) |
| Posición marcas | Proyección 3D→2D de puntos suelo `(tickX, floorY, refZTop)` / `(refXRight, floorY, tickZ)`; regla **derecha**: restar inset superior (`MA_STL_EDGE_RULER_THICKNESS_PX`, 26 px) en Y canvas; extensión visible = raycast suelo en esquinas y bordes del canvas WebGL |
| Actualización | `tick()` si reglas activas (con caché de estado cámara/anclaje/tamaño para no raycastear en frames idénticos); también `resizeRendererToHost`, `rebuildMaStlUcsOverlayDecor`, cambio fondo oscuro, toggle reglas |
| Toggle | `#ma-stl-edge-rulers-toggle` en submenú flyout `_Desing2BottomToolBar.cshtml`; **independiente** de `#ma-stl-ucs-rulers-toggle` (suelo) y de `#ma-stl-grid-toggle` |

## Anclaje de reglas

Punto clave del proyecto: el usuario elige el **punto de inserción** del objeto como origen de reglas (+X / −Z) y pivote de órbita.

| Elemento | Detalle |
|----------|---------|
| Estado | `maStlRulerAnchorMm` (`THREE.Vector3`), default `(0, 0, 0)` en suelo `MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM` |
| Modo rejilla — toolbar | `#ma-stl-ruler-anchor-pick-toggle` — `ri-crosshair-2-line` (submenú flyout barra inferior). Coloca reglas cuando el cruce al **incremental de rejilla** (Entorno, p. ej. 500 mm) está **connected** (verde) **o** cuando hay snap a **P1/P2/mid/cuerpo** de línea usuario (misma esfera cian/verde que herramienta línea). Clic fuera de snap muestra toast `StlPreview_RulerAnchorGridSnapRequiredToast` y **no** cambia anclaje |
| Modo objeto — toolbar | `#ma-stl-ruler-anchor-object-pick-toggle` — `ri-cube-line` (submenú flyout barra inferior). `pointermove`: raycast en `clipStlMeshes`; hover STL (azul) + recuadro verde en **punto de inserción** de esa malla — ver fila siguiente. Mutuamente excluyente con modo rejilla (`maStlRulerAnchorPickMode` `grid`/`object`/null). Toasts entran vía `_Desing2BottomToolBar.cshtml` (`data-ma-stl-*`) |
| **Qué significa “punto de inserción”** | Origen tipo **croquis CAD** en la **esquina inferior izquierda de la huella** del objeto en planta: `(min.x, suelo Y, max.z)` del AABB mundo del mesh impactado (`maStlGetInsertionPointBottomLeftFootprintWorld`). Convención planta Desing_2: cotas −Z = «arriba» en papel ⇒ inferior = +Z; izquierda = menor X. No hay punto guardado en el STL; si existiera metadata de bloque DWG en el futuro podría añadirse otro proveedor en `maStlInsertionPointProviders`. Piezas separadas (`*2.stl`) siguen teniendo cada una su propio AABB |
| Modo rejilla — interacción | Crosshair + probe suelo → prioridad `maStlFindFloorLineVertexSnapAtPointer` (P1/P2/mid/**cuerpo** líneas usuario, ≤ 100 mm XZ); si no, `maStlSnapFloorToGridIntersection`; HUD X/Z m; marcador cyan hasta “locked” green |
| Clic modo rejilla | Si snap línea o `snap.active` rejilla: `maStlSetRulerAnchorFromGridSnap` + toast `StlPreview_RulerAnchorGridIntersectionToast`; `maStlExitRulerAnchorPickAfterPlacement()` |
| Clic modo objeto | Si raycast STL: misma función de inserción + toast `StlPreview_RulerAnchorObjectInsertionToast`; sin impacto → `StlPreview_RulerAnchorObjectPickMissToast` |
| Snap rejilla | Paso configurable (**mm escena** = mm físicas con ×1000 de Desing_2): panel **Entorno** `#ma-stl-entorno-grid-snap-mm`; LOD de la rejilla multiplica este paso | 
| Marca | `maStlRulerAnchorMarkerGroup`: cruz cyan (toggle reglas) + esfera roja fija del cruce (no se oculta con `#ma-stl-ucs-rulers-toggle`) |
| Reglas | `maStlBuildPlanRulers(..., anchorXMm, anchorZMm)` — baselines desde anclaje; paso menor = incremental Entorno; paso mayor = **5×** ese valor; **etiquetas numéricas solo en metros enteros** (1, 2, 3…) vía `maStlRulerLabelMetersFromWorldM`, cada 1 m a lo largo del eje; marcas en medios metros (0,5 / 1,5…) más cortas; marcas en enteros más largas; **5 / 10 / 15… m**: trazo y número en `#2563eb`, etiqueta algo más separada del palo; enteros 1–4, 6–9… más cerca del palo (~87 % del offset base); color trazos restantes: gris translúcido en fondo claro, azul/cian si fondo negro. **Borde visor:** ver sección «Reglas borde visor» |
| Órbita | `maStlResetOrbitTargetToRulerAnchor()` — `controls.target` = anclaje (sin raycast al rotar). Ver [`desing-2-orbit-pivot.md`](desing-2-orbit-pivot.md) |
| Salida pick | Segundo clic en el botón del modo activo, o clic canvas tras colocación exitosa |
| Cookie | `rulerAnchor: { x, y, z }` y `environment: { gridSnapMm, rulerExtentCapM, offsetDefaultMm }` en `ma_stl_desing2_viewer_state_global` (perspectiva al pulsar **Guardar vista**) |

La rejilla infinita sigue en el plano Y=0 global; solo reglas y pivote de órbita usan el anclaje.

### Herramienta polilínea (barra superior `#ma-stl-tool-polyline`)

Única herramienta de trazado de segmentos en planta (no hay botón línea separado).

| Elemento | Detalle |
|----------|---------|
| Botón | `#ma-stl-tool-polyline` — icono `ri-route-line` |
| Estado | `maStlLineToolState` (`picking1` / `picking2`); `maStlLineToolKind === 'polyline'` |
| Flujo | Clic icono **o Enter** (modo apagado; repite último comando, predeterminado polilínea) → `picking1` → **P1** → `picking2` + caucho/cotas → **cada clic** confirma un tramo (`maStlCommitUserPlanLineSegmentMm`) y, si **no** hay snap connect, encadena desde el nuevo vértice (`maStlPolylineToolAdvanceAfterSegmentMm`). Cada tramo es un `Line2` independiente en `maStlUserLinesGroup` |
| Finalizar | **Snap connect en clic** (vértice P1/P2/mid o punto sobre cuerpo de otra línea, ≤ 100 mm XZ vía `maStlApplyLineToolVertexSnapOnClickMm` → `maStlStopLineToolModesToolbar`), **Enter** (sin distancia tecleada: termina; con distancia válida en `picking2`: confirma ese tramo y termina), **Escape** (descarta caucho del tramo en curso; conserva trazos ya confirmados), clic otra vez en el icono, otra herramienta de barra, o clic vacío en `picking1` |
| Snap | Hover: mismas marcas cian/verde que el pick de rejilla. **Prioridad** sobre rejilla: ≤ **100 mm** XZ de **P1**, **P2**, **punto medio** o **cuerpo** de otra línea usuario; clic fija el punto exacto (`kind`: `p1`|`p2`|`mid`|`segment`; sin orto 15° en P2). En `picking2` se excluye el propio P1 del trazo en curso |
| Snap / orto / cotas | `maStlLineToolComputeSegmentEndMmFromFloorPointMm`, orto 15° (F8 / `#ma-stl-tool-ortho-15`), preview cotas longitud en `picking2` |
| HUD | `#ma-stl-line-tool-hud` con `data-ma-stl-polyline-tool-instruction-first/second` (`StlPreview_PolylineToolInstructionFirst/Second`); coordenadas rejilla/snap; `#ma-stl-line-tool-hud-distance` en `picking2` |
| Hover / cotas tras colocación | Fuera de `picking*` — ver tabla **Gestos línea usuario** más abajo |
| Edición cotas | **Sólo `dblclick`** — ver tabla gestos |
| Persistencia escena | Segmentos en `maStlUserLinesGroup`; **`maStlRefactorUserFloorLinesMergeCollinear`** tras commit y al salir del modo |
| Exclusión | Mutuamente excluyente con pick rejilla/objeto, offset y placeholders de barra (`maStlStartPolylineToolModesToolbar` / `maStlStopLineToolModesToolbar`) |
| Cámara | Bajo modo polilínea, `maStlLockOrbitForRulerAnchorPick()` deja pan (RMB) y zoom; rotación OFF. Al salir (`maStlStopLineToolModesToolbar`) se rehabilita órbita. **Clic derecho corto** sin pan ejecuta refactor merge colineal manual |
| Undo | Un snapshot por segmento confirmado (`createUserFloorLine` vía `maStlCommitUserPlanLineSegmentMm`) |

#### Gestos línea usuario (segmentos ya colocados)

Variables de estado:

| Variable | Rol |
|----------|-----|
| `maStlHoveredUserFloorLine` | Línea bajo cursor (hover transitorio) |
| `maStlUserFloorLineSelected` | Línea seleccionada por clic simple (persiste hasta clic en vacío / Escape) |
| `maStlUserFloorDimDomHudEditing` | Sesión edición activa (doble clic) |
| `maStlUserFloorLineHudTargetLine()` | `maStlUserFloorLineSelected \|\| maStlHoveredUserFloorLine` — la selección prevalece sobre hover |

| Gesto | Resaltado trazo | Cotas DOM + guías CAD | Asas P1/P2 | Inputs edición |
|-------|-----------------|----------------------|------------|----------------|
| **Hover** (`onCanvasPointerMoveUserFloorLineHover`) | Sí (`#ffaa33`) | **Sólo longitud** (`lengthOnly=true`; sin ΔX/ΔZ) | No | No |
| **Clic simple** (`onCanvasClickUserFloorLineSelect`, ~260 ms) | Sí (selección) | **Longitud + ΔX + ΔZ** (`lengthOnly=false`) | Sí (extremos libres) | No |
| **Doble clic** (`onCanvasDblClickUserFloorLineDimension`) | Sí (selección) | **Longitud + ΔX + ΔZ** | Sí (durante edición según reglas asas) | Sí (`all` o cota bajo cursor) |
| **Ratón fuera del trazo** con selección activa | Base en otras líneas | **Tres cotas** de la línea seleccionada | Sí | No |
| **Arrastre asa P1/P2** | — | **Sólo longitud** (transitorio) | Sí (la arrastrada) | No |

Deselección: clic en lienzo vacío, Escape (`maStlClearUserFloorLineSelection`). Suprimido mientras hay `picking*` polilínea, modo offset, **modo borrar**, pick anclaje rejilla/objeto o edición cotas activa.

### Selección por ventana / cruce (marquee genérico)

Módulo reutilizable para herramientas Desing_2 que necesiten multi-selección por rectángulo en pantalla (líneas usuario + mallas STL).

| Elemento | Detalle |
|----------|---------|
| Activar | `maStlDesing2BeginWindowSelection({ enabled, additive, blocksMarqueeAt, filterLine, filterMesh, onMarqueeDragStart, onSelectionComplete })` |
| Desactivar | `maStlDesing2EndWindowSelection()` |
| Pointer | En `pointerdown` canvas (captura): `maStlDesing2WindowSelectionPointerDown(ev)` — sólo si hay sesión y `enabled()` |
| Estado | `maStlDesing2IsWindowSelectionActive()` (sesión); `maStlDesing2IsWindowSelectionBusy()` (arrastre pendiente/activo) |
| DOM | `#ma-stl-window-selection-marquee` — clase `.desing2-stl-window-selection-marquee` (`.--crossing` = dashed) |
| Umbral | `MA_STL_WINDOW_SELECTION_DRAG_THRESHOLD_PX` = 4 px — clic en entidad no dispara marquee |
| **Ventana** (L→R) | Línea: P1 **y** P2 dentro del rect. Malla: AABB pantalla **contenido** en el rect |
| **Cruce** (R→L) | Línea: segmento **intersecta** rect. Malla: AABB pantalla **solapa** rect |
| Callback | `onSelectionComplete(selectedLines, selectedMeshes, mode)` — `mode` = `'window'` \| `'crossing'` |
| Órbita | Pointer capture durante arrastre; cada herramienta debe bloquear órbita si no debe rotar (p. ej. borrar usa `maStlLockOrbitForRulerAnchorPick`) |
| i18n | `StlPreview_WindowSelectionHint` — texto reutilizable para HUDs futuros |
| Consumidor actual | Borrar: `maStlDeleteToolWireWindowSelection()` / `maStlDeleteToolUnwireWindowSelection()` |

**Ejemplo (futura herramienta):**

```javascript
maStlDesing2BeginWindowSelection({
  enabled: () => myToolActive,
  additive: true,
  blocksMarqueeAt: (x, y) => pickEntityAt(x, y) != null,
  filterLine: maStlIsUserFloorPlanLineObject,
  filterMesh: (m) => myToolAcceptsMesh(m),
  onMarqueeDragStart: () => clearHover(),
  onSelectionComplete: (lines, meshes, mode) => {
    lines.forEach(addToMySelection);
    meshes.forEach(addToMySelection);
  },
});
// pointerdown canvas → maStlDesing2WindowSelectionPointerDown(ev)
// al salir → maStlDesing2EndWindowSelection()
```

### Herramienta borrar (barra superior `#ma-stl-tool-delete`)

Elimina segmentos de línea usuario (`Line2` en `maStlUserLinesGroup`) **y** mallas STL cargadas (`clipStlMeshes`: primario + opcional `*2.stl`).

| Elemento | Detalle |
|----------|---------|
| Botón | `#ma-stl-tool-delete` — icono `ri-delete-bin-line` |
| Estado | `maStlDeleteToolActive` (`boolean`); selección pendiente `maStlDeleteToolSelectedLines` (`Set<Line2>`) + `maStlDeleteToolSelectedMeshes` (`Set<Mesh>`) |
| Flujo | Icono **o Enter** (modo apagado, si fue último comando) → crosshair + HUD → **clic** alterna línea o malla STL (50 % opacity) → **arrastre ventana** en lienzo vacío añade líneas/mallas al conjunto (ver Marquee) → **Enter/Espacio/Supr** confirma borrado y sale → **Escape** cancela sin borrar |
| Pick | Prioridad **línea usuario** (`maStlPickUserFloorLineForCanvasInteraction`, tol. 4 px) sobre **malla STL** (`maStlRaycastClipStlMeshFirst` en `clipStlMeshes`); excluye rejilla, reglas, overlays HUD, helpers |
| Marquee | API genérica `maStlDesing2BeginWindowSelection` — `#ma-stl-window-selection-marquee` (borde azul `#2563eb`; **cruce** = dashed). Umbral `MA_STL_WINDOW_SELECTION_DRAG_THRESHOLD_PX` (4 px). No inicia si down sobre línea/malla (`blocksMarqueeAt`). **Ventana** (L→R): línea con P1 **y** P2 dentro del rect; malla con AABB pantalla totalmente dentro. **Cruce** (R→L): segmento intersecta rect; AABB solapa rect. Unión aditiva vía `onSelectionComplete` → `maStlDeleteToolAdd*` |
| Visual línea | `maStlApplyUserFloorLineDeletePickMaterial` — color base `#ff6600`, `opacity` 0.5; hover no seleccionado = `#ffcc66` |
| Visual malla | `maStlDeleteToolApplyStlMeshDeletePickMaterial` — `opacity` 0.5, `transparent: true`; hover no seleccionado = emissive azul (mismo criterio pick objeto) |
| HUD | `#ma-stl-delete-tool-hud` — `StlPreview_DeleteToolInstruction` («clic o arrastre ventana/cruce… Enter/Espacio… Esc cancelar») |
| Undo | `maStlDesing2PushEditSnapshotUndo('deleteUserFloorLinesAndStlMeshes', before, after)` — snapshot incluye `stlMeshUuids[]`; mallas borradas van a `maStlStlMeshGraveyard` hasta undo o nueva carga STL |
| Exclusión | Mutuamente excluyente con polilínea, offset y pick anclaje rejilla/objeto |
| Cámara | Mismo lock que polilínea/offset; refit tras borrar mallas si queda al menos una; Escape / segundo clic icono cancela |

Copia paralela de una línea usuario en planta XZ a distancia configurable.

| Elemento | Detalle |
|----------|---------|
| Botón | `#ma-stl-tool-offset` — icono `ri-shape-line` |
| Estado | `maStlOffsetToolState`: `null` \| `pickLine` \| `pickDirection` |
| Distancia | `maStlOffsetToolDistanceMm` — inicialmente `maStlDesing2EnvOffsetDefaultMm()` (Configuración / cookie, default **300 mm**) |
| Flujo | Icono **o Enter** (modo apagado) → `pickLine` (hover resalta línea) → **clic en línea** → `pickDirection` + vista previa discontinua → **clic en planta** hacia el lado deseado → `maStlOffsetToolCommitParallelCopyMm` → sale del modo |
| Geometría | Dirección unitaria `u` en XZ; normal `n = (-u.z, u.x)`; signo ±1 según semiplano del clic dirección; `P1' = P1 + sign·offset·n`, `P2' = P2 + sign·offset·n`. **Miter interior (cadena cerrada / L / U / rectángulo):** en **cada** extremo con vecino no colineal y offset hacia el interior del ángulo (`maStlOffsetToolIsInwardOffsetAtCornerMm`), recorta en la intersección de las dos rectas offset (`maStlOffsetToolMiterPointAtCornerMm`); signo del vecino vía `maStlOffsetToolNeighborSideSignForMiterMm`. Si el vecino ya tiene copia offset confirmada (~`offsetMm`, paralela), se intersecta con su geometría real (`maStlOffsetToolFindCommittedOffsetOfNeighborMm`). Validación por extremo: el punto debe caer sobre la paralela offset (`maStlOffsetToolPointOnOffsetParallelLineMm`) y acortar el tramo (`maStlOffsetToolAcceptMiterEndpointMm`); sin vecino, offset exterior o recorte inválido ⇒ extremo paralelo sin recorte. Preview y commit usan la misma función. No modifica la línea vecina existente |
| Doble clic | En línea durante offset: edita distancia en `#ma-stl-offset-tool-hud-distance` (doble clic también selecciona línea y pasa a `pickDirection`) |
| HUD | `#ma-stl-offset-tool-hud` — instrucciones + fila distancia (`StlPreview_OffsetToolInstructionPickLine/PickDirection`, `StlPreview_OffsetToolHudDistanceAria`) |
| Preview | `maStlOffsetToolPreviewLine` — `Line2` discontinuo naranja durante `pickDirection` |
| Snap | Endpoints de la copia pasan por `maStlCommitUserPlanLineSegmentMm` (soldadura vértices ≤ 100 mm XZ) |
| Undo | Un snapshot por copia (`createUserFloorLine` vía commit) |
| Exclusión | Mutuamente excluyente con polilínea, pick rejilla/objeto y **borrar** |
| Cámara | Mismo lock que polilínea (`maStlLockOrbitForRulerAnchorPick`); Escape / segundo clic icono cancela |

### Barra superior Desing_2 — herramientas de dibujo y repetir último comando (Enter)

Parcial: `_Desing2TopToolBar.cshtml` (`#desing2-stl-top-toolbar`). Siempre visible, discreta, anclada **arriba-centro** del lienzo (mismo patrón CSS que la barra inferior: `desing2-stl-floating-toolbar`).

| ID | Rol |
|----|-----|
| `#ma-stl-tool-polyline` | Polilínea — `ri-route-line` |
| `#ma-stl-tool-offset` | Offset paralelo — `ri-shape-line` |
| `#ma-stl-tool-delete` | Borrar líneas usuario y objetos STL — `ri-delete-bin-line` |

| Elemento | Detalle |
|----------|---------|
| Variable | `maStlDesing2LastToolbarCommandId` — id del botón `#ma-stl-tool-*` activado por última vez; predeterminado `ma-stl-tool-polyline` |
| Registro | Clic en `#ma-stl-tool-polyline`, `#ma-stl-tool-offset`, `#ma-stl-tool-delete` → `maStlDesing2SetLastToolbarCommandId`. **No** registra `#ma-stl-tool-ortho-15` (modificador en barra inferior, no comando) |
| Enter global | `maStlDesing2ApplyWindowEnterToRepeatLastToolbarCommand` / `maStlWireDesingV2LineToolEnterActivateKeyListener` — con **modo borrar activo**, Enter/Espacio confirma borrado (`maStlDeleteToolApplyWindowKeydownConfirm`) si hay líneas **o** mallas STL seleccionadas; si no hay modo transitorio, repite `maStlDesing2LastToolbarCommandId`. Bloqueado durante `picking*` polilínea, edición cotas (`maStlIsUserFloorLineDimEditOverlayActive`), foco en `#ma-stl-line-tool-hud-distance` o inputs texto; en `picking2` la distancia tecleada / Enter en polilínea sigue usando `maStlLineToolApplyWindowKeydownToDistanceBuffer` |
| Activación | `maStlDesing2ActivateToolbarCommandById`: **polilínea** → `maStlStartPolylineToolModesToolbar`; **offset** → `maStlStartOffsetToolModesToolbar`; **delete** → `maStlStartDeleteToolModesToolbar`. Id legacy `ma-stl-tool-line` se redirige a polilínea |
| Borrar | Clic `#ma-stl-tool-delete` → crosshair + HUD; clic alterna línea o malla STL en selección (50 % opacity); **Enter/Espacio** confirma y sale; **Escape** cancela; segundo clic icono apaga modo. Registra último comando para Enter global |
| Tras segmento | Sin snap connect: encadena otro tramo. Con **snap connect en clic**: `maStlStopLineToolModesToolbar` (como Escape). Enter/distancia typed confirma tramo y termina. Último comando `#ma-stl-tool-polyline` |

### Barra inferior Desing_2 — deshacer, orto, submenú reglas

Parcial: `_Desing2BottomToolBar.cshtml` (`#desing2-stl-bottom-toolbar`). Siempre visible, discreta, anclada **abajo-derecha** del lienzo (sin hover ni pin; brújula arriba-izquierda, panel lateral izquierdo).

| ID | Rol |
|----|-----|
| `#ma-stl-tool-undo` | Deshacer — `ri-arrow-go-back-line`; disabled si `maStlDesing2UndoStack` vacío |
| `#ma-stl-tool-redo` | Rehacer — `ri-arrow-go-forward-line`; disabled si `maStlDesing2RedoStack` vacío |
| `#ma-stl-tool-ortho-15` | Orto 15° en planta (movido desde barra superior); **F8** |
| `#ma-stl-tool-wall-dim` | Cota espesor muro — `ri-pencil-ruler-2-line`; escaneo auto al activar |
| `#desing2-stl-rulers-flyout-toggle` | Botón padre submenú flyout — `ri-ruler-2-line`; abre panel `#desing2-stl-rulers-flyout-panel` hacia arriba; segundo clic o clic fuera cierra (`desing2-stl-viewer-toolbar-wiring.js`) |

**Submenú flyout reglas** (`#desing2-stl-rulers-flyout-panel`, grupo `#desing2-stl-rulers-flyout`):

| ID | Rol |
|----|-----|
| `#ma-stl-grid-toggle` | Rejilla infinita de fondo |
| `#ma-stl-ucs-rulers-toggle` | Reglas UCS en suelo (+X / +Z desde anclaje); movido desde barra de acciones superior |
| `#ma-stl-edge-rulers-toggle` | Reglas borde visor (+X arriba, +Z derecha) |
| `#ma-stl-ruler-anchor-pick-toggle` | Modo anclaje en cruce de rejilla — `ri-crosshair-2-line`; movido desde barra de acciones superior |
| `#ma-stl-ruler-anchor-object-pick-toggle` | Modo anclaje en punto de inserción STL — `ri-cube-line`; movido desde barra de acciones superior |

El botón padre muestra estado `has-active-children` cuando algún toggle del submenú está activo (`active` / `aria-pressed="true"`).

**Atajos undo/redo:** **Ctrl+Z** (deshacer), **Ctrl+Y** y **Ctrl+Shift+Z** (rehacer). No capturan teclas con foco en `input`/`textarea`/`select`/contenido editable.

### Herramienta cota muro (barra inferior `#ma-stl-tool-wall-dim`)

Cotas CAD estilo **construcción**: longitudes de tramo en contorno exterior, cota global si no duplica un tramo, y **espesor** entre pares paralelos (exterior ↔ interior tras offset).

| Elemento | Detalle |
|----------|---------|
| Botón | `#ma-stl-tool-wall-dim` — icono `ri-pencil-ruler-2-line` |
| Estado | `maStlWallDimToolActive` (`boolean`) |
| Flujo | Icono → escaneo automático → cotas visibles → segundo clic icono o **Escape** oculta |
| Longitudes tramo | Solo lados **oeste** y **sur** del contorno exterior (`MA_STL_WALL_DIM_SEGMENT_DIM_SIDES`); no se repiten norte/este (opuestos) |
| Sin redundancia | Si una cota tiene el **mismo valor** que otra (±5 mm), solo se muestra una; la **global superior** se omite si coincide con un tramo inferior |
| Espesor | Una cota por **espesor único** (±5 mm); pares enlazados por offset (`linkOffsetFromLineId`) |
| Offset mínimo | Línea de cota ≥ **1 m** del muro (`MA_STL_WALL_DIM_BASE_OFFSET_MM` = 1000 mm); cotas apiladas +1 m cada una (`MA_STL_WALL_DIM_STACK_STEP_MM`) |
| Visual | Grupo `maStlWallDimGroup` + overlay `#ma-stl-wall-dim-overlay` (metros, `maStlDesing2DimEditableMetersDisplayFromMm`); espesor en azul |
| Sin geometría | Toast `StlPreview_WallDimToolNoPairsToast`; clic en línea de muro reintenta detección |
| Órbita | **No** bloquea órbita |

Funciones clave: `maStlWallDimCollectAllPlacements`, `maStlWallDimFilterRedundantPlacements`, `maStlWallDimDetectWallPairsMm`, `maStlWallDimRefreshScanAndDraw`, `maStlStartWallDimToolModesToolbar`, `maStlStopWallDimToolModesToolbar`.

**Acciones registradas (snapshot):** crear segmento línea usuario (`createUserFloorLine`), copia offset paralela (`createUserFloorLine` vía offset), **borrar líneas usuario y/o mallas STL** (`deleteUserFloorLinesAndStlMeshes`), mover extremo P1/P2, mover segmento (asa midpoint en edición cotas), editar cotas (longitud/ΔX/ΔZ), conectar extremo tras snap (clic confirmación), refactor merge colineal (RMB corto), mover anclaje reglas (`rulerAnchor`).

**Extensión:** `window.maStlDesing2PushUndoAction({ label, undo: fn, redo: fn })` para comandos futuros sin tocar el stack interno.

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
| `offsetDefaultMm` | mm escena (= mm físicas) | Distancia por defecto de la herramienta offset; editable en pestaña Configuración (`#ma-stl-config-offset-default`, UI en metros). Default **300** (0,30 m). Getter: `maStlDesing2EnvOffsetDefaultMm()`. |

## Pestaña Configuración (panel lateral)

| ID | Rol |
|--------|-----|
| `#ma-stl-config-offset-default` | Input texto: distancia por defecto del offset (metros en UI, p. ej. `0,30` o `300 mm`). Commit en `change`/`blur` → `desing2EnvOffsetDefaultMm`. Persistencia en cookie `environment.offsetDefaultMm` al pulsar `#ma-stl-save-viewer-state`. |

## Persistencia en cookie (solo Desing_2)

Guardado **explícito** con el botón de barra `#ma-stl-save-viewer-state` (icono `ri-save-3-line`, tooltip i18n «Guardar vista»). No hay auto-guardado al salir ni al mover la cámara.

| Clave cookie | `ma_stl_desing2_viewer_state_global` (misma vista para cualquier oferta/diseño) |
| Contenido | `activeCamera` (`ortho`/`iso`), `cameraOrtho` / `cameraIso` (`position`, `up`, `zoom` si orto), `target` (órbita), `rulerAnchor` (`{x,y,z}`), `environment` (`gridSnapMm`, `rulerExtentCapM`, `offsetDefaultMm`), `toggles` (rejilla, cielo, sombra, fondo oscuro, cortes UI, ejes XYZ, reglas UCS suelo `ucsRulers`, reglas borde `edgeRulers`), sliders de corte X/Y |
| Restaurar | Cookie leída al arranque en `pendingDesing2Restore`. **Una vez** tras el **primer** `refitCamerasToObject` (STL cargado) o tras el arranque sin auto-carga STL. Orden en refit: frustum/rejilla → **omitir** `placeCamerasForModel` si hay restore pendiente o ya aplicado → `bindControls` → `maStlApplyDesing2ViewerStateFromCookie`. Refits posteriores (p. ej. `*2.stl`) **no** resetean cámara (`maStlDesing2StateRestored`). **`state.target` no se restaura** — pivote = `rulerAnchor`. |
| Lectura legado | Si no hay cookie global, se intenta la cookie antigua por `offerId`/`designId` o `ma_stl_desing2_viewer_state` |

Helpers módulo: `maStlDesing2ReadViewerStateFromCookie`, `maStlReadCookie`, `maStlWriteCookie`, `maStlDesing2BuildViewerStateSnapshot`, `maStlApplyDesing2ViewerStateFromCookie`, `maStlDesing2SaveViewerStateToCookie`, `maStlDesing2TryRestoreViewerStateFromCookie`. Arranque STL: evento `ma-stl-desing2-viewer-ready` (evita carrera módulo vs `#desing2-initial-stl-boot`). No aplica al visor de artículos maestro (sin `data-ma-stl-show-rulers-toggle`).
