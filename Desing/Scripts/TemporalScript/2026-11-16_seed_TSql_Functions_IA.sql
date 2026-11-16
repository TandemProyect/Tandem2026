/* =============================================================================
   Función «IA» (asistentes IA del visor Desing_2).
   Requiere: 2026-11-16_create_TSql_Functions y la semilla 2026-11-16_seed_TSql_Roles_V2_Functions.
   Idempotente (NOT EXISTS por TextCode).

   ACTIVA: solo la usan los roles que la tengan (y «Administrador de sistema», que tiene todas).
     - Botón IA de la barra de modos, pestaña «Asistente IA» del muro y del muro curvo.
     - Endpoints Desing_2/AiCommands, SaveAiUserSettings, SaveAiUnknownPhrase,
       AiUnknownPhrases y ResolveAiUnknownPhrase (403 sin la función).
   Para dejar la IA libre para todos basta con desactivarla en Configuración › Funciones.

   También crea el rol «Asistente IA» (AI_ASSISTANT) con esa función y se lo asigna a
   juan.godoy@vscad.com.
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

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Roles_V2 WHERE Is_Delete = 0 AND TextCode = N'AI_ASSISTANT')
    INSERT INTO dbo.TSql_Roles_V2
        (TextLabel, TextCode, TextDescription, Is_System, Is_AllFunctions,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Asistente IA', N'AI_ASSISTANT', N'Puede usar los asistentes IA del visor.', 0, 0,
         0, 1, @User, NULL, GETDATE(), NULL, 0);

INSERT INTO dbo.TSql_RoleFunction
    (TextLabel, LinkRoles_V2, LinkFunctions,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM dbo.TSql_Roles_V2 r
  JOIN dbo.TSql_Functions f ON f.TextCode = N'IA' AND f.Is_Delete = 0
 WHERE r.TextCode = N'AI_ASSISTANT' AND r.Is_Delete = 0
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_RoleFunction x
                    WHERE x.Is_Delete = 0 AND x.LinkRoles_V2 = r.IdObject AND x.LinkFunctions = f.IdObject);

INSERT INTO dbo.TSql_UserRole
    (TextLabel, LinkAspNetUsers, LinkRoles_V2,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(u.UserName + N' · ' + r.TextLabel, 500), u.Id, r.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM dbo.AspNetUsers u
  JOIN dbo.TSql_Roles_V2 r ON r.TextCode = N'AI_ASSISTANT' AND r.Is_Delete = 0
 WHERE u.UserName = N'juan.godoy@vscad.com'
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_UserRole x
                    WHERE x.Is_Delete = 0 AND x.LinkAspNetUsers = u.Id AND x.LinkRoles_V2 = r.IdObject);
GO

PRINT N'OK — función IA y rol «Asistente IA» sembrados (o ya existían).';
GO
