# Base de datos: alineación develop / producción y limpieza pendiente

Estado a **14/11/2026** (incluye el lote 1 de limpieza, §5). Qué se igualó entre las dos bases de datos, qué queda por decidir y qué habrá que borrar o refactorizar más adelante.

## 1. Contexto

- **Tandem 2026 es una segunda versión.** Convive con tablas, columnas y vistas de la primera (Desing 1, `DesignTools`), algunas ya sin uso.
- **develop y producción deben ser idénticas** hasta que producción arranque en **enero de 2027**. Cualquier cambio de esquema se aplica a las dos.
- Bases locales (`.\SQLEXPRESS`): `db_a197cd_desingdevelop` y `db_a197cd_desingproducction`. En `DAL/App.config`, la entrada que apunta a `NUCBOXG5\SQLEXPRESS` usa `db_a197cd_desingproducction`, así que no hay una tercera base develop local.
- **Ya hubo una limpieza el 16/05/2026.** `DAL/edmx-prune-unused.mjs` quitó del modelo las entidades heredadas y `Scripts/TemporalScript/2026-05-16_drop_unused_legacy_removed_from_edmx.sql` borró sus tablas y vistas en producción. Develop no se limpió.
- **Hay que decidir y planificar la limpieza.** La alineación del 14/11 (unión de las dos bases) **volvió a crear en producción 13 de esas tablas** (vacías, salvo 3 catálogos) y las columnas antiguas de `TSql_Language`. Las vistas no se recrearon. La lista del §4 sirve para hacer la limpieza de forma ordenada y en las dos bases a la vez.

## 2. Alineación del 14/11/2026

**Regla:** solo se añadió, sin borrar ni renombrar nada, sin cambiar tipos y sin tocar los datos existentes. El resultado es la **unión** de las dos bases.

Después de alinear, las dos bases tenían **81 tablas, 1209 columnas, 119 índices (incluidas las claves primarias) y 169 FK**. Las filas de las tablas que ya existían no cambiaron. No se omitió ninguna FK: no había filas huérfanas. Tras el lote 1 (§5) quedan **79 tablas** en cada base.

### 2.1 Añadido a develop (venía de producción)

- **17 tablas:** AA01Master, TSql_Client_V2, TSql_Design_V2, TSql_DesignWallArticle, TSql_Document, TSql_DocumentTypeExtension, TSql_Extension, TSql_Jobside, TSql_Offers, TSql_OfferState, TSql_Plantilla, TSql_PluginDeviceAuth, TSql_TelegramDesignAccess, TSql_TelegramDesignInbox, TSql_UiTranslation, TSql_XrDevice, TSql_XrPushJob.
- **79 columnas en tablas que ya existían:**
  - `TSql_Branch` (16): AddLetter, AttColor y las 14 `Loc_*` (dirección de Google Places).
  - `TSql_Company` (17): AddLetter, LinkLanguage, LinPlantilla y las 14 `Loc_*`.
  - `TSql_DesignWall` (9): Is_Special, NumberCurveCenterX/Y/Z, NumberCurveOuterRadius, NumberCurveFractionNum/Den, NumberCurveStartAngle, NumberCurveModulation.
  - `TSql_DocumentType` (12): el diseño nuevo completo (IdObject como identidad, TextLabel, TextCode, TextDescription, NumberMaxFileSizeBytes y auditoría). La tabla tenía el diseño antiguo y no tenía clave primaria.
  - `TSql_language` (12): el diseño nuevo completo (IdObject como identidad, TextLabel, TextCode, TextNativeName, Is_Default, LinkCountry y auditoría). También tenía el diseño antiguo y no tenía clave primaria.
  - `Tsql_Master_Articles` (13): IInsertinMaterArticles y las 12 `LinkBlockDwg*`.
- **20 claves primarias** (incluidas `PK_TSql_ConfigDesingDefault`, `PK_TSql_DocumentType` y `PK_TSql_Language`), **24 índices** y **57 FK**.

### 2.2 Añadido a producción (venía de develop)

