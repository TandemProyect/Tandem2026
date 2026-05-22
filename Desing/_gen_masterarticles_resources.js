const fs = require("fs");
const path = require("path");

const es = Object.fromEntries(
  Object.entries({
    Common_Home: "Inicio",
    Common_Intranet: "Intranet",
    Index_Breadcrumb: "Artículos",
    Index_CreateArticle: "Nuevo artículo",

    Col_AtenkoCode: "Código",
    Col_Description: "Descripción",
    Col_High: "Alto",
    Col_Width: "Ancho",
    Col_Long: "Largo",
    Col_Weight: "Peso",
    Col_Mts2: "M²",
    Col_Mts3: "M³",
    Col_BlockNumber: "Bloque DWG",
    Col_StlNumber: ".Stl",
    Col_Company: "Empresa",
    Col_System: "Sistema",
    Col_Plant3D: "Planta 3D",
    Col_VertElev3D: "Elev. vert. 3D",
    Col_HorzElev3D: "Elev. horiz. 3D",
    Col_PlantMockup: "Planta mock-up",
    Col_VertElevMockup: "Elev. vert. mock-up",
    Col_HorzElevMockup: "Elev. horiz. mock-up",
    Col_PlantStl: "Planta STL",
    Col_VertElevStl: "Elev. vert. STL",
    Col_HorzElevStl: "Elev. horiz. STL",
    Col_InsertMaster: "Ins. Master",
    Col_State: "Estado",
    Col_Actions: "Acciones",

    Dt_Rows_All: "Todas",
    Dt_Rows_N: "filas",
    Dt_MenuAria: "Opciones del listado",
    Dt_Section_Records: "Registros",
    Dt_Section_Export: "Exportar",
    Dt_Section_ColumnsVisible: "Columnas visibles",

    State_Active: "Activo",
    State_Inactive: "Inactivo",
    State_Yes: "Sí",
    State_No: "No",

    Js_ConfirmDeleteArticle: "¿Eliminar este artículo? Esta acción no se puede deshacer.",
    Js_ConfirmActivate: "¿Activar este artículo?",
    Js_ConfirmDeactivate: "¿Desactivar este artículo? Seguirá en la base de datos pero no se usará como activo.",

    ToastTitle_Articles: "Artículos",
    ToastTitle_CreateArticle: "Crear artículo",
    ToastTitle_EditArticle: "Editar artículo",
    ToastTitle_DeleteArticle: "Eliminar artículo",
    ToastMessage_ArticleCreated: "Artículo \"{0}\" creado correctamente.",
    ToastMessage_ArticleUpdated: "Artículo \"{0}\" actualizado correctamente.",
    ToastMessage_ArticleDeleted: "Artículo \"{0}\" eliminado correctamente.",
    ToastMessage_ArticleActivated: "Artículo \"{0}\" activado.",
    ToastMessage_ArticleDeactivated: "Artículo \"{0}\" desactivado.",
    ToastMessage_ArticleCreatedReviewAttachments:
      "Artículo creado. Revise los adjuntos en la edición.",
    ToastMessage_ArticleCreatedWithErrors:
      "Artículo creado. {0}",

    Val_CodeRequired: "El código es obligatorio.",
    Val_LabelRequired: "La descripción es obligatoria.",
    Val_SystemRequired: "Seleccione un sistema.",
    Val_SystemInvalid: "El sistema seleccionado no es válido.",
    Val_DuplicateCodeCreate:
      "Ya existe un artículo con este código en el mismo sistema.",
    Val_DuplicateCodeEdit:
      "Ya existe otro artículo con este código en el mismo sistema.",
    Val_BlockFileTooLarge:
      "Archivo demasiado grande (máx. 50 MB) para el bloque {0}.",
    Val_BlockFileExtensionRequired:
      "Extensión no válida en archivo para {0}.",
    Val_BlockFileExtensionInvalid:
      "Solo {0} para el bloque {1}.",
    Val_BlockFileEmpty: "Archivo vacío.",
    Val_BlockFileExtensionMissing:
      "El archivo no tiene extensión. Se permiten: {0}.",
    Val_BlockFileNotAllowedExtension:
      "Solo se permiten archivos {0} para este campo.",
    Val_BlockFileNameInvalid: "Nombre de archivo no válido.",
    Val_BlockFileSaveOverwriteFailed:
      "No se pudo sobrescribir el archivo existente: {0}",
    Val_BlockFileMaxSize:
      "El archivo supera el tamaño máximo permitido (50 MB).",

    Err_ArticleNotFound: "Artículo no encontrado.",
    Err_ArticleNotFoundOrDeleted: "El artículo no existe o ya fue eliminado.",
    Err_CannotDeleteHasReferences:
      "No se puede eliminar: el artículo está en uso (stock de reemplazo o listas temporales).",
    Err_DeleteFailed: "No se pudo eliminar el artículo. {0}",
    Err_ApsNotConfigured:
      "Autodesk APS no está configurado (ClientId y ClientSecret en Web.config).",
    Err_SlotKeyInvalid: "Parámetro slotKey no válido.",
    Err_NoDwgInBlock: "No hay archivo DWG en este bloque.",
    Err_DxfSiblingMissing:
      "No se encontró el DXF gemelo (mismo nombre y carpeta que el .dwg, extensión .dxf). Coloque el archivo en el servidor o genérelo al guardar el artículo.",

    Page_CreateTitle: "Crear artículo",
    Page_EditTitle: "Editar artículo",
    Page_DetailsTitle: "Detalles del artículo",
    Page_BackToList: "Volver a la lista",

    Lbl_CodeRequired: "Código *",
    Lbl_LabelRequired: "Descripción *",
    Lbl_SystemRequired: "Sistema *",
    Lbl_AtenkoCode: "Código Atenko",
    Lbl_Active: "Activo",
    Lbl_InsertInMaster: "Insertar en Master Articles",
    Lbl_High: "Alto",
    Lbl_Width: "Ancho",
    Lbl_Long: "Largo",
    Lbl_Weight: "Peso",
    Lbl_Mts2: "M²",
    Lbl_Mts3: "M³",
    Lbl_BlockNumber: "Bloque DWG",
    Lbl_StlNumber: ".Stl",
    Lbl_Color1: "Color 1",
    Lbl_Color2: "Color 2",
    Lbl_Color2Hint: "Máx. 10 caracteres al guardar.",
    Lbl_StlFilesSection: "Archivos modelo STL",
    Lbl_StlFilesHelp:
      "Adjunte un archivo .stl por cada vista (planta y elevaciones). Son opcionales; se guardan en Files/MasterArticles/blocks y se muestran en Detalles con el visor 3D.",
    Lbl_BlockDwgFilesSection: "Archivos de bloque — AutoCAD (solo .dwg)",
    Lbl_BlockDwgFilesHelp:
      "Estos .dwg son los archivos de trabajo (p. ej. con ZWCAD). Se guardan en la carpeta compartida Files/MasterArticles/blocks con el mismo nombre que adjunte. En la ficha, la vista previa usa el DXF gemelo (.dxf junto al .dwg).",
    Lbl_SystemPlaceholder: "-- Seleccionar --",

    BlockSlot_Plant3D: "Planta 3D",
    BlockSlot_VertElev3D: "Elevación vertical 3D",
    BlockSlot_HorzElev3D: "Elevación horizontal 3D",
    BlockSlot_PlantMockup: "Planta mock-up",
    BlockSlot_VertElevMockup: "Elevación vertical mock-up",
    BlockSlot_HorzElevMockup: "Elevación horizontal mock-up",
    BlockSlot_PlantStl: "Planta — archivo .stl",
    BlockSlot_VertElevStl: "Elevación vertical — archivo .stl",
    BlockSlot_HorzElevStl: "Elevación horizontal — archivo .stl",
    BlockSlot_OpenFile: "Abrir archivo",
    BlockSlot_SavedFile: "Archivo guardado:",
    BlockSlot_Remove: "Quitar",

    Btn_New: "Nuevo artículo",
    Btn_Save: "Guardar",
    Btn_SaveChanges: "Guardar cambios",
    Btn_Cancel: "Cancelar",
    Btn_BackToList: "Volver a la lista",
    Btn_EditArticle: "Editar artículo",
    Btn_CreateArticle: "Crear artículo",

    Details_Section_Identification: "Identificación y sistema",
    Details_Section_Measurements: "Medidas y referencias",
    Details_Field_Description: "Descripción",
    Details_Field_AtenkoCode: "Código Atenko",
    Details_Field_Company: "Empresa",
    Details_Field_System: "Sistema",
    Details_Field_Active: "Activo",
    Details_Field_InsertMaster: "Insertar en Master Articles",
    Details_Field_High: "Alto",
    Details_Field_Width: "Ancho",
    Details_Field_Long: "Largo",
    Details_Field_Weight: "Peso",
    Details_Field_Mts2: "M²",
    Details_Field_Mts3: "M³",
    Details_Field_BlockNumber: "Bloque DWG (texto)",
    Details_Field_StlNumber: ".Stl (texto)",
    Details_NoValue: "—",
    Details_YesValue: "Sí",
    Details_NoBoolValue: "No",

    List_LinkOpenTooltip: "Detalles del artículo",
    List_LinkEditTooltip: "Editar artículo",
    List_LinkDeleteTooltip: "Eliminar artículo",
    List_LinkDeleteLockedTooltip:
      "No se puede eliminar: el artículo está en uso (stock de reemplazo o listas temporales).",
    List_LinkActivateTooltip: "Activar artículo",
    List_LinkDeactivateTooltip: "Desactivar artículo",
    List_LinkAttachmentTooltip: "Abrir adjunto",

    StlPreview_GridHeaderEmpty: "",
    StlPreview_GridColPlant: "Planta",
    StlPreview_GridColVertElev: "Elevación vertical",
    StlPreview_GridColHorzElev: "Elevación horizontal",
    StlPreview_RowLabel3D: "3D",
    StlPreview_RowLabelMockup: "mock-up",
    StlPreview_RowLabelStl: "STL",
    StlPreview_NoFile: "Sin archivo.",
    StlPreview_LoadStlTooltip: "Ver en el visor 3D",
    StlPreview_NoStlInDiskTooltip:
      "No hay STL en disco para esta celda (mismo nombre base que el adjunto o archivo .stl en el slot).",
    StlPreview_NoStlInViewerTooltip: "Sin STL en el visor",
    StlPreview_StatusInitial:
      "Seleccione un bloque con STL disponible (botón naranja) para verlo aquí.",
    StlPreview_CameraLabel: "Cámara",
    StlPreview_CameraOrtho: "Ortogonal",
    StlPreview_CameraIso: "Isométrica",
    StlPreview_CameraOrthoTitle: "Vista ortogonal",
    StlPreview_CameraIsoTitle: "Vista isométrica",
    StlPreview_CameraModeAria: "Modo de cámara",
    StlPreview_FullscreenTitle: "Pantalla completa",
    StlPreview_SaveViewTitle: "Guardar vista",
    StlPreview_SaveViewAria: "Guardar vista",
    StlPreview_SaveViewSavedTitle: "Guardado",
    StlPreview_SaveViewSavedToast: "Vista guardada",
    StlPreview_EscapeCancelToolsToast: "Cancelado",
    StlPreview_GridToggleTitle: "Mostrar rejilla de fondo",
    StlPreview_GridToggleAria: "Rejilla de fondo",
    StlPreview_SkyToggleTitle: "Mostrar u ocultar cielo",
    StlPreview_SkyToggleAria: "Cielo STL",
    StlPreview_GroundShadowTitle: "Mostrar sombra en el suelo",
    StlPreview_GroundShadowAria: "Sombra en el suelo",
    StlPreview_DarkBgTitle: "Activar fondo negro",
    StlPreview_DarkBgAria: "Fondo negro",
    StlPreview_ClipToggleTitle: "Mostrar u ocultar cortes",
    StlPreview_ClipToggleAria: "Cortes STL",
    StlPreview_XyzAxesToggleTitle: "Mostrar u ocultar vectores de ejes X, Y y Z (verdes, con flecha)",
    StlPreview_XyzAxesAria: "Ejes XYZ",
    StlPreview_UcsRulersToggleTitle: "Mostrar u ocultar reglas de plano desde el anclaje (+X / +Z)",
    StlPreview_UcsRulersAria: "Reglas de plano",
    StlPreview_RulerAnchorGridPickTitle: "Colocar reglas en cruce de rejilla (500 mm)",
    StlPreview_RulerAnchorGridPickAria: "Modo rejilla: solo cruces activos del retículo menor (500 mm)",
    StlPreview_RulerAnchorGridPickModeToast:
      "Centre el cursor en un cruce de rejilla hasta el resaltado verde y haga clic",
    StlPreview_RulerAnchorGridSnapRequiredToast:
      "Aceérquese más al cruce (resaltado verde) antes de hacer clic — solo se permite snap de rejilla",
    StlPreview_RulerAnchorObjectPickTitle:
      "Punto de inserción desde la pieza STL (esquina inferior izquierda de la huella)",
    StlPreview_RulerAnchorObjectPickAria:
      "Sitúese sobre una pieza y haga clic: reglas en el origen tipo croquis CAD (esquina inferior izquierda en planta)",
    StlPreview_RulerAnchorObjectPickModeToast:
      "Pasar el ratón sobre el volumen STL (resaltado) y clic para las reglas en su punto de inserción",
    StlPreview_RulerAnchorObjectInsertionToast:
      "Punto de inserción aplicado a las reglas",
    StlPreview_RulerAnchorObjectPickMissToast:
      "Seleccione un volumen STL bajo el cursor",
    StlPreview_RulerAnchorGridCoordsHud: "X: {0} m  Z: {1} m",
    StlPreview_RulerAnchorGridIntersectionToast: "Reglas colocadas",
    StlPreview_ClipYTitle:
      "Corte por altura: 1000 = vista completa; al bajar el control, recorta desde arriba hacia abajo.",
    StlPreview_ClipYAria:
      "Corte horizontal (altura): 1000 vista completa; al bajar, recorta desde arriba hacia abajo",
    StlPreview_ClipXTitle:
      "Corte en planta: 1000 = vista completa; al bajar el valor, recorta desde la derecha hacia la izquierda.",
    StlPreview_ClipXAria:
      "Corte en planta (X): 1000 vista completa; al bajar, recorta desde la derecha hacia la izquierda",
    StlPreview_Desing2SideMenuAria: "Menú lateral del visor STL",
    StlPreview_Desing2TopToolbarAria: "Barra de herramientas del lienzo STL",
    StlPreview_Desing2TopToolbarCollapseTitle:
      "Ocultar barra hasta acercar de nuevo el puntero al borde superior centrado",
    StlPreview_Desing2TopToolbarPinTitle:
      "Fijar barra abierta (siempre expandida hasta desanclar)",
    StlPreview_Desing2TopToolbarUnpinTitle:
      "Desanclar barra y volver al modo expansión al pasar el puntero",
    StlPreview_LineToolInstructionFirst: "Clic para el primer punto (mueva el ratón para situar la vista previa).",
    StlPreview_LineToolInstructionSecond:
        "Segundo punto: clic en planta, o escriba la distancia y pulse Intro.",
    StlPreview_LineToolHudDistanceAria:
        "Longitud hasta el segundo punto (metros o milímetros). Intro termina el segmento; Escape cancela el modo línea.",
    StlPreview_LineToolHudDistancePlaceholder: "p. ej. 5 · 5,5 · 5000 mm",
    StlPreview_LineToolDistancePreviewApprox: "≈",
    StlPreview_LineToolDistanceInvalidToast:
        "Introduzca una distancia válida (p. ej. 5, 5,5 ó 5000 mm) o pulse clic para el segundo punto.",
    StlPreview_UserFloorLineDimEditAria:
      "Longitud del segmento de línea en planta en metros (o mm con sufijo). Intro para aplicar, Escape cancela.",
    StlPreview_UserFloorLineDimEditDeltaXAria:
      "ΔX desde el punto de anclaje de reglas hasta el segundo punto del segmento (P2) en metros, con signo (o mm). Intro aplicar; Escape cancela.",
    StlPreview_UserFloorLineDimEditDeltaZAria:
      "ΔZ desde el punto de anclaje de reglas hasta el segundo punto del segmento (P2) en metros, con signo (o mm). Intro aplicar; Escape cancela.",
    StlPreview_UserFloorLineDimReadoutDeltaXAria:
        "ΔX desde el cruce de reglas hasta el extremo libre del segmento (metros, con signo). Doble clic para editar.",
    StlPreview_UserFloorLineDimReadoutDeltaZAria:
        "ΔZ desde el cruce de reglas hasta el extremo libre del segmento (metros, con signo). Doble clic para editar.",
    StlPreview_UserFloorLineDragHandleAria:
        "Arrastrar para mover el segmento en planta (eje X y Z)",
    StlPreview_Desing2TopToolLinea: "Línea",
    StlPreview_Desing2TopToolLineaAria:
        "Segmento en el plano del suelo: segundo punto por clic o distancia por teclado (+ Intro); snap de rejilla informativo",
    StlPreview_Desing2TopToolOrtho15TitleOn:
      "Ortogonal 15° en planta activo — segundo punto en múltiplos de 15° (0° = eje +X). Pulse o F8 para desactivar.",
    StlPreview_Desing2TopToolOrtho15TitleOff:
      "Ortogonal 15° desactivado — ángulo libre hacia el segundo punto. Pulse o F8 para activar.",
    StlPreview_LineToolOrthoToastOn: "Ortogonal 15° activado (F8).",
    StlPreview_LineToolOrthoToastOff: "Ortogonal 15° desactivado (F8).",
    StlPreview_Desing2TopToolOffset: "Offset",
    StlPreview_Desing2TopToolOffsetAria:
      "Herramienta offset — próximamente (solo interfaz)",
    StlPreview_Desing2TopToolRecortar: "Recortar",
    StlPreview_Desing2TopToolRecortarAria:
      "Herramienta recortar — próximamente (solo interfaz)",
    StlPreview_Desing2TopToolAlargar: "Alargar",
    StlPreview_Desing2TopToolAlargarAria:
      "Herramienta alargar — próximamente (solo interfaz)",
    StlPreview_Desing2TopToolBorrar: "Borrar",
    StlPreview_Desing2TopToolBorrarAria:
      "Herramienta borrar — próximamente (solo interfaz)",
    StlPreview_Desing2SideMenuCollapseTitle:
      "Ocultar panel hasta acercar de nuevo el puntero al borde izquierdo",
    StlPreview_Desing2SideMenuTabEntorno: "Entorno",
    StlPreview_Desing2SideMenuTabConfiguracion: "Configuración",
    StlPreview_Desing2SideMenuPlaceholder: "Menú (próximamente)",
    StlPreview_Desing2EntornoGridSnapLabel: "Incremental de rejilla y snap",
    StlPreview_Desing2EntornoGridSnapAria:
      "Espaciado menor de rejilla en planta y paso del snap en modo punto de rejilla para las cotas.",
    StlPreview_Desing2EntornoRulerExtentLabel: "Alcance máximo del trazado de reglas",
    StlPreview_Desing2EntornoRulerExtentAria:
      "Distancia física máxima de los trazados de cotas desde el punto de referencia."
  })
);

