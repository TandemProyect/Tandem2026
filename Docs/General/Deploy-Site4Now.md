# Despliegue Site4Now / SmarterASP (proyecto **Design** MVC)

Este documento fija el flujo **repetible** para publicar la intranet MVC (`Desing\Design.csproj`, .NET Framework 4.8) en hospedaje compartido (**Site4Now / SmarterASP**). El hosting suele usar **FTP/FTPS**; **no hay `git pull` en el servidor** salvo VPS con SSH propio — el origen canónico del código sigue siendo este repositorio; lo que llega al sitio son los ficheros compilados/publicados.

**Índice:** [Opciones](#opciones-de-despliegue) · [Secrets en GitHub](#configuracion-una-vez-github--secrets-) · [SmarterASP / Site4Now](#configuracion-una-vez-panel-smarterasp--site4now) · [`Web.GoogleMaps.config`](#googles-mapas-en-producción) · [Verificación](#verificación-tras-el-despliegue) · [Fallas frecuentes](#fallos-frecuentes-y-remedios)

---

## Opciones de despliegue

| Método | Cuándo usarlo |
|--------|----------------|
| **A — GitHub Actions** (`push` a `master` o ejecución manual) | Preferido: compilación limpia en `windows-latest` + sincronización FTP. Ver [workflow](../../.github/workflows/deploy-site4now.yml). |
| **B — PowerShell local** (`Scripts\deploy-site4now.ps1`) | Publicar desde tu máquina y subir artefactos con FileZilla / WinSCP / panel. |
| **C — Visual Studio** | Publicar perfil sistema de ficheros `Site4Now-FileSystem.pubxml` → salida típica `artifacts\design_site4now\` (véase ese fichero). |

Credenciales **nunca** en Git; ver [`.gitignore`](../../.gitignore) (`*.pubxml.user`, `Web.GoogleMaps.config`, `bin/`/`obj/`).

---

## Configuración (una vez): GitHub — Secrets

Documentación oficial: [Secrets en Actions (GitHub)](https://docs.github.com/es/actions/how-tos/writing-workflows/choosing-what-your-workflow-does/using-secrets-in-github-actions) · [ mismo tema en inglés](https://docs.github.com/en/actions/how-tos/writing-workflows/choosing-what-your-workflow-does/using-secrets-in-github-actions).

En el repositorio: **Settings → Secrets and variables → Actions → New repository secret**.

Secrets previstos por el workflow `deploy-site4now.yml`:

| Secret | Obligatorio | Descripción |
|--------|-------------|-------------|
| `SITE4NOW_FTP_SERVER` | Sí | Host FTP (ej. `winxxxx.site4now.net` o el que indique el panel). |
| `SITE4NOW_FTP_USERNAME` | Sí | Usuario FTP del sitio/subcarpeta. |
| `SITE4NOW_FTP_PASSWORD` | Sí | Contraseña FTP. |

**Variables de Actions** (sin mascarar; ideales para rutas y protocolo no sensibles — **Settings → Secrets and variables → Actions → Variables**):

| Variable | Obligatorio | Descripción |
|----------|-------------|-------------|
| `SITE4NOW_FTP_SERVER_DIR` | No | Carpeta **remota** con `/` final. Si se deja vacío, el workflow usa `./tandemdesing/`. |
| `SITE4NOW_FTP_PROTOCOL` | No | `ftp` implícito si vacío. Usa `ftps` o `ftps-legacy` si el hosting exige TLS. **Tiene prioridad** sobre `ftp_protocol` al ejecutar el workflow manualmente. |

Ejecución manual (**Run workflow**): puedes indicar `ftp_protocol`; si `SITE4NOW_FTP_PROTOCOL` no está vacío, esa variable manda (ver `deploy-site4now.yml`).

---

## Configuración (una vez): panel SmarterASP / Site4Now

1. **FTP**: anota servidor, usuario, contraseña y **ruta raíz FTP** donde está el contenido público (`wwwroot`, carpeta virtual o subcarpeta `tandemdesing`). Debe coincidir con lo que configures en `SITE4NOW_FTP_SERVER_DIR`.
2. **Dominio / subdominio**: comprobar el binding al directorio físico correcto tras el FTP.
3. **.NET CLR**: grupo de aplicaciones en **Integrated** para .NET Framework 4.x según soporte del plan (ver FAQ del hosting).
4. **`Web.GoogleMaps.config`**: crear en el servidor (ver siguiente sección).

---

## Googles / mapas en producción

Google Cloud Console debe incluir restricciones de **referentes HTTP(S)** para tu dominio público (`https://tudominio.com/*`, etc.).

La clave **no** debe estar en Git; el artefacto de publicación no lleva `Web.GoogleMaps.config` (solo el `.example` en el repositorio).

**En el servidor** (junto a `Web.config`): sube el fichero con la clave o configura `GOOGLE_MAPS_API_KEY` si el hosting permite variables de entorno.

Más ayuda local: `Desing\Scripts\TemporalScript\GOOGLE_MAPS_LOCALHOST_REFERRERS.txt`.

---

## Flujo día a día

1. **Desarrollo local**: compilar desde Visual Studio o `Scripts\deploy-site4now.ps1` solo para validar carpeta publicada antes de FTP.
2. **Commit**: sin `bin/`, sin `packages/` restauradas en Git, sin claves (`Web.GoogleMaps.config`, `.pubxml.user`).
3. **Despliegue**:
   - **Automático**: `git push origin master` (solo si los paths del workflow coinciden con tus últimos cambios en `Desing/` o `DAL/`).
   - **Manual desde GitHub**: pestaña Actions → workflow **Deploy Site4Now** → Run workflow.

---

## Verificación tras el despliegue

1. Abrir URL pública (login/home).
2. Probar página que cargue Scripts y mapas si aplica.
3. Panel del hosting → **FTP file manager**: confirmar fecha/tamaños de DLL y `Views` actualizados.
4. Erroros **500**: revisar `Web.config`, `Logs`/`ELMAH`/`App_Data`/visor de errores ASP.NET si está habilitado (según plan).

---

## Fallos frecuentes y remedios

| Síntoma | Causa probable | Acción |
|---------|----------------|--------|
| `550`/`530` FTP | Credenciales o ruta FTP incorrectos | Comparar las del panel vs secrets; revisar `./` inicial de `SITE4NOW_FTP_SERVER_DIR`. |
| Mapas sin cargar | Sin `Web.GoogleMaps.config` ni env en servidor; referentes GCP mal | Crear archivo en servidor; añadir dominio producción en consola GCP. |
| Sitio muy antiguo | Deploy no ejecutado o FTP a carpeta equivocada | Comprobar en panel qué directorio sirve tu dominio. |
| Build en Actions falla (Web.targets) | Imagen cambió rutas VS | Workflow intenta resolver `Microsoft.WebApplication.targets` con workload Web; revisar logs del paso Publish. |

---

## Referencias en el repo

- Workflow: [.github/workflows/deploy-site4now.yml](../../.github/workflows/deploy-site4now.yml)
- Script local: [`Scripts/deploy-site4now.ps1`](../../Scripts/deploy-site4now.ps1)
- Perfil público FS (sin credenciales): `Desing/Properties/PublishProfiles/Site4Now-FileSystem.pubxml`
- Flujo Git: [Git-Workflow.md](./Git-Workflow.md)

**Última actualización:** 2026-05-21
