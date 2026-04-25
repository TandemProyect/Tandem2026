# 📜 Scripts de Gestión de Azure DevOps

Este directorio contiene scripts PowerShell para automatizar la gestión de User Stories y Tasks en Azure DevOps.

---

## 📚 Documentación Completa

**Antes de usar estos scripts, lee:**  
[📘 Docs/General/GESTION-PANEL-AZURE-DEVOPS.md](../Docs/General/GESTION-PANEL-AZURE-DEVOPS.md)

Incluye:
- ✅ Configuración correcta (organización, PAT, URLs)
- ❌ Errores comunes y cómo evitarlos
- 🛠️ Troubleshooting detallado
- 📋 Ejemplos completos

---

## 🚀 Scripts Disponibles

### 1. **Quick-US.ps1** ⚡ (Recomendado)

**Propósito:** Crear User Story + 3 Tasks estándar (Develop, Test, CR) en un solo comando.

**Uso:**
```powershell
.\Quick-US.ps1 "Título de la US"
.\Quick-US.ps1 "Título de la US" "Descripción detallada"
```

**Ejemplo:**
```powershell
.\Quick-US.ps1 "Implementar exportación de datos a CSV"
.\Quick-US.ps1 "Agregar filtros avanzados" "Permitir filtrar por fecha, usuario y estado"
```

**Salida:**
```
========================================
✅ USER STORY COMPLETA
========================================

📋 User Story #640
   Título: Implementar exportación de datos a CSV

📌 Tasks creadas:
   #641 - Develop
   #642 - Test
   #643 - CR

🔗 Enlaces:
   US:    https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/640
   Board: https://dev.azure.com/VSCAD/tandem2026/_boards/board/...

💡 Próximos pasos:
   1. Revisar la US en Azure DevOps
   2. Ajustar prioridad si es necesario:
	  .\Edit-US.ps1 640 -Prioridad 1
   3. Vincular commits con: AB#640
	  git commit -m "feat: descripción AB#640"
```

---

### 2. **US.ps1**

**Propósito:** Crear una User Story simple (sin tasks).

**Uso:**
```powershell
.\US.ps1 "Título"
.\US.ps1 "Título" "Descripción"
```

**Ejemplo:**
```powershell
.\US.ps1 "Mejorar performance del dashboard"
.\US.ps1 "Agregar soporte para dark mode" "Implementar tema oscuro en toda la aplicación"
```

**Cuándo usar:**
- Cuando necesitas crear solo la US sin tasks
- Para crear múltiples US rápidamente sin tasks asociadas
- Cuando las tasks se crearán más adelante manualmente

---

### 3. **Task.ps1**

**Propósito:** Crear una Task individual vinculada a una User Story.

**Uso:**
```powershell
.\Task.ps1 <ParentID> "Título de la Task"
.\Task.ps1 <ParentID> "Título" "Descripción"
.\Task.ps1 <ParentID> "Título" -Estado "Doing"
```

**Parámetros:**
- `ParentID`: ID de la User Story (ej: 613)
- `Titulo`: Título de la Task
- `Descripcion` (opcional): Descripción detallada
- `Estado` (opcional): "To Do" | "Doing" | "Done"

**Ejemplo:**
```powershell
.\Task.ps1 613 "Develop"
.\Task.ps1 613 "Test" "Validar funcionalidad en ZWCAD"
.\Task.ps1 613 "Documentación" "Actualizar README con ejemplos" -Estado "Doing"
```

**Cuándo usar:**
- Para agregar tasks adicionales a una US existente
- Para crear tasks con nombres personalizados (no solo Develop/Test/CR)
- Para crear tasks con un estado inicial específico

---

### 4. **Edit-US.ps1**

**Propósito:** Editar una User Story existente (estado, prioridad, título, asignación).

**Uso:**
```powershell
.\Edit-US.ps1 <ID> -Titulo "Nuevo título"
.\Edit-US.ps1 <ID> -Estado "Doing"
.\Edit-US.ps1 <ID> -Prioridad 1
.\Edit-US.ps1 <ID> -AsignadoA "email@example.com"
```

**Parámetros disponibles:**
- `-Titulo "..."`: Cambiar título
- `-Descripcion "..."`: Cambiar descripción
- `-Estado "To Do" | "Doing" | "Done"`: Cambiar estado
- `-Prioridad 1-4`: Cambiar prioridad (1=Alta, 4=Baja)
- `-AsignadoA "email"`: Asignar a usuario

**Ejemplos:**
```powershell
# Marcar US como en progreso
.\Edit-US.ps1 613 -Estado "Doing"

# Marcar US como completada
.\Edit-US.ps1 613 -Estado "Done"

# Cambiar título y prioridad
.\Edit-US.ps1 613 -Titulo "Nuevo nombre más descriptivo" -Prioridad 1

# Múltiples cambios
.\Edit-US.ps1 613 -Estado "Doing" -Prioridad 1 -AsignadoA "juan@example.com"
```

**Cuándo usar:**
- Para actualizar el estado de una US (To Do → Doing → Done)
- Para cambiar la prioridad según urgencia
- Para renombrar o aclarar el título
- Para asignar trabajo a un miembro del equipo

---

### 5. **Completar-Tasks-US.ps1**

**Propósito:** Marcar todas las Tasks de una US como "Done" de una vez.

**Uso:**
```powershell
.\Completar-Tasks-US.ps1 <UsId>
```

