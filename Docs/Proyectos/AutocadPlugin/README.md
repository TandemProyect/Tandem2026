# AutocadPlugin — AutoCAD 2026

Código: `TandemAutocadPlugin/AutocadPlugin/`

**Estado actual y cómo continuar:**

- **Retomar (2026-10-02):** [HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md](../Plugins-CAD/HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md)
- Muros / paletas base: [HANDOVER-2026-09-Plugins-Tandem-CAD.md](../Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md)
- Bloquing (contexto sept-29): [HANDOVER-2026-09-29-Autocad-Bloquing.md](../Plugins-CAD/HANDOVER-2026-09-29-Autocad-Bloquing.md)
- Contrato geométrico INSERT (código ya existe): [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](../Plugins-CAD/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md)

## Cargar

```text
dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
```

En AutoCAD 2026: `NETLOAD` de `TandemAutocadPlugin\AutocadPlugin\bin\Debug\AutocadPlugin.dll`  
(si la DLL está bloqueada, `OutputPath=bin\DebugN\`).

Pestaña **Tandem 2026** o `TANDEM`. Desing en `https://localhost:44384/`.

Comandos de trabajo: `TANDEM_MURO2D`, `TANDEM_MURO3D` / `GENERAR3D`.
