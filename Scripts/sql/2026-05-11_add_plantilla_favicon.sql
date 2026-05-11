-- =============================================================================
-- Migracion: anade columna AttFavicon a TSql_Plantilla
-- Fecha:     2026-05-11
-- Contexto:  Permitir que cada plantilla defina su propio favicon (.ico / .png).
--            Se aplica en los <link rel="icon"> de los layouts.
-- Idempotente: se puede ejecutar varias veces sin efectos secundarios.
-- =============================================================================

IF COL_LENGTH('dbo.TSql_Plantilla', 'AttFavicon') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Plantilla
        ADD AttFavicon NVARCHAR(500) NULL;
    PRINT 'Columna AttFavicon anadida a TSql_Plantilla.';
END
ELSE
BEGIN
    PRINT 'La columna AttFavicon ya existe. Nada que hacer.';
END
GO

-- Valor por defecto para las plantillas existentes (opcional, se deja apuntando
-- al favicon corporativo actual). Solo se rellena donde esta NULL.
UPDATE dbo.TSql_Plantilla
   SET AttFavicon = N'/assets/client/images/Default/Ico/at.ico'
 WHERE AttFavicon IS NULL;
GO
