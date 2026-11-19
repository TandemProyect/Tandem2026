# Oferta Diseño

Resumen operativo (compacto) para continuidad entre agentes.

## Objetivo funcional

Generar y editar una oferta de cliente desde diseño, con:

- plantilla seleccionable,
- contacto del cliente con estados,
- materiales/recuento,
- PDF,
- capturas de planos del visor insertadas entre materiales y despedida.

## Flujo actual

1. Desde visor Desing_2 se abre oferta (`OpenFromDesign`).
2. En oferta, el usuario pulsa **Adjuntar captura**.
3. Se abre visor en modo captura (sin UI en imagen).
4. En visor, **Foto del diseño** adjunta la captura a la oferta (repetible N veces).
5. En edición/print/PDF aparecen las capturas en bloque de planos.

## Estructura BD (SQL Server)

Tablas de oferta:

- `TSql_ClientOffer`
- `TSql_ClientOfferBrand`
- `TSql_ClientOfferSection`
- `TSql_ClientOfferAiMessage`
- `TSql_ClientOfferContact`
- `TSql_ClientOfferContactStateType`
- `TSql_ClientOfferContactState`
- `TSql_ClientOfferCapture` (nueva para capturas)

Reglas clave:

- estados base de contacto: `ALTA`, `BAJA`, `ENFERMO`;
- contacto por defecto único por cliente;
- contactos en BAJA/ENFERMO no válidos para default/selección operativa;
- múltiples capturas por oferta con orden.

## Scripts SQL relevantes (idempotentes)

- [2026-11-18_create_TSql_ClientOffer.sql](C:/Users/jag/00_Tandem2026/Desing/Scripts/TemporalScript/2026-11-18_create_TSql_ClientOffer.sql)
- [2026-11-18_alter_TSql_ClientOffer_add_contacts_and_template_seed.sql](C:/Users/jag/00_Tandem2026/Desing/Scripts/TemporalScript/2026-11-18_alter_TSql_ClientOffer_add_contacts_and_template_seed.sql)
- [2026-11-18_sync_clientoffer_contact_states_refactor.sql](C:/Users/jag/00_Tandem2026/Desing/Scripts/TemporalScript/2026-11-18_sync_clientoffer_contact_states_refactor.sql)
- [2026-11-18_alter_TSql_ClientOffer_add_captures.sql](C:/Users/jag/00_Tandem2026/Desing/Scripts/TemporalScript/2026-11-18_alter_TSql_ClientOffer_add_captures.sql)

Orden recomendado en un entorno nuevo: create -> contacts/refactor -> captures.

## Código principal

Backend oferta:

- [ClientOfferController.cs](C:/Users/jag/00_Tandem2026/Desing/Controllers/ClientOfferController.cs)
- [ClientOfferRepository.cs](C:/Users/jag/00_Tandem2026/Desing/Helpers/ClientOfferRepository.cs)
- [ClientOfferPdfWriter.cs](C:/Users/jag/00_Tandem2026/Desing/Helpers/ClientOfferPdfWriter.cs)

UI oferta:

- [Edit.cshtml](C:/Users/jag/00_Tandem2026/Desing/Views/ClientOffer/Edit.cshtml)
- [Print.cshtml](C:/Users/jag/00_Tandem2026/Desing/Views/ClientOffer/Print.cshtml)
- [Texts.cshtml](C:/Users/jag/00_Tandem2026/Desing/Views/ClientOffer/Texts.cshtml)

Integración visor captura:

- [Desing_2Controller.cs](C:/Users/jag/00_Tandem2026/Desing/Controllers/Desing_2Controller.cs)
- [Desing2ViewModels.cs](C:/Users/jag/00_Tandem2026/Desing/Models/Desing2ViewModels.cs)
- [_Desing2StlViewerWorkspace.cshtml](C:/Users/jag/00_Tandem2026/Desing/Views/Desing_2/_Desing2StlViewerWorkspace.cshtml)
- [master-article-details-stl-viewer.js](C:/Users/jag/00_Tandem2026/Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js)

Contactos cliente (estados/default):

- [ClientV2Controller.cs](C:/Users/jag/00_Tandem2026/Desing/Controllers/ClientV2Controller.cs)
- [Details.cshtml](C:/Users/jag/00_Tandem2026/Desing/Views/ClientV2/Details.cshtml)

## Endpoints clave

Oferta:

- `ClientOffer/OpenFromDesign`
- `ClientOffer/GeneratePdf`
- `ClientOffer/SaveCaptureFromViewer`
- `ClientOffer/DeleteCapture`
- `ClientOffer/ListContactStates` / `CreateContactState` / `DeleteContactState`

Visor:

- `Desing_2/Viewer` (parámetros `clientOfferId`, `captureMode=1` para captura de oferta)
- `Desing_2/SaveDesignPhoto` (foto de diseño general)

## i18n (recursos)

- [ClientOffer.resx](C:/Users/jag/00_Tandem2026/Desing/Resources/ClientOffer.resx)
- [ClientOffer.en.resx](C:/Users/jag/00_Tandem2026/Desing/Resources/ClientOffer.en.resx)
- [ClientV2.resx](C:/Users/jag/00_Tandem2026/Desing/Resources/ClientV2.resx)
- [ClientV2.en.resx](C:/Users/jag/00_Tandem2026/Desing/Resources/ClientV2.en.resx)

## Checklist de validación rápida

1. Crear/editar estado de contacto en configuración de oferta.
2. En Cliente, alta contacto + cambio de estado + default.
3. En oferta, comprobar selección de contacto válida.
4. Adjuntar varias capturas desde visor.
5. Ver bloque de capturas en print/PDF entre materiales y cierre.
6. Compilar [Design.csproj](C:/Users/jag/00_Tandem2026/Desing/Design.csproj) sin errores.

## Notas operativas

- Mantener scripts SQL idempotentes y aplicarlos en develop + producción.
- No copiar datos de negocio entre bases; solo esquema/catálogos según reglas del área BD.
- Si se amplía PDF real (render avanzado), conservar el orden actual de secciones.
