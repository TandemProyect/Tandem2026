# Plugins Tandem CAD (AutoCAD, BricsCAD, Revit)

Continuación del trabajo de plugins Tandem 2026, **aparte de ZWCAD**.

**Empezar aquí (AutoCAD, corte 2026-10-03):**

- **Retomar hoy:** [HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md](./HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md) — NETLOAD vs ATDESING, test de conexión, Git `develo`/`master`, US Encofrar manualmente.
- Sesión / home / bloquing (2026-10-02): [HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md](./HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md) — sesión persistente, menú obras/ofertas, biblioteca en AppData, insert/convert, UnAtdesing.
- Muros / paletas modo-herramientas (base): [HANDOVER-2026-09-Plugins-Tandem-CAD.md](./HANDOVER-2026-09-Plugins-Tandem-CAD.md)
- Bloquing (formulario, contexto sept-29): [HANDOVER-2026-09-29-Autocad-Bloquing.md](./HANDOVER-2026-09-29-Autocad-Bloquing.md)
- Contrato de inserción ATK-60 (geométrico; el código **ya existe**): [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md)

| Host | Código | Notas |
|------|--------|--------|
| AutoCAD 2026 | `TandemAutocadPlugin/` | NETLOAD |
| BricsCAD V26 | `TandemBricscadPlugin/` | NETLOAD |
| Revit 2026 | `TandemRevitPlugin/` | add-in + [MSI](../../TandemRevitPlugin/README.md) |

Geometría de muros: `LCornerDetector` vía `DesignToolsAutocad/ProcesarLineasZwcad`. No duplicar en los plugins.
