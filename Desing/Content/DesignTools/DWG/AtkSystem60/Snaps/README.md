# Marcas de acoplamiento ATK-60 (autoría)

Fuente de verdad: JSON en esta carpeta (`{CodeName}.json`). El plugin leerá eso; **no** capas en el DWG.

No hace falta comprar una aplicación. Con ~1000 bloques el primer año, las marcas de **paneles rectangulares** se generan por regla (`width`, `height`, `pitch`), no a mano.

## Convención de la caja (panel 2,70 × 0,90)

- Unidades **mm**.
- Origen = **esquina inferior izquierda** de la cara (INSBASE 0,0,0).
- **X** = ancho (0 → 900).
- **Y** = alto (0 → 2700).
- **Z** = 0 (plano de la cara). Si modelas espesor, se añade después.

Dibuja tu caja coincidiendo con este rectángulo. Luego `INSERT` o `_DXFIN` de `27904209-marcas.dxf` para ver los círculos (solo guía humana; el runtime usa el JSON).

## Primer fichero

| Fichero | Uso |
|---------|-----|
| `27904209.json` | 16 nudos `GRAPA_V` (lados izq/der, cada 300 mm, inset 300) |
| `27904209-marcas.dxf` | Rectángulo + círculos para abrir en AutoCAD |

Si el marco real no cae cada 300 mm, se cambia `rules.GRAPA_V` y se regenera. No editar 1000 DWG.

Contrato runtime: [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](../../../../../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md) §6 y §6.1.
