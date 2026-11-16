/* =============================================================================
   dbo.TSql_RoleFunction — Funciones de cada rol (N:N TSql_Roles_V2 ↔ TSql_Functions).
   Seguridad: roles y funciones (ver Desing/IA/docs/SEGURIDAD-ROLES-FUNCIONES.md).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_RoleFunction'
)
BEGIN
    CREATE TABLE [dbo].[TSql_RoleFunction]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_RoleFunction PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,
        [LinkRoles_V2]        BIGINT          NOT NULL,
        [LinkFunctions]       BIGINT          NOT NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_RoleFunction_Is_Delete         DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_RoleFunction_Is_Active         DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_RoleFunction_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_RoleFunction_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_RoleFunction_TSql_Roles_V2')
    ALTER TABLE [dbo].[TSql_RoleFunction] WITH CHECK
        ADD CONSTRAINT [FK_TSql_RoleFunction_TSql_Roles_V2] FOREIGN KEY ([LinkRoles_V2]) REFERENCES [dbo].[TSql_Roles_V2] ([IdObject]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_RoleFunction_TSql_Functions')
    ALTER TABLE [dbo].[TSql_RoleFunction] WITH CHECK
        ADD CONSTRAINT [FK_TSql_RoleFunction_TSql_Functions] FOREIGN KEY ([LinkFunctions]) REFERENCES [dbo].[TSql_Functions] ([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_RoleFunction_Role_Function_Live'
      AND object_id = OBJECT_ID(N'dbo.TSql_RoleFunction')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_RoleFunction_Role_Function_Live
        ON [dbo].[TSql_RoleFunction] ([LinkRoles_V2], [LinkFunctions])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_RoleFunction_Function'
      AND object_id = OBJECT_ID(N'dbo.TSql_RoleFunction')
)
    CREATE NONCLUSTERED INDEX IX_TSql_RoleFunction_Function
        ON [dbo].[TSql_RoleFunction] ([LinkFunctions])
        INCLUDE ([LinkRoles_V2])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_RoleFunction creada (o ya existía).';
GO
