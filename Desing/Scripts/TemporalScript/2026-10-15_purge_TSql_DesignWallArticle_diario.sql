/* =============================================================================
 dbo.TSql_DesignWallArticle — limpieza diaria de recuentos viejos
 =============================================================================
 DESVIACIÓN de la regla «nunca DELETE físico»:
 lo pide el recuento de piezas. Al guardar no se reescriben las piezas
 anteriores (eso tardaba minutos). El listado solo lee el último lote de
 cada diseño. Esta limpieza, una vez al día a las 02:00, quita el resto:
 piezas ya marcadas Is_Delete = 1 y lotes que ya no son el recuento vigente.
 No toca el lote más reciente de cada diseño.

 Un trigger no sirve: se dispara al cambiar una fila, no a una hora.
 En SQL Express no hay Agent; la aplicación lanza este mismo DELETE una vez
 al día a partir de las 02:00. Si hay Agent, el trabajo de abajo también.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.usp_TSql_DesignWallArticle_PurgeOld', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.usp_TSql_DesignWallArticle_PurgeOld AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.usp_TSql_DesignWallArticle_PurgeOld
AS
BEGIN
    SET NOCOUNT ON;

    DELETE a
    FROM dbo.TSql_DesignWallArticle a
    WHERE NOT EXISTS (
        SELECT 1
        FROM (
            SELECT LinkDesign_V2, MAX(AddDateMade) AS BatchAt
            FROM dbo.TSql_DesignWallArticle
            WHERE Is_Delete = 0
            GROUP BY LinkDesign_V2
        ) latest
        WHERE latest.LinkDesign_V2 = a.LinkDesign_V2
          AND latest.BatchAt = a.AddDateMade
          AND a.Is_Delete = 0
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesignWallArticle_Design_Batch'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesignWallArticle')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesignWallArticle_Design_Batch
        ON dbo.TSql_DesignWallArticle (LinkDesign_V2, AddDateMade)
        INCLUDE (Is_Delete, Is_Active);
GO

/* Trabajo a las 02:00 solo si SQL Agent está instalado. */
IF OBJECT_ID(N'msdb.dbo.sysjobs', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'TSql_DesignWallArticle_PurgeOld_0200')
BEGIN
    BEGIN TRY
        EXEC msdb.dbo.sp_add_job
            @job_name = N'TSql_DesignWallArticle_PurgeOld_0200',
            @enabled = 1,
            @description = N'Quita una vez al día los recuentos de piezas que ya no están vigentes.';

        DECLARE @db sysname = DB_NAME();
        EXEC msdb.dbo.sp_add_jobstep
            @job_name = N'TSql_DesignWallArticle_PurgeOld_0200',
            @step_name = N'Purge',
            @subsystem = N'TSQL',
            @database_name = @db,
            @command = N'EXEC dbo.usp_TSql_DesignWallArticle_PurgeOld;';

        EXEC msdb.dbo.sp_add_schedule
            @schedule_name = N'TSql_DesignWallArticle_PurgeOld_diario_0200',
            @freq_type = 4,
            @freq_interval = 1,
            @active_start_time = 020000;

        EXEC msdb.dbo.sp_attach_schedule
            @job_name = N'TSql_DesignWallArticle_PurgeOld_0200',
            @schedule_name = N'TSql_DesignWallArticle_PurgeOld_diario_0200';

        EXEC msdb.dbo.sp_add_jobserver
            @job_name = N'TSql_DesignWallArticle_PurgeOld_0200';
    END TRY
    BEGIN CATCH
        PRINT N'AVISO — SQL Agent no ha creado el trabajo. La aplicación hace la misma limpieza una vez al día.';
    END CATCH
END
GO

PRINT N'OK — dbo.usp_TSql_DesignWallArticle_PurgeOld lista (limpia a las 02:00).';
GO
