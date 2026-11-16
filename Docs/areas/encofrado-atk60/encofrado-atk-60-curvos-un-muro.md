# Encofrado ATK-60 — Muros curvos, encofrado de un solo muro, paneles que no hay (`_PanelStock`) y cambio guiado (`_PanelReplace`)

> Parte de [`encofrado-atk-60.md`](encofrado-atk-60.md) (partido el 15/11/2026 sin quitar contenido; la numeración de secciones es la original). Área: [`INDEX.md`](INDEX.md).

## 8. Muros curvos (13/11/2026)

Lo que hay hoy (pasos 1 a 5): el botón Insertar Muro Curvo pide diámetro exterior (10,00 m por defecto), espesor, altura, **porción** (1/1 = círculo completo, 1/2, 1/4, 1/33…), ángulo de inicio y **modulación** (ancho de panel 0,30–1,20 m); después el clic en el plano es el **centro** y se inserta el muro (ver `desing-2-stl-viewer-funciones.md`). Paso 5: **Encofrar** lo reparte en paneles planos (más abajo).

Cómo se guarda el muro curvo (lo que tendrá que leer el encofrado):
- Una sola línea de planta con `wallRole: 'curved'` y `_TypeMesh: 'MuroCurvo'`. P1/P2 = extremos del diámetro exterior (centro ± R, mismo Z): centro = punto medio, R = `_Datalong` / 2, espesor = `_DataWith`, altura = `_DataHeight` (m). Radio interior = R − espesor. Se guarda en DesignWalls como las demás líneas.
- Paso 3, columnas propias en `dbo.TSql_DesignWall` (NULL en muros rectos): `NumberCurveCenterX/Y/Z` y `NumberCurveOuterRadius` (mm), `NumberCurveFractionNum` / `NumberCurveFractionDen` (int) y `NumberCurveStartAngle` (grados, de +X hacia +Z). Barrido = 360 · Num / Den; el arco va de StartAngle a StartAngle + barrido. P1/P2 se siguen guardando (diámetro en la dirección de inicio). Con una porción menor que 1/1 el encofrado tendrá dos **tapes** rectos en los extremos del arco (de ancho = espesor).
- Paso 4, **modulación**: `NumberCurveModulation` (int, mm) = ancho del panel plano con el que se encofrará el muro curvo: 300, 450, 600, 750, 900 (por defecto), 1050 o 1200. NULL en muros rectos y en muros curvos anteriores (la app usa 900). El formulario muestra el reparto del paso 5: paneles por cara, giro por junta y remates (Ø 10 m, e 0,30, 0,90 m: 31 paneles, 11,61°, remates 116,9 / 51,0 mm), con aviso si un remate pasa de 0,15 o el interior no llega a 0,05.
- **Sin conexiones:** el muro curvo no se une a muros rectos ni a otros curvos (sin soldadura, fusión ni detección).
- Se eligió un elemento único y no un polígono de ejes rectos: un polígono haría que la detección de muros y ATK-60 vieran decenas de muros cortos con esquinas obtusas que hoy no saben resolver (encofrado basura). Con un elemento único se excluye fácil y el encofrado curvo podrá generarse aparte con la regla de paneles de abajo.
- Encofrar: el muro curvo sigue fuera de la detección de Muro 3D y de los muros rectos (`maStlBuildStraightWallsForFormwork`, `maStlBuildStraightWallsFromScene`; su anillo 3D no tiene `_TypeMesh`), pero desde el paso 5 viaja aparte (`maStlCollectCurvedFormworkWalls`) y lo encofra el servidor. Muros rectos, L, T y pilares siguen igual. Un muro recto que toca el anillo no se recorta contra él.

Qué dice el manual NEVI (Guía del Usuario, 89 págs.):
- No tiene ningún apartado de muros curvos o circulares, ni radio mínimo, ni paneles curvos. Este documento tampoco tenía nada.
- Lo más cercano es §3.4 «Esquinas distintas a 90°»: se resuelven con la **escuadra giratoria** (dos chapas simétricas con eje bisagra; alturas 2,7 / 2,4 / 1,2 m; sin agujeros para barra roscada; sirve para la cara interior y la exterior).
  - 75°–85°: escuadra G dentro y fuera. Dentro, gancho rigidizador + placa excéntrica + tuerca hexagonal 15 y compensación de al menos 5 cm (taco de madera) en un lado. Fuera, grapas fijas y compensación para poder poner tirantes. En 2,7 m: 3 uniones dentro, 5 grapas fijas fuera y 2 rigidizadores Comain 0,75 fuera.
  - 85°–180°: 5 grapas fijas (2,7 / 2,4 m; 3 en 1,2 m) dentro y fuera. Rigidizadores Comain 0,75 fuera siempre y dentro a partir de 100°; 2 en 2,7 m.
