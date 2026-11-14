# Encofrado ATK-60 (resumen de consulta)

Escrito el 12 nov 2026. Resume el manual NEVI de ULMA y los documentos de la carpeta. Si una ficha detallada y este resumen no coinciden, manda la ficha. **[HUECO]** marca lo que no dice ninguna fuente: no se inventa.

Fuentes:

- Manual: Guía del Usuario NEVI (ULMA, 89 págs.), en `Desing/Content/DesignTools/DWG/AtkSystem60/Snaps/NEVI_GUIA_USUARIO.pdf`. Páginas entre paréntesis.
- Fichas: `ENCOFRADO-AT60-BASE.md`, `ENCOFRADO-AT60-IMPLEMENTACION.md`, `esquina-L-atk60.md`, `esquina-T-atk60.md`, `agente-tapes-atk60.md`, `tapes-tipos.md`, `agente-grapas-atk60.md`, `HANDOVER-MUROS-T-CRUCE.md`, y `Docs/Proyectos/Desing/ATK60-Logica-Modulos-Wall.md`.

ATK-60 es el nombre del sistema en Desing. El manual es el del NEVI, que es el mismo sistema. Los códigos del manual (192xxxx) no son los del catálogo Desing (1000xxxx, E27…). Los dos se dan cuando se conocen.

## 1. Piezas

| Pieza | Manual NEVI | Desing | Para qué |
|-------|-------------|--------|----------|
| Panel estándar | Alto 2,7 / 2,4 / 1,2 m. Ancho 0,90 / 0,60 / 0,45 / 0,30 m. Fenólico de 15 mm. Presión máx. 60 kN/m² | Catálogo 0,30 a 0,90 en paso 0,15 (`ATK60-Logica-Modulos-Wall.md`) | Cara del muro |
| Panel universal 0,75 | Costillas en U con 14 orificios cada 5 cm (p. 44) | Tapes tipo 3 (`TapeS5`…) | Esquina exterior a 90°, tapes, pilares |
| Escuadra interior | 0,30 × 0,30 m. 1920881 (2,7), 1920887 (2,4), 1920893 (1,2) | `MA_ATK60_CORNER_PANEL_MM = 300`. Códigos E27004210 / E24004217 / E12004216 | Rincón de L, T, X |
| Angular | No aparece con ese nombre **[HUECO]** | 0,12 m; une, no encofra. 27000000 / 24000000 / 12000000 | Esquina exterior de la L en Desing |
| Escuadra G (giratoria) | 1920943 / 47 / 51 | No implementada **[HUECO]** | Esquinas distintas de 90° |
| Grapa fija | 1920818. Solo a tope | `10004220` | Toda junta entre dos paneles |
| Grapa regulable | 1920851. Admite suplemento hasta 50 cm | `10000221` / `10000221B` | Compensación 0–5 cm y tablón de remate |
| Rigidizador COMAIN | 0,75 / 1,6 / 3 m (1850162 / 1850159 / 1850433), fijador 1850164 | `1850162` en `CommonElement.SedUnionRigiHorizontal_0` | Compensaciones, juntas horizontales, tapes |
| Gancho cierre + tuerca placa | 1920811; tuerca placa fija / campana | Usados en tapes | Atar universal y rigidizador |
| Tirante | Barra roscada Ø15 (15/1 … 15/6) | Placa `10443020` + barra; a 43,7 mm del canto | Atar las dos caras |
| Compensadores | 5 cm (1920901/04/09), 5–18 (1920863/67/71), 8–30 (1920916/20/24) | Tablón de madera ≤ 0,15 (remate) | Cerrar medidas no modulares |
| Puntal / estabilizador | Tensores 1,1–1,7 / 2,4–3,5 / 3,6–4,8 / 5–6 m; cabezal 1920839; base 1900144 | `PushPullProp.cs` | Aplomar |
| Ménsula de trabajo | 1861094 | `WorkingBracket.cs` | Pasarela |
| Gancho de izado | 1920835, máx. 1 200 kg | — | Grúa |

## 2. Reglas del manual

### Juntas (3.1, p. 38–39)

