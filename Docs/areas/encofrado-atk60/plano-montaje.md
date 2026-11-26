# Plano de montaje (vistas por cara)

US «Generar plano de montaje». Hecho (25/11/2026): tablas, icono, cálculo de caras, ventana de vistas y visor de cada cara con cotas, línea de tierra y modo colores. Falta el plano imprimible (planta, perspectiva y vistas laterales acotadas, como el `88078-PLANO-…-ATK60_Núcleo.pdf` de `No_Publicar/Proyecto Vistas/`), que leerá estas tablas.

## Para continuar (agente nuevo)

Comando: «Lee AGENTS.md y Docs/areas/encofrado-atk60/plano-montaje.md y continúa el plano de montaje: <lo que quieres>». Área: encofrado ATK-60 + visor Desing_2; el visor es código común (`Docs/codigo-comun.md`).

| Parte | Archivo |
|-------|---------|
| Endpoints `GenerateAssemblyViews` (POST) y `AssemblyView` (GET) | `Desing/Controllers/Desing_2Controller.cs` |
| Generar vistas, cargar una vista, clasificar piezas (`DesignAssemblyPieceKind`) | `Desing/Repositories/RepositoryDesing2/DesignAssemblyViewRepository.cs` |
| Caras F-n, marco de la cara (`DesignWallFaceFrame`, `Holds`), lado de la pieza | `Desing/Repositories/RepositoryDesing2/DesignWallFaceBuilder.cs` (+ `.Verify.cs`) |
| Icono, lista, montaje de la vista, colores, etiquetas F-n | `Desing/Scripts/Desing2/desing2-assembly-plan.js` (módulo, `?v=` en `Viewer.cshtml`) |
| Planta → cara vista de frente | `Desing/Scripts/Desing2/desing2-assembly-view-frame.js` |
| Cotas y línea de tierra | `Desing/Scripts/Desing2/desing2-assembly-view-dims.js` |
| Visor (mismo del recuento): `loadScene` | `Desing/Scripts/DesignToolsAutocad/plugin-cad-block-preview.js` |
| Botón del icono | `Desing/Views/Desing_2/_Desing2ModeToolBar.cshtml` (junto a «Oferta cliente») |
| Ventana de vistas | `Desing/Views/Desing_2/Viewer.cshtml` (`#desing2AssemblyViewsModal`) |
| Caja del visor, botón colores, `window.Desing2ArticleStlBox` | `Desing/Views/Desing_2/_Desing2StlViewerWorkspace.cshtml` |
| Enganches en el visor principal (`maStlDesing2AssemblySceneGroups`, `maStlDesing2AssemblyShowFaces`) | `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js` |
| Textos es/en (`StlPreview_AssemblyPlan*`, `StlPreview_AssemblyViews*`) | `Desing/Scripts/ResourceGenerators/_gen_masterarticles_resources.js` → `node` regenera `Resources/MasterArticles.*` |

Al cambiar: `node --check` y subir `?v=` del módulo en `Viewer.cshtml`; MSBuild 17 con 0 errores; las dos pruebas de abajo. Diseños de prueba en local: 30009 (L correcta, 121 piezas), 40021 (L con encofrado antiguo guardado: 531 piezas fuera).

## Qué hace el icono

Icono **Plano de montaje** (`ri-draft-line`) en la barra flotante de modos del visor Desing_2, justo después de «Oferta cliente» (grupo de la IA, `_Desing2ModeToolBar.cshtml`) → `POST Desing_2/GenerateAssemblyViews?designId=N`. Solo en el visor web, no en la paleta de AutoCAD; se oculta si la función `Desing_2.GenerateAssemblyViews` está protegida y el usuario no la tiene.

1. Calcula las caras de los muros guardados, con los mismos **F-n** que Develop › «ID Faces».
2. Pone `NumberFaceId` a las piezas de la última tanda (`QueryLatestActive`) que no tienen una cara válida.
3. Pasa a borrado lógico las vistas anteriores del diseño y crea una vista por cara con sus piezas.

