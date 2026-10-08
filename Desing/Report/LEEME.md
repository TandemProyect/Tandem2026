# Informes quincenales

Cuando el usuario pida el **informe quincenal**, el agente lo crea aquí. No hace falta que recuerde la ruta: esta carpeta y la regla `.cursor/rules/informe-quincenal.mdc` son la instrucción.

## Dónde se guarda

```text
Desing/Report/<año>/yyyy-MM-dd_Informe-Quincenal.md
Desing/Report/<año>/yyyy-MM-dd_Informe-Quincenal.pdf
```

`<año>` es el año de la fecha del corte (hoy, al pedirlo).

- En 2026 los informes van en `Desing/Report/2026/`.
- El 1 de enero de 2027, el primer informe de ese año crea `Desing/Report/2027/`. No crear el año siguiente antes de tiempo.
- Si la carpeta del año no existe, crearla en ese momento. No pedir confirmación.

El informe 01 ya está en `Desing/Report/2026/2026-10-08_Informe-Quincenal.md` y en el PDF del mismo nombre.

## Qué periodo cubre

El 01 fue la excepción: dos meses, del 5 de agosto al 8 de octubre de 2026.

A partir de ese corte, cada informe es una quincena y no reescribe el anterior.

| Corte | Periodo |
|-------|---------|
| Día 15 | Desde el día siguiente al informe anterior hasta el 15 |
| Último día del mes | Desde el 16 hasta ese día |

Si lo piden a mitad de quincena, el periodo es desde el día siguiente al último informe hasta el día de la petición. El siguiente informe arranca al día siguiente de este.

El número sale del último informe existente (01, 02, 03…), mirando este año y el anterior.

## De dónde sale el contenido

Chats de Cursor, chats de Visual Studio Code (Copilot) y la documentación del repo (handovers, `LEEME`, README de lo que se tocó). El historial de git no es fuente: los mensajes de commit no siguen el trabajo.

Quedan fuera los chats personales (perfil, compras, encargos ajenos a Tandem).

El texto va en español, agrupado por fecha. Al final: en qué quedó cada frente, qué sigue abierto y la fecha del próximo corte.

## PDF

El Markdown y el PDF se guardan juntos, con el mismo nombre. Cada fichero nuevo se añade a `Desing/Design.csproj` como `<None Include="Report\...">`. Si no está en el proyecto, Visual Studio no muestra la carpeta Report en el Explorador de soluciones. El PDF se imprime desde un HTML con el mismo texto (A4, sin membrete del navegador). Chrome, si está instalado:

```powershell
& "C:\Program Files\Google\Chrome\Application\chrome.exe" `
  --headless=new --disable-gpu --no-first-run --no-pdf-header-footer `
  --print-to-pdf="Desing\Report\<año>\yyyy-MM-dd_Informe-Quincenal.pdf" `
  "file:///ruta/al.html"
```

El HTML es un paso intermedio. No se deja en `Report`.
