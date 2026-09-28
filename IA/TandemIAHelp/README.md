# TandemIAHelp

TandemIAHelp nace para convertir videos cortos de uso de Tandem en material de ayuda: explicacion automatica, voz, subtitulos y, mas adelante, avatar.

## MVP inicial

Empezamos con una marca minima por icono. El primer caso es `wall.create.plan`, asociado al boton **dibujar muro en planta** de la barra superior de Desing_2.

## Flujo previsto

1. El usuario graba un video usando una funcion concreta.
2. TandemIAHelp identifica la funcion por su marca `data-tandem-help-id` o por seleccion manual inicial.
3. Se carga la ficha de ayuda correspondiente desde `help-items`.
4. La IA genera el guion narrado a partir del video y de la ficha tecnica.
5. Una herramienta de montaje genera un nuevo video con voz, subtitulos y, en una fase posterior, avatar.

## Primer criterio de exito

Dado un video corto donde se pulsa el icono de dibujar muro en planta, el sistema debe poder asociarlo a `wall.create.plan` y generar una explicacion breve y correcta de la funcion.

## Generar primera narracion

Sin IA externa todavia, podemos comprobar que la ficha contiene suficiente contexto para producir una narracion base:

```powershell
.\IA\TandemIAHelp\tools\Generate-Narration.cmd -HelpId wall.create.plan -DurationSeconds 75
```

La salida se guarda en `IA/TandemIAHelp/output/wall.create.plan.narration.md`.

## Siguiente decision pendiente

Elegir si el primer prototipo sera solo documental (marca + ficha + guion generado) o si ya prepararemos una utilidad local para leer videos y producir una salida automatica.