- **16 tablas:** Maestro_articulos_temporal, Movimientos, perierase, tbDCulture, temporalmaestro, TSql_Branch1, TSql_Business, TSql_Business1, TSql_Client, TSql_Comercial1, TSql_DesingConfigWallDetail, TSql_DesingWallConfig, TSql_LanguageConcept, TSql_LanguageDetails, TSql_TypeDesingWallConfig, TSql_UserBranch. Se creó la estructura sin sus datos, salvo los catálogos del §2.3.
- **22 columnas:**
  - `TSql_Branch.LinBusiness`.
  - `TSql_DesignDetails`: IsRenderWall, LinDesingWallConfig, ModeLManualConfig.
  - `TSql_DocumentType` (8 antiguas): IDTIPO_DOCUMENTO, TIPO_DOCUMENTO, IDGRUPO_DOCUMENTO, ORDEN, TIPO_DOCUMENTO_CORTO, SIGLAS, IDESTADO_BASE, VINCULABLE.
  - `TSql_Language` (9 antiguas): SysObjectID, AttLabel, Linflag, AttSinglature, LinCreatedBy, AttCreated, AttLastModification, SysUpdateNumber, AttIsDeleted.
  - `Tsql_Master_Articles.ImgDetail`.
- **7 claves primarias** y **15 FK**.

### 2.3 Filas copiadas (solo catálogos pequeños, en tablas nuevas, conservando los Id)

| Destino | Tabla | Filas |
|---------|-------|-------|
| develop | TSql_OfferState | 4 |
| develop | TSql_Extension | 2 |
| develop | TSql_Plantilla | 1 («Plantilla por defecto») |
| develop | TSql_UiTranslation | 41 |
| producción | tbDCulture | 7 |
| producción | TSql_LanguageConcept | 1 |
| producción | TSql_LanguageDetails | 1 |

**No se copiaron:**
- `TSql_DocumentTypeExtension`: apunta a Id de `TSql_DocumentType` que en develop son distintos.
- `TSql_TypeDesingWallConfig`: su usuario no existe en producción y la FK lo impide.
- Datos de negocio o de usuario: diseños, ofertas, obras, clientes, `Movimientos`, `TSql_DesingWallConfig`.

### 2.4 Copias de seguridad (tomadas antes de aplicar)

- **Copia completa** (`COPY_ONLY`): `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\<db>_pre_pt109u_20261114.bak`, una por base.
- **Esquema en script** (SMO): `%TEMP%\pt109u\backup\<db>_schema_pre_pt109u.sql`. `%TEMP%` se puede vaciar; si hay que conservarlas, muévelas a una carpeta fija.
- La caja de Grok Bot tiene un duplicado: `/workspace/ia/p25/backup/backup_pt109u.zip` (scripts de esquema, logs y las comparaciones antes y después).

### 2.5 Scripts (en `Scripts/TemporalScript/`, idempotentes)

| Script | Qué hace |
|--------|----------|
| `2026-11-14_align_schema_develop.sql` | Añade a develop lo de producción |
| `2026-11-14_align_schema_production.sql` | Añade a producción lo de develop |
| `2026-11-14_align_data_develop.sql` | Catálogos copiados a develop |
| `2026-11-14_align_data_production.sql` | Catálogos copiados a producción |

- Se ejecutan con `sqlcmd -S <srv> -E -d <db> -I -x -f 65001 -b -i <script>`, primero el de esquema y después el de datos.
- Cada paso va protegido con `IF NOT EXISTS`. Las FK, claves primarias e índices van en `TRY/CATCH`: si fallan, escriben `SKIP …` y el script sigue.
- Se probaron en copias restauradas de las dos bases, incluida una segunda ejecución, que no añadió nada.
- **Si se aplican en los servidores remotos (site4now), conviene volver a comparar antes**: esos servidores pueden no coincidir con las bases locales.

### 2.6 Comprobaciones

- **Comparación de esquemas posterior:** iguales salvo lo del §3.
- **Compilación:** MSBuild de VS2022 (DAL y Desing), 0 errores.
- **Prueba EF:** las 51 tablas del modelo (`DbSet`) responden en las dos bases.
- **Regresión del solver:** 74+13, sin diferencias.
- **`Model.edmx`:** no se tocó. Todo lo que usa la app ya existía en el modelo, y lo antiguo añadido no lo usa el código.

