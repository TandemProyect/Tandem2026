-- 2026-11-14 pt109u: alineación ADITIVA de esquema: añade a db_a197cd_desingproducction lo que tiene db_a197cd_desingdevelop y le falta.
-- Idempotente (IF NOT EXISTS). No borra, no renombra, no cambia tipos ni datos existentes.
-- Ejecutar: sqlcmd -S <srv> -E -d db_a197cd_desingproducction -I -x -f 65001 -b -i <este archivo>
SET NOCOUNT ON;
SET XACT_ABORT OFF;
GO

/* ===== 1. Tablas nuevas ===== */
IF OBJECT_ID(N'dbo.Maestro_articulos_temporal', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[Maestro_articulos_temporal] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [TextCode] nvarchar(128) NOT NULL,
    [TextLabel] nvarchar(500) NOT NULL,
    [NumberHigh] float NULL,
    [NumberWidth] float NULL,
    [NumberLong] float NULL,
    [NumberWeight] float NULL,
    [TextBlockNumber] nvarchar(200) NULL,
    [TextStlNumber] nvarchar(200) NULL,
    [TextColor1] nvarchar(200) NULL,
    [TextColor2] nchar(10) NULL,
    [NumberMts2] float NULL,
    [NumberMts3] float NULL,
    [LinkMadeBy] nvarchar(128) NOT NULL,
    [AddDateMade] datetime NOT NULL,
    [AddChangeBy] nvarchar(128) NOT NULL,
    [AddLastDateChange] datetime NOT NULL,
    [Ntimeschanged] bigint NOT NULL,
    [AddIsActive] bit NOT NULL,
    [LinkSystem] bigint NOT NULL,
    [AddAtenkoCode] nvarchar(50) NULL
  );
  PRINT 'CREATE TABLE Maestro_articulos_temporal';
