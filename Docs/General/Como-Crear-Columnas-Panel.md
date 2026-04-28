# Columnas del Panel de Azure DevOps — Tandem 2026

> **Estado:** ✅ RESUELTO (2026-04-28)  
> **Script:** `C:\00_Tandem2026\Scripts\Restructurar-Panel.ps1`

## Estructura Actual (9 columnas)

| Columna | Tipo | WIP | Estado ADO |
|---------|------|-----|------------|
| **New** | incoming | 50 | To Do |
| **Tareas a Analizar** | inProgress | 10 | To Do |
| **Esperando documentacion** | inProgress | 10 | To Do |
| **Preparado para Realizar** | inProgress | 10 | Doing |
| **Realizando** | inProgress | 5 | Doing |
| **Mal Testeo Volver a Realizar** | inProgress | 5 | Doing |
| **Preparando a testear** | inProgress | 5 | Doing |
| **Preparado para presentar** | inProgress | 10 | Doing |
| **Closed** | outgoing | 300 | Done |

## Solución Correcta vía API

El error anterior (`Value cannot be null. Parameter name: options`) ocurría porque se usaba `PATCH` en lugar de `PUT` y no se incluían los IDs de las columnas fijas.

**Reglas de la API:**

1. Usar **`PUT`** (no PATCH) — reemplaza todas las columnas de una vez
2. Obtener primero los **IDs actuales** de las columnas `incoming` y `outgoing` con un GET previo
3. Incluir esos IDs en el payload — son columnas fijas que no se pueden eliminar
4. Las columnas `inProgress` solo admiten estados `To Do` o `Doing` — **nunca `Done`**
5. Solo la columna `outgoing` puede tener estado `Done`

**URL correcta:**
```
PUT https://dev.azure.com/VSCAD/tandem2026/tandem2026%20Team/_apis/work/boards/Issues/columns?api-version=7.0
```

**Para re-aplicar la estructura:**
```powershell
cd C:\00_Tandem2026\Scripts
.\Restructurar-Panel.ps1
```

**Para verificar columnas actuales:**
```powershell
.\Verificar-Panel.ps1
```

## IDs de Referencia

- **Board ID:** `892fa957-9c33-4237-a99f-2660bd9ec80d`
- **Columna incoming (New):** `720e658c-5da2-4ddd-a741-3863cc36ae6c`
- **Columna outgoing (Closed):** `bde86b62-6374-4bb3-8e65-0f5917ab8b20`
