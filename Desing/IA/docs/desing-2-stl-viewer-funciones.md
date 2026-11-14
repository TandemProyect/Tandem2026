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
| `onCanvasPointerMoveUserFloorLineHover(ev)` | Sin `picking*` ni modo pick rejilla/objeto: resalta trazo bajo cursor (pre-resaltado **cian** `#22d3ee`, 4 px — ver *Resaltado de selección*) y muestra **sólo cota longitud** (`maStlUpdateUserFloorLineDimHud(true)` — guías longitud + `#ma-stl-floor-dim-readout`; **sin** ΔX/ΔZ ni asas). Si hay `maStlUserFloorLineSelected`, prevalece sobre hover y mantiene las **tres cotas** + asas aunque el ratón salga del trazo |
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
| `maStlStartDeleteToolModesToolbar()` / `maStlStopDeleteToolModesToolbar()` / `maStlSyncDeleteToolHud()` / `maStlDeleteToolToggleLineSelection()` / `maStlDeleteToolAddLineSelection()` / `maStlDeleteToolAddMeshSelection()` / `maStlDeleteToolToggleMeshSelection()` / `maStlDeleteToolCommitSelection()` / `maStlDeleteToolApplyWindowKeydownConfirm()` / `maStlDeleteToolWireWindowSelection()` / `maStlDeleteToolUnwireWindowSelection()` / `maStlApplyUserFloorLineDeletePickMaterial()` / `maStlDeleteToolApplyStlMeshDeletePickMaterial()` / `maStlIsDeletableStlMesh()` / `maStlDetachStlMeshToGraveyard()` / `maStlRestoreStlMeshFromGraveyard()` / `maStlDesing2ApplyStlMeshSnapshotFromUuids()` | Modo borrar: `maStlDeleteToolActive`; multi-selección mixta `maStlDeleteToolSelectedLines` (`Set<Line2>`) + `maStlDeleteToolSelectedMeshes` (`Set<Mesh>`); **clic** alterna línea/malla (seleccionado = **verde flúor** `#39ff14`, ver *Resaltado de selección*); **marquee** vía API genérica (`maStlDeleteToolWireWindowSelection`) en lienzo vacío — unión aditiva; hover no seleccionado = cian; **Enter/Espacio/Supr** confirma (`deleteUserFloorLinesAndStlMeshes` undo snapshot) y sale; **Escape** cancela; HUD `StlPreview_DeleteToolInstruction`. Pick clic: línea prioriza sobre malla |
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
| `maStlClearStlPickHoverHighlight()` / `maStlApplyStlPickHoverHighlight(mesh)` | Hover cian temporal (modo objeto; `MA_STL_SELECTION_STYLE.meshHover*`) |
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
| `updateClipPlanes()` | Planos recorte Three.js. Los planos `clipPlaneY`/`clipPlaneX` nacen sin recorte (`MA_STL_CLIP_NO_CUT_MM` = 1e9, normales −Y/−X); con el deslizador a cero (f = 0) el plano queda en esa distancia y no depende de `clipBounds`. Antes nacían con `THREE.Plane()` por defecto (normal +X, constante 0), que descarta x < 0, y con f = 0 cortaban en el máximo de `clipBounds`, que podía no incluir lo último insertado: desaparecían paneles ATK-60 (medio muro curvo, algunos pilares). `maStlCurvedWallSyncFrame` llama a `updateClipPlanes()` tras ampliar `clipBounds` con el anillo |
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
| **Hover** (`onCanvasPointerMoveUserFloorLineHover`) | Sí (cian `#22d3ee`) | **Sólo longitud** (`lengthOnly=true`; sin ΔX/ΔZ) | No | No |
| **Clic simple** (`onCanvasClickUserFloorLineSelect`, ~260 ms) | Sí (selección, verde flúor `#39ff14` + pulso) | **Longitud + ΔX + ΔZ** (`lengthOnly=false`) | Sí (extremos libres) | No |
| **Doble clic** (`onCanvasDblClickUserFloorLineDimension`) | Sí (selección) | **Longitud + ΔX + ΔZ** | Sí (durante edición según reglas asas) | Sí (`all` o cota bajo cursor) |
| **Ratón fuera del trazo** con selección activa | Base en otras líneas | **Tres cotas** de la línea seleccionada | Sí | No |
| **Arrastre asa P1/P2** | — | **Sólo longitud** (transitorio) | Sí (la arrastrada) | No |

Deselección: clic en lienzo vacío, Escape (`maStlClearUserFloorLineSelection`). Suprimido mientras hay `picking*` polilínea, modo offset, **modo borrar**, pick anclaje rejilla/objeto o edición cotas activa.

### Resaltado de selección (hover vs seleccionado)

Configuración única: constante de módulo **`MA_STL_SELECTION_STYLE`** (cabecera de `master-article-details-stl-viewer.js`). Cambiar ahí colores, grosores y pulso.

| Estado | Líneas usuario / caras de muro (`Line2`) | Mallas STL |
|--------|-------------------------------------------|------------|
| Base | Naranja `#ff6600`, 3 px (`maStlApplyUserFloorLineBaseMaterial`); eje de muro gris discontinuo | Material original |
| Hover (pre-resaltado) | **Cian** `hoverHex` `#22d3ee`, `hoverLinewidthPx` 4 px (`maStlApplyUserFloorLineHoverBrightMaterial`) | Emissive `meshHoverEmissiveHex` + tinte `meshHoverColorHex` |
| Seleccionado / objeto del comando | **Verde flúor** `selectedHex` `#39ff14`, `selectedLinewidthPx` 6 px (eje: `selectedAxisLinewidthPx`, sigue discontinuo), opacidad 1 y **pulso suave** hacia `selectedPulseHex` (`maStlApplyUserFloorLineSelectedMaterial`) | Tinte `meshSelectedColorLerp` + emissive `meshSelectedEmissiveHex` (`maStlApplyStlMeshSelectedTint`) |

- Lo usan: selección general por clic, **borrar**, **offset** (línea/polilínea origen; el hover del grupo es cian), **copiar / mover / estirar** (selección de la herramienta copiar), **muro 3D**, y el hover de **recortar / alargar** (cian).
- `maStlUserFloorLineIsMarkedSelected(line)`: la selección manda sobre el hover — `maStlApplyUserFloorLineHoverBrightMaterialForLine` y el bucle de re-aplicación de materiales mantienen el verde en líneas seleccionadas.
- Pulso: `maStlSelectionPulseTick()` en `tick()`; recorre `maStlUserLinesGroup` y solo modula el color de materiales con `userData.maStlSelPulse` (sin asignaciones por frame). `selectedPulseAmount: 0` desactiva el pulso.
- Restauración: base/hover ponen `maStlSelPulse = false` y reponen color, opacidad y grosor; mallas vuelven por snapshot de cada herramienta (deseleccionar, Escape, fin de comando, undo). No se crean geometrías ni materiales extra.

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
| Visual línea | `maStlApplyUserFloorLineDeletePickMaterial` → `maStlApplyUserFloorLineSelectedMaterial` (verde flúor `#39ff14`, 6 px, pulso); hover no seleccionado = cian `#22d3ee` |
| Visual malla | `maStlDeleteToolApplyStlMeshDeletePickMaterial` → `maStlApplyStlMeshSelectedTint` (tinte + emissive verde flúor sobre el color original del snapshot; opacidad original); hover no seleccionado = emissive cian (mismo criterio pick objeto) |
| HUD | `#ma-stl-delete-tool-hud` — `StlPreview_DeleteToolInstruction` («clic o arrastre ventana/cruce… Enter/Espacio… Esc cancelar») |
| Undo | `maStlDesing2PushEditSnapshotUndo('deleteUserFloorLinesAndStlMeshes', before, after)` — snapshot incluye `stlMeshUuids[]`; mallas borradas van a `maStlStlMeshGraveyard` hasta undo o nueva carga STL |
| Exclusión | Mutuamente excluyente con polilínea, offset y pick anclaje rejilla/objeto |
| Cámara | Mismo lock que polilínea/offset; refit tras borrar mallas si queda al menos una; Escape / segundo clic icono cancela |

Copia paralela de una línea usuario en planta XZ a distancia configurable.

| Elemento | Detalle |
|----------|---------|
| Botón | `#ma-stl-tool-offset` — icono `ri-shape-line` |
| Estado | `maStlOffsetToolState`: `null` \| `pickLine` \| `pickDirection` |
| Distancia | `maStlOffsetToolDistanceMm` — al iniciar = `desing2EnvOffsetDefaultMm` (Configuración «Offset por defecto» / cookie / estado guardado; default **300 mm**). Cambiarla (pill inferior o HUD) pasa por `maStlOffsetToolSetDistanceMm()`: actualiza la distancia activa y la vista previa, y la recuerda como offset por defecto de la sesión (sincroniza `#ma-stl-config-offset-default`; se persiste con **Aceptar** / **Guardar vista**) |
| Flujo | Icono **o Enter** (modo apagado) → `pickLine` (hover resalta línea) → **clic en línea** → `pickDirection` + vista previa discontinua → **clic en planta** hacia el lado deseado → `maStlOffsetToolCommitParallelCopyMm` → sale del modo |
| Geometría | Dirección unitaria `u` en XZ; normal `n = (-u.z, u.x)`; signo ±1 según semiplano del clic dirección; `P1' = P1 + sign·offset·n`, `P2' = P2 + sign·offset·n`. **Miter interior (cadena cerrada / L / U / rectángulo):** en **cada** extremo con vecino no colineal y offset hacia el interior del ángulo (`maStlOffsetToolIsInwardOffsetAtCornerMm`), recorta en la intersección de las dos rectas offset (`maStlOffsetToolMiterPointAtCornerMm`); signo del vecino vía `maStlOffsetToolNeighborSideSignForMiterMm`. Si el vecino ya tiene copia offset confirmada (~`offsetMm`, paralela), se intersecta con su geometría real (`maStlOffsetToolFindCommittedOffsetOfNeighborMm`). Validación por extremo: el punto debe caer sobre la paralela offset (`maStlOffsetToolPointOnOffsetParallelLineMm`) y acortar el tramo (`maStlOffsetToolAcceptMiterEndpointMm`); sin vecino, offset exterior o recorte inválido ⇒ extremo paralelo sin recorte. Preview y commit usan la misma función. No modifica la línea vecina existente |
| Doble clic | En línea durante offset: edita distancia en `#ma-stl-offset-tool-hud-distance` (doble clic también selecciona línea y pasa a `pickDirection`) |
| HUD | `#ma-stl-offset-tool-hud` — instrucciones + fila distancia (`StlPreview_OffsetToolInstructionPickLine/PickDirection`, `StlPreview_OffsetToolHudDistanceAria`) |
| Barra inferior | `#ma-stl-offset-status-hud` (pill junto a `(F8)` orto, mismo estilo; `left` según ancho del pill F8) con `#ma-stl-offset-status-distance` (`data-decimal-input`, metros, unidad `m`). Visible **sólo** con offset activo (`maStlSyncOffsetStatusHud()`, llamado desde `maStlSyncOffsetToolHud()` y al cambiar F8). Al escribir aplica en vivo si es válido (> 0; coma o punto; también `300 mm`); **Enter** aplica, reescribe con punto y devuelve el foco al lienzo; **blur** aplica (inválido ⇒ toast `StlPreview_OffsetToolDistanceInvalidToast` y valor anterior); **Esc** descarta lo tecleado y cancela el offset (como AutoCAD). `keydown` no se propaga ⇒ los atajos del visor (Enter repetir, Supr, Ctrl+Z, letras) no actúan mientras se escribe; F8 sigue funcionando. Textos `StlPreview_OffsetStatusDistanceLabel` / `StlPreview_OffsetStatusDistanceAria`. Listeners registrados una vez al arrancar el visor |
| Preview | `maStlOffsetToolPreviewLine` — `Line2` discontinuo naranja durante `pickDirection` |
| Snap | Endpoints de la copia pasan por `maStlCommitUserPlanLineSegmentMm` (soldadura vértices ≤ 100 mm XZ) |
| Undo | Un snapshot por copia (`createUserFloorLine` vía commit) |
| Exclusión | Mutuamente excluyente con polilínea, pick rejilla/objeto y **borrar** |

### Herramientas recortar / alargar (barra superior `#ma-stl-tool-trim` / `#ma-stl-tool-extend`)

Equivalentes a RECORTA / ALARGA de AutoCAD en **modo Rápido**: sin paso de selección de aristas; editan líneas de planta simples y **tramos de muro 2D** (eje + 2 caras). Los pilares no se editan.

