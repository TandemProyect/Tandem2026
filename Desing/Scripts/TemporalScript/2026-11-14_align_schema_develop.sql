-- 2026-11-14 pt109u: alineación ADITIVA de esquema: añade a db_a197cd_desingdevelop lo que tiene db_a197cd_desingproducction y le falta.
-- Idempotente (IF NOT EXISTS). No borra, no renombra, no cambia tipos ni datos existentes.
-- Ejecutar: sqlcmd -S <srv> -E -d db_a197cd_desingdevelop -I -x -f 65001 -b -i <este archivo>
SET NOCOUNT ON;
SET XACT_ABORT OFF;
GO

/* ===== 1. Tablas nuevas ===== */
IF OBJECT_ID(N'dbo.AA01Master', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[AA01Master] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_AA01Master_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_AA01Master_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_AA01Master_AddDateMade] DEFAULT (getdate()),
    [AddChangeBy] nvarchar(128) NOT NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_AA01Master_Ntimeschanged] DEFAULT ((0))
  );
  PRINT 'CREATE TABLE AA01Master';
END
GO
IF OBJECT_ID(N'dbo.TSql_Client_V2', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Client_V2] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_Client_V2_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_Client_V2_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_Client_V2_AddDateMade] DEFAULT (getdate()),
    [AddLastDateChange] datetime NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_Client_V2_Ntimeschanged] DEFAULT ((0)),
    [LinkMethodOfPayment] bigint NULL,
    [Path_Ico] nvarchar(500) NOT NULL,
    [Path_Logo] nvarchar(500) NOT NULL,
    [TextCode] nvarchar(128) NULL,
    [TextTaxId] nvarchar(50) NULL,
    [TextEmail] nvarchar(256) NULL,
    [TextPhone] nvarchar(50) NULL,
    [Loc_Place_Id] nvarchar(255) NULL,
    [Loc_Formatted_Address] nvarchar(1000) NULL,
    [Loc_Lat] decimal(9,6) NULL,
    [Loc_Lng] decimal(9,6) NULL,
    [Loc_Street_Number] nvarchar(50) NULL,
    [Loc_Route] nvarchar(250) NULL,
    [Loc_Subpremise] nvarchar(100) NULL,
    [Loc_Locality] nvarchar(250) NULL,
    [Loc_Admin_Area_1] nvarchar(250) NULL,
    [Loc_Admin_Area_2] nvarchar(250) NULL,
    [Loc_Postal_Code] nvarchar(20) NULL,
    [Loc_Country_Code] nvarchar(10) NULL,
    [Loc_Country_Name] nvarchar(100) NULL,
    [Loc_Address_Components_Json] nvarchar(max) NULL
  );
  PRINT 'CREATE TABLE TSql_Client_V2';
END
GO
IF OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Design_V2] (
    [SysObjectID] bigint IDENTITY(1,1) NOT NULL,
    [AttLabel] nvarchar(500) NOT NULL,
    [AttDescription] nvarchar(2000) NULL,
    [AttCenterY] float NOT NULL,
    [AttCenterX] float NOT NULL,
    [AttCreated] datetime NOT NULL,
    [AttIsDeleted] bit NOT NULL,
    [AttThumbnail] nvarchar(2000) NULL,
    [AttActiveCameraType] bigint NOT NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [ItIsShared] bit NOT NULL CONSTRAINT [DF_TSql_Design_V2_ItIsShared] DEFAULT ((1)),
    [ItIsSharedMyGrup] bit NOT NULL CONSTRAINT [DF_TSql_Design_V2_ItIsSharedMyGrup] DEFAULT ((1)),
    [AttChange] datetime NOT NULL CONSTRAINT [DF_TSql_Design_V2_AttChange] DEFAULT (((1)/(1))/(2024)),
    [IsRenderAt60] bit NOT NULL CONSTRAINT [DF_TSql_Design_V2_IsRenderAt60] DEFAULT ((0)),
    [LinkOffers] bigint NOT NULL,
    [AttPhoto] nvarchar(500) NULL,
    [NumberPropSpacingMm] float NULL,
    [AttPhasePlanJson] nvarchar(max) NULL,
    [AttViewerStateJson] nvarchar(max) NULL,
    [AttCornerReferencePointsJson] nvarchar(max) NULL
  );
  PRINT 'CREATE TABLE TSql_Design_V2';
END
GO
IF OBJECT_ID(N'dbo.TSql_DesignWallArticle', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_DesignWallArticle] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [LinkDesign_V2] bigint NOT NULL,
    [LinkDesignWall] bigint NOT NULL,
    [LinkMasterArticle] bigint NULL,
    [TextCode] nvarchar(50) NULL,
    [TextView] nvarchar(20) NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_TextView] DEFAULT (N'3D'),
    [NumberSequence] bigint NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_NumberSequence] DEFAULT ((0)),
    [NumberInsertX] float NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_NumberInsertX] DEFAULT ((0)),
    [NumberInsertY] float NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_NumberInsertY] DEFAULT ((0)),
    [NumberInsertZ] float NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_NumberInsertZ] DEFAULT ((0)),
    [NumberXRotation] float NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_NumberXRotation] DEFAULT ((0)),
    [NumberYRotation] float NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_NumberYRotation] DEFAULT ((0)),
    [NumberZRotation] float NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_NumberZRotation] DEFAULT ((0)),
    [TextHandleCad] nvarchar(50) NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_AddDateMade] DEFAULT (getdate()),
    [AddLastDateChange] datetime NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_DesignWallArticle_Ntimeschanged] DEFAULT ((0)),
    [NumberPhase] bigint NULL
  );
  PRINT 'CREATE TABLE TSql_DesignWallArticle';
END
GO
IF OBJECT_ID(N'dbo.TSql_Document', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Document] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [AddDescription] nvarchar(max) NOT NULL,
    [LinkJobside] bigint NULL,
    [LinkClient_V2] bigint NULL,
    [LinkOffer] bigint NULL,
    [LinkPersonal] bigint NULL,
    [LinkExtension] bigint NULL,
    [LinkDocumentType] bigint NOT NULL,
    [AddPath] nvarchar(max) NOT NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_Document_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_Document_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_Document_AddDateMade] DEFAULT (getdate()),
    [AddChangeBy] nvarchar(128) NOT NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_Document_Ntimeschanged] DEFAULT ((0)),
    [ExpirationDate] datetime NULL,
    [NoticeDate] datetime NULL
  );
  PRINT 'CREATE TABLE TSql_Document';
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentTypeExtension', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_DocumentTypeExtension] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_DocumentTypeExtension_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_DocumentTypeExtension_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_DocumentTypeExtension_AddDateMade] DEFAULT (getdate()),
    [AddChangeBy] nvarchar(128) NOT NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_DocumentTypeExtension_Ntimeschanged] DEFAULT ((0)),
    [LinkExtension] bigint NOT NULL,
    [LinkDocumentType] bigint NOT NULL
  );
  PRINT 'CREATE TABLE TSql_DocumentTypeExtension';
