# HANDOVER GEOMETRIA AT60 L T X (2026-07-11)

Estado: consolidado desde documentacion base + estado real del codigo actual.

## 1) Objetivo de este handover

Dejar en un solo documento todo lo necesario para que otro agente siga trabajando en extrusion y logica geometrica AT-60 sin perder contexto.

Incluye:
- Pipeline 2D -> 3D (vision producto).
- Reglas de negocio del sistema AT-60.
- Estado actual implementado para esquinas L, T y X.
- Decisiones de esta sesion (incluyendo ajustes de cotas en T y reintroduccion de X).
- Riesgos y checklist de validacion.

## 2) Fuentes consolidadas

Documentacion leida:
- Desing/IA/docs/00-resumen.md
- Desing/IA/docs/04-pipeline-2d-a-3d.md
- Desing/IA/docs/ENCOFRADO-AT60-BASE.md
- Desing/IA/docs/ENCOFRADO-AT60-IMPLEMENTACION.md
- Desing/IA/docs/HANDOVER-MUROS-T-CRUCE.md

Codigo fuente consolidado:
- Desing/Scripts/MasterArticles/ma-stl-atk60-formwork.js
- Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js

## 3) Regla madre del proyecto (extrusion)

La geometria 3D NO es una extrusion literal del 2D.

Se trabaja asi:
1. 2D produce topologia (ejes, caras, nodos L/T/Cross).
2. Sistema de encofrado (AT-60) resuelve huellas/piezas por reglas propias.
3. 3D extruye huellas resultantes (rectos API + uniones AT-60 en cliente).

## 4) Reglas AT-60 que gobiernan la geometria

Constantes de catalogo:
- Esquina fija: 0,30 m (300 mm).
- Paso modular: 0,15 m (150 mm).
- Panel minimo: 0,30 m.

Seleccion de panel:
- panelMm = menor multiplo de 150 (desde 300) tal que panelMm >= necesidad.
- woodTacoMm = panelMm - necesidad.

Archivo y simbolos clave:
- maAtk60SelectPanelMm: ma-stl-atk60-formwork.js
- MA_ATK60_CORNER_PANEL_MM = 300
- MA_ATK60_MODULE_STEP_MM = 150

## 5) Estado actual implementado por tipo de esquina

### 5.1) Esquina L (estable)

Implementacion:
- maAtk60BuildLFootprintMm en ma-stl-atk60-formwork.js.
- Entrada por eje con espesores independientes e1/e2.
- Salida poligono 6 vertices + meta.kind = cornerL.

Regla geometrica:
- pata1: need = e1 + 300
- pata2: need = e2 + 300

Consecuencia:
- Soporta anchos mixtos en L (ejemplo 0,25 vertical y 0,30 horizontal) porque calcula cada pata por separado.

### 5.2) Esquina T (estable con ajuste de cotas)

Implementacion:
- maAtk60BuildTFootprintMm en ma-stl-atk60-formwork.js.
- Render y deteccion en bucket de uniones en master-article-details-stl-viewer.js.

Cambios importantes consolidados en esta sesion:
1. Se mantuvo T operativa y se corrigio para cumplir constante de esquina en hombros.
2. Se ajusto el atravesado para que incluya constante de esquina en ancho total:
   - throughPanel usa need = 2*E_through + 300.
3. Se alineo trim de T con la misma formula:
   - maAtk60GetTJunctionTrimMm usa need = 2*E_through + 300.

Efecto esperado (caso E_through = 0,30):
- Distancias de hombro (como T4-T3 y T7-T8) = 0,30.
- Cota total atravesado = 0,90.

Nota:
- branchHalf se mantiene ligado al espesor real de rama (eBranch * 0.5).
- Se conserva vBaseOffset para anclaje de huella T sobre cara exterior del atravesado.

### 5.3) Esquina X / Cross (reinsertada)

Implementacion:
- maAtk60BuildCrossFootprintMm en ma-stl-atk60-formwork.js.
- Clasificacion cornerX por vertexCount >= 12.
- Deteccion topologica en viewer con bucket de ejes:
  - maStlAtk60TryBuildCrossFootprintAtJunction
  - maStlAtk60TryRenderJunctionAtBucket