- Grapa fija siempre que se pueda. Junta vertical: 3 grapas en panel de 2,7 m; 2 en 2,4 y 1,2 m.
- Con compensación 0–5 cm: grapas regulables (3 / 3 / 2).
- Con compensación 5–18 cm: grapas fijas 3 / 2 / 2 y rigidizadores 2 / 2 / 1.
- Una junta junto a una esquina exterior a 90° o a un tape lleva una grapa más.
- Alturas de grapa usadas en Desing: 2,70 → 450, 1 350, 2 250 mm; 2,40 → 400, 2 000; 1,20 → 200, 800. Tirante: 550 / 2 150 (2,70), 550 / 1 850 (2,40), 350 / 850 (1,20). No poner grapa a la altura del tirante (`agente-grapas-atk60.md`).

### Compensaciones (3.2, p. 41–43)

| Hueco | Solución |
|-------|----------|
| 0–5 cm | Taco de madera en obra + grapa regulable + tuerca placa campana |
| 5 cm | Compensación 5 cm + grapa regulable |
| 5–18 cm | Compensador 5–18 (un panel solapa la chapa) + rigidizadores + grapas fijas |
| 8–30 cm | Compensador 8–30 (los dos paneles solapan) + rigidizadores 0,75 (4 / 4 / 3) |

En Desing el hueco se cierra con un tablón de madera: ≤ 0,15 m a lo largo, 0,12 m de ancho, grapa regulable en los dos cantos. **[HUECO]** El manual no da ese tablón: es regla de Desing.

### Esquina L a 90° (3.3, p. 44–45)

- Rincón: escuadra interior 0,30 × 0,30, con grapas fijas 3 / 2 / 2.
- Exterior: panel universal con estándar, o dos universales, atados con gancho cierre y tuerca placa fija.
- Desing (`esquina-L-atk60.md`): cara exterior = `selectPanel(espesor del otro muro + 0,30)`, esquina interior 0,30 y angular 0,12. Ejemplo: 0,47 + 0,30 = 0,77 → panel de 0,90.

### Esquinas que no son de 90° (3.4, p. 46–48)

- 75–85°: escuadra G dentro y fuera. Dentro va con gancho rigidizador y placa excéntrica, y lleva compensación ≥ 5 cm.
- 85–180°: escuadra G en los dos lados con grapas fijas (5 / 5 / 3).
- Rigidizadores 0,75 fuera (2 / 2 / 1); dentro solo a partir de 100°.
- **[HUECO]** Desing no lo hace.

### T y X (3.5, p. 49)

- El manual solo dice: escuadra interior combinada con anchos de panel según el espesor. El resto es dibujo (dos escuadras en la T, cuatro en la X).
- Desing, T (`esquina-T-atk60.md`):
  - Cara E (testero frente a la rama) = espesor de la rama + 0,30 + 0,30, sin angular. Hasta 0,90 es un panel con el remate al final; con más, dos paneles con el remate en medio.
  - Recorte de ejes: el de paso se retira ½ espesor de la rama + 0,30. La rama se retira ½ espesor del de paso + 0,30. El espesor del de paso es la **media** de los dos tramos (`TryTCornerTrimMm`).
  - Tipo 2: los dos tramos de paso tienen distinto espesor. El fino se corre (grueso − fino)/2 alejándose de la rama. El escalón queda en la cara de la rama y se cierra con tablón ≤ 0,15 y grapa variable.
- Desing, X: cuatro escuadras interiores. Cada brazo se retira ½ espesor del que cruza + 0,30 (`TryPlusCornerTrimMm`). Sin cara E ni angular. La X de igual espesor está aceptada.
- **[HUECO]** El remate con espesores distintos en T y X está pendiente desde el 29 oct (`esquina-T-atk60.md`, «Pendiente»). La T tipo 3 está nombrada pero sin definir.

### Tapes (3.7, p. 52–53)

- Con rigidizador: atado con gancho cierre y tuerca placa fija por la parte exterior del perfil. Rigidizadores 4 / 3 / 2. La junta contigua lleva una grapa más.
- Con panel universal: gancho cierre y tuerca placa campana.
- Desing usa los tipos 1 a 7 (`tapes-tipos.md`):

