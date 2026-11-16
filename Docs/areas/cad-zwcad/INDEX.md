# Área: Plugin ZWCAD

**Cubre:** plugin .NET Framework 4.8 para ZWCAD 2026: comandos, menú e iconos, instalación.

**No cubre:** otros CAD. Lo común está en `cad-comun`.

## Código

- `TamdenZwcadPluging/ZwcadPlugin/`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Visión general | [`zwcad-plugin.md`](zwcad-plugin.md) |
| Guía técnica | [`TECHNICAL_GUIDE.md`](TECHNICAL_GUIDE.md) |
| Instalar | [`README_INSTALACION.md`](README_INSTALACION.md) |
| Menú e iconos | [`MENU_INSTRUCCIONES.md`](MENU_INSTRUCCIONES.md), [`iconos/README_ICONOS.md`](iconos/README_ICONOS.md), [`iconos/GUIA_RAPIDA.md`](iconos/GUIA_RAPIDA.md) |
| Añadir un icono a un comando nuevo | [`INSTRUCCIONES_AGREGAR_ICONOS.md`](INSTRUCCIONES_AGREGAR_ICONOS.md), [`iconos/INSTRUCCIONES_CSPROJ.md`](iconos/INSTRUCCIONES_CSPROJ.md) |
| Comando seleccionar líneas | [`COMANDO_SELECCIONAR_LINEAS.md`](COMANDO_SELECCIONAR_LINEAS.md) |

## Código común

[`../cad-comun/INDEX.md`](../cad-comun/INDEX.md) y [`Docs/codigo-comun.md`](../../codigo-comun.md) §5 (archivos gemelos con AutoCAD).

## Reglas del área

- Compila con .NET Framework 4.8 (no net8).

## Historial (no leer salvo que haga falta)

- `historial/`: US-619 (icono), `TANDEM_SELECCIONAR_LINEAS`, imagen y tipificación de muros, resumen ATK60 de abril 2026
- `historial/US-613-INSTRUCCIONES.md`, `historial/INVESTIGACION_ICONOS_US619.md`, `historial/RESUMEN_CAMBIOS_SELECCIONAR_LINEAS.md`
- `test_icons/`: iconos y CUI de prueba
