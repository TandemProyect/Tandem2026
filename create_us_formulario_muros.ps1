$pat = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $pat))
$h = @{
    Authorization  = 'Basic ' + $base64Auth
    'Content-Type' = 'application/json-patch+json'
}
$baseUri = 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems'

# ---------- 1) Crear US ----------
$usTitle = 'Crear formulario de Altura y datos de los muros seleccionados'
$usDesc  = '**Objetivo:**
Permitir al usuario, tras seleccionar lineas/muros en ZWCAD (TANDEM_SELECCIONAR_LINEAS), introducir parametros de configuracion antes de generar las extrusiones de muro:

- **Altura del muro** (mm) — actualmente hardcodeada a 2700 en LCornerDetector. Debera ser configurable.
- **Espesor del muro** (mm) — opcional, para casos sin esquinas L donde no se deduce automaticamente.
- **Tipo / nombre del proyecto** (opcional, para identificar el conjunto).
- **Observaciones** (opcional, texto libre).

**Flujo deseado:**
1. Usuario ejecuta TANDEM_SELECCIONAR_LINEAS y selecciona objetos.
2. Antes de enviar al endpoint MVC, plugin abre formulario web (o dialog) con campos.
3. Usuario rellena valores (con defaults sensatos: altura 2700, espesor null = autodetectar).
4. POST al endpoint con LineasDTO + ConfiguracionMurosDTO.
5. LCornerDetector usa la Altura recibida en lugar del literal 2700 al construir las PolilineaDTO de capa ModelDesing.

**Beneficios:**
- Adios a valores hardcodeados.
- Base para futuros parametros (color de capa, escala, etc.).
- Mismo formulario reutilizable en futuro cliente three.js (regla de oro Cliente-Servidor).

**Predecesoras:** US-688 cerrada (muros B/C/A/D/E/F implementados).
**Bloqueada por:** Bug #696 (revisar antes de cerrar).'

$usBody = @(
    @{ op = 'add'; path = '/fields/System.Title';           value = $usTitle },
    @{ op = 'add'; path = '/fields/System.Description';     value = $usDesc },
    @{ op = 'add'; path = '/fields/System.AreaPath';        value = 'tandem2026' },
    @{ op = 'add'; path = '/fields/System.IterationPath';   value = 'tandem2026' }
)
$usResp = Invoke-RestMethod -Uri "$baseUri/`$User%20Story?api-version=7.1" -Method Post -Headers $h -Body (ConvertTo-Json $usBody -Depth 10)
$usId = $usResp.id
Write-Output ("US #{0} creada: {1}" -f $usId, $usTitle)