| Elemento | Detalle |
|----------|---------|
| Estado | `maStlLineEditToolKind`: `null` \| `trim` \| `extend`; `maStlLineEditToolState`: `null` \| `pick` |
| Aristas | `maStlLineEditToolCollectEdgeSegments()`: todas las líneas seleccionables de `maStlUserLinesGroup` (líneas, caras de muro `wallRole: 'face'`) + los 4 lados de la huella de cada pilar (`wallRole: 'pillar'`) + la **testa** de cada extremo libre de muro (`maStlLineEditToolWallFreeEndCapsMm`: de cara a cara). El eje discontinuo del muro (`axis`) **no** cuenta. **Muro curvo** (`maStlLineEditToolCurvedWallHitParams`): cruce real con los círculos exterior e interior dentro del barrido de la porción (desde el ángulo de inicio, de +X hacia +Z) y, en una porción, con las dos tapas radiales. Intersección proyectada en planta XZ (ignora Y), aristas como segmentos finitos, tolerancia 1 mm |
| Pick | `maStlLineEditToolPickTargetAtClient()` + `maStlLineEditToolChoosePickTarget()`: candidatos a ≤ `MA_STL_USER_FLOOR_LINE_SCREEN_PICK_PX` + 4 px. Sin línea simple cerca → cara de muro → huella de un muro (`maStlLineEditToolPickWallFootprintAtClient`, eje ± medio espesor). Línea simple **justo bajo el cursor** (≤ `MA_STL_LINE_EDIT_TOOL_ON_LINE_PX` = 3 px) ⇒ gana la línea, salvo que la cara del muro también esté a ≤ 3 px, más cerca, **y** el clic caiga dentro de la huella (así, clicar el pie de una línea que llega a la cara desde fuera del muro recorta la línea, no el muro). Línea más lejos ⇒ gana el **muro** si su cara está más cerca que la línea o si el clic cae dentro de su huella. Un muro se devuelve como su **eje** (`maStlLineEditToolWallAxisOf`); el hover resalta sus 2 caras. Si solo hay pilares cerca ⇒ toast `unsupported` |
| Flujo | Icono **o Enter** (último comando) → `pick` → clic en tramo → recorta/alarga; sigue activo → **Enter/Espacio/Esc** termina |
| Recortar | `maStlLineEditToolTrimLineAtClient`: quita el tramo entre las intersecciones que rodean el clic (o hasta el extremo). Tramo central ⇒ la línea se parte en dos (`maStlCommitUserPlanLineSegmentMm` con `skipWeld`/`skipCollinearMerge`). **Sin intersección ⇒ borra la línea** (`maStlCommitUserFloorLinesAndStlMeshesDelete`). **Polilínea** (`polylineGroupId`): el trozo nuevo entra en la polilínea y `maStlLineEditToolRegroupPolylineMm` reparte el grupo en trozos conexos por sus extremos: el trozo con la línea editada (o el mayor, si se borró) se queda el grupo y cada parte separada recibe uno nuevo (también al borrar un tramo entero). Guarda WallConnections (`trim-tool-line` / `extend-tool-line`) |
| Alargar | `maStlLineEditToolExtendLineAtClient`: extremo más cercano al clic (`t ≥ 0,5` ⇒ P2) hasta la primera arista que corta su prolongación; sin arista ⇒ toast |
| Muro: recortar | `maStlLineEditToolTrimWallAtClient`: trabaja sobre el **eje** del tramo. Límites (`maStlLineEditToolWallCutParams`): todo lo que **llega a la huella** del muro (`maStlLineEditToolEdgeVsWallBandMm`: toca el eje o una de las dos caras, con holgura `maStlLineEditToolWallTouchTolMm` = mín(`MA_STL_LINE_EDIT_TOOL_WALL_TOUCH_TOL_MM` 30 mm, ½ medio espesor) en los extremos de la arista, para líneas que visualmente llegan a la cara pero se quedan unos mm cortas): **ejes de otros tramos** (un muro que llega en **T** cuenta como límite tanto si su eje acaba en el eje como en la cara o algo antes), líneas simples, **caras huérfanas** (cara cuyo eje ya no existe) y pilares. El corte es siempre el cruce con el **eje** del muro recortado; en una T es el eje del muro que llega (el resto queda en L con él y el refactor de uniones lo resuelve). Las caras de muros con eje no cuentan (su eje ya lo hace). En una T ya partida por `finalizeWallAxes` (dos tramos en el nodo), recortar a un lado borra el tramo de ese lado entero, que es lo mismo que cortar en el eje de la T. Si no hay corte pero una línea o pilar entra en la huella dentro del tramo (esquina pisada), **no se borra** (toast «sin cambios»). Tramo central ⇒ el eje se acorta y se crea otro tramo (`maStlLineEditToolAddWallPieceMm`: mismo `wallGroupId`/`wallId`, espesor y propiedades escalares copiadas; id nuevo). Extremo ⇒ se acorta. El trozo nuevo conserva `wallGroupId`/`wallId` (son el grupo del diseño; `SaveDesignWalls` → `DesignWallRepository.ReplaceWalls` casa cada fila por `WallDbId` y, sin él, por geometría, así que el trozo entra como fila nueva sin chocar) y hereda los atributos del extremo final (`_Tape_2`, `_CornerTypeP2`, `_ChekSolapeEnd`); su inicio y el nuevo final del tramo original quedan como testa libre (`maStlLineEditToolTakeWallEndAttrs`). Sin cortes (tramo entre uniones o muro suelto) ⇒ se borra ese tramo (eje + caras; trabajadores asociados como en Borrar). Después `maStlWallEditFinalize().finalizeWallAxes()` sobre el tramo y los vecinos de los extremos afectados: caras, ingletes L/T/X (los trozos quedan a ras de la cara del muro que cruza) y guardado de WallConnections |
| Muro: abrir una cara (T de líneas) | Recortar el muro **entre dos líneas que sólo llegan a la misma cara** (no tocan el eje ni la cara opuesta, ni son ejes de muro; `maStlLineEditToolOneFaceGapMm` con el detalle `hits` de `maStlLineEditToolWallCutParams`) **no parte el muro**: abre sólo esa cara entre los dos puntos de contacto (`maStlWall2dToolAddFaceGapMm`); el eje y la cara opuesta siguen enteros, el encofrado no cambia y no se quita. Si el tramo lo acota un eje de muro, una línea que cruza el eje o líneas en caras distintas, se parte como siempre; a izquierda/derecha de la T también. **Modelo:** la apertura vive en la cara como `faceGapsMm` ([{a,b}] en planta, mm). En memoria la cara sigue siendo una sola línea de p1 a p2 (uniones, ingletes, muro 3D, WallConnections y encofrado la ven entera) y `maStlSetUserFloorLineGeometryMm` la dibuja a tramos (`maStlWallFaceVisiblePiecesMm`, `LineSegmentsGeometry`). Si cambia el nº de tramos, `maStlLine2EnsureSegmentCount` le pone una geometría nueva: Three fija `geometry._maxInstanceCount` la primera vez que la dibuja y, reutilizando la misma, sólo se veía el primer tramo (la cara quedaba cortada desde la primera línea hasta el final). Rehacer caras no la cierra (está en coordenadas de planta y se recorta a la cara); mover el muro la mueve (`maStlWall2dToolTranslateFaceGapsMm`); partir el eje (T real, recortar en medio) y unir ejes colineales la pasan al tramo nuevo (`maStlWall2dToolInheritFaceGapsMm`); duplicar no la copia. Las líneas de corte o límite usan sólo los tramos visibles, así que alargar una línea hacia la abertura llega hasta la cara opuesta. **Persistencia:** `maStlDesing2SerializeEditSnapshot` (guardar en BD y deshacer) escribe la cara como **una fila de cara por tramo visible** (mismo `linkOffsetFromLineId` y lado; ids extra estables en `faceGapPieceIds`, sin `WallDbId`), sin columnas nuevas en `TSql_DesignWall`; los plugins CAD ven el hueco. `maStlDesing2ApplyEditSnapshot` (cargar / deshacer) llama a `maStlWall2dToolMergeFacePiecesMm`, que junta las filas del mismo eje y lado en la de id menor y recupera los huecos. Para cerrar una apertura: deshacer |
| Muro: alargar | `maStlLineEditToolExtendWallAtClient`: extremo **libre** más cercano al clic hasta el siguiente eje de muro, línea o pilar (mismos límites que recortar: `maStlLineEditToolWallCutParams`); al llegar a otro muro el post-proceso crea la unión en T. Un extremo ya unido a otro muro no se alarga (toast) |
| Testa a ras de línea | `maStlWall2dToolApplyFreeEndLineCutsMm()` (al final de `maStlWall2dToolRefactorAllWallJunctionsMm`): un extremo libre de eje que cae (≤ 2 mm) sobre una línea simple que llega a la huella, con más de `MA_STL_WALL_FREE_END_LINE_CUT_MIN_DEG` (10°) entre ambos, corta cada cara en su cruce con la línea en vez de la testa recta. Línea perpendicular: sin cambios. El muro 3D sale de las caras y la sigue; el encofrado (servidor) usa el eje y no cambia. Si luego se borra o mueve la línea, la testa vuelve a ser recta la próxima vez que se rehagan las caras de ese tramo |
| Encofrado | Si al recortar/alargar un muro había encofrado (`maStlDesing2HasFormworkPanels`), se quita (`maStlDesing2ClearAtk60Visuals`) y, al terminar el comando (Enter/Espacio/Esc), se re-encofra pulsando `#ma-stl-mode-formwork`, como al modificar el muro curvo; no si se pasa a otra herramienta |
| Undo | Un snapshot por operación (`trimUserFloorLine` / `extendUserFloorLine` / `trimUserFloorWall` / `extendUserFloorWall` / `deleteUserFloorLinesAndStlMeshes`) |
| Escape | `maStlStopLineEditToolModesToolbar()` (también desde `maStlDesing2CancelTransientToolsEscape` y `maStlCancelAllViewerInteractionModes`): limpia hover, HUD, cursor y bloqueo de órbita; seguro si no está activa |
| HUD | `#ma-stl-trim-tool-hud` / `#ma-stl-extend-tool-hud` (`StlPreview_TrimToolInstruction`, `StlPreview_ExtendToolInstruction`, `*NoChangeToast`, `StlPreview_LineEditToolUnsupportedToast`) |
| Teclado | `maStlLineEditToolApplyWindowKeydownConfirm()` en `_maStlDesingV2LineToolEnterActivateKeyHandler` |
| Cámara | Mismo lock que polilínea (`maStlLockOrbitForRulerAnchorPick`); Escape / segundo clic icono cancela |

### Líneas paralelas → muro (13/11/2026)

Dos **líneas simples** (sin `wallRole`, de línea o de polilínea) **paralelas** (±0,5°), **solapadas** (solape ≥ 100 mm y ≥ la mitad de la más corta) y separadas **entre 50 mm y 1,20 m** se convierten en un **muro real** (eje + caras, como Importar DWG): eje en la línea media, espesor = separación, caras = las dos líneas. Es lo que hacía la detección del servidor (`ProcesarLineasZwcad` / `LCornerDetector`) cuando el 3D salía de las líneas; dejó de verse en diseños mixtos desde `77313a7` (07/07, «clean code»: si hay caras, solo se envían caras) y `9bbc344` (11/07, «Esquina en T»: si hay muros con eje, se ignora la detección).

| Paso | Función / detalle |
|------|-------------------|
| Emparejar (puro) | `maStlLinePairWallFindPairsMm(segs, min, max)`: cada línea en una sola pareja, primero las más cercanas; no empareja si hay otra paralela en medio |
| Plan (puro) | `maStlLinePairWallPlanMm`: un extremo que llega a la huella de otro muro (o de otra pareja) se prolonga hasta su **eje** (`maStlLinePairWallExtendEndMm`) para que el refactor haga la **T / L**; los sobrantes de una línea más larga fuera de la huella se conservan como línea; las tapas y restos dentro de la huella se quitan |
| Convertir | `maStlDesing2ConvertParallelLinePairsToWallsMm({ onlyLines?, skipUndo?, reason })`: borra las líneas, `maStlWall2dToolCommitWallSegmentMm(a, b, true, halfT, true)` por muro (grupo `maStlTakeDefaultWallGroupId`), luego partir cruces / refactor uniones / soldar (igual que DWG). Altura = `_DataHeight` del muro al que llega; si no, Configuración. Quita las aperturas `faceGapsMm` que quedan dentro del muro nuevo (la T ya abre la cara). Un paso de deshacer `linesToWall`; guarda `WallConnections` |
| Cuándo | Al generar **3D** (`maStlDesing2ApplyWall3dModelMode` → `maStlDesing2PersistLinePairWalls`, que además guarda muros), al **encofrar** (`desing2-stl-viewer-toolbar-wiring.js`, antes de fusionar ejes colineales) y tras un **desfase** (`maStlOffsetToolCommitParallelCopyMm`: original + copia a ≤ 1,20 m, en el mismo paso de deshacer que la copia; polilíneas: cada tramo con su copia) |
| No se tocan | Caras, ejes, pilares, muros curvos, líneas especiales, líneas que no solapan o fuera de rango |

Límites: una habitación dibujada con líneas de 1,20 m o menos de ancho también se convierte en muro (es la regla). Muro 3D por selección (herramienta antigua) no convierte. Harness: `/workspace/tr/pw/h6.js` (14 casos).

### Asistente IA en la edición de muro (fase 1: interfaz · fase 2: material del muro, 13/11/2026)

> Resumen completo del asistente (arquitectura, intérprete, órdenes, servidor, voz, persistencia, estudio del muro especial, preguntas abiertas, historial y cómo probar): [`ia-desing.md`](ia-desing.md).

Pestaña **«Asistente IA»** con chat (burbujas: usuario a la derecha en azul con texto blanco, asistente a la izquierda) y caja multilínea abajo (Enter envía, Mayús+Enter salto de línea; botón **Enviar**). Placeholder: «Ej.: dame el material de encofrado de este muro». Mensaje inicial: solo un **saludo según la hora local del navegador** (`maStlWallAiGreeting`): «Buenos días» 06:00–13:59 · «Buenas tardes» 14:00–20:59 · «Buenas noches» 21:00–05:59 (`MA_STL_WALL_AI_TEXT.intro` vacío; el curvo ya no lee `data-msg-ai-intro`). La conversación es **por muro y en memoria**: se conserva mientras el popup sigue abierto con el mismo muro y se vacía al cerrarlo o al abrirlo con otro muro.

| Pieza | Dónde / función |
|-------|-----------------|
| Chat (común) | `Scripts/MasterArticles/wall-ai-assistant.js`: `mountWallAiChat(container, { getContext, texts, height, onActions })` → `{ reset(key), ensureContext(key), focus(), send(), history(), input, sendBtn, log }`; textos por defecto `MA_STL_WALL_AI_TEXT` |
| **Gancho** | `maStlWallAiHandleMessage(wallCtx, text, history)` → `{ reply, actions: [], table?, buttons? }` (o Promise). Desde la fase 2 es el intérprete local (ver abajo). Se sustituye asignando `window.maStlWallAiHandleMessage`; las `actions` se pasan a `onActions` (muro recto: `options.onAiActions` del formulario). `table` se pinta como tabla con pie de totales y botón **Copiar**; `buttons` como botones de acción (`run()` devuelve otra respuesta) |
| Muro recto | Popup flotante «Modificar muro» de `wall-edit-form.js` (`mountWallEditForm`): quinta pestaña tras Muro / Configurar Tapes / Configurar Puntales / Acotar. API: `api.ai`, `api.resetAi(wallKey)`, `api.showTab('ai')`, `refs.aiPane/aiInput/aiSendBtn/aiLog`. En el visor: `maStlWallInspectOpenPopup` → `resetAi(clave del muro)`, `maStlWallInspectClosePopup` → `resetAi(null)`, contexto `maStlWallAiBuildStraightContext()` (`kind 'straight'`, `wallKey(s)`, `wallId`, `wallGroupId`, `lengthM`, `thicknessM`, `heightM`, `tapeIni/End`, `panel750`, `verticalPanels`, `ud`, `axisUd`). La página del plugin de AutoCAD (`WallForm.cshtml`) usa el mismo formulario; su contexto sale del modelo recibido |
| Muro curvo | Modal `#ma-stl-curved-wall-modal`: tercera pestaña `#ma-stl-curved-wall-tab-ai` / `#ma-stl-curved-wall-pane-ai` creada por JS (`maStlCurvedWallEnsureAiTab`, al abrir el formulario). Contexto `maStlCurvedWallAiContext()` (`kind 'curved'` o `'curved-new'`, Ø exterior, espesor, altura, modulación, ménsulas/puntales). Se vacía en `hidden.bs.modal` |
| Teclado | Las teclas del chat no llegan a los atajos del visor (Enter repetir comando, Ctrl+Z, Supr…): `stopPropagation` salvo Escape |
| Textos | Como el resto del formulario recto, en el JS (sin .resx). El curvo admite `data-msg-ai-tab` y `data-msg-ai-placeholder` en el modal para pasarlos a recursos más adelante (eso sí requerirá recompilar) |