| Espesor | Sin solape | Con solape |
|---------|------------|------------|
| < 0,30 | Tipo 2 (`TapeS3`) | Tipo 1 (`TapeS1` / `TapeS2`) |
| 0,30 / 0,45 / 0,60 / 0,90 exacto | Tipo 4 (`TapeS4`) o tipo 3 (`TapeS5`) | Tipo 1 |
| 0,30–0,60, no exacto | `TapeS6` / `TapeS7` (múltiplo de 0,05) o `TapeS8` | Tipo 1 |
| > 0,60, no exacto | `TapeS9` / `TapeS10` / `TapeS11` (múltiplo) o `TapeS13` / `TapeS12` | Tipo 1 |

- Solo se cierra un extremo libre (`EndIsFree`). Un extremo unido a otro muro no lleva tape.

### Otras soluciones (3.6, 3.8, 3.9, p. 50–57)

- Cambio de espesor de muro: hasta 15 cm con tacos de madera y rigidizadores 0,75; 20 cm con escuadra interior, gancho rigidizador y tuerca campana.
- Retranqueo: escuadra interior con paneles universales y estándar.
- Pilar: paneles universales unidos entre sí; tabla de orificios de 10×10 a 65×65 cm en paso de 5 (p. 54–55).
- Pilar embebido en muro o esquina: escuadra interior con universal y estándar. Saliente mínimo en esquina 200 × 300.
- **[HUECO]** En Desing solo `PillarFormwork.cs`; no se ha comprobado contra estas tablas.

## 3. Reparto del muro recto

`PackAtk60LengthMm` (`Atk60WallsRepository.cs`):

- n paneles de 2,70 + el mayor módulo final que quepa + el remate.
- El remate no pasa de 0,15. Si sobra más, el módulo final sube un paso.
- Si el sobrante está entre 0,16 y 0,29, un 2,70 se abre en 2,55 + 0,30.
- Extremo conectado: se recorta 450 mm (`ConnectedEndTrimMm`). La L usa `LCornerFormwork.AxisTrimMm`, la T `TryTCornerTrimMm` y la X `TryPlusCornerTrimMm`.

## 4. Dónde está en el código

**Cliente** (`Desing/Scripts/MasterArticles/`):

| Qué | Dónde |
|-----|-------|
| Panel y taco | `ma-stl-atk60-formwork.js`: `maAtk60SelectPanelMm` (múltiplo de 150 ≥ necesidad, mín. 300) |
| Pieza verde L / T (solo visor, **no** va al servidor) | `ma-stl-atk60-formwork.js`: `maAtk60BuildLFootprintMm`, `maAtk60BuildTFootprintMm` (P-1…P-8; brazos a ras de la cara real desde el 12 nov) |
| Recortes, orientación T, muro fino | `master-article-details-stl-viewer.js`: `maStlAtk60BuildAxisEndpointTrimMap`, `maStlAtk60CanonicalThroughDirFromTJunction`, `maStlAtk60TryRenderJunctionAtBucket`, `maStlAtk60ThinThroughShiftByAxisId` (el signo no se invierte), `maStlAtk60JunctionHeightMm` |
| Petición al servidor | `desing2-stl-viewer-toolbar-wiring.js`: muros desde los ejes (`maStlBuildStraightWallsFromScene` / `…ForFormwork` / `…FromWallModelSource`) con `_DataWith`, `_DataHeight`, caras, `wallConnections` y `_PropSpacingMm`. Se envía como `IdsJson` |
| Pintado | `maStlDesing2RenderAtk60Elements`, que recibe `ElementsForThreeJs` |

**Servidor** (`Desing/Repositories/`):