# ---------- 2) Crear Tasks hijas ----------
$tasks = @(
    @{
        Title = 'T1: ConfiguracionMurosDTO (servidor + plugin) + endpoint que la acepte'
        Desc  = 'Crear DTO ConfiguracionMurosDTO en Desing/Models/ZwcadModels.cs con campos:
- double AlturaMuro (default 2700)
- double? EspesorMuroMm (nullable, opcional)
- string Nombre (opcional)
- string Observaciones (opcional)

Replicar espejo en TamdenZwcadPluging/ZwcadPlugin/Models.cs.

Modificar el endpoint del controlador (DesignToolsAutocadController) que recibe SeleccionLineasDTO para tambien aceptar ConfiguracionMurosDTO en el body. Pasar los valores a LCornerDetector via parametros o sobrecarga del metodo DetectarEsquinasL.'
    },
    @{
        Title = 'T2: LCornerDetector usa AlturaMuro recibido en vez de literal 2700'
        Desc  = 'En Desing/Services/LCornerDetector.cs:
- Anadir parametro double alturaMuro al metodo principal (con default 2700 para no romper llamadas existentes).
- Reemplazar AlturaExtrusion = 2700 por AlturaExtrusion = alturaMuro en todos los AgregarMuroRecto y similares (US-679, T5, T6, T7).
- Si EspesorMuroMm viene rellenado y no se detecta automaticamente, usarlo como espesor para muros E/F (T7).

Sin cambios en el plugin: la altura se aplica server-side, el plugin sigue leyendo PolilineaDTO.AlturaExtrusion.'
    },
    @{
        Title = 'T3: Vista Razor del formulario en MVC (HTML + CSS + JS validacion)'
        Desc  = 'Crear vista Desing/Views/DesignToolsAutocad/FormularioConfiguracionMuros.cshtml con:
- Campos: Altura (number, default 2700, min 100, max 10000), Espesor (number opcional), Nombre (text), Observaciones (textarea).
- Estilo coherente con el resto de la app (Bootstrap o el framework usado).
- Validacion cliente con jQuery validate.
- Boton "Aceptar" que envia POST al endpoint con los datos del formulario + las lineas previamente capturadas.
- Boton "Cancelar" que vuelve atras.

El controlador correspondiente debe servir la vista por GET y procesar el POST.'
    },
    @{
        Title = 'T4: Plugin ZWCAD abre formulario web tras seleccionar lineas'
        Desc  = 'En TamdenZwcadPluging/ZwcadPlugin/Commands.cs:
- Tras seleccionar las lineas y antes de POST a /api/DesignToolsAutocad/DetectarEsquinasL, abrir navegador (System.Diagnostics.Process.Start) apuntando a la URL del formulario, pasando un id de sesion temporal (GUID) en query string.
- El servidor guarda en cache (HttpRuntime.Cache o similar) las lineas asociadas a ese GUID con TTL 5 min.
- Tras submit del formulario, el endpoint usa el GUID para recuperar las lineas + ConfiguracionMurosDTO y devuelve el DeteccionEsquinasLDTO.
- El plugin hace polling (cada 1s, max 5 min) al endpoint /api/.../EsperarResultado/{guid} hasta recibir el resultado y entonces dibuja en ZWCAD.

Alternativa mas simple si el flujo polling es complejo: dialog WPF/WinForms en el propio plugin con los mismos campos y enviar todo en una unica llamada POST. Decidir en T1 cual implementar.'
    },
    @{
        Title = 'T5: Pruebas integracion + documentacion'
        Desc  = 'Pruebas:
- Seleccionar 2 esquinas L con muro B -> cambiar altura a 3500 -> verificar Thickness=3500 en ZWCAD.
- Espesor opcional null -> autodetectar normalmente.
- Espesor opcional 200 -> forzar para muros E/F.
- Cancelar formulario -> no se dibuja nada.

Documentacion:
- Anadir Docs/Proyectos/Desing/US-XXX-Formulario-Configuracion-Muros.md con flujo y screenshots.
- Actualizar Docs/General/Arquitectura-Cliente-Servidor.md mencionando que el formulario es server-side (compartible con futuros clientes).'
    }
)

$taskIds = @()
foreach ($t in $tasks) {
    $body = @(
        @{ op = 'add'; path = '/fields/System.Title';       value = $t.Title },
        @{ op = 'add'; path = '/fields/System.Description'; value = $t.Desc  },
        @{
            op    = 'add'
            path  = '/relations/-'
            value = @{
                rel = 'System.LinkTypes.Hierarchy-Reverse'
                url = "$baseUri/$usId"
            }
        }
    )
    $r = Invoke-RestMethod -Uri "$baseUri/`$Task?api-version=7.1" -Method Post -Headers $h -Body (ConvertTo-Json $body -Depth 10)
    $taskIds += $r.id
    Write-Output ("  Task #{0}: {1}" -f $r.id, $t.Title)
}

# ---------- 3) Mover US a Active ----------
$patch = @( @{ op = 'add'; path = '/fields/System.State'; value = 'Active' } )
$r = Invoke-RestMethod -Uri "$baseUri/${usId}?api-version=7.1" -Method Patch -Headers $h -Body (ConvertTo-Json $patch -Depth 10)
Write-Output ("US #{0} -> {1}" -f $usId, $r.fields.'System.State')

Write-Output ''
Write-Output ('=== RESUMEN ===')
Write-Output ("US #{0}: {1}" -f $usId, $usTitle)
Write-Output ("Tasks creadas: {0}" -f ($taskIds -join ', '))
Write-Output ("URL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/{0}" -f $usId)

