# Desing_2 — Dibujo 2D: herramientas del visor

> Parte de `desing-2-stl-viewer-funciones.md`, partido por áreas el 14/11/2026 sin quitar contenido. Las otras partes: [visor](../desing2-visor/visor-funciones.md), [3d](../desing2-muros-3d/muros-3d-visor.md), [ia](../ia-asistentes/ia-en-el-visor.md).

### Herramienta polilínea (barra superior `#ma-stl-tool-polyline`)

Única herramienta de trazado de segmentos en planta (no hay botón línea separado).

| Elemento | Detalle |
|----------|---------|
| Botón | `#ma-stl-tool-polyline` — icono `ri-edit-2-line` (lápiz con línea; hasta el 14/11/2026 `ri-route-line`, igual que el modo Líneas `#ma-stl-mode-lines`, que lo conserva) |
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
| Muro: alargar | `maStlLineEditToolExtendWallAtClient` (el cálculo está en `maStlLineEditToolExtendWallEnd(axis, extendP2)`, que también usa la IA del diseño; mismo comportamiento): extremo **libre** más cercano al clic hasta el siguiente eje de muro, línea o pilar (mismos límites que recortar: `maStlLineEditToolWallCutParams`); al llegar a otro muro el post-proceso crea la unión en T. Un extremo ya unido a otro muro no se alarga (toast) |
| Testa a ras de línea | `maStlWall2dToolApplyFreeEndLineCutsMm()` (al final de `maStlWall2dToolRefactorAllWallJunctionsMm`): un extremo libre de eje que cae (≤ 2 mm) sobre una línea simple que llega a la huella, con más de `MA_STL_WALL_FREE_END_LINE_CUT_MIN_DEG` (10°) entre ambos, corta cada cara en su cruce con la línea en vez de la testa recta. Línea perpendicular: sin cambios. El muro 3D sale de las caras y la sigue; el encofrado (servidor) usa el eje y no cambia. Si luego se borra o mueve la línea, la testa vuelve a ser recta la próxima vez que se rehagan las caras de ese tramo |
| Encofrado | Si al recortar/alargar un muro había encofrado (`maStlDesing2HasFormworkPanels`), se quita (`maStlDesing2ClearAtk60Visuals`) y, al terminar el comando (Enter/Espacio/Esc), se re-encofra pulsando `#ma-stl-mode-formwork`, como al modificar el muro curvo; no si se pasa a otra herramienta |
| Undo | Un snapshot por operación (`trimUserFloorLine` / `extendUserFloorLine` / `trimUserFloorWall` / `extendUserFloorWall` / `deleteUserFloorLinesAndStlMeshes`) |
| Escape | `maStlStopLineEditToolModesToolbar()` (también desde `maStlDesing2CancelTransientToolsEscape` y `maStlCancelAllViewerInteractionModes`): limpia hover, HUD, cursor y bloqueo de órbita; seguro si no está activa |
| HUD | `#ma-stl-trim-tool-hud` / `#ma-stl-extend-tool-hud` (`StlPreview_TrimToolInstruction`, `StlPreview_ExtendToolInstruction`, `*NoChangeToast`, `StlPreview_LineEditToolUnsupportedToast`) |
| Teclado | `maStlLineEditToolApplyWindowKeydownConfirm()` en `_maStlDesingV2LineToolEnterActivateKeyHandler` |
| Cámara | Mismo lock que polilínea (`maStlLockOrbitForRulerAnchorPick`); Escape / segundo clic icono cancela |

### Girar y Simetría (barra superior `#ma-stl-tool-rotate` / `#ma-stl-tool-mirror`, 14/11/2026, `?v=20261114k`)

Dos herramientas nuevas en el grupo de Copiar / Desplazar de `_Desing2TopToolBar.cshtml`, como ROTATE y MIRROR de AutoCAD. Son dos tipos más de la máquina de estados de Copiar/Desplazar/Estirar (`maStlCopyToolKind` = `'rotate'` / `'mirror'`; fases `selecting` → `basePoint` → `displacement`), así que comparten selección, HUD, orto F8, Esc e Intro.