| Qué | Dónde |
|-----|-------|
| Entrada, recortes, reparto | `RepositoryAtk60/Atk60WallsRepository.cs`: `SolveFromIdsJson`, `ApplyConnectedEndTrim`, `CornerTrimMm`, `PackAtk60LengthMm`, `BuildPanel270ElementsForThreeJs` |
| L, T, X y tipo 2 | `Repository_Desing_2/Atk60/LCornerFormwork.cs`: `PlaceCorner`, `PlaceTPair`, `PlaceCaraE`, `PlacePlus`, `TryTipo2Step`, `ShiftWall`, `PlaceStepRemate`, `SumRematesAtCorner` |
| Remates de cara | `RepositoryAtk60/Atk60CornerFaceRemate.cs` |
| Tapes | `Repository_Desing_2/Atk60/ExactWidthEndTape.cs` (`ResolveTape`), `EndFiller.cs` (tope 0,15), `FormTie.cs` |
| Grapas y rigidizadores | `PanelClamp.cs`, `PanelStiffener.cs` |
| Tirantes, puntales, ménsulas, pilares | `FormTie.cs`, `PushPullProp.cs`, `WorkingBracket.cs`, `PillarFormwork.cs` |
| Muro curvo (§8) | `Repository_Desing_2/Atk60/CurvedWallFormwork.cs` (`Resolve`, `Compute`, `Place`); `SolveFromIdsJson` lo separa antes de recortes y esquinas |

## 5. Historial de implementación (L, T, +)

Sacado de `git log develo` y `git show` (12 nov 2026). Hora de Madrid. Todos los commits son de Juan Andrés Godoy López. Los commits «Agent host session…» de `refs/agents/*` son puntos de control de agentes y no cuentan aquí. «Aceptado» quiere decir que el usuario lo dio por bueno, según las fichas.

### Base común

| Commit | Fecha | Mensaje | Qué cambió |
|--------|-------|---------|------------|
| `9bbc344` | 11/07 09:32 | Esquina en T y documentación | Cliente: nace `ma-stl-atk60-formwork.js` con `maAtk60SelectPanelMm`, la huella L y la huella T (8 puntos), y las fichas BASE, IMPLEMENTACION y HANDOVER-MUROS-T-CRUCE |
| `0596af8` / `79125ef` | 15/07 | punto de inserción / insertar | Servidor: reparto en módulos y paneles de 2,70 en `Atk60WallsRepository` (+887 líneas) |
| `0d90910` | 08/09 16:41 | muro recto 08 09 2026 | `PackAtk60LengthMm`: reparto del muro recto |
| `dfe1c3c` | 03/10 09:19 | pluging | `ApplyConnectedEndTrim`: recorte de 450 mm en un extremo conectado (antes de que existieran L, T y +) |
| `2b00ef9` | 11/10 13:40 | puntal y mensulas y mas | Nacen `EndFiller`, `PanelClamp`, `FormTie`, `PanelFormwork`, `PushPullProp` y `WorkingBracket` |
| `8df6ce2` → `d80d0ec` | 20–21/10 | Tapes / bug tapes / Tape S7 | Nace `ExactWidthEndTape.cs` (tapes 1 a 7). Aceptado el 20–21 oct |

### L

| Commit | Fecha | Mensaje | Qué cambió |
|--------|-------|---------|------------|
| `9bbc344` | 11/07 09:32 | Esquina en T y documentación | Huella L en el cliente: pata = `selectPanel(espesor del otro + 0,30)` |
| `5e0af38` | 26/10 17:00 | cotas y mas | **Primera L en el servidor.** Nace `LCornerFormwork.cs`: `PlaceCorner`, `AxisTrimMm`, `PlaceExteriorCover`, `PlaceGap`, `PlaceCornerSeamClamps`, angular y esquina interior. Nacen `Atk60CornerFaceRemate.cs`, `esquina-L-atk60.md` y `agente-grapas-atk60.md`. La L se aceptó el 25 oct |
| `9dfc4de` | 29/10 19:50 | fases | `SumRematesAtCorner`: el remate de la esquina y el del tramo no se suman; cada tablón ≤ 0,15 |
| `778de4c` | 03/11 11:23 | Fases | `IsPhaseExit`: en el cabo `-exit` de una fase no se pone cubierta exterior ni hueco; el angular cierra |
| `83550c6` | 04/11 13:44 | **mal codigo de esquina** | Esquina regulable `Universal_X` / `Universal_Y` (`_CornerTypeP1/P2`, y si no `_Tape_0`): panel de 0,75 en la cara regulable, sin angular ni grapa de esquina, y herraje de pilar. **También cambia la L angular:** `PlaceCornerSeamClamps` pone un tirante nuevo (`FormTie.PlaceJointColumn`) en la junta de toda esquina. Cliente: el visor inventa las caras que faltan a partir del eje (`synthFaceFromAxis`) y la regla temporal pone `_IsFormwork = false` en el muro regulable |
| `1426b7d` | 04/11 17:38 | cambio para testear | Regulable: gancho y placa con desplazamientos fijos (10 / 120 mm, 300 / 390 + espesor) |
| `ec7b9cc` | 05/11 08:37 | df | Regulable: el herraje se mueve como un bloque hasta el gancho. Si no se genera gancho, **lanza `InvalidOperationException`** |
| `b6f1d54` | 05/11 17:38 | relise 1 | Regulable: el destino del gancho pierde el término de cara en X y lo mantiene en Z (`targetHookX` sin `sideX·faceOffsetMm`). Cliente: `CornerReferencePoints` (punto de referencia de la L) |

