const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del catalogo paises (TSql_Countrys).
 * Clase: Desing.Resources.Country  |  TextModule DbBackedResourceManager: "Country"
 *
 * node Scripts/ResourceGenerators/_gen_country_catalog_resources.js
 */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Settings: "Configuración",
    Index_Breadcrumb: "Países",
    Index_CreateCountry: "Nuevo país",

    Col_Flag: "Bandera",
    Col_Name: "Nombre",
    Col_Iso2: "ISO 2",
    Col_Iso3: "ISO 3",
    Col_IsoNumber: "N.º ISO",
    Col_State: "Estado",

    State_Active: "Activo",
    State_Inactive: "Inactivo",

    Js_ConfirmDeleteCountry:
      "¿Eliminar este país de forma permanente? Solo es posible si no está enlazado a idiomas o empresas.",

    ToastTitle_CreateCountry: "Crear país",
    ToastMessage_CountryCreated: "País \"{0}\" creado correctamente.",
    ToastTitle_EditCountry: "Editar país",
    ToastMessage_CountryUpdated: "País \"{0}\" actualizado correctamente.",
    ToastTitle_DeleteCountry: "Eliminar país",
    ToastMessage_CountryDeleted: "País \"{0}\" eliminado correctamente.",

    Val_NameRequired: "El nombre del país es obligatorio.",
    Val_NameTooLong: "El nombre no puede superar los 500 caracteres.",
    Val_DuplicateNameCreate: "Ya existe un país con ese nombre.",
    Val_DuplicateNameEdit: "Ya existe otro país con ese nombre.",
    Val_Iso2TooLong: "ISO 2 no puede superar los 50 caracteres.",
    Val_Iso3TooLong: "ISO 3 no puede superar los 50 caracteres.",
    Val_NumberIsoTooLong: "El número ISO no puede superar los 50 caracteres.",
    Val_FlagPathTooLong: "La ruta de la bandera no puede superar los 200 caracteres.",

    List_LinkOpenTooltip: "Ver país",
    List_LinkEditTooltip: "Editar país",
    List_LinkDeleteTooltip: "Eliminar país",
    List_LinkDeleteLockedLanguagesTooltip:
      "No se puede eliminar: un idioma activo enlaza este país.",
    List_LinkDeleteLockedCompaniesTooltip:
      "No se puede eliminar: una empresa activa enlaza este país.",
    List_NoFlag: "—",

    Err_CountryNotFound: "País no encontrado.",
    Err_CannotDeleteHasLanguages:
      "No se puede eliminar: un idioma activo enlaza este país.",
    Err_CannotDeleteHasCompanies:
      "No se puede eliminar: una empresa activa enlaza este país.",

    Page_CreateTitle: "Crear país",
    Page_EditTitle: "Editar país",
    Page_DetailsTitle: "Detalle del país",

    Lbl_NameRequired: "Nombre *",
    Lbl_Iso2: "ISO 2 (alpha-2)",
    Lbl_Iso3: "ISO 3 (alpha-3)",
    Lbl_NumberIso: "Número ISO",
    Lbl_FlagPath: "Ruta bandera (virtual)",
    Lbl_Active: "Activo",
    Lbl_ActiveCheckbox: "País activo",

    Ph_Name: "Ej.: España",
    Ph_Iso2: "ES",
    Ph_Iso3: "ESP",
    Ph_NumberIso: "724",
    Ph_FlagPath: "~/Content/flags/es.svg",

    Btn_New: "Nuevo país",

    Details_Field_Name: "Nombre",
    Details_Field_Iso2: "ISO 2",
    Details_Field_Iso3: "ISO 3",
    Details_Field_NumberIso: "Número ISO",
    Details_Field_Flag: "Bandera",
    Details_Field_Active: "Activo",
    Details_NoValue: "—",
    Details_YesValue: "Sí",
    Details_NoBoolValue: "No"
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Settings: "Settings",
  Index_Breadcrumb: "Countries",
  Index_CreateCountry: "New country",

  Col_Flag: "Flag",
  Col_Name: "Name",
  Col_Iso2: "ISO 2",
  Col_Iso3: "ISO 3",
  Col_IsoNumber: "ISO no.",
  Col_State: "State",

  State_Active: "Active",
  State_Inactive: "Inactive",

  Js_ConfirmDeleteCountry:
    "Permanently delete this country? Only allowed if no language or company links it.",

  ToastTitle_CreateCountry: "Create country",
  ToastMessage_CountryCreated: "Country \"{0}\" created successfully.",
  ToastTitle_EditCountry: "Edit country",
  ToastMessage_CountryUpdated: "Country \"{0}\" updated successfully.",
  ToastTitle_DeleteCountry: "Delete country",
  ToastMessage_CountryDeleted: "Country \"{0}\" deleted successfully.",

  Val_NameRequired: "Country name is required.",
  Val_NameTooLong: "Name cannot exceed 500 characters.",
  Val_DuplicateNameCreate: "A country with this name already exists.",
  Val_DuplicateNameEdit: "Another country already uses this name.",
  Val_Iso2TooLong: "ISO 2 cannot exceed 50 characters.",
  Val_Iso3TooLong: "ISO 3 cannot exceed 50 characters.",
  Val_NumberIsoTooLong: "ISO number cannot exceed 50 characters.",
  Val_FlagPathTooLong: "Flag path cannot exceed 200 characters.",

  List_LinkOpenTooltip: "Open country",
  List_LinkEditTooltip: "Edit country",
  List_LinkDeleteTooltip: "Delete country",
  List_LinkDeleteLockedLanguagesTooltip:
    "Cannot delete: an active language links this country.",
  List_LinkDeleteLockedCompaniesTooltip:
    "Cannot delete: an active company links this country.",
  List_NoFlag: "—",

  Err_CountryNotFound: "Country not found.",
  Err_CannotDeleteHasLanguages:
    "Cannot delete: an active language links this country.",
  Err_CannotDeleteHasCompanies:
    "Cannot delete: an active company links this country.",

  Page_CreateTitle: "Create country",
  Page_EditTitle: "Edit country",
  Page_DetailsTitle: "Country details",

  Lbl_NameRequired: "Name *",
  Lbl_Iso2: "ISO 2 (alpha-2)",
  Lbl_Iso3: "ISO 3 (alpha-3)",
  Lbl_NumberIso: "ISO number",
  Lbl_FlagPath: "Flag path (virtual)",
  Lbl_Active: "Active",
  Lbl_ActiveCheckbox: "Active country",

  Ph_Name: "E.g. Spain",
  Ph_Iso2: "ES",
  Ph_Iso3: "ESP",
  Ph_NumberIso: "724",
  Ph_FlagPath: "~/Content/flags/es.svg",

  Btn_New: "New country",

  Details_Field_Name: "Name",
  Details_Field_Iso2: "ISO 2",
  Details_Field_Iso3: "ISO 3",
  Details_Field_NumberIso: "ISO number",
  Details_Field_Flag: "Flag",
  Details_Field_Active: "Active",
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
// Auto-generated by _gen_country_catalog_resources.js — Country (TSql_Countrys) UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Countrys catalog: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Country
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Country", typeof(Country).Assembly, "Country"));

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
fs.writeFileSync(path.join(dir, "Country.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Country.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Country.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Country.*");
