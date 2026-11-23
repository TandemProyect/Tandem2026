/* =============================================================================
   IA del diseño: insertar muro (punto + L×e×h) y pilar (22/11/2026). Idempotente.
   · WALL_DRAW: ejemplos de inserción (ángulo 0 si no se dice).
   · TOOL_PILLARS: handler ai:pillar.insert, implementado; ejemplos.
   Requisitos: catálogo del 14/11/2026.
   Ejecutar en develop y producción (catálogo TSql_Ai*).
   sqlcmd -S 'NUCBOXG5\SQLEXPRESS' -E -d TdesingDevelop -I -f 65001 -b -i <este fichero>
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @User NVARCHAR(128) =
(
    SELECT Id FROM dbo.AspNetUsers WHERE UserName = N'juan.godoy@vscad.com'
);
IF @User IS NULL
    SET @User = (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id);

IF @User IS NULL
    PRINT N'Aviso: AspNetUsers vacío — semilla insertar muro/pilar omitida.';
ELSE IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NULL OR OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NULL
    PRINT N'Aviso: faltan las tablas del catálogo de la IA.';
ELSE
BEGIN
    BEGIN TRANSACTION;

    UPDATE dbo.TSql_AiCommand
       SET TextHandler = N'ai:pillar.insert',
           TextAiCommandId = N'pillar.insert',
           Is_AiImplemented = 1,
           TextDescription = N'Inserta un pilar en un punto (0,0,0 = origen). Largo × ancho × alto; ángulo 0 si no se dice. Lado 0,10–0,65 m.',
           TextParamsJson = N'[{"name":"punto","required":false,"help":"«en 0,0,0» (por defecto el origen)"},{"name":"sección y alto","required":true,"help":"«0,9 x 0,9 x 5»"},{"name":"ángulo","required":false,"help":"«a 30 grados» (por defecto 0)"}]',
           LinModifiedBy = @User,
           AddLastDateChange = GETDATE(),
           Ntimeschanged = ISNULL(Ntimeschanged, 0) + 1
     WHERE TextCode = N'TOOL_PILLARS' AND Is_Delete = 0
       AND (TextAiCommandId IS NULL OR TextAiCommandId <> N'pillar.insert' OR Is_AiImplemented = 0
            OR TextHandler NOT LIKE N'ai:pillar.insert%');

    DECLARE @Phr TABLE
    (
        TextCode NVARCHAR(60) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextPhrase NVARCHAR(200) NOT NULL,
        TextPhraseNorm NVARCHAR(200) NOT NULL, TextKind NVARCHAR(20) NOT NULL, TextConcept NVARCHAR(40) NULL, NumberOrder INT NOT NULL
    );
    INSERT INTO @Phr (TextCode, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberOrder)
    VALUES
        (N'WALL_DRAW', N'es', N'inserta un muro en el punto 0,0,0 con 10.03 mts de longitud 0,30 de ancho y 3,12 de altura',
         N'inserta un muro en el punto 0 0 0 con 10.03 mts de longitud 0 30 de ancho y 3 12 de altura', N'example', NULL, 410),
        (N'WALL_DRAW', N'es', N'Inserta muro 10 x 0,30 x 3,12',
         N'inserta muro 10 x 0 30 x 3 12', N'example', NULL, 420),
        (N'WALL_DRAW', N'es', N'Inserta un muro en 0,0,0 de 10 x 0,30 x 4,0 a 30 grados',
         N'inserta un muro en 0 0 0 de 10 x 0 30 x 4 0 a 30 grados', N'example', NULL, 430),
        (N'WALL_DRAW', N'en', N'insert wall 10 x 0.30 x 3.12',
         N'insert wall 10 x 0.30 x 3.12', N'example', NULL, 440),
        (N'WALL_DRAW', N'en', N'insert a wall at 0,0,0 of 10 x 0.30 x 4.0 at 30 degrees',
         N'insert a wall at 0 0 0 of 10 x 0.30 x 4.0 at 30 degrees', N'example', NULL, 450),
        (N'TOOL_PILLARS', N'es', N'inserta un pilar en 0,0,0 de 0,9 x 0,9 x 5',
         N'inserta un pilar en 0 0 0 de 0 9 x 0 9 x 5', N'example', NULL, 70),
        (N'TOOL_PILLARS', N'es', N'inserta un pilar 0,30 x 0,30 x 3',
         N'inserta un pilar 0 30 x 0 30 x 3', N'example', NULL, 80),
        (N'TOOL_PILLARS', N'en', N'insert a pillar at 0,0,0 of 0.9 x 0.9 x 5',
         N'insert a pillar at 0 0 0 of 0.9 x 0.9 x 5', N'example', NULL, 90);

    SELECT S.TextCode, S.TextLanguageCode, S.TextPhraseNorm, OC.TextCode AS YaEnComando
      FROM @Phr S
      JOIN dbo.TSql_AiCommand C ON C.TextCode = S.TextCode AND C.Is_Delete = 0
      JOIN dbo.TSql_AiCommandPhrase O ON O.TextContext = C.TextContext AND O.TextLanguageCode = S.TextLanguageCode
                                     AND O.TextPhraseNorm = S.TextPhraseNorm AND O.Is_Delete = 0
                                     AND O.TextKind IN (N'keyword', N'example') AND O.LinkAiCommand <> C.IdObject
      JOIN dbo.TSql_AiCommand OC ON OC.IdObject = O.LinkAiCommand
     WHERE S.TextKind IN (N'keyword', N'example');

    INSERT INTO dbo.TSql_AiCommandPhrase
    (
        LinkAiCommand, TextContext, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberWeight, NumberOrder, LinkEnvironment,
        TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
    )
    SELECT C.IdObject, C.TextContext, S.TextLanguageCode, S.TextPhrase, S.TextPhraseNorm, S.TextKind, S.TextConcept, NULL, S.NumberOrder, NULL,
           S.TextPhrase, 0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Phr S
      JOIN dbo.TSql_AiCommand C ON C.TextCode = S.TextCode AND C.Is_Delete = 0
     WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_AiCommandPhrase T
                        WHERE T.LinkAiCommand = C.IdObject AND T.TextLanguageCode = S.TextLanguageCode
                          AND T.TextPhraseNorm = S.TextPhraseNorm AND T.Is_Delete = 0)
       AND (S.TextKind NOT IN (N'keyword', N'example') OR NOT EXISTS (
               SELECT 1 FROM dbo.TSql_AiCommandPhrase O
                WHERE O.TextContext = C.TextContext AND O.TextLanguageCode = S.TextLanguageCode
                  AND O.TextPhraseNorm = S.TextPhraseNorm AND O.Is_Delete = 0 AND O.TextKind IN (N'keyword', N'example')));

    COMMIT TRANSACTION;
    PRINT N'OK — insertar muro/pilar en catálogo IA.';
END
GO
