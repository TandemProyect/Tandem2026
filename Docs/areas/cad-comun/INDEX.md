# Área: Plugins CAD — parte común

**Cubre:** lo que comparten todos los plugins: API `DesignToolsAutocad/*`, sesión y autorización de equipo, paletas MVC en WebView, detección de muros en servidor.

**No cubre:** lo propio de cada CAD (→ su área).

## Código

- `Desing/Controllers/DesignToolsAutocadController.cs`
- `Desing/Views/DesignToolsAutocad/*`
- `Desing/Helpers/PluginCadDeviceHelper.cs`, `PluginInstallLink.cs`, `PluginCadAtk60PanelStlHelper.cs`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Regla de arquitectura y estado de los plugins | [`plugins-cad.md`](plugins-cad.md) |
| Resumen de septiembre-octubre (paletas, muros, ATDESING, encofrado manual) | [`RESUMEN-2026-09-10-Trabajo-Agentes.md`](RESUMEN-2026-09-10-Trabajo-Agentes.md) |
| Autorización de equipo | [`HANDOVER-PLUGIN-DEVICE-AUTH.md`](HANDOVER-PLUGIN-DEVICE-AUTH.md) |
| Arquitectura cliente-servidor | [`Docs/areas/general/Arquitectura-Cliente-Servidor.md`](../general/Arquitectura-Cliente-Servidor.md) |

## Código común

Todo esto es código común: [`Docs/codigo-comun.md`](../../codigo-comun.md) §2, §3 y §5.

## Reglas del área

- Cambiar una respuesta de `DesignToolsAutocad/*` afecta a todos los plugins.
- AutoCAD y ZWCAD tienen ~40 archivos copiados: arreglar en uno, revisar el otro.

## Historial (no leer salvo que haga falta)

- `historial/HANDOVER-2026-09-Plugins-Tandem-CAD.md` (semana 22–28 sep 2026: AutoCAD, BricsCAD, Revit)
