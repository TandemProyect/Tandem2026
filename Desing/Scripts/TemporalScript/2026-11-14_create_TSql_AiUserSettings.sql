/* =============================================================================
   dbo.TSql_AiUserSettings — Preferencias de la IA por usuario (una fila por usuario; pt107u).
   Requiere antes: 2026-11-14_create_TSql_AiEnvironment.sql.
   LinkEnvironment: entorno elegido (NULL o sin fila = el entorno por defecto, EXECUTIVE).
   TextLanguageCode: idioma preferido BCP-47 (NULL = el de la interfaz).
   Va aparte de TSql_AiCommandUser porque aquella es por comando y esto es por usuario.
   Convención: .cursor/rules/sql-tsql-table-conventions.mdc (9 columnas de auditoría, borrado lógico).
   Script idempotente. Tras ejecutar: Update Model from Database (DAL/Model.edmx).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_AiUserSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_AiUserSettings
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_AiUserSettings PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        LinkAspNetUsers  NVARCHAR(128) NOT NULL,
        LinkEnvironment  BIGINT        NULL,
        TextLanguageCode NVARCHAR(10)  NULL,
        /* ----- Auditoría obligatoria ----- */
        TextLabel     NVARCHAR(500) NOT NULL,
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_AiUserSettings_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_AiUserSettings_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_AiUserSettings_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_AiUserSettings_Ntimeschanged DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.TSql_AiUserSettings', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiUserSettings_TSql_AiEnvironment' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiUserSettings'))
    ALTER TABLE dbo.TSql_AiUserSettings WITH CHECK ADD CONSTRAINT FK_TSql_AiUserSettings_TSql_AiEnvironment
        FOREIGN KEY (LinkEnvironment) REFERENCES dbo.TSql_AiEnvironment (IdObject);
GO
IF OBJECT_ID(N'dbo.TSql_AiUserSettings', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiUserSettings_AspNetUsers2' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiUserSettings'))
    ALTER TABLE dbo.TSql_AiUserSettings WITH CHECK ADD CONSTRAINT FK_TSql_AiUserSettings_AspNetUsers2
        FOREIGN KEY (LinkAspNetUsers) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiUserSettings', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiUserSettings_User' AND object_id = OBJECT_ID(N'dbo.TSql_AiUserSettings')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiUserSettings_User
        ON dbo.TSql_AiUserSettings (LinkAspNetUsers)
        INCLUDE (LinkEnvironment, TextLanguageCode, Is_Active)
        WHERE Is_Delete = 0;
GO
/* FK auditoría → AspNetUsers (patrón TSql_UiTranslation) */
IF OBJECT_ID(N'dbo.TSql_AiUserSettings', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiUserSettings_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiUserSettings'))
    ALTER TABLE dbo.TSql_AiUserSettings WITH CHECK ADD CONSTRAINT FK_TSql_AiUserSettings_AspNetUsers
        FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiUserSettings', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiUserSettings_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiUserSettings'))
    ALTER TABLE dbo.TSql_AiUserSettings WITH CHECK ADD CONSTRAINT FK_TSql_AiUserSettings_AspNetUsers1
        FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
GO

PRINT N'OK — dbo.TSql_AiUserSettings creada (o ya existía).';
GO
