# FreeCadPluging — Plugin Tandem 2026 para FreeCAD

Proyecto C# principal del plugin FreeCAD. FreeCAD no carga DLL .NET como AutoCAD (`NETLOAD`), por eso el workbench Python de `Workbench/Tandem2026` es solo un cargador minimo: registra el menu superior **Tandem 2026** y llama al host C# compilado.

## Arquitectura

La regla principal es que la logica C# debe ser la misma para FreeCAD, AutoCAD, BricsCAD, Revit y el proyecto `Desing`. `FreeCadPluging` no debe convertirse en una implementacion paralela: debe consumir codigo comun C# y endpoints MVC comunes. Solo debe contener lo imprescindible para adaptar FreeCAD al nucleo compartido.

Puede existir codigo especifico de FreeCAD cuando el host lo exige. En este proyecto eso incluye el Workbench Python, que registra menus y comandos porque FreeCAD funciona asi. Ese codigo Python no debe contener reglas de negocio, geometria, catalogos ni validaciones funcionales; debe delegar en el host C# o en MVC.

| Pieza | Rol |
|-------|-----|
| `FreeCadPluging.csproj` | Proyecto Visual Studio / .NET del plugin |
| `Program.cs` | Host C# inicial: servidor local/produccion, ping MVC, comandos base |
| `Workbench/Tandem2026/InitGui.py` | Registro del workbench en FreeCAD |
| `Workbench/Tandem2026/tandem_commands.py` | Puente Python -> `Host/FreeCadPluging.exe` |

Las pantallas y herramientas deben venir de MVC o de librerias C# comunes, igual que AutoCAD/BricsCAD. El codigo especifico de plugin solo adapta el host CAD.

## Codigo comun obligatorio

La direccion tecnica es extraer y usar una capa comun C# para compartir, como minimo:

- DTOs y modelos de intercambio entre CAD y `Desing`.
- Cliente HTTP, configuracion de servidor local/produccion y sesion.
- Geometria y calculos reutilizables, incluyendo deteccion de esquinas y muros.
- Catalogos, articulos, reglas de seleccion y validaciones.
- Contratos de comandos que los plugins puedan exponer con menus distintos.

Cada plugin puede tener clases propias solo para resolver diferencias del host: `NETLOAD` en AutoCAD/BricsCAD, add-in en Revit, Workbench Python en FreeCAD, seleccion de entidades nativas, conversion de coordenadas y generacion final de objetos nativos.

## Compilar e instalar

```powershell
dotnet build FreeCadPluging\FreeCadPluging.csproj -c Debug
```

El build copia automaticamente el workbench a:

```text
%APPDATA%\FreeCAD\v1-1\Mod\Tandem2026
```

Despues cierra y abre FreeCAD. Selecciona el workbench **Tandem 2026** en el desplegable superior.

## Comandos actuales

| Comando FreeCAD | Ejecuta en C# |
|-----------------|---------------|
| Conectar | `FreeCadPluging.exe ping` |
| Servidor local | `FreeCadPluging.exe set-local` |
| Servidor produccion | `FreeCadPluging.exe set-production` |
| Mostrar servidor | `FreeCadPluging.exe show-server` |
| Muro 2D | `FreeCadPluging.exe wall-2d` |
| Generar 3D | `FreeCadPluging.exe wall-3d` |

## Siguiente paso tecnico

Extraer una libreria comun C# tipo `TandemCad.Core` y hacer que `Desing`, AutoCAD, BricsCAD, Revit y FreeCAD lean de esa misma base. No duplicar logica en `FreeCadPluging`.