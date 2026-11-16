# Área: Dibujo 2D en Desing_2

**Cubre:** polilínea, selección (ventana/cruce), borrar, recortar, alargar, girar, simetría, repetir comando, importar DWG/DXF, barra inferior (deshacer, orto, reglas), cota de muro (con «Id de Artículo»).

**No cubre:** convertir líneas en muro 3D (→ `desing2-muros-3d`), encofrado, plugins CAD.

## Código

- Herramientas en `master-article-details-stl-viewer.js` (buscar `#ma-stl-tool-*`)
- `Desing/Views/Desing_2/_Desing2TopToolBar.cshtml`, `_Desing2ModeToolBar.cshtml`
- `Desing/Scripts/Desing2/desing2-dwg-import.js`; servidor `Services/Desing2DwgConverter.cs`, `Desing2DxfImportParser.cs`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Cualquier herramienta de dibujo | [`herramientas-2d.md`](herramientas-2d.md) (sección de la herramienta) |
| Reglas, punto de inserción, rejilla | [`../desing2-visor/visor-funciones.md`](../desing2-visor/visor-funciones.md) («Anclaje de reglas») |
| Girar y Simetría desde la IA | [`../ia-asistentes/ia-del-diseno-girar-catalogo-entornos.md`](../ia-asistentes/ia-del-diseno-girar-catalogo-entornos.md) §16.9 |
| Cota «Id de Artículo» (ID de cada panel «7.3», búsqueda por ID) | [`cota-id-de-articulo.md`](cota-id-de-articulo.md) |
| Cotas: solo el muro seleccionado / «Todo el Diseño»; selección del muro en 3D | [`herramientas-2d.md`](herramientas-2d.md) («Herramienta cota muro», fila «Alcance») y [`../ia-asistentes/ia-del-diseno-cotas-por-voz.md`](../ia-asistentes/ia-del-diseno-cotas-por-voz.md) §16.14 |

## Código común

Usa código común: lee [`Docs/codigo-comun.md`](../../codigo-comun.md) antes de cambiar funciones del visor.

## Reglas del área

- No hace falta leer nada de ZWCAD, AutoCAD ni OpenCAD para dibujar en Desing_2.
