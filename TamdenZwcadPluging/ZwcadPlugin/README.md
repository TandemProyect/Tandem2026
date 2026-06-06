# ZwcadPlugin — Plugin Tandem 2026 para ZWCAD

Class Library (.NET Framework 4.8) que expone comandos, menú CUI y UI WPF para ZWCAD 2026.

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