- Compensaciones: 0–5 cm (grapa regulable), 5 cm, 5–18 cm y 8–30 cm.

Paso 5, **encofrado del muro curvo** (13/11/2026, boceto del usuario; revisado a las 11:45 con la captura del usuario):

- **Regla.** Sectores N = el mayor número, como mucho floor(porción · 2π · R_int / b), con el **remate interior de al menos 50 mm** (`MinInnerRemateMm`); el exterior queda el que salga. Giro por junta θ = porción · 360° / N, con b = modulación. En cada sector, un panel b en cada cara, enfrentados y centrados en la bisectriz: el exterior tangente al arco exterior (polígono circunscrito, lado 2·R·tan(θ/2)) y el interior en la cuerda del arco interior (polígono inscrito, lado 2·r·sin(θ/2)). Así el hormigón no baja nunca del espesor.
- **Juntas.** Remate de madera en cada junta y cara, centrado en el vértice, con sus grapas regulables en las dos caras (`EndFiller`, tope 0,15; 10000221 + 10000221B a 450 / 1 350 / 2 250): exterior = 2·R·tan(θ/2) − b, interior = 2·r·sin(θ/2) − b. **Sin rigidizadores de junta** (cada sector es un solo panel; la primera versión los ponía y salían girados).
- **Cada sector es un tramo recto de un panel** (`Atk60ModuleAssembly.Build`): panel, grapas y **tirantes** que atraviesan el muro del panel exterior a su panel interior (barra `230xxx` según el espesor del sector R − r·cos(θ/2) + 490, placa 10443020 a 120 mm de cada cara). Posición = la del muro recto (`FormTie.Place`, agujero a 43,7 mm del canto, a 550 y 2 150 en 2,70; 350 / 850 en 1,20), **en los dos cantos de cada panel**: cada junta lleva remate de madera sin rigidizadores, y el manual NEVI (3.2.3, p. 43) pide «tirantes a ambos lados de la compensación» cuando los rigidizadores no solapan el panel. Así hay un tirante a cada lado del remate (dos por junta). En el extremo libre del arco manda su tape, como en el muro recto (`ExactWidthEndTape.SkipsPanelEndTie`). En los sectores sin extremo libre se pasa `tiesBeyondWall` para que la regla de muro < 0,30 (el tablón de cierre quita el tirante a < 100 mm del extremo) no quite los de las juntas. Los paneles exterior e interior del sector son iguales y están centrados en la bisectriz, así que el tirante pasa por los dos agujeros enfrentados. (Historia: v1 en los dos cantos; v2 uno por panel; v3, 13/11 11:52, otra vez en los dos cantos por la captura del usuario.) Rigidizador de junta horizontal solo si hay paneles apilados (muros de más de 2,70), como en el recto.
- **Alturas de más de 2,70 (13/11/2026, captura del usuario a 3,00: «no inserto los tirantes con lo que los paneles se caen»).** Con modulación 0,30–0,90 los paneles se apilan de pie como en el recto (`Modulo270HeightPanelCatalog.BuildNarrowModuleVerticalStack`, NEVI 3.1.2): 3,00 / 3,30 → 2,70 + 1,20 (borde alto 3,90), 4,00 → 2,70 + 2,40 (5,10), 5,40 → 2,70 + 2,70. `CurvedWallFormwork` pasa `stackedPanelTies` a `Atk60ModuleAssembly.Build` (por defecto apagado: el recto no cambia): `FormTie.Place` pone los tirantes de cada nivel a sus alturas hasta el borde alto del panel y no hasta el alto del muro (3,00: 0,55 / 2,15 / 3,05 / 3,55), en los dos cantos; `PanelClamp.PlaceStackedSeam` pone la grapa fija horizontal 10004220 (giro −π/2, como la junta de un tumbado) en la junta entre niveles, en las dos caras, a 200 mm de cada canto (a ¼ del panel si es de menos de 0,60). El rigidizador 1850162 de la junta horizontal va vertical con sus ganchos a ±200, igual que el recto. El remate sube al borde alto (`FormworkTopMm`) con sus grapas regulables a las alturas de `EndFillerHeightsMm` (3,90: 0,45 / 1,35 / 2,25 / 2,90 / 3,40). Ménsulas y puntales siguen con el alto del muro. Los paneles de arriba llevan el mismo giro que los de abajo (GLB 12904215 y 27904209 con origen 0, de pie). **Ø 10 / 0,22 / 3,00 / 0,90:** 124 paneles (62 de 2,70 + 62 de 1,20), 248 tirantes y 620 placas, 124 grapas 10004220, 62 rigidizadores y 124 ganchos, 62 remates de 3,90 con 310 + 310 grapas regulables. Pendiente: el muro recto con módulo estrecho de más de 2,70 tiene el mismo hueco (no se ha tocado); el tape del extremo de un arco parcial sigue al alto del muro.
- **Porción menor que 1/1.** Los paneles de los extremos van a ras del final del arco en la cara interior y el extremo se cierra con el tape del muro recto (`ExactWidthEndTape`, `_Tape_1` / `_Tape_2`), elegido con el espesor real del muro.
- **Datos.** El cliente envía el muro curvo con `_TypeMesh 'MuroCurvo'`, `_DataWith`, `_DataHeight` y `_Curve*` (centro, radio exterior, porción, ángulo de inicio, modulación). `CurvedWallFormwork.IsCurved` lo separa en `SolveFromIdsJson` antes de contar uniones y recortar; los elementos se añaden a `ElementsForThreeJs` y salen en la lista de materiales como los del recto. Sin cambios en BD.
- **Ejemplo** Ø 10,00, e 0,30, h 2,70, b 0,90, 1/1: con 32 sectores el remate interior sería 21,4 mm (< 50), así que N = **31**, θ = 11,61°; lado exterior 1 016,9 → remate exterior **116,9 mm**; lado interior 951,0 → remate interior **51,0 mm**; espesor del sector 324,1 mm → barra 230090. Piezas: 62 paneles 0,90 × 2,70 (31 por cara), 62 remates (31 por cara), 186 + 186 grapas regulables (3 alturas en cada remate, las dos caras), **124 tirantes** (31 paneles × 2 cantos × 2 alturas) con 248 placas, sin rigidizadores. Con 1/2: 15 sectores, 30 paneles, 28 remates, 56 tirantes (28 cantos de junta × 2 alturas; los dos cantos del extremo los resuelve el tape) y 2 tapes.
- **Ménsulas y puntales (13/11/2026, 12:15).** Mismos atributos, columnas y piezas que el muro recto: `_CHeckBracketInside/Outside` (`Is_CheckBracket*`), `_CHeckPropInside/Outside` (puntales superiores) y `_CHeckPropInsideInf/OutsideInf` (inferiores) en `TSql_DesignWall`, y `_PropSpacingMm` de Configuración; sin cambios en BD. Cara exterior = fuera del anillo (normal del sector); interior = hacia el centro. **Cantidad** = la del muro recto sobre el largo desarrollado de la cara exterior L = N · 2·R·tan(θ/2): ménsulas 2 por tramo de 2,40 m (resto ≥ 1,20 → 2; ≥ 0,60 → 1; `WorkingBracket.StationCount`), puntales ⌈L / separación máx.⌉ (2,40 m por defecto; `PushPullProp.MaxSpacingMm`), solo con muro de 2,00 m o más (como el recto). Como mucho uno por sector, repartidos a paso constante (sector ⌊(k + ½)·N / n⌋), sin duplicados ni huecos dobles. **Posición** en el sector (tramo recto de un panel), perpendicular al panel (radial): ménsula en el centro del panel (b/2), a altura − 300; puntal en el alojamiento a 50 mm del canto de inicio (NEVI 2.9), mismas piezas por altura (`PartsForHeight`). Sin madera de protección (solo la lleva el primer muro recto). **Cara interior**: el pie va hacia el centro; cabe si r·cos(θ/2) − 120 − vuelo > 0 y la cuerda entre vecinos es ≥ 0,40 m (vuelo ménsula 1,0 m; puntal 1,6 m del h270 o 0,6 · altura). Si no cabe, no se pone y el formulario y Encofrar avisan (`SupportsNoRoom`). Código: `CurvedWallFormwork.ResolveSupports`, `WorkingBracket.PlaceAt`, `PushPullProp.PlaceAt` (nuevos, el recto no cambia). **Ø 10 / 0,30 / 2,70 / 0,90:** L = 31 524 mm → **26 ménsulas** y **14 puntales** por cara (cada puntal: cuerpo HTipo1 + tubo H0_2270; con inferior, H0_237 + H0_203); interior cabe (pie del puntal a 2,94 m del centro). Con 1/2: 14 y 7. Ø 4 con 1,20: 8 y 6 por fuera; por dentro no caben.
- **Límites.** Tape del extremo perpendicular al panel, no radial; elegido con el espesor real (0,30) mientras los paneles del sector extremo están a 324,1 mm (24,1 mm de diferencia en la cara interior del extremo). Tirantes perpendiculares a los dos paneles (paralelos a la bisectriz), no estrictamente radiales. Ménsulas y puntales a una por sector como mucho (con modulación pequeña y Ø grande se reparten igual) y vuelo estimado para la comprobación de la cara interior. Remate de más de 0,15 (Ø pequeño con panel ancho, p. ej. Ø 4 con 1,20: exterior 456,9 mm): `EndFiller` lo topa en 0,15 y el formulario y Encofrar avisan. Muro de menos de 0,30 en porciones: en el primer y último sector la regla del tablón de cierre también quita el tirante del canto de la junta (pendiente si se usa). Más de 90° por sector o sin sitio para un panel (1/33 de Ø 10): no se encofra. Fases: el curvo solo entra con «Todo».
- **Verificación.** Banco .NET con el código del servidor: 74 casos de regresión (L, T, X, rectos, tapes, variantes) idénticos byte a byte; recto + curvo en la misma petición = recto solo + curvo solo. Planta y 3D renderizados desde la salida del servidor.

