# Sincronizar repositorio completo con Azure DevOps

## Problema

En **Azure DevOps > Repos > Files** solo aparece el proyecto antiguo del plugin:

```
ZwcadPlugin/
ZwcadPlugin.slnx
CorregirReferencias.ps1
MENU_INSTRUCCIONES.md
...
```

Ese repo se creó en marzo 2026 con **solo el plugin ZWCAD**. El monorepo completo vive en **GitHub**:

| Ubicación | Contenido |
|-----------|-----------|
| GitHub `tandemproyect/tandem2026` | Monorepo completo (`Design.sln`, `Desing`, `DAL`, `Common`, `Scripts`, `TamdenZwcadPluging`, ...) |
| Azure DevOps Repos `VSCAD/tandem2026` | Solo `ZwcadPlugin` (repo parcial, historial distinto) |

**Azure Boards** ya está integrado con GitHub (commits `AB#<id>`). Eso es independiente del código que ves en **Repos > Files** de Azure.

## Solución recomendada

Publicar la rama `master` de GitHub en el repo de Azure DevOps con el script:

```powershell
# 1. Regenerar PAT en Azure DevOps (scope: Code Read & write)
#    https://dev.azure.com/VSCAD/_usersSettings/tokens

$env:AZDO_PAT = "<tu-pat-nuevo>"

# 2. Desde la raíz del repo (C:\00_Tandem2026 o clon de GitHub)
cd C:\00_Tandem2026

# 3. Simular primero
.\Scripts\Sync-Repo-To-Azure.ps1 -Force -DryRun

# 4. Publicar (reemplaza el contenido solo-plugin)
.\Scripts\Sync-Repo-To-Azure.ps1 -Force
```

Tras el push, en **Repos > Files** deberías ver:

- `Design.sln`
- `Desing/`
- `DAL/`
- `Common/`
- `TamdenZwcadPluging/`
- `Scripts/`
- `Docs/`
- etc.

## Por qué hace falta `-Force`

El historial de Git en Azure (solo plugin) y el de GitHub (monorepo) **no comparten ancestro común**. Un `git push` normal falla; `-Force` sustituye el contenido del repo de Azure por el monorepo actual.

> El historial antiguo del repo solo-plugin en Azure quedará sobrescrito. El código sigue en GitHub como fuente principal.

## Alternativa: no usar Azure Repos

Si solo necesitáis **Boards + Pipelines** vinculados a GitHub:

1. Dejad el código en GitHub (`tandemproyect/tandem2026`).
2. En Azure DevOps, conectad el pipeline o el servicio a ese repo de GitHub.
3. Ignorad **Repos > Files** de Azure (o archivad el repo parcial).

Documentación de integración Boards: [`TamdenZwcadPluging/ZwcadPlugin/MNU/Iconos/INTEGRACION_AZURE.md`](../../TamdenZwcadPluging/ZwcadPlugin/MNU/Iconos/INTEGRACION_AZURE.md).

## Verificación

```powershell
# PAT y API
$headers = @{ Authorization = "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$env:AZDO_PAT")))" }
Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/git/repositories?api-version=7.1" -Headers $headers

# Tras sync: listar raíz en Azure (API)
$repoId = (Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/git/repositories/tandem2026?api-version=7.1" -Headers $headers).id
Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/git/repositories/$repoId/items?scopePath=/&recursionLevel=OneLevel&versionDescriptor.version=master&api-version=7.1" -Headers $headers
```

## Notas de seguridad

- No commitear PAT en scripts ni markdown. Usar `$env:AZDO_PAT`.
- Si un PAT antiguo aparece en el historial de Git, rotarlo (ver `SECURITY-INCIDENT-RESPONSE.md`).