## 3. Decisiones pendientes (no aplicadas)

1. **Identidad:** `TSql_ConfigDesingDefault.SysObjectID` es identidad en producción y no en develop. Las dos están vacías.
2. **Valor por defecto distinto:** `TSql_DesignDetails.IsPanel060` es `((1))` en develop y `((0))` en producción.
3. **Nulabilidad que ya era distinta:** `TSql_Language.LinModifiedBy` es NOT NULL en develop y nullable en producción.
4. **Columnas que quedaron nullable en un lado y NOT NULL en el otro.** En el origen eran NOT NULL sin valor por defecto y la tabla de destino ya tenía filas, así que se crearon nullable:
   - develop: `TSql_DocumentType.TextLabel` y `LinkMadeBy`; `TSql_language.TextLabel`, `TextCode` y `LinkMadeBy`.
   - producción: `TSql_Branch.LinBusiness`; las 8 antiguas de `TSql_DocumentType` (§2.2); las 9 antiguas de `TSql_Language` (§2.2).
   - Si las columnas antiguas se borran (§4), esta lista se reduce a las 5 de develop. Para dejarlas NOT NULL hay que rellenar antes las filas antiguas.
5. ~~**16 vistas que solo estaban en develop**~~ (AminData, Angel_01_Net_user, Angel_02_Employee, Angel_03_DefaulDesing, Desing_details, Empledos, Employee, Funciones de usuari, Master articles, MasterArticle, Personal, Q, Register, View_1, View_2, view_name). **Resuelto el 14/11/2026: borradas en el lote 1 (§5).** Las dos bases tienen ahora las mismas 2 vistas: `Cliente` y `TypeCorner`.
6. **`db_Tamdem`:** existe en el mismo servidor, ninguna configuración la menciona y es una copia antigua (52 tablas, todas contenidas en las otras dos; le faltan 30). No se ha tocado.
7. **Idioma `IdObject=1` en develop:** las 41 traducciones de `TSql_UiTranslation` llevan `LinkLanguage = 1`. En develop, el único idioma (diseño antiguo) recibió `IdObject = 1` al crear la columna, pero sus columnas nuevas (`TextCode`, `TextLabel`…) están vacías.
   - Hay que comprobar que es el idioma correcto, normalmente `es`, y rellenar `TextCode`, `TextLabel`, `LinkMadeBy`, o dar de alta los idiomas como en producción (Id 1 y 2).
8. **Mensaje antiguo:** `Resources/Branch.resx` dice «Cree al menos un TSql_Business…», pero ningún código usa `TSql_Business`. Hay que revisar el texto cuando se borre la tabla.

## 4. Limpieza y refactor futuros

### 4.1 Cómo se comprobaron las referencias (14/11/2026)

- Se buscó con `git grep -w -i` (incluye los archivos aún sin commit) en `*.cs`, `*.cshtml`, `*.js`, `*.resx`, etc. Se excluyeron: los `.md`, los scripts `2026-11-14_align_*` y las clases que genera `Model.tt` dentro de `DAL/`. El modelo (`DAL/Model.edmx`) se miró aparte.
- «0 referencias» quiere decir que solo aparece en scripts SQL de creación o borrado, en `DAL/edmx-prune-unused.mjs` o en reglas `.cursor`.
- Puede haber SQL que construya nombres de tabla al vuelo: **hay que volver a buscar justo antes de borrar**.

### 4.2 Candidatos a borrar (0 referencias en código)

> Actualizado tras el lote 1 (§5): las tachadas ya están borradas en las dos bases.

**Tablas fuera del modelo, 19 (quedan 17):**

