/* =============================================================================
   Iguala derechos y tres defaults de esquema entre develop y producción.
   Idempotente. No borra. No reescribe filas de muros ni diseños.
   - Unión de enlaces rol-función que ya existen en una de las dos bases.
   - Rol AI_ASSISTANT y función DevelopAi (si faltan).
   - juan.godoy@vscad.com recibe la unión de sus roles.
   - DEVELOP.Is_AllFunctions = 1 (así está en develop local).
   - Defaults de TSql_DesignWall e IsRenderWall, sin UPDATE de filas.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @User NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Roles_V2 WHERE Is_Delete = 0 AND TextCode = N'AI_ASSISTANT')
    INSERT INTO dbo.TSql_Roles_V2
        (TextLabel, TextCode, TextDescription, Is_System, Is_AllFunctions,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Asistente IA', N'AI_ASSISTANT', N'Puede usar los asistentes IA del visor.', 0, 0,
         0, 1, @User, NULL, GETDATE(), NULL, 0);

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND TextCode = N'DevelopAi')
    INSERT INTO dbo.TSql_Functions
        (TextLabel, TextCode, TextModule, TextDescription,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Develop › IA', N'DevelopAi', N'Develop',
         N'Menú Develop › IA y catálogo de funciones, órdenes y significado (TSql_AiCommand).',
         0, 1, @User, NULL, GETDATE(), NULL, 0);

UPDATE dbo.TSql_Roles_V2
   SET Is_AllFunctions = 1,
       LinModifiedBy = @User,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE Is_Delete = 0
   AND TextCode = N'DEVELOP'
   AND Is_AllFunctions = 0;

DECLARE @Links TABLE (RoleCode NVARCHAR(100), FunctionCode NVARCHAR(200));
INSERT INTO @Links VALUES
    (N'ADMINISTRATIVE', N'Employee'),
    (N'ADMINISTRATIVE', N'IA'),
    (N'AI_ASSISTANT',   N'IA'),
    (N'DESIGNER',       N'IA'),
    (N'DEVELOP',        N'DevelopAgent'),
    (N'DEVELOP',        N'DevelopAi'),
    (N'DEVELOP',        N'Employee'),
    (N'DEVELOP',        N'Function'),
    (N'DEVELOP',        N'IA'),
    (N'DEVELOP',        N'Role'),
    (N'DEVELOP',        N'TSql_Company'),
    (N'SECTION_HEAD',   N'IA'),
    (N'SYSADMIN',       N'IA');

INSERT INTO dbo.TSql_RoleFunction
    (TextLabel, LinkRoles_V2, LinkFunctions,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM @Links l
  JOIN dbo.TSql_Roles_V2 r ON r.TextCode = l.RoleCode AND r.Is_Delete = 0
  JOIN dbo.TSql_Functions f ON f.TextCode = l.FunctionCode AND f.Is_Delete = 0
 WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_RoleFunction x
                    WHERE x.Is_Delete = 0 AND x.LinkRoles_V2 = r.IdObject AND x.LinkFunctions = f.IdObject);

DECLARE @JuanRoles TABLE (RoleCode NVARCHAR(100));
INSERT INTO @JuanRoles VALUES
    (N'SYSADMIN'), (N'DEVELOP'), (N'ADMINISTRATIVE'), (N'AI_ASSISTANT'), (N'DESIGNER'), (N'SECTION_HEAD');

INSERT INTO dbo.TSql_UserRole
    (TextLabel, LinkAspNetUsers, LinkRoles_V2,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(u.UserName + N' · ' + r.TextLabel, 500), u.Id, r.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM dbo.AspNetUsers u
  JOIN @JuanRoles jr ON 1 = 1
  JOIN dbo.TSql_Roles_V2 r ON r.TextCode = jr.RoleCode AND r.Is_Delete = 0
 WHERE u.UserName = N'juan.godoy@vscad.com'
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_UserRole x
                    WHERE x.Is_Delete = 0 AND x.LinkAspNetUsers = u.Id AND x.LinkRoles_V2 = r.IdObject);

DECLARE @Lang BIGINT =
(
    SELECT TOP (1) l.IdObject FROM dbo.TSql_Language l
     WHERE l.Is_Default = 1 AND l.Is_Delete = 0 AND l.Is_Active = 1
     ORDER BY l.IdObject
);
IF @Lang IS NOT NULL AND OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_UiTranslation t
                    WHERE t.TextResourceKey = N'Menu.DevelopAi' AND t.LinkLanguage = @Lang AND t.Is_Delete = 0)
    INSERT INTO dbo.TSql_UiTranslation
        (TextResourceKey, TextModule, LinkLanguage, TextValue,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Menu.DevelopAi', N'Common', @Lang, N'IA',
         0, 1, @User, NULL, GETDATE(), NULL, 0);
GO

/* Defaults de esquema. No toca filas ya guardadas. */
DECLARE @fixes TABLE (TableName SYSNAME, ColumnName SYSNAME, ConstraintName SYSNAME, Definition NVARCHAR(40));
INSERT INTO @fixes VALUES
    (N'TSql_DesignWall',    N'Is_Check750R',     N'DF_TSql_DesignWall_Is_Check750R',     N'((1))'),
    (N'TSql_DesignWall',    N'Is_CheckDimWall',  N'DF_TSql_DesignWall_Is_CheckDimWall',  N'((1))'),
    (N'TSql_DesignWall',    N'Is_FormworkMode',  N'DF_TSql_DesignWall_Is_FormworkMode',  N'((1))'),
    (N'TSql_DesignDetails', N'IsRenderWall',     N'DF_TSql_DesignDetails_IsRenderWall',  N'((1))');

DECLARE @table SYSNAME, @column SYSNAME, @name SYSNAME, @definition NVARCHAR(40), @current SYSNAME, @sql NVARCHAR(MAX);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT TableName, ColumnName, ConstraintName, Definition FROM @fixes;
OPEN c;
FETCH NEXT FROM c INTO @table, @column, @name, @definition;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF COL_LENGTH(N'dbo.' + @table, @column) IS NOT NULL
       AND NOT EXISTS (
            SELECT 1
              FROM sys.default_constraints dc
              JOIN sys.columns col ON col.object_id = dc.parent_object_id AND col.column_id = dc.parent_column_id
             WHERE dc.parent_object_id = OBJECT_ID(N'dbo.' + @table)
               AND col.name = @column
               AND dc.definition = @definition)
    BEGIN
        SELECT @current = dc.name
          FROM sys.default_constraints dc
          JOIN sys.columns col ON col.object_id = dc.parent_object_id AND col.column_id = dc.parent_column_id
         WHERE dc.parent_object_id = OBJECT_ID(N'dbo.' + @table)
           AND col.name = @column;
        IF @current IS NOT NULL
            EXEC(N'ALTER TABLE dbo.[' + @table + N'] DROP CONSTRAINT [' + @current + N']');
        SET @sql = N'ALTER TABLE dbo.[' + @table + N'] ADD CONSTRAINT [' + @name + N'] DEFAULT ' + @definition + N' FOR [' + @column + N']';
        EXEC(@sql);
    END
    FETCH NEXT FROM c INTO @table, @column, @name, @definition;
END
CLOSE c;
DEALLOCATE c;
GO

PRINT N'OK — derechos y defaults de esquema igualados (o ya lo estaban).';
GO
