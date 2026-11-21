/* =============================================================================
   Función «IA.TraduccionSimultanea» (botón Traducción simultánea del chat IA).
   Requiere: 2026-11-16_create_TSql_Functions, semilla IA y roles.
   Idempotente (NOT EXISTS por TextCode / RoleFunction).

   ACTIVA: solo la usan los roles que la tengan (y «Administrador de sistema»).
     - Botón de traducción simultánea junto al altavoz del chat IA (muro y diseño).
   Misma asignación que «IA»: Administrativo, Jefe de Sección, Proyectista, Asistente IA.
   Para dejar el botón libre para todos basta con desactivarla en Configuración › Funciones.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @User NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND TextCode = N'IA.TraduccionSimultanea')
    INSERT INTO dbo.TSql_Functions
        (TextLabel, TextCode, TextModule, TextDescription,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Traducción simultánea IA', N'IA.TraduccionSimultanea', N'Desing_2 (comandos)',
         N'Botón Traducción simultánea del chat IA: orden azul y respuesta al inglés.',
         0, 1, @User, NULL, GETDATE(), NULL, 0);

INSERT INTO dbo.TSql_RoleFunction
    (TextLabel, LinkRoles_V2, LinkFunctions,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM dbo.TSql_Roles_V2 r
  JOIN dbo.TSql_Functions f ON f.TextCode = N'IA.TraduccionSimultanea' AND f.Is_Delete = 0
 WHERE r.Is_Delete = 0
   AND r.TextCode IN (N'AI_ASSISTANT', N'ADMINISTRATIVE', N'SECTION_HEAD', N'DESIGNER')
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_RoleFunction x
                    WHERE x.Is_Delete = 0 AND x.LinkRoles_V2 = r.IdObject AND x.LinkFunctions = f.IdObject);
GO

PRINT N'OK — función IA.TraduccionSimultanea sembrada (o ya existía).';
GO