| Tabla | Filas dev / prod | Nota |
|-------|------------------|------|
| perierase | 402 / 0 | Temporal o de prueba |
| temporalmaestro | 75 / 0 | Temporal |
| Maestro_articulos_temporal | 75 / 0 | Temporal |
| Movimientos | 77 915 / 0 | Importación antigua. «movimientos» sale en textos de recursos, no como tabla |
| AA01Master | 0 / 0 | Venía de producción, sin uso |
| TSql_Branch1 | 5 / 0 | Copia de TSql_Branch. En el edmx, `TSql_Branch1` es el nombre de una navegación de `AspNetUsers`, no la tabla |
| TSql_Business | 1 / 0 | Diseño antiguo de empresa y sede. Solo sale en el texto de `Branch.resx` |
| TSql_Business1 | 5 / 0 | Copia |
| TSql_Comercial1 | 2 / 0 | Copia |
| ~~TSql_Client~~ | — | **Borrada en el lote 1** (§5) |
| ~~TSql_UserBranch~~ | — | **Borrada en el lote 1** (§5) |
| tbDCulture | 7 / 7 | Sustituida por TSql_Language y TSql_UiTranslation |
| TSql_LanguageConcept | 1 / 1 | Traducciones antiguas |
| TSql_LanguageDetails | 1 / 1 | Traducciones antiguas |
| TSql_CompanyErase | 1 / 1 | Sin uso |
| TSql_Environment | 3 / 3 | Sin uso. No confundir con `TSql_AiEnvironment` |
| TSql_DesingWallConfig | 2 / 0 | Configuración de muro de Desing 1, sustituida por TSql_DesignWall |
| TSql_TypeDesingWallConfig | 2 / 0 | Igual |
| TSql_DesingConfigWallDetail | 0 / 0 | Igual |

**Columnas antiguas, 22:**
- `TSql_DocumentType` (8): IDTIPO_DOCUMENTO, TIPO_DOCUMENTO, IDGRUPO_DOCUMENTO, ORDEN, TIPO_DOCUMENTO_CORTO, SIGLAS, IDESTADO_BASE, VINCULABLE.
- `TSql_Language` (9): SysObjectID, AttLabel, Linflag, AttSinglature, LinCreatedBy, AttCreated, AttLastModification, SysUpdateNumber, AttIsDeleted.
- `TSql_DesignDetails` (3): IsRenderWall, LinDesingWallConfig, ModeLManualConfig.
- `Tsql_Master_Articles.ImgDetail`.
- `TSql_Branch.LinBusiness`. Los `LinBusiness` del código son de `TSql_Employee`, no de esta tabla.
- **Ojo:** en develop, las 16 filas de `TSql_DocumentType` y el idioma solo tienen datos en las columnas antiguas. Antes de borrarlas hay que pasar esos datos al diseño nuevo (o descartarlos).

~~**Vistas de prueba en develop, 16**~~: **borradas en el lote 1** (§5). Ninguna se usaba como vista: los aciertos de `Personal`, `MasterArticle`, `Employee` o `Register` eran palabras o clases con el mismo nombre.

**Base `db_Tamdem`:** 0 referencias en el repositorio.

**Total: 58 candidatos con 0 referencias en código** (19 tablas, 22 columnas, 16 vistas y 1 base de datos). **Tras el lote 1 quedan 40:** 17 tablas, 22 columnas y 1 base de datos.

**En el modelo, pero sin uso en código fuera de `DAL/` (revisar):** `TSql_ConfigDesing`, `TSql_ConfigDesingDefault`, `TSql_Register` (646 filas en develop) y `TSql_App`. Para borrarlas hay que quitarlas antes del `Model.edmx` (con la misma técnica que `edmx-prune-unused.mjs`) y regenerar las plantillas T4.

### 4.3 Tablas que parecen sustituidas (quién las usa hoy)

