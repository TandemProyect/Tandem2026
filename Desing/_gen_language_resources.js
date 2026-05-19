const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del modulo Language (TSql_Language).
 *
 * Sigue el patron Extension / DocumentType / Jobside / Common:
 *   - Clase estatica Desing.Resources.Language (DbBackedResourceManager).
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
 *   Country_*                       -> autocompletado + catalogo de paises
 *   Info_*                          -> avisos informativos del formulario
 *
 * NOTA: este modulo alimenta el switcher del navbar y DbBackedResourceManager.
 * Las cadenas NO afectan a la logica de seleccion de idioma; solo a la UI
 * del CRUD. La cache del DbBackedResourceManager se invalida automaticamente
 * tras el Import de UiTranslation.
 */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Settings: "Configuración",
    Index_Breadcrumb: "Idiomas",
    Index_CreateLanguage: "Nuevo idioma",
    Index_FallbackNote:
      "Si el usuario elige un idioma sin traducción para una clave, se muestra el texto del idioma <strong>predeterminado</strong> o el literal de la vista.",

    Col_Flag: "Bandera",
    Col_Name: "Nombre",
    Col_Code: "Código",
    Col_NativeName: "Nombre nativo",
    Col_Default: "Predeterminado",
    Col_Country: "País",
    Col_State: "Estado",

    State_Active: "Activo",
    State_Inactive: "Inactivo",
    State_Default: "Predeterminado",
    State_DefaultYes: "Sí",
    State_DefaultNo: "No",

    Js_ConfirmDeleteLanguage:
      "¿Eliminar este idioma? (borrado lógico).",

    ToastTitle_CreateLanguage: "Crear idioma",
    ToastMessage_LanguageCreated:
      "Idioma \"{0}\" creado correctamente.",
    ToastTitle_EditLanguage: "Editar idioma",
    ToastMessage_LanguageUpdated:
      "Idioma \"{0}\" actualizado correctamente.",
    ToastTitle_DeleteLanguage: "Eliminar idioma",
    ToastMessage_LanguageDeleted:
      "Idioma \"{0}\" eliminado correctamente.",

    Val_NameRequired: "El nombre del idioma es obligatorio.",
    Val_NameTooLong: "El nombre no puede superar los 500 caracteres.",
    Val_CodeRequired: "El código UI (TextCode) es obligatorio.",
    Val_CodeTooLong:
      "El código no puede superar los 20 caracteres.",
    Val_CodeFormat:
      "Use un código tipo «es», «en» o «en-gb» (letras y guion opcional).",
    Val_CodeDuplicate:
      "Ya existe un idioma activo con ese código.",
    Val_NativeNameTooLong:
      "El nombre nativo no puede superar los 100 caracteres.",
    Val_DefaultMustExist:
      "Debe existir un idioma por defecto. Designe otro idioma como predeterminado antes de quitar este.",

    List_LinkOpenTooltip: "Ver idioma",
    List_LinkEditTooltip: "Editar idioma",
    List_LinkDeleteTooltip: "Eliminar idioma",
    List_LinkDeleteLockedDefaultTooltip:
      "No se puede eliminar: el idioma está marcado como predeterminado.",
    List_LinkDeleteLockedCompaniesTooltip:
      "No se puede eliminar: el idioma está enlazado a empresas activas.",
    List_NoFlag: "—",
    List_NoCountry: "—",
    List_NoNativeName: "—",

    Err_LanguageNotFound: "Idioma no encontrado.",
    Err_CannotDeleteIsDefault:
      "No se puede eliminar el idioma marcado como predeterminado.",
    Err_CannotDeleteHasCompanies:
      "No se puede eliminar: el idioma está enlazado a empresas activas.",

    Page_CreateTitle: "Crear idioma",
    Page_EditTitle: "Editar idioma",
    Page_DetailsTitle: "Detalle del idioma",

    Lbl_NameRequired: "Nombre / etiqueta *",
    Lbl_CodeRequired: "Código UI (TextCode) *",
    Lbl_NativeName: "Nombre nativo (opcional)",
    Lbl_LinkCountry: "País (bandera e ISO desde catálogo)",
    Lbl_State: "Estado",
    Lbl_ActiveCheckbox: "Idioma activo",
    Lbl_Default: "Predeterminado",
    Lbl_DefaultCheckbox: "Idioma por defecto del sitio",

    Ph_Name: "Ej.: English",
    Ph_Code: "en",
    Ph_NativeName: "English",
    Ph_CountrySearch:
      "Buscar (3 letras) o usar la lista…",

    Btn_New: "Nuevo idioma",

    Country_AriaToggle:
      "Abrir catálogo de países",
    Country_TitleToggle: "Catálogo de países",
    Country_Loading: "Cargando…",
    Country_Empty: "Sin países en catálogo.",
    Country_LoadError:
      "No se pudo cargar el catálogo.",

    Info_NotDefaultOnCreate:
      "El nuevo idioma no será el predeterminado. El idioma por defecto del sitio sigue siendo el actual hasta que cambie el marcador en <strong>Editar</strong> o en otro idioma.",

    Details_Field_Name: "Nombre",
    Details_Field_Code: "Código",
    Details_Field_NativeName: "Nombre nativo",
    Details_Field_Country: "País",
    Details_Field_Flag: "Bandera",
    Details_Field_State: "Estado",
    Details_Field_Default: "Predeterminado",
    Details_NoValue: "—",
    Details_YesValue: "Sí",
    Details_NoBoolValue: "No"
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Settings: "Settings",
  Index_Breadcrumb: "Languages",
  Index_CreateLanguage: "New language",
  Index_FallbackNote:
    "If a user picks a language without a translation for a key, the text from the <strong>default</strong> language (or the literal in the view) is shown.",

  Col_Flag: "Flag",
  Col_Name: "Name",
  Col_Code: "Code",
  Col_NativeName: "Native name",
  Col_Default: "Default",
  Col_Country: "Country",
  Col_State: "State",

  State_Active: "Active",
  State_Inactive: "Inactive",
  State_Default: "Default",
  State_DefaultYes: "Yes",
  State_DefaultNo: "No",

  Js_ConfirmDeleteLanguage: "Delete this language? (soft delete).",

  ToastTitle_CreateLanguage: "Create language",
  ToastMessage_LanguageCreated:
    "Language \"{0}\" created successfully.",
  ToastTitle_EditLanguage: "Edit language",
  ToastMessage_LanguageUpdated:
    "Language \"{0}\" updated successfully.",
  ToastTitle_DeleteLanguage: "Delete language",
  ToastMessage_LanguageDeleted:
    "Language \"{0}\" deleted successfully.",

  Val_NameRequired: "Language name is required.",
  Val_NameTooLong: "Name cannot exceed 500 characters.",
  Val_CodeRequired: "The UI code (TextCode) is required.",
  Val_CodeTooLong: "Code cannot exceed 20 characters.",
  Val_CodeFormat:
    "Use a code like \"es\", \"en\" or \"en-gb\" (letters and optional dash).",
  Val_CodeDuplicate:
    "An active language with that code already exists.",
  Val_NativeNameTooLong:
    "Native name cannot exceed 100 characters.",
  Val_DefaultMustExist:
    "A default language must exist. Set another language as default before unmarking this one.",

  List_LinkOpenTooltip: "Open language",
  List_LinkEditTooltip: "Edit language",
  List_LinkDeleteTooltip: "Delete language",
  List_LinkDeleteLockedDefaultTooltip:
    "Cannot delete: this language is marked as default.",
  List_LinkDeleteLockedCompaniesTooltip:
    "Cannot delete: the language is linked to active companies.",
  List_NoFlag: "—",
  List_NoCountry: "—",
  List_NoNativeName: "—",

  Err_LanguageNotFound: "Language not found.",
  Err_CannotDeleteIsDefault:
    "Cannot delete the language marked as default.",
  Err_CannotDeleteHasCompanies:
    "Cannot delete: the language is linked to active companies.",

  Page_CreateTitle: "Create language",
  Page_EditTitle: "Edit language",
  Page_DetailsTitle: "Language details",

  Lbl_NameRequired: "Name / label *",
  Lbl_CodeRequired: "UI code (TextCode) *",
  Lbl_NativeName: "Native name (optional)",
  Lbl_LinkCountry: "Country (flag and ISO from catalog)",
  Lbl_State: "State",
  Lbl_ActiveCheckbox: "Active language",
  Lbl_Default: "Default",
  Lbl_DefaultCheckbox: "Default site language",

  Ph_Name: "E.g.: English",
  Ph_Code: "en",
  Ph_NativeName: "English",
  Ph_CountrySearch: "Search (3 letters) or use the list…",

  Btn_New: "New language",

  Country_AriaToggle: "Open country catalog",
  Country_TitleToggle: "Country catalog",
  Country_Loading: "Loading…",
  Country_Empty: "No countries in catalog.",
  Country_LoadError: "Could not load the catalog.",

  Info_NotDefaultOnCreate:
    "The new language will not be the default one. The site default language stays until you toggle the flag in <strong>Edit</strong> on another language.",

  Details_Field_Name: "Name",
  Details_Field_Code: "Code",
  Details_Field_NativeName: "Native name",
  Details_Field_Country: "Country",
  Details_Field_Flag: "Flag",
  Details_Field_State: "State",
  Details_Field_Default: "Default",
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
// Auto-generated by _gen_language_resources.js — Language UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Language module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Language
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Language", typeof(Language).Assembly, "Language"));

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
fs.writeFileSync(path.join(dir, "Language.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Language.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Language.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Language.*");
