# Actualización del .csproj para Copiar Iconos

**Archivo:** `TamdenZwcadPluging\ZwcadPlugin\ZwcadPlugin.csproj`

## Cambio Necesario

Reemplazar el target `CopiarMNU` (líneas 119-122) con:

```xml
  <!-- Copia la carpeta MNU al directorio de salida manteniendo la estructura -->
  <Target Name="CopiarMNU" AfterTargets="Build">
	<Copy SourceFiles="MNU\Tandem2026.cui" DestinationFolder="$(OutputPath)MNU\" SkipUnchangedFiles="true" />
	<!-- Copia todos los iconos de la carpeta MNU\Iconos -->
	<ItemGroup>
	  <IconosFiles Include="MNU\Iconos\**\*.*" />
	</ItemGroup>
	<Copy SourceFiles="@(IconosFiles)" DestinationFolder="$(OutputPath)MNU\Iconos\%(RecursiveDir)" SkipUnchangedFiles="true" />
  </Target>
```

## Cómo Aplicar

1. Cierra la solución en Visual Studio
2. Abre `ZwcadPlugin.csproj` con un editor de texto
3. Busca la sección `<Target Name="CopiarMNU"`
4. Reemplaza con el código de arriba
5. Guarda y reabre la solución

## Resultado

Después de compilar, la carpeta `bin\Debug\MNU\` contendrá:
```
MNU\
├── Tandem2026.cui
└── Iconos\
	├── .gitkeep
	├── README_ICONOS.md
	└── (todos los archivos .png que agregues)
```
