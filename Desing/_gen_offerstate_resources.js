const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del modulo OfferState (TSql_OfferState).
 * Patron alineado con Extension / DocumentType (DbBackedResourceManager).
 */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Settings: "Configuración",
    Index_Breadcrumb: "Estados de oferta",
    Index_CreateOfferState: "Nuevo estado",

    Col_Name: "Nombre",
    Col_Color: "Color",
    Col_State: "Estado",

    State_Active: "Activo",
    State_Inactive: "Inactivo",

    Js_ConfirmDeleteOfferState:
      "¿Eliminar este estado de oferta? (borrado lógico).",

    ToastTitle_CreateOfferState: "Crear estado de oferta",
    ToastMessage_OfferStateCreated:
      "Estado de oferta \"{0}\" creado correctamente.",
    ToastTitle_EditOfferState: "Editar estado de oferta",
    ToastMessage_OfferStateUpdated:
      "Estado de oferta \"{0}\" actualizado correctamente.",
    ToastTitle_DeleteOfferState: "Eliminar estado de oferta",
    ToastMessage_OfferStateDeleted:
      "Estado de oferta \"{0}\" eliminado correctamente.",

    Val_NameRequired: "El nombre del estado es obligatorio.",
    Val_NameTooLong: "El nombre no puede superar 500 caracteres.",
    Val_DuplicateNameCreate: "Ya existe un estado de oferta con ese nombre.",
    Val_DuplicateNameEdit: "Ya existe otro estado de oferta con ese nombre.",
    Val_ColorInvalidHex:
      "Si indica color, use formato HEX: #RGB o #RRGGBB (ej.: #28a745).",

    List_LinkOpenTooltip: "Ver estado de oferta",
    List_LinkEditTooltip: "Editar estado de oferta",
    List_LinkDeleteTooltip: "Eliminar estado de oferta",
    List_LinkDeleteLockedOffersTooltip:
      "No se puede eliminar: hay ofertas que usan este estado.",

    Err_OfferStateNotFound: "Estado de oferta no encontrado.",
    Err_CannotDeleteHasOffers:
      "No se puede eliminar: hay ofertas que usan este estado.",

    Page_CreateTitle: "Crear estado de oferta",
    Page_EditTitle: "Editar estado de oferta",
    Page_DetailsTitle: "Detalle del estado de oferta",

    Lbl_NameRequired: "Nombre *",
    Lbl_Color: "Color",
    Lbl_ColorPickerTitle: "Elegir color",
    Lbl_Active: "Activo",
    Lbl_ActiveCheckbox: "Estado activo",

    Ph_Name: "Ej.: Enviada, Aceptada, Rechazada",
    Ph_ColorHex: "#RRGGBB",

    Details_Field_Name: "Nombre",
    Details_Field_Color: "Color",
    Details_Field_Active: "Activo",
    Details_NoColor: "—"
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Settings: "Settings",
  Index_Breadcrumb: "Offer states",
  Index_CreateOfferState: "New offer state",

  Col_Name: "Name",
  Col_Color: "Color",
  Col_State: "State",

  State_Active: "Active",
  State_Inactive: "Inactive",

  Js_ConfirmDeleteOfferState: "Delete this offer state? (soft delete).",

  ToastTitle_CreateOfferState: "Create offer state",
  ToastMessage_OfferStateCreated:
    "Offer state \"{0}\" created successfully.",
  ToastTitle_EditOfferState: "Edit offer state",
  ToastMessage_OfferStateUpdated:
    "Offer state \"{0}\" updated successfully.",
  ToastTitle_DeleteOfferState: "Delete offer state",
  ToastMessage_OfferStateDeleted:
    "Offer state \"{0}\" deleted successfully.",

  Val_NameRequired: "Offer state name is required.",
  Val_NameTooLong: "Name cannot exceed 500 characters.",
  Val_DuplicateNameCreate: "An offer state with this name already exists.",
  Val_DuplicateNameEdit: "Another offer state already uses this name.",
  Val_ColorInvalidHex:
    "If you set a color, use HEX format: #RGB or #RRGGBB (e.g. #28a745).",

  List_LinkOpenTooltip: "Open offer state",
  List_LinkEditTooltip: "Edit offer state",
  List_LinkDeleteTooltip: "Delete offer state",
  List_LinkDeleteLockedOffersTooltip:
    "Cannot delete: offers are using this state.",

  Err_OfferStateNotFound: "Offer state not found.",
  Err_CannotDeleteHasOffers:
    "Cannot delete: offers are using this state.",

  Page_CreateTitle: "Create offer state",
  Page_EditTitle: "Edit offer state",
  Page_DetailsTitle: "Offer state details",

  Lbl_NameRequired: "Name *",
  Lbl_Color: "Color",
  Lbl_ColorPickerTitle: "Pick color",
  Lbl_Active: "Active",
  Lbl_ActiveCheckbox: "Active state",

  Ph_Name: "E.g.: Sent, Accepted, Rejected",
  Ph_ColorHex: "#RRGGBB",

  Details_Field_Name: "Name",
  Details_Field_Color: "Color",
  Details_Field_Active: "Active",
  Details_NoColor: "—"
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
// Auto-generated by _gen_offerstate_resources.js — OfferState UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_OfferState module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class OfferState
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.OfferState", typeof(OfferState).Assembly, "OfferState"));

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
fs.writeFileSync(path.join(dir, "OfferState.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "OfferState.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "OfferState.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/OfferState.*");