END
GO
IF OBJECT_ID(N'dbo.TSql_Extension', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Extension] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_Extension_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_Extension_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_Extension_AddDateMade] DEFAULT (getdate()),
    [AddChangeBy] nvarchar(128) NOT NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_Extension_Ntimeschanged] DEFAULT ((0)),
    [NumberMaxFileSizeBytes] bigint NOT NULL CONSTRAINT [DF_TSql_Extension_NumberMaxFileSizeBytes] DEFAULT ((10485760)),
    [Path_Ico] nvarchar(500) NULL
  );
  PRINT 'CREATE TABLE TSql_Extension';
END
GO
IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Jobside] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [AddNJobside] nvarchar(50) NULL,
    [AddNJobsideClient] nvarchar(50) NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_Jobside_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_Jobside_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_Jobside_AddDateMade] DEFAULT (getdate()),
    [AddLastDateChange] datetime NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_Jobside_Ntimeschanged] DEFAULT ((0)),
    [LinkClient_V2] bigint NULL,
    [BitBillSameAsLoc] bit NOT NULL CONSTRAINT [DF_TSql_Jobside_BitBillSameAsLoc] DEFAULT ((0)),
    [Loc_Place_Id] nvarchar(255) NULL,
    [Loc_Formatted_Address] nvarchar(1000) NULL,
    [Loc_Lat] decimal(9,6) NULL,
    [Loc_Lng] decimal(9,6) NULL,
    [Loc_Street_Number] nvarchar(50) NULL,
    [Loc_Route] nvarchar(250) NULL,
    [Loc_Subpremise] nvarchar(100) NULL,
    [Loc_Locality] nvarchar(250) NULL,
    [Loc_Admin_Area_1] nvarchar(250) NULL,
    [Loc_Admin_Area_2] nvarchar(250) NULL,
    [Loc_Postal_Code] nvarchar(20) NULL,
    [Loc_Country_Code] nvarchar(10) NULL,
    [Loc_Country_Name] nvarchar(100) NULL,
    [Loc_Address_Components_Json] nvarchar(max) NULL,
    [Bill_Place_Id] nvarchar(255) NULL,
    [Bill_Formatted_Address] nvarchar(1000) NULL,
    [Bill_Lat] decimal(9,6) NULL,
    [Bill_Lng] decimal(9,6) NULL,
    [Bill_Street_Number] nvarchar(50) NULL,
    [Bill_Route] nvarchar(250) NULL,
    [Bill_Subpremise] nvarchar(100) NULL,
    [Bill_Locality] nvarchar(250) NULL,
    [Bill_Admin_Area_1] nvarchar(250) NULL,
    [Bill_Admin_Area_2] nvarchar(250) NULL,
    [Bill_Postal_Code] nvarchar(20) NULL,
    [Bill_Country_Code] nvarchar(10) NULL,
    [Bill_Country_Name] nvarchar(100) NULL,
    [Bill_Address_Components_Json] nvarchar(max) NULL,
    [TextContractRef] nvarchar(500) NULL,
    [TextJobsideNotes] nvarchar(500) NULL,
    [LinBranch] bigint NOT NULL
  );
  PRINT 'CREATE TABLE TSql_Jobside';
END
GO
IF OBJECT_ID(N'dbo.TSql_Offers', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Offers] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [AddOfferNumber] nvarchar(500) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [AddDescription] nvarchar(max) NULL,
    [LinkJobside] bigint NOT NULL,
    [LinkClient_V2] bigint NOT NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_Offers_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_Offers_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_Offers_AddDateMade] DEFAULT (getdate()),
    [AddChangeBy] nvarchar(128) NOT NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_Offers_Ntimeschanged] DEFAULT ((0)),
    [LinkOfferState] bigint NOT NULL
  );
  PRINT 'CREATE TABLE TSql_Offers';
END
GO
IF OBJECT_ID(N'dbo.TSql_OfferState', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_OfferState] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [AddColor] nvarchar(50) NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_OfferState_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_OfferState_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_OfferState_AddDateMade] DEFAULT (getdate()),
    [AddChangeBy] nvarchar(128) NOT NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_OfferState_Ntimeschanged] DEFAULT ((0))
  );
  PRINT 'CREATE TABLE TSql_OfferState';
END
GO
IF OBJECT_ID(N'dbo.TSql_Plantilla', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Plantilla] (
    [SysObjectID] bigint IDENTITY(1,1) NOT NULL,
    [AttName] nvarchar(150) NOT NULL,
    [AttColor] nvarchar(20) NOT NULL CONSTRAINT [DF_TSql_Plantilla_AttColor] DEFAULT ('#349d7d'),
    [AttLogo] nvarchar(500) NOT NULL CONSTRAINT [DF_TSql_Plantilla_AttLogo] DEFAULT ('/Content/images/Login/at.png'),
    [AttIsDefault] bit NOT NULL CONSTRAINT [DF_TSql_Plantilla_AttIsDefault] DEFAULT ((0)),
    [AttIsDeleted] bit NOT NULL CONSTRAINT [DF_TSql_Plantilla_AttIsDeleted] DEFAULT ((0)),
    [LinCreatedBy] nvarchar(128) NULL,
    [AttCreated] datetime NOT NULL CONSTRAINT [DF_TSql_Plantilla_AttCreated] DEFAULT (getdate()),
    [LinModifiedBy] nvarchar(128) NULL,
    [AttLastModification] datetime NOT NULL CONSTRAINT [DF_TSql_Plantilla_AttLastModification] DEFAULT (getdate()),
    [SysUpdateNumber] bigint NOT NULL CONSTRAINT [DF_TSql_Plantilla_SysUpdateNumber] DEFAULT ((0)),
    [AttFavicon] nvarchar(500) NULL,
    [AttBrandText] nvarchar(120) NOT NULL CONSTRAINT [DF_TSql_Plantilla_AttBrandText] DEFAULT (N'T Desing.net'),
    [AttBrandTextColor] nvarchar(20) NULL,
    [AttBrandAccentColor] nvarchar(20) NOT NULL CONSTRAINT [DF_TSql_Plantilla_AttBrandAccentColor] DEFAULT (N'#f29100'),
    [AttLinkColor] nvarchar(20) NULL
  );
  PRINT 'CREATE TABLE TSql_Plantilla';
