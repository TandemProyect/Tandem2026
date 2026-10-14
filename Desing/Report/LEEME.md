# Informes quincenales

Cuando el usuario pida el **informe quincenal**, o cuando un agente tenga que anotar trabajo de producto, seguir esta carpeta y la regla `.cursor/rules/informe-quincenal.mdc`.

## Dónde se guarda

```text
Desing/Report/<año>/yyyy-MM-dd_Informe-Quincenal.md
Desing/Report/<año>/yyyy-MM-dd_Informe-Quincenal.pdf
```

`<año>` es el año del **día de corte / review** del informe.

- En 2026: `Desing/Report/2026/`.
- El 1 de enero de 2027, el primer informe de ese año crea `2027`. No crear años por adelantado.
- Si la carpeta del año no existe, crearla. No pedir confirmación.
- Markdown y PDF juntos. Ambos en `Desing/Design.csproj` como `<None Include="Report\...">`.

## Qué informe está abierto (regla para agentes)

| Informe | Fichero | Periodo | Review | Siguiente |
|---------|---------|---------|--------|-----------|
| 01 | `2026-10-08_Informe-Quincenal.md` | 5 ago – 8 oct 2026 | 8 oct | cerrado |
| **02 (abierto)** | `2026-10-22_Informe-Quincenal.md` | **9 oct – 22 oct 2026** | **22 oct** | **23 oct → informe 03** |
| 03 | (aún no) | desde 23 oct | según calendario | … |

**Mientras hoy esté entre el 9 y el 22 de octubre de 2026 inclusive:** el informe activo es el **02**. No preguntar al usuario si es trimestral o quincenal, ni qué fichero usar.

### Qué hacer en un chat (nuevo o antiguo)

1. Leer este `LEEME.md` y abrir el informe **abierto** (hoy: el 02).
2. Si el trabajo es un frente nuevo: crear el **siguiente apartado correlativo**  
   (`### N. Nombre del frente`, p. ej. `### 1. Proyecto XR`, `### 2. …`).
3. Si el frente ya tiene apartado: **trabajar en ese apartado** (ampliar bullets, estado, fechas). No abrir otro con el mismo nombre.
4. Si el usuario dice que empezamos algo nuevo: abrir el apartado ya, con la hora de inicio. Cuando diga que se termina o que se pasa a otra cosa, cerrar y anotar el tramo de reloj (inicio → ese momento). Es una estimación de la sesión, con el testeo. No es el tiempo del agente ni una hora de facturación. No alargarla.
5. **No** reescribir un informe cerrado (el 01).
6. Las **horas de programación** que ya se cierran van al **trimestral** (`yyyy-T<n>_Informe-Trimestral.md`). La estimación de sesión del punto 4 se queda en el apartado del quincenal y no se suma al trimestral.

### Creación automática del siguiente informe

- El día **después** del review del informe abierto, el agente **crea solo** el siguiente quincenal (nuevo número, nuevo `yyyy-MM-dd_Informe-Quincenal.md` con la fecha de corte prevista), registra MD+PDF en el `.csproj`, y deja de escribir en el anterior.
- Ejemplo actual: review del 02 = **22 oct** → el **23 oct** se crea el **03** sin que el usuario lo pida.
- No hace falta que diga «abre el informe». Basta con trabajar; el agente elige el fichero abierto o crea el nuevo si ya toca.

## Calendario (tras el 01)

El 01 fue la excepción (dos meses). A partir del 02:

| Informe | Periodo | Review |
|---------|---------|--------|
| 02 | 9–22 oct 2026 | 22 oct 2026 |
| 03 | desde 23 oct … | se fija al crearlo (siguiente bloque ~2 semanas / review acordada) |

Si en el futuro se vuelve al ritmo «corte el 15 / último día del mes», actualizar esta tabla. Mientras el usuario diga «del 9 al 22» / «el 23 uno nuevo», manda esa regla.

## De dónde sale el contenido

Chats de Cursor, chats de Visual Studio Code (Copilot) y la documentación del repo. **No** el historial de git.

Fuera: chats personales (perfil, compras, encargos ajenos a Tandem).

Español. Apartados numerados por frente. Al final del informe (o al review): frentes, abierto y fecha del próximo corte.

## PDF

Imprimir desde HTML intermedio (A4, sin membrete). Chrome, si está instalado:

```powershell
& "C:\Program Files\Google\Chrome\Application\chrome.exe" `
  --headless=new --disable-gpu --no-first-run --no-pdf-header-footer `
  --print-to-pdf="Desing\Report\<año>\yyyy-MM-dd_Informe-Quincenal.pdf" `
  "file:///ruta/al.html"
```

El HTML no se deja en `Report`. Al añadir o cerrar un apartado relevante, regenerar el PDF del informe abierto.

## Informe trimestral (horas)

Además del quincenal. El trimestral lleva las **horas**. Se actualiza al cerrar un trabajo.

```text
Desing/Report/<año>/yyyy-T<n>_Informe-Trimestral.md
Desing/Report/<año>/yyyy-T<n>_Informe-Trimestral.pdf
```

`n` = 1…4 (trimestre natural). Ahora: `2026-T4_Informe-Trimestral.md`.

| Clase | Qué es | Cuándo |
|-------|--------|--------|
| Programación | Código, visor, plugins, docs técnicas | Al cerrar ese trabajo |
| Manual | No programación (p. ej. bloques) | Cuando lo pidan, con horas |

No inventar horas. No mezclar clases. No reescribir un trimestre cerrado. Regla: `.cursor/rules/informe-trimestral.mdc`.
