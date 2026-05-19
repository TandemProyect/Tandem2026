const fs = require("fs");
const path = require("path");

/* -----------------------------------------------------------------------------
 * Module: Branch (TSql_Branch)
 * Generates: Resources/Branch.resx, Branch.en.resx, Branch.Designer.cs
 *
 * Scope:
 *   - All branch UI strings (currently consumed via Company.* in the
 *     _CompanyBranchesPanel.cshtml / _CompanyBranchesPanelList.cshtml /
 *     company-branches-panel.js + Branch_* endpoints in TSql_CompanyController).
 *   - The module is self-contained: ARIA labels, modal buttons and confirmation
 *     toasts shown by the branch widget all live here.
 * --------------------------------------------------------------------------- */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Intranet: "Intranet",

    /* Card / panel header */
    BranchPanel_Title: "Sedes (Branch)",
    BranchPanel_New: "Nueva sede",
    BranchPanel_CreateSectionTitle: "Crear sede",

    /* Inline form + modal labels */
    Branch_Lbl_NameRequiredModal: "Nombre *",
    Branch_Lbl_Description: "Descripción",
    Branch_Lbl_LetterShort: "Letra (máx. 2)",
    Branch_Lbl_AccentColor: "Color de acento (opcional)",
    Branch_Btn_SaveInline: "Guardar sede",

    /* Empty / disabled states */
    Branch_Msg_SaveToManageBranches:
      "Guarde la empresa para poder crear y gestionar sedes.",
    Branch_Msg_NoBusinessBlock:
      "No hay ningún negocio (TSql_Business) para esta empresa en base de datos. Debe existir al menos uno para poder crear sedes; el sistema asignará automáticamente ese negocio a cada nueva sede.",
    Branch_NoRows: "No hay sedes registradas.",

    /* Edit modal */
    Branch_ModalTitle_Edit: "Editar sede",
    Branch_ModalTitle_Create: "Nueva sede",
    Branch_ModalBtn_CancelModal: "Cancelar",
    Aria_CloseModal: "Cerrar",
    Btn_SaveChanges: "Guardar cambios",

    /* Row tooltips */
    Branch_RowTooltip_Edit: "Editar fila",
    Branch_RowTooltip_Delete: "Eliminar fila",

    /* Server-side validation / messages (controller JsonResults) */
    Branch_Err_InvalidCompany: "Empresa no válida.",
    Branch_Err_NameRequired: "El nombre de la sede es obligatorio.",
    Branch_Err_NoBusinessDetailed:
      "No hay negocio para esta empresa en base de datos. Cree al menos un TSql_Business con LinCompany igual a esta empresa antes de añadir sedes.",
    Branch_Err_InvalidData: "Datos no válidos.",
    Branch_Err_NotFound: "Sede no encontrada.",
    Branch_Msg_Created: "Sede creada correctamente.",
    Branch_Msg_Updated: "Sede actualizada correctamente.",
    Branch_Msg_Deleted: "Sede eliminada correctamente.",

    Branch_Page_EditTitle: "Editar sede",
    Branch_Page_DetailsTitle: "Detalle de sede",
    Branch_Btn_BackToCompany: "Volver a la empresa",
    Branch_Btn_OpenFullEdit: "Ficha completa (dirección)",
    Branch_Val_AttcolorHex: "El color debe ser un hexadecimal #RRGGBB (p. ej. #349d7d) o dejar vacío.",
    Branch_ToastTitle_Saved: "Sede",
    Branch_ToastMessage_Saved: "Datos de la sede guardados.",
    Branch_Details_Field_Name: "Nombre",
    Branch_Details_Field_Description: "Descripción",
    Branch_Details_Field_Letter: "Letra",
    Branch_Details_Field_Color: "Color",
    Branch_Details_Field_FormattedAddress: "Dirección (Google)",
    Branch_Details_NoValue: "—",

    /* JS i18n (data-i18n in _CompanyBranchesPanel.cshtml) */
    JsBranch_RefreshFail: "No se pudo actualizar la lista de sedes.",
    JsBranch_NewNeedsBusiness:
      "No puede crear sedes todavía: necesita al menos un negocio (TSql_Business) para esta empresa en base de datos.",
    JsBranch_NoBusinessInline:
      "No hay negocio asociado: no se puede crear la sede.",
    JsBranch_CreateFail: "Error al crear la sede.",
    JsBranch_NetworkError: "Error de red.",
    JsBranch_UseInlineFirst:
      "Use el botón «Nueva sede» para crear una sede en el cuadro de diálogo.",
    JsBranch_SaveFail: "Error al guardar.",
    JsBranch_DeleteConfirm: "¿Eliminar esta sede?",
    JsBranch_DeleteFail: "No se pudo eliminar.",
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Intranet: "Intranet",

  BranchPanel_Title: "Branches",
  BranchPanel_New: "New branch",
  BranchPanel_CreateSectionTitle: "Create branch",

  Branch_Lbl_NameRequiredModal: "Name *",
  Branch_Lbl_Description: "Description",
  Branch_Lbl_LetterShort: "Letter (max 2)",
  Branch_Lbl_AccentColor: "Accent color (optional)",
  Branch_Btn_SaveInline: "Save branch",

  Branch_Msg_SaveToManageBranches:
    "Save the company before you can add or manage branches.",
  Branch_Msg_NoBusinessBlock:
    "There is no TSql_Business row for this company. At least one is required before creating branches; new branches reuse that business automatically.",
  Branch_NoRows: "No branches yet.",

  Branch_ModalTitle_Edit: "Edit branch",
  Branch_ModalTitle_Create: "New branch",
  Branch_ModalBtn_CancelModal: "Cancel",
  Aria_CloseModal: "Close",
  Btn_SaveChanges: "Save changes",

  Branch_RowTooltip_Edit: "Edit",
  Branch_RowTooltip_Delete: "Delete",

  Branch_Err_InvalidCompany: "Invalid company.",
  Branch_Err_NameRequired: "Branch name is required.",
  Branch_Err_NoBusinessDetailed:
    "There is no business linked to this company. Create at least one TSql_Business with LinCompany pointing to this company before adding branches.",
  Branch_Err_InvalidData: "Invalid data.",
  Branch_Err_NotFound: "Branch not found.",
  Branch_Msg_Created: "Branch created successfully.",
  Branch_Msg_Updated: "Branch updated successfully.",
  Branch_Msg_Deleted: "Branch deleted successfully.",

  Branch_Page_EditTitle: "Edit branch",
  Branch_Page_DetailsTitle: "Branch details",
  Branch_Btn_BackToCompany: "Back to company",
  Branch_Btn_OpenFullEdit: "Full record (address)",
  Branch_Val_AttcolorHex: "Color must be #RRGGBB hex (e.g. #349d7d) or left empty.",
  Branch_ToastTitle_Saved: "Branch",
  Branch_ToastMessage_Saved: "Branch saved.",
  Branch_Details_Field_Name: "Name",
  Branch_Details_Field_Description: "Description",
  Branch_Details_Field_Letter: "Letter",
  Branch_Details_Field_Color: "Color",
  Branch_Details_Field_FormattedAddress: "Address (Google)",
  Branch_Details_NoValue: "—",

  JsBranch_RefreshFail: "Could not refresh the branch list.",
  JsBranch_NewNeedsBusiness:
    "You cannot add branches yet: at least one TSql_Business is required for this company.",
  JsBranch_NoBusinessInline: "No linked business — cannot create the branch.",
  JsBranch_CreateFail: "Could not create the branch.",
  JsBranch_NetworkError: "Network error.",
  JsBranch_UseInlineFirst:
    "Use the «New branch» button to create a branch in the dialog.",
  JsBranch_SaveFail: "Could not save.",
  JsBranch_DeleteConfirm: "Delete this branch?",
  JsBranch_DeleteFail: "Could not delete.",
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
// Auto-generated by _gen_branch_resources.js — Branch UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Branch module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Branch
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Branch", typeof(Branch).Assembly, "Branch"));

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
fs.writeFileSync(path.join(dir, "Branch.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Branch.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Branch.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Branch.*");