END
GO
IF OBJECT_ID(N'dbo.TSql_PluginDeviceAuth', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_PluginDeviceAuth] (
    [SysObjectID] bigint IDENTITY(1,1) NOT NULL,
    [DeviceId] nvarchar(128) NOT NULL,
    [LinAspNetUsert] nvarchar(128) NULL,
    [MachineName] nvarchar(128) NULL,
    [UsuarioWindows] nvarchar(128) NULL,
    [PluginVersion] nvarchar(50) NULL,
    [Allowed] bit NOT NULL CONSTRAINT [DF_TSql_PluginDeviceAuth_Allowed] DEFAULT ((1)),
    [IsActive] bit NOT NULL CONSTRAINT [DF_TSql_PluginDeviceAuth_IsActive] DEFAULT ((1)),
    [IsRevoked] bit NOT NULL CONSTRAINT [DF_TSql_PluginDeviceAuth_IsRevoked] DEFAULT ((0)),
    [Estado] nvarchar(20) NULL,
    [AttIsDeleted] bit NOT NULL CONSTRAINT [DF_TSql_PluginDeviceAuth_AttIsDeleted] DEFAULT ((0)),
    [LastCheckUtc] datetime NULL,
    [LinCreatedBy] nvarchar(128) NULL,
    [AttCreated] datetime NOT NULL CONSTRAINT [DF_TSql_PluginDeviceAuth_AttCreated] DEFAULT (getutcdate()),
    [LinModifiedBy] nvarchar(128) NULL,
    [AttLastModification] datetime NOT NULL CONSTRAINT [DF_TSql_PluginDeviceAuth_AttLastModification] DEFAULT (getutcdate())
  );
  PRINT 'CREATE TABLE TSql_PluginDeviceAuth';
END
GO
IF OBJECT_ID(N'dbo.TSql_TelegramDesignAccess', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_TelegramDesignAccess] (
    [SysObjectID] bigint IDENTITY(1,1) NOT NULL,
    [LinDesign] bigint NOT NULL,
    [TelegramChatId] nvarchar(64) NULL,
    [TelegramUserId] nvarchar(64) NULL,
    [Alias] nvarchar(150) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_TSql_TelegramDesignAccess_IsActive] DEFAULT ((1)),
    [FechaRegistroUtc] datetime2(0) NOT NULL CONSTRAINT [DF_TSql_TelegramDesignAccess_Fecha] DEFAULT (sysutcdatetime())
  );
  PRINT 'CREATE TABLE TSql_TelegramDesignAccess';
END
GO
IF OBJECT_ID(N'dbo.TSql_TelegramDesignInbox', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_TelegramDesignInbox] (
    [SysObjectID] bigint IDENTITY(1,1) NOT NULL,
    [LinDesign] bigint NOT NULL,
    [TelegramMessageId] nvarchar(64) NULL,
    [TelegramChatId] nvarchar(64) NULL,
    [TelegramUserId] nvarchar(64) NULL,
    [TelegramUserName] nvarchar(120) NULL,
    [Caption] nvarchar(1000) NULL,
    [FileId] nvarchar(255) NOT NULL,
    [FileUniqueId] nvarchar(255) NULL,
    [WidthPx] int NULL,
    [HeightPx] int NULL,
    [Estado] nvarchar(40) NOT NULL CONSTRAINT [DF_TSql_TelegramDesignInbox_Estado] DEFAULT ('Pendiente'),
    [FechaMensajeUtc] datetime2(0) NOT NULL,
    [FechaRegistroUtc] datetime2(0) NOT NULL CONSTRAINT [DF_TSql_TelegramDesignInbox_FechaRegistro] DEFAULT (sysutcdatetime())
  );
  PRINT 'CREATE TABLE TSql_TelegramDesignInbox';
END
GO
IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_UiTranslation] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextResourceKey] nvarchar(256) NOT NULL,
    [TextModule] nvarchar(100) NULL,
    [LinkLanguage] bigint NOT NULL,
    [TextValue] nvarchar(max) NOT NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_UiTranslation_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_UiTranslation_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_UiTranslation_AddDateMade] DEFAULT (getdate()),
    [AddLastDateChange] datetime NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_UiTranslation_Ntimeschanged] DEFAULT ((0)),
    [TextModuleNorm] AS (isnull([TextModule],N'')) PERSISTED
  );
  PRINT 'CREATE TABLE TSql_UiTranslation';
END
GO
IF OBJECT_ID(N'dbo.TSql_XrDevice', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_XrDevice] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [TextDeviceType] nvarchar(50) NOT NULL,
    [TextPairingCode] nvarchar(50) NOT NULL,
    [TextNotes] nvarchar(500) NULL,
    [Is_Paired] bit NOT NULL CONSTRAINT [DF_TSql_XrDevice_Is_Paired] DEFAULT ((0)),
    [DateLastSeen] datetime NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_XrDevice_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_XrDevice_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_XrDevice_AddDateMade] DEFAULT (getdate()),
    [AddLastDateChange] datetime NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_XrDevice_Ntimeschanged] DEFAULT ((0))
  );
  PRINT 'CREATE TABLE TSql_XrDevice';
END
GO
IF OBJECT_ID(N'dbo.TSql_XrPushJob', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_XrPushJob] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [LinkXrDevice] bigint NOT NULL,
    [LinkDesign] bigint NOT NULL,
    [LinkOffer] bigint NULL,
    [TextStatus] nvarchar(50) NOT NULL CONSTRAINT [DF_TSql_XrPushJob_TextStatus] DEFAULT (N'Pending'),
    [DateDelivered] datetime NULL,
    [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_XrPushJob_Is_Delete] DEFAULT ((0)),
    [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_XrPushJob_Is_Active] DEFAULT ((1)),
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NULL,
    [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_XrPushJob_AddDateMade] DEFAULT (getdate()),
    [AddLastDateChange] datetime NULL,
    [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_XrPushJob_Ntimeschanged] DEFAULT ((0))
  );
  PRINT 'CREATE TABLE TSql_XrPushJob';
END
GO

