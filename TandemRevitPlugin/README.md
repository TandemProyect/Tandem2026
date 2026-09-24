# Tandem 2026 — Plugin para Revit 2026

Mismo flujo que AutoCAD / BricsCAD: paletas MVC de Desing_2, muro 2D y **Generar muros 3D** con `LCornerDetector` (`DesignToolsAutocad/ProcesarLineasZwcad`).

No incluye código del plugin de empresa. Solo se reutilizó la **forma de instalar** en Revit (add-in por usuario + MSI WiX).

## Requisitos

- Autodesk Revit 2026 (x64)
- Windows 10/11
- .NET 8 (incluido con Revit 2026)
- Proyecto **Desing** en marcha (`https://localhost:44384/`) para las paletas y el 3D

## Cómo se instala (usuarios)

El instalador es **por usuario**: no pide administrador.

1. Compila el plugin (genera `bundle\`) y luego el MSI, o usa el MSI ya generado.
2. Ejecuta `Tandem.Revit.v2026.Installer.msi`.
3. Arranca **Revit 2026**.
4. En la cinta aparece la pestaña **Tandem 2026** (Menús, Muro 2D, Generar 3D).

### Dónde queda

| Qué | Ruta |
|-----|------|
| Manifiesto | `%AppData%\Autodesk\Revit\Addins\2026\Tandem.addin` |
| DLLs | `%AppData%\Autodesk\Revit\Addins\2026\Tandem.Revit.Addin\` |
| Registro | `HKCU\SOFTWARE\Tandem\Tandem.Plugin.Revit` |

Desinstalar: *Aplicaciones* de Windows o el propio MSI.

## Instalación manual (desarrollo)

Tras `dotnet build` el propio proyecto **copia** los ficheros a esas rutas de `%AppData%`.

Si lo haces a mano:

1. Copia `Revit\Tandem.addin` a `%AppData%\Autodesk\Revit\Addins\2026\`
2. Copia las DLL del output (y `WebView2Loader.dll`) a `%AppData%\Autodesk\Revit\Addins\2026\Tandem.Revit.Addin\`
3. El `.addin` apunta a `.\Tandem.Revit.Addin\Tandem.Revit.dll` (ruta relativa al manifiesto).
4. Cierra y abre Revit 2026.

## Compilar

```powershell
# Plugin (también copia a %AppData% y a TandemRevitPlugin\bundle\)
dotnet build TandemRevitPlugin\Revit\Tandem.Revit.csproj -c Debug

# Instalador MSI (WiX v4, per-user)
dotnet build TandemRevitPlugin\Installer\Installer.wixproj -c Release
```

El MSI sale en `TandemRevitPlugin\Installer\bin\x64\Release\Tandem.Revit.v2026.Installer.msi`.

API de Revit: `C:\Program Files\Autodesk\Revit 2026\` (o variable `REVIT_API_ROOT`).

## Uso

1. Desing en IIS Express.
2. Revit → pestaña **Tandem 2026** → **Menús** (paletas flotantes, mismos botones que Desing_2).
3. Vista de **planta** → **Muro 2D** (eje + dos caras, espesor 300 mm).
4. **Generar 3D**: llama al detector compartido y crea sólidos `TANDEM_MURO_3D`. Sale el popup “Generando modelo de muros…”.

Comandos nativos de Revit (Mover, Copiar, Recortar, etc.) se lanzan desde la paleta de herramientas.

## Arquitectura de instalación (referencia)

Revit solo carga add-ins si hay un `.addin` en:

- `%AppData%\Autodesk\Revit\Addins\2026\` (usuario, sin admin)
- o `C:\ProgramData\Autodesk\Revit\Addins\2026\` (todos los usuarios)

El MSI usa **WiX Toolset v4**, `Scope=perUser`, upgrade automático y desinstalación limpia. Misma idea que un instalador de add-in Revit estándar: manifiesto en `Addins\2026\` y binarios en una subcarpeta.
