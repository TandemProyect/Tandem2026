/* =============================================================================
   Semilla: menú lateral, barra superior y botones comunes (TextModule = Common).

   Idioma: solo filas para TSql_Language.Is_Default = 1 (resto vía Excel).

   Requisitos: TSql_Language + TSql_UiTranslation + AspNetUsers (como otros seeds).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @Lang BIGINT =
(
    SELECT TOP (1) l.IdObject
      FROM dbo.TSql_Language l
     WHERE l.Is_Default = 1 AND l.Is_Delete = 0 AND l.Is_Active = 1
     ORDER BY l.IdObject
);

DECLARE @User NVARCHAR(128) =
(
    SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id
);

IF @Lang IS NULL
    PRINT N'Aviso: sin idioma por defecto — ejecute create_TSql_Language.';
ELSE IF @User IS NULL
    PRINT N'Aviso: AspNetUsers vacío — semilla Common omitida.';
ELSE IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NULL
    PRINT N'Aviso: falta TSql_UiTranslation.';
ELSE
BEGIN
    DECLARE @Seed TABLE
    (
        TextResourceKey NVARCHAR(256) NOT NULL,
        TextValue       NVARCHAR(MAX) NOT NULL
    );

    INSERT INTO @Seed (TextResourceKey, TextValue)
    VALUES
        /* Menú lateral */
        (N'Menu.Home', N'Inicio'),
        (N'Menu.Design', N'Diseño'),
        (N'Menu.Administration', N'Administración'),
        (N'Menu.Companies', N'Empresas'),
        (N'Menu.Employees', N'Empleados'),
        (N'Menu.Articles', N'Artículos'),
        (N'Menu.Templates', N'Plantillas'),
        (N'Menu.Clients', N'Clientes'),
        (N'Menu.JobSites', N'Obras'),
        (N'Menu.ConfigurationSection', N'Configuración'),
        (N'Menu.Configuration', N'Configuración'),
        (N'Menu.DocumentTypes', N'Tipos de documento'),
        (N'Menu.Extensions', N'Extensiones'),
        (N'Menu.Languages', N'Idiomas'),
        (N'Menu.UiTranslations', N'Traducciones UI'),
        (N'Menu.Support', N'Soporte'),
        (N'Menu.Help', N'Ayuda'),
        /* Navbar */
        (N'Navbar.MenuToggleTitle', N'Mostrar/Ocultar menú'),
        (N'Navbar.EnvironmentBadge', N'Entorno: Develop'),
        (N'Navbar.UserFallback', N'Usuario'),
        (N'Navbar.MySpace', N'Mi espacio'),
        (N'Navbar.SignOut', N'Salir'),
        /* Formularios / acciones (reutilizar en vistas) */
        (N'Common.Save', N'Guardar'),
        (N'Common.Cancel', N'Cancelar'),
        (N'Common.Back', N'Volver'),
        (N'Common.Create', N'Crear'),
        (N'Common.Edit', N'Editar'),
        (N'Common.Delete', N'Eliminar'),
        (N'Common.Details', N'Detalles'),
        (N'Common.Search', N'Buscar');

    INSERT INTO dbo.TSql_UiTranslation
    (
        TextResourceKey,
        TextModule,
        LinkLanguage,
        TextValue,
        Is_Delete,
        Is_Active,
        LinkMadeBy,
        LinModifiedBy,
        AddDateMade,
        AddLastDateChange,
        Ntimeschanged
    )
    SELECT S.TextResourceKey,
           N'Common',
           @Lang,
           S.TextValue,
           0,
           1,
           @User,
           NULL,
           GETDATE(),
           NULL,
           0
      FROM @Seed S
     WHERE NOT EXISTS (
               SELECT 1
                 FROM dbo.TSql_UiTranslation T
                WHERE T.TextResourceKey = S.TextResourceKey
                  AND T.LinkLanguage = @Lang
                  AND T.Is_Delete = 0
           );

    PRINT N'OK — semilla Common (menú, navbar, botones genéricos).';
END
GO