El icono **solo se ve con Encofrar o Colores activos** (los materiales están en pantalla). Al pulsarlo: 1) si los muros de pantalla tienen cambios sin guardar, los guarda; 2) si el recuento falta o es anterior a los muros (`MaterialCountState`), guarda el encofrado de pantalla (como Guardar del recuento); 3) genera (forzado si guardó algo).

Si las vistas guardadas son posteriores al último cambio de los muros y de las piezas del diseño (altas, cambios y borrados, ± 0,5 s), no se regeneran: `DesignAssemblyViewRepository.LoadCurrent` las devuelve (`reused = true`, aviso «Sin cambios en el diseño»). Si algo cambió, `Generate` **borra de verdad** (DELETE, solo este diseño; lo pidió Juan) las vistas y sus piezas, recalcula la cara de todas las piezas sin fiarse del `NumberFaceId` guardado (lo reescribe, o lo pone a NULL si la pieza ya no está sobre ningún muro) y crea las vistas nuevas.

## Visor de vistas

Tras generar sale el panel flotante **Vistas del plano de montaje** (`#desing2AssemblyViewsModal` en `Viewer.cshtml`, estilos en un `<style>` junto a él): estrecho (230 px), solo nombre de la vista e icono (muro, grados y piezas en el texto al pasar el ratón), se arrastra por la cabecera dentro del visor, no bloquea el diseño y **no se cierra al elegir una cara** (la elegida queda marcada) para pasar de una a otra; se cierra con su X. Al elegir una:

1. `GET Desing_2/AssemblyView?designId=N&faceId=F` devuelve la cara (P1/P2, base, largo, alto, `UX/UZ`, `NX/NZ`) y sus piezas con posición, giros, `LocalX/LocalY/Depth` y STL (`DesignWallArticleRepository.ResolveStlPair`, igual que el recuento).
2. `Scripts/Desing2/desing2-assembly-plan.js` (módulo) monta la cara: plano gris semitransparente de largo × alto y las piezas, pasadas a la cara vista de frente con `desing2-assembly-view-frame.js` (X a lo largo desde P1, Y altura, Z hacia quien mira).
3. La pinta en **el mismo visor del recuento** (`#desing2-article-stl`, `TandemCadBlockPreview.loadScene`): ampliar, zoom, encuadrar, fondo oscuro, corte X/Y, ejes, cubo de vistas, orto/iso, pantalla completa, mover y redimensionar. Abre en FRONT.
4. **Cotas** (`desing2-assembly-view-dims.js`, como el plano de referencia): arriba la cadena de paneles y remates de la fila de abajo y, encima, el largo total de la cara; a la izquierda las alturas de panel (y la total si hay varias filas). Se miden con la caja de cada pieza ya colocada; el texto usa la medida nominal del maestro si encaja (±30 mm), si no la medida a 5 mm. Formato «0,6m». Debajo, la **línea de tierra**.
5. Botón **Pintar en colores** (paleta, `#plugin-blocks-tool-colors`, solo con una vista abierta): el bastidor de cada panel vertical con el color del maestro (`TextColor1`, igual que «Colores» del visor); el fenólico, los tumbados y el resto no cambian. Se mantiene al cambiar de vista.

`AssemblyView` clasifica cada pieza en el servidor (`DesignAssemblyPieceKind`): panel si el maestro trae ancho y alto de panel (sin maestro, por el código 27…, E27…, 12…, 24…, 3120…), remate si empieza por REMATE, tumbado si `RotationX` = 90; y devuelve ancho, alto y color nominales.

Origen de cada pieza, por orden: copia exacta del encofrado pintado en la escena (código + posición ±1 mm; cada objeto se usa una vez, con su fenólico) → artículo manual cargado con el diseño (`maStlManualArticleId`) → su STL con el giro guardado → cubo rojo si no hay STL. Si alguna no es exacta, el estado lo dice: «N aproximadas: encofra para verlas exactas».

