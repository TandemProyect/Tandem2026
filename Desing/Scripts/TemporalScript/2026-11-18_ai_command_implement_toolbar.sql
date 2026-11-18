/* =============================================================================
   La IA del diseño ejecuta los comandos que ya tenían botón o tecla en el visor.
   Is_AiImplemented = 1 salvo WALL_FILLET (no hay herramienta de empalme).
   WALL_TRIM pasa a la herramienta Recortar (el clic en el tramo sigue siendo del usuario).
   Idempotente. Solo develop local salvo que se pida producción.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

UPDATE dbo.TSql_AiCommand
   SET TextHandler = N'toolbar:trim|maStlDesing2ActivateToolbarCommandById(''trim'')',
       Is_AiImplemented = 1,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE Is_Delete = 0
   AND TextCode = N'WALL_TRIM'
   AND (Is_AiImplemented = 0 OR TextHandler NOT LIKE N'toolbar:trim%');

UPDATE dbo.TSql_AiCommand
   SET Is_AiImplemented = 1,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE Is_Delete = 0
   AND Is_AiImplemented = 0
   AND TextCode <> N'WALL_FILLET'
   AND (
        TextHandler LIKE N'toolbar:%'
        OR TextHandler LIKE N'click:%'
        OR TextHandler LIKE N'key:%'
   );
