/* =============================================================================
   Función ClientOffer (menú Configuración › Oferta cliente y textos/logo).
   Se asigna al rol DEVELOP y a los roles del usuario juan.godoy@vscad.com.
   Textos de menú en Common, idioma por defecto.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @User NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND TextCode = N'ClientOffer')
    INSERT INTO dbo.TSql_Functions
        (TextLabel, TextCode, TextModule, TextDescription,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Oferta al cliente', N'ClientOffer', N'Configuracion',
         N'OfertaCliente editable (encabezado, textos) y textos generales con logo.',
         0, 1, @User, NULL, GETDATE(), NULL, 0);

INSERT INTO dbo.TSql_RoleFunction
    (TextLabel, LinkRoles_V2, LinkFunctions,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM dbo.TSql_Roles_V2 r
  JOIN dbo.TSql_Functions f ON f.TextCode = N'ClientOffer' AND f.Is_Delete = 0
 WHERE r.Is_Delete = 0 AND r.Is_Active = 1
   AND (
        r.TextCode = N'DEVELOP'
        OR EXISTS (
            SELECT 1 FROM dbo.TSql_UserRole ur
            JOIN dbo.AspNetUsers u ON u.Id = ur.LinkAspNetUsers
             WHERE ur.Is_Delete = 0 AND ur.LinkRoles_V2 = r.IdObject
               AND u.UserName = N'juan.godoy@vscad.com')
       )
   AND NOT EXISTS (
        SELECT 1 FROM dbo.TSql_RoleFunction x
         WHERE x.Is_Delete = 0 AND x.LinkRoles_V2 = r.IdObject AND x.LinkFunctions = f.IdObject);

DECLARE @Lang BIGINT =
(
    SELECT TOP (1) l.IdObject FROM dbo.TSql_Language l
     WHERE l.Is_Default = 1 AND l.Is_Delete = 0 AND l.Is_Active = 1
     ORDER BY l.IdObject
);

IF @Lang IS NOT NULL AND OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
BEGIN
    DECLARE @Keys TABLE (TextResourceKey NVARCHAR(200), TextModule NVARCHAR(100), TextValue NVARCHAR(500));
    INSERT INTO @Keys VALUES
        (N'Menu.ClientOffer', N'Common', N'Oferta cliente'),
        (N'Menu.ClientOfferTexts', N'Common', N'Textos y logo de oferta');

    INSERT INTO dbo.TSql_UiTranslation
        (TextResourceKey, TextModule, LinkLanguage, TextValue,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    SELECT k.TextResourceKey, k.TextModule, @Lang, k.TextValue,
           0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Keys k
     WHERE NOT EXISTS (
        SELECT 1 FROM dbo.TSql_UiTranslation t
         WHERE t.Is_Delete = 0 AND t.TextResourceKey = k.TextResourceKey
           AND t.TextModule = k.TextModule AND t.LinkLanguage = @Lang);
END

PRINT N'OK — función ClientOffer y textos de menú (o ya existían).';
GO
