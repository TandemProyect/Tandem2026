# Gestion de Agentes para este repositorio

Este documento define como crear, nombrar y mantener agentes de Copilot para el proyecto Tandem.

## Objetivo

- Estandarizar el uso de agentes en tareas repetibles.
- Reducir errores en cambios de alto riesgo.
- Acelerar revisiones tecnicas con flujos consistentes.
- Controlar costo de computo con ciclos cortos y alcance acotado.

## Politica de ejecucion

- Trabajar en microtareas de 30-60 min.
- Entregar plan inicial de maximo 5 puntos.
- Implementar primero el minimo funcional.
- No avanzar sin confirmacion del usuario.
- Si hay 2 intentos fallidos, cambiar estrategia y explicar trade-offs.

## Ticket obligatorio

Cada solicitud debe incluir:

- Objetivo
- Input/Output esperado
- Restricciones
- Criterio de exito
- Tiempo limite

## Estructura recomendada

- `.github/agents/` para agentes personalizados.
- `.github/prompts/` para prompts reutilizables.
- `.github/instructions/` para reglas por tipo de archivo.

## Convenciones de nombres

- Agentes: `kebab-case.agent.md`
- Prompts: `kebab-case.prompt.md`
- Instrucciones: `kebab-case.instructions.md`

## Criterios para crear un agente

Crea un agente nuevo solo si cumple al menos una condicion:

- El flujo tiene 3 o mas pasos repetibles.
- Requiere aislamiento de contexto respecto al chat principal.
- Necesita restricciones de herramientas o salida muy definida.

## Flujo de gestion

1. Definir alcance y resultado esperado.
2. Crear o actualizar archivo en `.github/agents/`.
3. Probar el agente con al menos 2 escenarios reales.
4. Ajustar descripcion para mejorar descubrimiento.
5. Registrar cambios relevantes en el historial del repositorio.

## Mantenimiento

- Revisar agentes activos cada 30 dias.
- Eliminar agentes sin uso en 60 dias.
- Unificar agentes que se solapen en objetivo o salida.

## Checklist rapido

- Descripcion clara de cuando usarlo.
- Entradas esperadas bien definidas.
- Salida final con formato consistente.
- Sin instrucciones ambiguas o contradictorias.
