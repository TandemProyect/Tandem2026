const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del modulo XrDevice (TSql_XrDevice).
 */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Settings: "Configuración",
    Index_Breadcrumb: "Dispositivos XR",
    Index_Create: "Nuevo dispositivo",

    Col_Name: "Nombre",
    Col_Type: "Tipo",
    Col_Pairing: "Código emparejamiento",
    Col_Paired: "Emparejado",
    Col_State: "Estado",

    State_Active: "Activo",
    State_Inactive: "Inactivo",
    State_Paired: "Sí",
    State_Unpaired: "Pendiente",

    Type_Quest: "Quest (gafas)",
    Type_Tablet: "Tablet",

    Js_ConfirmDelete: "¿Eliminar este dispositivo XR? (borrado lógico).",

    ToastTitle_Create: "Crear dispositivo XR",
    ToastMessage_Created: "Dispositivo \"{0}\" creado correctamente.",
    ToastTitle_Edit: "Editar dispositivo XR",
    ToastMessage_Updated: "Dispositivo \"{0}\" actualizado correctamente.",
    ToastTitle_Delete: "Eliminar dispositivo XR",
    ToastMessage_Deleted: "Dispositivo \"{0}\" eliminado correctamente.",

    Val_NameRequired: "El nombre del dispositivo es obligatorio.",
    Val_NameTooLong: "El nombre no puede superar 500 caracteres.",
    Val_DuplicateNameCreate: "Ya existe un dispositivo con ese nombre.",
    Val_DuplicateNameEdit: "Ya existe otro dispositivo con ese nombre.",
    Val_PairingRequired: "El código de emparejamiento es obligatorio.",
    Val_PairingTooLong: "El código no puede superar 50 caracteres.",
    Val_DuplicatePairing: "Ese código de emparejamiento ya está en uso.",

    List_LinkOpenTooltip: "Ver dispositivo",
    List_LinkEditTooltip: "Editar dispositivo",
    List_LinkDeleteTooltip: "Eliminar dispositivo",

    Err_NotFound: "Dispositivo XR no encontrado.",
    Err_TablesMissing:
      "Faltan tablas XR en BD. Ejecute 2026-07-29_create_TSql_XrDevice.sql y 2026-07-29_create_TSql_XrPushJob.sql.",

    Page_CreateTitle: "Crear dispositivo XR",
    Page_EditTitle: "Editar dispositivo XR",
    Page_DetailsTitle: "Detalle del dispositivo XR",

    Lbl_NameRequired: "Nombre *",
    Lbl_Type: "Tipo *",
    Lbl_Pairing: "Código emparejamiento *",
    Lbl_Notes: "Notas",
    Lbl_Active: "Activo",
    Lbl_ActiveCheckbox: "Dispositivo activo",

    Ph_Name: "Ej.: Quest obra Madrid",
    Ph_Notes: "Ubicación, responsable…",
    Ph_Pairing: "Código para la app Unity",

    Details_Field_Name: "Nombre",
    Details_Field_Type: "Tipo",
    Details_Field_Pairing: "Código emparejamiento",
    Details_Field_Notes: "Notas",
    Details_Field_Paired: "Emparejado",
    Details_Field_LastSeen: "Última conexión",
    Details_Field_Active: "Activo",
    Details_NoNotes: "—",
    Details_NeverSeen: "Nunca"
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Settings: "Settings",
  Index_Breadcrumb: "XR devices",
  Index_Create: "New device",

  Col_Name: "Name",
  Col_Type: "Type",
  Col_Pairing: "Pairing code",
  Col_Paired: "Paired",
  Col_State: "State",

  State_Active: "Active",
  State_Inactive: "Inactive",
  State_Paired: "Yes",
  State_Unpaired: "Pending",

  Type_Quest: "Quest (headset)",
  Type_Tablet: "Tablet",

  Js_ConfirmDelete: "Delete this XR device? (soft delete).",

  ToastTitle_Create: "Create XR device",
  ToastMessage_Created: "Device \"{0}\" created successfully.",
  ToastTitle_Edit: "Edit XR device",
  ToastMessage_Updated: "Device \"{0}\" updated successfully.",
  ToastTitle_Delete: "Delete XR device",
  ToastMessage_Deleted: "Device \"{0}\" deleted successfully.",

  Val_NameRequired: "Device name is required.",
  Val_NameTooLong: "Name cannot exceed 500 characters.",
  Val_DuplicateNameCreate: "A device with this name already exists.",
  Val_DuplicateNameEdit: "Another device already uses this name.",
  Val_PairingRequired: "Pairing code is required.",
  Val_PairingTooLong: "Pairing code cannot exceed 50 characters.",
  Val_DuplicatePairing: "That pairing code is already in use.",

  List_LinkOpenTooltip: "Open device",
  List_LinkEditTooltip: "Edit device",
  List_LinkDeleteTooltip: "Delete device",

  Err_NotFound: "XR device not found.",
  Err_TablesMissing:
    "XR tables missing. Run 2026-07-29_create_TSql_XrDevice.sql and 2026-07-29_create_TSql_XrPushJob.sql.",

  Page_CreateTitle: "Create XR device",
  Page_EditTitle: "Edit XR device",
  Page_DetailsTitle: "XR device details",

  Lbl_NameRequired: "Name *",
  Lbl_Type: "Type *",
  Lbl_Pairing: "Pairing code *",
  Lbl_Notes: "Notes",
  Lbl_Active: "Active",
  Lbl_ActiveCheckbox: "Active device",

  Ph_Name: "E.g.: Quest Madrid site",
  Ph_Notes: "Location, owner…",
  Ph_Pairing: "Code for the Unity app",

  Details_Field_Name: "Name",
  Details_Field_Type: "Type",
  Details_Field_Pairing: "Pairing code",
  Details_Field_Notes: "Notes",
  Details_Field_Paired: "Paired",
  Details_Field_LastSeen: "Last seen",
  Details_Field_Active: "Active",
  Details_NoNotes: "—",
  Details_NeverSeen: "Never"
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
// Auto-generated by _gen_xrdevice_resources.js — XrDevice UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_XrDevice module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class XrDevice
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.XrDevice", typeof(XrDevice).Assembly, "XrDevice"));

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
fs.writeFileSync(path.join(dir, "XrDevice.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "XrDevice.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "XrDevice.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/XrDevice.*");
