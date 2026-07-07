const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del modulo Extension (TSql_Extension).
 *
 * Sigue el patron de DocumentType / Jobside / ClientV2 / Plantilla / Branch /
 * Common:
 *   - Clase estatica Desing.Resources.Extension (DbBackedResourceManager).
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
 *   Ico_*                           -> subseccion icono / archivo subido
 */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Settings: "Configuración",
    Index_Breadcrumb: "Extensiones",
    Index_CreateExtension: "Nueva extensión",

    Col_Icon: "Icono",
    Col_Name: "Nombre",
    Col_MaxFileSize: "Tamaño máx.",
    Col_DocumentTypes: "Tipos de documento",
    Col_State: "Estado",

    State_Active: "Activa",
    State_Inactive: "Inactiva",

    Js_ConfirmDeleteExtension:
      "¿Eliminar esta extensión? (borrado lógico).",

    ToastTitle_CreateExtension: "Crear extensión",
    ToastMessage_ExtensionCreated:
      "Extensión \"{0}\" creada correctamente.",
    ToastTitle_EditExtension: "Editar extensión",
    ToastMessage_ExtensionUpdated:
      "Extensión \"{0}\" actualizada correctamente.",
    ToastTitle_DeleteExtension: "Eliminar extensión",
    ToastMessage_ExtensionDeleted:
      "Extensión \"{0}\" eliminada correctamente.",

    Val_NameRequired: "El nombre de la extensión es obligatorio.",
    Val_NameTooLong: "El nombre no puede superar los 500 caracteres.",
    Val_DuplicateNameCreate: "Ya existe una extensión con ese nombre.",
    Val_DuplicateNameEdit: "Ya existe otra extensión con ese nombre.",
    Val_MaxFileSizeMin: "El tamaño máximo debe ser mayor que 0.",
    Val_MaxFileSizeMax:
      "El tamaño máximo no puede superar {0} bytes (2 GB).",
    Val_IcoPathTooLong:
      "La ruta virtual del icono no puede superar los 500 caracteres.",
    Val_IcoFileMissingExtension: "El archivo debe tener extensión.",
    Val_IcoFormatNotAllowed:
      "Formato no permitido (png, jpg, gif, ico, webp).",

    List_LinkOpenTooltip: "Ver extensión",
    List_LinkEditTooltip: "Editar extensión",
    List_LinkDeleteTooltip: "Eliminar extensión",
    List_LinkDeleteLockedDocumentTypesTooltip:
      "No se puede eliminar: la extensión está enlazada a tipos de documento activos.",
    List_LinkDeleteLockedDocumentsTooltip:
      "No se puede eliminar: la extensión está en uso por documentos.",
    List_NoIcon: "—",
    List_NoDocumentTypes: "—",

    Err_ExtensionNotFound: "Extensión no encontrada.",
    Err_CannotDeleteHasDocumentTypes:
      "No se puede eliminar: la extensión está enlazada a tipos de documento activos.",
    Err_CannotDeleteHasDocuments:
      "No se puede eliminar: la extensión está en uso por documentos.",

    Page_CreateTitle: "Crear extensión",
    Page_EditTitle: "Editar extensión",
    Page_DetailsTitle: "Detalle de la extensión",

    Lbl_NameRequired: "Nombre / Etiqueta *",
    Lbl_MaxFileSizeRequired: "Tamaño máximo (bytes) *",
    Lbl_Active: "Activo",
    Lbl_ActiveCheckbox: "Extensión activa",
    Lbl_Icon: "Icono (archivo opcional)",
    Lbl_DocumentTypesUsage: "Tipos de documento que la usan",

    Ph_Name: "Ej.: PDF, DWG, .pdf",
    Ph_MaxFileSize: "10485760 = 10 MB",
    Help_MaxFileSizeBytes: "(bytes)",

    Btn_New: "Nueva extensión",

    Details_Field_Name: "Nombre",
    Details_Field_MaxFileSize: "Tamaño máximo",
    Details_Field_Active: "Activo",
    Details_Field_Icon: "Icono",
    Details_Field_DocumentTypes: "Tipos de documento que la usan",
    Details_NoDocumentTypesAssigned: "— Sin tipos de documento asociados.",
    Details_NoValue: "—",
    Details_YesValue: "Sí",
    Details_NoBoolValue: "No"
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Settings: "Settings",
  Index_Breadcrumb: "Extensions",
  Index_CreateExtension: "New extension",

  Col_Icon: "Icon",
  Col_Name: "Name",
  Col_MaxFileSize: "Max size",
  Col_DocumentTypes: "Document types",
  Col_State: "State",

  State_Active: "Active",
  State_Inactive: "Inactive",

  Js_ConfirmDeleteExtension: "Delete this extension? (soft delete).",

  ToastTitle_CreateExtension: "Create extension",
  ToastMessage_ExtensionCreated:
    "Extension \"{0}\" created successfully.",
  ToastTitle_EditExtension: "Edit extension",
  ToastMessage_ExtensionUpdated:
    "Extension \"{0}\" updated successfully.",
  ToastTitle_DeleteExtension: "Delete extension",
  ToastMessage_ExtensionDeleted:
    "Extension \"{0}\" deleted successfully.",

  Val_NameRequired: "Extension name is required.",
  Val_NameTooLong: "Name cannot exceed 500 characters.",
  Val_DuplicateNameCreate: "An extension with this name already exists.",
  Val_DuplicateNameEdit: "Another extension already uses this name.",
  Val_MaxFileSizeMin: "Maximum size must be greater than 0.",
  Val_MaxFileSizeMax:
    "Maximum size cannot exceed {0} bytes (2 GB).",
  Val_IcoPathTooLong:
    "The icon virtual path cannot exceed 500 characters.",
  Val_IcoFileMissingExtension: "The file must have an extension.",
  Val_IcoFormatNotAllowed:
    "Format not allowed (png, jpg, gif, ico, webp).",

  List_LinkOpenTooltip: "Open extension",
  List_LinkEditTooltip: "Edit extension",
  List_LinkDeleteTooltip: "Delete extension",
  List_LinkDeleteLockedDocumentTypesTooltip:
    "Cannot delete: the extension is linked to active document types.",
  List_LinkDeleteLockedDocumentsTooltip:
    "Cannot delete: the extension is in use by documents.",
  List_NoIcon: "—",
  List_NoDocumentTypes: "—",

  Err_ExtensionNotFound: "Extension not found.",
  Err_CannotDeleteHasDocumentTypes:
    "Cannot delete: the extension is linked to active document types.",
  Err_CannotDeleteHasDocuments:
    "Cannot delete: the extension is in use by documents.",

  Page_CreateTitle: "Create extension",
  Page_EditTitle: "Edit extension",
  Page_DetailsTitle: "Extension details",

  Lbl_NameRequired: "Name / Label *",
  Lbl_MaxFileSizeRequired: "Maximum size (bytes) *",
  Lbl_Active: "Active",
  Lbl_ActiveCheckbox: "Active extension",
  Lbl_Icon: "Icon (optional file)",
  Lbl_DocumentTypesUsage: "Document types that use it",

  Ph_Name: "E.g.: PDF, DWG, .pdf",
  Ph_MaxFileSize: "10485760 = 10 MB",
  Help_MaxFileSizeBytes: "(bytes)",

  Btn_New: "New extension",

  Details_Field_Name: "Name",
  Details_Field_MaxFileSize: "Maximum size",
  Details_Field_Active: "Active",
  Details_Field_Icon: "Icon",
  Details_Field_DocumentTypes: "Document types that use it",
  Details_NoDocumentTypesAssigned: "— No linked document types.",
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
// Auto-generated by _gen_extension_resources.js — Extension UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Extension module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Extension
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Extension", typeof(Extension).Assembly, "Extension"));

        public static CultureInfo Culture
        {
            get => resourceCulture;
            set => resourceCulture = value;
        }
${propsBody}    }
}
`;

const dir = path.join(__dirname, "..", "..", "Resources");
fs.mkdirSync(dir, { recursive: true });
fs.writeFileSync(path.join(dir, "Extension.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Extension.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Extension.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Extension.*");
