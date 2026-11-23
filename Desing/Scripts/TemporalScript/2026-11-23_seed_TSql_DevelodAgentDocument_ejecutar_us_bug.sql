/* =============================================================================
   dbo.TSql_DevelodAgentDocument — «Ejecutar US-Bug»
   Orden para que cualquier agente valore o arregle una ficha del registro
   de US y Bugs (Docs/areas/intranet/us-bugs-agente.md). Idempotente.
   ============================================================================= */
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

DECLARE @Label NVARCHAR(500) = N'Ejecutar US-Bug';
DECLARE @Code NVARCHAR(100) = N'intranet.ejecutar-us-bug';
DECLARE @Command NVARCHAR(1000) = N'Lee AGENTS.md y Docs/areas/intranet/us-bugs-agente.md y arregla el bug o la US id <N>.';
DECLARE @Path NVARCHAR(300) = N'Docs/areas/intranet/us-bugs-agente.md';
DECLARE @Desc NVARCHAR(MAX) = N'Vale para cualquier agente (Devin, Cursor, VS Code/Copilot). Para solo valorar: «…y valora la id <N>». El agente lee la ficha y sus capturas, guarda la valoración y, si es pequeña o mediana, la pasa a «En proceso», la arregla en develo, compila y anota en la ficha qué cambió y cómo probarlo. Las grandes solo se valoran. No cierra la ficha: la cierra quien la prueba.';
DECLARE @Order INT = 240;

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

    PRINT N'OK — Ejecutar US-Bug actualizado (o ya coincidía).';
END
ELSE
BEGIN
    INSERT INTO dbo.TSql_DevelodAgentDocument
        (TextLabel, TextCode, TextCommand, TextPath, TextDescription, NumberOrder,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (@Label, @Code, @Command, @Path, @Desc, @Order,
         0, 1, @By, NULL, GETDATE(), NULL, 0);

    PRINT N'OK — Ejecutar US-Bug insertado.';
END;
