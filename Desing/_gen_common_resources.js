const fs = require("fs");
const path = require("path");

/* -----------------------------------------------------------------------------
 * Module: Common (UI global, transversal a todos los modulos)
 * Generates: Resources/Common.resx, Common.en.resx, Common.Designer.cs
 *
 * Scope (Bloque A de internacionalizacion):
 *   - Layout principal Materio (titulo de fallback, etiqueta entorno).
 *   - Footer.
 *   - Navbar (hamburguesa, badge de entorno, dropdown usuario).
 *   - Sidebar (cabeceras de seccion + items de menu).
 *   - Selector de idioma (estado bloqueado / fallback / aria).
 *   - Pagina Error y Lockout.
 *   - Cuentas: Login, Register, ForgotPassword, ResetPassword (labels + mensajes
 *     mostrados al usuario por AccountController).
 *   - Defaults DataTables (textos del menu colectivo: filas/exportar/visibles;
 *     se exponen tambien al cliente via window.tandemCommonDt en _LayoutMaterio).
 *   - Google Places address block (label de busqueda, campos genericos, mapa).
 *   - Botones genericos / paginacion (no machacar a modulos especificos: solo
 *     usar Common.Btn_* en pantallas neutras).
 *
 * IMPORTANT
 *   - El indice unico actual de TSql_UiTranslation es (TextResourceKey,
 *     LinkLanguage), sin TextModule. Para evitar colisiones con modulos ya
 *     migrados (Company.Save, ClientV2.Save, etc.) este modulo NO usa claves
 *     genericas tipo "Save" o "Cancel" pelados; todas estan prefijadas
 *     (Btn_Save, Btn_Cancel, ...). El controlador UiTranslationController los
 *     exportara como TextModule = "Common".
 *   - Las claves del menu/navbar que ya existen en BD via @Html.Ui (Menu.*,
 *     Navbar.*) NO se duplican aqui: siguen viviendo en TSql_UiTranslation
 *     con TextModule = "Common" y se siguen leyendo con @Html.Ui.
 * --------------------------------------------------------------------------- */

