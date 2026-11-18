# Área: Base de datos y DAL

**Cubre:** esquema de develop y producción, scripts idempotentes, `Model.edmx`, limpieza de tablas antiguas.

**No cubre:** traducciones de la intranet (→ `intranet`).

## Código

- `DAL/` (`Model.edmx`, `Model.tt`, `ConexionData`)
- `Desing/Scripts/TemporalScript/*.sql`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Crear o copiar registros en develop y producción (usuarios `@tediseng.net`, derechos, catálogos), conexiones | [`sincronizar-develop-produccion.md`](sincronizar-develop-produccion.md) |
| Estado de develop/producción, lotes de limpieza, pendientes | [`base-de-datos-alineacion.md`](base-de-datos-alineacion.md) |
| Convenciones de tablas `TSql_*` | `.cursor/rules/sql-tsql-table-conventions.mdc` |
| DAL en detalle (antiguo) | [`DAL.md`](DAL.md) |

## Código común

Usa código común: lee [`Docs/codigo-comun.md`](../../codigo-comun.md) antes de cambiar `DAL`: lo usa todo `Desing`.

## Reglas del área

- develop y producción idénticas: mismo script en las dos, copia `.bak` antes, prueba en copia restaurada.
- Local `NUCBOXG5\SQLEXPRESS` con `-E`. Remotos (site4now) solo si Juan lo pide; nunca escribas contraseñas en documentos ni scripts.
- Si cambia el esquema del modelo: actualizar `Model.edmx` y regenerar T4 con TextTransform de VS2022.
