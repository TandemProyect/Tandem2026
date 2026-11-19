SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.TSql_DevelodAgentDocument', N'U') IS NULL
BEGIN
    PRINT N'WARN — No existe dbo.TSql_DevelodAgentDocument.';
    RETURN;
END;

DECLARE @By NVARCHAR(128) =
(
    SELECT TOP (1) LinkMadeBy
      FROM dbo.TSql_DevelodAgentDocument
     WHERE Is_Delete = 0
     ORDER BY IdObject DESC
);

IF @By IS NULL OR LTRIM(RTRIM(@By)) = N''
    SET @By = N'system';

IF NOT EXISTS (
    SELECT 1
      FROM dbo.TSql_DevelodAgentDocument
     WHERE Is_Delete = 0
       AND TextCode = N'area.oferta-diseno'
)
BEGIN
    INSERT INTO dbo.TSql_DevelodAgentDocument
        (TextLabel, TextCode, TextCommand, TextPath, TextDescription, NumberOrder,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Oferta Diseño',
         N'area.oferta-diseno',
         N'Lee AGENTS.md y trabaja en Oferta Diseño: <lo que quieres>.',
         N'Docs/areas/general/Oferta-Diseño.md',
         N'Continuidad de Oferta Cliente desde diseño: plantillas, contactos/estados, capturas del visor y PDF.',
         431,
         0, 1, @By, NULL, GETDATE(), NULL, 0);
END;

PRINT N'OK — Agente Oferta Diseño sincronizado (o ya existía).';
