# Área: Visor Desing_2 (núcleo)

**Cubre:** arranque del visor, cámara, órbita y cubo de vistas, rejilla, reglas y punto de inserción, colores, pestañas Entorno y Configuración, cookie de estado.

**No cubre:** herramientas de dibujo 2D (→ `desing2-dibujo-2d`), muros 3D (→ `desing2-muros-3d`), encofrado (→ `encofrado-atk60`), IA (→ `ia-asistentes`).

## Código

- `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`
- `Desing/Views/Desing_2/` (`Viewer.cshtml`, `_Desing2StlViewerWorkspace.cshtml`, barras)
- `Desing/Scripts/Desing2/desing2-stl-viewer-toolbar-wiring.js`, `desing2-dim-kinds.js`, `desing2-connection-test.js`
- `Desing/Controllers/Desing_2Controller.cs`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Buscar una función del visor o la cookie | [`visor-funciones.md`](visor-funciones.md) |
| Tocar órbita, pivote o cubo 90° | [`desing-2-orbit-pivot.md`](desing-2-orbit-pivot.md) (regla `.cursor/rules/desing-2-view-cube-orbit.mdc`: no regredir) |
| Arrancar el proyecto en local, Ortho/F8 | [`historial/HANDOVER-DESING2-MUROS-ARRANQUE.md`](historial/HANDOVER-DESING2-MUROS-ARRANQUE.md) |

## Código común

Usa código común: lee [`Docs/codigo-comun.md`](../../codigo-comun.md) antes de cambiar el visor: `master-article-details-stl-viewer.js` lo usan 2D, 3D, encofrado, IA y `MasterArticles`.

## Reglas del área

- Sube `?v=` en `Viewer.cshtml` y en los imports al cambiar JS.
- Las fichas de 2D, 3D e IA salen del antiguo `desing-2-stl-viewer-funciones.md` (partido el 14/11/2026).

## Historial (no leer salvo que haga falta)

- `historial/HANDOVER-DESING2-MUROS-ARRANQUE.md` (julio 2026)
