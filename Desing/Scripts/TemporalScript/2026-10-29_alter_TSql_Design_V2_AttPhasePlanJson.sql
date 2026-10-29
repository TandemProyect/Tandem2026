/* =============================================================================
   dbo.TSql_Design_V2 — plan de encofrado por fases del diseño.
   No es tabla nueva: la entidad ya existe (Att* / Sys*).
   La cámara del visor queda en AttViewerStateJson.
   AttPhasePlanJson guarda fases, equipos, muros, material en el suelo
   y la cámara del cuadro de fases.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NULL
BEGIN
    PRINT N'— dbo.TSql_Design_V2 no está en esta base. AttPhasePlanJson no se añade.';
END
ELSE IF COL_LENGTH(N'dbo.TSql_Design_V2', N'AttPhasePlanJson') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_Design_V2]
        ADD [AttPhasePlanJson] NVARCHAR(MAX) NULL;
    PRINT N'OK — dbo.TSql_Design_V2.AttPhasePlanJson creada.';
END
ELSE
BEGIN
    PRINT N'OK — dbo.TSql_Design_V2.AttPhasePlanJson ya existía.';
END
GO

IF OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NULL
BEGIN
    PRINT N'— dbo.TSql_Design_V2 no está en esta base. AttViewerStateJson no se añade.';
END
ELSE IF COL_LENGTH(N'dbo.TSql_Design_V2', N'AttViewerStateJson') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_Design_V2]
        ADD [AttViewerStateJson] NVARCHAR(MAX) NULL;
    PRINT N'OK — dbo.TSql_Design_V2.AttViewerStateJson creada.';
END
ELSE
BEGIN
    PRINT N'OK — dbo.TSql_Design_V2.AttViewerStateJson ya existía.';
END
GO