/* ===== 2. Columnas nuevas en tablas existentes ===== */
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'AddLetter') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [AddLetter] nchar(2) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'AttColor') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [AttColor] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Address_Components_Json') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Address_Components_Json] nvarchar(max) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Admin_Area_1') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Admin_Area_1] nvarchar(250) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Admin_Area_2') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Admin_Area_2] nvarchar(250) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Country_Code') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Country_Code] nvarchar(10) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Country_Name') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Country_Name] nvarchar(100) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Formatted_Address') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Formatted_Address] nvarchar(1000) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Lat') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Lat] decimal(9,6) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Lng') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Lng] decimal(9,6) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Locality') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Locality] nvarchar(250) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Place_Id') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Place_Id] nvarchar(255) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Postal_Code') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Postal_Code] nvarchar(20) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Route') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Route] nvarchar(250) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Street_Number') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Street_Number] nvarchar(50) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Subpremise') IS NULL
  ALTER TABLE dbo.[TSql_Branch] ADD [Loc_Subpremise] nvarchar(100) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'AddLetter') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [AddLetter] nchar(2) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'LinkLanguage') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [LinkLanguage] bigint NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'LinPlantilla') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [LinPlantilla] bigint NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Address_Components_Json') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Address_Components_Json] nvarchar(max) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Admin_Area_1') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Admin_Area_1] nvarchar(250) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Admin_Area_2') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Admin_Area_2] nvarchar(250) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Country_Code') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Country_Code] nvarchar(10) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Country_Name') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Country_Name] nvarchar(100) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Formatted_Address') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Formatted_Address] nvarchar(1000) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Lat') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Lat] decimal(9,6) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Lng') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Lng] decimal(9,6) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Locality') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Locality] nvarchar(250) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Place_Id') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Place_Id] nvarchar(255) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Postal_Code') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Postal_Code] nvarchar(20) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Route') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Route] nvarchar(250) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Street_Number') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Street_Number] nvarchar(50) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Company', N'Loc_Subpremise') IS NULL
  ALTER TABLE dbo.[TSql_Company] ADD [Loc_Subpremise] nvarchar(100) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'Is_Special') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [Is_Special] bit NOT NULL CONSTRAINT [DF_TSql_DesignWall_Is_Special] DEFAULT ((0)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveCenterX') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [NumberCurveCenterX] float NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveCenterY') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [NumberCurveCenterY] float NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveCenterZ') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [NumberCurveCenterZ] float NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveFractionDen') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [NumberCurveFractionDen] int NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveFractionNum') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [NumberCurveFractionNum] int NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveModulation') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [NumberCurveModulation] int NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveOuterRadius') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [NumberCurveOuterRadius] float NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveStartAngle') IS NULL
  ALTER TABLE dbo.[TSql_DesignWall] ADD [NumberCurveStartAngle] float NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'AddDateMade') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_DocumentType_AddDateMade] DEFAULT (getdate()) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'AddLastDateChange') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [AddLastDateChange] datetime NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'IdObject') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [IdObject] bigint IDENTITY(1,1) NOT NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'Is_Active') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_DocumentType_Is_Active] DEFAULT ((1)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'Is_Delete') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_DocumentType_Is_Delete] DEFAULT ((0)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'LinkMadeBy') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [LinkMadeBy] nvarchar(128) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [LinkMadeBy] nvarchar(128) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'LinModifiedBy') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [LinModifiedBy] nvarchar(128) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'Ntimeschanged') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_DocumentType_Ntimeschanged] DEFAULT ((0)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'NumberMaxFileSizeBytes') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [NumberMaxFileSizeBytes] bigint NOT NULL CONSTRAINT [DF_TSql_DocumentType_NumberMaxFileSizeBytes] DEFAULT ((10485760)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'TextCode') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [TextCode] nvarchar(50) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'TextDescription') IS NULL
  ALTER TABLE dbo.[TSql_DocumentType] ADD [TextDescription] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'TextLabel') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [TextLabel] nvarchar(500) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [TextLabel] nvarchar(500) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'AddDateMade') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [AddDateMade] datetime NOT NULL CONSTRAINT [DF_TSql_Language_AddDateMade] DEFAULT (getdate()) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'AddLastDateChange') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [AddLastDateChange] datetime NULL;
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'IdObject') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [IdObject] bigint IDENTITY(1,1) NOT NULL;
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'Is_Active') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [Is_Active] bit NOT NULL CONSTRAINT [DF_TSql_Language_Is_Active] DEFAULT ((1)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'Is_Default') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [Is_Default] bit NOT NULL CONSTRAINT [DF_TSql_Language_Is_Default] DEFAULT ((0)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'Is_Delete') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [Is_Delete] bit NOT NULL CONSTRAINT [DF_TSql_Language_Is_Delete] DEFAULT ((0)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'LinkCountry') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [LinkCountry] bigint NULL;
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'LinkMadeBy') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_language])
    ALTER TABLE dbo.[TSql_language] ADD [LinkMadeBy] nvarchar(128) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_language] ADD [LinkMadeBy] nvarchar(128) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'Ntimeschanged') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [Ntimeschanged] bigint NOT NULL CONSTRAINT [DF_TSql_Language_Ntimeschanged] DEFAULT ((0)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'TextCode') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_language])
    ALTER TABLE dbo.[TSql_language] ADD [TextCode] nvarchar(20) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_language] ADD [TextCode] nvarchar(20) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'TextLabel') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_language])
    ALTER TABLE dbo.[TSql_language] ADD [TextLabel] nvarchar(500) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_language] ADD [TextLabel] nvarchar(500) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_language', N'TextNativeName') IS NULL
  ALTER TABLE dbo.[TSql_language] ADD [TextNativeName] nvarchar(100) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'IInsertinMaterArticles') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [IInsertinMaterArticles] bit NOT NULL CONSTRAINT [DF_Tsql_Master_Articles_IInsertinMaterArticles] DEFAULT ((0)) WITH VALUES;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwg3D') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwg3D] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwg3DRef') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwg3DRef] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgHorizontalElevation3D') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgHorizontalElevation3D] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgHorizontalElevation3ds') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgHorizontalElevation3ds] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgHorizontalElevationMock-up') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgHorizontalElevationMock-up] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgPlant3D') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgPlant3D] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgPlant3ds') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgPlant3ds] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgPlantMck-up') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgPlantMck-up] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgVerticalElevation3D') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgVerticalElevation3D] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgVerticalElevation3ds') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgVerticalElevation3ds] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgVerticalElevationMock-up') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgVerticalElevationMock-up] nvarchar(500) NULL;
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgXr') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [LinkBlockDwgXr] nvarchar(500) NULL;
GO

/* ===== 3. Defaults que faltan en columnas existentes ===== */