Capturas fase 1: `/workspace/ia/asistente-ia-muro-recto.png`, `/workspace/ia/asistente-ia-muro-curvo.png`.

#### Modo compacto (14/11/2026): ver el modelo mientras trabaja la IA

Al elegir la pestaña **«Asistente IA»** (recto y curvo), el formulario pasa a un **panel compacto anclado a la derecha del visor** (`#ma-stl-viewer-shell` recortado a la ventana): **450 × 800 px** con 12 px de margen; si la ventana es más baja, el alto disponible (la conversación hace scroll dentro; la entrada siempre visible). **Sin backdrop ni bloqueo**: el visor 2D/3D sigue recibiendo clics, arrastres y rueda, y los clics fuera **no cierran** el panel. Solo se ve la conversación y la entrada (se ocultan las pestañas y Aceptar/Cancelar). Cabecera: «Asistente IA – Muro 12» / «AI Assistant – Wall 12» (curvo: «Muro curvo …») y un icono **Volver al formulario / Back to form** (dibujo de Remix `ri-arrow-go-back-line`, en SVG) que restaura el formulario con su tamaño, posición y pestaña anteriores. La conversación **se conserva** al cambiar de modo; cerrar (×, Esc en el curvo) sigue funcionando y la próxima apertura es el formulario normal.

| Pieza | Dónde / función |
|-------|-----------------|
| Común | `wall-ai-assistant.js`: `MA_STL_WALL_AI_COMPACT` (450, 800, margen 12), `maStlWallAiCompactArea()`, `maStlWallAiCompactRect(area)`, `maStlWallAiCompactTitle(ctx, T)`, `maStlWallAiCompactBackLabel(lang)`, `maStlWallAiCompactBackButton({ color, onClick })` |
| Muro recto | `wall-edit-form.js` (solo `floating`; la página del plugin no cambia): `showTab('ai')` → `enterAiCompact(pestaña anterior)`; API `isAiCompact()`, `exitAiCompact(showPrev)`, `refreshAiCompact()`, `refs.aiBackBtn`. En compacto la cabecera no se arrastra y se recoloca al redimensionar. Visor: `maStlWallInspectClosePopup` → `exitAiCompact(false)`; `maStlWallInspectOpenPopup` con el panel compacto abierto (otro muro) no lo mueve: `refreshAiCompact()` (título del muro nuevo; la conversación es la del muro, como antes) |
| Muro curvo | `maStlWallAiBootstrapCompact(modalEl, { chat, aiTabBtn, getTitle })` desde `maStlCurvedWallEnsureAiTab` (`maStlCurvedWallAiCompact`). Entra en `shown.bs.tab` de la pestaña IA (`relatedTarget` = pestaña anterior). Compacto: `.modal` con `pointer-events: none` (el diálogo sí recibe), backdrop oculto, trampa de foco de Bootstrap desactivada (`_focustrap.deactivate()`), sin `modal-dialog-centered`, cabecera con el título y el icono antes de la ×, pestañas y pie ocultos. `exit(true)` restaura todo y muestra la pestaña anterior; `hidden.bs.modal` → `exit(false)`. Abrir otro muro curvo con el panel compacto abierto se queda en la conversación (`refreshTitle()`) |

Imports `?v=20261114a` (`wall-edit-form.js`, `wall-ai-assistant.js`; `WallForm.cshtml`). El visor se versiona por fecha del archivo. Pruebas: `/workspace/ia/p8/test.mjs` (funciones + `harness-p8.html` en Chrome headless). Capturas: `/workspace/ia/p8-compacto-recto.png`, `p8-vuelta-formulario-recto.png`, `p8-compacto-curvo.png`, `p8-vuelta-formulario-curvo.png`, `p8-compacto-recto-en.png`, `p8-compacto-ventana-baja.png`.

#### Micrófono: dictar órdenes al Asistente IA (14/11/2026)

Botón de **micrófono** junto a la entrada del chat (entre la caja y **Enviar**), en el formulario normal (página del plugin), en el modo compacto y en el muro curvo. Usa la **Web Speech API** del navegador (`SpeechRecognition` / `webkitSpeechRecognition`). Sin palabra de activación, sin base de datos (las respuestas en voz alta van aparte: ver «Respuestas habladas»).

- **Encender / apagar**: pulsando el icono (Remix `ri-mic-line`; mientras escucha `ri-mic-fill`, botón gris 70% `#4d4d4d` con icono blanco y pulso `rgba(77,77,77,…)`) o con la tecla **Alt sola** (pulsar y soltar Alt sin otra tecla) con la pestaña IA visible, **también con el foco en la entrada del chat**. Alt+Tab, Alt+F4, Alt+letra, Mayús+Alt y **AltGr** (`AltGraph` o Ctrl+Alt) no hacen nada: se arma en el `keydown` de Alt, se desarma con cualquier otra tecla, clic o pérdida de foco y actúa en el `keyup`; ahí `preventDefault` + `stopPropagation` (evita que el navegador active la barra de menú). La tecla Espacio ya **no** se usa (escribía espacios). Apagar a mano quita de la caja lo que se estaba dictando a medias (no envía nada a medias); lo ya enviado queda enviado.
- **Mientras habla**: franja gris (fondo `#f2f2f2`, borde `#c4c4c4`, texto, punto y barras `#4d4d4d`) **«Escuchando… habla ahora» / «Listening… speak now»** encima de la entrada, con barras que siguen el **volumen real** del micrófono (`getUserMedia` + `AudioContext`/`AnalyserNode`); si no hay acceso al audio, barras con animación fija. El texto **parcial** se ve en la caja (`interimResults`).
- **Modo continuo**: el micrófono **no se apaga al terminar una orden**. Cada resultado final se envía solo por el **mismo camino que Enviar** (`send()`: intérprete, «¿Querías decir…?»; decir «sí»/«no» contesta a la confirmación) y sigue escuchando (`continuous = true`). Chrome corta la sesión tras un silencio o ~60 s: en `onend` se reinicia solo mientras siga encendido (la franja sigue visible y el medidor de volumen no se reabre). Sin duplicados: cada resultado final se envía una vez (índice de resultados ya enviados por sesión y misma frase en < 1,5 s se ignora). Si la IA está contestando, las órdenes dictadas esperan en cola y salen en orden. Si ya había texto escrito al encender, la primera orden lo lleva delante. Enviar a mano mientras escucha manda lo escrito y el micrófono sigue.
- **Idioma**: `es-ES` por defecto; `en-US` si el último mensaje del usuario se detecta en inglés (`maStlWallAiDetectLanguage`) o, si no se distingue, si la interfaz está en inglés (`maStlWallAiVoiceLang`). Los avisos van en ese idioma.
- **Solo se apaga** con el icono, con Alt, al cambiar de pestaña fuera de IA, al **volver del compacto al formulario** (se muestra la pestaña anterior, la IA deja de verse), al cambiar de muro (`reset`) y al cerrar el formulario (o por un error grave). Al apagar se liberan el micrófono (`track.stop()`) y el `AudioContext`, y no se reinicia.
- **Errores** (`maStlWallAiVoiceErrorPolicy`; avisos breves en el chat, sin entrar en el historial del intérprete): `not-allowed` / `service-not-allowed` (falta permiso: candado de la barra de direcciones), `audio-capture` (sin micrófono) y otros → aviso y **se apaga del todo**; `no-speech` y `aborted` → se reinicia **en silencio**; `network` → reintenta con espera (1 s, 2 s) y tras **3 fallos seguidos** avisa «…no responde. Micrófono apagado.» y se apaga. Antibucle: más de 6 reinicios en 4 s sin oír nada → aviso y se apaga.
- **Sin soporte** (Firefox, o página sin HTTPS/localhost): icono deshabilitado con tooltip que lo explica.
- **Limitaciones**: en Chrome/Edge el audio se procesa en servidores de Google/Microsoft (hace falta conexión) y exige **HTTPS o localhost** (`https://localhost:44384` vale). La primera vez el navegador pide permiso del micrófono. Mientras escucha el navegador puede abrir el micro dos veces (reconocimiento + medidor de volumen).

| Pieza | Dónde / función |
|-------|-----------------|
| Chat | `mountWallAiChat`: `micBtn`, `isListening()` (encendido por el usuario), `stopVoice(keepText)`, `meter`, `meterInfo()`; `send(fromVoice)`; `reset()` apaga el micrófono |
| Funciones | `maStlWallAiSpeechRecognitionCtor()`, `maStlWallAiVoiceLang(history, uiLang)`, `maStlWallAiVoiceErrorText(code, T)`, `maStlWallAiVoiceErrorPolicy(code)`, `maStlWallAiIsBareAltKey(ev)`, `maStlWallAiIsEditableTarget(el)`; textos `mic`, `micStop`, `micOff`, `micUnsupported`, `micInsecure`, `micListening`, `micErr*` en `MA_STL_WALL_AI_TEXT(_EN)` |
| Muro recto | `wall-edit-form.js` `showTab(≠'ai')` → `aiChat.stopVoice(false)` (cubre volver del compacto); cerrar → `resetAi(null)` |
| Muro curvo | `maStlCurvedWallEnsureAiTab`: `hide.bs.tab` de la pestaña IA → `stopVoice(false)` (cubre volver del compacto); `hidden.bs.modal` → `reset(null)` |

Imports `?v=20261114d` (Alt + modo continuo; antes `20261114c` gris 70%, `20261114b` primera versión) (`wall-edit-form.js`, `wall-ai-assistant.js`; `WallForm.cshtml`). Pruebas: `/workspace/ia/p11/test.mjs` (funciones + `harness-p11.html` con mock de `SpeechRecognition` continuo con resultados acumulados, de `getUserMedia` y de `AudioContext`: modo continuo, «sí» por voz, duplicados, reinicio tras fin de sesión, cola de órdenes, Alt/Alt+Tab/AltGr, errores y antibucle). Capturas: `/workspace/ia/p9-voz-compacto-escuchando.png`, `p9-voz-indicador-detalle.png`, `p9-voz-curvo-escuchando.png`, `p9-voz-compacto-escuchando-en.png`, `p9-voz-formulario-plugin-escuchando.png`, `p9-voz-sin-soporte.png` (tooltips con «Espacio», ya sustituido por Alt), `p10-voz-gris.png` (colores grises actuales).

#### Respuestas habladas: el Asistente IA contesta en voz alta (14/11/2026, `?v=20261114g`; números, voz local, saludo y avisos `?v=20261114h`)

Botón de **altavoz** a la derecha del micrófono (caja · micrófono · altavoz · **Enviar**), en el formulario normal (página del plugin), en el modo compacto y en el muro curvo (es el mismo `mountWallAiChat`). Usa `speechSynthesis` del navegador con las **voces del sistema** (gratis; en Windows las voces de Microsoft, en Chrome también las de Google). Solo JS: sin C#, sin BD.

- **Icono**: Remix `ri-volume-up-line` (encendido: botón gris 70% `#4d4d4d` con icono blanco, como el micrófono escuchando; mientras habla, pulso `ma-stl-wall-ai__spk--talking`) y `ri-volume-mute-line` (apagado: fondo blanco, icono azul). Toggle propio, **encendido por defecto** (lo pidió el usuario) y recordado en `localStorage` (`maStlWallAi.speak` = `'1'`/`'0'`; `maStlWallAiSpeakPref` / `maStlWallAiSetSpeakPref`). El recto y el curvo releen la preferencia al enfocar o cambiar de muro.
- **Sin soporte**: sin `speechSynthesis` → icono deshabilitado con tooltip «Este navegador no puede leer en voz alta…»; sin ninguna voz del idioma de la conversación (lista cargada o 2,5 s sin `voiceschanged`) → deshabilitado con «No hay ninguna voz de es-ES instalada en este equipo (Windows: Configuración › Hora e idioma › Voz)». Mientras la lista de voces no ha llegado no habla (Chrome la carga al momento con `voiceschanged`).
- **Idioma y voz**: el de la conversación (`maStlWallAiVoiceLang`: último mensaje del usuario; si no se distingue, la interfaz) → `es-ES` / `en-US`. `maStlWallAiPickVoice(voices, tag)` elige la **más eficiente** (respuesta del usuario: «la que sea más eficiente»): mismo idioma obligatorio; la región exacta primero (es-ES antes que es-MX/es-US); dentro de ella, las **voces locales del sistema** (`localService = true`: Microsoft Helena/Laura/Pablo en es-ES, David/Zira en en-US) antes que las remotas, porque no usan red y empiezan antes; si no hay ninguna local del idioma, la remota (Edge «… Online (Natural)» > Google `Google español` / `Google US English`); desempate por nombres conocidos y la predeterminada.
- **Qué lee** (`maStlWallAiSpeechText(res, lang)`): el texto de la respuesta en lenguaje natural, nunca tablas ni códigos de artículo.
  - Si la respuesta trae `speech`, solo eso: **recuento** → «Este muro lleva N piezas de M referencias; te dejo la lista en pantalla.»; **resumen del cambio guiado** → «Paneles de 270 por 90: 13 pasan a 9, usas 9 de tus 10. Te dejo el resumen y la tabla en pantalla. ¿Lo hago?».
  - Listas (`·`, `•`) no se leen: se sustituyen por «Te dejo el detalle en pantalla.» (antes de la pregunta final si la hay). **Opciones** en corto: «Opción 1: 60 más 30. Opción 2: 45 más 45…» (máximo 4; sin el «→ en total…»).
  - Otras respuestas con tabla: primera línea + preguntas + aviso de pantalla. Máximo ~600 caracteres (se corta en una frase + aviso). Las medidas del título del muro («(L 6,70 · e 0,30 · h 2,70)») no se leen.
  - **Saludo inicial** («Buenos días»…): se lee una vez por conversación al mostrarse la pestaña IA (`focus()`; otra vez al cambiar de muro). Si el navegador no deja hablar sin interacción previa (`navigator.userActivation.hasBeenActive === false`) no se intenta; si la voz falla con `not-allowed` (autoplay) se deja: **no se reintenta** (ni en bucle ni al volver a abrir). **Avisos del micrófono** (errores y «Micrófono apagado.») también se leen, en el idioma del micrófono. La franja «Te estoy contestando… (micrófono en pausa)» **no** se lee (sería recursivo). No se leen «Calculando…»/«Aplicando…» (`pendingText`).