| Antigua | Nueva | Uso de la antigua | Uso de la nueva |
|---------|-------|-------------------|-----------------|
| TSql_Client | TSql_Client_V2 | Ninguno | 16 archivos: `ClientV2Controller`, `JobsideController`, `HomeController`, `DesignToolsAutocadController`, vistas `ClientV2/*` y `Jobside/*`, `IntranetAuditHelper` |
| TSql_Design (Desing 1) | TSql_Design_V2 | 8 archivos: `DesignToolsController` y vistas `DesignTools/*`, `ModelDesign3d`, y puntos sueltos en `Desing_2Controller`, `EmployeeController`, `TelegramWebhookController` | 11 archivos: `Desing_2Controller`, `DesignWallRepository`, `ClientV2Controller`, `JobsideController`, `HomeController`, `TandemXrApiController`, `DesignToolsAutocadController` y helpers |
| TSql_DesignDetails, TSql_DesignCamera, TSql_TotalListsDesing (hijas de TSql_Design) | TSql_DesignWall, TSql_DesignWallArticle | `DesignToolsController`, `ModelDesign3d`. Además, `Desing_2Controller` usa `TSql_DesignCamera` | Desing_2 (repositorios `DesignWall*`) |
| TSql_Branch1, TSql_Business, TSql_Business1, TSql_Comercial1, TSql_UserBranch | TSql_Branch, TSql_Company | Ninguno | `TSql_Branch`: 20 archivos (`TSql_BranchController`, `JobsideController`, DesignTools…) |
| Columnas antiguas de TSql_DocumentType y TSql_Language | Diseño `IdObject` / `Text*` / `Is_*` | Ninguno | `DocumentTypeController`, `ExtensionController`, `LanguageController`, `BaseController`, `CountryController` |
| tbDCulture, TSql_LanguageConcept, TSql_LanguageDetails | TSql_Language, TSql_UiTranslation | Ninguno | Traducciones de la intranet |
| TSql_DesingWallConfig, TSql_TypeDesingWallConfig, TSql_DesingConfigWallDetail | TSql_DesignWall | Ninguno | Desing_2 |

- `TSql_Design` y sus hijas siguen en uso por el Desing 1 (`DesignTools`) y por la tabla `TSql_DefaultDesign`. Esta última la usan 93 archivos, el motor ATK60 entre ellos, y **no es candidata a borrar**.
- Retirar `TSql_Design` es un refactor: primero hay que llevar a V2 lo que aún la usa (Telegram, Employee, la cámara en Desing_2) o retirar el Desing 1.

### 4.4 Procedimiento seguro de borrado (siempre en las dos bases)

1. **Comprobar referencias** justo antes de borrar: `git grep -w -i <nombre>` en todo el repositorio, además del `Model.edmx`, las vistas o procedimientos que la usen (`sys.sql_expression_dependencies`) y las FK que apunten a ella.
2. **Copia de seguridad** de develop y producción (`.bak` `COPY_ONLY` y script de esquema), con fecha en el nombre.
3. **Script idempotente** `Scripts/TemporalScript/AAAA-MM-DD_drop_<lote>.sql`:
   - `IF OBJECT_ID(...) IS NOT NULL` en cada objeto.
   - Primero las FK que apunten a la tabla, después las tablas hijas y al final las padres.
   - Para columnas: antes los valores por defecto, índices y FK sobre la columna, después `ALTER TABLE … DROP COLUMN`.
   - Se puede partir del script del 16/05.
4. Si el objeto está en el modelo: **quitarlo del `Model.edmx`** (SSDL, CSDL, MSL y diagrama) y **regenerar las plantillas T4** (TextTransform de VS2022). El csproj del DAL se actualiza aparte.
5. **Aplicar en develop** y probar:
   - compilación sin errores;
   - prueba EF de todos los `DbSet`;
   - regresión del solver 74+13;
   - pruebas del visor e intranet afectadas.
6. **Aplicar el mismo script en producción** y volver a comparar los esquemas: deben quedar idénticos.
7. **Documentar el lote** en este archivo (sección nueva con fecha, objetos borrados y dónde está la copia).

**Orden recomendado de lotes:**
1. ~~Vistas de prueba~~ y tablas vacías sin uso: **hecho, lote 1** (§5).
2. Tablas temporales y copias `*1` (tienen filas en develop: decidir antes si se conservan o se exportan).
3. Traducciones antiguas (`tbDCulture`, `TSql_LanguageConcept`, `TSql_LanguageDetails`).
4. Columnas antiguas de `TSql_DocumentType` y `TSql_Language`, tras migrar los datos de develop.
5. `TSql_Business*` (`TSql_Client` y `TSql_UserBranch` ya se borraron en el lote 1).
6. Configuración de muro de Desing 1.
7. Tablas del modelo sin uso.
8. Por último, el refactor de `TSql_Design` → `TSql_Design_V2`.
9. `db_Tamdem`, cuando Juan Andrés lo confirme.

