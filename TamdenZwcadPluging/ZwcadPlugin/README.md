# ZwcadPlugin — Plugin Tandem 2026 para ZWCAD

Class Library (.NET Framework 4.8) para ZWCAD 2026. Desde el 2026-10-07 lleva el encofrado manual que estaba en el plugin de AutoCAD: paleta Desing, insertar paneles ATK-60, simétrico, salvar y abrir diseño.

Cargar:

```
NETLOAD → TamdenZwcadPluging\ZwcadPlugin\bin\Debug\ZwcadPlugin.dll
```

Comandos de trabajo: `TANDEM` (paleta), `ATDESING` (biblioteca local, una vez), `TANDEM_SALVAR`, `TANDEM_ABRIRDISENO`, `INSERTARBLOQUE`. `HOLA` lista el resto. `TANDEM_SELECCIONAR_LINEAS` sigue en el código anterior.

## Compilar

Abrir `TamdenZwcadPluging/ZwcadPlugin.slnx` en Visual Studio y compilar (Ctrl+Shift+B).

Requisito: ZWCAD 2026 instalado en `C:\Program Files\ZWSOFT\ZWCAD 2026\` (o definir `ZWCAD_API_ROOT`).

## Cargar en ZWCAD

```
NETLOAD → bin\Debug\ZwcadPlugin.dll
```

## Estructura

```
ZwcadPlugin/
├── Commands.cs, CuixBuilder.cs, MenuManager.cs …
├── MNU/           Menú CUI e iconos
├── UI/            Ventanas WPF (Views + ViewModels)
└── lib/           Newtonsoft.Json.dll
```

## Documentación

Toda la documentación está en [`Docs/Proyectos/ZwcadPlugin/`](../../Docs/Proyectos/ZwcadPlugin/):

- [README del proyecto](../../Docs/Proyectos/ZwcadPlugin/README.md) — arquitectura, comandos, MVVM
- [Guía técnica](../../Docs/Proyectos/ZwcadPlugin/TECHNICAL_GUIDE.md) — compilación, deploy, debugging
- [Iconos](../../Docs/Proyectos/ZwcadPlugin/Iconos/README_ICONOS.md) — sistema de iconos del ribbon
- [Investigación US-619](../../Docs/Proyectos/ZwcadPlugin/INVESTIGACION_ICONOS_US619.md)
