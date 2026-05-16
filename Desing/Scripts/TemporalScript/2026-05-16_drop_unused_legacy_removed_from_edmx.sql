/* =============================================================================
  Drops DB objects that were REMOVED from DAL/Model.edmx (2026-05-16 cleanup).

  Idempotent: OBJECT_ID guards per object.

  IMPORTANT
  - Review FK dependencies in YOUR database before running. If DROP fails,
    drop dependent FKs first or reorder sections.
  - Do NOT run against production without backup.
  - Legacy client table dropped here is dbo.TSql_Client (pre–Client_V2).
    Intranet dbo.TSql_Client_V2 is NOT included.

  Optional: dbo.sysdiagrams is commented out (SSMS database diagrams).
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ----- Views (removed from EF model) ----- */
IF OBJECT_ID(N'dbo.AminData', N'V') IS NOT NULL DROP VIEW dbo.AminData;
IF OBJECT_ID(N'dbo.Angel_01_Net_user', N'V') IS NOT NULL DROP VIEW dbo.Angel_01_Net_user;
IF OBJECT_ID(N'dbo.Angel_02_Employee', N'V') IS NOT NULL DROP VIEW dbo.Angel_02_Employee;
IF OBJECT_ID(N'dbo.Angel_03_DefaulDesing', N'V') IS NOT NULL DROP VIEW dbo.Angel_03_DefaulDesing;
IF OBJECT_ID(N'dbo.Desing_details', N'V') IS NOT NULL DROP VIEW dbo.Desing_details;
IF OBJECT_ID(N'dbo.Empledos', N'V') IS NOT NULL DROP VIEW dbo.Empledos;
IF OBJECT_ID(N'dbo.Employee', N'V') IS NOT NULL DROP VIEW dbo.Employee;
IF OBJECT_ID(N'dbo.[Funciones de usuari]', N'V') IS NOT NULL DROP VIEW dbo.[Funciones de usuari];
IF OBJECT_ID(N'dbo.MasterArticle', N'V') IS NOT NULL DROP VIEW dbo.MasterArticle;
IF OBJECT_ID(N'dbo.Personal', N'V') IS NOT NULL DROP VIEW dbo.Personal;
IF OBJECT_ID(N'dbo.Q', N'V') IS NOT NULL DROP VIEW dbo.Q;
IF OBJECT_ID(N'dbo.Register', N'V') IS NOT NULL DROP VIEW dbo.Register;
IF OBJECT_ID(N'dbo.View_1', N'V') IS NOT NULL DROP VIEW dbo.View_1;
IF OBJECT_ID(N'dbo.View_2', N'V') IS NOT NULL DROP VIEW dbo.View_2;
IF OBJECT_ID(N'dbo.view_name', N'V') IS NOT NULL DROP VIEW dbo.view_name;
GO

/* ----- Tables: prefer children / junctions first (adjust if FK errors) ----- */
IF OBJECT_ID(N'dbo.TSql_LanguageDetails', N'U') IS NOT NULL DROP TABLE dbo.TSql_LanguageDetails;
IF OBJECT_ID(N'dbo.TSql_LanguageConcept', N'U') IS NOT NULL DROP TABLE dbo.TSql_LanguageConcept;
IF OBJECT_ID(N'dbo.TSql_language', N'U') IS NOT NULL DROP TABLE dbo.TSql_language;
IF OBJECT_ID(N'dbo.TSql_UserBranch', N'U') IS NOT NULL DROP TABLE dbo.TSql_UserBranch;

IF OBJECT_ID(N'dbo.Maestro_articulos_temporal', N'U') IS NOT NULL DROP TABLE dbo.Maestro_articulos_temporal;
IF OBJECT_ID(N'dbo.Movimientos', N'U') IS NOT NULL DROP TABLE dbo.Movimientos;
IF OBJECT_ID(N'dbo.perierase', N'U') IS NOT NULL DROP TABLE dbo.perierase;
IF OBJECT_ID(N'dbo.temporalmaestro', N'U') IS NOT NULL DROP TABLE dbo.temporalmaestro;

IF OBJECT_ID(N'dbo.TSql_Branch1', N'U') IS NOT NULL DROP TABLE dbo.TSql_Branch1;
IF OBJECT_ID(N'dbo.TSql_Business1', N'U') IS NOT NULL DROP TABLE dbo.TSql_Business1;
IF OBJECT_ID(N'dbo.TSql_Business', N'U') IS NOT NULL DROP TABLE dbo.TSql_Business;
IF OBJECT_ID(N'dbo.TSql_Comercial1', N'U') IS NOT NULL DROP TABLE dbo.TSql_Comercial1;
IF OBJECT_ID(N'dbo.TSql_Client', N'U') IS NOT NULL DROP TABLE dbo.TSql_Client;

IF OBJECT_ID(N'dbo.tbDCulture', N'U') IS NOT NULL DROP TABLE dbo.tbDCulture;
GO

/*
-- Uncomment ONLY if you intentionally remove SSMS database diagrams storage:
-- IF OBJECT_ID(N'dbo.sysdiagrams', N'U') IS NOT NULL DROP TABLE dbo.sysdiagrams;
*/
GO

PRINT N'OK — drop script finished (see messages above if any step was skipped).';
GO
