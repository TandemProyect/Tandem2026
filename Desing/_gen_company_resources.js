const fs = require("fs");
const path = require("path");

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Index_Breadcrumb: "Empresas",
    Index_CreateCompany: "Crear nueva empresa",

    Col_Name: "Nombre",
    Col_AddLeter: "Signadura",
    Col_Logo: "Logo",
    Col_Description: "Descripción",
    Col_Address: "Dirección",
    Col_Address2: "Dirección 2",
    Col_PostalCode: "Código postal",
    Col_City: "Ciudad",
    Col_Country: "País",
    Col_State: "Estado",

    Dt_Rows_All: "Todas",
    Dt_Rows_N: "filas",
    Dt_MenuAria: "Opciones del listado",
    Dt_Section_Records: "Registros",
    Dt_Section_Export: "Exportar",
    Dt_Section_ColumnsVisible: "Columnas visibles",

    State_Active: "Activa",
    State_Inactive: "Desactivada",

    Js_ConfirmDeleteCompany: "¿Eliminar esta empresa?",
    Js_ConfirmToggleCompany: "¿Desactivar o activar esta empresa?",

    ToastTitle_CreateCompany: "Crear empresa",
    ToastMessage_CompanySaved: "Empresa creada correctamente.",
    ToastTitle_EditCompany: "Editar empresa",
    ToastMessage_CompanyUpdated: "Empresa actualizada correctamente.",

    Val_CompanyNameRequired: "El nombre de la empresa es obligatorio.",
    Val_DuplicateNameCreate: "Ya existe una empresa con ese nombre.",
    Val_DuplicateNameEdit: "Ya existe otra empresa con ese nombre.",
    Val_UiLanguageRequired:
      "Seleccione el idioma de trabajo para todos los empleados de esta empresa.",

    List_LinkOpenTooltip: "Abrir empresa",
    List_LinkEditTooltip: "Editar empresa",
    List_LinkDeleteTooltip: "Eliminar empresa",
    List_LinkToggleTooltip: "Activar o desactivar empresa",

    Err_CompanyNotFound: "Empresa no encontrada.",
    Err_CannotDeleteRelated:
      "No se puede eliminar. La empresa tiene datos relacionados.",
    Msg_CompanyDeleted: "Empresa eliminada correctamente.",
    Msg_CompanyPaused: "Empresa desactivada.",
    Msg_CompanyResumed: "Empresa activada.",

    Plantilla_InheritGlobalDefault: "— Heredar plantilla global por defecto —",

    Google_NotConfiguredStrong: "Google Maps no configurado.",
    Google_NotConfiguredBody:
      "Añada GoogleMaps:ApiKey en Web.config para autocompletado y mapa. Puede rellenar la dirección manualmente.",

    Lbl_NameRequired: "Nombre *",
    Lbl_AddLeter: "Signadura",
    Lbl_Country: "País",
    Dd_SelectPlaceholder: "— Seleccionar —",

    Lbl_VisualTemplate: "Plantilla visual (colores, marca)",
    Help_VisualTemplateEmployees:
      "Todos los empleados de esta empresa verán esta plantilla. Vacío = plantilla global por defecto.",

    Lbl_WorkUiLanguageRequired: "Idioma de trabajo (empleados) *",

    Lbl_Description: "Descripción",

    Lbl_AddressGooglePlaces: "Dirección (Google Places)",
    Lbl_LegacyBlockHint: "Campos de texto legacy (opcional)",
    Lbl_AddressLegacy: "Dirección",
    Lbl_AddressLegacy2: "Dirección 2",
    Lbl_PostalLegacy: "Código postal",
    Lbl_CityLegacy: "Ciudad",
    Lbl_LogoPath: "Logo (texto/ruta)",

    Btn_Edit: "Editar",
    Btn_Cancel: "Cancelar",
    Btn_SaveCompany: "Guardar empresa",
    Btn_SaveChanges: "Guardar cambios",

    Page_CreateTitle: "Crear empresa",
    Page_EditTitle: "Editar empresa",
    Details_Field_Name: "Nombre",
    Details_Field_AddLeter: "Signadura",
    Details_Field_Description: "Descripción",
    Details_Field_Address: "Dirección",
    Details_Field_Address2: "Dirección 2",
    Details_Field_Postal: "Código postal",
    Details_Field_City: "Ciudad",
    Details_Field_Country: "País",

    Details_Field_WorkLanguage: "Idioma de trabajo (empleados)",
    Details_NoValue: "-",
    Details_WorkLanguageHint:
      "La bandera corresponde al país asociado al idioma en Configuración → Idiomas. Los empleados de esta empresa usan este idioma en la aplicación.",

    LanguageSelect_FlagHint:
      "La bandera viene del país asociado al idioma en Configuración → Idiomas (campo país). Ej.: español → España.",
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Index_Breadcrumb: "Companies",
  Index_CreateCompany: "New company",

  Col_Name: "Name",
  Col_AddLeter: "Abbrev.",
  Col_Logo: "Logo",
  Col_Description: "Description",
  Col_Address: "Address",
  Col_Address2: "Address line 2",
  Col_PostalCode: "Postal code",
  Col_City: "City",
  Col_Country: "Country",
  Col_State: "Status",

  Dt_Rows_All: "All",
  Dt_Rows_N: "rows",
  Dt_MenuAria: "List options",
  Dt_Section_Records: "Records",
  Dt_Section_Export: "Export",
  Dt_Section_ColumnsVisible: "Visible columns",

  State_Active: "Active",
  State_Inactive: "Inactive",

  Js_ConfirmDeleteCompany: "Delete this company?",
  Js_ConfirmToggleCompany: "Deactivate or activate this company?",

  ToastTitle_CreateCompany: "Create company",
  ToastMessage_CompanySaved: "Company created successfully.",
  ToastTitle_EditCompany: "Edit company",
  ToastMessage_CompanyUpdated: "Company updated successfully.",

  Val_CompanyNameRequired: "Company name is required.",
  Val_DuplicateNameCreate: "A company with this name already exists.",
  Val_DuplicateNameEdit: "Another company already uses this name.",
  Val_UiLanguageRequired:
    "Choose the working language for all employees of this company.",

  List_LinkOpenTooltip: "Open company",
  List_LinkEditTooltip: "Edit company",
  List_LinkDeleteTooltip: "Delete company",
  List_LinkToggleTooltip: "Activate or deactivate company",

  Err_CompanyNotFound: "Company not found.",
  Err_CannotDeleteRelated:
    "Cannot delete. The company still has related data.",
  Msg_CompanyDeleted: "Company deleted successfully.",
  Msg_CompanyPaused: "Company deactivated.",
  Msg_CompanyResumed: "Company activated.",

  Plantilla_InheritGlobalDefault: "— Use default global layout —",

  Google_NotConfiguredStrong: "Google Maps is not configured.",
  Google_NotConfiguredBody:
    "Add GoogleMaps:ApiKey in Web.config for autocomplete and maps. You can still enter the address manually.",

  Lbl_NameRequired: "Name *",
  Lbl_AddLeter: "Abbrev.",
  Lbl_Country: "Country",
  Dd_SelectPlaceholder: "— Select —",

  Lbl_VisualTemplate: "Visual template (colors, branding)",
  Help_VisualTemplateEmployees:
    "Employees of this company will see this template. Empty means the global default template.",

  Lbl_WorkUiLanguageRequired: "Employees' working UI language *",

  Lbl_Description: "Description",

  Lbl_AddressGooglePlaces: "Address (Google Places)",
  Lbl_LegacyBlockHint: "Legacy text fields (optional)",
  Lbl_AddressLegacy: "Address",
  Lbl_AddressLegacy2: "Address line 2",
  Lbl_PostalLegacy: "Postal code",
  Lbl_CityLegacy: "City",
  Lbl_LogoPath: "Logo (text/path)",

  Btn_Edit: "Edit",
  Btn_Cancel: "Cancel",
  Btn_SaveCompany: "Save company",
  Btn_SaveChanges: "Save changes",

  Page_CreateTitle: "Create company",
  Page_EditTitle: "Edit company",
  Details_Field_Name: "Name",
  Details_Field_AddLeter: "Abbrev.",
  Details_Field_Description: "Description",
  Details_Field_Address: "Address",
  Details_Field_Address2: "Address line 2",
  Details_Field_Postal: "Postal code",
  Details_Field_City: "City",
  Details_Field_Country: "Country",

  Details_Field_WorkLanguage: "Employees' working UI language",
  Details_NoValue: "-",
  Details_WorkLanguageHint:
    "The flag comes from the country linked to the language under Settings → Languages. Employees at this company use this language across the application.",

  LanguageSelect_FlagHint:
    "The flag comes from the country linked to each language under Settings → Languages.",
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
// Auto-generated by _gen_company_resources.js — Empresa UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Company module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Company
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Company", typeof(Company).Assembly, "Company"));

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
fs.writeFileSync(path.join(dir, "Company.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Company.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Company.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Company.*");
