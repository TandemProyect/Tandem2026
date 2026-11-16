/* =============================================================================
   dbo.TSql_Functions — Funciones (comandos / líneas de menú protegidas). Sustituye a TSql_RollFunctions y TSql_RollGrupDesing.
   Regla: si existe una función activa con el TextCode de un comando, solo la usan los roles que la tengan;
   si no existe, el comando es libre para todos.
   Seguridad: roles y funciones (ver Desing/IA/docs/SEGURIDAD-ROLES-FUNCIONES.md).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_Functions'
)
BEGIN
    CREATE TABLE [dbo].[TSql_Functions]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_Functions PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,
        /* Clave del comando: «Controlador», «Controlador.Accion» o comando JS («Tool.polyline»). */
        [TextCode]            NVARCHAR(200)   NOT NULL,
        /* Agrupación en pantalla (sustituye a TSql_RollGrupDesing). */
        [TextModule]          NVARCHAR(100)   NULL,
        [TextDescription]     NVARCHAR(MAX)   NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_Functions_Is_Delete         DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_Functions_Is_Active         DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_Functions_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_Functions_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_Functions_TextCode_Live'
      AND object_id = OBJECT_ID(N'dbo.TSql_Functions')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_Functions_TextCode_Live
        ON [dbo].[TSql_Functions] ([TextCode])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Functions_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_Functions')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Functions_TextLabel_Active
        ON [dbo].[TSql_Functions] ([TextLabel])
        INCLUDE ([Is_Active])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_Functions creada (o ya existía).';
GO