- **Números** (`maStlWallAiSpeakable`, `maStlWallAiSayNumber`; respuesta del usuario 14/11/2026), siempre con cifras para que la voz diga «90» natural: **medidas de panel/pieza** (metros con 2 decimales, sin unidad) en **centímetros**: «0,90» → «90», «2,70×0,90» → «270 por 90», «0,60 + 0,30» → «60 más 30». **Cualquier otro número** con parte decimal no nula se lee «X punto Y» («90,45» → «90 punto 45»; distancias: «3,60 m» → «3 punto 60 metros», «4,50 m» → «4 punto 50 metros»; «2,5°» → «2 punto 5 grados») y con decimales cero solo la parte entera («90.00» → «90», «1.667,0 kg» → «1667 kilos»; sin separador de miles). En inglés «point». `kg` → kilos, `mm` → milímetros, `°` → grados, `Ø` → diámetro, `→` → «pasan a», `nº` → número, `BD` → base de datos. Se quitan emojis, comillas, viñetas, paréntesis y flechas.
- **Trozos** (`maStlWallAiSpeechChunks`, fallo de Chrome que corta frases de más de ~15 s): por frases, juntando las cortas hasta 180 caracteres; una frase más larga se parte por comas y, si hace falta, por palabras. Cada trozo es un `SpeechSynthesisUtterance` con la voz elegida; el siguiente sale en `onend`/`onerror`, con vigilante (4 s + 110 ms/carácter) por si Chrome no avisa.
- **Con el micrófono en modo continuo**: mientras habla, el reconocimiento se **para** (`abort`, sin apagar el modo continuo; la franja dice «Te estoy contestando… (micrófono en pausa)») y cualquier resultado que llegue se ignora: no se escucha a sí misma. Al terminar vuelve a escuchar (300 ms). **Pulsar el micrófono o Alt mientras habla corta la voz** (`speechSynthesis.cancel()`) y deja el micrófono escuchando (si estaba apagado, lo enciende). **Enviar** un mensaje o pulsar un botón de una respuesta también corta la voz (y se lee la respuesta nueva). Apagar el altavoz corta la voz.
- **Se corta** también al cambiar de pestaña fuera de IA o volver del compacto al formulario (`stopVoice`), al cambiar de muro o cerrar el formulario (`reset`, recto y curvo), si el chat deja de verse, al ocultar la página (`visibilitychange`) y en `pagehide`.

| Pieza | Dónde / función |
|-------|-----------------|
| Chat | `mountWallAiChat`: `speakBtn`, `isSpeaking()`, `isSpeakOn()`, `stopSpeaking()`, `speak(res)` (lo usa `pushAssistant`), `speakInfo()`; `stopVoice()` y `reset()` cortan la voz |
| Funciones | `maStlWallAiSpeakPref`, `maStlWallAiSetSpeakPref`, `MA_STL_WALL_AI_SPEAK_KEY`, `maStlWallAiPickVoice`, `maStlWallAiSpeakable`, `maStlWallAiSpeechText`, `maStlWallAiSpeechChunks`; textos `spk`, `spkOff`, `spkUnsupported`, `spkNoVoice`, `micPaused`, `speakOption`, `speakScreen`, `matSpeech`, `swapSpeakConfirm` (es/en) |
| Respuestas | `speech` en el recuento (`materialsFinish`) y en el resumen del cambio guiado (`swapReview`); `pending: true` en los `pendingText` de los botones |

Imports `?v=20261114h` (antes `20261114g`) (`wall-edit-form.js`, `wall-ai-assistant.js`; `WallForm.cshtml`; visor). Pruebas: `/workspace/ia/p15/test.mjs` (antes p14) (funciones, qué se lee en el flujo real del cambio guiado con el solver, y `harness-p15.html` con mock de `speechSynthesis` y de `SpeechRecognition`: icono, voz elegida, recuento/opciones, trozos, pausa del micro, cortes con micro/Alt/enviar/botón/apagar/cambio de pestaña/muro/cerrar, sin API, sin voz, voces tardías, preferencia guardada; saludo una vez por conversación, sin interacción previa (`?ua=none`) y bloqueado por autoplay (`?ss=blocked`) sin reintentos, avisos del micrófono leídos, medidas en cm y «punto»; recto, curvo, plugin, EN). `harness-p11.html` arranca con el altavoz apagado (solo prueba el micrófono). Capturas: `/workspace/ia/p14/shots/` (`p14-recto-hablando.png`, `p14-iconos-detalle.png`, `p14-iconos-apagado-detalle.png`, `p14-curvo-hablando-en.png`, `p14-plugin-sin-voz.png`).
- **Limitaciones**: la calidad depende de las voces instaladas (Windows sin voces de español → icono deshabilitado). Las voces de Google de Chrome usan conexión; las de Microsoft de escritorio son locales. Chrome solo deja hablar tras una interacción con la página (aquí siempre la hay: escribir, pulsar o el micrófono). Si hay un altavoz muy alto con el micro, el final de la frase puede colarse en el reconocimiento al reanudar (se reanuda 300 ms después de terminar).

#### Recuento = encofrar solo el muro editado (14/11/2026, `?v=20261114e`)

Al pedir el recuento («material de encofrado de este muro») de un muro **sin piezas en la escena**, además de la lista se **dibuja en el visor el encofrado de SOLO ese muro**: las mismas piezas de la lista (resultado de `previewWall`: vista previa con `Preview=true`, vecinos a 2 saltos solo como contexto de las esquinas y filtrado por `IdWall`, con las esquinas compartidas asignadas como en el recuento). El resto del diseño no se encofra ni se pinta y no se guarda nada. Mensaje: «He encofrado en el visor solo este muro (N piezas, las de la lista). Es provisional…». Si el muro ya tiene piezas del «Encofrar» real, se usan esas (no hace falta provisional).

- **Atributo `_IsFormwork`** (BD `Design_Wall.Is_Formwork`, se guarda con `maStlDesing2SaveDesignWallsToServer` / `DesignWallRepository`): `false` = el muro no se encofra; «Encofrar» filtra esos muros en el cliente (`maStlDesing2WallIsSpecial`; el cálculo C# no lo lee). **No se usa poniendo `_IsFormwork = false` a los demás muros**: `false` equivale a «muro especial»; al pulsar «Encofrar» o pasar a 3D, `maStlDesing2ReleaseEmptySpecialWalls` vuelve a `true` los muros especiales sin piezas manuales **y guarda el diseño en el servidor**, y el guardado normal también lo persistiría; además, quitar los vecinos cambiaría las esquinas respecto al recuento. Se aplica la misma regla («solo este muro se encofra») sobre la **copia de la petición** de la vista previa: no cambia `_IsFormwork` de ningún muro, ni en memoria ni en BD.
- **Pintado**: `maStlWallAiShowTempFormwork(idWall, items)` usa el mismo pintor que «Encofrar» (`maStlDesing2RenderAtk60Elements`) y pasa las piezas a un grupo propio de la escena (`maStlWallAiTempFormwork`), fuera del grupo ATK-60: no cuentan como encofrado (`maStlDesing2HasFormworkPanels`, botón «Cantidad de material», re-encofrar al editar un muro), **no se guardan** (`maStlDesing2CollectPersistArticles` solo recorre el grupo ATK-60), no se seleccionan ni se editan (sin `maStlAtk60Element`/`maStlAtk60SamplePlaced`, `raycast` vacío) y el «último encofrado» (`maStlDesing2LastAtk60PaintPayload`) se deja como estaba. Visible con el grupo ATK-60 (vista 3D de muros); en vista de líneas queda preparado y oculto («se ve en la vista 3D de muros»). Sin `MARK_`.
- **Se quita** (`maStlWallAiClearTempFormwork`, también `window.`): con un recuento nuevo (sustituye), al cambiar de muro o cerrar el formulario (`reset` del chat, recto y curvo), al pulsar «Encofrar» (`maStlDesing2RenderAtk60AnchorPoints` con `clearPrevious`), al limpiar el encofrado (`maStlDesing2ClearAtk60Visuals`, cambio de modo), antes de aplicar cambios de la IA (`applyWall`) y si la vista previa falla. No interfiere con «Encofrar»: este lo borra al empezar y pinta lo suyo como siempre.
- **`ctx.formwork`**: `showTempWall(previewRes)`, `clearTempWall()`, `tempWallInfo()`.
- **Servidor**: sin cambios; la vista previa ya devuelve la geometría (`ElementsForThreeJs.Elements`, la misma que pinta «Encofrar»). No hace falta recompilar.
- **Pruebas**: `/workspace/ia/p12/test.mjs` (datos reales del solver: solo piezas del muro, nada en el grupo ATK-60 ni en el último encofrado, sin marcas de pieza, sustituye, limpia, `applyWall`, muro ya encofrado, diseño encofrado sin este muro, vista sin 3D, EN, error) + `harness-p12.html?temp=1` (recto, curvo, EN: se quita al cambiar de muro y al cerrar).

#### Cambio guiado de paneles (14/11/2026, `?v=20261114f`)

Orden de un solo ancho en un muro recto, en español o inglés y tolerante a faltas y al dictado: «de 13 paneles de atk60 de 2,70 x 90 solo tengo 10», «me faltan 3 de 0,90», «no tengo de 0,90», «I only have 10 panels of 2.70 by 0.90» (`maStlWallAiParseSwapOrder`: ancho, alto, total que dice el usuario, «tengo N», «me faltan N», «no tengo»). Un número que no es medida ni cantidad («del 19») no se interpreta: se pregunta, con el botón «Seguir sin…». Las órdenes de varios anchos y el muro curvo siguen con la fase 3 de antes.

1. **Recuento real**: «En este muro hay 13 paneles de 2,70×0,90: 6 parejas enfrentadas (una en cada cara) y 1 suelto en una esquina o remate. Tienes 10, así que te faltan 3.» Si el usuario dice otro total, se le avisa («Dices 12, pero en este muro cuento 13»). Si le llega, no se cambia nada. Si hay varias alturas de ese ancho y no la dice, se pregunta.
2. **Por parejas**: cada panel va enfrentado al de la otra cara (el tirante ata los dos), así que se cambian las dos caras a la vez: faltan 3 → 2 parejas = 4 paneles, «quedan 9 (usas 9 de tus 10)».
3. **¿Todos de una vez o uno a uno?** (botones o texto/voz: «todos», «de una vez», «uno a uno», «one by one»; «cancelar»/«no» cancela). Con una sola pareja se va directo al panel.
   - **Todos**: se eligen las parejas desde el final del muro hacia el inicio; se listan («nº 6 desde el inicio del muro, de 4,50 a 5,40 m, en las dos caras») y se **resaltan en naranja** en el visor.
   - **Uno a uno**: «Panel 1 de 2: el nº 6 desde el inicio del muro (de 4,50 a 5,40 m), en las dos caras», resaltado. «Otro panel» lista las parejas libres numeradas (también «el 3»).
   - Si el muro no tiene piezas en la escena, primero se dibuja el encofrado provisional del muro (como el recuento) para poder resaltar.
4. **Opciones** (`maStlWallAiSwapCombos`): solo anchos del catálogo (0,30–0,90 de 0,15 en 0,15) que suman el ancho del panel, de 2 a 3 piezas, mismo alto, **nunca el ancho que falta**, sin los anchos que el muro tiene excluidos ni pasarse de los máximos de `_PanelStock`. Orden: menos piezas y, a igualdad, el más ancho primero. 0,90 → 1) 0,60 + 0,30 · 2) 0,45 + 0,45 · 3) 0,30 + 0,30 + 0,30 (con el total: «+4 de 0,60 y +4 de 0,30»). 0,75 → 0,45 + 0,30 · 0,60 → 0,30 + 0,30 · 0,45 y 0,30 → ninguna (se dice, y se ofrece «Recolocar el módulo entero (automático)», que es la fase 3 de siempre con `_PanelStock`). Respuesta por botón o texto/voz: «la 2», «la segunda», «dos de 0,45», «0,60 + 0,30», «una de 60 y otra de 30», «number 2», «two of 0.45» (`maStlWallAiParseSwapChoice`).
5. **Vista previa** con `_PanelReplace` (solo en esa petición) → ANTES / DESPUÉS (tabla con nombres del catálogo), resumen («nº 5 (3,60–4,50 m): 0,90 → 0,45 + 0,45, las dos caras · Paneles de 2,70×0,90: 13 → 9 (usas 9 de tus 10)») y Sí/No (también «sí»/«no» escrito o dictado). El asistente comprueba en la respuesta que el servidor ha cambiado cada panel; si no ha cambiado ninguno, avisa de que falta recompilar.
6. **Aplicar** = `applyWall(idWall, piezas, { _PanelReplace })`: se quitan las piezas de ese muro (paneles y sus grapas, tirantes, rigidizadores, puntales…) y se pintan las nuevas que devuelve el servidor; el resto del encofrado no se toca. «Deshacer» deja las piezas y el atributo como estaban.

- **Persistencia**: `_PanelReplace` vive en el `userData` del muro (eje y sólido) durante la sesión, está en `MA_STL_WALL_LINE_ATTR_KEYS` para que un «Encofrar» posterior lo respete y no se guarda en BD (no hay columna; propuesta en `encofrado-atk-60.md` §9.4). No cambia `_IsFormwork` ni marca el muro como especial. Si cambia la geometría del muro, las reglas que ya no casan con un panel no hacen nada.
- **Resaltado** (`maStlWallAiHighlightPanels(idWall, [{ code, alongMm, upMm }])`, `maStlWallAiClearPanelHighlight`, también `window.`): cambia solo el material en pantalla de los paneles que casan por `IdWall`, código y posición del servidor (`LocalAlongMm`/`LocalUpMm`, ±2 mm), en el grupo ATK-60 o en el provisional. El pintor guarda esa posición en `userData.maStlAtk60LocalAlongMm/LocalUpMm` (no va a `maStlDesing2CollectPersistArticles`). Se quita al aplicar, cancelar, cambiar de muro, cerrar el formulario y al limpiar el provisional. `ctx.formwork`: `highlightPanels(idWall, list)`, `clearHighlight()`.
- **Servidor**: necesita el C# nuevo (`_PanelReplace`, ver `encofrado-atk-60.md` §9.4): **hay que recompilar y publicar** Desing.
- **Pruebas**: `/workspace/ia/p13/test.mjs` (65 comprobaciones con el solver real: L con el muro 4 de 6,70 m = 13 × 2,70×0,90) y `harness-p13.html?check=1` (11, chat real); capturas en `/workspace/ia/p13/shots/`.

