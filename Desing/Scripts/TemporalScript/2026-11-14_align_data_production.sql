-- 2026-11-14 pt109u: copia de filas de referencia (catálogos pequeños) de db_a197cd_desingdevelop a db_a197cd_desingproducction.
-- Solo tablas nuevas en destino. Idempotente: inserta solo los Id que no existan. Conserva los Id originales.
-- Ejecutar DESPUÉS de 2026-11-14_align_schema_production.sql: sqlcmd -S <srv> -E -d db_a197cd_desingproducction -I -x -f 65001 -b -i <este archivo>
SET NOCOUNT ON;
GO
IF OBJECT_ID(N'dbo.tbDCulture', N'U') IS NOT NULL
BEGIN TRY
  INSERT INTO dbo.[tbDCulture] ([SysObjectID], [AttCode], [AttDescription], [AttIsActive], [AttLastModification], [AttIsDefaultForSubscription], [AttFlagPosition])
  SELECT CAST(v.[SysObjectID] AS bigint) , CAST(v.[AttCode] AS nvarchar(10)) , CAST(v.[AttDescription] AS nvarchar(250)) , CAST(v.[AttIsActive] AS bit) , CAST(v.[AttLastModification] AS datetime) , CAST(v.[AttIsDefaultForSubscription] AS bit) , CAST(v.[AttFlagPosition] AS nvarchar(200)) 
  FROM (VALUES
    (10000000, N'en', N'English', 1, N'2016-01-01T00:00:00', 1, N'-144px -154px'),
    (10000001, N'es', N'Spanish', 1, N'2016-01-01T00:00:00', 0, N'-16px -44px'),
    (10000002, N'de', N'German', 1, N'2016-01-01T00:00:00', 0, N'-80px -33px'),
    (10000003, N'ru', N'Russian', 1, N'2018-04-12T15:32:50.923', 0, N'-224px -121px'),
    (10010005, N'pl', N'Polish', 1, N'2019-01-31T15:02:06.347', 0, NULL),
    (10020003, N'da', N'Danish', 1, N'2019-08-12T12:09:03.473', 0, N'-112px -33px'),
    (10020004, N'sv', N'Swedish', 1, N'2020-09-18T12:57:32.593', 0, N'-80px -132px')
  ) v ([SysObjectID], [AttCode], [AttDescription], [AttIsActive], [AttLastModification], [AttIsDefaultForSubscription], [AttFlagPosition])
  WHERE NOT EXISTS (SELECT 1 FROM dbo.[tbDCulture] t WHERE t.[SysObjectID] = v.[SysObjectID]);
  PRINT 'COPY tbDCulture: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' filas';
END TRY
BEGIN CATCH
  PRINT 'SKIP COPY tbDCulture: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.TSql_LanguageConcept', N'U') IS NOT NULL
BEGIN TRY
  INSERT INTO dbo.[TSql_LanguageConcept] ([SysObjectID], [AttLanguageConcept], [LinCreatedBy], [AttCreated], [LinModifiedBy], [AttLastModification], [SysUpdateNumber], [AttIsDeleted], [Attfas])
  SELECT CAST(v.[SysObjectID] AS bigint) , CAST(v.[AttLanguageConcept] AS nvarchar(500)) , CAST(v.[LinCreatedBy] AS nvarchar(128)) , CAST(v.[AttCreated] AS datetime) , CAST(v.[LinModifiedBy] AS nvarchar(128)) , CAST(v.[AttLastModification] AS datetime) , CAST(v.[SysUpdateNumber] AS bigint) , CAST(v.[AttIsDeleted] AS bit) , CAST(v.[Attfas] AS nvarchar(500)) 
  FROM (VALUES
    (10002, N'Employee', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', N'2022-01-01T00:00:00', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', N'2022-01-01T00:00:00', 1, 0, N'fas fa-user-friends')
  ) v ([SysObjectID], [AttLanguageConcept], [LinCreatedBy], [AttCreated], [LinModifiedBy], [AttLastModification], [SysUpdateNumber], [AttIsDeleted], [Attfas])
  WHERE NOT EXISTS (SELECT 1 FROM dbo.[TSql_LanguageConcept] t WHERE t.[SysObjectID] = v.[SysObjectID]);
  PRINT 'COPY TSql_LanguageConcept: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' filas';
END TRY
BEGIN CATCH
  PRINT 'SKIP COPY TSql_LanguageConcept: ' + ERROR_MESSAGE();
END CATCH
GO
IF OBJECT_ID(N'dbo.TSql_LanguageDetails', N'U') IS NOT NULL
BEGIN TRY
  INSERT INTO dbo.[TSql_LanguageDetails] ([SysObjectID], [AttLabel], [LinLanguageConcept], [Linlanguage], [LinCreatedBy], [AttCreated], [LinModifiedBy], [AttLastModification], [SysUpdateNumber], [AttIsDeleted])
  SELECT CAST(v.[SysObjectID] AS bigint) , CAST(v.[AttLabel] AS nvarchar(500)) , CAST(v.[LinLanguageConcept] AS bigint) , CAST(v.[Linlanguage] AS bigint) , CAST(v.[LinCreatedBy] AS nvarchar(128)) , CAST(v.[AttCreated] AS datetime) , CAST(v.[LinModifiedBy] AS nvarchar(128)) , CAST(v.[AttLastModification] AS datetime) , CAST(v.[SysUpdateNumber] AS bigint) , CAST(v.[AttIsDeleted] AS bit) 
  FROM (VALUES
    (10003, N'Index_New_User', 10002, 10004, N'1287a994-0fb7-4b08-bdf7-de3d936cedca', N'2022-01-01T00:00:00', N'1287a994-0fb7-4b08-bdf7-de3d936cedca', N'2022-01-01T00:00:00', 1, 0)
  ) v ([SysObjectID], [AttLabel], [LinLanguageConcept], [Linlanguage], [LinCreatedBy], [AttCreated], [LinModifiedBy], [AttLastModification], [SysUpdateNumber], [AttIsDeleted])
  WHERE NOT EXISTS (SELECT 1 FROM dbo.[TSql_LanguageDetails] t WHERE t.[SysObjectID] = v.[SysObjectID]);
  PRINT 'COPY TSql_LanguageDetails: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' filas';
END TRY
BEGIN CATCH
  PRINT 'SKIP COPY TSql_LanguageDetails: ' + ERROR_MESSAGE();
END CATCH
GO
PRINT 'FIN datos';
GO
