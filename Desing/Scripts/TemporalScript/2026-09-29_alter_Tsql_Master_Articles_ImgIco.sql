/* =============================================================================
 dbo.Tsql_Master_Articles — ImgIco (icono de lista / bloquing CAD)
 Tabla legada (no TSql_* de auditoría nueva). Solo se añade la columna de icono.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'ImgIco') IS NULL
BEGIN
    ALTER TABLE [dbo].[Tsql_Master_Articles]
        ADD [ImgIco] NVARCHAR(500) NULL;
END
GO

UPDATE a
SET a.ImgIco = N'~/Files/MaterialIco/panel.png'
FROM [dbo].[Tsql_Master_Articles] AS a
WHERE a.ImgIco IS NULL
  AND (
        a.TextLabel LIKE N'%Atk%'
     OR a.TextLabel LIKE N'%Panel%'
     OR a.TextLabel LIKE N'%panel%'
  );
GO

PRINT N'OK — dbo.Tsql_Master_Articles.ImgIco creada/sembrada (o ya existía).';
GO
