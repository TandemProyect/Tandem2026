/* =============================================================================
   dbo.TSql_DevelodAgentDocument — «Plano de montaje»
   Orden para que cualquier agente continúe la US «Generar plano de montaje»
   (Docs/areas/encofrado-atk60/plano-montaje.md). Idempotente: develop y producción.
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

DECLARE @Label NVARCHAR(500) = N'Plano de montaje';
DECLARE @Code NVARCHAR(100) = N'encofrado.plano-montaje';
DECLARE @Command NVARCHAR(1000) = N'Lee AGENTS.md y Docs/areas/encofrado-atk60/plano-montaje.md y continúa el plano de montaje: <lo que quieres>.';
DECLARE @Path NVARCHAR(300) = N'Docs/areas/encofrado-atk60/plano-montaje.md';
DECLARE @Desc NVARCHAR(MAX) = N'US «Generar plano de montaje» en el visor Desing_2. Hecho: tablas TSql_DesignAssemblyView y TSql_DesignAssemblyViewElement (develop y producción) y columna TSql_DesignWallArticle.NumberFaceId; icono junto a «Oferta cliente» que genera una vista por cara (F-n) de cada muro; ventana con la lista de vistas; visor de la cara (el del recuento) con cotas de paneles, largo total y alturas, línea de tierra y botón de colores; etiquetas F-n en la escena y cara abierta resaltada; las piezas de muros borrados no entran. La ficha tiene el mapa de archivos, los convenios (Z de escena = −Y de CAD), las pruebas y lo pendiente: el plano imprimible con planta, perspectiva y vistas laterales.';
DECLARE @Order INT = 135;

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

    PRINT N'OK — Plano de montaje actualizado (o ya coincidía).';
END
ELSE
BEGIN
    INSERT INTO dbo.TSql_DevelodAgentDocument
        (TextLabel, TextCode, TextCommand, TextPath, TextDescription, NumberOrder,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (@Label, @Code, @Command, @Path, @Desc, @Order,
         0, 1, @By, NULL, GETDATE(), NULL, 0);

    PRINT N'OK — Plano de montaje insertado.';
END;
