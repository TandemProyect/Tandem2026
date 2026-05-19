const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del modulo Jobside (TSql_Jobside / "Obras").
 *
 * Convencion de claves (alineada con Company / ClientV2 / MasterArticles):
 *   Common_*, Index_*, Page_*       -> breadcrumbs / titulos
 *   Col_*                           -> cabeceras DataTables
 *   Lbl_*, Ph_*, Help_*             -> formularios
 *   Dt_*, State_*                   -> menu tabla + badges
 *   Val_*                           -> ModelState (validacion servidor)
 *   ToastTitle_*, ToastMessage_*    -> TempData (Index)
 *   Err_*, Msg_*                    -> JSON / AJAX
 *   Js_*                            -> confirmaciones JS
 *   List_*                          -> tooltips de fila (servidor)
 *   Details_*                       -> ficha
 *   Btn_*                           -> botones especificos del modulo
 *
 * Las claves verdaderamente genericas (Btn_Save, Btn_Cancel, Btn_Edit,
 * Btn_Delete, Aria_*, Google_*, Dt_Btn_*, etc.) viven en Common.* y se
 * usan via Common.Key desde las vistas/controlador.
 */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Intranet: "Intranet",
    Index_Breadcrumb: "Obras",
    Index_CreateJobside: "Nueva obra",

    Col_AddNJobside: "Código obra",
    Col_AddNJobsideClient: "Código cliente",
    Col_Name: "Nombre",
    Col_Client: "Cliente",
    Col_LocAddress: "Dirección local",
    Col_State: "Estado",

    Dt_Rows_All: "Todas",
    Dt_Rows_N: "filas",
    Dt_MenuAria: "Opciones del listado",
    Dt_Section_Records: "Registros",
    Dt_Section_Export: "Exportar",
    Dt_Section_ColumnsVisible: "Columnas visibles",

    State_Active: "Activo",
    State_Inactive: "Inactivo",

    Js_ConfirmDeleteJobside:
      "¿Eliminar esta obra? (borrado lógico).",

    ToastTitle_CreateJobside: "Crear obra",
    ToastMessage_JobsideCreated: "Obra \"{0}\" creada correctamente.",
    ToastTitle_EditJobside: "Editar obra",
    ToastMessage_JobsideUpdated: "Obra \"{0}\" actualizada correctamente.",
    ToastTitle_DeleteJobside: "Eliminar obra",
    ToastMessage_JobsideDeleted: "Obra \"{0}\" eliminada correctamente.",

    Val_NameRequired: "El nombre de la obra es obligatorio.",
    Val_DuplicateNameCreate:
      "Ya existe una obra con ese nombre para el cliente seleccionado.",
    Val_DuplicateNameEdit:
      "Ya existe otra obra con ese nombre para el cliente seleccionado.",
    Val_ClientInvalid: "El cliente seleccionado no es válido.",
    Val_BranchRequired: "Seleccione una delegación / sucursal.",
    Val_BranchInvalid: "La delegación seleccionada no es válida.",

    List_LinkOpenTooltip: "Ver obra",
    List_LinkEditTooltip: "Editar obra",
    List_LinkDeleteTooltip: "Eliminar obra",
    List_LinkDeleteLockedDocumentsTooltip:
      "No se puede eliminar: la obra tiene documentos asociados.",
    List_LinkDeleteLockedOffersTooltip:
      "No se puede eliminar: la obra tiene ofertas asociadas.",

    Err_JobsideNotFound: "Obra no encontrada.",
    Err_CannotDeleteHasDocuments:
      "No se puede eliminar: la obra tiene documentos asociados.",
    Err_CannotDeleteHasOffers:
      "No se puede eliminar: la obra tiene ofertas asociadas.",

    Page_CreateTitle: "Crear obra",
    Page_EditTitle: "Editar obra",
    Page_DetailsTitle: "Detalles de la obra",

    Lbl_NameRequired: "Nombre obra *",
    Lbl_ContractRef: "Ref. contrato / expediente",
    Help_ContractRefOptional: "(opcional)",
    Lbl_JobsideNotes: "Notas de obra",
    Lbl_AddNJobside: "Código interno de obra",
    Ph_ContractRef: "Ej.: EXP-2026-001",
    Ph_JobsideNotes: "Observaciones (opcional)",
    Help_AddNJobsidePending: "Se generará automáticamente al guardar.",
    Lbl_Client: "Cliente",
    Lbl_ClientEmpty: "-- Sin cliente --",
    Lbl_Branch: "Delegación / sucursal",
    Lbl_BranchEmpty: "-- Seleccione delegación --",
    Lbl_Active: "Activo",
    Lbl_ActiveCheckbox: "Obra activa",
    Lbl_BillSameAsLoc: "Facturación = misma dirección que local",
    Lbl_LocAddress: "Dirección local",
    Lbl_BillAddress: "Dirección facturación",
    Lbl_BillSameAsLocShort: "(igual que local)",

    Ph_Name: "Ej.: Obra Calle Mayor 23",

    Google_NotConfiguredStrong: "Google Maps no configurado.",
    Google_NotConfiguredBody:
      " Añada GoogleMaps:ApiKey en Web.config para autocompletado y mapa. Puede rellenar la dirección manualmente.",

    Btn_New: "Nueva obra",

    Details_Field_Name: "Nombre",
    Details_Field_Client: "Cliente",
    Details_Field_Branch: "Delegación / sucursal",
    Details_Field_ContractRef: "Ref. contrato / expediente",
    Details_Field_JobsideNotes: "Notas",
    Details_Field_AddNJobside: "Código obra",
    Details_Field_Active: "Activo",
    Details_Field_LocAddress: "Dirección local",
    Details_Field_BillAddress: "Dirección facturación",
    Details_NoValue: "—",
    Details_YesValue: "Sí",
    Details_NoBoolValue: "No",

    Workspace_LeftTitle: "Datos de la obra",
    Tab_Offers: "Ofertas",
    Tab_Documents: "Documentos",
    Tab_Chat: "Chat",
    Tab_ComingSoon: "Contenido próximamente.",
    Details_Section_LocationDetail: "Detalle dirección local",
    Details_Section_BillingDetail: "Detalle dirección facturación",
    Details_Field_BillSameAsLocLabel: "Facturación igual que local",

    Docs_Col_Name: "Nombre",
    Docs_Col_Type: "Tipo",
    Docs_Col_Extension: "Extensión",
    Docs_Col_Date: "Alta",
    Docs_Col_Actions: "Acciones",
    Docs_DownloadTooltip: "Descargar archivo",
    Docs_NoFile: "Sin archivo",
    Docs_NoDocuments: "No hay documentos para esta obra.",
    Docs_DtTitle: "Documentos de la obra",
    Docs_Btn_AttachDocument: "Adjuntar documento",
    Docs_UploadModalTitle: "Adjuntar documento a la obra",
    Docs_Upload_Type: "Tipo de documento",
    Docs_Upload_File: "Archivo",
    Docs_Upload_Description: "Descripción (opcional)",
    Docs_Upload_Submit: "Subir",
    Docs_Upload_Success: "Documento guardado correctamente.",
    Docs_Upload_Failed: "No se pudo subir el documento.",
    Docs_Val_NoFile: "Seleccione un archivo.",
    Docs_Val_DocTypeInvalid: "El tipo de documento no es válido.",
    Docs_Val_ExtensionMissing: "El archivo debe tener extensión.",
    Docs_Val_ExtensionNotAllowed:
      "La extensión del archivo no está permitida para el tipo seleccionado.",
    Docs_Val_SaveFailed: "Error al guardar el archivo.",
    Docs_Val_FileTooLarge: "El archivo supera el tamaño máximo permitido ({0}).",
    Docs_Upload_NoDocTypes:
      "No hay tipos de documento activos. Cree uno en Configuración antes de adjuntar archivos.",

    Workspace_MapTitle: "Mapa (dirección local)",
    Workspace_Tooltip_ExpandPanel: "Expandir panel",
    Workspace_Tooltip_RestorePanel: "Restaurar",

    List_LinkWorkspaceTooltip: "Espacio de trabajo de la obra"
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Intranet: "Intranet",
  Index_Breadcrumb: "Jobsites",
  Index_CreateJobside: "New jobsite",

  Col_AddNJobside: "Jobsite code",
  Col_AddNJobsideClient: "Client code",
  Col_Name: "Name",
  Col_Client: "Client",
  Col_LocAddress: "Site address",
  Col_State: "State",

  Dt_Rows_All: "All",
  Dt_Rows_N: "rows",
  Dt_MenuAria: "List options",
  Dt_Section_Records: "Records",
  Dt_Section_Export: "Export",
  Dt_Section_ColumnsVisible: "Visible columns",

  State_Active: "Active",
  State_Inactive: "Inactive",

  Js_ConfirmDeleteJobside: "Delete this jobsite? (soft delete).",

  ToastTitle_CreateJobside: "Create jobsite",
  ToastMessage_JobsideCreated: "Jobsite \"{0}\" created successfully.",
  ToastTitle_EditJobside: "Edit jobsite",
  ToastMessage_JobsideUpdated: "Jobsite \"{0}\" updated successfully.",
  ToastTitle_DeleteJobside: "Delete jobsite",
  ToastMessage_JobsideDeleted: "Jobsite \"{0}\" deleted successfully.",

  Val_NameRequired: "Jobsite name is required.",
  Val_DuplicateNameCreate:
    "A jobsite with this name already exists for the selected client.",
  Val_DuplicateNameEdit:
    "Another jobsite already uses this name for the selected client.",
  Val_ClientInvalid: "The selected client is not valid.",
  Val_BranchRequired: "Select a branch.",
  Val_BranchInvalid: "The selected branch is not valid.",

  List_LinkOpenTooltip: "Open jobsite",
  List_LinkEditTooltip: "Edit jobsite",
  List_LinkDeleteTooltip: "Delete jobsite",
  List_LinkDeleteLockedDocumentsTooltip:
    "Cannot delete: the jobsite has linked documents.",
  List_LinkDeleteLockedOffersTooltip:
    "Cannot delete: the jobsite has linked offers.",

  Err_JobsideNotFound: "Jobsite not found.",
  Err_CannotDeleteHasDocuments:
    "Cannot delete: the jobsite has linked documents.",
  Err_CannotDeleteHasOffers:
    "Cannot delete: the jobsite has linked offers.",

  Page_CreateTitle: "Create jobsite",
  Page_EditTitle: "Edit jobsite",
  Page_DetailsTitle: "Jobsite details",

  Lbl_NameRequired: "Jobsite name *",
  Lbl_ContractRef: "Contract / file reference",
  Help_ContractRefOptional: "(optional)",
  Lbl_JobsideNotes: "Jobsite notes",
  Lbl_AddNJobside: "Internal jobsite code",
  Ph_ContractRef: "e.g. EXP-2026-001",
  Ph_JobsideNotes: "Notes (optional)",
  Help_AddNJobsidePending: "Assigned automatically when you save.",
  Lbl_Client: "Client",
  Lbl_ClientEmpty: "-- No client --",
  Lbl_Branch: "Branch",
  Lbl_BranchEmpty: "-- Select branch --",
  Lbl_Active: "Active",
  Lbl_ActiveCheckbox: "Active jobsite",
  Lbl_BillSameAsLoc: "Billing = same address as site",
  Lbl_LocAddress: "Site address",
  Lbl_BillAddress: "Billing address",
  Lbl_BillSameAsLocShort: "(same as site)",

  Ph_Name: "E.g.: 23 Main Street site",

  Google_NotConfiguredStrong: "Google Maps is not configured.",
  Google_NotConfiguredBody:
    " Add GoogleMaps:ApiKey in Web.config to enable autocomplete and the map. You can also fill the address manually.",

  Btn_New: "New jobsite",

  Details_Field_Name: "Name",
  Details_Field_Client: "Client",
  Details_Field_Branch: "Branch",
  Details_Field_ContractRef: "Contract / file reference",
  Details_Field_JobsideNotes: "Notes",
  Details_Field_AddNJobside: "Jobsite code",
  Details_Field_Active: "Active",
  Details_Field_LocAddress: "Site address",
  Details_Field_BillAddress: "Billing address",
  Details_NoValue: "—",
  Details_YesValue: "Yes",
  Details_NoBoolValue: "No",

  Workspace_LeftTitle: "Jobsite data",
  Tab_Offers: "Offers",
  Tab_Documents: "Documents",
  Tab_Chat: "Chat",
  Tab_ComingSoon: "Coming soon.",
  Details_Section_LocationDetail: "Site address detail",
  Details_Section_BillingDetail: "Billing address detail",
  Details_Field_BillSameAsLocLabel: "Billing same as site",

  Docs_Col_Name: "Name",
  Docs_Col_Type: "Type",
  Docs_Col_Extension: "Extension",
  Docs_Col_Date: "Uploaded",
  Docs_Col_Actions: "Actions",
  Docs_DownloadTooltip: "Download file",
  Docs_NoFile: "No file",
  Docs_NoDocuments: "No documents for this jobsite.",
  Docs_DtTitle: "Jobsite documents",
  Docs_Btn_AttachDocument: "Attach document",
  Docs_UploadModalTitle: "Attach document to jobsite",
  Docs_Upload_Type: "Document type",
  Docs_Upload_File: "File",
  Docs_Upload_Description: "Description (optional)",
  Docs_Upload_Submit: "Upload",
  Docs_Upload_Success: "Document saved successfully.",
  Docs_Upload_Failed: "Could not upload the document.",
  Docs_Val_NoFile: "Select a file.",
  Docs_Val_DocTypeInvalid: "The document type is not valid.",
  Docs_Val_ExtensionMissing: "The file must have an extension.",
  Docs_Val_ExtensionNotAllowed:
    "The file extension is not allowed for the selected type.",
  Docs_Val_SaveFailed: "Error while saving the file.",
  Docs_Val_FileTooLarge: "The file exceeds the maximum allowed size ({0}).",
  Docs_Upload_NoDocTypes:
    "There are no active document types. Create one under Settings before attaching files.",

  Workspace_MapTitle: "Map (site address)",
  Workspace_Tooltip_ExpandPanel: "Expand panel",
  Workspace_Tooltip_RestorePanel: "Restore",

  List_LinkWorkspaceTooltip: "Jobsite workspace"
});

