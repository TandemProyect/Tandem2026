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
    COALESCE(
        (SELECT TOP (1) Id FROM dbo.AspNetUsers WHERE UserName = N'juan.godoy@vscad.com'),
        (SELECT TOP (1) LinkMadeBy FROM dbo.TSql_DevelodAgentDocument WHERE Is_Delete = 0 ORDER BY IdObject DESC),
        N'system');

DECLARE @Label NVARCHAR(500) = N'Generar Informe Develop';
DECLARE @Code NVARCHAR(100) = N'informe.generar-develop';
DECLARE @Command NVARCHAR(1000) = N'Lee AGENTS.md y genera el informe Develop: <rango o últimas dos semanas>.';
DECLARE @Path NVARCHAR(300) = N'Docs/areas/general/Generar-Informe-Develop.md';
DECLARE @Desc NVARCHAR(MAX) = N'Cómo generar el informe semanal US/BUG y el PDF: fichero abierto, fuentes (chats, no git), convención US azul / BUG rojo, Chrome headless y semilla en Agentes.';
DECLARE @Order INT = 510;

IF EXISTS (
    SELECT 1 FROM dbo.TSql_DevelodAgentDocument
     WHERE Is_Delete = 0 AND TextCode = @Code
)
BEGIN
    UPDATE dbo.TSql_DevelodAgentDocument
       SET TextLabel = @Label,
           TextCommand = @Command,
           TextPath = @Path,
           TextDescription = @Desc,
           NumberOrder = @Order,
           LinModifiedBy = @By,
           AddLastDateChange = GETDATE(),
           Ntimeschanged = Ntimeschanged + 1
     WHERE Is_Delete = 0
       AND TextCode = @Code
       AND (
            TextLabel <> @Label OR TextCommand <> @Command
            OR ISNULL(TextPath, N'') <> ISNULL(@Path, N'')
            OR ISNULL(TextDescription, N'') <> ISNULL(@Desc, N'')
            OR NumberOrder <> @Order
       );

    PRINT N'OK — Generar Informe Develop actualizado (o ya coincidía).';
END
ELSE
BEGIN
    INSERT INTO dbo.TSql_DevelodAgentDocument
        (TextLabel, TextCode, TextCommand, TextPath, TextDescription, NumberOrder,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (@Label, @Code, @Command, @Path, @Desc, @Order,
         0, 1, @By, NULL, GETDATE(), NULL, 0);

    PRINT N'OK — Generar Informe Develop insertado.';
END;