### T

| Commit | Fecha | Mensaje | Qué cambió |
|--------|-------|---------|------------|
| `9bbc344` | 11/07 09:32 | Esquina en T y documentación | **Primera T, solo en el cliente:** huella P-1…P-8, cotas y recortes |
| `f0881bb` | 11/07 21:46 | esquina en t | Orientación canónica del atravesado (`maStlAtk60CanonicalThroughDirFromTJunction`) |
| `9dfc4de` | 29/10 19:50 | fases | **Primera T en el servidor:** `PlaceTPair`, `PlaceCaraE` (rama + 0,30 + 0,30), `PlaceTFromFacingGaps`, `TryTipo2Step` / `ShiftWall` / `PlaceStepRemate` (tipo 2) y `TryTCornerTrimMm` (½ espesor del otro + 0,30). Nace `esquina-T-atk60.md`. La T se aceptó el 28 oct |
| `5432fa4` | 30/10 14:25 | planificacioón | La T ya no pone tape en los brazos (`PlaceEndTape` y `ExactWidthEndTape.PlaceOneEnd` fuera); `StripTapePanelAtEnd` quita el panel de tape en el extremo de cada brazo. Se quita `ReleaseEndsWithoutNeighbor`: un extremo enlazado a un muro que no se encofra ya no queda libre, así que no sale tape ahí |
| sin commit | 12/11 | — | Cliente: la pieza 3D de la T va a ras de la cara real (`vNear`). Solo visor; no va al servidor |

### + (cruce)

| Commit | Fecha | Mensaje | Qué cambió |
|--------|-------|---------|------------|
| `9e8e088` | 11/07 21:57 | esquina en X conseguido | **Primera +, solo en el cliente:** `maAtk60BuildCrossFootprintMm` |
| `9dfc4de` | 29/10 19:50 | fases | **Primera + en el servidor:** `PlacePlus` / `PlacePlusSide`, cuatro esquinas interiores, `TryPlusCornerTrimMm` (½ espesor del que cruza + 0,30), y tape en cada brazo (`PlaceOneEnd`). La + de igual espesor se aceptó el 29 oct |
| `5432fa4` | 30/10 14:25 | planificacioón | Fuera el tape de los brazos de la + (la ficha pasa a «No lleva cara E. No lleva angular») |

### Último estado bueno conocido

- **L angular:** `5e0af38` (26/10), aceptada el 25 oct. Con la suma de remates, `9dfc4de` (29/10).
- **T y +:** `9dfc4de` (29/10 19:50), aceptadas el 28 y el 29 oct. Después vino `5432fa4`, un cambio de criterio sobre el tape de los brazos que no figura como validado.
- **Tapes:** `d80d0ec` / `c4b2e38` (21–22/10), aceptados. Hasta `5432fa4` los tocan `5e0af38` y `9dfc4de`.
- Candidato para comparar todo junto: `2a1b80e` (30/10 15:08), el último commit antes de los cambios de noviembre.

### Sospechosos de la rotura (análisis; no se ha tocado código)