const es = Object.fromEntries(
  Object.entries({
    /* ===== Layout / branding (textos cuando no hay plantilla configurada) ===== */
    App_BrandFallback: "T Desing.net",
    App_EnvironmentLabel: "Entorno Develop",

    /* ===== Footer ===== */
    Footer_LastReleaseLabel: "Última publicación:",

    /* ===== Aria / accesibilidad ===== */
    Aria_TogglePassword: "Mostrar u ocultar contraseña",
    Aria_LanguageSwitcher: "Selector de idioma",
    Aria_CloseModal: "Cerrar",

    /* ===== Selector de idioma ===== */
    LanguageSwitcher_LockedSuffix: "(idioma de la empresa)",
    LanguageSwitcher_LockedBadge: "Empresa",

    /* ===== Pagina Error ===== */
    Error_Title: "Error.",
    Error_Subtitle: "Se ha producido un error al procesar la solicitud.",

    /* ===== Pagina Lockout ===== */
    Lockout_PageTitle: "Cuenta bloqueada",
    Lockout_Title: "Cuenta bloqueada.",
    Lockout_Subtitle: "Esta cuenta ha sido bloqueada temporalmente. Inténtelo de nuevo más tarde.",

    /* ===== Account / Login ===== */
    Login_PageTitle: "Iniciar sesión",
    Login_Welcome: "Bienvenido a {0}",
    Login_Subtitle: "Inicia sesión para acceder a tu espacio de trabajo",
    Login_Lbl_Email: "Email",
    Login_Lbl_Password: "Contraseña",
    Login_Lbl_RememberMe: "Recordarme",
    Login_Link_ForgotPassword: "¿Olvidé mi contraseña?",
    Login_Btn_SignIn: "Entrar",

    /* ===== Account / Register ===== */
    Register_PageTitle: "Crear cuenta",
    Register_BreadcrumbHome: "Inicio",
    Register_BreadcrumbEmployees: "Empleados",
    Register_PageHeading: "Crear nuevo usuario",
    Register_CardTitle: "Datos del nuevo usuario",
    Register_Lbl_Email: "Email",
    Register_Lbl_Password: "Contraseña",
    Register_Lbl_ConfirmPassword: "Confirmar contraseña",
    Register_Btn_Cancel: "Cancelar",
    Register_Btn_Create: "Crear usuario",

    /* ===== Account / Forgot password ===== */
    Forgot_PageTitle: "Recuperar contraseña",
    Forgot_Heading: "Recuperar contraseña",
    Forgot_Subtitle: "Introduce tu email y te enviaremos un enlace para restablecer tu contraseña.",
    Forgot_Lbl_Email: "Email",
    Forgot_Btn_Send: "Enviar enlace",
    Forgot_Link_BackToLogin: "Volver a iniciar sesión",
    Forgot_ConfirmationTitle: "Solicitud enviada",
    Forgot_ConfirmationMessage: "Por favor, revisa tu correo para restablecer la contraseña.",

    /* ===== Account / Reset password ===== */
    Reset_PageTitle: "Restablecer contraseña",
    Reset_Heading: "Restablecer contraseña",
    Reset_Subtitle: "Introduce tu nueva contraseña para volver a acceder a {0}.",
    Reset_Lbl_Email: "Email",
    Reset_Lbl_NewPassword: "Nueva contraseña",
    Reset_Lbl_ConfirmPassword: "Confirmar contraseña",
    Reset_Btn_Submit: "Cambiar contraseña",
    Reset_Link_BackToLogin: "Volver a iniciar sesión",
    Reset_ConfirmationTitle: "Contraseña restablecida",
    Reset_ConfirmationLead: "Tu contraseña se ha restablecido correctamente.",
    Reset_ConfirmationLink: "Pulsa aquí para iniciar sesión",

    /* ===== Account / Confirm email + External login failure ===== */
    ConfirmEmail_Title: "Confirmación de email",
    ConfirmEmail_Lead: "Gracias por confirmar tu email.",
    ConfirmEmail_Link: "Pulsa aquí para iniciar sesión",
    ExternalLogin_FailureTitle: "Error de inicio de sesión",
    ExternalLogin_FailureSubtitle: "No se pudo iniciar sesión con el proveedor externo.",

    /* ===== Account / mensajes mostrados al usuario por AccountController ===== */
    Account_Err_LoginRequired: "Debes iniciar sesión para acceder a esa página.",
    Account_Err_EmailNotConfirmed:
      "Tienes que confirmar el email; la confirmación ha sido enviada.",
    Account_Err_UserNotRegistered: "El usuario no está registrado.",
    Account_Err_InvalidCode: "Código no válido.",
    Account_EmailConfirmation_Subject: "Por favor, valida tu cuenta",

    /* ===== Validaciones de modelo (AccountViewModels) ===== */
    Val_EmailRequired: "El email es obligatorio.",
    Val_EmailInvalid: "El formato del email no es válido.",
    Val_PasswordRequired: "La contraseña es obligatoria.",
    Val_PasswordTooShort:
      "El campo {0} debe tener al menos {2} caracteres.",
    Val_PasswordsDoNotMatch:
      "La contraseña y la confirmación no coinciden.",
    Val_CodeRequired: "El código es obligatorio.",

    /* ===== Display names para [Display] ===== */
    Display_Email: "Email",
    Display_Password: "Contraseña",
    Display_ConfirmPassword: "Confirmar contraseña",
    Display_RememberMe: "¿Recordarme?",
    Display_RememberBrowser: "¿Recordar este navegador?",
    Display_UserName: "Nombre de usuario",
    Display_Code: "Código",

    /* ===== Botones genericos (uso en pantallas neutras) ===== */
    Btn_Save: "Guardar",
    Btn_SaveChanges: "Guardar cambios",
    Btn_Cancel: "Cancelar",
    Btn_Back: "Volver",
    Btn_Create: "Crear",
    Btn_Edit: "Editar",
    Btn_Delete: "Eliminar",
    Btn_Details: "Detalles",
    Btn_Search: "Buscar",
    Btn_Close: "Cerrar",
    Btn_Send: "Enviar",
    Btn_Confirm: "Confirmar",

    /* ===== DataTables (i18n inyectado en window.tandemCommonDt) ===== */
    Dt_Section_Records: "Registros",
    Dt_Section_Export: "Exportar",
    Dt_Section_ColumnsVisible: "Columnas visibles",
    Dt_MenuAria: "Opciones del listado",
    Dt_Rows_N: "filas",
    Dt_Rows_All: "Todas",
    Dt_Btn_Print: "Imprimir",
    Dt_Btn_Copy: "Copiar",
    Dt_Btn_Pdf: "PDF",
    Dt_Btn_Csv: "CSV",
    Dt_Btn_Excel: "Excel",
    Dt_Btn_PageLength: "Filas por página",
    Dt_Btn_ColVis: "Visibilidad",

    /* ===== Google Places address block ===== */
    Google_Title: "Dirección",
    Google_Lbl_Search: "Buscar dirección",
    Google_Ph_Search: "Escriba calle, localidad o código postal…",
    Google_Lbl_FormattedAddress: "Dirección formateada",
    Google_Lbl_StreetNumber: "Nº",
    Google_Lbl_Route: "Vía",
    Google_Lbl_Subpremise: "Piso / puerta",
    Google_Lbl_Locality: "Localidad",
    Google_Lbl_AdminArea1: "Provincia",
    Google_Lbl_PostalCode: "CP",
    Google_Lbl_CountryCode: "País",
    Google_Lbl_Map: "Ubicación en mapa",
    Google_Aria_MapPreview: "Vista previa del mapa",
    Google_Msg_NoAddress: "Seleccione una dirección para ver el mapa",
    Google_Err_ApiLoadFailed:
      "No se pudo cargar Google Maps. Compruebe la clave en GoogleMaps:ApiKey (Web.config) y la configuración en Google Cloud Console.",

    /* ===== Home / dashboard ===== */
    Home_PageHeading: "Panel",
    Action_OpenList: "Ver listado completo",
  })
);

