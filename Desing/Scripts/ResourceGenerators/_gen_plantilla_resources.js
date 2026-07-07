const fs = require("fs");
const path = require("path");

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Index_Breadcrumb: "Plantillas",
    Index_CreatePlantilla: "Crear nueva plantilla",

    Col_Name: "Nombre",
    Col_Color: "Color",
    Col_Logo: "Logo",
    Col_BrandText: "Marca",
    Col_Default: "Por defecto",
    Col_CreatedDate: "Creada",

    Dt_Rows_All: "Todas",
    Dt_Rows_N: "filas",
    Dt_MenuAria: "Opciones del listado",
    Dt_Section_Records: "Registros",
    Dt_Section_Export: "Exportar",
    Dt_Section_ColumnsVisible: "Columnas visibles",

    State_Default: "Por defecto",
    State_NotDefault: "—",

    Js_ConfirmDeletePlantilla:
      "¿Eliminar esta plantilla? (borrado lógico). Las empresas que la usen pasarán a la plantilla por defecto.",

    ToastTitle_CreatePlantilla: "Crear plantilla",
    ToastMessage_PlantillaCreated: "Plantilla \"{0}\" creada correctamente.",
    ToastTitle_EditPlantilla: "Editar plantilla",
    ToastMessage_PlantillaUpdated: "Plantilla \"{0}\" actualizada correctamente.",
    ToastTitle_DeletePlantilla: "Eliminar plantilla",
    ToastMessage_PlantillaDeleted: "Plantilla \"{0}\" eliminada correctamente.",
    ToastTitle_DbError: "Base de datos",
    ToastMessage_DbBrandColumnsMissing:
      "La tabla TSql_Plantilla no incluye las columnas de marca (AttBrandText, AttBrandTextColor, AttBrandAccentColor). Ejecuta en el servidor SQL el script: App_Data/Sql/TSql_Plantilla_add_brand_text_and_colors.sql (carpeta del proyecto Desing).",

    Val_NameRequired: "El nombre de la plantilla es obligatorio.",
    Val_BrandTextRequired: "El texto de marca es obligatorio.",
    Val_AccentColorRequired: "El color de acento es obligatorio.",
    Val_AccentColorHexFormat:
      "El color de acento debe estar en formato HEX, ej.: #f29100.",
    Val_BrandTextColorHexFormat:
      "Color de texto: vacío (hereda color primario) o HEX, ej.: #4c4c4c.",
    Val_MainColorRequired: "El color principal es obligatorio.",
    Val_MainColorHexFormat:
      "El color principal debe estar en formato HEX, ej.: #349d7d.",
    Val_LogoPathRequired: "La ruta del logo es obligatoria.",
    Val_DuplicateNameCreate: "Ya existe una plantilla con ese nombre.",
    Val_DuplicateNameEdit: "Ya existe otra plantilla con ese nombre.",

    List_LinkEditTooltip: "Editar plantilla",
    List_LinkDeleteTooltip: "Eliminar plantilla",
    List_LinkOpenTooltip: "Ver plantilla",
    List_DefaultLockedTooltip:
      "No se puede eliminar la plantilla marcada como por defecto.",

    Err_PlantillaNotFound: "Plantilla no encontrada.",
    Err_CannotDeleteDefault:
      "No se puede eliminar la plantilla marcada como por defecto.",
    Err_LogoFormatNotAllowed: "Formato de logo no permitido. Usa: {0}.",
    Err_LogoTooLarge: "El logo supera el tamaño máximo permitido (2 MB).",
    Err_LogoSaveFailed: "No se pudo guardar el logo: {0}",
    Err_FaviconFormatNotAllowed:
      "Formato de favicon no permitido. Usa: {0}.",
    Err_FaviconTooLarge: "El favicon supera el tamaño máximo permitido (512 KB).",
    Err_FaviconSaveFailed: "No se pudo guardar el favicon: {0}",
    Msg_PlantillaDeleted: "Plantilla eliminada correctamente.",

    Page_CreateTitle: "Crear plantilla",
    Page_EditTitle: "Editar plantilla",

    Lbl_NameRequired: "Nombre de la plantilla *",
    Lbl_BrandTextRequired: "Nombre mostrado (marca) *",
    Lbl_BrandAccentColor: "Color primera letra (acento) *",
    Lbl_BrandAccentColorTitle: "Color primera letra",
    Lbl_BrandTextColor: "Color texto marca (resto)",
    Lbl_BrandTextColorTitle: "Color resto del texto (opcional)",
    Lbl_MainColor: "Color principal *",
    Lbl_Logo: "Logo",
    Lbl_Favicon: "Favicon",
    Lbl_IsDefault: "Marcar como plantilla por defecto",

    Ph_Name: "Ej.: Plantilla Cliente Acme",
    Ph_BrandText: "Ej.: T Desing.net",
    Ph_BrandTextColor: "Vacío = color primario",
    Ph_LogoPath: "/Content/images/Login/at.png",
    Ph_FaviconPath: "/assets/client/images/Default/Ico/at.ico",

    Help_BrandText:
      "Se muestra en cabecera, login y pie. La primera letra usa el color de acento.",
    Help_BrandTextColor:
      "Vacío: el resto del nombre hereda el color primario del tema.",
    Help_LogoFormats: "PNG, JPG, GIF, SVG, WEBP o ICO; máx. 2 MB.",
    Help_FaviconSize: "Máx. 512 KB.",
    Help_OnlyOneDefault:
      "Solo puede haber una por defecto; al marcar esta se desmarcará la anterior.",

    Preview_Title: "Vista previa",
    Preview_ColorLabel: "Color:",

    Js_FileTooLargeLogo: "El archivo supera 2 MB.",
    Js_FileTooLargeFavicon: "El favicon supera 512 KB.",

    Btn_Cancel: "Cancelar",
    Btn_Exit: "Salir",
    Btn_SavePlantilla: "Crear plantilla",
    Btn_SaveChanges: "Guardar cambios",
    Btn_Edit: "Editar",
    Btn_Delete: "Eliminar",

    Details_Field_Name: "Nombre",
    Details_Field_BrandText: "Marca",
    Details_Field_MainColor: "Color principal",
    Details_Field_AccentColor: "Color acento",
    Details_Field_BrandTextColor: "Color texto marca",
    Details_Field_Logo: "Logo",
    Details_Field_Favicon: "Favicon",
    Details_Field_IsDefault: "Plantilla por defecto",
    Details_Field_CreatedDate: "Fecha de creación",
    Details_NoValue: "-",
    Details_YesValue: "Sí",
    Details_NoBoolValue: "No",
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Index_Breadcrumb: "Templates",
  Index_CreatePlantilla: "New template",

  Col_Name: "Name",
  Col_Color: "Color",
  Col_Logo: "Logo",
  Col_BrandText: "Brand",
  Col_Default: "Default",
  Col_CreatedDate: "Created",

  Dt_Rows_All: "All",
  Dt_Rows_N: "rows",
  Dt_MenuAria: "List options",
  Dt_Section_Records: "Records",
  Dt_Section_Export: "Export",
  Dt_Section_ColumnsVisible: "Visible columns",

  State_Default: "Default",
  State_NotDefault: "—",

  Js_ConfirmDeletePlantilla:
    "Delete this template? (soft delete). Companies using it will fall back to the default template.",

  ToastTitle_CreatePlantilla: "Create template",
  ToastMessage_PlantillaCreated: "Template \"{0}\" created successfully.",
  ToastTitle_EditPlantilla: "Edit template",
  ToastMessage_PlantillaUpdated: "Template \"{0}\" updated successfully.",
  ToastTitle_DeletePlantilla: "Delete template",
  ToastMessage_PlantillaDeleted: "Template \"{0}\" deleted successfully.",
  ToastTitle_DbError: "Database",
  ToastMessage_DbBrandColumnsMissing:
    "TSql_Plantilla is missing the brand columns (AttBrandText, AttBrandTextColor, AttBrandAccentColor). Run on the SQL server the script: App_Data/Sql/TSql_Plantilla_add_brand_text_and_colors.sql (Desing project folder).",

  Val_NameRequired: "Template name is required.",
  Val_BrandTextRequired: "Brand text is required.",
  Val_AccentColorRequired: "Accent color is required.",
  Val_AccentColorHexFormat: "Accent color must be a HEX value, e.g. #f29100.",
  Val_BrandTextColorHexFormat:
    "Text color: empty (inherits primary color) or HEX, e.g. #4c4c4c.",
  Val_MainColorRequired: "Main color is required.",
  Val_MainColorHexFormat: "Main color must be a HEX value, e.g. #349d7d.",
  Val_LogoPathRequired: "Logo path is required.",
  Val_DuplicateNameCreate: "A template with this name already exists.",
  Val_DuplicateNameEdit: "Another template already uses this name.",

  List_LinkEditTooltip: "Edit template",
  List_LinkDeleteTooltip: "Delete template",
  List_LinkOpenTooltip: "Open template",
  List_DefaultLockedTooltip: "The default template cannot be deleted.",

  Err_PlantillaNotFound: "Template not found.",
  Err_CannotDeleteDefault: "The default template cannot be deleted.",
  Err_LogoFormatNotAllowed: "Logo format is not allowed. Use: {0}.",
  Err_LogoTooLarge: "The logo exceeds the maximum size (2 MB).",
  Err_LogoSaveFailed: "Could not save the logo: {0}",
  Err_FaviconFormatNotAllowed: "Favicon format is not allowed. Use: {0}.",
  Err_FaviconTooLarge: "The favicon exceeds the maximum size (512 KB).",
  Err_FaviconSaveFailed: "Could not save the favicon: {0}",
  Msg_PlantillaDeleted: "Template deleted successfully.",

  Page_CreateTitle: "Create template",
  Page_EditTitle: "Edit template",

  Lbl_NameRequired: "Template name *",
  Lbl_BrandTextRequired: "Displayed brand *",
  Lbl_BrandAccentColor: "First letter color (accent) *",
  Lbl_BrandAccentColorTitle: "First-letter color",
  Lbl_BrandTextColor: "Brand text color (rest)",
  Lbl_BrandTextColorTitle: "Rest-of-text color (optional)",
  Lbl_MainColor: "Main color *",
  Lbl_Logo: "Logo",
  Lbl_Favicon: "Favicon",
  Lbl_IsDefault: "Mark as default template",

  Ph_Name: "E.g.: Acme Client Template",
  Ph_BrandText: "E.g.: T Desing.net",
  Ph_BrandTextColor: "Empty = primary color",
  Ph_LogoPath: "/Content/images/Login/at.png",
  Ph_FaviconPath: "/assets/client/images/Default/Ico/at.ico",

  Help_BrandText:
    "Shown in the navbar, login and footer. The first letter uses the accent color.",
  Help_BrandTextColor:
    "If empty, the rest of the name inherits the theme primary color.",
  Help_LogoFormats: "PNG, JPG, GIF, SVG, WEBP or ICO; max. 2 MB.",
  Help_FaviconSize: "Max. 512 KB.",
  Help_OnlyOneDefault:
    "Only one template can be the default; marking this will unmark the previous one.",

  Preview_Title: "Preview",
  Preview_ColorLabel: "Color:",

  Js_FileTooLargeLogo: "File exceeds 2 MB.",
  Js_FileTooLargeFavicon: "Favicon exceeds 512 KB.",

  Btn_Cancel: "Cancel",
  Btn_Exit: "Exit",
  Btn_SavePlantilla: "Create template",
  Btn_SaveChanges: "Save changes",
  Btn_Edit: "Edit",
  Btn_Delete: "Delete",

  Details_Field_Name: "Name",
  Details_Field_BrandText: "Brand",
  Details_Field_MainColor: "Main color",
  Details_Field_AccentColor: "Accent color",
  Details_Field_BrandTextColor: "Brand text color",
  Details_Field_Logo: "Logo",
  Details_Field_Favicon: "Favicon",
  Details_Field_IsDefault: "Default template",
  Details_Field_CreatedDate: "Created on",
  Details_NoValue: "-",
  Details_YesValue: "Yes",
  Details_NoBoolValue: "No",
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
// Auto-generated by _gen_plantilla_resources.js — Plantilla UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Plantilla module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Plantilla
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Plantilla", typeof(Plantilla).Assembly, "Plantilla"));

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
fs.writeFileSync(path.join(dir, "Plantilla.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Plantilla.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Plantilla.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Plantilla.*");
