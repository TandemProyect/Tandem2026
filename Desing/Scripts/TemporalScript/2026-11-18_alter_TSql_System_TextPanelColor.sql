-- Color de paneles del sistema. SQL directo: no se regenera el EDMX.
-- Idempotente. No pisa un color ya guardado.
IF COL_LENGTH(N'dbo.TSql_System', N'TextPanelColor') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_System ADD TextPanelColor NVARCHAR(20) NULL;
END
GO