END
GO
IF OBJECT_ID(N'dbo.Movimientos', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[Movimientos] (
    [IdObject] bigint IDENTITY(1,1) NOT NULL,
    [remitoclientecontratoid] bigint NULL,
    [remitoclienteid] bigint NULL,
    [AddDate] datetime NULL,
    [clientenombre] nvarchar(500) NULL,
    [piezanumero] bigint NULL,
    [piezanombre] nvarchar(500) NULL,
    [remitoclientelineacantidad] float NULL,
    [remitoclientelineaprecioalquiler] float NULL,
    [remitoclientetipo] nvarchar(500) NULL
  );
  PRINT 'CREATE TABLE Movimientos';
END
GO
IF OBJECT_ID(N'dbo.perierase', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[perierase] (
    [article] nchar(10) NULL,
    [stl] nchar(10) NULL,
    [id] bigint IDENTITY(1,1) NOT NULL
  );
  PRINT 'CREATE TABLE perierase';
END
GO
IF OBJECT_ID(N'dbo.tbDCulture', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[tbDCulture] (
    [SysObjectID] bigint NOT NULL,
    [AttCode] nvarchar(10) NOT NULL,
    [AttDescription] nvarchar(250) NOT NULL,
    [AttIsActive] bit NOT NULL,
    [AttLastModification] datetime NOT NULL,
    [AttIsDefaultForSubscription] bit NOT NULL,
    [AttFlagPosition] nvarchar(200) NULL
  );
  PRINT 'CREATE TABLE tbDCulture';
END
GO
IF OBJECT_ID(N'dbo.temporalmaestro', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[temporalmaestro] (
    [id] bigint IDENTITY(1,1) NOT NULL,
    [codigoatenko] nvarchar(50) NULL,
    [codigodesing] nvarchar(50) NULL
  );
  PRINT 'CREATE TABLE temporalmaestro';
END
GO
IF OBJECT_ID(N'dbo.TSql_Branch1', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Branch1] (
    [SysObjectID] bigint NOT NULL,
    [LinCompany] bigint NOT NULL,
    [AttLabel] nvarchar(500) NOT NULL,
    [AddLeter] nvarchar(50) NULL,
    [AttDescription] nvarchar(2000) NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [AttCreated] smalldatetime NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [AttLastModification] smalldatetime NOT NULL,
    [SysUpdateNumber] bigint NOT NULL
  );
  PRINT 'CREATE TABLE TSql_Branch1';
END
GO
IF OBJECT_ID(N'dbo.TSql_Business', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Business] (
    [SysObjectID] bigint NOT NULL,
    [AttLabel] nvarchar(500) NOT NULL,
    [AttDescription] nvarchar(2000) NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [AttCreated] datetime NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [AttLastModification] datetime NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [LinCompany] bigint NOT NULL
  );
  PRINT 'CREATE TABLE TSql_Business';
END
GO
IF OBJECT_ID(N'dbo.TSql_Business1', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Business1] (
    [SysObjectID] bigint NOT NULL,
    [AddLabel] nvarchar(500) NOT NULL,
    [AddLeter] nvarchar(50) NULL,
    [AddDescription] nvarchar(2000) NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [AttCreated] smalldatetime NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [AttLastModification] smalldatetime NOT NULL,
    [SysUpdateNumber] bigint NOT NULL
  );
  PRINT 'CREATE TABLE TSql_Business1';
END
GO
IF OBJECT_ID(N'dbo.TSql_Client', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Client] (
    [Ds_Wb_Sys_ID] bigint NOT NULL,
    [Is_Delete] bit NOT NULL,
    [Is_Active] bit NOT NULL,
    [Id_Company] nvarchar(128) NOT NULL,
    [Id_CreatedBy] nvarchar(128) NOT NULL,
    [Id_Change] nvarchar(128) NOT NULL,
    [Date_Created] datetime NOT NULL,
    [Date_Change] datetime NOT NULL,
    [Date_Last_Active] datetime NOT NULL,
    [N_Change_Table] bigint NOT NULL,
    [Label_Client] nvarchar(500) NOT NULL,
    [Path_Ico] nvarchar(500) NOT NULL,
    [Path_Logo] nvarchar(500) NOT NULL
  );
  PRINT 'CREATE TABLE TSql_Client';
END
GO
IF OBJECT_ID(N'dbo.TSql_Comercial1', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_Comercial1] (
    [SysObjectID] bigint NOT NULL,
    [AttNumber] nvarchar(50) NULL,
    [AttLabel] nvarchar(500) NOT NULL,
    [AttDescription] nvarchar(2000) NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [AttCreated] smalldatetime NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [AttLastModification] smalldatetime NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [LinEmployee] bigint NOT NULL
  );
  PRINT 'CREATE TABLE TSql_Comercial1';
END
GO
IF OBJECT_ID(N'dbo.TSql_DesingConfigWallDetail', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_DesingConfigWallDetail] (
    [SysObjectID] bigint IDENTITY(1,1) NOT NULL,
    [LinDesingConfig] bigint NOT NULL,
    [MeshPoisition_X] nvarchar(128) NOT NULL,
    [MeshPoisition_Y] nvarchar(128) NOT NULL,
    [MeshPoisition_Z] nvarchar(128) NOT NULL,
    [MeshScale_X] nvarchar(128) NOT NULL,
    [MeshScale_Y] nvarchar(128) NOT NULL,
    [MeshScale_Z] nvarchar(128) NOT NULL,
    [MeshRotation_X] nvarchar(128) NOT NULL,
    [MeshRotation_Z] nvarchar(128) NOT NULL,
    [WallPoisition_X] nvarchar(128) NOT NULL,
    [WallPoisition_Y] nvarchar(128) NOT NULL,
    [WallPoisition_Z] nvarchar(128) NOT NULL,
    [WallType] nvarchar(128) NOT NULL,
    [MeshName] nvarchar(128) NOT NULL,
    [MeshIdWall] nvarchar(128) NOT NULL,
    [MeshId] nvarchar(128) NOT NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [AttChange] datetime NOT NULL CONSTRAINT [DF_TSql_DesingConfigDetail_AttChange] DEFAULT (((1)/(1))/(2024))
  );
  PRINT 'CREATE TABLE TSql_DesingConfigWallDetail';
END
GO
IF OBJECT_ID(N'dbo.TSql_DesingWallConfig', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_DesingWallConfig] (
    [SysObjectID] bigint IDENTITY(1,1) NOT NULL,
    [LinDesing] bigint NOT NULL,
    [IdWall] nvarchar(128) NOT NULL,
    [AttLabel] nvarchar(500) NOT NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [AttChange] datetime NOT NULL CONSTRAINT [DF_TSql_DesingConfig_AttChange] DEFAULT (((1)/(1))/(2024)),
    [LinTypeDesingWallConfig] bigint NOT NULL
  );
  PRINT 'CREATE TABLE TSql_DesingWallConfig';
END
GO
IF OBJECT_ID(N'dbo.TSql_LanguageConcept', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_LanguageConcept] (
    [SysObjectID] bigint NOT NULL,
    [AttLanguageConcept] nvarchar(500) NOT NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [AttCreated] datetime NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [AttLastModification] datetime NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [AttIsDeleted] bit NOT NULL,
    [Attfas] nvarchar(500) NOT NULL
  );
  PRINT 'CREATE TABLE TSql_LanguageConcept';
END
GO
IF OBJECT_ID(N'dbo.TSql_LanguageDetails', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_LanguageDetails] (
    [SysObjectID] bigint NOT NULL,
    [AttLabel] nvarchar(500) NOT NULL,
    [LinLanguageConcept] bigint NOT NULL,
    [Linlanguage] bigint NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [AttCreated] datetime NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [AttLastModification] datetime NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [AttIsDeleted] bit NOT NULL
  );
  PRINT 'CREATE TABLE TSql_LanguageDetails';
END
GO
IF OBJECT_ID(N'dbo.TSql_TypeDesingWallConfig', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_TypeDesingWallConfig] (
    [SysObjectID] bigint IDENTITY(1,1) NOT NULL,
    [AttLabel] nvarchar(500) NOT NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [AttChange] datetime NOT NULL CONSTRAINT [DF_TypeDesingWallConfig_AttChange] DEFAULT (((1)/(1))/(2024))
  );
  PRINT 'CREATE TABLE TSql_TypeDesingWallConfig';
END
GO
IF OBJECT_ID(N'dbo.TSql_UserBranch', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.[TSql_UserBranch] (
    [SysObjectID] bigint NOT NULL,
    [LinUser] nvarchar(128) NOT NULL,
    [LinBranch] bigint NOT NULL,
    [LinCreatedBy] nvarchar(128) NOT NULL,
    [AttCreated] datetime NOT NULL,
    [LinModifiedBy] nvarchar(128) NOT NULL,
    [AttLastModification] datetime NOT NULL,
    [SysUpdateNumber] bigint NOT NULL,
    [AttIsDeleted] bit NOT NULL
  );
  PRINT 'CREATE TABLE TSql_UserBranch';
END
GO

/* ===== 2. Columnas nuevas en tablas existentes ===== */
IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Branch', N'LinBusiness') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Branch])
    ALTER TABLE dbo.[TSql_Branch] ADD [LinBusiness] bigint NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Branch] ADD [LinBusiness] bigint NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DesignDetails', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignDetails', N'IsRenderWall') IS NULL
  ALTER TABLE dbo.[TSql_DesignDetails] ADD [IsRenderWall] bit NULL CONSTRAINT [DF_TSql_DesignDetails_IsRenderWall] DEFAULT ((1));
GO
IF OBJECT_ID(N'dbo.TSql_DesignDetails', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignDetails', N'LinDesingWallConfig') IS NULL
  ALTER TABLE dbo.[TSql_DesignDetails] ADD [LinDesingWallConfig] bigint NULL;
GO
IF OBJECT_ID(N'dbo.TSql_DesignDetails', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DesignDetails', N'ModeLManualConfig') IS NULL
  ALTER TABLE dbo.[TSql_DesignDetails] ADD [ModeLManualConfig] bit NULL CONSTRAINT [DF_TSql_DesignDetails_ModeLManualConfig] DEFAULT ((0));
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'IDESTADO_BASE') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [IDESTADO_BASE] int NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [IDESTADO_BASE] int NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'IDGRUPO_DOCUMENTO') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [IDGRUPO_DOCUMENTO] int NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [IDGRUPO_DOCUMENTO] int NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'IDTIPO_DOCUMENTO') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [IDTIPO_DOCUMENTO] int NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [IDTIPO_DOCUMENTO] int NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'ORDEN') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [ORDEN] int NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [ORDEN] int NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'SIGLAS') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [SIGLAS] nvarchar(max) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [SIGLAS] nvarchar(max) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'TIPO_DOCUMENTO') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [TIPO_DOCUMENTO] nvarchar(max) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [TIPO_DOCUMENTO] nvarchar(max) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'TIPO_DOCUMENTO_CORTO') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [TIPO_DOCUMENTO_CORTO] nvarchar(max) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [TIPO_DOCUMENTO_CORTO] nvarchar(max) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_DocumentType', N'VINCULABLE') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_DocumentType])
    ALTER TABLE dbo.[TSql_DocumentType] ADD [VINCULABLE] int NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_DocumentType] ADD [VINCULABLE] int NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'AttCreated') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [AttCreated] datetime NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [AttCreated] datetime NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'AttIsDeleted') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [AttIsDeleted] bit NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [AttIsDeleted] bit NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'AttLabel') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [AttLabel] nvarchar(500) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [AttLabel] nvarchar(500) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'AttLastModification') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [AttLastModification] datetime NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [AttLastModification] datetime NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'AttSinglature') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [AttSinglature] nvarchar(3) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [AttSinglature] nvarchar(3) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'LinCreatedBy') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [LinCreatedBy] nvarchar(128) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [LinCreatedBy] nvarchar(128) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'Linflag') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [Linflag] nvarchar(500) NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [Linflag] nvarchar(500) NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'SysObjectID') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [SysObjectID] bigint NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [SysObjectID] bigint NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_Language', N'SysUpdateNumber') IS NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Language])
    ALTER TABLE dbo.[TSql_Language] ADD [SysUpdateNumber] bigint NULL; -- NOT NULL sin default en origen y la tabla tiene filas
  ELSE
    ALTER TABLE dbo.[TSql_Language] ADD [SysUpdateNumber] bigint NOT NULL;