| Commit | Afecta a | Evidencia | Probabilidad |
|--------|----------|-----------|--------------|
| `12e2e56` (09/11, «mal codigo») | **Tapes** | `ExactWidthEndTape.EnableTapeRemateVisualInsertion = false` sigue así en HEAD. Ya no salen la franja naranja ni los tablones del TapeS1/S2, las grapas del suplemento, las grapas fijas del canto (`AddEndClamp`) ni las variables del tape. Apaga piezas propias del tape, no solo remates | **Alta.** Explica «tapes rotos» |
| `12e2e56` (visor) | T y L, posible | Al cargar ya no se llama a `maStlWall2dToolRefactorAllWallJunctionsMm()`, y los bucles de partir y soldar muros paran a las 64 pasadas. Si el diseño guardado no tenía las uniones al día, lo que se envía al servidor sale con uniones viejas | Media. Sin probar |
| `83550c6` (04/11, «mal codigo de esquina») | **L** (todas) | En HEAD toda esquina, también la angular, lleva un tirante nuevo en la junta (`includeTieColumn = true`). El 05/11 el informe ya decía, para la angular de 0,22: «falta remate/tirante interior y solape de paneles» | **Alta** para la L |
| `ec7b9cc` / `b6f1d54` | L regulable | `throw` sin `catch` en `SolveFromIdsJson`. Si una esquina regulable cae en un tramo de menos de 1 mm (`PillarFormwork.PlaceCornerHardware` no genera nada), falla **todo** el encofrado. `b6f1d54` deja el herraje torcido (X sin término de cara, Z con él) | Media (solo con regulable). Los datos del usuario no tienen `Universal_*` |
| `5432fa4` (30/10) | T, + y tapes | Sin tape en los brazos de la T y la +. Sin `ReleaseEndsWithoutNeighbor`, un extremo enlazado a un muro que no se encofra (fase u `_IsFormwork = false`) se queda sin tape | Media. Fue un cambio de criterio, pero junto con `_IsFormwork = false` de `83550c6` puede dejar cantos abiertos |
| Cambios de hoy | — | No cambian lo que se envía al servidor (comprobado con los datos guardados del usuario) | Baja |

### Comparación y arreglo (12 nov)

Mismo `IdsJson` (diseño guardado del usuario y casos L, T, +, tipo 2 y tape) con el servidor de `5e0af38`, `9dfc4de`, `2a1b80e`, `83550c6`~1, `83550c6`, `b6f1d54`, `12e2e56`~1 y HEAD. Arnés: el código del servidor compilado aparte, sin tocar `develo`.

| Problema | Causa | Arreglo (sin commit) |
|----------|-------|----------------------|
| T y + sin esquinas en el diseño del usuario: 4 piezas de esquina en vez de 16 y la cruz a medias | Antiguo, no de noviembre. `ResolveSceneMm` miraba cada cota sola: ≤ 50 se leía como metros. El brazo de la T se recorta hasta x = 50 mm (muro de paso en x = 500) y ese 50 pasaba a 50 m | `Atk60WallsRepository`: las cotas del eje se leen en mm. Solo se pasan de metros si las cuatro son pequeñas y el largo cuadra con `_Datalong`. Los puntos con `xMm` ya están en mm |
| Extremo libre recortado 450 mm y sin tape | Un eje de largo 0 (punto suelto del dibujo) contaba como muro unido | Los ejes de menos de 1 mm no cuentan como unión ni reciben tape ni placas |
| Tapes abiertos | `12e2e56`: `EnableTapeRemateVisualInsertion = false` | Vuelve a `true`: franja naranja, tablones, grapas fijas y variables del tape |
| Muro recto de menos de 0,30 con tape: los paneles empiezan tarde, el muro se queda sin encofrar en un extremo y el tape del otro se borra | `9dfc4de` (29/10): `SumRematesAtCorner` tomaba el fenólico y los tablones del tape como remates de tramo. Corría los paneles el ancho del tape (440–610 mm) y borraba el tape del otro extremo. `12e2e56` lo tapó al apagar las piezas del tape; al volver a encenderlas (arreglo anterior de hoy) reapareció | `LCornerFormwork.IsTapeWood`: los colores `wood-orange` y `wood-side` (piezas del tape) no entran en la suma de remates |
| Tirante doble en la junta de la esquina | `83550c6`: `PlaceJointColumn` en toda esquina | Se mantiene la columna (en 0,22 faltaba ese tirante) pero no se repite donde el tramo ya lo puso |
| Todo el encofrado falla si la esquina regulable no genera gancho | `ec7b9cc`: `throw` sin `catch` | Se sale sin alinear |
| Herraje regulable mal puesto en muros en Z | `b6f1d54`: X sin el término de cara | Se suma `sideX · faceOffset`. En muros en X no cambia nada |
| Remate del tramo junto al angular, sin enfrentar al de la otra cara (L 0,20 / 0,22 / 0,40 y T) | `9dfc4de` (29/10): `SumRunRemates` corría también el módulo de fuera de la esquina y ponía los dos tablones en el rincón: fuera, pegado al angular; dentro, metido en el otro muro. Quedaban huecos en la cara interior. Un remate de extremo libre o el de la cara E de la otra punta se tomaba por el de la esquina | Ver la regla de abajo. `InCornerZone`: solo es tablón de esquina el que pasa del extremo del tramo. El módulo de la esquina, la cara E y sus grapas no se corren. El remate del tramo va con `EndFiller.Build` en las dos caras, en el extremo del tramo |

