/* =============================================================================
   dbo.TSql_AiEnvironment — Entornos de conversación de la IA (tono y jerga): Ejecutivo, Coloquial,
   Colega barriobajero, Técnico… (pt107u).
   TextCode: código estable (EXECUTIVE, COLLOQUIAL, STREET, TECHNICAL), único con Is_Delete = 0.
   Is_Default = 1: entorno por defecto (usuarios sin preferencia). Solo uno: índice único filtrado.
   Las respuestas de cada entorno están en dbo.TSql_AiEnvironmentReply; las frases propias de un entorno,
   en TSql_AiCommandPhrase / TSql_AiCommandUserPhrase (LinkEnvironment; NULL = todos los entornos).
   Convención: .cursor/rules/sql-tsql-table-conventions.mdc (9 columnas de auditoría, borrado lógico).
   Script idempotente. Tras ejecutar: Update Model from Database (DAL/Model.edmx).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_AiEnvironment
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_AiEnvironment PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        TextCode        NVARCHAR(40)   NOT NULL,
        TextNameEs      NVARCHAR(200)  NOT NULL,
        TextNameEn      NVARCHAR(200)  NOT NULL,
        TextDescription NVARCHAR(1000) NULL,
        Is_Default      BIT NOT NULL
            CONSTRAINT DF_TSql_AiEnvironment_Is_Default DEFAULT (0),
        NumberOrder     INT NOT NULL
            CONSTRAINT DF_TSql_AiEnvironment_NumberOrder DEFAULT (0),
        /* ----- Auditoría obligatoria ----- */
        TextLabel     NVARCHAR(500) NOT NULL,
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_AiEnvironment_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_AiEnvironment_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_AiEnvironment_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_AiEnvironment_Ntimeschanged DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiEnvironment_TextCode_Active' AND object_id = OBJECT_ID(N'dbo.TSql_AiEnvironment')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiEnvironment_TextCode_Active
        ON dbo.TSql_AiEnvironment (TextCode)
        WHERE Is_Delete = 0;
GO
IF OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiEnvironment_Default' AND object_id = OBJECT_ID(N'dbo.TSql_AiEnvironment')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiEnvironment_Default
        ON dbo.TSql_AiEnvironment (Is_Default)
        WHERE Is_Default = 1 AND Is_Delete = 0;
GO
/* FK auditoría → AspNetUsers (patrón TSql_UiTranslation) */
IF OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiEnvironment_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiEnvironment'))
    ALTER TABLE dbo.TSql_AiEnvironment WITH CHECK ADD CONSTRAINT FK_TSql_AiEnvironment_AspNetUsers
        FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiEnvironment_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiEnvironment'))
    ALTER TABLE dbo.TSql_AiEnvironment WITH CHECK ADD CONSTRAINT FK_TSql_AiEnvironment_AspNetUsers1
        FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
GO

PRINT N'OK — dbo.TSql_AiEnvironment creada (o ya existía).';
GO
