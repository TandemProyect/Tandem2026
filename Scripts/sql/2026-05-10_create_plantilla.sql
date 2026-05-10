-- =============================================================================
-- 2026-05-10  Plantillas de estilo por Usuario (Empleado)
-- -----------------------------------------------------------------------------
-- Crea la tabla TSql_Plantilla (color + logo), inserta la plantilla por defecto
-- (color #349d7d y logo /Content/images/Login/at.png) y anade la columna
-- LinPlantilla a TSql_Employee enlazando todos los empleados existentes a la
-- plantilla por defecto.
-- =============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- 1) Tabla TSql_Plantilla ------------------------------------------------------
IF OBJECT_ID('dbo.TSql_Plantilla', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_Plantilla
    (
        SysObjectID          BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_Plantilla PRIMARY KEY,
        AttName              NVARCHAR(150) NOT NULL,
        AttColor             NVARCHAR(20)  NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttColor   DEFAULT('#349d7d'),
        AttLogo              NVARCHAR(500) NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttLogo    DEFAULT('/Content/images/Login/at.png'),
        AttIsDefault         BIT           NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttIsDefault DEFAULT(0),
        AttIsDeleted         BIT           NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttIsDeleted DEFAULT(0),
        LinCreatedBy         NVARCHAR(128) NULL,
        AttCreated           DATETIME      NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttCreated DEFAULT(GETDATE()),
        LinModifiedBy        NVARCHAR(128) NULL,
        AttLastModification  DATETIME      NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttLastModification DEFAULT(GETDATE()),
        SysUpdateNumber      BIGINT        NOT NULL
            CONSTRAINT DF_TSql_Plantilla_SysUpdateNumber DEFAULT(0)
    );
END
GO

-- Indice unico filtrado: solo una plantilla marcada como "por defecto" activa.
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_TSql_Plantilla_Default'
      AND object_id = OBJECT_ID('dbo.TSql_Plantilla')
)
BEGIN
    CREATE UNIQUE INDEX UX_TSql_Plantilla_Default
        ON dbo.TSql_Plantilla(AttIsDefault)
        WHERE AttIsDefault = 1 AND AttIsDeleted = 0;
END
GO

-- 2) Semilla: plantilla por defecto -------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Plantilla WHERE AttIsDefault = 1 AND AttIsDeleted = 0)
BEGIN
    INSERT INTO dbo.TSql_Plantilla
        (AttName, AttColor, AttLogo, AttIsDefault, AttIsDeleted,
         LinCreatedBy, AttCreated, LinModifiedBy, AttLastModification, SysUpdateNumber)
    VALUES
        (N'Plantilla por defecto', N'#349d7d', N'/Content/images/Login/at.png',
         1, 0, N'system', GETDATE(), N'system', GETDATE(), 0);
END
GO

DECLARE @DefaultPlantillaId BIGINT =
    (SELECT TOP 1 SysObjectID
       FROM dbo.TSql_Plantilla
      WHERE AttIsDefault = 1 AND AttIsDeleted = 0
      ORDER BY SysObjectID);

-- 3) Columna LinPlantilla en TSql_Employee ------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'LinPlantilla'
      AND Object_ID = OBJECT_ID(N'dbo.TSql_Employee')
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) =
        N'ALTER TABLE dbo.TSql_Employee
            ADD LinPlantilla BIGINT NULL
                CONSTRAINT DF_TSql_Employee_LinPlantilla DEFAULT(' +
                CAST(@DefaultPlantillaId AS NVARCHAR(20)) + N');';
    EXEC sp_executesql @sql;
END
GO

-- Rellenamos empleados existentes con la plantilla por defecto.
DECLARE @DefaultPlantillaId2 BIGINT =
    (SELECT TOP 1 SysObjectID
       FROM dbo.TSql_Plantilla
      WHERE AttIsDefault = 1 AND AttIsDeleted = 0
      ORDER BY SysObjectID);

UPDATE dbo.TSql_Employee
   SET LinPlantilla = @DefaultPlantillaId2
 WHERE LinPlantilla IS NULL;
GO

-- FK TSql_Employee.LinPlantilla -> TSql_Plantilla.SysObjectID
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_TSql_Employee_TSql_Plantilla'
      AND parent_object_id = OBJECT_ID('dbo.TSql_Employee')
)
BEGIN
    ALTER TABLE dbo.TSql_Employee
        ADD CONSTRAINT FK_TSql_Employee_TSql_Plantilla
            FOREIGN KEY (LinPlantilla) REFERENCES dbo.TSql_Plantilla(SysObjectID);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_TSql_Employee_LinPlantilla'
      AND object_id = OBJECT_ID('dbo.TSql_Employee')
)
BEGIN
    CREATE INDEX IX_TSql_Employee_LinPlantilla
        ON dbo.TSql_Employee(LinPlantilla)
        WHERE AttIsDeleted = 0;
END
GO

PRINT 'OK - TSql_Plantilla creada y TSql_Employee.LinPlantilla enlazada.';
GO