Regla geometrica aplicada para cumplir constante 0,30:
- Para cada eje:
  - half = E/2
  - span = half + 300
- Es decir, desde cara de muro a punta de brazo hay 300 mm por lado.

Meta generada en X:
- kind: cornerX
- cornerConstMm: 300
- spanUMm, spanVMm
- pointLabels X-1..X-12

Guia visual X:
- maStlAtk60AddXFootprintPointGuide en viewer.
- Solo se dibuja en render 2D del modo muro planta.

## 6) Integracion 2D/3D vigente en viewer

Archivo: master-article-details-stl-viewer.js

Puntos clave:
- Clasifica corner L/T/X para color, renderOrder y pieceKind de extrusion.
- Trims de rectos alrededor de uniones se construyen con:
  - maStlAtk60BuildAxisEndpointTrimMap
  - maStlAtk60RayPolygonFirstHitMm
- Para T:
  - Usa direccion canonica del atravesado para evitar espejos por orden de ejes.
- Para X:
  - Detecta 2 pares colineales opuestos perpendiculares.
  - Calcula trims desde interseccion rayo-huella para cada brazo.

## 7) Estado funcional resumido (hoy)

- L: funcional y estable.
- T: funcional y ajustada a constante 0,30 en hombros y 0,90 total en caso E=0,30.
- X: reinsertada, detectada y renderizada en 2D/3D con hombro 0,30 por brazo.

## 8) Riesgos tecnicos a vigilar

1. Cruces no ortogonales:
   - maAtk60BuildCrossFootprintMm rechaza si dot(u,v) no es casi perpendicular.
2. Segmentos muy cortos o nodos mal particionados:
   - Puede degradar deteccion de T/Cross en bucket.
3. Orden topologico de ejes:
   - Aunque hay normalizacion en T, revisar casos raros con split interior.
4. Solapes visuales de etiquetas:
   - T/X usan offset radial; puede requerir tuning por caso.

## 9) Casos de prueba minimos recomendados

### L
1. E1=0,25 y E2=0,30 (asimetrica).
2. E1=E2=0,2345 para validar panel/taco.

### T
1. E_through=0,30 y E_branch=0,30.
   - Verificar hombros 0,30 y total 0,90.
2. E_through=0,45 y E_branch=0,25.
   - Verificar seleccion de panel en meta.

### X
1. Cruz ortogonal con E iguales (0,30 en los 4 brazos).
   - Verificar hombro 0,30 en los 4 lados.
2. Cruz ortogonal con espesores mixtos por eje (ejemplo U=0,25 y V=0,40).
   - Verificar que span por eje respeta half(E)+0,30.

## 10) Siguientes pasos sugeridos para otro agente

1. Congelar tests visuales con capturas en IA/Examples para L/T/X.
2. Añadir acotacion especifica de X en maAtk60BuildFootprintDimPlacements (hoy sin cota dedicada de X).
3. Afinar posicion de etiquetas X para evitar cruces con R-* en layouts densos.
4. Si se requiere robustez industrial, incorporar validadores de ortogonalidad y longitudes minimas antes de extruir.

## 11) Mapa rapido de funciones (codigo actual)

ma-stl-atk60-formwork.js
- maAtk60BuildLFootprintMm
- maAtk60BuildTFootprintMm
- maAtk60BuildCrossFootprintMm
- maAtk60GetTJunctionTrimMm
- maAtk60ClassifyFootprintKind

master-article-details-stl-viewer.js
- maStlAtk60TryBuildCrossFootprintAtJunction
- maStlAtk60BuildAxisEndpointTrimMap
- maStlAtk60AddTFootprintPointGuide
- maStlAtk60AddXFootprintPointGuide
- maStlAtk60TryRenderJunctionAtBucket

---

Si se retoma trabajo de geometria, usar este documento como fuente primaria y solo despues complementar con los handovers historicos de Desing/IA/docs.