const en = Object.assign({}, es, {
  Common_Home: "Home",
  Common_Intranet: "Intranet",
  Index_Breadcrumb: "Articles",
  Index_CreateArticle: "New article",

  Col_AtenkoCode: "Code",
  Col_Description: "Description",
  Col_High: "Height",
  Col_Width: "Width",
  Col_Long: "Length",
  Col_Weight: "Weight",
  Col_Mts2: "M²",
  Col_Mts3: "M³",
  Col_BlockNumber: "DWG block",
  Col_StlNumber: ".Stl",
  Col_Company: "Company",
  Col_System: "System",
  Col_Plant3D: "Plan 3D",
  Col_VertElev3D: "Vert. elev. 3D",
  Col_HorzElev3D: "Horz. elev. 3D",
  Col_PlantMockup: "Plan mock-up",
  Col_VertElevMockup: "Vert. elev. mock-up",
  Col_HorzElevMockup: "Horz. elev. mock-up",
  Col_PlantStl: "Plan STL",
  Col_VertElevStl: "Vert. elev. STL",
  Col_HorzElevStl: "Horz. elev. STL",
  Col_InsertMaster: "Insert master",
  Col_State: "State",
  Col_Actions: "Actions",

  Dt_Rows_All: "All",
  Dt_Rows_N: "rows",
  Dt_MenuAria: "List options",
  Dt_Section_Records: "Records",
  Dt_Section_Export: "Export",
  Dt_Section_ColumnsVisible: "Visible columns",

  State_Active: "Active",
  State_Inactive: "Inactive",
  State_Yes: "Yes",
  State_No: "No",

  Js_ConfirmDeleteArticle:
    "Delete this article? This action cannot be undone.",
  Js_ConfirmActivate: "Activate this article?",
  Js_ConfirmDeactivate:
    "Deactivate this article? It will remain in the database but will not be used as active.",

  ToastTitle_Articles: "Articles",
  ToastTitle_CreateArticle: "Create article",
  ToastTitle_EditArticle: "Edit article",
  ToastTitle_DeleteArticle: "Delete article",
  ToastMessage_ArticleCreated: "Article \"{0}\" created successfully.",
  ToastMessage_ArticleUpdated: "Article \"{0}\" updated successfully.",
  ToastMessage_ArticleDeleted: "Article \"{0}\" deleted successfully.",
  ToastMessage_ArticleActivated: "Article \"{0}\" activated.",
  ToastMessage_ArticleDeactivated: "Article \"{0}\" deactivated.",
  ToastMessage_ArticleCreatedReviewAttachments:
    "Article created. Please review the attachments in the edit form.",
  ToastMessage_ArticleCreatedWithErrors: "Article created. {0}",

  Val_CodeRequired: "Code is required.",
  Val_LabelRequired: "Description is required.",
  Val_SystemRequired: "Please select a system.",
  Val_SystemInvalid: "The selected system is not valid.",
  Val_DuplicateCodeCreate:
    "An article with this code already exists in the same system.",
  Val_DuplicateCodeEdit:
    "Another article with this code already exists in the same system.",
  Val_BlockFileTooLarge:
    "File too large (max. 50 MB) for block {0}.",
  Val_BlockFileExtensionRequired:
    "Invalid extension in file for {0}.",
  Val_BlockFileExtensionInvalid:
    "Only {0} files are allowed for block {1}.",
  Val_BlockFileEmpty: "Empty file.",
  Val_BlockFileExtensionMissing:
    "The file has no extension. Allowed extensions: {0}.",
  Val_BlockFileNotAllowedExtension:
    "Only {0} files are allowed for this field.",
  Val_BlockFileNameInvalid: "Invalid file name.",
  Val_BlockFileSaveOverwriteFailed:
    "Could not overwrite the existing file: {0}",
  Val_BlockFileMaxSize: "The file exceeds the maximum allowed size (50 MB).",

  Err_ArticleNotFound: "Article not found.",
  Err_ArticleNotFoundOrDeleted: "The article does not exist or has already been deleted.",
  Err_CannotDeleteHasReferences:
    "Cannot delete: the article is in use (replacement stock or temporary lists).",
  Err_DeleteFailed: "Could not delete the article. {0}",
  Err_ApsNotConfigured:
    "Autodesk APS is not configured (ClientId and ClientSecret in Web.config).",
  Err_SlotKeyInvalid: "Invalid slotKey parameter.",
  Err_NoDwgInBlock: "No DWG file in this block.",
  Err_DxfSiblingMissing:
    "Sibling DXF not found (same base name and folder as the .dwg, .dxf extension). Place the file on the server or generate it when saving the article.",

  Page_CreateTitle: "Create article",
  Page_EditTitle: "Edit article",
  Page_DetailsTitle: "Article details",
  Page_BackToList: "Back to list",

  Lbl_CodeRequired: "Code *",
  Lbl_LabelRequired: "Description *",
  Lbl_SystemRequired: "System *",
  Lbl_AtenkoCode: "Atenko code",
  Lbl_Active: "Active",
  Lbl_InsertInMaster: "Insert in Master Articles",
  Lbl_High: "Height",
  Lbl_Width: "Width",
  Lbl_Long: "Length",
  Lbl_Weight: "Weight",
  Lbl_Mts2: "M²",
  Lbl_Mts3: "M³",
  Lbl_BlockNumber: "DWG block",
  Lbl_StlNumber: ".Stl",
  Lbl_Color1: "Color 1",
  Lbl_Color2: "Color 2",
  Lbl_Color2Hint: "Max. 10 characters when saved.",
  Lbl_StlFilesSection: "STL model files",
  Lbl_StlFilesHelp:
    "Attach an .stl file for each view (plan and elevations). They are optional; saved in Files/MasterArticles/blocks and shown in Details with the 3D viewer.",
  Lbl_BlockDwgFilesSection: "Block files — AutoCAD (only .dwg)",
  Lbl_BlockDwgFilesHelp:
    "These .dwg files are work files (e.g. with ZWCAD). They are saved in the shared folder Files/MasterArticles/blocks with the same name you attach. In the details view, the preview uses the sibling DXF (.dxf next to the .dwg).",
  Lbl_SystemPlaceholder: "-- Select --",

  BlockSlot_Plant3D: "Plan 3D",
  BlockSlot_VertElev3D: "Vertical elevation 3D",
  BlockSlot_HorzElev3D: "Horizontal elevation 3D",
  BlockSlot_PlantMockup: "Plan mock-up",
  BlockSlot_VertElevMockup: "Vertical elevation mock-up",
  BlockSlot_HorzElevMockup: "Horizontal elevation mock-up",
  BlockSlot_PlantStl: "Plan — .stl file",
  BlockSlot_VertElevStl: "Vertical elevation — .stl file",
  BlockSlot_HorzElevStl: "Horizontal elevation — .stl file",
  BlockSlot_OpenFile: "Open file",
  BlockSlot_SavedFile: "Saved file:",
  BlockSlot_Remove: "Remove",

  Btn_New: "New article",
  Btn_Save: "Save",
  Btn_SaveChanges: "Save changes",
  Btn_Cancel: "Cancel",
  Btn_BackToList: "Back to list",
  Btn_EditArticle: "Edit article",
  Btn_CreateArticle: "Create article",

  Details_Section_Identification: "Identification and system",
  Details_Section_Measurements: "Measurements and references",
  Details_Field_Description: "Description",
  Details_Field_AtenkoCode: "Atenko code",
  Details_Field_Company: "Company",
  Details_Field_System: "System",
  Details_Field_Active: "Active",
  Details_Field_InsertMaster: "Insert in Master Articles",
  Details_Field_High: "Height",
  Details_Field_Width: "Width",
  Details_Field_Long: "Length",
  Details_Field_Weight: "Weight",
  Details_Field_Mts2: "M²",
  Details_Field_Mts3: "M³",
  Details_Field_BlockNumber: "DWG block (text)",
  Details_Field_StlNumber: ".Stl (text)",
  Details_NoValue: "—",
  Details_YesValue: "Yes",
  Details_NoBoolValue: "No",

  List_LinkOpenTooltip: "Article details",
  List_LinkEditTooltip: "Edit article",
  List_LinkDeleteTooltip: "Delete article",
  List_LinkDeleteLockedTooltip:
    "Cannot delete: the article is in use (replacement stock or temporary lists).",
  List_LinkActivateTooltip: "Activate article",
  List_LinkDeactivateTooltip: "Deactivate article",
  List_LinkAttachmentTooltip: "Open attachment",

  StlPreview_GridHeaderEmpty: "",
  StlPreview_GridColPlant: "Plan",
  StlPreview_GridColVertElev: "Vertical elevation",
  StlPreview_GridColHorzElev: "Horizontal elevation",
  StlPreview_RowLabel3D: "3D",
  StlPreview_RowLabelMockup: "mock-up",
  StlPreview_RowLabelStl: "STL",
  StlPreview_NoFile: "No file.",
  StlPreview_LoadStlTooltip: "View in the 3D viewer",
  StlPreview_NoStlInDiskTooltip:
    "No STL on disk for this cell (same base name as the attachment or .stl file in the slot).",
  StlPreview_NoStlInViewerTooltip: "No STL in the viewer",
  StlPreview_StatusInitial:
    "Select a block with available STL (orange button) to view it here.",
  StlPreview_CameraLabel: "Camera",
  StlPreview_CameraOrtho: "Orthographic",
  StlPreview_CameraIso: "Isometric",
  StlPreview_CameraOrthoTitle: "Orthographic view",
  StlPreview_CameraIsoTitle: "Isometric view",
  StlPreview_CameraModeAria: "Camera mode",
  StlPreview_FullscreenTitle: "Fullscreen",
  StlPreview_SaveViewTitle: "Save view",
  StlPreview_SaveViewAria: "Save view",
  StlPreview_SaveViewSavedTitle: "Saved",
  StlPreview_SaveViewSavedToast: "View saved",
  StlPreview_EscapeCancelToolsToast: "Canceled",
  StlPreview_GridToggleTitle: "Show background grid",
  StlPreview_GridToggleAria: "Background grid",
  StlPreview_SkyToggleTitle: "Show or hide sky",
  StlPreview_SkyToggleAria: "STL sky",
  StlPreview_GroundShadowTitle: "Show ground shadow",
  StlPreview_GroundShadowAria: "Ground shadow",
  StlPreview_DarkBgTitle: "Enable dark background",
  StlPreview_DarkBgAria: "Dark background",
  StlPreview_ClipToggleTitle: "Show or hide clipping",
  StlPreview_ClipToggleAria: "STL clipping",
  StlPreview_XyzAxesToggleTitle: "Show or hide X, Y, and Z axis vectors (green, with arrowheads)",
  StlPreview_XyzAxesAria: "XYZ axes",
  StlPreview_UcsRulersToggleTitle: "Show or hide plan rulers from anchor (+X / +Z)",
  StlPreview_UcsRulersAria: "Plan rulers",
  StlPreview_RulerAnchorGridPickTitle: "Place rulers on a grid intersection (500 mm)",
  StlPreview_RulerAnchorGridPickAria: "Grid mode: only activated minor-grid intersections",
  StlPreview_RulerAnchorGridPickModeToast:
    "Move to a grid junction until it highlights green, then click",
  StlPreview_RulerAnchorGridSnapRequiredToast:
    "Move closer to an active junction (green highlight); only grid snap is accepted",
  StlPreview_RulerAnchorObjectPickTitle:
    "Insertion point from the STL part (floor plan bottom-left footprint corner)",
  StlPreview_RulerAnchorObjectPickAria:
    "Hover an STL mesh and click: rulers anchor at the CAD sketch origin (footprint bottom-left corner)",
  StlPreview_RulerAnchorObjectPickModeToast:
    "Hover the STL (highlight), then click to anchor rulers at its insertion point",
  StlPreview_RulerAnchorObjectInsertionToast:
    "Insertion point applied to rulers",
  StlPreview_RulerAnchorObjectPickMissToast: "Pick an STL mesh under the cursor",
  StlPreview_RulerAnchorGridCoordsHud: "X: {0} m  Z: {1} m",
  StlPreview_RulerAnchorGridIntersectionToast: "Rulers placed",
  StlPreview_ClipYTitle:
    "Height clipping: 1000 = full view; lowering the slider clips from top to bottom.",
  StlPreview_ClipYAria:
    "Horizontal clipping (height): 1000 full view; lowering it clips from top to bottom",
  StlPreview_ClipXTitle:
    "Plan clipping: 1000 = full view; lowering the slider clips from right to left.",
  StlPreview_ClipXAria:
    "Plan clipping (X): 1000 full view; lowering it clips from right to left",
  StlPreview_Desing2SideMenuAria: "STL viewer side menu",
  StlPreview_Desing2TopToolbarAria: "STL canvas tool bar",
  StlPreview_Desing2TopToolbarCollapseTitle:
    "Collapse bar until you move the pointer near the top center strip again",
  StlPreview_Desing2TopToolbarPinTitle:
    "Pin toolbar open (stay expanded until unpinned)",
  StlPreview_Desing2TopToolbarUnpinTitle:
    "Unpin toolbar and return to hover-to-expand mode",
  StlPreview_LineToolInstructionFirst: "Click for the first point (move the mouse for preview).",
  StlPreview_LineToolInstructionSecond:
    "Second point: click on the floor, or type the distance and press Enter.",
  StlPreview_LineToolHudDistanceAria:
    "Length to the second point (metres or millimetres). Enter completes the segment; Escape cancels line mode.",
  StlPreview_LineToolHudDistancePlaceholder: "e.g. 5 · 5.5 · 5000 mm",
  StlPreview_LineToolDistancePreviewApprox: "≈",
  StlPreview_LineToolDistanceInvalidToast:
    "Enter a valid distance (e.g. 5, 5.5 or 5000 mm) or click for the second point.",
  StlPreview_UserFloorLineDimEditAria:
    "Floor line segment length in metres (or type mm suffix). Enter to apply, Escape cancels.",
  StlPreview_UserFloorLineDimEditDeltaXAria:
    "ΔX from the ruler anchor to the line’s second endpoint (P2) in metres (signed; mm suffix OK). Enter to apply; Escape cancels.",
  StlPreview_UserFloorLineDimEditDeltaZAria:
    "ΔZ from the ruler anchor to the line’s second endpoint (P2) in metres (signed; mm suffix OK). Enter to apply; Escape cancels.",
  StlPreview_UserFloorLineDimReadoutDeltaXAria:
    "ΔX from ruler intersection to the free end of the segment (signed metres). Double-click to edit.",
  StlPreview_UserFloorLineDimReadoutDeltaZAria:
    "ΔZ from ruler intersection to the free end of the segment (signed metres). Double-click to edit.",
  StlPreview_UserFloorLineDragHandleAria:
    "Drag to move the segment on the floor plan (X and Z axes)",
  StlPreview_Desing2TopToolLinea: "Line",
  StlPreview_Desing2TopToolLineaAria:
    "Floor-plan segment: place the second point with a click or by typing a distance (+ Enter); optional grid snap hints",
  StlPreview_Desing2TopToolOrtho15TitleOn:
    "15° floor ortho on — second point snaps to 15° steps (0° = +X axis). Click or F8 to turn off.",
  StlPreview_Desing2TopToolOrtho15TitleOff:
    "15° floor ortho off — free angle to the second point. Click or F8 to turn on.",
  StlPreview_LineToolOrthoToastOn: "15° ortho on (F8).",
  StlPreview_LineToolOrthoToastOff: "15° ortho off (F8).",
  StlPreview_Desing2TopToolOffset: "Offset",
  StlPreview_Desing2TopToolOffsetAria:
    "Offset tool — coming soon (UI placeholder only)",
  StlPreview_Desing2TopToolRecortar: "Trim",
  StlPreview_Desing2TopToolRecortarAria:
    "Trim tool — coming soon (UI placeholder only)",
  StlPreview_Desing2TopToolAlargar: "Extend",
  StlPreview_Desing2TopToolAlargarAria:
    "Extend tool — coming soon (UI placeholder only)",
  StlPreview_Desing2TopToolBorrar: "Delete",
  StlPreview_Desing2TopToolBorrarAria:
    "Delete tool — coming soon (UI placeholder only)",
  StlPreview_Desing2SideMenuCollapseTitle:
    "Hide panel until you move the pointer near the left edge again",
  StlPreview_Desing2SideMenuTabEntorno: "Environment",
  StlPreview_Desing2SideMenuTabConfiguracion: "Configuration",
  StlPreview_Desing2SideMenuPlaceholder: "Menu (coming soon)",
  StlPreview_Desing2EntornoGridSnapLabel: "Grid increment and snap",
  StlPreview_Desing2EntornoGridSnapAria:
    "Minor grid spacing on the floor plane and snap step in grid pick mode for ruler placement.",
  StlPreview_Desing2EntornoRulerExtentLabel: "Maximum ruler drawing reach",
  StlPreview_Desing2EntornoRulerExtentAria:
    "Maximum physical distance of ruler tick marks from the reference point."
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
// Auto-generated by _gen_masterarticles_resources.js — MasterArticles UI strings (${keys.length}
// entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>Tsql_Master_Articles module: localized strings (.resx); UICulture desde LanguageUiHelper.</summary>
    public class MasterArticles
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new global::Desing.Helpers.DbBackedResourceManager(
                "Desing.Resources.MasterArticles", typeof(MasterArticles).Assembly, "MasterArticles"));

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
fs.writeFileSync(path.join(dir, "MasterArticles.resx"), buildResx(es), "utf8");
fs.writeFileSync(path.join(dir, "MasterArticles.en.resx"), buildResx(en), "utf8");
fs.writeFileSync(path.join(dir, "MasterArticles.Designer.cs"), designer, "utf8");
console.log("OK", keys.length, "keys -> Resources/MasterArticles.*");
