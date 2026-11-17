# Área: Intranet (T Desing.net)

**Cubre:** pantallas CRUD (Materio), DataTables, paneles del inicio, formularios, direcciones Google Places, traducciones `.resx` + `TSql_UiTranslation`, foto del diseño y menú.

**No cubre:** el visor Desing_2.

## Código

- `Desing/Controllers/*Controller.cs` (salvo `Desing_2`, `DesignToolsAutocad`, `TandemXrApi`)
- `Desing/Views/**`, `Desing/Scripts/Intranet/**`, `Desing/Scripts/ResourceGenerators/**`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Visión general de la intranet | [`intranet-documentacion.md`](intranet-documentacion.md) |
| Formularios y estilo | [`UI-Formularios-y-Estilo.md`](UI-Formularios-y-Estilo.md) |
| Dirección con mapa | [`Google-Places-Direcciones.md`](Google-Places-Direcciones.md) |
| Layout Materio, DataTables, cookies | [`agente-ui-materio-datatables.md`](agente-ui-materio-datatables.md), [`intranet-scroll-layout.md`](intranet-scroll-layout.md) |
| Traducciones | [`agente-traducciones-modulos.md`](agente-traducciones-modulos.md) |
| Foto del diseño y «Últimos diseños» | [`agente-foto-diseno-y-menu.md`](agente-foto-diseno-y-menu.md) |
| Telegram: fotos al diseño (MVP) | [`TELEGRAM-MVP-FOTOS-DISENO.md`](TELEGRAM-MVP-FOTOS-DISENO.md) |
| Login Materio | [`HANDOVER-PLANTILLA-LOGIN-MATERIO-2026-05-08.md`](historial/HANDOVER-PLANTILLA-LOGIN-MATERIO-2026-05-08.md) |

## Código común

Helpers compartidos (`IntranetAuditHelper`, `LanguageUiHelper`…): [`Docs/codigo-comun.md`](../../codigo-comun.md) §2.

## Reglas del área

- Reglas de Cursor: `intranet-ui-forms.mdc`, `intranet-paneles-datatables.mdc`, `intranet-mvc-i18n-resx-uitranslation.mdc`.

## Historial (no leer salvo que haga falta)

- `historial/HANDOVER-SESION-2026-10-23.md`