| Paso | Girar | Simetría |
|------|-------|----------|
| 1. Selección | Clic en muros o líneas (o mallas STL), o selección por ventana/cruce; **Intro** para seguir. Un muro arrastra su eje, sus caras y el resto de su polilínea (`maStlDesing2ExpandLinesWithWallGroups`) | Igual (las mallas STL y los muros curvos no se reflejan) |
| 2. Punto | Punto base: clic (con imanes) o **X;Z** tecleado + Intro | Primer punto del eje de simetría: igual |
| 3. Segundo dato | **Ángulo**: clic (la recta base → ratón da el ángulo; con orto F8 en pasos de 90°), grados tecleados («90», «-30») o X;Z. + = antihorario en planta. El HUD enseña «≈ 90°» y una vista previa morada | **Segundo punto del eje**: clic, X;Z o un ángulo en grados (la recta pasa por el primer punto con ese ángulo; «90» = vertical). Vista previa rosa |
| Opción | Casilla «Mantener original (copia) [C]» (tecla **C**). Por defecto gira en su sitio | Casilla «¿Borrar originales? [B]» (tecla **B**). Por defecto **mantiene** los originales y crea los reflejados, como MIRROR |
| Final | Un solo paso de Deshacer (`rotateUserFloorLinesAndStlMeshes`); la herramienta se cierra; Intro sin herramienta la repite | Igual (`mirrorUserFloorLinesAndStlMeshes`) |

| Elemento | Detalle |
|----------|---------|
| HUD | `#ma-stl-rotate-tool-hud` / `#ma-stl-mirror-tool-hud` en `_Desing2StlViewerWorkspace.cshtml` (instrucción, coordenadas, campo de grados/punto, vista previa y casilla `-option`). Textos en el `.cshtml` (ES, `data-ma-stl-*-tool-instruction-*`) y en inglés desde el JS (`maStlXformToolApplyUiTexts`) si `<html lang="en">`. Sin `.resx` nuevos |
| Transformación | `maStlXformMakeRotate(cx, cz, grados)` (x' = cx + dx·cos + dz·sin, z' = cz − dx·sin + dz·cos) y `maStlXformMakeMirror(a, b)`; `maStlXformMapPointMm` redondea a 1e-4 mm. Las mallas STL giran alrededor de Y (`quaternion.premultiply`) |
| En su sitio | `maStlXformToolCommitMm(xf, false)`: mueve p1/p2 (y `wallDrawP1/P2`) de cada línea y los ajusta a la rejilla; deja el eje en su sentido canónico (`maStlWall2dToolEnsureClockwiseAxisEndsMm`: hacia +X, o +Z si es vertical). Si se cambian los extremos, se cambian también `_Tape_1/_Tape_2`, `_CornerTypeP1/P2`, `_ChekSolapeIni/End` y `_Idconnection_1/2` (`maStlXformSwapWallEndAttrs`) y el signo `numberWallFaceSideSign` de las caras, que también se invierte en la simetría; así cada cara sigue en su lado. Las aperturas de cara (`faceGapsMm`) se transforman con su cara y se normalizan. Después, el mismo post-proceso que Desplazar (cruces, uniones, conexiones, cotas) y guardado `rotate-tool-commit` / `mirror-tool-commit` |
| Con copia | `maStlCopyToolCommitDuplicateMappedMm(partes, mallas, mapPt, …)`, el núcleo de Copiar generalizado (Copiar lo usa con una traslación, sin cambios): grupos nuevos, sin aperturas ni atributos de extremo |
| Ángulo 0 | Girar 0° (o 360°) no hace nada |
| IA | Las mismas funciones desde la IA del diseño: `bridge.rotateWall` / `bridge.mirrorWall` (ver [`ia-del-diseno-girar-catalogo-entornos.md`](../ia-asistentes/ia-del-diseno-girar-catalogo-entornos.md) §16.9) |
| Barra | Con los dos botones, la barra sigue sin tocar el cubo de vistas (comprobado a 1600, 1366, 1280 y 1024 px); si no cabe, pasa a la segunda fila como antes. `site.css` sin cambios |

### Barra superior Desing_2 — herramientas de dibujo y repetir último comando (Enter)

Parcial: `_Desing2TopToolBar.cshtml` (`#desing2-stl-top-toolbar`). Siempre visible, discreta, anclada **arriba-centro** del lienzo (mismo patrón CSS que la barra inferior: `desing2-stl-floating-toolbar`).

| ID | Rol |
|----|-----|
| `#ma-stl-tool-polyline` | Polilínea — `ri-edit-2-line` (antes `ri-route-line`, el mismo que el modo Líneas) |
| `#ma-stl-tool-offset` | Offset paralelo — `ri-shape-line` |
| `#ma-stl-tool-delete` | Borrar líneas usuario y objetos STL — `ri-delete-bin-line` |
| `#ma-stl-tool-rotate` | Girar (después de Desplazar) — `ri-anticlockwise-2-line`. Ver «Girar y Simetría» |
| `#ma-stl-tool-mirror` | Simetría (después de Girar) — `ri-flip-horizontal-2-line`. Ver «Girar y Simetría» |

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

