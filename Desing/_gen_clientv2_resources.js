const fs = require("fs");
const path = require("path");

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Intranet: "Intranet",
    Index_Breadcrumb: "Clientes",
    Index_CreateClient: "Nuevo cliente",

    Col_Name: "Nombre",
    Col_Code: "Código",
    Col_TaxId: "CIF/NIF",
    Col_Email: "Email",
    Col_Phone: "Teléfono",
    Col_Logo: "Logo",
    Col_State: "Estado",

    Dt_Rows_All: "Todas",
    Dt_Rows_N: "filas",
    Dt_MenuAria: "Opciones del listado",
    Dt_Section_Records: "Registros",
    Dt_Section_Export: "Exportar",
    Dt_Section_ColumnsVisible: "Columnas visibles",

    State_Active: "Activo",
    State_Inactive: "Inactivo",

    Js_ConfirmDeleteClient:
      "¿Eliminar este cliente? (borrado lógico).",

    ToastTitle_CreateClient: "Crear cliente",
    ToastMessage_ClientCreated: "Cliente \"{0}\" creado correctamente.",
    ToastTitle_EditClient: "Editar cliente",
    ToastMessage_ClientUpdated: "Cliente \"{0}\" actualizado correctamente.",
    ToastTitle_DeleteClient: "Eliminar cliente",
    ToastMessage_ClientDeleted: "Cliente \"{0}\" eliminado correctamente.",

    Val_NameRequired: "El nombre del cliente es obligatorio.",
    Val_DuplicateNameCreate: "Ya existe un cliente con ese nombre.",
    Val_DuplicateNameEdit: "Ya existe otro cliente con ese nombre.",
    Val_EmailFormat: "El email no tiene un formato válido.",

    List_LinkOpenTooltip: "Ver cliente",
    List_LinkEditTooltip: "Editar cliente",
    List_LinkDeleteTooltip: "Eliminar cliente",
    List_LinkDeleteLockedJobsidesTooltip:
      "No se puede eliminar: tiene obras asociadas.",
    List_LinkDeleteLockedDocumentsTooltip:
      "No se puede eliminar: tiene documentos asociados.",
    List_LinkDeleteLockedOffersTooltip:
      "No se puede eliminar: tiene ofertas asociadas.",

    Err_ClientNotFound: "Cliente no encontrado.",
    Err_CannotDeleteHasJobsides:
      "No se puede eliminar: tiene obras asociadas.",
    Err_CannotDeleteHasDocuments:
      "No se puede eliminar: tiene documentos asociados.",
    Err_CannotDeleteHasOffers:
      "No se puede eliminar: tiene ofertas asociadas.",
    Err_IconFormatNotAllowed: "Formato de icono no permitido.",
    Err_LogoFormatNotAllowed: "Formato de logo no permitido.",
    Err_IconSaveFailed: "No se pudo guardar el icono: {0}",
    Err_LogoSaveFailed: "No se pudo guardar el logo: {0}",

    Page_CreateTitle: "Crear cliente",
    Page_EditTitle: "Editar cliente",

    Lbl_NameRequired: "Nombre *",
    Lbl_Code: "Código interno",
    Lbl_TaxId: "CIF / NIF",
    Lbl_Email: "Email",
    Lbl_Phone: "Teléfono",
    Lbl_MethodOfPayment: "Método de pago",
    Lbl_MethodOfPaymentEmpty: "-- Sin asignar --",
    Lbl_Active: "Activo",
    Lbl_ActiveCheckbox: "Cliente activo",
    Lbl_Icon: "Icono",
    Lbl_Logo: "Logo",

    Ph_Name: "Ej.: Constructora Acme, S.L.",
    Ph_Code: "Ej.: ACME-001",
    Ph_TaxId: "Ej.: B12345678",
    Ph_Email: "cliente@empresa.com",
    Ph_Phone: "+34 600 000 000",

    Btn_New: "Nuevo cliente",
    Btn_Save: "Guardar",
    Btn_SaveChanges: "Guardar cambios",
    Btn_Cancel: "Cancelar",
    Btn_Exit: "Salir",
    Btn_Edit: "Editar",
    Btn_Delete: "Eliminar",

    Details_Field_Name: "Nombre",
    Details_Field_Code: "Código",
    Details_Field_TaxId: "CIF/NIF",
    Details_Field_Email: "Email",
    Details_Field_Phone: "Teléfono",
    Details_Field_MethodOfPayment: "Método de pago",
    Details_Field_Active: "Activo",
    Details_Field_Icon: "Icono",
    Details_Field_Logo: "Logo",
    Details_NoValue: "—",
    Details_YesValue: "Sí",
    Details_NoBoolValue: "No",
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Intranet: "Intranet",
  Index_Breadcrumb: "Clients",
  Index_CreateClient: "New client",

  Col_Name: "Name",
  Col_Code: "Code",
  Col_TaxId: "Tax ID",
  Col_Email: "Email",
  Col_Phone: "Phone",
  Col_Logo: "Logo",
  Col_State: "State",

  Dt_Rows_All: "All",
  Dt_Rows_N: "rows",
  Dt_MenuAria: "List options",
  Dt_Section_Records: "Records",
  Dt_Section_Export: "Export",
  Dt_Section_ColumnsVisible: "Visible columns",

  State_Active: "Active",
  State_Inactive: "Inactive",

  Js_ConfirmDeleteClient: "Delete this client? (soft delete).",

  ToastTitle_CreateClient: "Create client",
  ToastMessage_ClientCreated: "Client \"{0}\" created successfully.",
  ToastTitle_EditClient: "Edit client",
  ToastMessage_ClientUpdated: "Client \"{0}\" updated successfully.",
  ToastTitle_DeleteClient: "Delete client",
  ToastMessage_ClientDeleted: "Client \"{0}\" deleted successfully.",

  Val_NameRequired: "Client name is required.",
  Val_DuplicateNameCreate: "A client with this name already exists.",
  Val_DuplicateNameEdit: "Another client already uses this name.",
  Val_EmailFormat: "The email has an invalid format.",

  List_LinkOpenTooltip: "Open client",
  List_LinkEditTooltip: "Edit client",
  List_LinkDeleteTooltip: "Delete client",
  List_LinkDeleteLockedJobsidesTooltip:
    "Cannot delete: it has linked jobsides.",
  List_LinkDeleteLockedDocumentsTooltip:
    "Cannot delete: it has linked documents.",
  List_LinkDeleteLockedOffersTooltip:
    "Cannot delete: it has linked offers.",

  Err_ClientNotFound: "Client not found.",
  Err_CannotDeleteHasJobsides: "Cannot delete: it has linked jobsides.",
  Err_CannotDeleteHasDocuments: "Cannot delete: it has linked documents.",
  Err_CannotDeleteHasOffers: "Cannot delete: it has linked offers.",
  Err_IconFormatNotAllowed: "Icon format is not allowed.",
  Err_LogoFormatNotAllowed: "Logo format is not allowed.",
  Err_IconSaveFailed: "Could not save the icon: {0}",
  Err_LogoSaveFailed: "Could not save the logo: {0}",

  Page_CreateTitle: "Create client",
  Page_EditTitle: "Edit client",

  Lbl_NameRequired: "Name *",
  Lbl_Code: "Internal code",
  Lbl_TaxId: "Tax ID",
  Lbl_Email: "Email",
  Lbl_Phone: "Phone",
  Lbl_MethodOfPayment: "Payment method",
  Lbl_MethodOfPaymentEmpty: "-- Unassigned --",
  Lbl_Active: "Active",
  Lbl_ActiveCheckbox: "Active client",
  Lbl_Icon: "Icon",
  Lbl_Logo: "Logo",

  Ph_Name: "E.g.: Acme Builders Ltd.",
  Ph_Code: "E.g.: ACME-001",
  Ph_TaxId: "E.g.: B12345678",
  Ph_Email: "client@company.com",
  Ph_Phone: "+1 555 000 0000",

  Btn_New: "New client",
  Btn_Save: "Save",
  Btn_SaveChanges: "Save changes",
  Btn_Cancel: "Cancel",
  Btn_Exit: "Exit",
  Btn_Edit: "Edit",
  Btn_Delete: "Delete",

  Details_Field_Name: "Name",
  Details_Field_Code: "Code",
  Details_Field_TaxId: "Tax ID",
  Details_Field_Email: "Email",
  Details_Field_Phone: "Phone",
  Details_Field_MethodOfPayment: "Payment method",
  Details_Field_Active: "Active",
  Details_Field_Icon: "Icon",
  Details_Field_Logo: "Logo",
  Details_NoValue: "—",
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
// Auto-generated by _gen_clientv2_resources.js — ClientV2 UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Client_V2 module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class ClientV2
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.ClientV2", typeof(ClientV2).Assembly, "ClientV2"));

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
fs.writeFileSync(path.join(dir, "ClientV2.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "ClientV2.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "ClientV2.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/ClientV2.*");