**Plantas** (dos filas fijas arriba de la lista, `buildPlanGroup(views, mode)` en `desing2-assembly-plan.js`; se montan en el cliente con las vistas ya guardadas, sin tabla nueva):

- **Planta** (`formwork`): todo el diseño visto desde arriba con todas las piezas (las que pasan por dos caras, una vez), la leyenda F-n de cada cara y, por fuera de cada cara, su cadena de paneles (la misma que su vista); una cara sin paneles muestra su largo. Admite «Pintar en colores».
- **Planta muros** (`walls`): solo el contorno relleno de cada muro (P1/P2 exterior + P1/P2 interior) y el largo de cada cara con su F-n.
- Mundo girado X +90° (planta = x, Y de CAD = −Z de escena, altura hacia la cámara) y abre en FRONT. Las vistas se piden con `fetchView` y se guardan en caché hasta regenerar.

Enganches: `window.maStlDesing2AssemblySceneGroups` (visor principal) y `window.Desing2ArticleStlBox` (script del recuento en `_Desing2StlViewerWorkspace.cshtml`).

**Caras en la escena** (`window.maStlDesing2AssemblyShowFaces(on, faceId)`, visor principal): al generar se ve una etiqueta F-n de cada cara, del color de la cara, a tamaño fijo en pantalla (como las cotas: se recalcula en cada fotograma, orto y perspectiva) y separada del muro 600 mm + 34 px con una línea guía; al abrir una vista su cara se resalta en naranja con la etiqueta más grande. Se apaga al cerrar la lista sin elegir, al cerrar el visor o al abrir un artículo del recuento. Usa la misma numeración que el servidor (`buildDesignWallFaces`), así que la escena debe tener los muros guardados.

## Caras (F-n)

- Servidor: `Repositories/RepositoryDesing2/DesignWallFaceBuilder.cs`, copia exacta de `Scripts/Desing2/desing2-wall-faces.js` (`buildDesignWallFaces`). **Si cambias uno, cambia el otro** y repite la prueba de paridad.
- Solo ejes `TextWallRole = 'axis'`, en el orden de carga (`NumberLineId`). Grosor = 2 × `NumberOffsetMm` de su primera cara; si no tiene, `NumberWidth`.
- Por grupo conectado (extremos a menos de 50 mm): primero las exteriores en sentido horario desde la de abajo, luego las interiores. Interior = lado del centroide del grupo. Un muro que acaba en el centro de otro (T) forma otro grupo, igual que en el JS.
- Muros curvos y pilares no tienen caras: sus piezas cuentan como «sin cara».

## Pieza → cara

`DesignAssemblyViewRepository.Generate`: cada pieza va a una cara solo si está encima (`DesignWallFaceFrame.Holds`: a lo largo dentro de la cara ± 400 mm, en profundidad entre la otra cara del muro y 4 m por delante). Orden: su `NumberFaceId` guardado → la cara de su muro (`LinkDesignWall`; si apunta a una línea de cara, su eje) → la cara más cercana de cualquier muro. Si no está sobre ninguna, es encofrado de muros que ya no existen: no entra en las vistas y el aviso lo cuenta (`outside`). Cada vez que se genera, las vistas anteriores pasan a borrado lógico.

El lado (exterior / interior) lo decide `DesignWallFaceAssigner` por la distancia con signo al eje.

- Sobre el eje (≤ 1 mm, tirantes): se le asigna la exterior, `Is_Through = 1` y sale en las dos vistas.
- Si la pieza ya tiene un `NumberFaceId` válido, se respeta.

## Tablas (develop y producción)

Scripts: `Desing/Scripts/TemporalScript/2026-11-25_create_TSql_DesignAssemblyView.sql`, `2026-11-25_alter_TSql_DesignWallArticle_NumberFaceId.sql`. Aplicados en `TdesingDevelop` y en producción el 25/11/2026 (esquema idéntico). Aún no están en `Model.edmx`: el repositorio escribe con SQL.