END
GO
IF OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tsql_Master_Articles', N'ImgDetail') IS NULL
  ALTER TABLE dbo.[Tsql_Master_Articles] ADD [ImgDetail] nvarchar(500) NULL;
GO

/* ===== 3. Defaults que faltan en columnas existentes ===== */

/* ===== 4. Claves primarias, únicas e índices ===== */
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.Maestro_articulos_temporal') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[Maestro_articulos_temporal] ADD CONSTRAINT [PK_Maestro_articulos_temporal] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY Maestro_articulos_temporal.PK_Maestro_articulos_temporal: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.Movimientos') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[Movimientos] ADD CONSTRAINT [PK_Movimientos] PRIMARY KEY CLUSTERED ([IdObject] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY Movimientos.PK_Movimientos: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.perierase') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[perierase] ADD CONSTRAINT [PK_perierase] PRIMARY KEY CLUSTERED ([id] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY perierase.PK_perierase: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.temporalmaestro') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[temporalmaestro] ADD CONSTRAINT [PK_temporalmaestro] PRIMARY KEY CLUSTERED ([id] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY temporalmaestro.PK_temporalmaestro: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_DesingConfigWallDetail') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingConfigWallDetail] ADD CONSTRAINT [PK_TSql_DesingConfigDetail] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_DesingConfigWallDetail.PK_TSql_DesingConfigDetail: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_DesingWallConfig') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingWallConfig] ADD CONSTRAINT [PK_TSql_DesingConfig] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_DesingWallConfig.PK_TSql_DesingConfig: ' + ERROR_MESSAGE();
END CATCH
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.TSql_TypeDesingWallConfig') AND type='PK')
BEGIN TRY
  ALTER TABLE dbo.[TSql_TypeDesingWallConfig] ADD CONSTRAINT [PK_TypeDesingWallConfig] PRIMARY KEY CLUSTERED ([SysObjectID] ASC);
