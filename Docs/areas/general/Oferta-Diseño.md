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

0. En **Configuración > Plantilla oferta**, fase actual del constructor visual:
   - izquierda: página A4 en blanco;
   - se puede arrastrar y redimensionar el logo libremente; posición+tamaño se guardan en `TextLayoutJson`; al colocarlo en A4 desaparece del panel derecho;
   - sección **Direcciones** opcional con 2 bloques fijos (arrastrables): **Datos del cliente** y **Datos de la empresa**; al arrastrar uno al A4 desaparece del panel derecho.
   - sección **Texto inicial y final** con botón **Adjuntar texto** (popup editor rico: negrita, cursiva, subrayado, alineación y listas), bloques arrastrables al A4 y opción **Guardar como** para reutilizar en otras plantillas de la misma empresa.
   - sección **Complementos** con **Recuento del diseño** (placeholder de ejemplo), arrastrable/redimensionable en A4, persistido en `TextLayoutJson` y oculto en el panel derecho cuando ya está colocado.
1. Desde visor Desing_2 se abre oferta (`OpenFromDesign`):
   - al pulsar, se muestra estado de carga **solo mientras el iframe no está listo**;
   - al cargar el selector de plantillas el overlay se oculta y **no vuelve a los 2,5 s**;
   - el overlay reaparece al pulsar **Cargar plantilla** (POST) hasta que salga el resultado;
   - el popup de oferta es movible (arrastrable por la cabecera) sobre el visor.
   - no cambia el modo activo del visor (si estás en encofrado, permanece en encofrado).
2. En popup de oferta, **solo se elige una plantilla de la empresa del usuario**.
   - sistema, empresa, diseño, oferta, obra y cliente se infieren del contexto;
   - al elegir, se crea el borrador y se abre la edición con el resultado.
3. El popup de edición **no usa el layout de intranet** (sin menú, navbar ni «Datos de la oferta»):
   - izquierda: **Resultado** (vista previa);
   - derecha (estrecho): **Adjuntar planos** e **IA** (historial, input, micrófono y altavoz).
4. Al adjuntar plano se abre visor en modo captura (sin UI en imagen) y **Foto del diseño** lo inserta en la oferta (N veces).
5. En edición/print/PDF aparecen las capturas en bloque de planos.

## Estructura BD (SQL Server)

Tablas de oferta:

- `TSql_ClientOffer`
- `TSql_ClientOfferBrand`
- `TSql_ClientOfferBrandLogo` (N logos por plantilla)
- `TSql_ClientOfferBrandText` (N textos por plantilla)
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
- [2026-11-19_alter_TSql_ClientOfferBrand_template_builder.sql](C:/Users/jag/00_Tandem2026/Desing/Scripts/TemporalScript/2026-11-19_alter_TSql_ClientOfferBrand_template_builder.sql)
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
