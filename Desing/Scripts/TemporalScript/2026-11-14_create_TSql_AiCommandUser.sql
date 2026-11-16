/* =============================================================================
   dbo.TSql_AiCommandUser — Ajustes de cada usuario sobre cada comando (una fila por usuario y comando).
   Requiere antes: 2026-11-14_create_TSql_AiCommand.sql.

   Modelo acordado (pt106u): solo cambios. La fila se crea al primer ajuste del usuario;
   sin fila = valores por defecto. «Todos los comandos del usuario» = catálogo LEFT JOIN esta tabla.

   Is_Active (auditoría) = 0  → comando desactivado para ese usuario.
   Is_UseDefaultPhrases = 1   → sus frases SE SUMAN a las de por defecto (por defecto, acordado);
                          0   → solo valen sus frases (reemplaza las de por defecto en ese comando).
   LinkAspNetUsers → AspNetUsers.Id (mismo patrón que TSql_DefaultDesign.LinkAspNetUsers).
   Convención: .cursor/rules/sql-tsql-table-conventions.mdc (9 columnas de auditoría, borrado lógico).
   Script idempotente. Tras ejecutar: Update Model from Database (DAL/Model.edmx).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_AiCommandUser', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_AiCommandUser
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_AiCommandUser PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        LinkAspNetUsers      NVARCHAR(128) NOT NULL,
        LinkAiCommand        BIGINT        NOT NULL,
        Is_UseDefaultPhrases BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommandUser_Is_UseDefaultPhrases DEFAULT (1),
        NumberOrder          INT NULL,
        /* ----- Auditoría obligatoria ----- */
        TextLabel     NVARCHAR(500) NOT NULL,
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommandUser_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommandUser_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_AiCommandUser_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_AiCommandUser_Ntimeschanged DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.TSql_AiCommandUser', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUser_TSql_AiCommand' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUser'))
    ALTER TABLE dbo.TSql_AiCommandUser WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUser_TSql_AiCommand
        FOREIGN KEY (LinkAiCommand) REFERENCES dbo.TSql_AiCommand (IdObject);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUser', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUser_AspNetUsers2' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUser'))
    ALTER TABLE dbo.TSql_AiCommandUser WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUser_AspNetUsers2
        FOREIGN KEY (LinkAspNetUsers) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUser', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiCommandUser_User_Command' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandUser')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiCommandUser_User_Command
        ON dbo.TSql_AiCommandUser (LinkAspNetUsers, LinkAiCommand)
        WHERE Is_Delete = 0;
GO
/* FK auditoría → AspNetUsers (patrón TSql_UiTranslation) */
IF OBJECT_ID(N'dbo.TSql_AiCommandUser', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUser_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUser'))
    ALTER TABLE dbo.TSql_AiCommandUser WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUser_AspNetUsers
        FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUser', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUser_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUser'))
    ALTER TABLE dbo.TSql_AiCommandUser WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUser_AspNetUsers1
        FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
GO

PRINT N'OK — dbo.TSql_AiCommandUser creada (o ya existía).';
GO