const en = Object.assign({}, es, {
  /* Layout / branding */
  App_BrandFallback: "T Desing.net",
  App_EnvironmentLabel: "Develop environment",

  /* Footer */
  Footer_LastReleaseLabel: "Last release:",

  /* Aria */
  Aria_TogglePassword: "Show or hide password",
  Aria_LanguageSwitcher: "Language switcher",
  Aria_CloseModal: "Close",

  /* Language switcher */
  LanguageSwitcher_LockedSuffix: "(company language)",
  LanguageSwitcher_LockedBadge: "Company",

  /* Error */
  Error_Title: "Error.",
  Error_Subtitle: "An error occurred while processing your request.",

  /* Lockout */
  Lockout_PageTitle: "Locked out",
  Lockout_Title: "Locked out.",
  Lockout_Subtitle: "This account has been locked out, please try again later.",

  /* Login */
  Login_PageTitle: "Sign in",
  Login_Welcome: "Welcome to {0}",
  Login_Subtitle: "Sign in to access your workspace",
  Login_Lbl_Email: "Email",
  Login_Lbl_Password: "Password",
  Login_Lbl_RememberMe: "Remember me",
  Login_Link_ForgotPassword: "Forgot password?",
  Login_Btn_SignIn: "Sign in",

  /* Register */
  Register_PageTitle: "Create account",
  Register_BreadcrumbHome: "Home",
  Register_BreadcrumbEmployees: "Employees",
  Register_PageHeading: "Create new user",
  Register_CardTitle: "New user details",
  Register_Lbl_Email: "Email",
  Register_Lbl_Password: "Password",
  Register_Lbl_ConfirmPassword: "Confirm password",
  Register_Btn_Cancel: "Cancel",
  Register_Btn_Create: "Create user",

  /* Forgot */
  Forgot_PageTitle: "Forgot password",
  Forgot_Heading: "Forgot password",
  Forgot_Subtitle: "Enter your email and we will send you a link to reset your password.",
  Forgot_Lbl_Email: "Email",
  Forgot_Btn_Send: "Send link",
  Forgot_Link_BackToLogin: "Back to sign in",
  Forgot_ConfirmationTitle: "Request sent",
  Forgot_ConfirmationMessage: "Please check your email to reset your password.",

  /* Reset */
  Reset_PageTitle: "Reset password",
  Reset_Heading: "Reset password",
  Reset_Subtitle: "Enter your new password to access {0} again.",
  Reset_Lbl_Email: "Email",
  Reset_Lbl_NewPassword: "New password",
  Reset_Lbl_ConfirmPassword: "Confirm password",
  Reset_Btn_Submit: "Change password",
  Reset_Link_BackToLogin: "Back to sign in",
  Reset_ConfirmationTitle: "Password reset",
  Reset_ConfirmationLead: "Your password has been reset.",
  Reset_ConfirmationLink: "Click here to sign in",

  /* Confirm email / External login */
  ConfirmEmail_Title: "Email confirmation",
  ConfirmEmail_Lead: "Thank you for confirming your email.",
  ConfirmEmail_Link: "Click here to sign in",
  ExternalLogin_FailureTitle: "Sign-in error",
  ExternalLogin_FailureSubtitle: "Could not sign in with the external provider.",

  /* Account messages */
  Account_Err_LoginRequired: "You must sign in to access that page.",
  Account_Err_EmailNotConfirmed:
    "You must confirm your email. The confirmation has been sent.",
  Account_Err_UserNotRegistered: "The user is not registered.",
  Account_Err_InvalidCode: "Invalid code.",
  Account_EmailConfirmation_Subject: "Please verify your account",

  /* Validations */
  Val_EmailRequired: "Email is required.",
  Val_EmailInvalid: "Email format is invalid.",
  Val_PasswordRequired: "Password is required.",
  Val_PasswordTooShort: "The {0} must be at least {2} characters long.",
  Val_PasswordsDoNotMatch: "The password and confirmation password do not match.",
  Val_CodeRequired: "Code is required.",

  /* Display names */
  Display_Email: "Email",
  Display_Password: "Password",
  Display_ConfirmPassword: "Confirm password",
  Display_RememberMe: "Remember me?",
  Display_RememberBrowser: "Remember this browser?",
  Display_UserName: "User name",
  Display_Code: "Code",

  /* Generic buttons */
  Btn_Save: "Save",
  Btn_SaveChanges: "Save changes",
  Btn_Cancel: "Cancel",
  Btn_Back: "Back",
  Btn_Create: "Create",
  Btn_Edit: "Edit",
  Btn_Delete: "Delete",
  Btn_Details: "Details",
  Btn_Search: "Search",
  Btn_Close: "Close",
  Btn_Send: "Send",
  Btn_Confirm: "Confirm",

  /* DataTables */
  Dt_Section_Records: "Records",
  Dt_Section_Export: "Export",
  Dt_Section_ColumnsVisible: "Visible columns",
  Dt_MenuAria: "List options",
  Dt_Rows_N: "rows",
  Dt_Rows_All: "All",
  Dt_Btn_Print: "Print",
  Dt_Btn_Copy: "Copy",
  Dt_Btn_Pdf: "PDF",
  Dt_Btn_Csv: "CSV",
  Dt_Btn_Excel: "Excel",
  Dt_Btn_PageLength: "Rows per page",
  Dt_Btn_ColVis: "Visibility",

  /* Google Places */
  Google_Title: "Address",
  Google_Lbl_Search: "Search address",
  Google_Ph_Search: "Type street, city or postal code…",
  Google_Lbl_FormattedAddress: "Formatted address",
  Google_Lbl_StreetNumber: "No.",
  Google_Lbl_Route: "Street",
  Google_Lbl_Subpremise: "Floor / door",
  Google_Lbl_Locality: "City",
  Google_Lbl_AdminArea1: "Region",
  Google_Lbl_PostalCode: "ZIP",
  Google_Lbl_CountryCode: "Country",
  Google_Lbl_Map: "Map location",
  Google_Aria_MapPreview: "Map preview",
  Google_Msg_NoAddress: "Select an address to display the map",
  Google_Err_ApiLoadFailed:
    "Could not load Google Maps. Check the key at GoogleMaps:ApiKey (Web.config) and the Google Cloud Console settings.",

  /* Home dashboard */
  Home_PageHeading: "Dashboard",
  Action_OpenList: "Open full list",
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
// Auto-generated by _gen_common_resources.js — Common UI strings (${keys.length}
// entries). Cubre layout, footer, navbar/sidebar/switcher, Error, Lockout,
// cuentas (Login/Register/Forgot/Reset), defaults DataTables y Google Places.
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>Modulo Common: cadenas de UI globales (.resx + override BD vía DbBackedResourceManager).</summary>
    public class Common
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.Common", typeof(Common).Assembly, "Common"));

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
fs.writeFileSync(path.join(dir, "Common.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "Common.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "Common.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/Common.*");
