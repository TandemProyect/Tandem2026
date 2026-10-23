# Informes de trabajo

Cuando el usuario pida el **informe** (quincenal hasta el 03, semanal desde el 04), o cuando un agente tenga que anotar trabajo de producto, seguir esta carpeta y la regla `.cursor/rules/informe-quincenal.mdc`.

## Dónde se guarda

```text
Desing/Report/<año>/yyyy-MM-dd_Informe-Quincenal.md
Desing/Report/<año>/yyyy-MM-dd_Informe-Semanal.md
```

El 01, el 02 y el 03 llevan `Informe-Quincenal` en el nombre. Desde el 04, `Informe-Semanal`. El PDF va al lado, con el mismo nombre.

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

**Mientras hoy sea anterior al 4 de noviembre de 2026:** el informe activo es el **03**. El **4 de noviembre** el agente crea el **04**, ya semanal, y pasa a ese fichero. No preguntar al usuario si es trimestral, quincenal o semanal.

### Qué hacer en un chat (nuevo o antiguo)

1. Leer este `LEEME.md` y abrir el informe **abierto** (hoy: el 03).
2. Si el trabajo es un frente nuevo: crear el **siguiente apartado correlativo**  
   (`### N. Nombre del frente`, p. ej. `### 1. Proyecto XR`, `### 2. …`).
3. Si el frente ya tiene apartado: **trabajar en ese apartado** (ampliar bullets, estado, fechas). No abrir otro con el mismo nombre.
4. Si el usuario dice que empezamos algo nuevo: abrir el apartado ya y anotar la hora de la persona y la de Cursor o Copilot. Al devolver un bloque para probar, anotar la hora de esa entrega. Cuando diga que se termina o que se pasa a otra cosa, cerrar: fin de la persona en ese mensaje, fin del agente en la última entrega (o en el cierre, si siguió hasta entonces). El tramo de la persona incluye el testeo. No alargarlo. No sumar los dos relojes. Copilot solo se anota en el momento.
5. **No** reescribir un informe cerrado (el 01 y el 02).
6. No hay informe trimestral. Relato y horas van en el informe abierto. El T4 de 2026 se unió al informe 01 (`2026-10-08_Informe-Quincenal.md`). No volver a crear `yyyy-T<n>_Informe-Trimestral`.

### Creación automática del siguiente informe

- El día **del review** del informe abierto, el agente **crea solo** el siguiente (nuevo número, nuevo fichero con la fecha de corte prevista), registra MD+PDF en el `.csproj`, y deja de escribir en el anterior.
- El **03** se cierra el **4 nov 2026**. Ese día se crea el **04**: `2026-11-11_Informe-Semanal.md`, periodo **4–11 nov**, review **11 nov**. Desde ahí el corte es semanal, cada miércoles.
- No hace falta que diga «abre el informe». Basta con trabajar; el agente elige el fichero abierto o crea el nuevo si ya toca.

## Calendario (tras el 01)

El 01 fue la excepción (dos meses). A partir del 02:

| Informe | Periodo | Review |
|---------|---------|--------|
| 02 | 9–22 oct 2026 | 22 oct 2026 |
| 03 | 22 oct – 4 nov 2026 | 4 nov 2026 |

El **22 oct** es el corte del 02 y el primer día del 03. A partir de ese día el trabajo nuevo va al 03. Si más adelante cambia el corte, actualizar esta tabla.

## Pasa a ser semanal (decisión del 23 oct 2026)

El 03 sigue quincenal y abierto hasta su review. **No se parte ni se reescribe.**

| Informe | Fichero | Periodo | Review |
|---------|---------|---------|--------|
| 03 (último quincenal, abierto) | `2026-11-04_Informe-Quincenal.md` | 22 oct – 4 nov 2026 | 4 nov 2026 |
| **04 (primer semanal)** | `2026-11-11_Informe-Semanal.md` | **4 – 11 nov 2026** | **11 nov 2026** |

A partir del 04 el corte es **cada miércoles**, siete días, y el nombre del fichero es `yyyy-MM-dd_Informe-Semanal.md` (la fecha es el día de review). El 11 nov se crea el 05 con review el 18 nov, y así sucesivamente. Las horas, los apartados y el PDF se hacen igual que hasta ahora. No renombrar el 01, el 02 ni el 03.

El **23 oct 2026** se abrió además `Report/2026/Informe Semanal 26102026-2112026.md`, periodo **26 oct – 2 nov 2026**. Lleva el relato del 22 oct (con las imágenes) y lo de esta semana. Las horas ya cerradas del 22 no se vuelven a sumar: siguen en el 03. Las horas nuevas de esta semana van en ese semanal. El 03 no se reescribe.

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
