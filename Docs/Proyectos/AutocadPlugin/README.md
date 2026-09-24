# AutocadPlugin — Plugin para AutoCAD 2026

Plugin Tandem 2026 paralelo a ZwcadPlugin. Target: AutoCAD 2026 (`net8.0-windows`).

## Ubicación

`TandemAutocadPlugin/AutocadPlugin/`

## Cargar

1. Compilar AutocadPlugin.
2. En AutoCAD 2026: `NETLOAD` → `bin\Debug\AutocadPlugin.dll`.
3. Pestaña **Tandem 2026** → **Menús**, o comando `TANDEM`.
4. Salen dos paletas MVC (mismos botones que Desing_2). Desing tiene que estar en marcha.

API: `C:\Program Files\Autodesk\AutoCAD 2026\` (o `AUTOCAD_API_ROOT`).

## Menú

Misma estructura que ZWCAD:

- Principal: Panel
- Modelo 3D: Detectar / Generar 3D / Regenerar
- Datos MVC: Leer / Crear / Guardar
- Herramientas: Seleccionar / Analizar Img

El ribbon se crea en runtime (AdWindows). `MNU/Tandem2026.cui` queda para MENULOAD/CUILOAD.

Los comandos de negocio están registrados; la lógica (WPF, muros, 3D) se porta a continuación.