Parcial: `_Desing2BottomToolBar.cshtml` (`#desing2-stl-bottom-toolbar`). Siempre visible, discreta, anclada **abajo-derecha** del lienzo (sin hover ni pin; panel lateral izquierdo; la brújula se quitó el 14/11/2026).

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
| Alcance (15/11, pt117u) | **Por defecto, solo el muro seleccionado** (`maStlWallDimScope`): con un muro seleccionado (clic en su línea en Líneas, clic en su sólido en Muro 3D o «selecciona el muro 7» de la IA) se ven solo sus cotas y su ID, y cambian al seleccionar otro (`maStlOnWallSelectionChanged`). Al abrir Cotas se mantiene la selección. **Sin selección**: solo el «ID n» de cada muro, para elegir uno (sin aviso de «no hay pares»). **Todo el diseño**: casilla «Todo el Diseño» (`all-walls`, la primera del menú, apagada, se guarda en la cookie) o «acota todo» de la IA (`maStlWallDimAllOverride`, hasta apagar Cotas). Filtro: `maStlWallDimAiFocusKeepsAxis` (cotas de eje, ID de muro, curvo) y `maStlWallDimKeepsWallKey` (paneles, remates y alturas del encofrado: `IdWall` = id del eje). Esc quita la selección y apaga Cotas, como antes |
| Id de Artículo (15/11) | Casilla `article-id` (apagada por defecto): cada cota de panel lleva el ID del panel, «0,90 · 7.3» (muro 7, 3.ª columna). Ver [`cota-id-de-articulo.md`](cota-id-de-articulo.md) |
| Muro curvo (13/11) | `maStlWallDimAppendCurvedPlacements` (desde `maStlWallDimCollectAllPlacements`, sin tocar las cotas del recto). **Ejes y diámetro del muro curvo** (`curved-axes`): ejes X y Z por el centro en trazo y punto, sin etiqueta; Ø exterior por arriba (+Z) como la cota total del recto y Ø interior en diagonal; en una porción, R exterior e interior. **Ángulo entre paneles del muro curvo** (`curved-angle`): θ entre los paneles de dos sectores contiguos (líneas radiales del centro al centro de cada panel, arco con flechas, «11,61°»), hacia 135° en planta en el círculo completo y a un cuarto del arco en una porción; en una porción, también el ángulo de inicio desde +X. **Remates**: una sola cota por cara en la primera junta (exterior hacia fuera, interior hacia el centro), con el valor de sus piezas (`PieceWidthMm` más repetido de cada cara) y el formato del recto («0,12 remate»). **Dimensión de Paneles**: un solo panel en planta (sector 2, cara exterior) y una sola cadena de alturas en su canto (2,70 + 1,20); solo paneles del catálogo (sin tapes). Remates y paneles solo con el encofrado pintado. Respeta `_CHeckDimWall`. Geometría de sectores como `CurvedWallFormwork.Place` (θ y remates de `maStlCurvedWallModulationInfo`). Ejes, ángulos y radios: `curvedGuide` (segmentos + flechas trazados en `maStlWallDimRebuildFromPlacements`), `noReadout` en los ejes |

Funciones clave: `maStlWallDimCollectAllPlacements`, `maStlWallDimFilterRedundantPlacements`, `maStlWallDimDetectWallPairsMm`, `maStlWallDimRefreshScanAndDraw`, `maStlStartWallDimToolModesToolbar`, `maStlStopWallDimToolModesToolbar`.

**Acciones registradas (snapshot):** crear segmento línea usuario (`createUserFloorLine`), copia offset paralela (`createUserFloorLine` vía offset), **borrar líneas usuario y/o mallas STL** (`deleteUserFloorLinesAndStlMeshes`), mover extremo P1/P2, mover segmento (asa midpoint en edición cotas), editar cotas (longitud/ΔX/ΔZ), conectar extremo tras snap (clic confirmación), refactor merge colineal (RMB corto), mover anclaje reglas (`rulerAnchor`).

**Extensión:** `window.maStlDesing2PushUndoAction({ label, undo: fn, redo: fn })` para comandos futuros sin tocar el stack interno.