/* ===== 4. Claves primarias, únicas e índices ===== */
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.AA01Master') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[AA01Master] ADD CONSTRAINT [PK_AA01Master] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY AA01Master.PK_AA01Master: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_Client_V2') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_Client_V2] ADD CONSTRAINT [PK_TSql_Client_V2] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_Client_V2.PK_TSql_Client_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_ConfigDesingDefault') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_ConfigDesingDefault] ADD CONSTRAINT [PK_TSql_ConfigDesingDefault] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_ConfigDesingDefault.PK_TSql_ConfigDesingDefault: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_Design_V2') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_Design_V2] ADD CONSTRAINT [PK_TSql_Design_V2] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_Design_V2.PK_TSql_Design_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_DesignWallArticle') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesignWallArticle] ADD CONSTRAINT [PK_TSql_DesignWallArticle] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_DesignWallArticle.PK_TSql_DesignWallArticle: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_Document') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_Document] ADD CONSTRAINT [PK_TSql_Document] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_Document.PK_TSql_Document: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_DocumentType') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_DocumentType] ADD CONSTRAINT [PK_TSql_DocumentType] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_DocumentType.PK_TSql_DocumentType: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_DocumentTypeExtension') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_DocumentTypeExtension] ADD CONSTRAINT [PK_TSql_DocumentTypeExtension] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_DocumentTypeExtension.PK_TSql_DocumentTypeExtension: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_Extension') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_Extension] ADD CONSTRAINT [PK_TSql_Extension] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_Extension.PK_TSql_Extension: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_Jobside') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_Jobside] ADD CONSTRAINT [PK_TSql_Jobside] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_Jobside.PK_TSql_Jobside: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_language') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_language] ADD CONSTRAINT [PK_TSql_Language] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_language.PK_TSql_Language: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_Offers') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_Offers] ADD CONSTRAINT [PK_TSql_Offers] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_Offers.PK_TSql_Offers: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_OfferState') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_OfferState] ADD CONSTRAINT [PK_TSql_OfferState] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_OfferState.PK_TSql_OfferState: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_Plantilla') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_Plantilla] ADD CONSTRAINT [PK_TSql_Plantilla] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_Plantilla.PK_TSql_Plantilla: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_PluginDeviceAuth') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_PluginDeviceAuth] ADD CONSTRAINT [PK_TSql_PluginDeviceAuth] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_PluginDeviceAuth.PK_TSql_PluginDeviceAuth: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_TelegramDesignAccess') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_TelegramDesignAccess] ADD CONSTRAINT [PK__TSql_Tel__C1E4E6CE727B687A] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_TelegramDesignAccess.PK__TSql_Tel__C1E4E6CE727B687A: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_TelegramDesignInbox') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_TelegramDesignInbox] ADD CONSTRAINT [PK__TSql_Tel__C1E4E6CE0028CDCB] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_TelegramDesignInbox.PK__TSql_Tel__C1E4E6CE0028CDCB: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_UiTranslation') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_UiTranslation] ADD CONSTRAINT [PK_TSql_UiTranslation] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_UiTranslation.PK_TSql_UiTranslation: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_XrDevice') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_XrDevice] ADD CONSTRAINT [PK_TSql_XrDevice] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_XrDevice.PK_TSql_XrDevice: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_XrPushJob') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_XrPushJob] ADD CONSTRAINT [PK_TSql_XrPushJob] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_XrPushJob.PK_TSql_XrPushJob: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_Client_V2') AND name=N'IX_TSql_Client_V2_Loc_Place_Id')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_Client_V2_Loc_Place_Id] ON dbo.[TSql_Client_V2] ([Loc_Place_Id] ASC) WHERE ([Loc_Place_Id] IS NOT NULL);
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_Client_V2.IX_TSql_Client_V2_Loc_Place_Id: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_Client_V2') AND name=N'IX_TSql_Client_V2_TextCode')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_Client_V2_TextCode] ON dbo.[TSql_Client_V2] ([TextCode] ASC) WHERE ([TextCode] IS NOT NULL AND [Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_Client_V2.IX_TSql_Client_V2_TextCode: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_Client_V2') AND name=N'IX_TSql_Client_V2_TextLabel_Active')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_Client_V2_TextLabel_Active] ON dbo.[TSql_Client_V2] ([TextLabel] ASC) INCLUDE ([Is_Active], [Path_Ico], [Path_Logo]) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_Client_V2.IX_TSql_Client_V2_TextLabel_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_Company') AND name=N'IX_TSql_Company_Loc_Place_Id')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_Company_Loc_Place_Id] ON dbo.[TSql_Company] ([Loc_Place_Id] ASC) WHERE ([Loc_Place_Id] IS NOT NULL);
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_Company.IX_TSql_Company_Loc_Place_Id: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_DesignWallArticle') AND name=N'IX_TSql_DesignWallArticle_LinkDesign_V2_Active')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_DesignWallArticle_LinkDesign_V2_Active] ON dbo.[TSql_DesignWallArticle] ([LinkDesign_V2] ASC) INCLUDE ([LinkDesignWall], [Is_Active]) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_DesignWallArticle.IX_TSql_DesignWallArticle_LinkDesign_V2_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_DesignWallArticle') AND name=N'IX_TSql_DesignWallArticle_LinkDesignWall_Active')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_DesignWallArticle_LinkDesignWall_Active] ON dbo.[TSql_DesignWallArticle] ([LinkDesignWall] ASC, [NumberSequence] ASC) INCLUDE ([TextCode], [TextView], [Is_Active]) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_DesignWallArticle.IX_TSql_DesignWallArticle_LinkDesignWall_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_DesignWallArticle') AND name=N'IX_TSql_DesignWallArticle_Phase')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_DesignWallArticle_Phase] ON dbo.[TSql_DesignWallArticle] ([LinkDesign_V2] ASC, [NumberPhase] ASC) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_DesignWallArticle.IX_TSql_DesignWallArticle_Phase: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_DocumentType') AND name=N'IX_TSql_DocumentType_TextCode')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_DocumentType_TextCode] ON dbo.[TSql_DocumentType] ([TextCode] ASC) WHERE ([TextCode] IS NOT NULL AND [Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_DocumentType.IX_TSql_DocumentType_TextCode: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_DocumentType') AND name=N'IX_TSql_DocumentType_TextLabel_Active')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_DocumentType_TextLabel_Active] ON dbo.[TSql_DocumentType] ([TextLabel] ASC) INCLUDE ([Is_Active], [TextCode]) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_DocumentType.IX_TSql_DocumentType_TextLabel_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_Jobside') AND name=N'IX_TSql_Jobside_LinkClient_V2')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_Jobside_LinkClient_V2] ON dbo.[TSql_Jobside] ([LinkClient_V2] ASC) WHERE ([LinkClient_V2] IS NOT NULL AND [Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_Jobside.IX_TSql_Jobside_LinkClient_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_Jobside') AND name=N'IX_TSql_Jobside_Loc_Place_Id')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_Jobside_Loc_Place_Id] ON dbo.[TSql_Jobside] ([Loc_Place_Id] ASC) WHERE ([Loc_Place_Id] IS NOT NULL);
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_Jobside.IX_TSql_Jobside_Loc_Place_Id: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_language') AND name=N'IX_TSql_Language_TextLabel_Active')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_Language_TextLabel_Active] ON dbo.[TSql_language] ([TextLabel] ASC) INCLUDE ([Is_Active], [TextCode]) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_language.IX_TSql_Language_TextLabel_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_language') AND name=N'UX_TSql_Language_Default_Active')
BEGIN TRY
  CREATE UNIQUE NONCLUSTERED INDEX [UX_TSql_Language_Default_Active] ON dbo.[TSql_language] ([Is_Default] ASC) WHERE ([Is_Default]=(1) AND [Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_language.UX_TSql_Language_Default_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_language') AND name=N'UX_TSql_Language_TextCode_Active')
BEGIN TRY
  CREATE UNIQUE NONCLUSTERED INDEX [UX_TSql_Language_TextCode_Active] ON dbo.[TSql_language] ([TextCode] ASC) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_language.UX_TSql_Language_TextCode_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_Plantilla') AND name=N'UX_TSql_Plantilla_Default')
BEGIN TRY
  CREATE UNIQUE NONCLUSTERED INDEX [UX_TSql_Plantilla_Default] ON dbo.[TSql_Plantilla] ([AttIsDefault] ASC) WHERE ([AttIsDefault]=(1) AND [AttIsDeleted]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_Plantilla.UX_TSql_Plantilla_Default: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_PluginDeviceAuth') AND name=N'UX_TSql_PluginDeviceAuth_DeviceId')
BEGIN TRY
  CREATE UNIQUE NONCLUSTERED INDEX [UX_TSql_PluginDeviceAuth_DeviceId] ON dbo.[TSql_PluginDeviceAuth] ([DeviceId] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_PluginDeviceAuth.UX_TSql_PluginDeviceAuth_DeviceId: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_TelegramDesignAccess') AND name=N'IX_TSql_TelegramDesignAccess_LinDesign_Active')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_TelegramDesignAccess_LinDesign_Active] ON dbo.[TSql_TelegramDesignAccess] ([LinDesign] ASC, [IsActive] ASC, [TelegramChatId] ASC, [TelegramUserId] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_TelegramDesignAccess.IX_TSql_TelegramDesignAccess_LinDesign_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_TelegramDesignInbox') AND name=N'IX_TSql_TelegramDesignInbox_LinDesign_Estado')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_TelegramDesignInbox_LinDesign_Estado] ON dbo.[TSql_TelegramDesignInbox] ([LinDesign] ASC, [Estado] ASC, [FechaRegistroUtc] DESC);
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_TelegramDesignInbox.IX_TSql_TelegramDesignInbox_LinDesign_Estado: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_UiTranslation') AND name=N'IX_TSql_UiTranslation_Lookup')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_UiTranslation_Lookup] ON dbo.[TSql_UiTranslation] ([TextResourceKey] ASC, [LinkLanguage] ASC) INCLUDE ([TextValue], [TextModule]) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_UiTranslation.IX_TSql_UiTranslation_Lookup: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_UiTranslation') AND name=N'IX_TSql_UiTranslation_Module_Active')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_UiTranslation_Module_Active] ON dbo.[TSql_UiTranslation] ([TextModule] ASC, [TextResourceKey] ASC) WHERE ([Is_Delete]=(0) AND [TextModule] IS NOT NULL);
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_UiTranslation.IX_TSql_UiTranslation_Module_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_UiTranslation') AND name=N'UX_TSql_UiTranslation_Key_Language_Module_Active')
BEGIN TRY
  CREATE UNIQUE NONCLUSTERED INDEX [UX_TSql_UiTranslation_Key_Language_Module_Active] ON dbo.[TSql_UiTranslation] ([TextResourceKey] ASC, [LinkLanguage] ASC, [TextModuleNorm] ASC) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_UiTranslation.UX_TSql_UiTranslation_Key_Language_Module_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_XrDevice') AND name=N'IX_TSql_XrDevice_TextLabel_Active')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_XrDevice_TextLabel_Active] ON dbo.[TSql_XrDevice] ([TextLabel] ASC) INCLUDE ([Is_Active], [TextDeviceType]) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_XrDevice.IX_TSql_XrDevice_TextLabel_Active: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_XrDevice') AND name=N'UX_TSql_XrDevice_TextPairingCode')