## 9. Encofrado de un solo muro y paneles que no hay (13/11/2026, Asistente IA)

### 9.1 Calcular solo un muro (PASO 1)

Cuando el Asistente IA pide el material de un muro que no tiene piezas pintadas (diseño sin encofrar, o muro nuevo o modificado después del último Encofrar), ya **no se encofra todo el diseño**: se calcula **solo ese muro** y **no se pinta nada**.

- **Cliente** (`desing2-stl-viewer-toolbar-wiring.js`): `window.maStlDesing2PreviewWallFormworkAtk60(keys, axisMm, { attrs })`.
  - Arma el mismo `IdsJson` que Encofrar (`buildAtk60IdsDetailed`, extraído de `wireFormworkButton` sin cambiar su resultado).
  - Elige el muro por `Id` / `LineId` / `_idObject` (si no, por el eje ± 25 mm).
  - Envía ese muro y sus **vecinos a 2 saltos** (muros que se tocan, tolerancia 10 mm; los curvos nunca son vecinos).
  - Hace POST con `Preview=true` y se queda con las piezas cuyo `IdWall` es el del muro.
  - No toca la escena, no persiste parejas de líneas, no fusiona colineales y no guarda el diseño (los pasos previos de Encofrar).
  - Sí llama a `maStlDesing2BuildWallConnectionsPayload()`, igual que Encofrar.
