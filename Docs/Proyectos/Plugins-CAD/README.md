# Plugins Tandem CAD (AutoCAD, BricsCAD, Revit)

Continuación del trabajo de plugins Tandem 2026, **aparte de ZWCAD**.

**Empezar aquí:** [HANDOVER-2026-09-Plugins-Tandem-CAD.md](./HANDOVER-2026-09-Plugins-Tandem-CAD.md)

| Host | Código | Notas |
|------|--------|--------|
| AutoCAD 2026 | `TandemAutocadPlugin/` | NETLOAD |
| BricsCAD V26 | `TandemBricscadPlugin/` | NETLOAD |
| Revit 2026 | `TandemRevitPlugin/` | add-in + [MSI](../../TandemRevitPlugin/README.md) |

Geometría de muros: `LCornerDetector` vía `DesignToolsAutocad/ProcesarLineasZwcad`. No duplicar en los plugins.
