-- 2026-11-14 pt109u: copia de filas de referencia (catálogos pequeños) de db_a197cd_desingproducction a db_a197cd_desingdevelop.
-- Solo tablas nuevas en destino. Idempotente: inserta solo los Id que no existan. Conserva los Id originales.
-- Ejecutar DESPUÉS de 2026-11-14_align_schema_develop.sql: sqlcmd -S <srv> -E -d db_a197cd_desingdevelop -I -x -f 65001 -b -i <este archivo>
SET NOCOUNT ON;
GO
IF OBJECT_ID(N'dbo.TSql_OfferState', N'U') IS NOT NULL
BEGIN TRY
  SET IDENTITY_INSERT dbo.[TSql_OfferState] ON;
  INSERT INTO dbo.[TSql_OfferState] ([IdObject], [TextLabel], [AddColor], [Is_Delete], [Is_Active], [LinkMadeBy], [LinModifiedBy], [AddDateMade], [AddChangeBy], [Ntimeschanged])
  SELECT CAST(v.[IdObject] AS bigint) , CAST(v.[TextLabel] AS nvarchar(500)) , CAST(v.[AddColor] AS nvarchar(50)) , CAST(v.[Is_Delete] AS bit) , CAST(v.[Is_Active] AS bit) , CAST(v.[LinkMadeBy] AS nvarchar(128)) , CAST(v.[LinModifiedBy] AS nvarchar(128)) , CAST(v.[AddDateMade] AS datetime) , CAST(v.[AddChangeBy] AS nvarchar(128)) , CAST(v.[Ntimeschanged] AS bigint) 
  FROM (VALUES
    (1, N'Ofertada', N'#f3f708', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-19T09:13:31.090', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', 0),
    (2, N'Aceptada', N'#018df9', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-19T09:14:17.490', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', 0),
    (3, N'Rechazada', N'#f92601', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-19T09:14:58.827', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', 0),
    (4, N'En Espera', N'#9a3232', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-10-22T19:35:23.867', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', 0)
  ) v ([IdObject], [TextLabel], [AddColor], [Is_Delete], [Is_Active], [LinkMadeBy], [LinModifiedBy], [AddDateMade], [AddChangeBy], [Ntimeschanged])
  WHERE NOT EXISTS (SELECT 1 FROM dbo.[TSql_OfferState] t WHERE t.[IdObject] = v.[IdObject]);
  PRINT 'COPY TSql_OfferState: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' filas';
  SET IDENTITY_INSERT dbo.[TSql_OfferState] OFF;
END TRY
BEGIN CATCH
  IF OBJECTPROPERTY(OBJECT_ID(N'dbo.TSql_OfferState'), 'TableHasIdentity') = 1 SET IDENTITY_INSERT dbo.[TSql_OfferState] OFF;
  PRINT 'SKIP COPY TSql_OfferState: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.TSql_Extension', N'U') IS NOT NULL
BEGIN TRY
  SET IDENTITY_INSERT dbo.[TSql_Extension] ON;
  INSERT INTO dbo.[TSql_Extension] ([IdObject], [TextLabel], [Is_Delete], [Is_Active], [LinkMadeBy], [LinModifiedBy], [AddDateMade], [AddChangeBy], [Ntimeschanged], [NumberMaxFileSizeBytes], [Path_Ico])
  SELECT CAST(v.[IdObject] AS bigint) , CAST(v.[TextLabel] AS nvarchar(500)) , CAST(v.[Is_Delete] AS bit) , CAST(v.[Is_Active] AS bit) , CAST(v.[LinkMadeBy] AS nvarchar(128)) , CAST(v.[LinModifiedBy] AS nvarchar(128)) , CAST(v.[AddDateMade] AS datetime) , CAST(v.[AddChangeBy] AS nvarchar(128)) , CAST(v.[Ntimeschanged] AS bigint) , CAST(v.[NumberMaxFileSizeBytes] AS bigint) , CAST(v.[Path_Ico] AS nvarchar(500)) 
  FROM (VALUES
    (2, N'.dwg', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', N'2026-05-16T11:56:40.443', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', 1, 10485760, NULL),
    (3, N'.PDF', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-16T17:56:21.177', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', 0, 10485760, NULL)
  ) v ([IdObject], [TextLabel], [Is_Delete], [Is_Active], [LinkMadeBy], [LinModifiedBy], [AddDateMade], [AddChangeBy], [Ntimeschanged], [NumberMaxFileSizeBytes], [Path_Ico])
  WHERE NOT EXISTS (SELECT 1 FROM dbo.[TSql_Extension] t WHERE t.[IdObject] = v.[IdObject]);
  PRINT 'COPY TSql_Extension: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' filas';
  SET IDENTITY_INSERT dbo.[TSql_Extension] OFF;