## 5. Registro de lotes de limpieza

### 5.1 Lote 1 — 14/11/2026 (aprobado por Juan Andrés: «Sí, limpia el primer lote en las dos bases»)

**Alcance pedido:** lo del script del 16/05 que la alineación volvió a crear o que seguía en develop, más las 16 vistas de prueba, con 0 referencias en código y en `Model.edmx`. Excluidos: `db_Tamdem`, `TSql_Design`, `TSql_DefaultDesign`, las 4 tablas «a revisar» y cualquier tabla con filas originales en develop.

**Borrado:**

| Objeto | develop | producción |
|--------|---------|------------|
| 16 vistas de prueba (AminData, Angel_01_Net_user, Angel_02_Employee, Angel_03_DefaulDesing, Desing_details, Empledos, Employee, Funciones de usuari, Master articles, MasterArticle, Personal, Q, Register, View_1, View_2, view_name) | Borradas | No existían |
| `TSql_UserBranch` (0 filas, sin FK) | Borrada | Borrada |
| `TSql_Client` (0 filas, sin FK) | Borrada | Borrada |

**No se borró porque develop tiene filas originales (anteriores a hoy).** Para que las dos bases sigan iguales, tampoco se borraron en producción:

| Tabla | Filas develop / producción |
|-------|----------------------------|
| Movimientos | 77 915 / 0 |
| perierase | 402 / 0 |
| temporalmaestro | 75 / 0 |
| Maestro_articulos_temporal | 75 / 0 |
| TSql_Branch1 | 5 / 0 |
| TSql_Business1 | 5 / 0 |
| TSql_Comercial1 | 2 / 0 |
| TSql_Business | 1 / 0 |
| tbDCulture | 7 / 7 (las de producción se copiaron hoy) |
| TSql_LanguageConcept | 1 / 1 (ídem) |
| TSql_LanguageDetails | 1 / 1 (ídem) |
| 9 columnas antiguas de `TSql_Language` | el único idioma de develop tiene sus datos solo en ellas / vacías |

Antes de borrarlas hay que decidir si esos datos se descartan, se exportan o se migran (los del idioma, al diseño nuevo; §3.7).

**Comprobación de referencias (justo antes de borrar):** `git grep` en todo el repositorio (con archivos sin commit) buscando uso como objeto SQL (`dbo.X`, `[X]`, `FROM/JOIN X`) y en `Model.edmx`: 0 referencias. `sys.sql_expression_dependencies`: ningún objeto dependía de ellas. Ninguna vista era `SCHEMABINDING`.

**Script:** `Scripts/TemporalScript/2026-11-14_cleanup_batch1.sql` (idempotente). Borra primero las FK que apunten a la tabla o salgan de ella, y **no borra una tabla si tiene filas** (escribe `SKIP`). Se ejecuta con `sqlcmd -S <srv> -E -d <db> -I -x -f 65001 -b -i <script>` en las dos bases.

**Copias de seguridad (antes de aplicar, `COPY_ONLY` + `CHECKSUM`, verificadas con `RESTORE VERIFYONLY`):**
- `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\db_a197cd_desingdevelop_pt111u_pre_batch1_20261114_1611.bak` (25,5 MB)
- `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\db_a197cd_desingproducction_pt111u_pre_batch1_20261114_1611.bak` (14,5 MB)

**Pruebas:**
- En copias restauradas de las dos bases: aplicado dos veces (la segunda no hace nada) y comparación sin diferencias de tablas ni vistas.
- develop real: aplicado; compilación MSBuild VS2022 (DAL + Desing) 0 errores; prueba EF 51/51 `DbSet`; regresión del solver 74+13 sin diferencias.
- producción real: aplicado; prueba EF 51/51.
- Comparación final: **mismas 79 tablas y mismas 2 vistas** en las dos bases. Solo quedan las diferencias de columna ya conocidas del §3 (identidad, valor por defecto y nulabilidad).
