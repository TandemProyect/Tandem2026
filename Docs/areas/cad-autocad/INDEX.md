# Área: Plugin AutoCAD

**Cubre:** plugin `net8.0-windows` para AutoCAD 2026: paletas, muro 2D/3D, bloquing, encofrado manual ATK-60, ATDESING.

**No cubre:** otros CAD.

## Código

- `TandemAutocadPlugin/AutocadPlugin/`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Estado y cómo continuar | [`autocad-plugin.md`](autocad-plugin.md) |
| Compilar y cargar | [`TandemAutocadPlugin/AutocadPlugin/README.md`](../../../TandemAutocadPlugin/AutocadPlugin/README.md) |
| Retomar encofrado manual / posición en visor | [`HANDOVER-2026-10-07-…`](historial/HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md), [`HANDOVER-2026-10-06-…`](historial/HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md) |
| Conexión ATDESING | [`HANDOVER-2026-10-03-…`](historial/HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md) |
| Sesión, bloquing, inserción de bloques | [`HANDOVER-2026-10-02-…`](historial/HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md), [`HANDOVER-2026-09-29-Autocad-Bloquing.md`](historial/HANDOVER-2026-09-29-Autocad-Bloquing.md), [`HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md`](historial/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md) |
| Artículos de Desing 2 sin bloque | [`articulos-desing2-sin-bloque-autocad.md`](articulos-desing2-sin-bloque-autocad.md) |

## Código común

[`../cad-comun/INDEX.md`](../cad-comun/INDEX.md); el encofrado manual usa el motor ATK-60: [`Docs/codigo-comun.md`](../../codigo-comun.md).

## Reglas del área

- Si AutoCAD bloquea la DLL, compila a otra carpeta (ver README del plugin).

## Historial (no leer salvo que haga falta)

- `historial/`: handovers de septiembre-octubre 2026 (bloquing, inserción, sesión, ATDESING, encofrado manual)
- `../cad-comun/historial/HANDOVER-2026-09-Plugins-Tandem-CAD.md`
