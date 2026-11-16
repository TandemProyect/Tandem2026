/* =============================================================================
   Función «IA» (asistentes IA del visor Desing_2).
   Requiere: 2026-11-16_create_TSql_Functions y la semilla 2026-11-16_seed_TSql_Roles_V2_Functions.
   Idempotente (NOT EXISTS por TextCode).

   ACTIVA: solo la usan los roles que la tengan (y «Administrador de sistema», que tiene todas).
     - Botón IA de la barra de modos, pestaña «Asistente IA» del muro y del muro curvo.
     - Endpoints Desing_2/AiCommands, SaveAiUserSettings, SaveAiUnknownPhrase,
       AiUnknownPhrases y ResolveAiUnknownPhrase (403 sin la función).
   Para dejar la IA libre para todos basta con desactivarla en Configuración › Funciones.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @User NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND TextCode = N'IA')
    INSERT INTO dbo.TSql_Functions
        (TextLabel, TextCode, TextModule, TextDescription,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Asistente IA', N'IA', N'Desing_2 (comandos)',
         N'Botón IA del visor, pestañas «Asistente IA» de los muros y endpoints Desing_2/Ai*.',
         0, 1, @User, NULL, GETDATE(), NULL, 0);
GO

PRINT N'OK — función IA sembrada (o ya existía).';
GO
