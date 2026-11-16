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

Toda la documentación está en [`Docs/areas/cad-zwcad/zwcad-plugin.md`](../../Docs/areas/cad-zwcad/zwcad-plugin.md):

- [README del proyecto](../../Docs/areas/cad-zwcad/zwcad-plugin.md) — arquitectura, comandos, MVVM
- [Guía técnica](../../Docs/areas/cad-zwcad/TECHNICAL_GUIDE.md) — compilación, deploy, debugging
- [Iconos](../../Docs/areas/cad-zwcad/iconos/README_ICONOS.md) — sistema de iconos del ribbon
- [Investigación US-619](../../Docs/areas/cad-zwcad/historial/INVESTIGACION_ICONOS_US619.md)
