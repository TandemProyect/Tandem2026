/* =============================================================================
   dbo.TSql_UserRole — Roles de cada usuario (N:N AspNetUsers ↔ TSql_Roles_V2). Sustituye a TSql_RolesFuntionUser.
   Seguridad: roles y funciones (ver Desing/IA/docs/SEGURIDAD-ROLES-FUNCIONES.md).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_UserRole'
)
BEGIN
    CREATE TABLE [dbo].[TSql_UserRole]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_UserRole PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,
        [LinkAspNetUsers]     NVARCHAR(128)   NOT NULL,
        [LinkRoles_V2]        BIGINT          NOT NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_UserRole_Is_Delete         DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_UserRole_Is_Active         DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_UserRole_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_UserRole_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_UserRole_AspNetUsers')
    ALTER TABLE [dbo].[TSql_UserRole] WITH CHECK
        ADD CONSTRAINT [FK_TSql_UserRole_AspNetUsers] FOREIGN KEY ([LinkAspNetUsers]) REFERENCES [dbo].[AspNetUsers] ([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_UserRole_TSql_Roles_V2')
    ALTER TABLE [dbo].[TSql_UserRole] WITH CHECK
        ADD CONSTRAINT [FK_TSql_UserRole_TSql_Roles_V2] FOREIGN KEY ([LinkRoles_V2]) REFERENCES [dbo].[TSql_Roles_V2] ([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_UserRole_User_Role_Live'
      AND object_id = OBJECT_ID(N'dbo.TSql_UserRole')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_UserRole_User_Role_Live
        ON [dbo].[TSql_UserRole] ([LinkAspNetUsers], [LinkRoles_V2])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_UserRole creada (o ya existía).';
GO