#### Fase 2 (13/11/2026): intérprete local y «material de encofrado de este muro»

Intérprete **local** (sin servicios externos) en `wall-ai-assistant.js`, **tolerante a faltas y frases mal formuladas** (13/11/2026, imports `?v=20261113e`):

| Paso | Función / regla |
|------|-----------------|
| Normalizar | `maStlWallAiNormalize(text, { keepEnye, collapse })`: minúsculas, sin tildes ni diéresis (ñ→n salvo `keepEnye`), sin signos, espacios simples; `collapse` colapsa letras repetidas («MATERIALES!!», «materialesss») |
| Abreviaturas | `maStlWallAiTokens`: q/k/ke→que · pa/xa→para · xq/pq/xk→porque · d→de · dl→del · x→por · tb/tmb→también · mat→material · mats→materiales · deste→de este · ste→este · kg→kilos · nec→necesito · tng→tengo · porfa/xfa/pls→por favor · ok/oki→vale · sip/sii→sí… |
| Fonética | `maStlWallAiPhonetic`: c(e,i)/z/s→s · c(a,o,u)/qu/k→k · b/v→b · ll/y→y · h muda · g(e,i)/j→j · gu(e,i)→g («nesesito»=«necesito», «yeva»=«lleva», «piesas»=«piezas», «ase»=«hace») |
| Difuso | `maStlWallAiDamerau` (Damerau-Levenshtein OSA, transposición = 1) sobre la palabra y su clave fonética; errores admitidos por longitud de la palabra del vocabulario (`maStlWallAiFuzzyLimit`): ≤3 letras 0 · 4–8 letras 1 · >8 letras 2. Calidad (`maStlWallAiWordMatch`): exacta 1 · fonética 0,95 · 1 error 0,8 · 2 errores 0,65. Pronombres pegados («sustitúyelos», «encofrarlo») se prueban sin el pronombre |
| Puntuación | `MA_STL_WALL_AI_VOCAB`: por intención, conceptos con peso (material/despiece 3, piezas 1,8, necesito/falta 1,5, lleva 1,5, peso/kilos 1,5, lista 1, encofrar 1, cuánto 0,8, paneles/grapas… 0,8, dame/muéstrame 0,6, muro 0,3, que 0,3; **borra/elimina/mueve/gira/copia/dibuja/crea −3**). Cada palabra cuenta solo para su mejor concepto. `maStlWallAiAnalyze(text)` → `{ intent, score, confidence, scores, tokens, hits }` |
| Umbrales | `MA_STL_WALL_AI_SCORE`: **alta** ≥ 2 con margen ≥ 0,6 sobre la segunda intención → actúa · **media** ≥ 1 → en «material» pregunta «¿Querías decir: «dame el material de encofrado de este muro»?» con botones **Sí / No** (también se puede contestar escribiendo «sí», «vale», «ok», «no»: `maStlWallAiYesNo` + `confirm` guardado en el historial) · **baja** → «No he entendido…» + ayuda |
| Números (siguiente fase) | `maStlWallAiParseMeasure(text)` → `{ m, mm }`: «0,90» «0.90» «0'90» «,90» «90» (entero ≥ 10 = cm) «noventa» «cero noventa» → 0,90 · «uno veinte» «un metro veinte» «1 20» «1,2» «120» «ciento veinte» → 1,20 · «setenta y cinco» → 0,75 · «medio metro» · unidades mm/cm/m. `maStlWallAiParseNumberWords` (0…999 999 en palabras, tolerante a faltas: «nobenta»). Aún no se usan |

`maStlWallAiDetectIntent(text)` sigue existiendo (intención con confianza alta o media, o null).

#### Español e inglés (13/11/2026, `?v=20261113e`)

| Pieza | Detalle |
|-------|---------|
| Idioma del mensaje | `maStlWallAiDetectLanguage(text, fallback)` → `{ lang, es, en, decided }`: palabras propias de cada idioma (`MA_STL_WALL_AI_LANG_WORDS`: de/el/que/para/dame/muro/q/pa/xq… · the/of/what/how/give/need/wall/u/ur/pls/thx/wat…) 1 punto; con faltas contra vocabulario largo de cada idioma 0,6; tildes/ñ/¿¡ +2 al español; contracciones inglesas (don't, what's…) +2 al inglés. Palabras compartidas (material, panel, BOM, ok, no, kilos…) no cuentan. **Empate → idioma de la conversación** (último mensaje del asistente con `lang`) **y si no, el de la interfaz** |
| Idioma de la interfaz | `maStlWallAiUiLang()`: `<html lang>` (lo pone `_LayoutMaterio.cshtml` con la cultura UI que resuelve `LanguageUiHelper`: cookie `tandem_lang` / `tandem_ui_culture` o idioma impuesto por la empresa) → cookies `tandem_lang` / `tandem_ui_culture` si traen un código (no un IdObject `i:12`) → `es`. `WallForm.cshtml` (plugin AutoCAD) tiene `lang="es"` fijo |
| Textos | `MA_STL_WALL_AI_TEXT` (es) y `MA_STL_WALL_AI_TEXT_EN` (en), mismas claves; `maStlWallAiTexts(lang)`, `maStlWallAiUiTexts()`. **Interfaz** (pestaña «Asistente IA»/«AI Assistant», placeholder «Ej.: dame el material…»/«E.g.: give me the formwork material for this wall», Enviar/Send, Tú/You, saludo Good morning/Good afternoon/Good evening con los mismos tramos 06–13:59 / 14–20:59 / 21–05:59) según la UI. **Respuestas** según el idioma del mensaje: título y resumen («Formwork material for wall 1 (L 6.00 · t 0.30 · h 2.70) — 16 items · 143 pieces · 1,667.0 kg»), cabeceras Code/Description/Qty/Weight (kg), Total, números 1,667.0 vs 1.667,0, avisos, ayuda, «Did you mean…?» con Yes/No, Calculate/Recalculate formwork, Copy/Copied, «next phase», «You're welcome» |
| Intenciones EN | Mismos conceptos y pesos con palabras inglesas: material(s)/BOM/bill of materials/takeoff · breakdown · parts/pieces/items · list · formwork/forms · need (1,8)/required · take(s) (1) · how many/much · weigh/weight (1,8)/heavy · panels/clamps/brackets/props · give/show/get/send · what/which · wall · negativas delete/erase/remove/move/rotate/copy/draw/create/undo (−3); replace/substitute/swap · change/remove · have/got · not/only/just/don't; help/commands · hello/hi/hey · can/could · do/does; gracias/thanks/thx/ty → «De nada»/«You're welcome» |
| Abreviaturas EN | u→you · ur→your · r→are · pls/plz→please · thx/ty→thanks · wat/wht→what · whats→what is · hw→how · gimme→give me · dont→do not · mats→materials · pcs→pieces · qty · lbs · yeah/yep/yup→yes · nope/nah→no · k/okay→ok |
| Faltas | Igual que en español (Damerau-Levenshtein con los mismos umbrales + clave fonética); desde ahora las palabras de ≤ 3 letras solo cuentan exactas o fonéticas (evita «are»≈«hace») |
| Números EN | `maStlWallAiParseMeasure`: «0.90», «ninety»/«ninty», «point nine», «zero point nine», «point nine zero», «zero ninety», «one twenty», «one point two», «one meter twenty», «120 cm», «one hundred (and) twenty», «seventy-five», «half a meter», «two meters», «ninety centimeters», «750 mm». `maStlWallAiParseNumberWords` admite inglés (hundred, thousand, and, a hundred) |
| Descripciones | Los nombres de artículo son los del catálogo (`Tsql_Master_Articles.TextLabel`): **no hay descripción en inglés en BD** (la tabla solo tiene `TextLabel`; `TSql_Language*` no traduce artículos). Si algún día `PreviewMaterialCount` devuelve `nameEn`/`textLabelEn`, se usa en inglés; si no, la original. No se traducen nombres |

Tests: `/workspace/ia/p5/test.mjs` (172) + `p3` (173) + `p2` (32). Capturas `p5-recto-en.png`, `p5-pregunta-en.png`, `p5-ui-es-mensaje-en.png`, `p5-curvo-en.png`.

| Orden | Ejemplos reconocidos | Respuesta |
|-------|----------------------|-----------|
| `materials` | «dame el material de encofrado de este muro», «materiales», «lista de materiales», «lista de piezas», «despiece», «¿qué necesito para encofrar este muro?», «qué hace falta…», «cuántos paneles lleva», «cuánto pesa», «muéstrame las piezas»; con faltas: «dame el matrial de encofado deste muro», «q nesesito pa encofrar», «cuantos paneles yeva», «despice», «kuanto pesa», «lista d piesas», «MATERIALES!!» | Lista de materiales del muro (tabla); confianza media («dame el encofrado», «piezas») → «¿Querías decir…?» |
| `replace` | «no tengo paneles de 0,90, sustitúyelos», «solo tengo 3 paneles de 0,90 del 19» | Desde la fase 3: antes/después y Sí/No (ver «Fase 3» más abajo) |
| `help` / desconocida | «ayuda», «hola», «q sabes acer» / cualquier otra cosa («borra el muro», «gira el muro») | Lista de órdenes disponibles (la desconocida cita el texto: «No he entendido «…»») |

**Material del muro** (`maStlWallAiMaterialsReply(ctx)`): reutiliza el mecanismo del popup «Cantidad de material» (`#ma-stl-material-count`): mismas piezas (`maStlDesing2LastAtk60PaintPayload.elements` del último Encofrar), mismas reglas de código (`maStlDesing2FormworkCountRows(filter)`: quita `PANEL_`, ignora `_PHENOLIC` y `MARK_*`, quita `_FRAME`) y mismo endpoint `Desing_2/PreviewMaterialCount` (atributo `data-ma-stl-material-count-preview-url` del shell) para nombres (`TextLabel`), código Atenko (`AddAtenkoCode`), pesos (`NumberWeight`), puntales legacy `HTipo*` desglosados, `H0_2*`/`10000221B` ocultos y reemplazos de stock. Solo cambia el filtro: piezas cuyo `IdWall` es el del muro editado.

| Pieza | Detalle |
|-------|---------|
| `ctx.formwork` | Lo pone el visor con `maStlWallAiFormworkAccess(idWallKeys, axisMm)`: `hasAny()`, `elements()`, `anchors()`, `countRows(filter)`, `isEdited(id)`, `previewUrl`, `runFormwork()` |
| IdWall recto | `_idObject` del muro 3D = `id ?? wallId ?? wallGroupId` del eje (después `_idObject`, `wallDbId`). Respaldo: anclas del cálculo (`walls[].Debug.Start/End`) colineales (≤ 5 mm) y solapadas con el eje (`maStlWallAiResolveIdWalls`) |
| IdWall curvo | `String(wallId ?? id)` (igual que `maStlCollectCurvedFormworkWalls`, p. ej. `c3`). Muro curvo nuevo (`curved-new`): «aún no existe» |
| Sin encofrado | Si el diseño no tiene encofrado: mensaje + botón **Calcular encofrado**; si lo tiene pero sin piezas de este muro (calculado antes de crearlo/modificarlo): botón **Recalcular encofrado**. El botón pulsa el mismo «Encofrar» de la barra (`#ma-stl-mode-formwork`; calcula **todo el diseño**, desde la fase 3 solo si falla el cálculo de solo este muro), espera `desing2-formwork-settled` y vuelve a responder con la lista. No se toca la escena si el usuario no pulsa |
| Tabla | Código (Atenko si existe) · Descripción (catálogo; «—» si el artículo no está en el maestro, p. ej. `REMATE_WOOD`) · Cant. · Peso (kg, si hay pesos) + fila **Total** (piezas y kg). Cabecera con referencias/piezas/kg. Botón **Copiar**: TSV (pegar en Excel) |
| Avisos | Esquinas compartidas: cuentan en el muro al que el cálculo asignó la pieza. Piezas movidas/borradas a mano (`maStlDesing2FormworkEditedKeys`): la lista refleja el cálculo. Catálogo caído: se muestran solo códigos y cantidades (nada inventado) |

`window.maStlDesing2FormworkCountRows()` (popup de material) sigue igual, sin filtro. Sin cambios de C#: basta **Ctrl+F5** (imports a `?v=20261113e`).

Harness fase 2 (datos reales: solver ATK-60 local `MIX_L150_curve` —rectos `1`/`4` en L + curvo `c3`— y artículos de `Tsql_Master_Articles` de develop; funciones reales del visor extraídas de `viewer.js`; `PreviewMaterialCount` emulado con la lógica del controlador): `/workspace/ia/p2/test.mjs` (32 comprobaciones) y `/workspace/ia/p3/test.mjs` (173: incluye 12 de saludo por hora y límites, 46 frases de material con faltas, 7 de confianza media, 19 negativas, 13 de otras intenciones, normalización/abreviaturas/fonética/Damerau/umbrales, sí/no, 38 de números, conversación). Capturas `/workspace/ia/p2-recto.png`, `p2-curvo.png`, `p2-sin-encofrado.png`, `p2-sin-encofrado-calculado.png`, `p2-ayuda.png`, `p3-faltas.png`, `p3-pregunta.png`, `p3-pregunta-si.png`, `p3-negativa.png`.

#### Fase 3 (13/11/2026, `?v=20261113f`): material de SOLO este muro y paneles que no hay

Desde el 14/11/2026 (`?v=20261114f`), en el muro recto una orden de **un solo ancho** abre el **cambio guiado** (ver «Cambio guiado de paneles»); el reparto automático con `_PanelStock` de esta fase sigue para órdenes de varios anchos, para el muro curvo y con el botón «Recolocar el módulo entero (automático)».

