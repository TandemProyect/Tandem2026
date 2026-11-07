# TandemOpenCadStudioPlugin — Menu Tandem 2026 (OpenCADStudio)

Plugin nativo (`cdylib`) para OpenCADStudio que delega acciones al bridge C# [OpenCadStudioPluging](../OpenCadStudioPluging/README.md).

## Objetivo

- Exponer comando `TANDEM` en OpenCADStudio sin submenus de ribbon.
- Reutilizar logica existente del bridge C# (MVC + `--serve`).
- Mantener la regla de arquitectura: negocio/geometria en `Desing`, cliente CAD como adaptador.
- Abrir login MVC una vez por arranque y luego mostrar barras MVC (`PaletteMode` y `PaletteTools`).

## Comandos

- `TANDEM` → `OpenCadStudioPluging.exe connect-ui`
- `TANDEM_MURO2D` → inicia comando interactivo; cada clic consulta `Desing/DesignToolsAutocad/PluginWall2dStep`
  (C#) para resolver geometría incremental de muro (eje/caras) y OpenCAD solo la renderiza.
  Los tramos intermedios reservan ajuste de conexión y al terminar con Enter se finaliza el último extremo abierto.
  En esa finalización, el plugin recompone el grupo del muro para evitar duplicados y aplica corte de cara en encuentros T interiores.
  Los ejes existentes se envían al servidor para que Desing resuelva también encuentros en T/+ sin lógica geométrica local.
  Si el usuario cancela con `ESC`, el plugin lanza una finalización/revisión automática del último tramo usando el mismo solver C#.

El flujo se ejecuta desde la barra de comandos superior, sin submenus ni botones auxiliares en ribbon.
Ademas, el plugin abre Tandem automaticamente cuando se abre un dibujo (`DocumentOpened`).

## Variables de entorno

- `TANDEM_OPENCAD_BRIDGE` (preferida) o `TANDEM_OPENCADSTUDIO_BRIDGE`: ruta absoluta al `OpenCadStudioPluging.exe`.

Si no existen, el plugin prueba estas rutas:
1. Mismo directorio del plugin.
2. `Host\OpenCadStudioPluging.exe` relativo al plugin.
3. `%APPDATA%\Tandem\OpenCadStudioPluging\Host\OpenCadStudioPluging.exe`.
4. `OpenCadStudioPluging.exe` en `PATH`.

## Build local

```powershell
cargo build --release
```

## Instalacion local (dev)

Instalacion automatica (recomendada):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File TandemOpenCadStudioPlugin\install-local.ps1
```

Opcionalmente puedes forzar rutas:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File TandemOpenCadStudioPlugin\install-local.ps1 `
  -PluginDllPath "C:\ruta\plugin.dll" `
  -BridgeSourceDir "C:\00_Tandem2026\OpenCadStudioPluging\bin\Debug"
```

Este script:
- Copia `plugin.toml` + DLL a `%APPDATA%\OpenCADStudio\plugins\tandem.opencadstudio\`
- Copia el bridge C# a `%APPDATA%\Tandem\OpenCadStudioPluging\Host\`
- Muestra errores claros si falta compilar Rust o falta el bridge.

## Troubleshooting rapido

- Si no aparece el menu:
  1. Verifica que existe `%APPDATA%\OpenCADStudio\plugins\tandem.opencadstudio\`
  2. Dentro debe haber `plugin.toml` + una `.dll`
  3. Reinicia OpenCADStudio (el plugin carga al iniciar)

## Handoff de estado funcional (L/T/+)

Para continuar este trabajo con otro agente sin perder contexto técnico y errores ya cometidos:

- [HANDOFF-MURO2D-OPENCAD.md](./HANDOFF-MURO2D-OPENCAD.md)
