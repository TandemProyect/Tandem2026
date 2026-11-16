# Guía Manual: Configurar Columnas del Board Tandem 2026

Esta guía te ayudará a configurar manualmente las columnas del board en Azure DevOps.

## 🎯 Columnas a Crear

1. **Tareas a Analizar** (WIP: 10)
2. **Esperando documentacion** (WIP: 10)
3. **Preparado para Realizar** (WIP: 10)
4. **Realizando** (WIP: 5)
5. **Mal Testeo Volver a Realizar** (WIP: 5)
6. **Preparando a testear** (WIP: 5)
7. **Preparado para presentar** (WIP: 10)

---

## 📋 Pasos para Configurar Manualmente

### **Paso 1: Abrir Configuración del Board**

1. Ir a: https://dev.azure.com/VSCAD/tandem2026/_settings/board-team
2. Asegurarse de estar en:
   - **Team:** tandem2026 Team
   - **Board:** Issues

---

### **Paso 2: Eliminar Columnas No Deseadas (si existen)**

Si tienes columnas como "Doing" o "Done" que no necesitas:

1. Buscar la columna en la lista
2. Click en **⋯** (tres puntos)
3. Seleccionar **"Delete"**
4. Confirmar eliminación

⚠️ **Nota:** Solo puedes eliminar columnas personalizadas, no las del sistema.

---

### **Paso 3: Crear Nuevas Columnas**

Para cada columna de la lista, seguir estos pasos:

#### **3.1 Agregar Columna**

1. Click en **"+ New column"** o **"Agregar columna"**

#### **3.2 Configurar Columna 1: Tareas a Analizar**

- **Name:** `Tareas a Analizar`
- **WIP limit:** `10`
- **Column type:** `In Progress`
- **Description:** `Tareas pendientes de análisis`
- **State mapping:**
  - Issue → Active
- Click **"Save"**

---

#### **3.3 Configurar Columna 2: Esperando documentacion**

- **Name:** `Esperando documentacion`
- **WIP limit:** `10`
- **Column type:** `In Progress`
- **Description:** `Tareas esperando documentación`
- **State mapping:**
  - Issue → Active
- Click **"Save"**

---

#### **3.4 Configurar Columna 3: Preparado para Realizar**

- **Name:** `Preparado para Realizar`
- **WIP limit:** `10`
- **Column type:** `In Progress`
- **Description:** `Listo para comenzar desarrollo`
- **State mapping:**
  - Issue → Active
- Click **"Save"**

---

#### **3.5 Configurar Columna 4: Realizando**

- **Name:** `Realizando`
- **WIP limit:** `5`
- **Column type:** `In Progress`
- **Description:** `En desarrollo activo`
- **State mapping:**
  - Issue → Active
- Click **"Save"**

---

#### **3.6 Configurar Columna 5: Mal Testeo Volver a Realizar**

- **Name:** `Mal Testeo Volver a Realizar`
- **WIP limit:** `5`
- **Column type:** `In Progress`
- **Description:** `Requiere corrección después de testing`
- **State mapping:**
  - Issue → Active
- Click **"Save"**

---

#### **3.7 Configurar Columna 6: Preparando a testear**

- **Name:** `Preparando a testear`
- **WIP limit:** `5`
- **Column type:** `In Progress`
- **Description:** `Preparando para pruebas`
- **State mapping:**
  - Issue → Active
- Click **"Save"**

---

#### **3.8 Configurar Columna 7: Preparado para presentar**

- **Name:** `Preparado para presentar`
- **WIP limit:** `10`
- **Column type:** `In Progress`
- **Description:** `Listo para presentar`
- **State mapping:**
  - Issue → Resolved
- Click **"Save"**

---

### **Paso 4: Ordenar Columnas**

Una vez creadas todas las columnas, ordénalas arrastrándolas en este orden:

1. New
2. Tareas a Analizar
3. Esperando documentacion
4. Preparado para Realizar
5. Realizando
6. Mal Testeo Volver a Realizar
7. Preparando a testear
8. Preparado para presentar
9. Closed

---

### **Paso 5: Guardar Configuración**

1. Click en **"Save"** en la parte superior/inferior de la página
2. Confirmar los cambios

---

## ✅ Verificar Cambios

1. Ir al board: https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
2. Verificar que aparezcan todas las columnas en el orden correcto
3. Probar mover un Issue entre columnas

---

## 📊 Resultado Final

Tu board debería verse así:

```
┌─────┬──────────────────────┬─────────────────────────┬────────────────────────┬────────────┐
│ New │ Tareas a Analizar    │ Esperando documentacion │ Preparado para Realizar│ Realizando │
│(50) │        (10)          │          (10)           │         (10)           │    (5)     │
└─────┴──────────────────────┴─────────────────────────┴────────────────────────┴────────────┘

┌─────────────────────────────┬─────────────────────┬───────────────────────────┬────────┐
│ Mal Testeo Volver a Realizar│ Preparando a testear│ Preparado para presentar  │ Closed │
│            (5)              │        (5)          │          (10)             │ (300)  │
└─────────────────────────────┴─────────────────────┴───────────────────────────┴────────┘
```

**WIP Limits:**
- New: 50
- Tareas a Analizar: 10
- Esperando documentacion: 10
- Preparado para Realizar: 10
- Realizando: 5
- Mal Testeo Volver a Realizar: 5
- Preparando a testear: 5
- Preparado para presentar: 10
- Closed: 300

---

## 🔧 Solución de Problemas

### **No puedo eliminar una columna**

- Solo se pueden eliminar columnas personalizadas
- Las columnas del sistema (New, Active, Resolved, Closed) no se pueden eliminar
- Si una columna tiene Work Items, debes moverlos antes de eliminarla

### **No puedo cambiar State Mapping**

- Los estados dependen del Process Template usado
- Si usas "Basic" process, tendrás: New, Active, Resolved, Closed
- Mapea las columnas a los estados disponibles en tu proceso

### **Los cambios no se guardan**

- Asegúrate de tener permisos de administrador del proyecto
- Verifica que no haya errores de validación en los campos
- Intenta refrescar la página y volver a intentar

---

## 📞 Ayuda

Si tienes problemas:
1. Verifica permisos en el proyecto
2. Consulta la documentación oficial: https://learn.microsoft.com/azure/devops/boards/boards/
3. Crea un Issue en Azure DevOps

---

**Última actualización:** 24/04/2026  
**Mantenido por:** Equipo Tandem 2026
