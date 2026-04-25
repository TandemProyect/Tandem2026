# INSTRUCCIONES: Crear Columnas en el Panel de Azure DevOps

## Problema Encontrado
La API de Azure DevOps (v7.1-preview.1) para actualizar columnas del board está devolviendo el error:
`Value cannot be null. Parameter name: options`

Esto parece ser una limitación o bug de la API preview.

## Solución Manual (Recomendada)

1. Abre el panel en Azure DevOps:
   https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues

2. Haz clic en el icono de configuración (⚙️) en la esquina superior derecha del board

3. Selecciona "Column options" o "Configurar columnas"

4. Elimina las columnas actuales (To Do, Doing, Done) excepto la primera y la última

5. Agrega las siguientes columnas en orden con sus límites WIP:

   - **New** (incoming) - WIP: 50 - Estado: To Do
   - **Tareas a Analizar** (inProgress) - WIP: 10 - Estado: To Do
   - **Esperando documentacion** (inProgress) - WIP: 10 - Estado: To Do
   - **Preparado para Realizar** (inProgress) - WIP: 10 - Estado: Doing
   - **Realizando** (inProgress) - WIP: 5 - Estado: Doing
   - **Mal Testeo Volver a Realizar** (inProgress) - WIP: 5 - Estado: Doing
   - **Preparando a testear** (inProgress) - WIP: 5 - Estado: Doing
   - **Preparado para presentar** (inProgress) - WIP: 10 - Estado: Done
   - **Closed** (outgoing) - WIP: 300 - Estado: Done

6. Guarda los cambios

## Alternativa: Azure CLI

Si tienes Azure CLI instalado, puedes intentar:
```bash
az boards work-item update --id <work-item-id> --fields System.BoardColumn="Tareas a Analizar"
```

## Nota para Futuras Sesiones
Este problema con la API debe ser reportado a Microsoft o esperarse una versión estable de la API de boards.