function xmlEsc(s) {
  return String(s)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

function buildResx(table) {
  const header =
    `<?xml version="1.0" encoding="utf-8"?>\n` +
    `<root>\n` +
    `  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>\n` +
    `  <resheader name="version"><value>2.0</value></resheader>\n` +
    `  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>\n` +
    `  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>\n`;
  const rows = Object.keys(es)
    .sort()
    .map(
      (k) =>
        `  <data name="${k}" xml:space="preserve">\n    <value>${xmlEsc(
          table[k]
        )}</value>\n  </data>\n`
    )
    .join("");
  return header + rows + `</root>\n`;
}

const keys = Object.keys(es).sort();
const propsBody = keys
  .map(
    (k) =>
      `        public static string ${k} => ResourceManager.GetString(nameof(${k}), resourceCulture);\n`
  )
  .join("");

const designer = `//------------------------------------------------------------------------------
// Auto-generated by _gen_jobside_resources.js — Jobside UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Jobside module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Jobside
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Jobside", typeof(Jobside).Assembly, "Jobside"));

        public static CultureInfo Culture
        {
            get => resourceCulture;
            set => resourceCulture = value;
        }
${propsBody}    }
}
`;

const dir = path.join(__dirname, "Resources");
fs.mkdirSync(dir, { recursive: true });
fs.writeFileSync(path.join(dir, "Jobside.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Jobside.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Jobside.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Jobside.*");
