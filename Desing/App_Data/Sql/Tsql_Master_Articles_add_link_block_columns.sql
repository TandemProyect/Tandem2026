/*
  Tsql_Master_Articles: enlaces nvarchar(500) NULL a .dwg / .stl (u otras rutas).
  Ejecutar una vez contra la base de datos de la aplicación.

  Tras renombrar columnas *3ds -> *Stl en SQL, actualice DAL\Model.edmx:
  en SSDL (storage), las tres propiedades deben llamarse LinkBlockDwgPlantStl, etc.
  y en MSL ScalarProperty ColumnName debe coincidir (LinkBlockDwgPlantStl, ...).
*/
IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgPlant3D') IS NULL
    ALTER TABLE dbo.Tsql_Master_Articles ADD LinkBlockDwgPlant3D nvarchar(500) NULL;

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgVerticalElevation3D') IS NULL
    ALTER TABLE dbo.Tsql_Master_Articles ADD LinkBlockDwgVerticalElevation3D nvarchar(500) NULL;

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgHorizontalElevation3D') IS NULL
    ALTER TABLE dbo.Tsql_Master_Articles ADD LinkBlockDwgHorizontalElevation3D nvarchar(500) NULL;

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgPlantMck-up') IS NULL
    ALTER TABLE dbo.Tsql_Master_Articles ADD [LinkBlockDwgPlantMck-up] nvarchar(500) NULL;

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgVerticalElevationMock-up') IS NULL
    ALTER TABLE dbo.Tsql_Master_Articles ADD [LinkBlockDwgVerticalElevationMock-up] nvarchar(500) NULL;

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgHorizontalElevationMock-up') IS NULL
    ALTER TABLE dbo.Tsql_Master_Articles ADD [LinkBlockDwgHorizontalElevationMock-up] nvarchar(500) NULL;

/* STL por bloque: renombra columnas antiguas *3ds o crea *Stl si faltan. */
IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgPlantStl') IS NULL
BEGIN
    IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgPlant3ds') IS NOT NULL
        EXEC sp_rename N'dbo.Tsql_Master_Articles.LinkBlockDwgPlant3ds', N'LinkBlockDwgPlantStl', N'COLUMN';
    ELSE
        ALTER TABLE dbo.Tsql_Master_Articles ADD LinkBlockDwgPlantStl nvarchar(500) NULL;
END

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgVerticalElevationStl') IS NULL
BEGIN
    IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgVerticalElevation3ds') IS NOT NULL
        EXEC sp_rename N'dbo.Tsql_Master_Articles.LinkBlockDwgVerticalElevation3ds', N'LinkBlockDwgVerticalElevationStl', N'COLUMN';
    ELSE
        ALTER TABLE dbo.Tsql_Master_Articles ADD LinkBlockDwgVerticalElevationStl nvarchar(500) NULL;
END

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgHorizontalElevationStl') IS NULL
BEGIN
    IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'LinkBlockDwgHorizontalElevation3ds') IS NOT NULL
        EXEC sp_rename N'dbo.Tsql_Master_Articles.LinkBlockDwgHorizontalElevation3ds', N'LinkBlockDwgHorizontalElevationStl', N'COLUMN';
    ELSE
        ALTER TABLE dbo.Tsql_Master_Articles ADD LinkBlockDwgHorizontalElevationStl nvarchar(500) NULL;
END

IF COL_LENGTH(N'dbo.Tsql_Master_Articles', N'IInsertinMaterArticles') IS NULL
BEGIN
    ALTER TABLE dbo.Tsql_Master_Articles ADD IInsertinMaterArticles bit NOT NULL
        CONSTRAINT DF_Tsql_Master_Articles_IInsertinMaterArticles DEFAULT (0);
END
