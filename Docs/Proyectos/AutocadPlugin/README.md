# AutocadPlugin — AutoCAD 2026

Código: `TandemAutocadPlugin/AutocadPlugin/`

**Estado actual y cómo continuar:**

- Muros / sesión: [HANDOVER-2026-09-Plugins-Tandem-CAD.md](../Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md)
- Biblioteca de bloques: [HANDOVER-2026-09-29-Autocad-Bloquing.md](../Plugins-CAD/HANDOVER-2026-09-29-Autocad-Bloquing.md)
- Inserción 3D/3DRef: [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](../Plugins-CAD/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md)

## Cargar

```text
dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
```

En AutoCAD 2026: `NETLOAD` de `TandemAutocadPlugin\AutocadPlugin\bin\Debug\AutocadPlugin.dll`  
(si la DLL está bloqueada, `OutputPath=bin\DebugN\`).

Pestaña **Tandem 2026** o `TANDEM`. Desing en `https://localhost:44384/`.

Comandos de trabajo: `TANDEM_MURO2D`, `TANDEM_MURO3D` / `GENERAR3D`.