BEGIN TRY
  CREATE UNIQUE NONCLUSTERED INDEX [UX_TSql_XrDevice_TextPairingCode] ON dbo.[TSql_XrDevice] ([TextPairingCode] ASC) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_XrDevice.UX_TSql_XrDevice_TextPairingCode: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TSql_XrPushJob') AND name=N'IX_TSql_XrPushJob_Device_Pending')
BEGIN TRY
  CREATE NONCLUSTERED INDEX [IX_TSql_XrPushJob_Device_Pending] ON dbo.[TSql_XrPushJob] ([LinkXrDevice] ASC, [TextStatus] ASC, [AddDateMade] ASC) WHERE ([Is_Delete]=(0));
END TRY
BEGIN CATCH
  PRINT 'SKIP INDEX TSql_XrPushJob.IX_TSql_XrPushJob_Device_Pending: ' + ERROR_MESSAGE();
END CATCH
GO

/* ===== 5. Check constraints ===== */

/* ===== 6. Foreign keys (si hay huérfanos se omite y se informa) ===== */
IF OBJECT_ID(N'dbo.FK_TSql_Branch_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Branch] WITH CHECK ADD CONSTRAINT [FK_TSql_Branch_AspNetUsers] FOREIGN KEY ([LinCreatedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Branch_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Branch_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Branch_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Branch] WITH CHECK ADD CONSTRAINT [FK_TSql_Branch_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Branch_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Branch_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Branch_TSql_Company1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Branch] WITH CHECK ADD CONSTRAINT [FK_TSql_Branch_TSql_Company1] FOREIGN KEY ([LinCompany]) REFERENCES dbo.[TSql_Company] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_Branch_TSql_Company1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Branch_TSql_Company1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Client_V2_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Client_V2] WITH CHECK ADD CONSTRAINT [FK_TSql_Client_V2_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Client_V2_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Client_V2_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Client_V2_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Client_V2] WITH CHECK ADD CONSTRAINT [FK_TSql_Client_V2_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Client_V2_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Client_V2_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Company_TSql_Language', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Company] WITH CHECK ADD CONSTRAINT [FK_TSql_Company_TSql_Language] FOREIGN KEY ([LinkLanguage]) REFERENCES dbo.[TSql_Language] ([IdObject]);
  PRINT 'FK OK FK_TSql_Company_TSql_Language';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Company_TSql_Language: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Company_TSql_Plantilla', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Company] WITH CHECK ADD CONSTRAINT [FK_TSql_Company_TSql_Plantilla] FOREIGN KEY ([LinPlantilla]) REFERENCES dbo.[TSql_Plantilla] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_Company_TSql_Plantilla';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Company_TSql_Plantilla: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesgloseGrupo_TSql_Design_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesgloseGrupo] WITH CHECK ADD CONSTRAINT [FK_TSql_DesgloseGrupo_TSql_Design_V2] FOREIGN KEY ([LinkDesign_V2]) REFERENCES dbo.[TSql_Design_V2] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesgloseGrupo_TSql_Design_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesgloseGrupo_TSql_Design_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesgloseOfertaCliente_TSql_Client_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesgloseOfertaCliente] WITH CHECK ADD CONSTRAINT [FK_TSql_DesgloseOfertaCliente_TSql_Client_V2] FOREIGN KEY ([LinkClient_V2]) REFERENCES dbo.[TSql_Client_V2] ([IdObject]);
  PRINT 'FK OK FK_TSql_DesgloseOfertaCliente_TSql_Client_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesgloseOfertaCliente_TSql_Client_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesgloseOfertaCliente_TSql_Design_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesgloseOfertaCliente] WITH CHECK ADD CONSTRAINT [FK_TSql_DesgloseOfertaCliente_TSql_Design_V2] FOREIGN KEY ([LinkDesign_V2]) REFERENCES dbo.[TSql_Design_V2] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesgloseOfertaCliente_TSql_Design_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesgloseOfertaCliente_TSql_Design_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesgloseOfertaCliente_TSql_Offers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesgloseOfertaCliente] WITH CHECK ADD CONSTRAINT [FK_TSql_DesgloseOfertaCliente_TSql_Offers] FOREIGN KEY ([LinkOffers]) REFERENCES dbo.[TSql_Offers] ([IdObject]);
  PRINT 'FK OK FK_TSql_DesgloseOfertaCliente_TSql_Offers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesgloseOfertaCliente_TSql_Offers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Design_V2_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Design_V2] WITH CHECK ADD CONSTRAINT [FK_TSql_Design_V2_AspNetUsers] FOREIGN KEY ([LinCreatedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Design_V2_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Design_V2_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Design_V2_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Design_V2] WITH CHECK ADD CONSTRAINT [FK_TSql_Design_V2_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Design_V2_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Design_V2_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Design_V2_TSql_Offers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Design_V2] WITH CHECK ADD CONSTRAINT [FK_TSql_Design_V2_TSql_Offers] FOREIGN KEY ([LinkOffers]) REFERENCES dbo.[TSql_Offers] ([IdObject]);
  PRINT 'FK OK FK_TSql_Design_V2_TSql_Offers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Design_V2_TSql_Offers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesignDetails_TSql_Design', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesignDetails] WITH CHECK ADD CONSTRAINT [FK_TSql_DesignDetails_TSql_Design] FOREIGN KEY ([LinDesaing]) REFERENCES dbo.[TSql_Design] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesignDetails_TSql_Design';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesignDetails_TSql_Design: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesignWall_TSql_Design_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesignWall] WITH CHECK ADD CONSTRAINT [FK_TSql_DesignWall_TSql_Design_V2] FOREIGN KEY ([LinkDesign_V2]) REFERENCES dbo.[TSql_Design_V2] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesignWall_TSql_Design_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesignWall_TSql_Design_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesignWallArticle_TSql_Design_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesignWallArticle] WITH CHECK ADD CONSTRAINT [FK_TSql_DesignWallArticle_TSql_Design_V2] FOREIGN KEY ([LinkDesign_V2]) REFERENCES dbo.[TSql_Design_V2] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesignWallArticle_TSql_Design_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesignWallArticle_TSql_Design_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesignWallArticle_TSql_DesignWall', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesignWallArticle] WITH CHECK ADD CONSTRAINT [FK_TSql_DesignWallArticle_TSql_DesignWall] FOREIGN KEY ([LinkDesignWall]) REFERENCES dbo.[TSql_DesignWall] ([IdObject]);
  PRINT 'FK OK FK_TSql_DesignWallArticle_TSql_DesignWall';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesignWallArticle_TSql_DesignWall: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesignWallArticle_Tsql_Master_Articles', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesignWallArticle] WITH CHECK ADD CONSTRAINT [FK_TSql_DesignWallArticle_Tsql_Master_Articles] FOREIGN KEY ([LinkMasterArticle]) REFERENCES dbo.[Tsql_Master_Articles] ([IdObject]);
  PRINT 'FK OK FK_TSql_DesignWallArticle_Tsql_Master_Articles';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesignWallArticle_Tsql_Master_Articles: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesingFasesDetail_TSql_Design_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingFasesDetail] WITH CHECK ADD CONSTRAINT [FK_TSql_DesingFasesDetail_TSql_Design_V2] FOREIGN KEY ([LinkDesign_V2]) REFERENCES dbo.[TSql_Design_V2] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesingFasesDetail_TSql_Design_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesingFasesDetail_TSql_Design_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Document_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Document] WITH CHECK ADD CONSTRAINT [FK_TSql_Document_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Document_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Document_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Document_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Document] WITH CHECK ADD CONSTRAINT [FK_TSql_Document_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Document_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Document_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Document_TSql_Client_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Document] WITH CHECK ADD CONSTRAINT [FK_TSql_Document_TSql_Client_V2] FOREIGN KEY ([LinkClient_V2]) REFERENCES dbo.[TSql_Client_V2] ([IdObject]);
  PRINT 'FK OK FK_TSql_Document_TSql_Client_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Document_TSql_Client_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Document_TSql_DocumentType', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Document] WITH CHECK ADD CONSTRAINT [FK_TSql_Document_TSql_DocumentType] FOREIGN KEY ([LinkDocumentType]) REFERENCES dbo.[TSql_DocumentType] ([IdObject]);
  PRINT 'FK OK FK_TSql_Document_TSql_DocumentType';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Document_TSql_DocumentType: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Document_TSql_DocumentTypeExtension', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Document] WITH CHECK ADD CONSTRAINT [FK_TSql_Document_TSql_DocumentTypeExtension] FOREIGN KEY ([LinkExtension]) REFERENCES dbo.[TSql_DocumentTypeExtension] ([IdObject]);
  PRINT 'FK OK FK_TSql_Document_TSql_DocumentTypeExtension';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Document_TSql_DocumentTypeExtension: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Document_TSql_Jobside', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Document] WITH CHECK ADD CONSTRAINT [FK_TSql_Document_TSql_Jobside] FOREIGN KEY ([LinkJobside]) REFERENCES dbo.[TSql_Jobside] ([IdObject]);
  PRINT 'FK OK FK_TSql_Document_TSql_Jobside';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Document_TSql_Jobside: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Document_TSql_Offers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Document] WITH CHECK ADD CONSTRAINT [FK_TSql_Document_TSql_Offers] FOREIGN KEY ([LinkOffer]) REFERENCES dbo.[TSql_Offers] ([IdObject]);
  PRINT 'FK OK FK_TSql_Document_TSql_Offers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Document_TSql_Offers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DocumentType_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DocumentType] WITH CHECK ADD CONSTRAINT [FK_TSql_DocumentType_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_DocumentType_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DocumentType_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DocumentType_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DocumentType] WITH CHECK ADD CONSTRAINT [FK_TSql_DocumentType_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_DocumentType_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DocumentType_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DocumentTypeExtension_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DocumentTypeExtension] WITH CHECK ADD CONSTRAINT [FK_TSql_DocumentTypeExtension_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_DocumentTypeExtension_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DocumentTypeExtension_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DocumentTypeExtension_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DocumentTypeExtension] WITH CHECK ADD CONSTRAINT [FK_TSql_DocumentTypeExtension_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_DocumentTypeExtension_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DocumentTypeExtension_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DocumentTypeExtension_TSql_DocumentType', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DocumentTypeExtension] WITH CHECK ADD CONSTRAINT [FK_TSql_DocumentTypeExtension_TSql_DocumentType] FOREIGN KEY ([LinkDocumentType]) REFERENCES dbo.[TSql_DocumentType] ([IdObject]);
  PRINT 'FK OK FK_TSql_DocumentTypeExtension_TSql_DocumentType';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DocumentTypeExtension_TSql_DocumentType: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DocumentTypeExtension_TSql_Extension', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DocumentTypeExtension] WITH CHECK ADD CONSTRAINT [FK_TSql_DocumentTypeExtension_TSql_Extension] FOREIGN KEY ([LinkExtension]) REFERENCES dbo.[TSql_Extension] ([IdObject]);
  PRINT 'FK OK FK_TSql_DocumentTypeExtension_TSql_Extension';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DocumentTypeExtension_TSql_Extension: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Extension_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Extension] WITH CHECK ADD CONSTRAINT [FK_TSql_Extension_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Extension_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Extension_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Extension_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Extension] WITH CHECK ADD CONSTRAINT [FK_TSql_Extension_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Extension_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Extension_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Extension_AspNetUsers2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Extension] WITH CHECK ADD CONSTRAINT [FK_TSql_Extension_AspNetUsers2] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Extension_AspNetUsers2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Extension_AspNetUsers2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Extension_AspNetUsers3', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Extension] WITH CHECK ADD CONSTRAINT [FK_TSql_Extension_AspNetUsers3] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Extension_AspNetUsers3';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Extension_AspNetUsers3: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Jobside_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Jobside] WITH CHECK ADD CONSTRAINT [FK_TSql_Jobside_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Jobside_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Jobside_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Jobside_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Jobside] WITH CHECK ADD CONSTRAINT [FK_TSql_Jobside_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Jobside_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Jobside_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Jobside_TSql_Branch', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Jobside] WITH CHECK ADD CONSTRAINT [FK_TSql_Jobside_TSql_Branch] FOREIGN KEY ([LinBranch]) REFERENCES dbo.[TSql_Branch] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_Jobside_TSql_Branch';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Jobside_TSql_Branch: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Jobside_TSql_Client_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Jobside] WITH CHECK ADD CONSTRAINT [FK_TSql_Jobside_TSql_Client_V2] FOREIGN KEY ([LinkClient_V2]) REFERENCES dbo.[TSql_Client_V2] ([IdObject]);
  PRINT 'FK OK FK_TSql_Jobside_TSql_Client_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Jobside_TSql_Client_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Language_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Language] WITH CHECK ADD CONSTRAINT [FK_TSql_Language_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Language_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Language_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Language_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Language] WITH CHECK ADD CONSTRAINT [FK_TSql_Language_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Language_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Language_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Language_TSql_Countrys', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Language] WITH CHECK ADD CONSTRAINT [FK_TSql_Language_TSql_Countrys] FOREIGN KEY ([LinkCountry]) REFERENCES dbo.[TSql_Countrys] ([IdObject]);
  PRINT 'FK OK FK_TSql_Language_TSql_Countrys';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Language_TSql_Countrys: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Offers_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Offers] WITH CHECK ADD CONSTRAINT [FK_TSql_Offers_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Offers_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Offers_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Offers_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Offers] WITH CHECK ADD CONSTRAINT [FK_TSql_Offers_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Offers_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Offers_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Offers_TSql_Client_V2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Offers] WITH CHECK ADD CONSTRAINT [FK_TSql_Offers_TSql_Client_V2] FOREIGN KEY ([LinkClient_V2]) REFERENCES dbo.[TSql_Client_V2] ([IdObject]);
  PRINT 'FK OK FK_TSql_Offers_TSql_Client_V2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Offers_TSql_Client_V2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Offers_TSql_Jobside', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Offers] WITH CHECK ADD CONSTRAINT [FK_TSql_Offers_TSql_Jobside] FOREIGN KEY ([LinkJobside]) REFERENCES dbo.[TSql_Jobside] ([IdObject]);
  PRINT 'FK OK FK_TSql_Offers_TSql_Jobside';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Offers_TSql_Jobside: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Offers_TSql_OfferState', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Offers] WITH CHECK ADD CONSTRAINT [FK_TSql_Offers_TSql_OfferState] FOREIGN KEY ([LinkOfferState]) REFERENCES dbo.[TSql_OfferState] ([IdObject]);
  PRINT 'FK OK FK_TSql_Offers_TSql_OfferState';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Offers_TSql_OfferState: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_OfferState_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_OfferState] WITH CHECK ADD CONSTRAINT [FK_TSql_OfferState_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_OfferState_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_OfferState_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_OfferState_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_OfferState] WITH CHECK ADD CONSTRAINT [FK_TSql_OfferState_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_OfferState_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_OfferState_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_PluginDeviceAuth_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_PluginDeviceAuth] WITH CHECK ADD CONSTRAINT [FK_TSql_PluginDeviceAuth_AspNetUsers] FOREIGN KEY ([LinAspNetUsert]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_PluginDeviceAuth_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_PluginDeviceAuth_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_TelegramDesignAccess_TSql_Design', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_TelegramDesignAccess] WITH CHECK ADD CONSTRAINT [FK_TSql_TelegramDesignAccess_TSql_Design] FOREIGN KEY ([LinDesign]) REFERENCES dbo.[TSql_Design] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_TelegramDesignAccess_TSql_Design';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_TelegramDesignAccess_TSql_Design: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_TelegramDesignInbox_TSql_Design', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_TelegramDesignInbox] WITH CHECK ADD CONSTRAINT [FK_TSql_TelegramDesignInbox_TSql_Design] FOREIGN KEY ([LinDesign]) REFERENCES dbo.[TSql_Design] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_TelegramDesignInbox_TSql_Design';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_TelegramDesignInbox_TSql_Design: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_UiTranslation_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_UiTranslation] WITH CHECK ADD CONSTRAINT [FK_TSql_UiTranslation_AspNetUsers] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_UiTranslation_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_UiTranslation_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_UiTranslation_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_UiTranslation] WITH CHECK ADD CONSTRAINT [FK_TSql_UiTranslation_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_UiTranslation_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_UiTranslation_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_XrPushJob_LinkXrDevice', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_XrPushJob] WITH CHECK ADD CONSTRAINT [FK_TSql_XrPushJob_LinkXrDevice] FOREIGN KEY ([LinkXrDevice]) REFERENCES dbo.[TSql_XrDevice] ([IdObject]);
  PRINT 'FK OK FK_TSql_XrPushJob_LinkXrDevice';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_XrPushJob_LinkXrDevice: ' + ERROR_MESSAGE();
END CATCH
GO
PRINT 'FIN align db_a197cd_desingdevelop';
GO
