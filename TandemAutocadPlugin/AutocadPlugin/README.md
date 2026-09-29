# AutocadPlugin — Plugin Tandem 2026 para AutoCAD

Class Library (`net8.0-windows`) para AutoCAD 2026. Paletas MVC, muro 2D y generar 3D.

**Handover (continuar aquí):** [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md)

## Compilar

Abrir `Design.sln` y compilar AutocadPlugin. Requisito: AutoCAD 2026 en `C:\Program Files\Autodesk\AutoCAD 2026\` (o `AUTOCAD_API_ROOT`).

```
dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
```

Si AutoCAD deja la DLL bloqueada, usa otra carpeta: `-p:OutputPath=bin\Debug9\` y NETLOAD esa.

## Cargar en AutoCAD

```
NETLOAD → TandemAutocadPlugin\AutocadPlugin\bin\Debug\AutocadPlugin.dll
```

Pestaña **Tandem 2026** o comando `TANDEM`. Por defecto habla con **https://tdesing.net/**. Para IIS Express local: `TANDEM_MVC_BASE_URL=https://localhost:44384/`.

Si no hay sesión, aparece el **login de Desing**. Tras conectar, una paleta lista diseños; al elegir uno se dibujan los muros (`TSql_DesignWall`).

Abre paletas (sin título, compactas, se mueven):

- `DesignToolsAutocad/PaletteMode` — modo + Atk-60
- `DesignToolsAutocad/PaletteTools` — herramientas CAD de Desing_2

No se usa el submenú CUI clásico (Panel / Detectar / …).

## Comandos

| Comando | Uso |
|---------|-----|
| `TANDEM` | Mostrar/ocultar paletas MVC |
| `TANDEM_MURO2D` | Dibujar muro 2D (eje + caras 300 mm) |
| `TANDEM_MURO3D` / `GENERAR3D` | Sólidos 3D vía `LCornerDetector` |
| `TANDEM_PROBAR_CONEXION` | Ping al MVC |
| `TANDEM_CARGAR_MENU` | Recarga ribbon |

Hay stubs heredados de ZWCAD (`INSERTARBLOQUE`, `DETECTARMUROS`, …) sin lógica de negocio; no hacen falta para el flujo actual.

## Documentación

[`Docs/Proyectos/Plugins-CAD/`](../../Docs/Proyectos/Plugins-CAD/)