**Ejemplo:**
```powershell
.\Completar-Tasks-US.ps1 613
```

**Salida:**
```
US #613: CR

Completando 3 tasks...
✓ Task #634 - Develop marcada como Done
✓ Task #635 - Test marcada como Done
✓ Task #636 - CR marcada como Done

✓ Todas las tasks completadas
```

**Cuándo usar:**
- Al finalizar todo el trabajo de una US
- Antes de marcar la US como "Done"
- Para hacer cleanup rápido de tasks pendientes

---

## 🎯 Flujos de Trabajo Comunes

### Flujo 1: Crear y completar una US completa

```powershell
# 1. Crear US con tasks
.\Quick-US.ps1 "Implementar nueva funcionalidad"
# Resultado: US #640 con tasks #641, #642, #643

# 2. Trabajar en el código y hacer commits
git add .
git commit -m "feat: Implementar base de la funcionalidad AB#640"
git commit -m "test: Agregar tests unitarios AB#640"
git push

# 3. Actualizar estado de la US
.\Edit-US.ps1 640 -Estado "Doing"

# 4. Al terminar, completar todas las tasks
.\Completar-Tasks-US.ps1 640

# 5. Cerrar la US
.\Edit-US.ps1 640 -Estado "Done"

# 6. Commit final
git commit -m "feat: Completar funcionalidad Closes AB#640"
git push
```

---

### Flujo 2: Agregar tasks adicionales a una US existente

```powershell
# Supongamos que US #613 ya existe con Develop/Test/CR
# Pero necesitamos agregar más tasks

.\Task.ps1 613 "Documentación" "Actualizar docs con ejemplos"
.\Task.ps1 613 "Deploy" "Desplegar a staging"
.\Task.ps1 613 "Performance Testing" "Validar tiempos de respuesta"
```

---

### Flujo 3: Crear múltiples US rápidamente

```powershell
# Crear varias US sin tasks (para planificación)
.\US.ps1 "Implementar autenticación OAuth"
.\US.ps1 "Agregar logging centralizado"
.\US.ps1 "Mejorar UI del dashboard"
.\US.ps1 "Implementar cache Redis"

# Luego, para cada una que se vaya a trabajar, agregar tasks
# Por ejemplo, para US #650:
.\Task.ps1 650 "Develop"
.\Task.ps1 650 "Test"
.\Task.ps1 650 "CR"
```

---

### Flujo 4: Gestión diaria de trabajo

```powershell
# Mañana: Marcar US en progreso
.\Edit-US.ps1 613 -Estado "Doing"

# Durante el día: Commits vinculados
git commit -m "feat: Avance en funcionalidad AB#613"
git commit -m "fix: Corregir validación AB#613"

# Tarde: Completar tasks individuales (manual en Azure DevOps o API)

# Final del día: Si terminas
.\Completar-Tasks-US.ps1 613
.\Edit-US.ps1 613 -Estado "Done"
```

---

## ⚠️ Configuración y Errores Comunes

### ✅ Valores Correctos (NO MODIFICAR)

```powershell
$org = "VSCAD"
$project = "tandem2026"
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
```

### ❌ Error 404: "No se encontró"

**Causa:** Organización o proyecto incorrecto

**Solución:**
```powershell
# ❌ INCORRECTO
$org = "juangodoylopez"
$project = "Tandem 2026"

# ✅ CORRECTO
$org = "VSCAD"
$project = "tandem2026"
```

### ❌ Error 401: "No autorizado"

**Causa:** PAT incorrecto o expirado

**Solución:** Verificar el PAT en el script `US.ps1` línea 10 y copiar exactamente ese valor.

---

## 🔗 Enlaces Útiles

| Recurso | URL |
|---------|-----|
| **Panel del Proyecto** | https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues |
| **Documentación Completa** | [GESTION-PANEL-AZURE-DEVOPS.md](../Docs/General/GESTION-PANEL-AZURE-DEVOPS.md) |
| **Crear Work Item Manual** | https://dev.azure.com/VSCAD/tandem2026/_workitems/create/Issue |
| **API REST Docs** | https://learn.microsoft.com/rest/api/azure/devops/wit/ |

---

## 📋 Resumen de Comandos

| Acción | Comando |
|--------|---------|
| Crear US + Tasks | `.\Quick-US.ps1 "Título"` |
| Crear US sola | `.\US.ps1 "Título"` |
| Crear Task | `.\Task.ps1 613 "Título"` |
| Cambiar estado US | `.\Edit-US.ps1 613 -Estado "Doing"` |
| Cambiar prioridad | `.\Edit-US.ps1 613 -Prioridad 1` |
| Completar todas tasks | `.\Completar-Tasks-US.ps1 613` |
| Cerrar US | `.\Edit-US.ps1 613 -Estado "Done"` |

---

## 💡 Tips

1. **Usa `Quick-US.ps1`** como comando principal para nuevas US
2. **Vincula commits desde el primer cambio** con `AB#<ID>`
3. **Actualiza estados regularmente** para reflejar progreso real
4. **Lee la documentación completa** en `Docs/General/GESTION-PANEL-AZURE-DEVOPS.md` al menos una vez
5. **No modifiques los valores de `$org` y `$project`** en los scripts

---

**Última actualización:** 2026-04-24  
**Mantenido por:** Equipo Tandem 2026
