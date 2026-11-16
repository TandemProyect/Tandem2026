# Área: Muros 3D en Desing_2

**Cubre:** paso de líneas a muro, muro recto y curvo, uniones L/T/+, modificar esquina, exportar/importar muros cifrados.

**No cubre:** piezas de encofrado sobre el muro (→ `encofrado-atk60`).

## Código

- `master-article-details-stl-viewer.js` (muros, `maStlWall*`)
- `Desing/Scripts/MasterArticles/wall-edit-form.js`, `ma-stl-wall-edit-finalize.js`, `wall-bulk-list.js`
- `Desing/Scripts/Desing2/desing2-wall-exchange.js`
- `Desing/Repositories/RepositoryDesing2/DesignWallRepository.cs`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Herramientas de muro del visor (paralelas, curvo, esquina, cierre L) | [`muros-3d-visor.md`](muros-3d-visor.md) |
| Uniones T y cruce, `WallConnections.json` (solo lectura) | [`HANDOVER-MUROS-T-CRUCE.md`](HANDOVER-MUROS-T-CRUCE.md) |
| Exportar/importar muros cifrados | [`muro-json-cifrado.md`](muro-json-cifrado.md) |

## Código común

Usa código común: lee [`Docs/codigo-comun.md`](../../codigo-comun.md) antes de cambiar `wall-edit-form.js` (también lo carga la paleta de los plugins) o `RepositoryDesing2`.

## Reglas del área

- No toques `Desing/IA/Communication/WallConnections.json`.
