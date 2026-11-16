# Prep code review — arquitectura Tandem 2026

Para el equipo. Corte **2026-10-03**. Qué hay que defender y qué queda como deuda.

## Cómo está montado (y por qué)

| Capa | Qué es | Convención |
|------|--------|------------|
| Intranet | ASP.NET MVC 5, Identity, Materio | Un `ConexionData` por request (`BaseController.Dispose`). CRUD `TSql_*` con auditoría. |
| Datos | EF6 EDMX (`DAL`) | `SetInitializer(null)`. No EF Core ahora. |
| Diseño 3D | `Desing_2` + visor STL | Misma lógica de muros que CAD: `Atk60WallsRepository.SolveFromIdsJson`. |
| AutoCAD | Paletas WebView2 + AppData | `NETLOAD` conecta. `ATDESING` instala bloques. Autoload apagado. |
| Brics / Revit | Hosts delgados | No copiar bloquing hasta un `TandemCad.Core`. |

Rama de trabajo: **`develo`** (no existe `develop`). `master` se fast-forward cuando se pida.

## Conexión (va a ocurrir a menudo)

1. Handshake CAD (`PluginSession` / `PluginCadAuth` / `PluginReady`) **no** carga plantilla ni idioma desde SQL.
2. `PluginPing` = ADO `SELECT 1` (`SqlConnectionPing`). Sin EDMX. Sin cultura.
3. Pie CAD: 20 s si Buena/Aceptable, 8 s si no; parado si la paleta está oculta.
4. `HttpClient` único por host. TLS estricto en `tdesing.net`; certificado laxo solo en localhost.
5. Logo y permiso CAD se recuerdan (cookie / 45 s). Reabrir home no vuelve a Personal.

## Lo que un review no debe revertir

- Instalar la biblioteca en NETLOAD / `Show` / `MarkAuthenticated`.
- Medir “servidor” con el listado de obras o con `navigator.connection.downlink`.
- Devolver Data Source / catálogo / contraseñas en JSON de diagnóstico.
- Meter DataTables/pdfmake otra vez en el Viewer (`SkipIntranetDataTables`).
- Añadir acciones nuevas a `DesignToolsController` (~2500 líneas). Sitio: repositorio + controlador fino.

## Deuda que el review puede señalar (conocida)

| Tema | Estado | No hacer ahora |
|------|--------|----------------|
| Secretos SQL/Telegram en `Web.config` | Sigue. Rotar fuera de este cambio. | No commitear un `Web.config` vacío: rompe IIS del equipo. |
| `TSql_Employee.AttPassAspNetUsert` | Columna legado. **Ya no** va en el JSON del listado ni en Session al editar. El alta Register usa `TempData` una vez. El mail de “enviar contraseña” ya no incluye el texto. | Migrar Identity reset en otra US. |
| God controllers | No partidos (diff enorme). | Strangle: US nuevas fuera. |
| Tres plugins | HttpClient/TLS alineados. Código de paletas aún copiado. | `TandemCad.Core` cuando Brics copie bloquing. |
| Contraseñas en BD en claro | Sigue el campo. | US de seguridad aparte. |

## Cómo probar antes del review

1. Company / Jobside: DataTables sigue.
2. Desing_2 Viewer: no pide pdfmake/jszip.
3. AutoCAD: segundo `NETLOAD` no copia bloques. `ATDESING` sí.
4. Home CAD dos veces: la segunda no consulta logo/Personal si el cookie/caché está.
5. `TANDEM_PRODUCCION`: certificado inválido **no** se acepta. Local IIS Express sí.