- **Decisión: vecinos como contexto + filtro por `IdWall`, y no el muro aislado.**
  - Un muro aislado se recortaría como extremo libre (tapes) y perdería las piezas de esquina L/T/X que le asigna el cálculo.
  - Con vecinos de contexto, sus recortes y esquinas salen como en el diseño completo.
  - Con 1 salto no basta: la T/X y el «tipo 2» del vecino dependen del vecino del vecino.
- **Verificación** (bench .NET con el solver real, 74 casos de `pt51`): subconjunto de anillo 2 frente al diseño completo, recuento por muro idéntico en **126/126** muros.
- **Limitaciones en esquinas:**
  - Una pieza compartida cuenta en el muro al que la asignó el cálculo (igual que antes).
  - Un vecino a más de 2 saltos que cambie la esquina (muy raro) no se ve.
  - Si el muro editado aún no se ha guardado ni sincronizado con el modelo, se usa lo que haya en `maStlDesing2GetStraightWallsFromWallModelSource` / escena.
- **Muro curvo:** es independiente (`CurvedWallFormwork`), así que se envía solo.
- **Servidor** (aditivo):
  - `Desing2FormworkIdsRequestDto.Preview` (bool): si es true, no se escribe el JSON de depuración del último Encofrar (`SaveAtk60RequestDebugToTemp`).
  - La respuesta añade `Preview` y `PanelStockNotes`.
  - Sin `Preview` todo sale igual que antes: regresión de 74 casos con salida byte a byte idéntica y 13 curvos idénticos.
