const fs = require("fs");
const path = require("path");

/*
 * Generador de recursos i18n del modulo DevelopIssue (Develop › US y Bugs:
 * TSql_DevelopIssue + TSql_DevelopIssueDetail).
 * Patron alineado con Security (DbBackedResourceManager).
 */

const es = {
  Common_Home: "Inicio",
  Common_Develop: "Develop",
  Index_Breadcrumb: "US y Bugs",
  Index_Create: "Nueva US / Bug",
  Page_CreateTitle: "Nueva US / Bug",
  Page_EditTitle: "Editar US / Bug",
  Navbar_Button: "US y Bugs",
  Navbar_New: "Nuevo",
  Navbar_List: "Ver lista",
  Popup_Title: "Nueva US / Bug",
  Popup_Capture: "Capturar pantalla",
  Popup_Attach: "Adjuntar",
  Popup_AttachWall: ".tnwall",
  Popup_AttachWallTitle: "Adjuntar archivos de muros (.tnwall)",
  Popup_CaptureHint: "Clic en una esquina y clic en la esquina opuesta. Esc cancela.",
  Popup_CaptureUnsupported: "Este navegador no permite capturar la pantalla. Adjunta la imagen o pégala con Ctrl+V.",
  Popup_CaptureDenied: "No se ha concedido permiso para capturar la pantalla.",
  Popup_SaveError: "No se ha podido guardar.",
  Popup_Open: "Abrir",
  Popup_Uploading: "Subiendo archivo {0} de {1}…",
  Col_Name: "Nombre",
  Col_Type: "Tipo",
  Col_Urgency: "Urgencia",
  Col_State: "Estado",
  Col_Date: "Fecha de creación",
  Col_Details: "Detalles",
  Lbl_NameRequired: "Nombre *",
  Lbl_Type: "Tipo",
  Lbl_Urgency: "Urgencia",
  Lbl_State: "Estado",
  Lbl_Date: "Fecha de creación",
  Lbl_Observation: "Observaciones",
  Lbl_Captures: "Capturas de pantalla",
  Lbl_Walls: "Archivos de muros (.tnwall)",
  Ph_Name: "Nombre de la US o del bug",
  Ph_Observation: "Observaciones",
  Ph_Paste: "Pega aquí una captura (Ctrl+V)",
  Type_US: "US",
  Type_BUG: "Bug",
  Urgency_LOW: "Bajo",
  Urgency_MEDIUM: "Medio",
  Urgency_URGENT: "Urgente",
  State_OPEN: "Abierto",
  State_IN_PROGRESS: "En proceso",
  State_CLOSED: "Cerrado",
  Details_Title: "Detalles",
  Details_Empty: "Sin detalles todavía.",
  Details_AddTitle: "Añadir detalle",
  Details_By: "Por {0} el {1}",
  Btn_AddDetail: "Añadir detalle",
  Btn_RemoveFile: "Quitar",
  Btn_EditImage: "Editar imagen",
  Editor_Title: "Editar imagen",
  Editor_Draw: "Dibujar",
  Editor_Bubble: "Bocadillo",
  Editor_Color: "Color",
  Editor_Italic: "Itálica",
  Editor_Shadow: "Sombreado",
  Editor_Undo: "Deshacer",
  Editor_BubbleHint: "Bocadillo: clic en lo que quieres señalar y arrastra hasta donde irá el texto. Intro confirma, Mayús+Intro salto de línea.",
  Editor_TextPlaceholder: "Texto",
  Toast_ImageSaved: "Imagen guardada.",
  Assess_Title: "Valoración",
  Assess_Empty: "Todavía no hay ninguna valoración.",
  Assess_Button: "Valorar con IA",
  Assess_Working: "Valorando…",
  Assess_Done: "Valoración guardada.",
  Assess_Error: "La IA no ha podido valorar. Inténtalo de nuevo.",
  Assess_NoKey: "La clave de OpenAI no está configurada en este servidor.",
  Assess_Hours: "{0} h estimadas",
  Assess_Area: "Zona afectada",
  Assess_Note: "La valoración de la IA es orientativa: no conoce el código.",
  Size_SMALL: "Pequeño",
  Size_MEDIUM: "Mediano",
  Size_LARGE: "Grande",
  Source_AI: "IA de la intranet",
  Source_AGENT: "Agente de programación",
  List_OpenTooltip: "Ver la US o el bug",
  List_EditTooltip: "Editar",
  List_DeleteTooltip: "Eliminar",
  Js_ConfirmDelete: "¿Eliminar esta US o bug?",
  Js_ConfirmDeleteDetail: "¿Eliminar este detalle?",
  Err_NameRequired: "El nombre es obligatorio.",
  Err_InvalidValue: "Valor no válido.",
  Err_NotFound: "No se encuentra la US o el bug.",
  Err_DetailEmpty: "Escribe una observación o añade una captura.",
  Err_ImageFormat: "Formato de captura no admitido (png, jpg, gif, webp).",
  Err_ImageSize: "La captura supera 10 MB.",
  Err_WallFormat: "Solo se admiten archivos .tnwall exportados desde el visor.",
  Err_WallSize: "El archivo .tnwall supera 20 MB.",
  Toast_Title: "US y Bugs",
  Toast_Created: "«{0}» creado.",
  Toast_Saved: "«{0}» guardado.",
  Toast_DetailAdded: "Detalle añadido.",
  Toast_Deleted: "Eliminado.",
  Msg_NoTable: "Faltan las tablas TSql_DevelopIssue y TSql_DevelopIssueDetail: ejecuta los scripts 2026-11-23_create_TSql_DevelopIssue*.sql."
};

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Develop: "Develop",
  Index_Breadcrumb: "US and Bugs",
  Index_Create: "New US / Bug",
  Page_CreateTitle: "New US / Bug",
  Page_EditTitle: "Edit US / Bug",
  Navbar_Button: "US and Bugs",
  Navbar_New: "New",
  Navbar_List: "Show list",
  Popup_Title: "New US / Bug",
  Popup_Capture: "Capture screen",
  Popup_Attach: "Attach",
  Popup_AttachWall: ".tnwall",
  Popup_AttachWallTitle: "Attach wall files (.tnwall)",
  Popup_CaptureHint: "Click one corner, then click the opposite corner. Esc cancels.",
  Popup_CaptureUnsupported: "This browser cannot capture the screen. Attach the image or paste it with Ctrl+V.",
  Popup_CaptureDenied: "Screen capture permission was not granted.",
  Popup_SaveError: "Could not save.",
  Popup_Open: "Open",
  Popup_Uploading: "Uploading file {0} of {1}…",
  Col_Name: "Name",
  Col_Type: "Type",
  Col_Urgency: "Urgency",
  Col_State: "State",
  Col_Date: "Created on",
  Col_Details: "Details",
  Lbl_NameRequired: "Name *",
  Lbl_Type: "Type",
  Lbl_Urgency: "Urgency",
  Lbl_State: "State",
  Lbl_Date: "Created on",
  Lbl_Observation: "Notes",
  Lbl_Captures: "Screenshots",
  Lbl_Walls: "Wall files (.tnwall)",
  Ph_Name: "US or bug name",
  Ph_Observation: "Notes",
  Ph_Paste: "Paste a screenshot here (Ctrl+V)",
  Type_US: "US",
  Type_BUG: "Bug",
  Urgency_LOW: "Low",
  Urgency_MEDIUM: "Medium",
  Urgency_URGENT: "Urgent",
  State_OPEN: "Open",
  State_IN_PROGRESS: "In progress",
  State_CLOSED: "Closed",
  Details_Title: "Details",
  Details_Empty: "No details yet.",
  Details_AddTitle: "Add detail",
  Details_By: "By {0} on {1}",
  Btn_AddDetail: "Add detail",
  Btn_RemoveFile: "Remove",
  Btn_EditImage: "Edit image",
  Editor_Title: "Edit image",
  Editor_Draw: "Draw",
  Editor_Bubble: "Speech bubble",
  Editor_Color: "Color",
  Editor_Italic: "Italic",
  Editor_Shadow: "Shadow",
  Editor_Undo: "Undo",
  Editor_BubbleHint: "Bubble: click what you want to point at and drag to where the text goes. Enter confirms, Shift+Enter adds a line break.",
  Editor_TextPlaceholder: "Text",
  Toast_ImageSaved: "Image saved.",
  Assess_Title: "Assessment",
  Assess_Empty: "There is no assessment yet.",
  Assess_Button: "Assess with AI",
  Assess_Working: "Assessing…",
  Assess_Done: "Assessment saved.",
  Assess_Error: "The AI could not assess it. Try again.",
  Assess_NoKey: "The OpenAI key is not configured on this server.",
  Assess_Hours: "{0} h estimated",
  Assess_Area: "Affected area",
  Assess_Note: "The AI assessment is indicative: it does not know the code.",
  Size_SMALL: "Small",
  Size_MEDIUM: "Medium",
  Size_LARGE: "Large",
  Source_AI: "Intranet AI",
  Source_AGENT: "Coding agent",
  List_OpenTooltip: "Open the US or bug",
  List_EditTooltip: "Edit",
  List_DeleteTooltip: "Delete",
  Js_ConfirmDelete: "Delete this US or bug?",
  Js_ConfirmDeleteDetail: "Delete this detail?",
  Err_NameRequired: "Name is required.",
  Err_InvalidValue: "Invalid value.",
  Err_NotFound: "US or bug not found.",
  Err_DetailEmpty: "Write a note or add a screenshot.",
  Err_ImageFormat: "Unsupported screenshot format (png, jpg, gif, webp).",
  Err_ImageSize: "The screenshot is larger than 10 MB.",
  Err_WallFormat: "Only .tnwall files exported from the viewer are accepted.",
  Err_WallSize: "The .tnwall file is larger than 20 MB.",
  Toast_Title: "US and Bugs",
  Toast_Created: "“{0}” created.",
  Toast_Saved: "“{0}” saved.",
  Toast_DetailAdded: "Detail added.",
  Toast_Deleted: "Deleted.",
  Msg_NoTable: "Tables TSql_DevelopIssue and TSql_DevelopIssueDetail are missing: run the 2026-11-23_create_TSql_DevelopIssue*.sql scripts."
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
// Auto-generated by _gen_developissue_resources.js — DevelopIssue (US y Bugs) UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>DevelopIssue module (TSql_DevelopIssue): localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class DevelopIssue
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.DevelopIssue", typeof(DevelopIssue).Assembly, "DevelopIssue"));

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
fs.writeFileSync(path.join(dir, "DevelopIssue.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "DevelopIssue.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "DevelopIssue.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/DevelopIssue.*");
