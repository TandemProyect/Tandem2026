/* =============================================================================
   Script 3 — Migración dbo.TSql_Jobside existente
   Requiere: Script 1 (TSql_Client_V2).
   No ejecutar Script 2 si la tabla ya existe (usar este script en su lugar).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* 1) Quitar FK / índice antiguos de Link_Client (si existían) */
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_Jobside_TSql_Client'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside')
)
    ALTER TABLE dbo.TSql_Jobside DROP CONSTRAINT FK_TSql_Jobside_TSql_Client;
GO

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Jobside_Link_Client'
      AND object_id = OBJECT_ID(N'dbo.TSql_Jobside')
)
    DROP INDEX IX_TSql_Jobside_Link_Client ON dbo.TSql_Jobside;
GO

/* 2) Añadir LinkClient_V2 si no existe */
IF COL_LENGTH(N'dbo.TSql_Jobside', N'LinkClient_V2') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside
        ADD LinkClient_V2 BIGINT NULL;
END
GO

/* 3) Migrar datos Link_Client → LinkClient_V2 (solo si existía la columna antigua) */
IF COL_LENGTH(N'dbo.TSql_Jobside', N'Link_Client') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE dbo.TSql_Jobside
           SET LinkClient_V2 = Link_Client
         WHERE LinkClient_V2 IS NULL
           AND Link_Client IS NOT NULL;
    ');
END
GO

/* 4) FK hacia TSql_Client_V2 */
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_Jobside_TSql_Client_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside')
)
BEGIN
    ALTER TABLE dbo.TSql_Jobside
        ADD CONSTRAINT FK_TSql_Jobside_TSql_Client_V2
            FOREIGN KEY (LinkClient_V2)
            REFERENCES dbo.TSql_Client_V2 (IdObject);
END
GO

/* 5) Índice en LinkClient_V2 */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Jobside_LinkClient_V2'
      AND object_id = OBJECT_ID(N'dbo.TSql_Jobside')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Jobside_LinkClient_V2
        ON dbo.TSql_Jobside (LinkClient_V2)
        WHERE LinkClient_V2 IS NOT NULL AND Is_Delete = 0;
GO

/* 6) Eliminar columna obsoleta Link_Client */
IF COL_LENGTH(N'dbo.TSql_Jobside', N'Link_Client') IS NOT NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside DROP COLUMN Link_Client;
END
GO

PRINT N'OK — TSql_Jobside migrada a LinkClient_V2.';
GO