END TRY
BEGIN CATCH
  IF OBJECTPROPERTY(OBJECT_ID(N'dbo.TSql_Extension'), 'TableHasIdentity') = 1 SET IDENTITY_INSERT dbo.[TSql_Extension] OFF;
  PRINT 'SKIP COPY TSql_Extension: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.TSql_Plantilla', N'U') IS NOT NULL
BEGIN TRY
  SET IDENTITY_INSERT dbo.[TSql_Plantilla] ON;
  INSERT INTO dbo.[TSql_Plantilla] ([SysObjectID], [AttName], [AttColor], [AttLogo], [AttIsDefault], [AttIsDeleted], [LinCreatedBy], [AttCreated], [LinModifiedBy], [AttLastModification], [SysUpdateNumber], [AttFavicon], [AttBrandText], [AttBrandTextColor], [AttBrandAccentColor], [AttLinkColor])
  SELECT CAST(v.[SysObjectID] AS bigint) , CAST(v.[AttName] AS nvarchar(150)) , CAST(v.[AttColor] AS nvarchar(20)) , CAST(v.[AttLogo] AS nvarchar(500)) , CAST(v.[AttIsDefault] AS bit) , CAST(v.[AttIsDeleted] AS bit) , CAST(v.[LinCreatedBy] AS nvarchar(128)) , CAST(v.[AttCreated] AS datetime) , CAST(v.[LinModifiedBy] AS nvarchar(128)) , CAST(v.[AttLastModification] AS datetime) , CAST(v.[SysUpdateNumber] AS bigint) , CAST(v.[AttFavicon] AS nvarchar(500)) , CAST(v.[AttBrandText] AS nvarchar(120)) , CAST(v.[AttBrandTextColor] AS nvarchar(20)) , CAST(v.[AttBrandAccentColor] AS nvarchar(20)) , CAST(v.[AttLinkColor] AS nvarchar(20)) 
  FROM (VALUES
    (1, N'Plantilla por defecto', N'#186cb7', N'/Content/images/tDesing/t-desingc.png', 1, 0, N'system', N'2026-05-10T03:59:51.190', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', N'2026-10-16T10:02:25.100', 8, N'/Content/images/tDesing/tDesing.png', N'T Desing.net', NULL, N'#f29100', NULL)
  ) v ([SysObjectID], [AttName], [AttColor], [AttLogo], [AttIsDefault], [AttIsDeleted], [LinCreatedBy], [AttCreated], [LinModifiedBy], [AttLastModification], [SysUpdateNumber], [AttFavicon], [AttBrandText], [AttBrandTextColor], [AttBrandAccentColor], [AttLinkColor])
  WHERE NOT EXISTS (SELECT 1 FROM dbo.[TSql_Plantilla] t WHERE t.[SysObjectID] = v.[SysObjectID]);
  PRINT 'COPY TSql_Plantilla: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' filas';
  SET IDENTITY_INSERT dbo.[TSql_Plantilla] OFF;
END TRY
BEGIN CATCH
  IF OBJECTPROPERTY(OBJECT_ID(N'dbo.TSql_Plantilla'), 'TableHasIdentity') = 1 SET IDENTITY_INSERT dbo.[TSql_Plantilla] OFF;
  PRINT 'SKIP COPY TSql_Plantilla: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
BEGIN TRY
  SET IDENTITY_INSERT dbo.[TSql_UiTranslation] ON;
  INSERT INTO dbo.[TSql_UiTranslation] ([IdObject], [TextResourceKey], [TextModule], [LinkLanguage], [TextValue], [Is_Delete], [Is_Active], [LinkMadeBy], [LinModifiedBy], [AddDateMade], [AddLastDateChange], [Ntimeschanged])
  SELECT CAST(v.[IdObject] AS bigint) , CAST(v.[TextResourceKey] AS nvarchar(256)) , CAST(v.[TextModule] AS nvarchar(100)) , CAST(v.[LinkLanguage] AS bigint) , CAST(v.[TextValue] AS nvarchar(max)) , CAST(v.[Is_Delete] AS bit) , CAST(v.[Is_Active] AS bit) , CAST(v.[LinkMadeBy] AS nvarchar(128)) , CAST(v.[LinModifiedBy] AS nvarchar(128)) , CAST(v.[AddDateMade] AS datetime) , CAST(v.[AddLastDateChange] AS datetime) , CAST(v.[Ntimeschanged] AS bigint) 
  FROM (VALUES
    (1, N'UiTranslation.PageTitle', N'UiTranslation', 1, N'Traducciones UI', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (2, N'UiTranslation.BreadcrumbHome', N'UiTranslation', 1, N'Inicio', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (3, N'UiTranslation.BreadcrumbConfiguration', N'UiTranslation', 1, N'Configuración', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (4, N'UiTranslation.ExportHeading', N'UiTranslation', 1, N'Exportar', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (5, N'UiTranslation.ExportDescription', N'UiTranslation', 1, N'Descarga un .xlsx con TextResourceKey, TextModule, texto del idioma por defecto y una columna por cada idioma activo (TextCode).', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (6, N'UiTranslation.ExportButton', N'UiTranslation', 1, N'Descargar Excel', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (7, N'UiTranslation.ImportHeading', N'UiTranslation', 1, N'Importar', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (8, N'UiTranslation.ImportDescription', N'UiTranslation', 1, N'Sube un .xlsx con la columna TextResourceKey, opcionalmente TextModule e Is_Active, y columnas por idioma (mismo encabezado que el TextCode en BD).', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (9, N'UiTranslation.ImportButton', N'UiTranslation', 1, N'Importar', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (10, N'UiTranslation.ErrorReportButton', N'UiTranslation', 1, N'Descargar informe de errores (.xlsx)', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:58:43.680', NULL, 0),
    (11, N'Menu.Home', N'Common', 1, N'Inicio', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (12, N'Menu.Design', N'Common', 1, N'Diseño', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (13, N'Menu.Administration', N'Common', 1, N'Administración', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (14, N'Menu.Companies', N'Common', 1, N'Empresas', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (15, N'Menu.Employees', N'Common', 1, N'Empleados', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (16, N'Menu.Articles', N'Common', 1, N'Artículos', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (17, N'Menu.Templates', N'Common', 1, N'Plantillas', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (18, N'Menu.Clients', N'Common', 1, N'Clientes', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (19, N'Menu.JobSites', N'Common', 1, N'Obras', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (20, N'Menu.ConfigurationSection', N'Common', 1, N'Configuración', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (21, N'Menu.Configuration', N'Common', 1, N'Configuración', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (22, N'Menu.DocumentTypes', N'Common', 1, N'Tipos de documento', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (23, N'Menu.Extensions', N'Common', 1, N'Extensiones', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (24, N'Menu.UiTranslations', N'Common', 1, N'Traducciones UI', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (25, N'Menu.Support', N'Common', 1, N'Soporte', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (26, N'Menu.Help', N'Common', 1, N'Ayuda', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (27, N'Navbar.MenuToggleTitle', N'Common', 1, N'Mostrar/Ocultar menú', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (28, N'Navbar.EnvironmentBadge', N'Common', 1, N'Entorno: Develop', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (29, N'Navbar.UserFallback', N'Common', 1, N'Usuario', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (30, N'Navbar.MySpace', N'Common', 1, N'Mi espacio', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (31, N'Navbar.SignOut', N'Common', 1, N'Salir', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (32, N'Common.Save', N'Common', 1, N'Guardar', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (33, N'Common.Cancel', N'Common', 1, N'Cancelar', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (34, N'Common.Back', N'Common', 1, N'Volver', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (35, N'Common.Create', N'Common', 1, N'Crear', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (36, N'Common.Edit', N'Common', 1, N'Editar', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (37, N'Common.Delete', N'Common', 1, N'Eliminar', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (38, N'Common.Details', N'Common', 1, N'Detalles', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (39, N'Common.Search', N'Common', 1, N'Buscar', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T02:59:29.130', NULL, 0),
    (40, N'Menu.Languages', N'Common', 1, N'Idiomas', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-05-17T03:37:05.837', NULL, 0),
    (41, N'Menu.XrDevices', N'Common', 1, N'Dispositivos XR', 0, 1, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', NULL, N'2026-07-29T13:08:18.563', NULL, 0)
  ) v ([IdObject], [TextResourceKey], [TextModule], [LinkLanguage], [TextValue], [Is_Delete], [Is_Active], [LinkMadeBy], [LinModifiedBy], [AddDateMade], [AddLastDateChange], [Ntimeschanged])
  WHERE NOT EXISTS (SELECT 1 FROM dbo.[TSql_UiTranslation] t WHERE t.[IdObject] = v.[IdObject]);
  PRINT 'COPY TSql_UiTranslation: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' filas';
  SET IDENTITY_INSERT dbo.[TSql_UiTranslation] OFF;
END TRY
BEGIN CATCH
  IF OBJECTPROPERTY(OBJECT_ID(N'dbo.TSql_UiTranslation'), 'TableHasIdentity') = 1 SET IDENTITY_INSERT dbo.[TSql_UiTranslation] OFF;
  PRINT 'SKIP COPY TSql_UiTranslation: ' + ERROR_MESSAGE();
END CATCH
GO
PRINT 'FIN datos';
GO
