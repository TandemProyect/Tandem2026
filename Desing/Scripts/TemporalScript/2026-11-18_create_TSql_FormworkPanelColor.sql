/* =============================================================================
   dbo.TSql_FormworkPanelColor — color de panel por sistema y ancho.
   Catálogo (esquema + filas ATK60 + función de menú) para develop y producción.
   Idempotente. No borra filas. El ancho se guarda en milímetros (0,90 m = 900).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_FormworkPanelColor'
)
BEGIN
    CREATE TABLE [dbo].[TSql_FormworkPanelColor]
    (
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_FormworkPanelColor PRIMARY KEY CLUSTERED,

        [TextLabel]           NVARCHAR(500)   NOT NULL,
        [LinkSystem]          BIGINT          NOT NULL,
        [NumberWidthMm]       INT             NOT NULL,
        [TextColor]           NVARCHAR(7)     NOT NULL,

        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_FormworkPanelColor_Is_Delete         DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_FormworkPanelColor_Is_Active         DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_FormworkPanelColor_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_FormworkPanelColor_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_FormworkPanelColor_System'
)
AND OBJECT_ID(N'dbo.TSql_System', N'U') IS NOT NULL
AND EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.TSql_System') AND is_primary_key = 1
)
    ALTER TABLE [dbo].[TSql_FormworkPanelColor]
        ADD CONSTRAINT FK_TSql_FormworkPanelColor_System
        FOREIGN KEY ([LinkSystem]) REFERENCES [dbo].[TSql_System] ([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_FormworkPanelColor_System_Width'
      AND object_id = OBJECT_ID(N'dbo.TSql_FormworkPanelColor')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_FormworkPanelColor_System_Width
        ON [dbo].[TSql_FormworkPanelColor] ([LinkSystem], [NumberWidthMm])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_FormworkPanelColor_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_FormworkPanelColor')
)
    CREATE NONCLUSTERED INDEX IX_TSql_FormworkPanelColor_TextLabel_Active
        ON [dbo].[TSql_FormworkPanelColor] ([TextLabel])
        INCLUDE ([Is_Active])
        WHERE [Is_Delete] = 0;
GO

DECLARE @User NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

DECLARE @System BIGINT =
(
    SELECT TOP (1) s.IdObject
      FROM dbo.TSql_System s
     WHERE s.AddIsActive = 1
       AND REPLACE(REPLACE(UPPER(LTRIM(RTRIM(s.TextLabel))), N'-', N''), N' ', N'') = N'ATK60'
     ORDER BY s.IdObject
);

IF @System IS NULL
    PRINT N'AVISO — no hay sistema ATK60 activo; no se siembran colores.';
ELSE
BEGIN
    DECLARE @Seed TABLE (WidthMm INT NOT NULL, Color NCHAR(7) NOT NULL, Label NVARCHAR(500) NOT NULL);
    INSERT INTO @Seed (WidthMm, Color, Label) VALUES
        (900, N'#000000', N'Atk60 · 0,90 m'),
        (600, N'#FF0000', N'Atk60 · 0,60 m'),
        (450, N'#0000FF', N'Atk60 · 0,45 m'),
        (300, N'#FF00FF', N'Atk60 · 0,30 m'),
        (750, N'#00FF00', N'Atk60 · 0,75 m');

    INSERT INTO dbo.TSql_FormworkPanelColor
        (TextLabel, LinkSystem, NumberWidthMm, TextColor,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    SELECT seed.Label, @System, seed.WidthMm, seed.Color,
           0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Seed seed
     WHERE NOT EXISTS (
        SELECT 1 FROM dbo.TSql_FormworkPanelColor x
         WHERE x.Is_Delete = 0 AND x.LinkSystem = @System AND x.NumberWidthMm = seed.WidthMm
     );
END
GO

DECLARE @UserFn NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

IF OBJECT_ID(N'dbo.TSql_Functions', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND TextCode = N'FormworkPanelColor')
        INSERT INTO dbo.TSql_Functions
            (TextLabel, TextCode, TextModule, TextDescription,
             Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
        VALUES
            (N'Develop › Colores de panel', N'FormworkPanelColor', N'Develop',
             N'Catálogo de color de panel por sistema y ancho.',
             0, 1, @UserFn, NULL, GETDATE(), NULL, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND TextCode = N'FormworkPanelColor.Index')
        INSERT INTO dbo.TSql_Functions
            (TextLabel, TextCode, TextModule, TextDescription,
             Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
        VALUES
            (N'Develop › Colores de panel › listado', N'FormworkPanelColor.Index', N'Develop',
             N'Listado del catálogo de color de panel.',
             0, 1, @UserFn, NULL, GETDATE(), NULL, 0);

    INSERT INTO dbo.TSql_RoleFunction
        (TextLabel, LinkRoles_V2, LinkFunctions,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
           0, 1, @UserFn, NULL, GETDATE(), NULL, 0
      FROM dbo.TSql_Roles_V2 r
      JOIN dbo.TSql_Functions f
        ON f.Is_Delete = 0 AND f.TextCode IN (N'FormworkPanelColor', N'FormworkPanelColor.Index')
     WHERE r.Is_Delete = 0
       AND (
            r.TextCode = N'DEVELOP'
            OR EXISTS (
                SELECT 1
                  FROM dbo.TSql_RoleFunction has
                  JOIN dbo.TSql_Functions df ON df.IdObject = has.LinkFunctions AND df.Is_Delete = 0
                 WHERE has.Is_Delete = 0 AND has.LinkRoles_V2 = r.IdObject AND df.TextCode = N'DevelopAi'
            )
       )
       AND NOT EXISTS (
            SELECT 1 FROM dbo.TSql_RoleFunction x
             WHERE x.Is_Delete = 0 AND x.LinkRoles_V2 = r.IdObject AND x.LinkFunctions = f.IdObject
       );
END

DECLARE @Lang BIGINT =
(
    SELECT TOP (1) l.IdObject FROM dbo.TSql_Language l
     WHERE l.Is_Default = 1 AND l.Is_Delete = 0 AND l.Is_Active = 1
     ORDER BY l.IdObject
);

IF @Lang IS NOT NULL AND OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM dbo.TSql_UiTranslation t
     WHERE t.TextResourceKey = N'Menu.FormworkPanelColor' AND t.LinkLanguage = @Lang AND t.Is_Delete = 0
)
    INSERT INTO dbo.TSql_UiTranslation
        (TextResourceKey, TextModule, LinkLanguage, TextValue,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Menu.FormworkPanelColor', N'Common', @Lang, N'Colores de panel',
         0, 1, @UserFn, NULL, GETDATE(), NULL, 0);
GO

PRINT N'OK — dbo.TSql_FormworkPanelColor creada (o ya existía).';
GO
