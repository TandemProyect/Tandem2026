# OpenCadStudioPluging — Bridge Tandem 2026 para OpenCADStudio

Proyecto C# (`net8.0`) para integrar Tandem con OpenCADStudio mediante el canal nativo `--serve`.

La logica geometrica y de negocio sigue en `Desing` (MVC); este proyecto solo orquesta:

1. Conexion con MVC para leer/escribir datos.
2. Ejecucion de comandos en OpenCADStudio para insertar geometria.
3. Host UI WebView2 para login MVC + barras `PaletteMode` y `PaletteTools`.

Este ejecutable tambien es el backend del menu nativo Rust en
[`TandemOpenCadStudioPlugin/`](../TandemOpenCadStudioPlugin/README.md).

## Arquitectura

- **Servidor**: `Desing/` resuelve negocio + geometria.
- **Cliente CAD**: `OpenCadStudioPluging/` adapta datos al host OpenCADStudio.
- **Canal OpenCADStudio**: proceso local `OpenCADStudio --serve` (JSON lineal por stdin/stdout).

## Compilar

```powershell
dotnet build OpenCadStudioPluging\OpenCadStudioPluging.csproj -c Debug
```

## Comandos

```text
OpenCadStudioPluging.exe ping
OpenCadStudioPluging.exe tandem
OpenCadStudioPluging.exe show-server
OpenCadStudioPluging.exe set-local
OpenCadStudioPluging.exe set-production
OpenCadStudioPluging.exe ocs-smoke
OpenCadStudioPluging.exe ocs-run "LINE 0,0 1000,0"
OpenCadStudioPluging.exe ocs-run "TANDEM_MURO2D"
OpenCadStudioPluging.exe ui-run "TANDEM_MURO2D"
OpenCadStudioPluging.exe wall-step 0.30 1 "0,0,0;10,0,0;10,5,0" "0,0,0,10,0,0"
OpenCadStudioPluging.exe ocs-insert-line 0 0 1000 0
```

`wall-step` consulta `DesignToolsAutocad/PluginWall2dStep` en `Desing` y devuelve
las líneas incrementales (`axis`/`face`) que OpenCAD debe dibujar en ese clic.
`r=1` reserva extremo para la conexión del siguiente punto; `r=0` finaliza abierto.
El último argumento (`s1;...;sn`) es opcional y representa ejes existentes para
que Desing resuelva encuentros en T/+ en el servidor.

`tandem` (o `connect-ui`) abre el login y las barras MVC. La autenticacion se
recuerda por servidor durante el arranque actual del equipo; tras reiniciar,
vuelve a pedir login.

## Configuracion

- `TANDEM_MVC_BASE_URL`: fuerza URL MVC (si no, usa `set-local` / `set-production`).
- `TANDEM_OPENCADSTUDIO_PATH`: ruta completa a `OpenCADStudio.exe` si no esta en `PATH`.

## Tests

```powershell
dotnet test OpenCadStudioPluging.Tests\OpenCadStudioPluging.Tests.csproj -c Debug
```