- **Si el muro ya tiene piezas pintadas**, el asistente usa esas piezas (como antes) y no calcula nada.

### 9.2 Paneles que no hay en obra: `_PanelStock` (PASO 2)

**Regla por muro** en el atributo `_PanelStock` (en `IdsJson` → `Attributes`), por ejemplo `{ "excludeMm": [900], "maxMm": { "750": 2 } }`:

- `excludeMm`: anchos que no se usan.
- `maxMm`: paneles de ese ancho como mucho en el muro, contando las dos caras.

Sin el atributo, el resultado es idéntico al de antes. La clase es `Repository_Desing_2/Atk60/PanelStock.cs` (`Atk60PanelStock`, nuevo; hay que añadirlo a `Design.csproj`).

**Dónde se aplica:**
- Tramo recto: `Modulo270HeightPanelCatalog.ResolveForModule` → `ReshapeWithStock`.
  - Mantiene la forma del módulo: filas tumbadas de 0,90 → 0,60 + 0,30; columnas verticales 2,70 / 2,40 / 1,20 con el mismo apilado.
  - Se reparte con los anchos permitidos (de mayor a menor, con vuelta atrás). Grapas, tirantes y rigidizadores siguen el nuevo reparto (`PanelClamp.PlaceOnModule`, `PlaceLayoutUnions`).
  - Las dos caras van enfrentadas (una llamada a `Atk60ModuleAssembly.Build` pone las dos), así que `maxMm` se consume por pares.
- Paneles de esquina L / cara E / hueco de esquina en `LCornerFormwork`.
- **No se aplica a:**
  - Tapes (`ExactWidthEndTape`): el tipo de tape exige el ancho exacto, así que un 0,30 de tape se queda y el asistente lo explica.
  - Tablones/remates.
  - El muro curvo, que tiene un solo ancho: ahí se cambia la modulación.
- **Sin combinación posible** (p. ej. un módulo de 0,75 solo con 0,30): se deja el módulo original y se anota `sin-combinacion:<L>` en `PanelStockNotes[].Notes`.
  - El asistente explica el hueco que quedaría y la compensación del manual (§2, 3.2): 0–5 cm taco y grapa regulable · 5–18 cm compensador 5–18 con rigidizadores · 8–30 cm compensador 8–30 con rigidizadores 0,75 · más de 30 cm nada del manual.
  - En Desing el remate de madera sigue topado en 0,15.
- **Respuesta:** `PanelStockNotes[]` = `{ IdWall, ExcludedMm, MaxPanels, UsedPanels, ReplacedModules, Notes }`.
- **Pruebas:** bench de 52 casos de stock (`/workspace/ia/p6/stock/run.txt`): cobertura igual por cara, sin huecos nuevos y el resto de muros idénticos. MIX_L150 muro 1 sin 0,90: 12 × 0,90 → 12 × 0,75 + 4 × 0,45 (143 → 171 piezas).
- **Persistencia:** `_PanelStock` se guarda en el `userData` del muro (eje y sólido 3D) durante la sesión. `TSql_DesignWall` no tiene columna JSON libre, así que **no se guarda en BD**.
  - **Propuesta (no creada):** `ALTER TABLE dbo.TSql_DesignWall ADD TextPanelStockJson nvarchar(max) NULL;`, más lectura y escritura en el guardado del diseño, como `NumberCurveModulation`.

### 9.3 Muro curvo sin un ancho

