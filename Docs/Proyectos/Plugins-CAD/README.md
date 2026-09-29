# Plugins Tandem CAD (AutoCAD, BricsCAD, Revit)

Continuación del trabajo de plugins Tandem 2026, **aparte de ZWCAD**.

**Empezar aquí:**

- Muros, sesión, paletas modo/herramientas: [HANDOVER-2026-09-Plugins-Tandem-CAD.md](./HANDOVER-2026-09-Plugins-Tandem-CAD.md)
- Biblioteca de bloques AutoCAD (formulario listo): [HANDOVER-2026-09-29-Autocad-Bloquing.md](./HANDOVER-2026-09-29-Autocad-Bloquing.md) — local/producción §14.
- Inserción ATK-60 (3D / 3DRef, sin XREF; aún no código): [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md)

| Host | Código | Notas |
|------|--------|--------|
| AutoCAD 2026 | `TandemAutocadPlugin/` | NETLOAD |
| BricsCAD V26 | `TandemBricscadPlugin/` | NETLOAD |
| Revit 2026 | `TandemRevitPlugin/` | add-in + [MSI](../../TandemRevitPlugin/README.md) |

Geometría de muros: `LCornerDetector` vía `DesignToolsAutocad/ProcesarLineasZwcad`. No duplicar en los plugins.