Recuento (piezas de esquina interior / tablones / grapas fijas / tirantes):

| Caso | `2a1b80e` | HEAD antes | Arreglado |
|------|-----------|------------|-----------|
| Diseño del usuario | 4 / 23 / 204 / 66 (764) | 4 / 14 / 204 / 66 (755) | 16 / 23 / 252 / 68 (822) |
| L 0,30 | 2 / 4 / 78 / 20 | 2 / 4 / 78 / 24 | 2 / 4 / 78 / 20 |
| L 0,22 | 2 / 4 / 48 / 16 | 2 / 4 / 42 / 20 | 2 / 10 / 42 / 20 (las 48 grapas de `2a1b80e` eran de las juntas corridas 190 mm) |
| + 0,30/0,20 | 8 / 11 / 96 / 12 | 8 / 8 / 96 / 12 | 8 / 14 / 96 / 12 |

Muro recto suelto, «Tipo 1» en los dos extremos (paneles de la cara, del mm 0 al largo):

| Espesor | `5e0af38` | `9dfc4de` a `12e2e56`~1 | HEAD antes | Arreglado |
|---------|-----------|--------------------------|------------|-----------|
| 0,20 / 0,22 (3,00 a 6,20 m) | 0 → L, tape en los dos | 440–610 → L + 440–610, tape solo en un extremo | 0 → L, sin tape | 0 → L, tape en los dos |
| 0,30 / 0,40 | 0 → L | 0 → L | 0 → L | 0 → L (0,40 sigue sin tape, ver §6) |
| T 0,30/0,30, T 0,30/0,20, T 0,22/0,40, + 0,30/0,30, T tipo 2 | iguales en todas las versiones | | iguales |

Regla del remate en la esquina (12 nov, tarde):
- El remate del tramo va **enfrentado en las dos caras**, en el extremo del tramo junto a la esquina. Fuera queda entre el módulo de la esquina (otro espesor + 0,30) y el primer panel. Dentro queda junto al tablón de la esquina.
- El tablón de la esquina (el sobrante de la cara interior) se queda en su sitio: entre la esquina interior y el panel, nunca en el rincón del angular.
- Si tablón de esquina + remate del tramo pasa de 0,15, el del tramo no se mueve: se queda en medio del tramo, ya enfrentado.
- Comprobado en el arnés: L de 0,20 / 0,22 / 0,30 / 0,40, giro a izquierda y derecha, ejes en los dos sentidos y espesores mezclados (35 casos). Sin huecos ni solapes, y los remates del tramo iguales en las dos caras. T, +, tipo 2, muro recto y tapes: sin huecos. La L 0,30, T 0,30/0,30, + y muro recto salen igual que antes.

