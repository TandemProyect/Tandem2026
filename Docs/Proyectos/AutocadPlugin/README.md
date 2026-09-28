# AutocadPlugin — AutoCAD 2026

Código: `TandemAutocadPlugin/AutocadPlugin/`

**Estado actual y cómo continuar:**  
[HANDOVER-2026-09-Plugins-Tandem-CAD.md](../Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md)

## Cargar

```text
dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
```

En AutoCAD 2026: `NETLOAD` de `TandemAutocadPlugin\AutocadPlugin\bin\Debug\AutocadPlugin.dll`  
(si la DLL está bloqueada, `OutputPath=bin\DebugN\`).

Pestaña **Tandem 2026** o `TANDEM`. Desing en `https://localhost:44384/`.

Comandos de trabajo: `TANDEM_MURO2D`, `TANDEM_MURO3D` / `GENERAR3D`.