| Tabla | Clave | Campos de negocio |
|-------|-------|-------------------|
| `TSql_DesignAssemblyView` | una fila por cara | `LinkDesign_V2`, `LinkDesignWall` (eje), `NumberFaceId`, `TextFaceName` («F-3 Exterior»), `TextFaceSide`, `NumberWallAngleDeg`, `NumberFaceNormalDeg`, `NumberP1X/P1Z/P2X/P2Z`, `NumberBaseY`, `NumberLengthMm`, `NumberHeightMm`, `NumberElementCount` |
| `TSql_DesignAssemblyViewElement` | una fila por pieza y vista | `LinkDesignAssemblyView`, `LinkDesignWallArticle` (sin FK: la purga de tandas borra piezas), `LinkMasterArticle`, `TextCode`, `NumberSequence`, `NumberInsertX/Y/Z`, `NumberX/Y/ZRotation`, `NumberLocalX`, `NumberLocalY`, `NumberLocalDepth`, `Is_Through` |
| `TSql_DesignWallArticle` | + `NumberFaceId` | F-n de la pieza (NULL = sin asignar) |

## Convenios de la vista

- Escena del visor: Y arriba, X de escena = X de CAD, Z de escena = −Y de CAD (como la importación DWG, `maStlDesing2ImportDwgGeometry`). Ojo: `WallCadXData.ToDesingMm` del plugin de AutoCAD usa +Y.
- `NumberWallAngleDeg`: eje del muro en la planta de CAD, antihorario desde +X, en [0, 180) con 3 decimales (0, 90, 33,69…). Sirve para elegir la vista.
- `NumberFaceNormalDeg`: hacia dónde mira la cara en la planta de CAD, en [0, 360).
- Mirando la cara de frente (desde fuera si es exterior, desde dentro si es interior), P1 queda a la izquierda: derecha = (nz, −nx) en escena. `NumberLocalX` = mm desde P1 hacia P2; `NumberLocalY` = altura desde `NumberBaseY`; `NumberLocalDepth` > 0 = delante de la cara.

## Pruebas

- `DesignWallFaceBuilder.Verify.cs` (fuera del `.csproj`): `csc /main:Desing.Repositories.RepositoryDesing2.DesignWallFaceBuilderVerify DesignWallFaceBuilder.cs DesignWallFaceBuilder.Verify.cs`. Con el argumento `print` saca las caras con el mismo formato que `buildDesignWallFaces` para compararlas línea a línea con el JS (rectángulo, L, T, muro suelto, oblicuo y dos grupos).
- `node Scripts/Desing2/desing2-assembly-plan.test.mjs` (desde `Desing/`): matriz de la cara, montaje de la vista (exactas, repetidas, manual, sin STL, vista vacía), cotas (cadena del plano de referencia, remate, tumbado, nominal frente a medida), línea de tierra y modo colores.
- Diseño 30009 (L, 121 piezas): 4 vistas, 129 elementos (8 tirantes en las dos caras). Regenerar no vuelve a estampar y deja una sola tanda activa.

## Pendiente

- Plano imprimible (PDF/DWG) con planta, perspectiva y vistas laterales acotadas, leyendo estas tablas y eligiendo la vista por `NumberWallAngleDeg`. Rótulo «VISTA: …» como en el plano de referencia.
- Opcional: borrar del recuento las piezas de muros eliminados al encofrar/guardar (hoy solo se excluyen de las vistas).
- Muros curvos y pilares: sin caras todavía.
- «Update Model from Database» en `DAL/Model.edmx` para tener las entidades en EF (hoy SQL directo).
- Revisar aparte: `WallCadXData.ToDesingMm` (plugin AutoCAD) guarda +Y de CAD como Z; el visor usa −Y.
- Sin probar en navegador por el agente (la web pide sesión): validar con Juan en 30009 y 40021.
