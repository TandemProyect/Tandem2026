/* =============================================================================
   Semilla: documentos para agentes (dbo.TSql_DevelodAgentDocument).
   Clave natural: TextCode. Inserta lo que falta y actualiza lo que haya cambiado
   (auditoría LinModifiedBy / AddLastDateChange / Ntimeschanged + 1). Idempotente.
   Al crear, renombrar o quitar un área en AGENTS.md, cambia esta lista y vuelve a ejecutarla.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @User NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

DECLARE @Docs TABLE
(
    NumberOrder     INT,
    TextCode        NVARCHAR(100),
    TextLabel       NVARCHAR(500),
    TextCommand     NVARCHAR(1000),
    TextPath        NVARCHAR(300),
    TextDescription NVARCHAR(MAX)
);

INSERT INTO @Docs VALUES
(10, N'agents', N'Entrada para agentes', N'Lee AGENTS.md y trabaja en <área>: <lo que quieres>.', N'AGENTS.md',
 N'Punto de entrada único. Reglas de siempre (rama develo, sin commit ni push, bases idénticas, compilar con 0 errores, responder en español) y tabla «Si trabajas en X, lee solo Y» que lleva al INDEX.md de cada área. Si la tarea cruza áreas, nómbralas todas.'),
(20, N'como-trabajar', N'Cómo trabajar con agentes', N'Lee Docs/COMO-TRABAJAR.md y explícame cómo añadir un tema nuevo.', N'Docs/COMO-TRABAJAR.md',
 N'Guía para Juan Andrés: qué decirle a un agente nuevo, cómo añadir un tema o un área, cómo mantener los documentos pequeños y mapa rápido de áreas.'),
(100, N'area.desing2-visor', N'Visor Desing_2', N'Lee AGENTS.md y trabaja en visor Desing_2: <lo que quieres>.', N'Docs/areas/desing2-visor/INDEX.md',
 N'Arranque del visor, cámara, órbita y cubo de vistas, rejilla, reglas y punto de inserción, colores, pestañas Entorno y Configuración, cookie de estado.'),
(110, N'area.desing2-dibujo-2d', N'Dibujo 2D en Desing_2', N'Lee AGENTS.md y trabaja en dibujo 2D: <lo que quieres>.', N'Docs/areas/desing2-dibujo-2d/INDEX.md',
 N'Polilínea, selección (ventana/cruce), borrar, recortar, alargar, girar, simetría, repetir comando, importar DWG/DXF, barra inferior (deshacer, orto, reglas) y cota de muro.'),
(120, N'area.desing2-muros-3d', N'Muros 3D en Desing_2', N'Lee AGENTS.md y trabaja en muros 3D: <lo que quieres>.', N'Docs/areas/desing2-muros-3d/INDEX.md',
 N'Paso de líneas a muro, muro recto y curvo, uniones L/T/+, modificar esquina, exportar e importar muros cifrados.'),
(130, N'area.encofrado-atk60', N'Encofrado ATK-60', N'Lee AGENTS.md y trabaja en encofrado ATK-60: <lo que quieres>.', N'Docs/areas/encofrado-atk60/INDEX.md',
 N'Piezas y reglas NEVI, reparto del muro recto, esquinas L/T/X, tapes, grapas, rigidizadores, tirantes, muro curvo, encofrado por fases y recuento.'),
(140, N'area.ia-asistentes', N'Asistentes IA', N'Lee AGENTS.md y trabaja en asistentes IA: <lo que quieres>.', N'Docs/areas/ia-asistentes/INDEX.md',
 N'Asistente IA del muro (chat, voz, recuento, cambio de paneles), IA del diseño (barra de modos), catálogo de comandos y frases en BD, entornos de conversación.'),
(200, N'area.base-de-datos', N'Base de datos y DAL', N'Lee AGENTS.md y trabaja en base de datos: <lo que quieres>.', N'Docs/areas/base-de-datos/INDEX.md',
 N'Esquema de develop y producción, scripts idempotentes en Desing/Scripts/TemporalScript, Model.edmx y limpieza de tablas antiguas.'),
(210, N'bd.sincronizar', N'Base de datos: develop y producción', N'Lee AGENTS.md y trabaja en base de datos: copia a producción <lo nuevo>.', N'Docs/areas/base-de-datos/sincronizar-develop-produccion.md',
 N'Bases (TdesingDevelop local y producción en site4now), qué registros van a las dos (esquema, usuarios @tediseng.net, derechos, catálogos), cómo copiarlos sin duplicar cruzando por clave natural, receta de usuario del sistema y consultas para ver qué falta.'),
(220, N'area.seguridad', N'Derechos: roles y funciones', N'Lee AGENTS.md y trabaja en derechos: <lo que quieres>.', N'Docs/areas/seguridad/INDEX.md',
 N'Quién puede ver y usar cada menú, pantalla, acción MVC, botón y comando JS. Usuario → varios roles → varias funciones; sin función activa, libre para todos. Recetas para proteger controladores, acciones, botones y comandos y para crear funciones y roles.'),
(230, N'area.intranet', N'Intranet', N'Lee AGENTS.md y trabaja en intranet: <lo que quieres>.', N'Docs/areas/intranet/INDEX.md',
 N'Pantallas CRUD (Materio), DataTables, paneles del inicio, formularios, direcciones Google Places, traducciones .resx + TSql_UiTranslation, foto del diseño y menú.'),
(300, N'area.cad-comun', N'Plugins CAD: parte común', N'Lee AGENTS.md y trabaja en plugins CAD (parte común): <lo que quieres>.', N'Docs/areas/cad-comun/INDEX.md',
 N'Lo que comparten todos los plugins: API DesignToolsAutocad/*, sesión y autorización de equipo, paletas MVC en WebView y detección de muros en servidor.'),
(310, N'area.cad-zwcad', N'Plugin ZWCAD', N'Lee AGENTS.md y trabaja en plugin ZWCAD: <lo que quieres>.', N'Docs/areas/cad-zwcad/INDEX.md',
 N'Plugin .NET Framework 4.8 para ZWCAD 2026: comandos, menú e iconos, instalación.'),
(320, N'area.cad-autocad', N'Plugin AutoCAD', N'Lee AGENTS.md y trabaja en plugin AutoCAD: <lo que quieres>.', N'Docs/areas/cad-autocad/INDEX.md',
 N'Plugin net8.0-windows para AutoCAD 2026: paletas, muro 2D/3D, bloquing, encofrado manual ATK-60, ATDESING.'),
(330, N'area.cad-bricscad-revit', N'Plugins BricsCAD y Revit', N'Lee AGENTS.md y trabaja en BricsCAD y Revit: <lo que quieres>.', N'Docs/areas/cad-bricscad-revit/INDEX.md',
 N'Plugins net8.0-windows para BricsCAD V26 y Revit 2026 (paletas, muro 2D/3D).'),
(340, N'area.cad-opencad', N'OpenCADStudio', N'Lee AGENTS.md y trabaja en OpenCADStudio: <lo que quieres>.', N'Docs/areas/cad-opencad/INDEX.md',
 N'Bridge C# OpenCadStudioPluging (--serve, WebView2) y menú nativo Rust TandemOpenCadStudioPlugin (TANDEM, TANDEM_MURO2D).'),
(350, N'area.cad-freecad', N'FreeCAD', N'Lee AGENTS.md y trabaja en FreeCAD: <lo que quieres>.', N'Docs/areas/cad-freecad/INDEX.md',
 N'Host C# FreeCadPluging y workbench Python Tandem2026 (FreeCAD 1.1).'),
(360, N'area.tandemxr', N'TandemXR (gafas)', N'Lee AGENTS.md y trabaja en TandemXR: <lo que quieres>.', N'Docs/areas/tandemxr/INDEX.md',
 N'Visor XR web (TandemXR) y Unity (TandemXR-Unity), dispositivos y trabajos de envío.'),
(400, N'area.despliegue', N'Despliegue', N'Lee AGENTS.md y trabaja en despliegue: <lo que quieres>.', N'Docs/areas/despliegue/INDEX.md',
 N'Publicar Desing en Site4Now/SmarterASP (develop y producción) con GitHub Actions.'),
(410, N'area.azure-devops', N'Azure DevOps y scripts', N'Lee AGENTS.md y trabaja en Azure DevOps: <lo que quieres>.', N'Docs/areas/azure-devops/INDEX.md',
 N'Crear y cerrar US y tareas, board, commits vinculados y scripts PowerShell de Scripts/.'),
(420, N'area.linkedin', N'LinkedIn', N'Lee AGENTS.md y trabaja en LinkedIn: <lo que quieres>.', N'Docs/areas/linkedin/INDEX.md',
 N'Contenido, calendario y perfil de la página.'),
(430, N'area.general', N'General', N'Lee AGENTS.md y trabaja en general: <lo que quieres>.', N'Docs/areas/general/INDEX.md',
 N'Arquitectura cliente-servidor, convenciones de código, estructura del repo, flujo de Git y el proyecto Desing en conjunto.'),
(500, N'informe', N'Informe de trabajo', N'Lee AGENTS.md y actualiza el informe de trabajo con <lo hecho>.', N'Desing/Report/LEEME.md',
 N'Solo cuando se pide: dónde está el informe abierto, cómo anotar horas y cómo abrir un frente.'),
(510, N'informe.generar-develop', N'Generar Informe Develop', N'Lee AGENTS.md y genera el informe Develop: <rango o últimas dos semanas>.', N'Docs/areas/general/Generar-Informe-Develop.md',
 N'Cómo generar el informe semanal US/BUG y el PDF: fichero abierto, fuentes (chats, no git), convención US azul / BUG rojo, Chrome headless y semilla en Agentes.');

UPDATE t
   SET t.TextLabel = d.TextLabel, t.TextCommand = d.TextCommand, t.TextPath = d.TextPath,
       t.TextDescription = d.TextDescription, t.NumberOrder = d.NumberOrder,
       t.LinModifiedBy = @User, t.AddLastDateChange = GETDATE(), t.Ntimeschanged = t.Ntimeschanged + 1
  FROM dbo.TSql_DevelodAgentDocument t
  JOIN @Docs d ON d.TextCode = t.TextCode
 WHERE t.Is_Delete = 0
   AND (t.TextLabel <> d.TextLabel OR t.TextCommand <> d.TextCommand
        OR ISNULL(t.TextPath, N'') <> ISNULL(d.TextPath, N'')
        OR ISNULL(t.TextDescription, N'') <> ISNULL(d.TextDescription, N'')
        OR t.NumberOrder <> d.NumberOrder);

INSERT INTO dbo.TSql_DevelodAgentDocument
    (TextLabel, TextCode, TextCommand, TextPath, TextDescription, NumberOrder,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT d.TextLabel, d.TextCode, d.TextCommand, d.TextPath, d.TextDescription, d.NumberOrder,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM @Docs d
 WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_DevelodAgentDocument t WHERE t.Is_Delete = 0 AND t.TextCode = d.TextCode);
GO

PRINT N'OK — documentos para agentes sembrados (o ya existían).';
GO
