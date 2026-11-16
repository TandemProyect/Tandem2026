/* =============================================================================
   dbo.TSql_Roles_V2 — Roles de usuario (sustituye a TSql_Roles). Un usuario puede tener varios roles.
   Seguridad: roles y funciones (ver Desing/IA/docs/SEGURIDAD-ROLES-FUNCIONES.md).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_Roles_V2'
)
BEGIN
    CREATE TABLE [dbo].[TSql_Roles_V2]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_Roles_V2 PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,
        [TextCode]            NVARCHAR(100)   NULL,
        [TextDescription]     NVARCHAR(2000)  NULL,
        /* Rol de sistema: no se puede borrar desde la intranet. */
        [Is_System]           BIT             NOT NULL
            CONSTRAINT DF_TSql_Roles_V2_Is_System       DEFAULT (0),
        /* Concede todas las funciones (evita quedarse sin acceso al proteger algo nuevo). */
        [Is_AllFunctions]     BIT             NOT NULL
            CONSTRAINT DF_TSql_Roles_V2_Is_AllFunctions DEFAULT (0),

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_Roles_V2_Is_Delete         DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_Roles_V2_Is_Active         DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_Roles_V2_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_Roles_V2_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_Roles_V2_TextLabel_Live'
      AND object_id = OBJECT_ID(N'dbo.TSql_Roles_V2')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_Roles_V2_TextLabel_Live
        ON [dbo].[TSql_Roles_V2] ([TextLabel])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_Roles_V2_TextCode_Live'
      AND object_id = OBJECT_ID(N'dbo.TSql_Roles_V2')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_Roles_V2_TextCode_Live
        ON [dbo].[TSql_Roles_V2] ([TextCode])
        WHERE [Is_Delete] = 0 AND [TextCode] IS NOT NULL;
GO

PRINT N'OK — dbo.TSql_Roles_V2 creada (o ya existía).';
GO