**Necesita recompilar y publicar** (C# nuevo: `Preview`, `_PanelStock`, `PanelStockNotes`). Con el JS nuevo y el servidor antiguo, el material sale (con aviso «hay que recompilar») y el cambio de paneles responde que el servidor aún no sabe hacerlo (no cambia nada).

| Pieza | Detalle |
|-------|---------|
| Material sin piezas pintadas | `maStlWallAiMaterialsReply`: si el muro no tiene piezas en la escena, `ctx.formwork.previewWall()` calcula **solo ese muro** (vecinos a 2 saltos como contexto de esquinas, `Preview=true`) y desde el 14/11/2026 **encofra en el visor solo ese muro, de forma provisional** (ver «Recuento = encofrar solo el muro editado»): «Calculado solo para este muro (vista previa: no cambia ni guarda el encofrado del diseño)». Con piezas pintadas, usa esas piezas, como antes. El botón «Calcular encofrado» (todo el diseño) queda solo si el cálculo del muro falla |
| `ctx.formwork` (visor) | Además de lo anterior: `countRows(filter, list)`, `previewWall({ attrs })` → `{ ok, idWall, elements, notes, contextWalls }` (`window.maStlDesing2PreviewWallFormworkAtk60` del wiring), `applyWall(idWall, items, attrs)` (cambia en la escena solo las piezas de ese `IdWall` y escribe `attrs` en el `userData` del muro; devuelve `{ previous, previousAttrs }` para deshacer) y `wallAttr(key)` |
| Orden de paneles | `maStlWallAiParseStockOrder(text, lang)`. «no tengo paneles de 0,90, sustitúyelos» → excluir 0,90 · «solo tengo 3 paneles de 0,90» → como mucho 3 · «solo tengo 3 de 0,90 y 2 de 0,75» · «no tengo de 0,9 ni de 0,75» · «quita los de 45» · «tengo 0 paneles de 0,60» (= excluir) · «I don't have 0.90 panels, replace them» · «I only have twenty two panels of 0.60» · faltas y abreviaturas («no tngo paneles d 90»), números en palabras («tres de cero noventa», «zero point nine») |
| Lo que no entiende | Un número que no es cantidad ni ancho («del 19») **no se interpreta**: «Entiendo: como mucho 3 paneles de 0,90. Pero no sé qué quiere decir «del 19»…» con el botón «Seguir sin «del 19»». Sin ancho («no tengo paneles») → pregunta cuál |
| Antes / después | «Antes» = piezas pintadas del muro (o vista previa si no hay). «Después» = vista previa con `_PanelStock` = regla del muro + orden (`maStlWallAiMergeStock`). Paneles por ancho (0,90: 12 → 0 · 0,75: 0 → 12…), piezas y kg, tabla **Código · Descripción · Antes · Después · Dif.** (solo lo que cambia; paneles primero; Copiar en TSV). Avisos: tramo sin combinación (hueco y compensación del manual), paneles que quedan en tapes, uso frente a límite («se usan 2 de los 3 que tienes»), ancho que el muro no usa («no hay nada que cambiar»), 1,05/1,20 en muro recto |
| Sí / No | Botones **Sí, aplicar** / **No**; también «sí» / «no» escritos (`confirm: 'stock'` + `apply`/`cancel` en el historial). Se aplica una sola vez; «No» después de aplicar no deshace |
| Aplicar y deshacer | `applyWall` re-encofra en la escena **solo ese muro** (las demás piezas no se tocan) y guarda `_PanelStock` en el muro. Botón **Deshacer** (un uso): vuelve a las piezas y a la regla de antes. Encofrar después respeta la regla (va en `IdsJson`). La regla dura la sesión: **no se guarda en BD** (ver `encofrado-atk-60.md` §9.2, columna propuesta) |
| Muro curvo | Un solo ancho: si la orden afecta a la modulación, propone la válida más cercana (Ahora / Propuesta / Otra opción, con paneles por cara, giro y remates, y avisos > 0,15 / < 0,05), tabla de diferencia (vista previa con `_CurveModulationMm`). Al aplicar, cambia el desplegable de modulación (`ctx.setModulation`) y repinta ese muro; se guarda con **Aceptar**. Deshacer devuelve las dos cosas. Si no le afecta: «no le afecta» (y con límite: «Necesita 14 y tienes 20») |
| Textos | Ayuda y `helpShort` ya no dicen «próximamente»; todo en ES y EN (`stock*`, `curved*`, `comp*`, `matSummaryPreview`, `matPreview*`, `colBefore/After/Diff`) |

Tests: `/workspace/ia/p6t/test.mjs` (78; con salidas reales del solver C# modificado para MIX_L150_curve: `out/p6-previews.json`), más p2 (32), p3 (173) y p5 (172). Capturas: `p6-paso1-solo-este-muro.png`, `p6-paso2-antes-despues.png`, `p6-paso2-aplicado-deshacer.png`, `p6-paso2-del19-pregunta.png`, `p6-paso2-manual-compensador.png`, `p6-paso2-curvo-propuesta.png`, `p6-paso2-en.png`. En el arnés, los nombres y pesos de 0,45 y 0,75 son estimados (no estaban en el extracto del maestro).

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
| Borrar | Clic `#ma-stl-tool-delete` → crosshair + HUD; clic alterna línea o malla STL en selección (verde flúor); **Enter/Espacio** confirma y sale; **Escape** cancela; segundo clic icono apaga modo. Registra último comando para Enter global |
| Tras segmento | Sin snap connect: encadena otro tramo. Con **snap connect en clic**: `maStlStopLineToolModesToolbar` (como Escape). Enter/distancia typed confirma tramo y termina. Último comando `#ma-stl-tool-polyline` |

### Importar DWG (barra superior `#ma-stl-tool-dwg-import`)

Botón en el grupo exportar/importar muros de `_Desing2TopToolBar.cshtml` (icono `ri-file-upload-line`). Cliente en `Scripts/Desing2/desing2-dwg-import.js` (script clásico, cargado en `Viewer.cshtml` tras `desing2-wall-exchange.js`); servidor en `Desing_2Controller.ImportDwg` + `Services/Desing2DwgConverter.cs` + `Services/Desing2DxfImportParser.cs`.

| Elemento | Detalle |
|--------|-----|
| Modal | `#ma-stl-dwg-import-modal` (`modal-lg`, en `_Desing2StlViewerWorkspace.cshtml`). Paso 1 `#ma-stl-dwg-import-step-select`: zona de arrastre `#ma-stl-dwg-import-drop`, input `#ma-stl-dwg-import-file` (`accept=".dwg,.dxf"`). Spinner `#ma-stl-dwg-import-busy`. Paso 2 `#ma-stl-dwg-import-summary`: unidades, tamaño, recuentos, vista previa `<canvas>`, capas, muros. Botones Cambiar archivo / Cancelar / Importar |
| Validación | Extensión `.dwg` / `.dxf`, tamaño > 0 y ≤ `data-ma-dwg-max-mb` (45 MB, `dwgImportMaxMb`; `httpRuntime maxRequestLength` = 50 MB), DWG con cabecera `AC10xx` |
| Análisis | Al elegir archivo: `POST Desing_2/ImportDwg` (multipart `file`, opcional `toMeters`). Temporales en `%TEMP%\Desing2DwgImport\{guid}` (se borran siempre). DWG → DXF ACAD2018 ASCII con **ODA File Converter** (`Desing2:OdaFileConverterExe` en Web.config; vacío = busca `C:\Program Files\ODA\ODAFileConverter*\ODAFileConverter.exe`; si no hay ODA usa `MasterArticles:DwgToDxfExe` si existe; timeout `Desing2:OdaFileConverterTimeoutMs`, 120 s). Un `.dxf` se analiza directamente (sin ODA). Respuesta JSON camelCase; errores `errorCode`: `no_file`, `bad_extension`, `oda_missing`, `convert_timeout`, `convert_failed`, `convert_no_output`, `dxf_binary`, `dxf_parse`, `dxf_empty` |
| Unidades | `$INSUNITS` (1 in, 2 ft, 4 mm, 5 cm, 6 m, 14 dm…) con comprobación de tamaño; sin unidades: `$LUNITS` 3/4 ⇒ pulgadas, si no se estiman por el tamaño (planta ≈ decenas de metros, preferencia métrica) y por el grueso de muros detectados. `units`: `code`, `toMeters`, `source` (`insunits` / `insunits-fixed` si con `$INSUNITS` el dibujo mediría < 20 cm o > 50 km y se usa la estimación / `lunits` / `guess` / `override`), `confidence` (`high` / `medium` / `low`), `suggestedCode`. El desplegable de unidades vuelve a analizar con `toMeters` (los umbrales de muros dependen de la escala) |
| Geometría | LINE, LWPOLYLINE, POLYLINE 2D (arcos por bulge en tramos ≤ 10°), INSERT recursivo (posición, escala también negativa, rotación, MINSERT, bloques anidados, OCS/extrusión), capa `0` dentro de bloque hereda la del INSERT. Se omiten capas congeladas / apagadas, `Defpoints` y espacio papel. ARC, CIRCLE, TEXT… se cuentan en `ignored`. Límite 250 000 segmentos (`warnings: segments_truncated`). `segments`: `[x1, y1, x2, y2, capa, grupo, flags]` en unidades del dibujo (`grupo` ≥ 0 = misma polilínea; `flags & 1` = forma parte de un muro detectado) |
| Muros | (a) capas con nombre de muro (`MUR`, `WALL`, `TABIQ`, `PARED`, `CERRAM`, `FACHAD`, `FÁBRICA`, `LADRILL`, `HORMIG`, `PARTI`, `TRASDOS`) ⇒ `wallLike`; (b) pares de líneas paralelas a 5–50 cm con solape ⇒ eje + grosor (`pair-lines`), esquinas en L/T unidas; (c) polilíneas cerradas rectangulares, HATCH y SOLID/TRACE rectangulares ⇒ `polyline-rect` / `hatch-rect` / `solid-rect`. 3DSOLID/BODY, REGION, proxies y objetos `AEC_*` solo se cuentan (`stats.solids3d`, `regions`, `proxies`, `aecWalls`, `aecObjects`) |
| Resumen | Unidades + nota (archivo / estimadas / sospechosas / a mano), tamaño en m, líneas · polilíneas · bloques explotados, tipos no importados, capas con casilla (nº líneas · nº muros, insignia «muros»), «Convertir a muros», «Solo los de capas de muros» (activo si alguna capa de muros tiene muros), «Importar también las líneas que forman esos muros» (por defecto no), recuento final y vista previa (gris = todo, negro = se importa, verde = muros) |
| Importar | Convierte a mm (`toMeters × 1000`) relativo a la esquina mínima del dibujo y llama a `window.maStlDesing2ImportDwgGeometry({ segments, walls })` (viewer.js): la esquina cae en el anclaje de reglas, X → +X, Y del plano → −Z. Líneas con `skipWeld`, `skipCollinearMerge`, `skipGridQuantize` (sin ajuste a rejilla) y un `polylineGroupId` por polilínea; muros con `maStlWall2dToolCommitWallSegmentMm(a, b, true, grosor/2, true)` en un grupo nuevo (`maStlTakeDefaultWallGroupId`), luego cruces, uniones, soldado y estilos como en Recinto. **Un solo paso de deshacer** (`importDwg`) y `maStlSaveWallConnectionsNow` si hay muros |
| Teclado | Con el modal abierto: `keydown` dentro del modal no se propaga (salvo Tab) ⇒ no saltan Supr / Enter / Ctrl+Z / letras del visor; `maStlDesingV2AvoidKeyboardShortcutSteal` devuelve `true` dentro de `.modal.show` y F8 se ignora con un modal abierto. **Esc** cierra el modal (no cancela herramientas del visor) |
| Recursos | `StlPreview_Desing2DwgImport*` en `_gen_masterarticles_resources.js` (ES + EN); los mensajes con parámetros van en `data-ma-dwg-texts` (JSON) del modal |
| Instalar ODA | Descarga gratuita: https://www.opendesign.com/guestfiles/oda_file_converter (Windows 64 bits). Si se instala en otra ruta, indicarla en `Desing2:OdaFileConverterExe`. Sin ODA el modal muestra `StlPreview_Desing2DwgImportOdaMissing` y se puede importar un `.dxf`. **Hosting compartido (TDesing / Atenko):** carpeta portable en `Desing\App_Data\Tools\ODA` (fuera de git, `.gitignore`) con las DLL VC++ 14.x junto al exe; el target `Desing2IncludeOdaFileConverter` de `Design.csproj` la añade a la publicación y `Web.config` / `Web.Release.config` usan `~/App_Data/Tools/ODA/ODAFileConverter.exe`. Temporales en `%TEMP%` o, si no se puede escribir, `App_Data\Temp\Desing2DwgImport`. Los errores devuelven `detail` (exe, código de salida, pista de DLL / permisos) y el modal lo muestra |

### Barra inferior Desing_2 — deshacer, orto, submenú reglas

Parcial: `_Desing2BottomToolBar.cshtml` (`#desing2-stl-bottom-toolbar`). Siempre visible, discreta, anclada **abajo-derecha** del lienzo (sin hover ni pin; brújula arriba-izquierda, panel lateral izquierdo).

| ID | Rol |
|----|-----|
| `#ma-stl-tool-undo` | Deshacer — `ri-arrow-go-back-line`; disabled si `maStlDesing2UndoStack` vacío |
| `#ma-stl-tool-redo` | Rehacer — `ri-arrow-go-forward-line`; disabled si `maStlDesing2RedoStack` vacío |
| `#ma-stl-tool-ortho-15` | Orto 15° en planta (movido desde barra superior); **F8** |
| `#ma-stl-tool-wall-dim` | Acotar — barra azul, junto a variables de los muros. Clic muestra u oculta. Doble clic abre `#desing2-dim-kinds-pop` |
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

### Herramienta cota muro (barra azul `#ma-stl-tool-wall-dim`)

Cotas CAD estilo **construcción**: longitudes de tramo en contorno exterior, cota global si no duplica un tramo, y **espesor** entre pares paralelos (exterior ↔ interior tras offset).

| Elemento | Detalle |
|----------|---------|
| Botón | `#ma-stl-tool-wall-dim` — barra azul, junto a variables de los muros. Icono `ri-pencil-ruler-2-line` |
| Estado | `maStlWallDimToolActive` (`boolean`) |
| Flujo | Clic → cotas de muro → otro clic o **Escape** oculta. Doble clic abre `#desing2-dim-kinds-pop`. La cabecera lo arrastra. En cada muro con encuentro: **Total** (cara exterior a cara exterior), **A ejes** (eje a eje) y **Parcial** (espesor de cada encuentro + tramo entre caras interiores). Un muro solo, sin encuentros, lleva únicamente el total. La parcial de esquina es el panel (`selectPanel` del espesor del otro muro + 0,30), no el espesor. Van en bandas distintas, la parcial la más pegada y el total la más fuera, para poder verse las tres. El resto de cotas (espesor, tramos, global, paneles, remates, alturas) no se pinta de momento. Las casillas filtran estas tres y el id de muro. Se guardan en `sessionStorage` (`desing2.dimKinds`) |
| Longitudes tramo | Solo lados **oeste** y **sur** del contorno exterior (`MA_STL_WALL_DIM_SEGMENT_DIM_SIDES`); no se repiten norte/este (opuestos) |
| Sin redundancia | Si una cota tiene el **mismo valor** que otra (±5 mm), solo se muestra una; la **global superior** se omite si coincide con un tramo inferior |
| Espesor | Una cota por **espesor único** (±5 mm); pares enlazados por offset (`linkOffsetFromLineId`) |
| Offset mínimo | Línea de cota ≥ **1 m** del muro (`MA_STL_WALL_DIM_BASE_OFFSET_MM` = 1000 mm); cotas apiladas +1 m cada una (`MA_STL_WALL_DIM_STACK_STEP_MM`) |
| Visual | Grupo `maStlWallDimGroup` + overlay `#ma-stl-wall-dim-overlay` (metros, `maStlDesing2DimEditableMetersDisplayFromMm`); espesor en azul |
| Sin geometría | Toast `StlPreview_WallDimToolNoPairsToast`; clic en línea de muro reintenta detección |
| Órbita | **No** bloquea órbita |
| Modo | No cambia de modo: en Líneas, Muro 3D o encofrado se queda en el mismo y al apagarla se sigue en él. Las cotas van encima (`depthTest: false`); las de paneles y remates salen del último encofrado (`maStlDesing2LastAtk60PaintPayload`). Desde `5790606` (09/11) el botón pasaba a Líneas y quitaba el encofrado; corregido el 12/11 |
| Muro curvo (13/11) | `maStlWallDimAppendCurvedPlacements` (desde `maStlWallDimCollectAllPlacements`, sin tocar las cotas del recto). **Ejes y diámetro del muro curvo** (`curved-axes`): ejes X y Z por el centro en trazo y punto, sin etiqueta; Ø exterior por arriba (+Z) como la cota total del recto y Ø interior en diagonal; en una porción, R exterior e interior. **Ángulo entre paneles del muro curvo** (`curved-angle`): θ entre los paneles de dos sectores contiguos (líneas radiales del centro al centro de cada panel, arco con flechas, «11,61°»), hacia 135° en planta en el círculo completo y a un cuarto del arco en una porción; en una porción, también el ángulo de inicio desde +X. **Remates**: una sola cota por cara en la primera junta (exterior hacia fuera, interior hacia el centro), con el valor de sus piezas (`PieceWidthMm` más repetido de cada cara) y el formato del recto («0,12 remate»). **Dimensión de Paneles**: un solo panel en planta (sector 2, cara exterior) y una sola cadena de alturas en su canto (2,70 + 1,20); solo paneles del catálogo (sin tapes). Remates y paneles solo con el encofrado pintado. Respeta `_CHeckDimWall`. Geometría de sectores como `CurvedWallFormwork.Place` (θ y remates de `maStlCurvedWallModulationInfo`). Ejes, ángulos y radios: `curvedGuide` (segmentos + flechas trazados en `maStlWallDimRebuildFromPlacements`), `noReadout` en los ejes |

Funciones clave: `maStlWallDimCollectAllPlacements`, `maStlWallDimFilterRedundantPlacements`, `maStlWallDimDetectWallPairsMm`, `maStlWallDimRefreshScanAndDraw`, `maStlStartWallDimToolModesToolbar`, `maStlStopWallDimToolModesToolbar`.

**Acciones registradas (snapshot):** crear segmento línea usuario (`createUserFloorLine`), copia offset paralela (`createUserFloorLine` vía offset), **borrar líneas usuario y/o mallas STL** (`deleteUserFloorLinesAndStlMeshes`), mover extremo P1/P2, mover segmento (asa midpoint en edición cotas), editar cotas (longitud/ΔX/ΔZ), conectar extremo tras snap (clic confirmación), refactor merge colineal (RMB corto), mover anclaje reglas (`rulerAnchor`).

**Extensión:** `window.maStlDesing2PushUndoAction({ label, undo: fn, redo: fn })` para comandos futuros sin tocar el stack interno.

### Insertar Muro Curvo (barra superior `#ma-stl-tool-insert-curved-wall`) — pasos 1 a 5

US del 13/11/2026. Paso 1: pide las medidas. Paso 2: pide el punto central e inserta el muro (planta, Muro 3D, guardar, deshacer). Paso 3: porción del muro (1/1, 1/2, 1/4, 1/33…), ángulo de inicio, columnas propias en BD y modificar con doble clic. Paso 4: **Modulación (panel)** con su columna en BD. Todavía no genera encofrado y **no tiene conexiones**.

| Elemento | Detalle |
|----------|---------|
| Botón | `#ma-stl-tool-insert-curved-wall`, a la derecha de Insertar trabajador (`_Desing2TopToolBar.cshtml`). Icono SVG de dos arcos (`desing2-stl-tool-icon--curved-wall`). Tooltip `StlPreview_Desing2ToolInsertCurvedWallAria` |
| Formulario | Modal Bootstrap `#ma-stl-curved-wall-modal` (`_Desing2StlViewerWorkspace.cshtml`), como el de Elige el trabajador. Campos en metros: Diámetro Exterior, Espesor del Muro y Altura de muro. Debajo se muestra el diámetro interior (diámetro − 2 × espesor). Paso 3: **Porción del muro** (`#ma-stl-curved-wall-portion`: 1/1 (100 %, círculo completo) por defecto, 1/2, 1/3, 1/4 u «Otra»; con Otra se escribe `a/b`, `N` (= 1/N) o un % entero en `#ma-stl-curved-wall-portion-custom`; debajo, % y grados del arco) y **Ángulo de inicio** (`#ma-stl-curved-wall-start`, grados desde +X girando hacia +Z; desactivado con 1/1). Paso 4: **Modulación (panel)** (`#ma-stl-curved-wall-modulation`: 0,30 · 0,45 · 0,60 · 0,75 · **0,90** (por defecto) · 1,05 · 1,20 m; valor en mm); debajo, el reparto del encofrado «N paneles por cara · giro por junta X° · remate exterior A mm, interior B mm» con N = el mayor ≤ ⌊barrido · r / b⌋ con B ≥ 50 mm (`MA_STL_CURVED_WALL_MIN_INNER_REMATE_MM`), giro = barrido / N, A = 2·R·tan(giro/2) − b y B = 2·r·sin(giro/2) − b (`maStlCurvedWallModulationInfo`, mismo cálculo que el servidor; `maStlCurvedWallFillFormworkInfo` añade el aviso `RemateWarning` si un remate pasa de 150 mm o el interior no llega a 50). Aceptar, Cancelar, la X y **Escape** cierran; **Intro** acepta. **Dos pestañas** (13/11/2026): «Muro» (los campos anteriores) y «Ménsulas y puntales»: arriba Ménsulas (`#ma-stl-curved-wall-bracket-in/out` → `_CHeckBracketInside/Outside`), abajo Puntales (`#ma-stl-curved-wall-prop-in/out` superiores → `_CHeckPropInside/Outside`; `-prop-in-inf/out-inf` inferiores → `_CHeckPropInsideInf/OutsideInf`); mismos textos y valores por defecto que el muro recto (sin ménsulas; puntales superiores en las dos caras), lista `MA_STL_CURVED_WALL_SUPPORT_FIELDS`. Debajo, «Por cara: X ménsulas y Y puntales» (`maStlCurvedWallSupportsInfo`, mismo cálculo que `CurvedWallFormwork.ResolveSupports`) y, si la cara interior está marcada y no cabe, el aviso `SupportsNoRoom` (`maStlCurvedWallSupportsWarning`). Al abrir se muestra la pestaña Muro (`maStlCurvedWallShowTab`); con un error de validación vuelve a ella |
| Valores por defecto | Espesor y altura de Configuración (`desing2EnvWallThicknessMm`, `desing2EnvWallHeightMm`). Diámetro: **10,00 m** (`MA_STL_CURVED_WALL_DEFAULT_DIAMETER_M`); después, el último aceptado en la sesión |
| Validación | `maStlCurvedWallValidateMeters`: diámetro 1,00–100,00 m; espesor 0,10–1,00 m y menor que el radio exterior; altura 0,30–6,00 m (el mismo tope que la altura de Configuración). Coma o punto decimal, admite `m` / `mm` (`maStlParseLengthInputValueToMm`). Porción: `maStlCurvedWallParseFraction`, fracción simplificada con 1 ≤ a ≤ b ≤ 360 (`MA_STL_CURVED_WALL_FRACTION_DEN_MAX`); ángulo −360…360 normalizado a 0–360 (`maStlCurvedWallParseAngleDeg`). Mensajes en `data-msg-*` del modal (`data-msg-portion`, `data-msg-start`) |
| Aceptar | Guarda `window.maCurvedWallPending = { outerDiameterM, thicknessM, heightM, outerRadiusM, innerRadiusM, fractionNum, fractionDen, startAngleDeg, modulationMm, supports, createdAt }` (`supports` = las seis casillas `_CHeck*`, `maStlCurvedWallReadSupports`; pasan a la línea al insertar), avisa con un toast («… haz clic en el punto central del muro curvo (Esc cancela)»), lanza el evento `desing2-curved-wall-pending` (`detail` = el mismo objeto) y llama a `maStlCurvedWallBeginPlacement(pending)` |
| Colocación (paso 2) | `maStlCurvedWallBeginPlacement`: cancela otras herramientas, bloquea la órbita y pone cursor en cruz (como el pilar). Un fantasma azul (círculos exterior e interior y cruz del centro) sigue al ratón. El **clic izquierdo es el centro** del muro, ajustado a la rejilla igual que el pilar (`maStlClientRayToWorkspaceFloor` + `maStlQuantizeUserFloorPlanPointToGridMm`). **Escape** cancela (toast «Inserción del muro curvo cancelada»). Un muro por cada Aceptar; el botón queda marcado mientras se coloca |
| Inserción | `maStlPlaceCurvedWallAt(centro, pending)` → `maStlCommitUserPlanLineSegmentMm(centro − R·dir, centro + R·dir, …, { wallRole: 'curved', _DataWith, _DataHeight, _CurveFractionNum, _CurveFractionDen, _CurveModulationMm, skipWeld, skipCollinearMerge, skipGridQuantize })`, con `dir` = dirección del ángulo de inicio. Un paso de deshacer (`createUserFloorLine`) y se guarda como cualquier línea. Toast «Muro curvo insertado: Ø … m, espesor … m, altura … m, porción 1/4 (25 %), panel 0,90 m» y evento `desing2-curved-wall-placed` |
| Representación | **Una línea de planta por muro**: `wallRole: 'curved'`, `_TypeMesh: 'MuroCurvo'`, p1/p2 = extremos del **diámetro exterior** en la dirección del ángulo de inicio (centro ± R·dir; el arco empieza en p2 y barre 360·a/b° de +X hacia +Z). Así `_Datalong` = diámetro exterior, `_DataWith` = espesor (m), `_DataHeight` = altura (m), y el centro es el punto medio (mover/girar siguen funcionando). `maStlWallLineEnsureAttrs` reconoce `wallRole 'curved'` o `_TypeMesh 'MuroCurvo'` y rellena `_CurveCenterX/Y/Z` (mm), `_CurveOuterRadiusMm`, `_CurveFractionNum/Den` (1/1 si falta o no es válida), `_CurveStartAngleDeg` y `_CurveModulationMm` (900 si falta o no está en la lista, `maStlCurvedWallModulationMmOf`); en muros rectos no existen |
| Base de datos (paso 3) | `dbo.TSql_DesignWall` tiene 7 columnas nuevas, todas NULL (NULL en muros rectos): `NumberCurveCenterX/Y/Z` (float, mm), `NumberCurveOuterRadius` (float, mm), `NumberCurveFractionNum` / `NumberCurveFractionDen` (int; 1/33 exacto) y `NumberCurveStartAngle` (float, grados). Script `Desing/Scripts/TemporalScript/2026-11-13_alter_TSql_DesignWall_curved_wall.sql` (solo ADD, idempotente). Paso 4: `NumberCurveModulation` (int, mm; script `2026-11-13_alter_TSql_DesignWall_curved_modulation.sql`); el servidor guarda 900 si llega un valor fuera de la lista y NULL en muros rectos. EDMX + `DAL.TSql_DesignWall`; `DesignWallRepository` (`MapCurveToEntity` / `MapEntityToLine`, DTO `_Curve*`) las guarda y las devuelve. Al cargar, `maStlCurvedWallNormalizeFromServer` rehace p1/p2 desde centro, radio y ángulo y quita los `_Curve*` nulos; un muro curvo guardado antes del paso 3 (columnas a NULL) se carga como círculo completo |
| Planta (Líneas) | `maStlSetUserFloorLineGeometryMm` dibuja la línea `curved` como el **círculo exterior** (96 tramos, `maStlCurvedWallCirclePositionsMm`) o, si la porción es menor que 1/1, como el **sector anular cerrado** (arco exterior, tapa, arco interior, tapa; `maStlCurvedWallOutlinePositionsMm`); el círculo interior (solo con 1/1) y la cruz del centro son guías (`desing2-curved-wall-guides`). Se selecciona con clic dentro del anillo/sector o cerca del contorno (`maStlCurvedWallScreenDistSqPx` en `maStlPickUserFloorLineNearScreenMm`); **Supr** / Borrar lo quitan con deshacer; mover/copiar desplazan el círculo. Sin asas de estirar extremos. **Doble clic** sobre el muro curvo abre el formulario como «Modificar muro curvo» con sus valores (las dos pestañas): en Líneas sobre el anillo en planta; en Muro 3D y encofrado sobre la malla del anillo o sus paneles/tirantes (`maStlCurvedWallPickLine3d`, margen `MA_STL_CURVED_WALL_FORMWORK_PICK_MARGIN_MM` = 400 mm alrededor del anillo vía `maStlCurvedWallMatchPointMm`; si delante hay un muro recto o una pieza de otro muro, lo atiende el popup del recto). `onCanvasDblClickCurvedWallEdit` se registra antes que `onCanvasDblClickWallInspectGlobal` y corta el evento; Aceptar cambia Ø, espesor, altura, porción, ángulo, modulación, ménsulas y puntales manteniendo el centro (`maStlCurvedWallApplyEdit`, un paso de deshacer `editCurvedWall`, toast «Muro curvo modificado…»). Si estaba encofrado, al Aceptar borra el encofrado y vuelve a pulsar Encofrar (`window.maStlDesing2HasFormworkPanels`, como al cambiar la separación de puntales) |
| Muro 3D / Encofrado | Anillo o sector extruido en cliente (`maStlCurvedWallBuildMesh`: forma con agujero si es 1/1; si no, sector cerrado arco exterior → arco interior; radios metidos 6 mm como los muros rectos, altura `_DataHeight`) en `maStlWall3dMeshesGroup` con `maStlWall3dKind 'curved'`, `curvedLineId` y **sin** `_TypeMesh` |
| Sincronía | `maStlCurvedWallSyncFrame()` (en `tick`, solo recorre las líneas `curved`): rehace el círculo de la línea, las guías y los anillos 3D cuando cambian centro/diámetro/espesor/altura/porción/ángulo o tras deshacer, borrar, recargar o regenerar Muro 3D |
| Fuera de los muros rectos | La línea `curved` no entra en: la detección de Muro 3D (`maStlDesing2CollectWallModelSourceLines`; con solo muros curvos/pilares no se llama al servidor), `maStlBuildStraightWallsForFormwork`, `maStlBuildStraightWallsFromScene`, soldadura de extremos, fusión colineal ni límites de la herramienta Muro 2D. **Sin conexiones**: no se suelda ni se une a otros muros; en el JSON de conexiones (`WallConnections`) aparece como `userLine` (solo incidencia de nodos) y el encofrado lo filtra |
| Encofrar (paso 5) | `maStlCollectCurvedFormworkWalls` añade cada muro curvo (no especial, `_IsFormwork` ≠ false) a `maStlDesing2GetStraightWallsFromWallModelSource`, como los pilares: `WallRole 'curved'`, P1/P2 del diámetro y `Attributes` con `_TypeMesh 'MuroCurvo'`, `InicioX/Y/Z`, `FinX/Y/Z` y los `_Curve*`. En `desing2-stl-viewer-toolbar-wiring.js` va en la misma petición `GetWallsAtk60`; con una fase elegida se quita (solo con «Todo»). El servidor lo reparte con `CurvedWallFormwork` (ver `encofrado-atk-60.md` §8) y los paneles, remates, grapas regulables (dos caras) y tirantes (uno por panel, en el agujero del canto de inicio, como el recto) se pintan y cuentan como los del recto; sin rigidizadores de junta. Al pulsar Encofrar sale el toast `FormworkSummary` con el reparto de cada muro curvo |
| Textos | `StlPreview_Desing2ToolInsertCurvedWallAria` y `StlPreview_Desing2CurvedWall*` (incluye `Placed`, `Cancelled`, `FormworkSummary`, `RemateWarning`, `FormPortion*`, `FormStartAngle*`, `PortionInvalid`, `StartAngleInvalid`, `EditTitle`, `Edited`, `FormModulation`, `ModulationInfo`) en `_gen_masterarticles_resources.js` (ES + EN); en el modal como `data-msg-placed`, `data-msg-cancelled`, `data-msg-formwork-summary`, `data-msg-remate-warning`, `data-msg-portion`, `data-msg-start`, `data-msg-edit-title`, `data-msg-edited`, `data-msg-modulation-info` |
| Límites | Encofrado del muro curvo: sin puntales ni ménsulas, tape del extremo perpendicular al panel, fuera de las fases (ver `encofrado-atk-60.md` §8). Editar la cota (longitud) de la línea cambia el diámetro. Se selecciona con clic simple solo en Líneas (en Muro 3D solo el doble clic para modificar). Porción máxima 1/360 de denominador. Un muro recto que toca el anillo no se recorta ni se une a él |

### Formulario Modificar esquina (Muro 3D) — altura

Clic en una pieza de esquina en **Muro 3D** abre el popup `maStlEnsureCornerEditPopup` / `maStlOpenCornerEditPopup` (tipo de solución + **altura**). Mismo patrón que la altura de **Modificar muro** (`wall-edit-form.js`).

| Elemento | Detalle |
|----------|---------|
| Campo | `#ma-stl-corner-height` — **Altura de la esquina (m)**, `type=number` 0,30–10,00 paso 0,15; coma → punto al aceptar. Enter acepta, Escape cierra |
| Valor inicial | La **mayor** altura (`_DataHeight`) de los muros que llegan a la esquina (`maStlCornerEditResolveHeightRange`); si difieren, aviso con el rango (al cambiar se igualan) |
| Qué cambia | La esquina no tiene altura propia: se escribe `_DataHeight` (m, `maStlRoundMeters3`) en **todas las líneas (eje + caras) de cada muro** de la esquina (`maStlCornerEditCollectHeightTargets` → `maStlResolveWallSegmentLinesForLine`) |
| Aplicar a todos | Casilla **Aplicar altura a todos los muros** = misma regla que en Modificar muro (todas las líneas, fija `maStlWallApplyAllHeightMm`) |
| 3D | Cada pieza de esquina se extruye con la altura del muro **más alto** que llega a ella (`maStlAtk60JunctionHeightMm` en `maStlAtk60PlaceJunctionMesh` y punto de referencia de esquina). Antes usaba una altura global (o 2,70 m si los muros diferían) |
| Deshacer / guardar | Altura + tipo en **un solo paso** de deshacer (`editCornerType`); guardado `maStlSaveWallConnectionsNow` (`corner-type-edit`), `_DataHeight` ya viaja en WallConnections y DesignWalls |
| Error | Valor vacío o ≤ 0 → toast `Altura invalida` (igual que el muro) |

**Modificar muro — deshacer altura:** cambiar solo la altura (o **Aplicar altura a todos los muros**) también crea **un** paso de deshacer (`editUserFloorLineDimension`) que restaura las alturas anteriores. Se detecta comparando `id:_DataHeight` de todas las líneas antes/después (`maStlWallInspectHeightSignature`). Si además cambian longitud/espesor/grupo, sigue siendo un único paso.

### Cierre de una L sin partir el muro (2D → Muro 3D)

Un extremo que cae junto al final de otro eje (dentro o un poco pasado) es una **L en ese extremo**, nunca una T.

| Pieza | Regla |
|-------|-------|
| Guarda de extremo | `maStlWall2dToolAxisEndGuardMm(span)` = `maStlWall2dToolJunctionClusterEpsMm()` (1,2 × medio espesor de Configuración, ≈ 132 mm con 0,22), como mucho ¼ del eje. `maStlWall2dToolAxisParamNearEndMm(t, span)` dice si un punto del eje está a esa distancia de un extremo |
| Snap al dibujar / reparar | `maStlWall2dToolSnapPlanPointToNearestAxisBodyMm(p, avoidPtMm)`: a ≤ guarda de un extremo, `p` se une a ese extremo; si no, se proyecta al cuerpo (T) como antes. `avoidPtMm` (el otro extremo del tramo) impide un muro de largo cero |
| Partir ejes | `maStlWall2dToolSplitAxisAtInteriorPointMm` no parte a ≤ guarda de un extremo. `maStlWall2dToolSplitAxesAtInteriorHitsMm` y `maStlWall2dToolSplitAllAxisInteriorCrossingsMm` usan la guarda en mm. Antes el margen era `GRID_MINOR × 0,001` (0,5 mm) y la holgura 1e-4 del cruce lo superaba en muros de más de 5 m: un extremo contaba como interior y salían trozos de pocos mm y ejes de largo cero |
| Curar datos guardados | `maStlWall2dToolHealSpuriousAxisSplitsMm()` al principio de `maStlWall2dToolMergeCollinearAxisRuns` (al confirmar un tramo y al entrar en Muro 3D vía `maStlDesing2PersistCollinearAxisMerge`, que guarda). Quita ejes de largo < 1 mm con sus caras; un trozo colineal ≤ guarda, del mismo muro y espesor, con un extremo libre y en el otro su muro y una sola rama, se une otra vez: el muro recupera su largo y la rama se lleva a ese extremo (L) |
| Sin cambios | T y cruces lejos de los extremos, espesor de las mitades de una T, pieza T enrasada, altura de esquina |

### Punto de inserción modo objeto — fórmula

1. Calcular `box = new THREE.Box3().setFromObject(mesh)` tras `mesh.updateMatrixWorld(true)` (AABB **alineado a ejes mundo**, no OBB local).
2. **Posición:** `(box.min.x, MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM, box.max.z)`.
3. **Justificación convención «inferior izquierda»:** en `maStlBuildPlanRulers` las marcas en dirección −Z se etiquetan como «arriba» en planta; por tanto la arista inferior del rectángulo de huella corresponde a **mayor Z**. La izquierda en planta (brazo +X hacia la derecha) es **menor X**. Es distinto de `(min.x, min.z)`, que sería la esquina superior-izquierda en esa misma convención.
4. **STL:** el formato triangular no incluye punto de inserción DWG; en el flujo maestro artículos / Desing_2 no hay otro campo persistido para este origen — todo deriva del envolvente. Un proveedor extra en `maStlInsertionPointProviders` podría leer metadata futura si existiera.

## Chrome del visor Desing_2 (colores fijos)

El chrome del visor no lee la plantilla activa; usa colores fijos para que la barra y los menús de herramientas sean estables en claro/oscuro.

| Zona | Regla vigente | Archivos |
|------|---------------|----------|
| Barra superior `#master-article-stl-viewer-toolbar.desing2-stl-actions-toolbar` | Fondo fijo `#186cb7`; iconos/textos blancos (`#fff`) | `Desing/Views/Desing_2/Viewer.cshtml` |
| Botones del visor fuera de la barra (`.desing2-stl-view-scope .btn`) | Texto/iconos fijos `#186cb7`; borde outline `rgba(24,108,183,.45)` | `Desing/Views/Desing_2/Viewer.cshtml` |
| Menús de herramientas (`#desing2-article-stl`, `#plugin-blocks-tools`, `.plugin-blocks__tools`, `[id$="toolbar"]`, `[class*="modes-toolbar"]`) | Fondo blanco siempre, también con fondo oscuro; `color-scheme: light`; iconos no activos `#186cb7` | `Desing/Views/Desing_2/Viewer.cshtml`, `Desing/assets/materio/css/site.css` |

Notas: el toggle de fondo oscuro (`#ma-stl-dark-bg-toggle`) solo cambia escena/materiales; el chrome DOM se mantiene claro por las reglas `!important` de `site.css`. No regresar a fondos translúcidos que dejen ver el lienzo oscuro.

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
| `#ma-stl-config-accept` | Botón **Aceptar** arriba del panel. Lee todos los campos (puntales, offset, anchos X/Y, recinto L, W, Ex1, Ex2, Ey1, Ey2) y los guarda en la cookie **`ma_stl_desing2_config`** (`maStlDesing2AcceptConfigForm`). Al arrancar y al restaurar una vista guardada se aplican con `maStlDesing2ApplyAcceptedConfigFromCookie`, que gana a `environment` de la vista. Puntales se guarda además en servidor por diseño. |
| Espesor al dibujar | **Dibujar muro**: la media de X e Y de Configuración (`maStlWall2dToolThicknessHalfMm`); si el tramo conecta con un muro existente, hereda el espesor de ese muro (`maStlWall2dToolResolveConnectedHalfThicknessMm`). **Insertar recinto**: cada muro con su espesor (izquierdo Ex1, derecho Ex2, inferior Ey1, superior Ey2). **Insertar esquina**: X e Y por pata. |
| Unión T/X — espesor del muro atravesado | Al partir el muro atravesado (`maStlWall2dToolSplitAxisAtInteriorPointMm`) las dos mitades conservan **su** espesor (`maStlWall2dToolAxisFaceHalfThicknessMm`), no el de Configuración; antes, con muros de distinto espesor quedaban escalones en caras 2D y en la pieza 3D de la T. Cambiar el espesor de una mitad aplica también a sus tramos colineales del mismo muro (`maStlWall2dToolCollectCollinearSameWallAxisLinesMm`) |
| Unión T — pieza 3D enrasada | `maAtk60BuildTFootprintMm` (`ma-stl-atk60-formwork.js`): los brazos de la pieza T llegan a la cara del muro atravesado por su **espesor real** (`vNear = eRef`). Antes usaban `vShoulder` = panel del vástago (redondeado a 150 mm) − 0,30 m, así que con espesores no múltiplos de 150 (p. ej. 200, 220, 250) los brazos sobresalían 50–100 mm hacia la rama (escalón/ranura en Muro 3D). Largos de brazo/vástago (paneles) sin cambios. Import `?v=20261112-tflush` |
| `#ma-stl-config-offset-default` | Input texto: distancia por defecto del offset (metros en UI, p. ej. `0,30` o `300 mm`). Commit en `change`/`blur` → `desing2EnvOffsetDefaultMm`. Persistencia en cookie `environment.offsetDefaultMm` al pulsar `#ma-stl-save-viewer-state`. |

## Persistencia en cookie (solo Desing_2)

Guardado **explícito** con el botón de barra `#ma-stl-save-viewer-state` (icono `ri-save-3-line`, tooltip i18n «Guardar vista»). No hay auto-guardado al salir ni al mover la cámara.

| Clave cookie | `ma_stl_desing2_viewer_state_global` (misma vista para cualquier oferta/diseño) |
| Contenido | `activeCamera` (`ortho`/`iso`), `cameraOrtho` / `cameraIso` (`position`, `up`, `zoom` si orto), `target` (órbita), `rulerAnchor` (`{x,y,z}`), `environment` (`gridSnapMm`, `rulerExtentCapM`, `offsetDefaultMm`), `toggles` (rejilla, cielo, sombra, fondo oscuro, cortes UI, ejes XYZ, reglas UCS suelo `ucsRulers`, reglas borde `edgeRulers`), sliders de corte X/Y |
| Restaurar | Cookie leída al arranque en `pendingDesing2Restore`. **Una vez** tras el **primer** `refitCamerasToObject` (STL cargado) o tras el arranque sin auto-carga STL. Orden en refit: frustum/rejilla → **omitir** `placeCamerasForModel` si hay restore pendiente o ya aplicado → `bindControls` → `maStlApplyDesing2ViewerStateFromCookie`. Refits posteriores (p. ej. `*2.stl`) **no** resetean cámara (`maStlDesing2StateRestored`). **`state.target` no se restaura** — pivote = `rulerAnchor`. |
| Lectura legado | Si no hay cookie global, se intenta la cookie antigua por `offerId`/`designId` o `ma_stl_desing2_viewer_state` |

Helpers módulo: `maStlDesing2ReadViewerStateFromCookie`, `maStlReadCookie`, `maStlWriteCookie`, `maStlDesing2BuildViewerStateSnapshot`, `maStlApplyDesing2ViewerStateFromCookie`, `maStlDesing2SaveViewerStateToCookie`, `maStlDesing2TryRestoreViewerStateFromCookie`. Arranque STL: evento `ma-stl-desing2-viewer-ready` (evita carrera módulo vs `#desing2-initial-stl-boot`). No aplica al visor de artículos maestro (sin `data-ma-stl-show-rulers-toggle`).
