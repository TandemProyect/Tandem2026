const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del modulo Security (roles y funciones:
 * TSql_Roles_V2, TSql_Functions, TSql_RoleFunction, TSql_UserRole).
 * Patron alineado con OfferState (DbBackedResourceManager).
 */

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Security: "Configuración",
    State_Active: "Activo",
    State_Inactive: "Inactivo",
    Col_State: "Estado",

    AccessDenied_Title: "Acceso denegado",
    AccessDenied_Message: "No tiene ningún rol con la función necesaria para usar esta opción.",
    AccessDenied_Function: "Función requerida",
    AccessDenied_BackHome: "Volver al inicio",

    Role_Index_Breadcrumb: "Roles",
    Role_Index_Create: "Nuevo rol",
    Role_Col_Name: "Rol",
    Role_Col_Code: "Código",
    Role_Col_Functions: "Funciones",
    Role_Col_Users: "Usuarios",
    Role_Badge_System: "Sistema",
    Role_Badge_AllFunctions: "Todas las funciones",
    Role_Page_CreateTitle: "Crear rol",
    Role_Page_EditTitle: "Editar rol",
    Role_Lbl_NameRequired: "Nombre *",
    Role_Lbl_Code: "Código",
    Role_Lbl_Description: "Descripción",
    Role_Lbl_Active: "Activo",
    Role_Lbl_ActiveCheckbox: "Rol activo",
    Role_Lbl_AllFunctions: "Todas las funciones",
    Role_Lbl_AllFunctionsCheckbox: "Concede todas las funciones, también las que se creen después",
    Role_Lbl_Functions: "Funciones del rol",
    Role_Lbl_Users: "Usuarios con este rol",
    Role_Lbl_NoFunctions: "No hay funciones creadas.",
    Role_Lbl_NoUsers: "No hay usuarios.",
    Role_Lbl_SystemLocked: "Rol de sistema: siempre activo y con todas las funciones.",
    Role_Ph_Name: "Ej.: Administrativo, Gestión de empresas",
    Role_Ph_Code: "Ej.: ADMINISTRATIVE",
    Role_Ph_Description: "Qué puede hacer quien tenga este rol",
    Role_List_OpenTooltip: "Ver rol",
    Role_List_EditTooltip: "Editar rol",
    Role_List_DeleteTooltip: "Eliminar rol",
    Role_List_DeleteLockedTooltip: "Los roles de sistema no se pueden eliminar.",
    Role_Js_ConfirmDelete: "¿Eliminar este rol? Los usuarios dejarán de tenerlo (borrado lógico).",
    Role_Toast_CreateTitle: "Crear rol",
    Role_Toast_Created: "Rol \"{0}\" creado correctamente.",
    Role_Toast_EditTitle: "Editar rol",
    Role_Toast_Updated: "Rol \"{0}\" actualizado correctamente.",
    Role_Toast_Deleted: "Rol \"{0}\" eliminado correctamente.",
    Role_Val_NameRequired: "El nombre del rol es obligatorio.",
    Role_Val_NameTooLong: "El nombre no puede superar 500 caracteres.",
    Role_Val_CodeTooLong: "El código no puede superar 100 caracteres.",
    Role_Val_DuplicateName: "Ya existe un rol con ese nombre.",
    Role_Val_DuplicateCode: "Ya existe un rol con ese código.",
    Role_Err_NotFound: "Rol no encontrado.",
    Role_Err_CannotDeleteSystem: "Los roles de sistema no se pueden eliminar.",
    Role_AssignFunctions_Title: "Asignar funciones",
    Role_AssignFunctions_Tooltip: "Asignar funciones",
    Role_AssignFunctions_Assigned: "Funciones asignadas",
    Role_AssignFunctions_Available: "Funciones disponibles",
    Role_AssignFunctions_Hint: "Arrastra una función de una tabla a la otra para asignarla o quitarla. Se guarda en el momento.",
    Role_AssignFunctions_DropHere: "Suelta aquí la función",
    Role_AssignFunctions_BtnAssign: "Asignar",
    Role_AssignFunctions_BtnRemove: "Quitar",
    Role_AssignFunctions_EmptyAssigned: "El rol no tiene funciones asignadas.",
    Role_AssignFunctions_EmptyAvailable: "No quedan funciones por asignar.",
    Role_AssignFunctions_Added: "Función \"{0}\" asignada al rol {1}.",
    Role_AssignFunctions_Removed: "Función \"{0}\" quitada del rol {1}.",
    Role_AssignFunctions_FunctionNotFound: "Función no encontrada o inactiva.",
    Role_AssignFunctions_SystemLocked: "Rol de sistema: las funciones no se pueden cambiar.",
    Role_AssignFunctions_SelfLockout: "No puedes quitar esta función: perderías el acceso a la gestión de roles.",
    Role_AssignFunctions_AllFunctionsHint: "Este rol tiene «todas las funciones». La lista de abajo se usa si desmarcas esa opción.",
    Role_AssignFunctions_FromEditHint: "Las funciones se asignan arrastrando, no en este formulario.",

    Function_Index_Breadcrumb: "Funciones",
    Function_Index_Create: "Nueva función",
    Function_Col_Name: "Función",
    Function_Col_Code: "Comando",
    Function_Col_Module: "Módulo",
    Function_Col_Roles: "Roles",
    Function_State_Protected: "Protegida",
    Function_State_Free: "Libre (inactiva)",
    Function_Page_CreateTitle: "Crear función",
    Function_Page_EditTitle: "Editar función",
    Function_Lbl_NameRequired: "Nombre *",
    Function_Lbl_CodeRequired: "Comando *",
    Function_Lbl_Module: "Módulo",
    Function_Lbl_Description: "Descripción",
    Function_Lbl_Active: "Activa",
    Function_Lbl_ActiveCheckbox: "Proteger el comando (solo lo usan los roles que tengan esta función)",
    Function_Lbl_Roles: "Roles con esta función",
    Function_Lbl_NoRoles: "No hay roles creados.",
    Function_Ph_Name: "Ej.: Gestión de empresas, Dibujar polilínea",
    Function_Ph_Code: "Ej.: TSql_Company, Employee.Create_Employee, Tool.polyline",
    Function_Ph_Module: "Se rellena solo si se deja vacío",
    Function_Ph_Description: "Qué permite hacer",
    Function_List_OpenTooltip: "Ver función",
    Function_List_EditTooltip: "Editar función",
    Function_List_DeleteTooltip: "Eliminar función",
    Function_Js_ConfirmDelete: "¿Eliminar esta función? El comando quedará libre para todos (borrado lógico).",
    Function_Toast_CreateTitle: "Crear función",
    Function_Toast_Created: "Función \"{0}\" creada correctamente.",
    Function_Toast_EditTitle: "Editar función",
    Function_Toast_Updated: "Función \"{0}\" actualizada correctamente.",
    Function_Toast_Deleted: "Función \"{0}\" eliminada correctamente.",
    Function_Val_NameRequired: "El nombre de la función es obligatorio.",
    Function_Val_NameTooLong: "El nombre no puede superar 500 caracteres.",
    Function_Val_CodeRequired: "El comando es obligatorio.",
    Function_Val_CodeTooLong: "El comando no puede superar 200 caracteres.",
    Function_Val_CodeInvalid: "El comando solo admite letras, números, espacios y los signos . _ -",
    Function_Val_CodeExempt: "Ese comando no se puede proteger (entrar, salir o cambiar idioma).",
    Function_Val_DuplicateCode: "Ya existe una función para ese comando.",
    Function_Val_ModuleTooLong: "El módulo no puede superar 100 caracteres.",
    Function_Err_NotFound: "Función no encontrada.",

    Employee_Lbl_Roles: "Roles del usuario",
    Employee_Lbl_NoRoles: "No hay roles creados.",
    Employee_AssignRoles_Title: "Asignar roles",
    Employee_AssignRoles_Tooltip: "Asignar roles",
    Employee_AssignRoles_Saved: "Roles de \"{0}\" guardados correctamente.",
    Employee_AssignRoles_UserNotFound: "Usuario no encontrado.",
    Employee_UserRoles_Assigned: "Roles asignados",
    Employee_UserRoles_Available: "Roles disponibles",
    Employee_UserRoles_Hint: "Arrastra un rol de una tabla a la otra para asignarlo o quitarlo. Se guarda en el momento.",
    Employee_UserRoles_DropHere: "Suelta aquí el rol",
    Employee_UserRoles_BtnAssign: "Asignar",
    Employee_UserRoles_BtnRemove: "Quitar",
    Employee_UserRoles_EmptyAssigned: "El usuario no tiene roles asignados.",
    Employee_UserRoles_EmptyAvailable: "No quedan roles por asignar.",
    Employee_UserRoles_Added: "Rol \"{0}\" asignado a {1}.",
    Employee_UserRoles_Removed: "Rol \"{0}\" quitado a {1}.",
    Employee_UserRoles_RoleNotFound: "Rol no encontrado o inactivo.",
    Employee_UserRoles_SelfLockout: "No puedes quitarte este rol: perderías el acceso a la gestión de roles.",
    Employee_UserRoles_EditEmployee: "Ficha del empleado"
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Security: "Settings",
  State_Active: "Active",
  State_Inactive: "Inactive",
  Col_State: "State",

  AccessDenied_Title: "Access denied",
  AccessDenied_Message: "None of your roles has the function required to use this option.",
  AccessDenied_Function: "Required function",
  AccessDenied_BackHome: "Back to home",

  Role_Index_Breadcrumb: "Roles",
  Role_Index_Create: "New role",
  Role_Col_Name: "Role",
  Role_Col_Code: "Code",
  Role_Col_Functions: "Functions",
  Role_Col_Users: "Users",
  Role_Badge_System: "System",
  Role_Badge_AllFunctions: "All functions",
  Role_Page_CreateTitle: "Create role",
  Role_Page_EditTitle: "Edit role",
  Role_Lbl_NameRequired: "Name *",
  Role_Lbl_Code: "Code",
  Role_Lbl_Description: "Description",
  Role_Lbl_Active: "Active",
  Role_Lbl_ActiveCheckbox: "Active role",
  Role_Lbl_AllFunctions: "All functions",
  Role_Lbl_AllFunctionsCheckbox: "Grants every function, including those created later",
  Role_Lbl_Functions: "Role functions",
  Role_Lbl_Users: "Users with this role",
  Role_Lbl_NoFunctions: "No functions created yet.",
  Role_Lbl_NoUsers: "No users.",
  Role_Lbl_SystemLocked: "System role: always active and with every function.",
  Role_Ph_Name: "E.g.: Administrative, Company management",
  Role_Ph_Code: "E.g.: ADMINISTRATIVE",
  Role_Ph_Description: "What users with this role can do",
  Role_List_OpenTooltip: "Open role",
  Role_List_EditTooltip: "Edit role",
  Role_List_DeleteTooltip: "Delete role",
  Role_List_DeleteLockedTooltip: "System roles cannot be deleted.",
  Role_Js_ConfirmDelete: "Delete this role? Users will lose it (soft delete).",
  Role_Toast_CreateTitle: "Create role",
  Role_Toast_Created: "Role \"{0}\" created successfully.",
  Role_Toast_EditTitle: "Edit role",
  Role_Toast_Updated: "Role \"{0}\" updated successfully.",
  Role_Toast_Deleted: "Role \"{0}\" deleted successfully.",
  Role_Val_NameRequired: "Role name is required.",
  Role_Val_NameTooLong: "Name cannot exceed 500 characters.",
  Role_Val_CodeTooLong: "Code cannot exceed 100 characters.",
  Role_Val_DuplicateName: "A role with this name already exists.",
  Role_Val_DuplicateCode: "A role with this code already exists.",
  Role_Err_NotFound: "Role not found.",
  Role_Err_CannotDeleteSystem: "System roles cannot be deleted.",
  Role_AssignFunctions_Title: "Assign functions",
  Role_AssignFunctions_Tooltip: "Assign functions",
  Role_AssignFunctions_Assigned: "Assigned functions",
  Role_AssignFunctions_Available: "Available functions",
  Role_AssignFunctions_Hint: "Drag a function from one table to the other to assign or remove it. Changes are saved immediately.",
  Role_AssignFunctions_DropHere: "Drop the function here",
  Role_AssignFunctions_BtnAssign: "Assign",
  Role_AssignFunctions_BtnRemove: "Remove",
  Role_AssignFunctions_EmptyAssigned: "The role has no functions assigned.",
  Role_AssignFunctions_EmptyAvailable: "No functions left to assign.",
  Role_AssignFunctions_Added: "Function \"{0}\" assigned to role {1}.",
  Role_AssignFunctions_Removed: "Function \"{0}\" removed from role {1}.",
  Role_AssignFunctions_FunctionNotFound: "Function not found or inactive.",
  Role_AssignFunctions_SystemLocked: "System role: functions cannot be changed.",
  Role_AssignFunctions_SelfLockout: "You cannot remove this function: you would lose access to role management.",
  Role_AssignFunctions_AllFunctionsHint: "This role has “all functions”. The list below is used if you turn that option off.",
  Role_AssignFunctions_FromEditHint: "Functions are assigned by dragging, not in this form.",

  Function_Index_Breadcrumb: "Functions",
  Function_Index_Create: "New function",
  Function_Col_Name: "Function",
  Function_Col_Code: "Command",
  Function_Col_Module: "Module",
  Function_Col_Roles: "Roles",
  Function_State_Protected: "Protected",
  Function_State_Free: "Free (inactive)",
  Function_Page_CreateTitle: "Create function",
  Function_Page_EditTitle: "Edit function",
  Function_Lbl_NameRequired: "Name *",
  Function_Lbl_CodeRequired: "Command *",
  Function_Lbl_Module: "Module",
  Function_Lbl_Description: "Description",
  Function_Lbl_Active: "Active",
  Function_Lbl_ActiveCheckbox: "Protect the command (only roles with this function can use it)",
  Function_Lbl_Roles: "Roles with this function",
  Function_Lbl_NoRoles: "No roles created yet.",
  Function_Ph_Name: "E.g.: Company management, Draw polyline",
  Function_Ph_Code: "E.g.: TSql_Company, Employee.Create_Employee, Tool.polyline",
  Function_Ph_Module: "Filled automatically when empty",
  Function_Ph_Description: "What it allows",
  Function_List_OpenTooltip: "Open function",
  Function_List_EditTooltip: "Edit function",
  Function_List_DeleteTooltip: "Delete function",
  Function_Js_ConfirmDelete: "Delete this function? The command becomes free for everyone (soft delete).",
  Function_Toast_CreateTitle: "Create function",
  Function_Toast_Created: "Function \"{0}\" created successfully.",
  Function_Toast_EditTitle: "Edit function",
  Function_Toast_Updated: "Function \"{0}\" updated successfully.",
  Function_Toast_Deleted: "Function \"{0}\" deleted successfully.",
  Function_Val_NameRequired: "Function name is required.",
  Function_Val_NameTooLong: "Name cannot exceed 500 characters.",
  Function_Val_CodeRequired: "Command is required.",
  Function_Val_CodeTooLong: "Command cannot exceed 200 characters.",
  Function_Val_CodeInvalid: "Command only allows letters, digits, spaces and . _ -",
  Function_Val_CodeExempt: "This command cannot be protected (log in, log out or language).",
  Function_Val_DuplicateCode: "A function for this command already exists.",
  Function_Val_ModuleTooLong: "Module cannot exceed 100 characters.",
  Function_Err_NotFound: "Function not found.",

  Employee_Lbl_Roles: "User roles",
  Employee_Lbl_NoRoles: "No roles created yet.",
  Employee_AssignRoles_Title: "Assign roles",
  Employee_AssignRoles_Tooltip: "Assign roles",
  Employee_AssignRoles_Saved: "Roles for \"{0}\" saved successfully.",
  Employee_AssignRoles_UserNotFound: "User not found.",
  Employee_UserRoles_Assigned: "Assigned roles",
  Employee_UserRoles_Available: "Available roles",
  Employee_UserRoles_Hint: "Drag a role from one table to the other to assign or remove it. Changes are saved immediately.",
  Employee_UserRoles_DropHere: "Drop the role here",
  Employee_UserRoles_BtnAssign: "Assign",
  Employee_UserRoles_BtnRemove: "Remove",
  Employee_UserRoles_EmptyAssigned: "The user has no roles assigned.",
  Employee_UserRoles_EmptyAvailable: "No roles left to assign.",
  Employee_UserRoles_Added: "Role \"{0}\" assigned to {1}.",
  Employee_UserRoles_Removed: "Role \"{0}\" removed from {1}.",
  Employee_UserRoles_RoleNotFound: "Role not found or inactive.",
  Employee_UserRoles_SelfLockout: "You cannot remove this role from yourself: you would lose access to role management.",
  Employee_UserRoles_EditEmployee: "Employee record"
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
// Auto-generated by _gen_security_resources.js — Security (roles y funciones) UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>Security module (TSql_Roles_V2 / TSql_Functions): localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class Security
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Security", typeof(Security).Assembly, "Security"));

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
fs.writeFileSync(path.join(dir, "Security.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Security.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Security.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Security.*");
