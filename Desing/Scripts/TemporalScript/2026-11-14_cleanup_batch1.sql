/* =============================================================================
  2026-11-14 pt111u — Limpieza, lote 1 (develop y producción deben quedar iguales).

  Borra SOLO objetos heredados (script 2026-05-16_drop_unused_legacy_removed_from_edmx.sql)
  con 0 referencias en código y en Model.edmx, y sin datos:
    - 16 vistas de prueba (solo existían en develop).
    - 2 tablas vacías: TSql_UserBranch, TSql_Client.

  NO borra (tienen filas originales en develop; ver base-de-datos-alineacion.md §5):
    perierase, temporalmaestro, Maestro_articulos_temporal, Movimientos, TSql_Branch1,
    TSql_Business, TSql_Business1, TSql_Comercial1, tbDCulture, TSql_LanguageConcept,
    TSql_LanguageDetails y las 9 columnas antiguas de TSql_Language.

  Idempotente. Seguridad: una tabla con filas NO se borra (PRINT SKIP).
  FKs que apunten a la tabla o salgan de ella se borran antes.
  Ejecutar en cada base: sqlcmd -S <srv> -E -d <db> -I -x -f 65001 -b -i <este archivo>
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ----- 1. Vistas de prueba ----- */
IF OBJECT_ID(N'dbo.[AminData]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[AminData];
  PRINT N'DROP VIEW AminData';
END
IF OBJECT_ID(N'dbo.[Angel_01_Net_user]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Angel_01_Net_user];
  PRINT N'DROP VIEW Angel_01_Net_user';
END
IF OBJECT_ID(N'dbo.[Angel_02_Employee]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Angel_02_Employee];
  PRINT N'DROP VIEW Angel_02_Employee';
END
IF OBJECT_ID(N'dbo.[Angel_03_DefaulDesing]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Angel_03_DefaulDesing];
  PRINT N'DROP VIEW Angel_03_DefaulDesing';
END
IF OBJECT_ID(N'dbo.[Desing_details]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Desing_details];
  PRINT N'DROP VIEW Desing_details';
END
IF OBJECT_ID(N'dbo.[Empledos]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Empledos];
  PRINT N'DROP VIEW Empledos';
END
IF OBJECT_ID(N'dbo.[Employee]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Employee];
  PRINT N'DROP VIEW Employee';
END
IF OBJECT_ID(N'dbo.[Funciones de usuari]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Funciones de usuari];
  PRINT N'DROP VIEW Funciones de usuari';
END
IF OBJECT_ID(N'dbo.[Master articles]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Master articles];
  PRINT N'DROP VIEW Master articles';
END
IF OBJECT_ID(N'dbo.[MasterArticle]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[MasterArticle];
  PRINT N'DROP VIEW MasterArticle';
END
IF OBJECT_ID(N'dbo.[Personal]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Personal];
  PRINT N'DROP VIEW Personal';
END
IF OBJECT_ID(N'dbo.[Q]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Q];
  PRINT N'DROP VIEW Q';
END
IF OBJECT_ID(N'dbo.[Register]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[Register];
  PRINT N'DROP VIEW Register';
END
IF OBJECT_ID(N'dbo.[View_1]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[View_1];
  PRINT N'DROP VIEW View_1';
END
IF OBJECT_ID(N'dbo.[View_2]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[View_2];
  PRINT N'DROP VIEW View_2';
END
IF OBJECT_ID(N'dbo.[view_name]', N'V') IS NOT NULL
BEGIN
  DROP VIEW dbo.[view_name];
  PRINT N'DROP VIEW view_name';
END
GO

/* ----- 2. Tablas vacías (primero sus FK) ----- */
IF OBJECT_ID(N'dbo.TSql_UserBranch', N'U') IS NOT NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_UserBranch])
    PRINT N'SKIP TABLE TSql_UserBranch: tiene filas';
  ELSE
  BEGIN
    DECLARE @sql nvarchar(max) = N'';
    SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                 + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N'; PRINT N''DROP FK ' + REPLACE(fk.name, N'''', N'''''') + N''';'
    FROM sys.foreign_keys fk
    WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.TSql_UserBranch') OR fk.parent_object_id = OBJECT_ID(N'dbo.TSql_UserBranch');
    IF @sql <> N'' EXEC sys.sp_executesql @sql;
    DROP TABLE dbo.[TSql_UserBranch];
    PRINT N'DROP TABLE TSql_UserBranch';
  END
END
GO
IF OBJECT_ID(N'dbo.TSql_Client', N'U') IS NOT NULL
BEGIN
  IF EXISTS (SELECT 1 FROM dbo.[TSql_Client])
    PRINT N'SKIP TABLE TSql_Client: tiene filas';
  ELSE
  BEGIN
    DECLARE @sql nvarchar(max) = N'';
    SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                 + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N'; PRINT N''DROP FK ' + REPLACE(fk.name, N'''', N'''''') + N''';'
    FROM sys.foreign_keys fk
    WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.TSql_Client') OR fk.parent_object_id = OBJECT_ID(N'dbo.TSql_Client');
    IF @sql <> N'' EXEC sys.sp_executesql @sql;
    DROP TABLE dbo.[TSql_Client];
    PRINT N'DROP TABLE TSql_Client';
  END
END
GO
PRINT N'FIN cleanup batch1';
GO
