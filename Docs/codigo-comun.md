# Código común (lo que usan varias áreas)

Revisado el 14/11/2026 con las referencias de los `.csproj`, los `using` y los `import`. Si cambias algo de esta lista, prueba **todas** las áreas de su fila.

**Regla:** el `INDEX.md` de un área enlaza a este documento si su código usa algo de la tabla. Quien cambie una firma, un DTO, un endpoint o un formato JSON de esta lista avisa en la ficha de cada área afectada.

## 1. No hay proyecto compartido

- La solución (`Design.sln`) no tiene proyecto común: ningún `.csproj` enlaza archivos de otro, salvo `Desing → DAL` y los proyectos de pruebas.
- `Common/` solo tiene `Utils/FileHelper.cs` y no lo compila nadie. `Docs/areas/general/Common.md` y `Common/README.md` describen un plan que no se hizo.
- Lo común real está **dentro de `Desing`** (servidor y JS del visor) y en el **contrato HTTP** que usan los plugins.

## 2. Servidor (`Desing`, C#)

| Código | Lo usan | Probar |
|--------|---------|--------|
| `DAL/` (EF6, `Model.edmx`, `ConexionData`) | Todo `Desing` | Compilar; prueba EF de todos los `DbSet`; ver `Docs/areas/base-de-datos/INDEX.md` |
| Motor ATK-60: `Repositories/RepositoryAtk60/` (`Atk60WallsRepository`, `Atk60FaceRemateRules`, `ModulosATK60/*`), `Repositories/Repository_Desing_2/Atk60/*` (15 archivos), `Repositories/RepositoryCommun/` (`FormworkDtos`, `FormworkJsonCommonRepository`) | `Desing_2Controller` (visor, encofrado, recuento de la IA) y `DesignToolsAutocadController` (`PluginEncofrarAtk60` de los plugins) | Regresión del solver 74+13 byte a byte; visor; encofrado manual en AutoCAD |
| `Repositories/RepositoryDesing2/` (`DesignWallRepository`, `DesignWallArticleRepository`, `DesignWorkerRepository`) | `Desing_2Controller`, `DesignToolsAutocadController` (`PluginSaveDesignWalls`, `PluginSaveWallArticle`), `BaseController` | Guardar diseño en el visor y desde un plugin |
| `Repositories/RepositoryDesing2/DesignWallFaceBuilder` (copia en C# de `Scripts/Desing2/desing2-wall-faces.js`: mismos F-n) | Visor (JS, «ID Faces») y `Desing_2Controller.GenerateAssemblyViews` (plano de montaje) | Si cambias uno, cambia el otro: `DesignWallFaceBuilder.Verify.cs` con `print` contra el JS (`Docs/areas/encofrado-atk60/plano-montaje.md`) |
| `Repositories/Atk60/Wall/*` (114 archivos, diseño antiguo) | Solo `DesignToolsController` (Desing 1) | Desing 1 |
| `Services/LCornerDetector`, `SketchWallBuilder`, `ImageAnalysisService`, `SketchPlanSolver`, `OsmBuildingImportService` | `DesignToolsAutocadController` (`ProcesarLineasZwcad`, `DetectarEsquinasImagen`, `ResolverBocetoImagen`): ZWCAD, AutoCAD, BricsCAD, Revit y el boceto desde imagen de Desing_2 | Detección de muros desde un plugin; boceto: `dotnet test Desing.Tests\Desing.Tests.csproj` |
| `Services/CadWall2dService` | `DesignToolsAutocadController` (`PluginWall2dStep`): OpenCAD y plugins | Muro 2D paso a paso |
| `Helpers/PluginCadAtk60PanelStlHelper` | `Desing_2Controller`, `DesignToolsAutocadController`, `DesignWallArticleRepository` | Paneles STL en visor y plugin |
| `Helpers/PluginCadDeviceHelper`, `PluginInstallLink` | Plugins (autorización de equipo), `Desing_2`, `Employee`, `Account`, `Base`, `Plantilla` | Login de plugin; `Docs/areas/cad-comun/` |
| Derechos: `Helpers/FunctionAuthorizeFilter` (filtro global), `FunctionAccessService`, `FunctionHtmlExtensions`, `Scripts/Intranet/tandem-functions.js` | Todos los controladores, menú lateral, layouts Materio y paletas de los plugins | Usuario con y sin la función: menú, URL directa (403) y AJAX; `Docs/areas/seguridad/` |
| `Helpers/IntranetAuditHelper` | Casi todos los controladores | Altas y cambios con auditoría |
| `Helpers/ApplicationStlUrlHelper`, `DesignGroupHelper`, `JobsideCodeHelper`, `OfferNumberHelper` | Intranet (Home, Jobside), `Desing_2`, plugins, TandemXR | Obra/oferta en intranet y plugin |
| `Helpers/LanguageUiHelper`, `DbBackedResourceManager` | Intranet y layout | Traducciones: `Docs/areas/intranet/` |

## 3. Contratos HTTP (cliente ↔ servidor)

| Contrato | Clientes | Nota |
|----------|----------|------|
| `DesignToolsAutocadController`: `PluginSession`, `PluginReady`, `PluginPing`, `PluginCadAuth`, `ValidarEquipoPlugin`, `PaletteMode`, `PaletteTools`, `PluginBlocks`, `PluginBlockLibrary`, `PluginSaveDesignWalls`, `PluginSaveWallArticle`, `PluginEncofrarAtk60`, `PluginWall2dStep`, `ProcesarLineasZwcad`, `DetectarEsquinasImagen`, `WallForm` | ZWCAD, AutoCAD, BricsCAD, Revit, OpenCAD, FreeCAD | Cambiar una respuesta rompe a todos los plugins |
| `Views/DesignToolsAutocad/*` (paletas MVC que abren los plugins en WebView) | Todos los plugins | `WallForm.cshtml` carga `wall-edit-form.js` del visor |
| `TandemXrApiController` | `TandemXR`, `TandemXR-Unity` | Manifiesto del diseño |
| `Desing_2Controller` (`AiCommands`, encofrado, guardar) | Visor Desing_2 | |

## 4. JavaScript del visor (`Desing/Scripts/`)

| Archivo | Lo usan |
|---------|---------|
| `MasterArticles/master-article-details-stl-viewer.js` (≈41 000 líneas) | Visor, dibujo 2D, muros 3D, encofrado, IA; también las vistas de `MasterArticles` |
| `MasterArticles/wall-edit-form.js`, `wall-ai-assistant.js`, `ai-command-catalog.js` | Visor Desing_2 **y** paleta de los plugins (`Views/DesignToolsAutocad/WallForm.cshtml`) |
| `Desing2/desing2-stl-viewer-toolbar-wiring.js` | Barra del visor, petición de encofrado, vista previa de un muro para la IA |
| `MasterArticles/ma-stl-atk60-formwork.js` | Encofrado en el visor (pieza verde L/T) |
| `DesignToolsAutocad/plugin-cad-block-preview.js` (`TandemCadBlockPreview`: `loadStl`, `loadScene`) | Visor del artículo del recuento en Desing_2, recuento de `MasterArticles` (`_MasterArticleCountStlPanelBoot.cshtml`), bloques de los plugins (`PluginBlocks.cshtml`) y visor de vistas del plano de montaje (`loadScene`) |

Un cambio en estos archivos: `node --check`, subir `?v=` en **todos** los sitios que lo importan (incluido `WallForm.cshtml`) y probar el visor y la paleta del plugin.

## 5. Código duplicado entre plugins (no compartido)

- **AutoCAD ↔ ZWCAD**: unos 40 archivos copiados con pocas diferencias (`MVCApiService`, `PaletteHost`, `Models`, `Wall2dCommand`, `Wall3dCommand`, `BlockInsertCommand`, `Atk60LibrarySync`, `FormworkCommand`, `PluginSessionStore`, `UI/Views/*`…). Si arreglas uno, mira el mismo archivo en el otro.
- **BricsCAD ↔ Revit**: comparten forma con AutoCAD en `Commands`, `MVCApiService`, `Models`, `PaletteHost`, `Wall2dCommand`, `Wall3dCommand`, `PluginExceptionHelper`, `CadUnits`, pero con más diferencias.
- Solo en AutoCAD: `WallEditAttrs`, `WallEditCommand`, `WallFormHost`.
- Propuesta pendiente de decidir: un proyecto común `net8.0` + `net48` para estos archivos (`Docs/areas/cad-comun/plugins-cad.md`, «Regla arquitectonica principal»).

## 6. Reglas transversales

- Tablas SQL: `.cursor/rules/sql-tsql-table-conventions.mdc`.
- Textos visibles de la intranet: `.cursor/rules/intranet-mvc-i18n-resx-uitranslation.mdc`.
- Arquitectura: `Docs/areas/general/Arquitectura-Cliente-Servidor.md`.