Se queda como está:
- `5432fa4`: el extremo libre lleva tape, el unido no. No se recupera `ReleaseEndsWithoutNeighbor`: pondría tape en el muro de una esquina regulable (`_IsFormwork = false`), que ya cierra el panel de 0,75.
- Visor (`12e2e56`): no se vuelve a recalcular las uniones al cargar. El fallo no estaba ahí.

## 6. Puntos abiertos

- 12 nov: T, + y tapes arreglados en el servidor (§5, «Comparación y arreglo»). Falta probarlo en el visor.
- Tape «Tipo 1» en muros de 0,30 a 0,60 que no son 0,30 / 0,45 / 0,60 (por ejemplo 0,40): el servidor no lo traduce a un `TapeS*` y el extremo queda sin tape. Pasa igual desde octubre.
- Herraje regulable: la posición en muros en Z sale de igualarla a la de muros en X. Comprobar en el visor.
- Remate de T y X con espesores distintos: pendiente desde el 29 oct.
- T tipo 3: sin definir.
- Esquinas distintas de 90°, compensadores NEVI 5–18 / 8–30 y cambio de espesor de muro: no implementados.
- Muro curvo (§8): puntales y ménsulas sin poner; tape del extremo perpendicular al panel y elegido con el espesor real; fases solo con «Todo». Falta probarlo en el visor.

## 7. Regresiones conocidas (no repetir)

| Commit | Qué rompió | Regla para no repetirlo |
|--------|------------|-------------------------|
| `9dfc4de` (29/10, «fases») | Piezas del tape sumadas como remates (muro recto que se cierra antes). Remate del angular no enfrentado al de la otra cara | En la suma de remates solo entran remates del tramo (`wood-remate`), nunca las piezas del tape. El módulo de la esquina y la cara E no se corren. El remate del tramo va enfrentado en las dos caras, en el extremo del tramo; tope 0,15 |
| `12e2e56` (09/11, «mal codigo») | Tapes apagados (`EnableTapeRemateVisualInsertion = false`) | No apagar piezas para ocultar otro fallo: ese apagado escondía el de `9dfc4de` |
| `83550c6` (04/11, «mal codigo de esquina») | Tirante doble en la junta de la L | Antes de añadir una pieza en la junta, mirar si el tramo ya la puso |
| `ec7b9cc` (05/11) / `b6f1d54` (05/11) | Esquina regulable: `throw` sin `catch`; herraje mal en muros en Z | Un fallo en una esquina no debe parar todo el encofrado. Probar muros en X y en Z |
| `5790606` (09/11, «reliase») | Acotar pasaba a Modo Líneas y quitaba el encofrado | Solo las herramientas de dibujo pasan a Líneas. Acotar y las de consulta no cambian de modo |
| `77313a7` (07/07, «clean code») + `9bbc344` (11/07, «Esquina en T») | Dos líneas simples paralelas ya no salían como muro en 3D ni en encofrado si el diseño tenía algún muro con eje (solo se enviaban caras y, con caras, se ignoraba la detección) | Desde el 13/11 las parejas de líneas (50 mm–1,20 m, solapadas) se convierten en muro real al generar 3D, al encofrar y al hacer desfase (`maStlDesing2ConvertParallelLinePairsToWallsMm`). Probar siempre un diseño mixto: muro con eje + dos líneas en T |
| Antiguos | Cotas ≤ 50 leídas como metros; ejes de largo 0 como unión; extremo unos mm dentro de otro muro partido como T | Cotas del eje en mm. Ignorar ejes < 1 mm. Un punto cerca del extremo de un muro se une al extremo, no lo parte |

Antes de dar por bueno un cambio de encofrado: L 0,20 / 0,22 / 0,30 / 0,40 (giro a izquierda y derecha), T de uno y dos espesores, +, T tipo 2, muro recto con tape de 3,00 a 6,20 m, el diseño del usuario y, desde el 13/11, que un muro curvo junto a ellos no cambie nada de lo recto. Sin huecos ni solapes por cara, remates del tramo iguales en las dos caras, y los recuentos sin cambios donde no se tocó nada.

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
