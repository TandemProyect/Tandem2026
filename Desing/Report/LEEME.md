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
| 02 | `2026-10-22_Informe-Quincenal.md` | 9 oct – 22 oct 2026 | 22 oct | cerrado el 22 oct |
| **03 (abierto)** | `2026-11-04_Informe-Quincenal.md` | **22 oct – 4 nov 2026** | **4 nov** | el **4 nov** se crea el 04 |

**Mientras hoy sea el 22 de octubre de 2026 o posterior, y no haya pasado el 4 de noviembre:** el informe activo es el **03**. El **4 de noviembre** el agente crea el **04** y pasa a ese fichero. No preguntar al usuario si es trimestral o quincenal, ni qué fichero usar.

### Qué hacer en un chat (nuevo o antiguo)

1. Leer este `LEEME.md` y abrir el informe **abierto** (hoy: el 03).
2. Si el trabajo es un frente nuevo: crear el **siguiente apartado correlativo**  
   (`### N. Nombre del frente`, p. ej. `### 1. Proyecto XR`, `### 2. …`).
3. Si el frente ya tiene apartado: **trabajar en ese apartado** (ampliar bullets, estado, fechas). No abrir otro con el mismo nombre.
4. Si el usuario dice que empezamos algo nuevo: abrir el apartado ya y anotar la hora de la persona y la de Cursor o Copilot. Al devolver un bloque para probar, anotar la hora de esa entrega. Cuando diga que se termina o que se pasa a otra cosa, cerrar: fin de la persona en ese mensaje, fin del agente en la última entrega (o en el cierre, si siguió hasta entonces). El tramo de la persona incluye el testeo. No alargarlo. No sumar los dos relojes. Copilot solo se anota en el momento.
5. **No** reescribir un informe cerrado (el 01 y el 02).
6. No hay informe trimestral. Las horas y el relato van en el quincenal, en el apartado del frente. El T4 de 2026 se unió al informe 01 (`2026-10-08_Informe-Quincenal.md`). No volver a crear `yyyy-T<n>_Informe-Trimestral`.

### Creación automática del siguiente informe

- El día **después** del review del informe abierto, el agente **crea solo** el siguiente quincenal (nuevo número, nuevo `yyyy-MM-dd_Informe-Quincenal.md` con la fecha de corte prevista), registra MD+PDF en el `.csproj`, y deja de escribir en el anterior.
- Ejemplo: el **22 oct 2026** se creó el **03** (`2026-11-04_Informe-Quincenal.md`), periodo **22 oct – 4 nov**, review **4 nov**. El 02 quedó cerrado ese día. El **4 nov** se crea el **04** y se deja de escribir en el 03.
- No hace falta que diga «abre el informe». Basta con trabajar; el agente elige el fichero abierto o crea el nuevo si ya toca.

## Calendario (tras el 01)

El 01 fue la excepción (dos meses). A partir del 02:

| Informe | Periodo | Review |
|---------|---------|--------|
| 02 | 9–22 oct 2026 | 22 oct 2026 |
| 03 | 22 oct – 4 nov 2026 | 4 nov 2026 |

El **22 oct** es el corte del 02 y el primer día del 03. A partir de ese día el trabajo nuevo va al 03. Si más adelante cambia el corte, actualizar esta tabla.

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

## Horas

No hay informe trimestral. El de T4 2026 se unió al quincenal 01. En cada apartado van tres horas, escritas en el momento: inicio de la persona, inicio de Cursor o de Copilot, y la entrega del agente. Al cerrar, el fin de cada uno. No inventarlas y no sumarlas entre sí. Al final del informe: resumen y las tres sumas por separado. Las manuales (lo que no es programación) solo cuando se pidan, y no mezcladas con las de programación.