La modulación es un único ancho (`_CurveModulationMm`): 1,05 = 0,75 + 0,30 y 1,20 = 0,90 + 0,30.

- Si la orden lo afecta, el asistente propone la modulación válida más cercana, con preferencia por remate exterior ≤ 0,15 e interior ≥ 0,05; en empate, la menor.
  - Muestra paneles por cara, giro y remates de ahora y de la propuesta (`maStlCurvedWallModulationInfo`) y la diferencia de material (vista previa con `_CurveModulationMm`).
  - Ejemplo, c3 (R 5 m, 1/4, e 0,30): 0,90 → 0,75 da 9 paneles por cara, 10,00°, remates 124,9 / 69,3 mm.
- Al aplicar se cambia la modulación del formulario y se repinta ese muro. Se guarda con «Aceptar».

### 9.4 Cambiar paneles concretos: `_PanelReplace` (14/11/2026, cambio guiado del Asistente IA)

«De 13 paneles de 2,70×0,90 solo tengo 10»: el usuario elige qué paneles cambia y por qué combinación. Atributo del muro (en `IdsJson` → `Attributes`), una lista de reglas:

`[ { "alongMm": 4500, "upMm": 0, "pieceWidthMm": 900, "pieceHeightMm": 2700, "orientation": "Vertical", "useMm": [600, 300] } ]`

- `alongMm`, `upMm`, `pieceWidthMm`, `pieceHeightMm`, `orientation`: los del panel que devuelve el servidor (`LocalAlongMm`, `LocalUpMm`, `PieceWidthMm`, `PieceHeightMm`, `Orientation`). Casa con ±2 mm a lo largo y en altura.
- `useMm`: anchos de los paneles nuevos en orden desde el inicio del panel (de pie: a lo largo del muro; tumbado: de abajo arriba). Tienen que ser del catálogo y sumar el ancho del panel; si no, la regla se ignora.
- Clase `Repository_Desing_2/Atk60/PanelReplace.cs` (`Atk60PanelReplace`, nuevo; añadido a `Design.csproj`) + `Modulo270HeightPanelCatalog.ResizePanelPiece` (nuevo, mismo alto/banda/orientación con otro ancho). Enganche en `Atk60ModuleAssembly.Build` solo con `applyPanelStock` (tramo recto principal): por módulo, `moduleLayout = replace.Apply(layout, moduleBaseAlongMm)`; el panel se parte en su sitio y **grapas (`PanelClamp.PlaceOnModule`), rigidizadores, tirantes (`FormTie`) y puntales** se calculan con el módulo nuevo. Sin reglas que casen es el mismo objeto `layout` (no se copia).
- Las dos caras van juntas (una regla cambia la pareja). No se aplica a esquinas L / cara E / hueco de esquina, tapes, tablones ni curvo: ahí la regla no casa y el panel se queda; el asistente lo detecta con una vista previa de sonda y no ofrece esos paneles.
- Con `_PanelStock` a la vez: primero el stock rehace el módulo y después se cambian los paneles que casen (el consumo de `maxMm` se cuenta antes del cambio).
- Sin respuesta nueva: el cliente comprueba en `ElementsForThreeJs` que el panel ya no está. Sin DTO ni controlador nuevos.
- **Regresión**: 74 rectos + 13 curvos byte-idénticos sin el atributo (`/workspace/ia/p13/regress.sh`).
- **Ejemplo** (L 2,35/1,50 con el muro 4 de 6,70 m: 13 × 2,70×0,90 = 6 parejas + 1 de esquina): parejas en 3,60 y 4,50 → 0,45 + 0,45: 0,90 13 → 9, 0,45 0 → 8, grapas 48 → 60 (3 por junta nueva y cara), tuercas 28 → 36, barras 0,80 14 → 18; puntales 8 → 6 (los coloca `PushPullProp` según los paneles, igual que con `_PanelStock`). La regla del panel de esquina (−900) no hace nada. Los demás muros, idénticos.
- **Persistencia**: solo en el `userData` del muro durante la sesión (no hay columna). **Propuesta (no creada):** `ALTER TABLE dbo.TSql_DesignWall ADD TextPanelReplaceJson nvarchar(max) NULL;` (o una sola columna JSON para `_PanelStock` y `_PanelReplace`), más lectura y escritura en el guardado del diseño.
