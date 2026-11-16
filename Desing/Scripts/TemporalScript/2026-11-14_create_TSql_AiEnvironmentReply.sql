/* =============================================================================
   dbo.TSql_AiEnvironmentReply — Plantillas de respuesta de la IA por entorno e idioma (pt107u).
   Requiere antes: 2026-11-14_create_TSql_AiEnvironment.sql.
   TextMessageKey: '<módulo>.<clave del texto JS>' — design.drawn, design.confirmDelete, design.didYouMean,
     wall.morning, wall.thanks… (la misma clave que MA_STL_DESIGN_AI_TEXT / MA_STL_WALL_AI_TEXT, así no hay tabla de equivalencias).
   TextTemplate: texto con huecos posicionales {0}, {1}… (mismo orden que el texto JS de esa clave).
   Respaldo: si un entorno no tiene la clave → la del entorno por defecto (EXECUTIVE) → el texto del JS.
   TextLanguageCode: BCP-47 en minúsculas, NVARCHAR(10) ('es', 'en', 'es-ar'…); una variante usa lo suyo y,
     si falta, lo del idioma base (ver TSql_AiCommandPhrase).
   Convención: .cursor/rules/sql-tsql-table-conventions.mdc (9 columnas de auditoría, borrado lógico).
   Script idempotente. Tras ejecutar: Update Model from Database (DAL/Model.edmx).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_AiEnvironmentReply
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_AiEnvironmentReply PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        LinkEnvironment  BIGINT         NOT NULL,
        TextLanguageCode NVARCHAR(10)   NOT NULL,
        TextMessageKey   NVARCHAR(100)  NOT NULL,
        TextTemplate     NVARCHAR(2000) NOT NULL,
        /* ----- Auditoría obligatoria ----- */
        TextLabel     NVARCHAR(500) NOT NULL,
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_AiEnvironmentReply_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_AiEnvironmentReply_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_AiEnvironmentReply_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_AiEnvironmentReply_Ntimeschanged DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiEnvironmentReply_TSql_AiEnvironment' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiEnvironmentReply'))
    ALTER TABLE dbo.TSql_AiEnvironmentReply WITH CHECK ADD CONSTRAINT FK_TSql_AiEnvironmentReply_TSql_AiEnvironment
        FOREIGN KEY (LinkEnvironment) REFERENCES dbo.TSql_AiEnvironment (IdObject);
GO
IF OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiEnvironmentReply_Env_Lang_Key' AND object_id = OBJECT_ID(N'dbo.TSql_AiEnvironmentReply')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiEnvironmentReply_Env_Lang_Key
        ON dbo.TSql_AiEnvironmentReply (LinkEnvironment, TextLanguageCode, TextMessageKey)
        INCLUDE (TextTemplate, Is_Active)
        WHERE Is_Delete = 0;
GO
/* FK auditoría → AspNetUsers (patrón TSql_UiTranslation) */
IF OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiEnvironmentReply_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiEnvironmentReply'))
    ALTER TABLE dbo.TSql_AiEnvironmentReply WITH CHECK ADD CONSTRAINT FK_TSql_AiEnvironmentReply_AspNetUsers
        FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiEnvironmentReply_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiEnvironmentReply'))
    ALTER TABLE dbo.TSql_AiEnvironmentReply WITH CHECK ADD CONSTRAINT FK_TSql_AiEnvironmentReply_AspNetUsers1
        FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
GO

PRINT N'OK — dbo.TSql_AiEnvironmentReply creada (o ya existía).';
GO
