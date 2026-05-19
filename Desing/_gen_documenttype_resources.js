const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del modulo DocumentType (TSql_DocumentType).
 *
 * Sigue el patron de Jobside / ClientV2 / Plantilla / Branch / Common:
 *   - Clase estatica Desing.Resources.DocumentType (DbBackedResourceManager).
 *   - Diccionarios `es` y `en` con todas las claves user-facing del modulo.
 *   - Las claves verdaderamente genericas (Btn_Save, Btn_Cancel, Btn_Edit,
 *     Btn_SaveChanges, Btn_Delete, Dt_* de menu DataTables, etc.) viven en
 *     Common.* y se reutilizan desde las vistas; aqui NO se duplican.
 *
 * Convencion de prefijos:
 *   Common_*, Index_*, Page_*       -> breadcrumbs / titulos
 *   Col_*                           -> cabeceras DataTables
 *   Lbl_*, Ph_*, Help_*             -> formularios
 *   State_*                         -> badges
 *   Val_*                           -> ModelState (validacion servidor)
 *   ToastTitle_*, ToastMessage_*    -> TempData (Index / CRUD)
 *   Err_*, Msg_*                    -> JSON / AJAX
 *   Js_*                            -> confirmaciones JS
 *   List_*                          -> tooltips de fila (servidor)
 *   Details_*                       -> ficha
 *   Btn_*                           -> botones especificos del modulo
 *   Ext_*                           -> subseccion "extensiones permitidas"
 */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Settings: "Configuración",
    Index_Breadcrumb: "Tipos de documento",
    Index_CreateDocumentType: "Nuevo tipo",

    Col_Name: "Nombre",
    Col_Code: "Código",
    Col_Description: "Descripción",
    Col_Extensions: "Extensiones",
    Col_State: "Estado",

    State_Active: "Activo",
    State_Inactive: "Inactivo",

    Js_ConfirmDeleteDocumentType:
      "¿Eliminar este tipo de documento? (borrado lógico).",

    ToastTitle_CreateDocumentType: "Crear tipo de documento",
    ToastMessage_DocumentTypeCreated:
      "Tipo de documento \"{0}\" creado correctamente.",
    ToastTitle_EditDocumentType: "Editar tipo de documento",
    ToastMessage_DocumentTypeUpdated:
      "Tipo de documento \"{0}\" actualizado correctamente.",
    ToastTitle_DeleteDocumentType: "Eliminar tipo de documento",
    ToastMessage_DocumentTypeDeleted:
      "Tipo de documento \"{0}\" eliminado correctamente.",

    Val_NameRequired: "El nombre del tipo de documento es obligatorio.",
    Val_DuplicateCodeCreate:
      "Ya existe un tipo de documento con ese código.",
    Val_DuplicateCodeEdit:
      "Ya existe otro tipo de documento con ese código.",

    List_LinkOpenTooltip: "Ver tipo de documento",
    List_LinkEditTooltip: "Editar tipo de documento",
    List_LinkDeleteTooltip: "Eliminar tipo de documento",
    List_LinkDeleteLockedDocumentsTooltip:
      "No se puede eliminar: el tipo de documento tiene documentos asociados.",
    List_NoExtensions: "—",

    Err_DocumentTypeNotFound: "Tipo de documento no encontrado.",
    Err_CannotDeleteHasDocuments:
      "No se puede eliminar: el tipo de documento tiene documentos asociados.",

    Page_CreateTitle: "Crear tipo de documento",
    Page_EditTitle: "Editar tipo de documento",
    Page_DetailsTitle: "Detalle del tipo de documento",

    Lbl_NameRequired: "Nombre *",
    Lbl_Code: "Código",
    Lbl_Description: "Descripción",
    Lbl_Active: "Activo",
    Lbl_ActiveCheckbox: "Tipo activo",

    Lbl_Extensions: "Extensiones permitidas",
    Ph_Name: "Ej.: Plano, Factura, Contrato",
    Ph_Code: "Ej.: PLA, FAC",
    Ph_Description: "Descripción opcional del tipo de documento.",
    Ph_ExtensionFilter: "Buscar extensión por nombre (.pdf, .dwg, ...)",

    Ext_EmptyCatalog:
      "No hay extensiones de fichero registradas todavía. Crea al menos una en el catálogo de extensiones para poder asociarlas.",

    Btn_New: "Nuevo tipo",

    Details_Field_Name: "Nombre",
    Details_Field_Code: "Código",
    Details_Field_Description: "Descripción",
    Details_Field_Active: "Activo",
    Details_Field_Extensions: "Extensiones permitidas",
    Details_NoExtensionsAssigned: "— Sin extensiones asociadas.",
    Details_NoValue: "—",
    Details_YesValue: "Sí",
    Details_NoBoolValue: "No"
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Settings: "Settings",
  Index_Breadcrumb: "Document types",
  Index_CreateDocumentType: "New document type",

  Col_Name: "Name",
  Col_Code: "Code",
  Col_Description: "Description",
  Col_Extensions: "Extensions",
  Col_State: "State",

  State_Active: "Active",
  State_Inactive: "Inactive",

  Js_ConfirmDeleteDocumentType: "Delete this document type? (soft delete).",

  ToastTitle_CreateDocumentType: "Create document type",
  ToastMessage_DocumentTypeCreated:
    "Document type \"{0}\" created successfully.",
  ToastTitle_EditDocumentType: "Edit document type",
  ToastMessage_DocumentTypeUpdated:
    "Document type \"{0}\" updated successfully.",
  ToastTitle_DeleteDocumentType: "Delete document type",
  ToastMessage_DocumentTypeDeleted:
    "Document type \"{0}\" deleted successfully.",

  Val_NameRequired: "Document type name is required.",
  Val_DuplicateCodeCreate:
    "A document type with this code already exists.",
  Val_DuplicateCodeEdit:
    "Another document type already uses this code.",

  List_LinkOpenTooltip: "Open document type",
  List_LinkEditTooltip: "Edit document type",
  List_LinkDeleteTooltip: "Delete document type",
  List_LinkDeleteLockedDocumentsTooltip:
    "Cannot delete: the document type has linked documents.",
  List_NoExtensions: "—",

  Err_DocumentTypeNotFound: "Document type not found.",
  Err_CannotDeleteHasDocuments:
    "Cannot delete: the document type has linked documents.",

  Page_CreateTitle: "Create document type",
  Page_EditTitle: "Edit document type",
  Page_DetailsTitle: "Document type details",

  Lbl_NameRequired: "Name *",
  Lbl_Code: "Code",
  Lbl_Description: "Description",
  Lbl_Active: "Active",
  Lbl_ActiveCheckbox: "Active type",

  Lbl_Extensions: "Allowed extensions",
  Ph_Name: "E.g.: Drawing, Invoice, Contract",
  Ph_Code: "E.g.: DRW, INV",
  Ph_Description: "Optional description for the document type.",
  Ph_ExtensionFilter: "Search extension by name (.pdf, .dwg, ...)",

  Ext_EmptyCatalog:
    "There are no file extensions registered yet. Create at least one in the extensions catalog before linking them.",

  Btn_New: "New type",

  Details_Field_Name: "Name",
  Details_Field_Code: "Code",
  Details_Field_Description: "Description",
  Details_Field_Active: "Active",
  Details_Field_Extensions: "Allowed extensions",
  Details_NoExtensionsAssigned: "— No extensions linked.",
  Details_NoValue: "—",
  Details_YesValue: "Yes",
  Details_NoBoolValue: "No"
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
// Auto-generated by _gen_documenttype_resources.js — DocumentType UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_DocumentType module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class DocumentType
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.DocumentType", typeof(DocumentType).Assembly, "DocumentType"));

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
fs.writeFileSync(path.join(dir, "DocumentType.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "DocumentType.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "DocumentType.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/DocumentType.*");
