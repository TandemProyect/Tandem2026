/* =============================================================================
   dbo.TSql_Company — Idioma UI obligatorio por empresa (empleados).

   LinkLanguage → dbo.TSql_Language.IdObject (nullable hasta migrar filas existentes).

   Tras ejecutar: validar EDMX / regenerar si usáis el diseñador.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Company', N'LinkLanguage') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Company ADD LinkLanguage BIGINT NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Company', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Company', N'LinkLanguage') IS NOT NULL
   AND OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
         FROM sys.foreign_keys fk
        WHERE fk.name = N'FK_TSql_Company_TSql_Language'
          AND fk.parent_object_id = OBJECT_ID(N'dbo.TSql_Company')
   )
BEGIN
    ALTER TABLE dbo.TSql_Company
        ADD CONSTRAINT FK_TSql_Company_TSql_Language
            FOREIGN KEY (LinkLanguage) REFERENCES dbo.TSql_Language (IdObject);
END
GO

PRINT N'OK — dbo.TSql_Company.LinkLanguage (FK idioma UI por empresa).';
GO
