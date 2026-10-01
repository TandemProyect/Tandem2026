/* =============================================================================
 dbo.Tsql_Master_Articles — tres DWG de tipo de bloque (3D / 3DRef / Xr)
 Tabla LEGADA (AddIsActive, AddChangeBy, sin las 9 columnas TSql_* nuevas).
 No se eliminan las 9 columnas Plant/Elevación/Mock-up/3ds: quedan obsoletas
 en UI; se conservan para no romper filas existentes ni el EDMX antiguo.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwg3D') IS NULL
BEGIN
    ALTER TABLE [dbo].[Tsql_Master_Articles]
        ADD [LinkBlockDwg3D] NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwg3DRef') IS NULL
BEGIN
    ALTER TABLE [dbo].[Tsql_Master_Articles]
        ADD [LinkBlockDwg3DRef] NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgXr') IS NULL
BEGIN
    ALTER TABLE [dbo].[Tsql_Master_Articles]
        ADD [LinkBlockDwgXr] NVARCHAR(500) NULL;
END
GO

DECLARE @sys BIGINT;
SELECT TOP (1) @sys = s.IdObject
FROM [dbo].[TSql_System] AS s
WHERE s.AddIsActive = 1
  AND (
        s.TextLabel LIKE N'%ATK-60%'
     OR s.TextLabel LIKE N'%ATK 60%'
     OR s.TextLabel LIKE N'%ATK60%'
     OR s.TextLabel LIKE N'%AtkSystem60%'
     OR s.TextLabel LIKE N'%ATK System 60%'
     OR s.TextDescription LIKE N'%ATK-60%'
     OR s.TextDescription LIKE N'%ATK 60%'
  )
ORDER BY
    CASE
        WHEN s.TextLabel LIKE N'%ATK-60%' THEN 0
        WHEN s.TextLabel LIKE N'%ATK 60%' THEN 1
        ELSE 2
    END,
    s.IdObject;

IF @sys IS NULL
BEGIN
    PRINT N'AVISO — no hay TSql_System ATK-60 activo. Se omite el seed de paneles.';
END
ELSE
BEGIN
    DECLARE @user NVARCHAR(128);
    SELECT TOP (1) @user = u.Id
    FROM [dbo].[AspNetUsers] AS u
    ORDER BY u.Id;

    IF @user IS NULL
        SET @user = N'seed-atk60';

    DECLARE @now DATETIME = GETDATE();

    DECLARE @p TABLE
    (
        TextCode        NVARCHAR(128) NOT NULL PRIMARY KEY,
        AddAtenkoCode   NVARCHAR(50)  NOT NULL,
        TextLabel       NVARCHAR(500) NOT NULL,
        NumberHigh      FLOAT         NOT NULL,
        NumberWidth     FLOAT         NOT NULL,
        NumberLong      FLOAT         NOT NULL
    );

    INSERT INTO @p (TextCode, AddAtenkoCode, TextLabel, NumberHigh, NumberWidth, NumberLong)
    VALUES
        (N'27904209', N'3120270090', N'Panel ATK-60 2,70×0,90', 2.70, 0.90, 0.06),
        (N'27604207', N'3120270060', N'Panel ATK-60 2,70×0,60', 2.70, 0.60, 0.06),
        (N'27454206', N'3120270045', N'Panel ATK-60 2,70×0,45', 2.70, 0.45, 0.06),
        (N'27304205', N'3120270030', N'Panel ATK-60 2,70×0,30', 2.70, 0.30, 0.06),
        (N'24904240', N'3120240090', N'Panel ATK-60 2,40×0,90', 2.40, 0.90, 0.06),
        (N'24604242', N'3120240060', N'Panel ATK-60 2,40×0,60', 2.40, 0.60, 0.06),
        (N'24454243', N'3120240045', N'Panel ATK-60 2,40×0,45', 2.40, 0.45, 0.06),
        (N'24304244', N'3120240030', N'Panel ATK-60 2,40×0,30', 2.40, 0.30, 0.06),
        (N'12904215', N'3120120090', N'Panel ATK-60 1,20×0,90', 1.20, 0.90, 0.06),
        (N'12604213', N'3120120060', N'Panel ATK-60 1,20×0,60', 1.20, 0.60, 0.06),
        (N'12454212', N'3120120045', N'Panel ATK-60 1,20×0,45', 1.20, 0.45, 0.06),
        (N'12304211', N'3120120030', N'Panel ATK-60 1,20×0,30', 1.20, 0.30, 0.06),
        (N'27104219', N'3120270075', N'Panel universal ATK-60 2,70×0,75', 2.70, 0.75, 0.06),
        (N'24104224', N'3120240075', N'Panel universal ATK-60 2,40×0,75', 2.40, 0.75, 0.06),
        (N'12104120', N'3120120075', N'Panel universal ATK-60 1,20×0,75', 1.20, 0.75, 0.06);

    MERGE [dbo].[Tsql_Master_Articles] AS t
    USING @p AS s
        ON t.TextCode = s.TextCode
    WHEN MATCHED THEN
        UPDATE SET
            t.AddAtenkoCode = COALESCE(NULLIF(LTRIM(RTRIM(t.AddAtenkoCode)), N''), s.AddAtenkoCode),
            t.TextBlockNumber = COALESCE(NULLIF(LTRIM(RTRIM(t.TextBlockNumber)), N''), s.TextCode),
            t.LinkSystem = @sys,
            t.AddIsActive = 1,
            t.IInsertinMaterArticles = 1,
            t.LinkBlockDwg3D = N'~/Content/DesignTools/DWG/AtkSystem60/3D/' + s.TextCode + N'.dwg',
            t.LinkBlockDwg3DRef = N'~/Content/DesignTools/DWG/AtkSystem60/3DRef/' + s.TextCode + N'R.dwg',
            t.LinkBlockDwgXr = N'~/Content/DesignTools/DWG/AtkSystem60/Xr/' + s.TextCode + N'X.dwg',
            t.AddChangeBy = @user,
            t.AddLastDateChange = @now,
            t.Ntimeschanged = t.Ntimeschanged + 1
    WHEN NOT MATCHED THEN
        INSERT
        (
            AddAtenkoCode, TextCode, TextLabel,
            NumberHigh, NumberWidth, NumberLong, NumberWeight,
            TextBlockNumber, TextStlNumber, TextColor1, TextColor2,
            NumberMts2, NumberMts3,
            LinkMadeBy, AddDateMade, AddChangeBy, AddLastDateChange, Ntimeschanged,
            AddIsActive, LinkSystem, IInsertinMaterArticles,
            LinkBlockDwg3D, LinkBlockDwg3DRef, LinkBlockDwgXr
        )
        VALUES
        (
            s.AddAtenkoCode, s.TextCode, s.TextLabel,
            s.NumberHigh, s.NumberWidth, s.NumberLong, NULL,
            s.TextCode, NULL, NULL, NULL,
            s.NumberHigh * s.NumberWidth, NULL,
            @user, @now, @user, @now, 0,
            1, @sys, 1,
            N'~/Content/DesignTools/DWG/AtkSystem60/3D/' + s.TextCode + N'.dwg',
            N'~/Content/DesignTools/DWG/AtkSystem60/3DRef/' + s.TextCode + N'R.dwg',
            N'~/Content/DesignTools/DWG/AtkSystem60/Xr/' + s.TextCode + N'X.dwg'
        );

    /* ImgIco puede no existir en local: el UPDATE va por SQL dinámico
       para que el lote compile aunque la columna aún no esté. */
    IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'ImgIco') IS NOT NULL
    BEGIN
        DECLARE @imgSql NVARCHAR(MAX) = N'
            UPDATE a
            SET a.ImgIco = N''~/Files/MaterialIco/panel.png''
            FROM [dbo].[Tsql_Master_Articles] AS a
            WHERE (a.ImgIco IS NULL OR LTRIM(RTRIM(a.ImgIco)) = N'''')
              AND a.TextCode IN (
                    N''27904209'', N''27604207'', N''27454206'', N''27304205'',
                    N''24904240'', N''24604242'', N''24454243'', N''24304244'',
                    N''12904215'', N''12604213'', N''12454212'', N''12304211'',
                    N''27104219'', N''24104224'', N''12104120''
              );';
        EXEC sys.sp_executesql @imgSql;
    END

    PRINT N'OK — seed ATK-60 (15 paneles) sobre LinkSystem=' + CONVERT(NVARCHAR(20), @sys) + N'.';
END
GO

PRINT N'OK — dbo.Tsql_Master_Articles.LinkBlockDwg3D / 3DRef / Xr creadas (o ya existían).';
GO
