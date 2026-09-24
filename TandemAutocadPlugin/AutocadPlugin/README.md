# AutocadPlugin — Plugin Tandem 2026 para AutoCAD

Class Library (`net8.0-windows`) que expone comandos, ribbon y CUI para AutoCAD 2026.

## Compilar

Abrir `TandemAutocadPlugin/AutocadPlugin.slnx` o `Design.sln` y compilar AutocadPlugin.

Requisito: AutoCAD 2026 en `C:\Program Files\Autodesk\AutoCAD 2026\` (o definir `AUTOCAD_API_ROOT`).

```
dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
```

## Cargar en AutoCAD

```
NETLOAD → TandemAutocadPlugin\AutocadPlugin\bin\Debug\AutocadPlugin.dll
```

Tras NETLOAD: pestaña **Tandem 2026** → **Menús**, o comando `TANDEM`.
Abre dos formularios MVC (Desing debe estar en IIS Express):

- `DesignToolsAutocad/PaletteMode` — modo + Atk-60
- `DesignToolsAutocad/PaletteTools` — herramientas CAD de Desing_2

Barra clásica (opcional): `MENUBAR` = 1.

## Comandos

| Comando | Estado |
|---------|--------|
| `TANDEM` | Lista de comandos |
| `TANDEM_CARGAR_MENU` | Recarga ribbon + CUI |
| `TANDEM_PROBAR_CONEXION` | Ping al MVC |
| `TANDEM_DEVICE_ID` | DeviceId de autorización |
| `HOLA` | Ayuda |
| Resto del menú ZWCAD | Registrados; lógica pendiente de portar |

## Estructura

```
AutocadPlugin/
├── Commands.cs, MenuManager.cs, CuixBuilder.cs
├── MVCApiService.cs, Models.cs, PluginExceptionHelper.cs
└── MNU/           CUI e iconos
```

## Documentación

[`Docs/Proyectos/AutocadPlugin/`](../../Docs/Proyectos/AutocadPlugin/)
