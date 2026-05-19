const fs = require("fs");
const path = require("path");

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Index_Breadcrumb: "Empleados",
    Index_CreateEmployee: "Crear nuevo empleado",

    Col_Avatar: "Avatar",
    Col_UserName: "Usuario",
    Col_Name: "Nombre",
    Col_Surname: "Apellido",
    Col_Company: "Empresa",
    Col_CompanyLetter: "Sig.",
    Col_Password: "Contraseña",
    Col_CreatedDate: "Fecha alta",
    Col_TotalDesigns: "Diseños",
    Col_Account: "Cuenta",
    Col_State: "Estado",

    Dt_Rows_All: "Todas",
    Dt_Rows_N: "filas",
    Dt_MenuAria: "Opciones del listado",
    Dt_Section_Records: "Registros",
    Dt_Section_Export: "Exportar",
    Dt_Section_ColumnsVisible: "Columnas visibles",

    State_Active: "Activo",
    State_Inactive: "Inactivo",
    State_AccountConfirmed: "Confirmada",
    State_AccountUnconfirmed: "Sin confirmar",

    Js_ConfirmDeleteEmployee: "¿Eliminar este empleado? (borrado lógico)",
    Js_ConfirmToggleEmployee: "¿Activar o desactivar la cuenta de este empleado?",
    Js_ConfirmSendMail: "¿Enviar al empleado un correo con su contraseña?",
    Js_SendMailSuccessFallback: "Correo enviado correctamente.",
    Js_SendMailErrorFallback: "No se pudo enviar el correo.",
    Js_SendMailUnexpected: "Respuesta inesperada del servidor.",
    Js_SendMailNetworkError: "Ocurrió un error al enviar el mensaje.",

    ToastTitle_CreateEmployee: "Crear empleado",
    ToastMessage_EmployeeSaved: "Empleado guardado correctamente.",
    ToastTitle_EditEmployee: "Editar empleado",
    ToastMessage_EmployeeUpdated: "Empleado actualizado correctamente.",
    ToastTitle_DeleteEmployee: "Eliminar empleado",
    ToastMessage_EmployeeDeleted: "Empleado eliminado correctamente.",
    ToastTitle_ToggleEmployee: "Activar / desactivar empleado",
    ToastMessage_EmployeeActivated: "Cuenta del empleado activada.",
    ToastMessage_EmployeeDeactivated: "Cuenta del empleado desactivada.",

    Val_NameRequired: "El nombre del empleado es obligatorio.",
    Val_SurnameRequired: "El apellido del empleado es obligatorio.",
    Val_CompanyRequired: "Seleccione la empresa del empleado.",

    List_LinkOpenTooltip: "Ver empleado",
    List_LinkEditTooltip: "Editar empleado",
    List_LinkDeleteTooltip: "Eliminar empleado",
    List_LinkToggleTooltip: "Activar / desactivar cuenta",
    List_LinkSendMailTooltip: "Enviar contraseña por correo",

    Err_EmployeeNotFound: "Empleado no encontrado.",
    Err_UserNotFound: "Usuario asociado no encontrado.",
    Err_CannotDeleteRelated: "No se puede eliminar: el empleado tiene diseños asignados.",
    Err_MailNotConfigured: "Envío de correo no disponible.",
    Err_GenericFailure: "Se produjo un error al procesar la operación.",
    Msg_EmployeeDeleted: "Empleado eliminado correctamente.",
    Msg_EmployeeActivated: "Cuenta del empleado activada.",
    Msg_EmployeeDeactivated: "Cuenta del empleado desactivada.",
    Msg_MailSent: "Correo enviado correctamente.",

    Page_CreateTitle: "Crear empleado",
    Page_EditTitle: "Editar empleado",

    Lbl_NameRequired: "Nombre *",
    Lbl_SurnameRequired: "Apellido *",
    Lbl_Company: "Empresa",
    Lbl_CompanyHint: "La plantilla visual y el idioma de trabajo los hereda el empleado de la empresa.",
    Lbl_Photo: "Foto de perfil",
    Lbl_PhotoHint: "Sube una imagen cuadrada (se redimensionará automáticamente a 100×100).",
    Lbl_DeviceSectionTitle: "Autorización de equipo para plugin",
    Lbl_DeviceName: "Nombre del equipo",
    Lbl_DeviceId: "ID del equipo (DeviceId)",
    Lbl_DeviceAllowed: "Autorizar plugin para este equipo",
    Ph_NameRequired: "Inserta nombre",
    Ph_SurnameRequired: "Inserta apellido",
    Ph_DeviceName: "Ej.: PC-OFICINA-01",
    Ph_DeviceId: "Hash DeviceId del plugin",

    Header_AccountInfo: "Alta de empleado para el usuario:",

    Btn_Cancel: "Cancelar",
    Btn_Exit: "Salir",
    Btn_SaveEmployee: "Guardar empleado",
    Btn_SaveChanges: "Guardar cambios",
    Btn_Edit: "Editar",
    Btn_Delete: "Eliminar",
    Btn_Toggle: "Activar / Desactivar",
    Btn_SendMail: "Enviar correo",

    Details_Field_Name: "Nombre",
    Details_Field_Surname: "Apellido",
    Details_Field_Company: "Empresa",
    Details_Field_Account: "Cuenta de usuario",
    Details_Field_CreatedDate: "Fecha de alta",
    Details_Field_TotalDesigns: "Diseños creados",
    Details_NoValue: "-",

    MySpace_Breadcrumb: "Mi espacio",
    MySpace_Welcome: "Bienvenido, {0}",
    MySpace_SectionTitle: "Información del usuario",
    MySpace_Field_FullName: "Nombre completo:",
    MySpace_Field_UserName: "Usuario:",
    MySpace_Field_FirstLogin: "Fecha del primer inicio de sesión:",
    MySpace_Field_TotalDesigns: "Diseños creados:",

    Modal_DeleteLegend: "¿Deseas eliminar al empleado?",
    Modal_DeleteConfirm: "Esta acción marca el empleado como eliminado (borrado lógico). Sus datos quedarán fuera de los listados normales.",
    Modal_ActiveLegendActivated: "El usuario está activado.",
    Modal_ActiveLegendDeactivated: "El usuario está desactivado.",
    Modal_ActiveQuestionActivate: "¿Quieres activar al usuario {0}?",
    Modal_ActiveQuestionDeactivate: "¿Quieres desactivar al usuario {0}?",
    Modal_BtnActivate: "Activar",
    Modal_BtnDeactivate: "Desactivar",
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Index_Breadcrumb: "Employees",
  Index_CreateEmployee: "New employee",

  Col_Avatar: "Avatar",
  Col_UserName: "User",
  Col_Name: "Name",
  Col_Surname: "Surname",
  Col_Company: "Company",
  Col_CompanyLetter: "Abbrev.",
  Col_Password: "Password",
  Col_CreatedDate: "Created",
  Col_TotalDesigns: "Designs",
  Col_Account: "Account",
  Col_State: "Status",

  Dt_Rows_All: "All",
  Dt_Rows_N: "rows",
  Dt_MenuAria: "List options",
  Dt_Section_Records: "Records",
  Dt_Section_Export: "Export",
  Dt_Section_ColumnsVisible: "Visible columns",

  State_Active: "Active",
  State_Inactive: "Inactive",
  State_AccountConfirmed: "Confirmed",
  State_AccountUnconfirmed: "Unconfirmed",

  Js_ConfirmDeleteEmployee: "Delete this employee? (soft delete)",
  Js_ConfirmToggleEmployee: "Activate or deactivate this employee account?",
  Js_ConfirmSendMail: "Send the employee an email with their password?",
  Js_SendMailSuccessFallback: "Email sent successfully.",
  Js_SendMailErrorFallback: "Could not send the email.",
  Js_SendMailUnexpected: "Unexpected response from server.",
  Js_SendMailNetworkError: "An error occurred while sending the message.",

  ToastTitle_CreateEmployee: "Create employee",
  ToastMessage_EmployeeSaved: "Employee saved successfully.",
  ToastTitle_EditEmployee: "Edit employee",
  ToastMessage_EmployeeUpdated: "Employee updated successfully.",
  ToastTitle_DeleteEmployee: "Delete employee",
  ToastMessage_EmployeeDeleted: "Employee deleted successfully.",
  ToastTitle_ToggleEmployee: "Activate / deactivate employee",
  ToastMessage_EmployeeActivated: "Employee account activated.",
  ToastMessage_EmployeeDeactivated: "Employee account deactivated.",

  Val_NameRequired: "Employee name is required.",
  Val_SurnameRequired: "Employee surname is required.",
  Val_CompanyRequired: "Select the employee's company.",

  List_LinkOpenTooltip: "Open employee",
  List_LinkEditTooltip: "Edit employee",
  List_LinkDeleteTooltip: "Delete employee",
  List_LinkToggleTooltip: "Activate / deactivate account",
  List_LinkSendMailTooltip: "Email password to the employee",

  Err_EmployeeNotFound: "Employee not found.",
  Err_UserNotFound: "Linked user account not found.",
  Err_CannotDeleteRelated: "Cannot delete: the employee has designs assigned.",
  Err_MailNotConfigured: "Email delivery is not available.",
  Err_GenericFailure: "An error occurred while processing the operation.",
  Msg_EmployeeDeleted: "Employee deleted successfully.",
  Msg_EmployeeActivated: "Employee account activated.",
  Msg_EmployeeDeactivated: "Employee account deactivated.",
  Msg_MailSent: "Email sent successfully.",

  Page_CreateTitle: "Create employee",
  Page_EditTitle: "Edit employee",

  Lbl_NameRequired: "Name *",
  Lbl_SurnameRequired: "Surname *",
  Lbl_Company: "Company",
  Lbl_CompanyHint: "Visual template and working language are inherited from the company.",
  Lbl_Photo: "Profile photo",
  Lbl_PhotoHint: "Upload a square image (it will be resized to 100×100).",
  Lbl_DeviceSectionTitle: "Device authorisation for the plugin",
  Lbl_DeviceName: "Device name",
  Lbl_DeviceId: "Device ID (DeviceId)",
  Lbl_DeviceAllowed: "Authorise the plugin on this device",
  Ph_NameRequired: "Enter name",
  Ph_SurnameRequired: "Enter surname",
  Ph_DeviceName: "E.g.: PC-OFFICE-01",
  Ph_DeviceId: "DeviceId hash from the plugin",

  Header_AccountInfo: "Creating an employee for user:",

  Btn_Cancel: "Cancel",
  Btn_Exit: "Exit",
  Btn_SaveEmployee: "Save employee",
  Btn_SaveChanges: "Save changes",
  Btn_Edit: "Edit",
  Btn_Delete: "Delete",
  Btn_Toggle: "Activate / Deactivate",
  Btn_SendMail: "Send email",

  Details_Field_Name: "Name",
  Details_Field_Surname: "Surname",
  Details_Field_Company: "Company",
  Details_Field_Account: "User account",
  Details_Field_CreatedDate: "Created",
  Details_Field_TotalDesigns: "Designs created",
  Details_NoValue: "-",

  MySpace_Breadcrumb: "My space",
  MySpace_Welcome: "Welcome, {0}",
  MySpace_SectionTitle: "User information",
  MySpace_Field_FullName: "Full name:",
  MySpace_Field_UserName: "User:",
  MySpace_Field_FirstLogin: "First sign-in date:",
  MySpace_Field_TotalDesigns: "Designs created:",

  Modal_DeleteLegend: "Delete this employee?",
  Modal_DeleteConfirm: "This action flags the employee as deleted (soft delete). Their data will be hidden from regular listings.",
  Modal_ActiveLegendActivated: "The user is active.",
  Modal_ActiveLegendDeactivated: "The user is inactive.",
  Modal_ActiveQuestionActivate: "Do you want to activate user {0}?",
  Modal_ActiveQuestionDeactivate: "Do you want to deactivate user {0}?",
  Modal_BtnActivate: "Activate",
  Modal_BtnDeactivate: "Deactivate",
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
// Auto-generated by _gen_employee_resources.js — Empleado UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>TSql_Employee module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Employee
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Employee", typeof(Employee).Assembly, "Employee"));

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
fs.writeFileSync(path.join(dir, "Employee.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Employee.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Employee.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Employee.*");