END TRY
BEGIN CATCH
  PRINT 'SKIP PRIMARY KEY TSql_TypeDesingWallConfig.PK_TypeDesingWallConfig: ' + ERROR_MESSAGE();
END CATCH
GO

/* ===== 5. Check constraints ===== */

/* ===== 6. Foreign keys (si hay huérfanos se omite y se informa) ===== */
IF OBJECT_ID(N'dbo.FK_TSql_Branch_TSql_Company', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Branch] WITH CHECK ADD CONSTRAINT [FK_TSql_Branch_TSql_Company] FOREIGN KEY ([LinCompany]) REFERENCES dbo.[TSql_Company] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_Branch_TSql_Company';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Branch_TSql_Company: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Company_AspNetUsers2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Company] WITH CHECK ADD CONSTRAINT [FK_TSql_Company_AspNetUsers2] FOREIGN KEY ([LinkMadeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Company_AspNetUsers2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Company_AspNetUsers2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_Company_AspNetUsers3', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_Company] WITH CHECK ADD CONSTRAINT [FK_TSql_Company_AspNetUsers3] FOREIGN KEY ([AddChangeBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_Company_AspNetUsers3';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_Company_AspNetUsers3: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesignDetails_TSql_Design1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesignDetails] WITH CHECK ADD CONSTRAINT [FK_TSql_DesignDetails_TSql_Design1] FOREIGN KEY ([LinDesaing]) REFERENCES dbo.[TSql_Design] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesignDetails_TSql_Design1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesignDetails_TSql_Design1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesingConfig_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingWallConfig] WITH CHECK ADD CONSTRAINT [FK_TSql_DesingConfig_AspNetUsers] FOREIGN KEY ([LinCreatedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_DesingConfig_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesingConfig_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesingConfig_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingWallConfig] WITH CHECK ADD CONSTRAINT [FK_TSql_DesingConfig_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_DesingConfig_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesingConfig_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesingConfig_TSql_TypeDesingWallConfig', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingWallConfig] WITH CHECK ADD CONSTRAINT [FK_TSql_DesingConfig_TSql_TypeDesingWallConfig] FOREIGN KEY ([LinTypeDesingWallConfig]) REFERENCES dbo.[TSql_TypeDesingWallConfig] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesingConfig_TSql_TypeDesingWallConfig';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesingConfig_TSql_TypeDesingWallConfig: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesingConfigDetail_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingConfigWallDetail] WITH CHECK ADD CONSTRAINT [FK_TSql_DesingConfigDetail_AspNetUsers] FOREIGN KEY ([LinCreatedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_DesingConfigDetail_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesingConfigDetail_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesingConfigDetail_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingConfigWallDetail] WITH CHECK ADD CONSTRAINT [FK_TSql_DesingConfigDetail_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_DesingConfigDetail_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesingConfigDetail_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesingConfigDetail_TSql_DesingConfig', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingConfigWallDetail] WITH CHECK ADD CONSTRAINT [FK_TSql_DesingConfigDetail_TSql_DesingConfig] FOREIGN KEY ([LinDesingConfig]) REFERENCES dbo.[TSql_DesingWallConfig] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesingConfigDetail_TSql_DesingConfig';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesingConfigDetail_TSql_DesingConfig: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_DesingWallConfig_TSql_Design', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_DesingWallConfig] WITH CHECK ADD CONSTRAINT [FK_TSql_DesingWallConfig_TSql_Design] FOREIGN KEY ([LinDesing]) REFERENCES dbo.[TSql_Design] ([SysObjectID]);
  PRINT 'FK OK FK_TSql_DesingWallConfig_TSql_Design';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_DesingWallConfig_TSql_Design: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_TypeDesingWallConfig_AspNetUsers', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_TypeDesingWallConfig] WITH CHECK ADD CONSTRAINT [FK_TSql_TypeDesingWallConfig_AspNetUsers] FOREIGN KEY ([LinCreatedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_TypeDesingWallConfig_AspNetUsers';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_TypeDesingWallConfig_AspNetUsers: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_TypeDesingWallConfig_AspNetUsers1', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_TypeDesingWallConfig] WITH CHECK ADD CONSTRAINT [FK_TSql_TypeDesingWallConfig_AspNetUsers1] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_TypeDesingWallConfig_AspNetUsers1';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_TypeDesingWallConfig_AspNetUsers1: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_TypeDesingWallConfig_AspNetUsers2', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_TypeDesingWallConfig] WITH CHECK ADD CONSTRAINT [FK_TSql_TypeDesingWallConfig_AspNetUsers2] FOREIGN KEY ([LinCreatedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_TypeDesingWallConfig_AspNetUsers2';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_TypeDesingWallConfig_AspNetUsers2: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.FK_TSql_TypeDesingWallConfig_AspNetUsers3', N'F') IS NULL
BEGIN TRY
  ALTER TABLE dbo.[TSql_TypeDesingWallConfig] WITH CHECK ADD CONSTRAINT [FK_TSql_TypeDesingWallConfig_AspNetUsers3] FOREIGN KEY ([LinModifiedBy]) REFERENCES dbo.[AspNetUsers] ([Id]);
  PRINT 'FK OK FK_TSql_TypeDesingWallConfig_AspNetUsers3';
END TRY
BEGIN CATCH
  PRINT 'SKIP FK FK_TSql_TypeDesingWallConfig_AspNetUsers3: ' + ERROR_MESSAGE();
END CATCH
GO
PRINT 'FIN align db_a197cd_desingproducction';
GO
