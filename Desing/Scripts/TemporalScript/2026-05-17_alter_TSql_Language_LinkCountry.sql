/* =============================================================================
   dbo.TSql_Language — Añade LinkCountry (FK opcional a dbo.TSql_Countrys.IdObject)

   El modelo EF (DAL/Model.edmx) ya mapea esta columna y la asociación
   FK_TSql_Language_TSql_Countrys; si falta en SQL, LINQ falla con
   «Invalid column name 'LinkCountry'».

   Idempotente: seguro ejecutar varias veces.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'LinkCountry') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Language ADD LinkCountry BIGINT NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'LinkCountry') IS NOT NULL
   AND OBJECT_ID(N'dbo.TSql_Countrys', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
         FROM sys.foreign_keys fk
        WHERE fk.name = N'FK_TSql_Language_TSql_Countrys'
          AND fk.parent_object_id = OBJECT_ID(N'dbo.TSql_Language')
   )
BEGIN
    ALTER TABLE dbo.TSql_Language
        ADD CONSTRAINT FK_TSql_Language_TSql_Countrys
            FOREIGN KEY (LinkCountry) REFERENCES dbo.TSql_Countrys (IdObject);
END
GO

PRINT N'OK — dbo.TSql_Language.LinkCountry y FK (si aplicaba).';
GO
