/*
  Plantilla por empresa: LinPlantilla en TSql_Company; se elimina LinPlantilla en TSql_Employee.

  Orden: anadir columna empresa -> copiar desde empleados -> quitar FK/columna empleado -> FK empresa.

  RECOMENDACION: detener el sitio (pool IIS), ejecutar este script, publicar DAL/Desing nuevo y arrancar.
  Si la web antigua sigue en marcha al quitar LinPlantilla del empleado, fallara hasta publicar el nuevo binario.

  Ejecutar en la misma base que ConexionData (Web.config). Tras ejecutar, publicar DAL/Desing actualizado.
*/

SET NOCOUNT ON;

IF COL_LENGTH('dbo.TSql_Company', 'LinPlantilla') IS NULL
    ALTER TABLE dbo.TSql_Company ADD LinPlantilla BIGINT NULL;
GO

/* Copiar plantilla desde empleados (si habia varias distintas, se toma MAX por determinismo) */
EXEC(N'
UPDATE c SET c.LinPlantilla = x.Pid
FROM dbo.TSql_Company c
INNER JOIN (
    SELECT LinCompany AS CompanyId, MAX(LinPlantilla) AS Pid
    FROM dbo.TSql_Employee
    WHERE AttIsDeleted = 0 AND LinPlantilla IS NOT NULL
    GROUP BY LinCompany
) x ON x.CompanyId = c.SysObjectID
WHERE c.LinPlantilla IS NULL AND x.Pid IS NOT NULL;
');
GO

/* Quitar FK, indice, todos los DEFAULT sobre LinPlantilla y la columna (un solo lote, sin QUOTENAME) */
IF COL_LENGTH('dbo.TSql_Employee', 'LinPlantilla') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_TSql_Employee_TSql_Plantilla'
          AND parent_object_id = OBJECT_ID(N'dbo.TSql_Employee'))
        ALTER TABLE dbo.TSql_Employee DROP CONSTRAINT FK_TSql_Employee_TSql_Plantilla;

    IF EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.object_id = OBJECT_ID(N'dbo.TSql_Employee')
          AND i.name = N'IX_TSql_Employee_LinPlantilla'
          AND i.index_id > 0)
        DROP INDEX IX_TSql_Employee_LinPlantilla ON dbo.TSql_Employee;

    DECLARE @dropSql nvarchar(4000), @dcName sysname;
    WHILE EXISTS (
        SELECT 1
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c
            ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Employee')
          AND c.name = N'LinPlantilla')
    BEGIN
        SET @dcName = NULL;
        SELECT TOP (1) @dcName = dc.name
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c
            ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Employee')
          AND c.name = N'LinPlantilla'
        ORDER BY dc.name;

        IF @dcName IS NULL BREAK;

        /* Corchetes + escape de ] en nombres raros; sin QUOTENAME */
        SET @dropSql = N'ALTER TABLE dbo.TSql_Employee DROP CONSTRAINT ['
            + REPLACE(@dcName, N']', N']]') + N']';
        EXEC sys.sp_executesql @dropSql;
    END

    ALTER TABLE dbo.TSql_Employee DROP COLUMN LinPlantilla;
END
GO

/* FK empresa -> plantilla (opcional) */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Company_TSql_Plantilla')
   AND COL_LENGTH('dbo.TSql_Company', 'LinPlantilla') IS NOT NULL
BEGIN
    ALTER TABLE dbo.TSql_Company ADD CONSTRAINT FK_TSql_Company_TSql_Plantilla
        FOREIGN KEY (LinPlantilla) REFERENCES dbo.TSql_Plantilla (SysObjectID);
END
GO

SELECT COL_LENGTH('dbo.TSql_Company', 'LinPlantilla') AS Company_Has_LinPlantilla,
       COL_LENGTH('dbo.TSql_Employee', 'LinPlantilla') AS Employee_LinPlantilla_ShouldBe_NULL;
