# AutocadPlugin — Plugin Tandem 2026 para AutoCAD

Class Library (`net8.0-windows`) para AutoCAD 2026. Paletas MVC, muro 2D y generar 3D.

**Handover:**

- Muros / sesión: [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md)
- Biblioteca de bloques (formulario listo): [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Bloquing.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Bloquing.md)
- Inserción ATK-60 3D/3DRef (contrato, sin código): [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md)

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

Pestaña **Tandem 2026** o comando `TANDEM`. Por defecto habla con **https://localhost:44384/** (`TANDEM_LOCAL`). Producción: `TANDEM_PRODUCCION` (tdesing.net). Override: `TANDEM_MVC_BASE_URL`.

Si no hay sesión, aparece el **login de Desing**. Tras conectar, paletas de modo/herramientas; el primer botón abre el menú de diseños. Biblioteca de bloques: botón de la barra de modo o `INSERTARBLOQUE`.

Abre paletas (sin título, compactas, se mueven):

- `DesignToolsAutocad/PaletteMode` — modo + Atk-60 + abrir bloques
- `DesignToolsAutocad/PaletteTools` — herramientas CAD de Desing_2
- `DesignToolsAutocad/PluginBlocks` — biblioteca (formulario; no inserta aún)

Cada `dotnet build` copia a `bin\DebugN` si AutoCAD bloquea `bin\Debug`. **Cerrar AutoCAD** antes de NETLOAD de una DLL nueva; si no, los mensajes `collapse-blocks` se imprimen como `[Tandem paleta]`.

No se usa el submenú CUI clásico (Panel / Detectar / …).

## Comandos

| Comando | Uso |
|---------|-----|
| `TANDEM` | Mostrar/ocultar paletas MVC |
| `TANDEM_MURO2D` | Dibujar muro 2D (eje + caras 300 mm) |
| `TANDEM_MURO3D` / `GENERAR3D` | Sólidos 3D vía `LCornerDetector` |
| `TANDEM_PROBAR_CONEXION` | Ping al MVC |
| `TANDEM_LOCAL` | Destino IIS Express (`localhost:44384`) y recarga paletas |
| `TANDEM_PRODUCCION` | Destino tdesing.net y recarga paletas |
| `TANDEM_SERVIDOR` | Muestra URL activa |
| `TANDEM_CARGAR_MENU` | Recarga ribbon |
| `UnAtdesing` | Desarrolladores: borra sesión, cookies WebView2 y biblioteca local (confirma con `SI`). Si AutoCAD bloquea ficheros, cerrar CAD y `reset-cad-dev-local.ps1` |

`INSERTARBLOQUE` abre la biblioteca de bloques (formulario). La inserción en el DWG está pendiente (ver handover bloquing). Otros stubs ZWCAD (`DETECTARMUROS`, …) siguen sin lógica.

## Documentación

[`Docs/Proyectos/Plugins-CAD/`](../../Docs/Proyectos/Plugins-CAD/)
