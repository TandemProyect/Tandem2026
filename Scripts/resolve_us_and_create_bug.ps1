$pat = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $pat))
$headersJsonPatch = @{
    Authorization  = 'Basic ' + $base64Auth
    'Content-Type' = 'application/json-patch+json'
}

$org = 'VSCAD'
$proj = 'tandem2026'
$baseUri = "https://dev.azure.com/$org/$proj/_apis/wit/workitems"

# ---- 1) Mover US #688 a Resolved ----
$usPatch = @(
    @{ op = 'add'; path = '/fields/System.State'; value = 'Resolved' }
)
$json = ConvertTo-Json $usPatch -Depth 10
try {
    $resp = Invoke-RestMethod -Uri "$baseUri/688?api-version=7.1" -Method Patch -Headers $headersJsonPatch -Body $json
    Write-Output ("US #688 -> {0}" -f $resp.fields.'System.State')
} catch {
    Write-Output ("ERROR moviendo US: " + $_.Exception.Message)
}

# Mover tasks 693, 689, 694, 695 a Closed
foreach ($id in 693, 689, 694, 695) {
    $patch = @( @{ op = 'add'; path = '/fields/System.State'; value = 'Closed' } )
    $j = ConvertTo-Json $patch -Depth 10
    try {
        $r = Invoke-RestMethod -Uri "$baseUri/$id?api-version=7.1" -Method Patch -Headers $headersJsonPatch -Body $j
        Write-Output ("Task #$id -> {0}" -f $r.fields.'System.State')
    } catch {
        Write-Output ("ERROR Task #${id}: " + $_.Exception.Message)
    }
}

# ---- 2) Crear Bug ----
$bugTitle = 'Muro tipo CC se dibuja en posicion/orientacion incorrecta en vista 3D'
$bugRepro = '**Pasos para reproducir:**
1. Dibujar varios muros con esquinas L (incluyendo configuracion tipo CC: L a la derecha + extension hacia la izquierda hasta extremo libre).
2. Ejecutar TANDEM_SELECCIONAR_LINEAS sobre todas las lineas del dibujo.
3. Cambiar a vista 3D para ver las extrusiones de la capa ModelDesing.

**Resultado actual:**
Algunos muros tipo CC (espejo de muro A) aparecen con un trozo extra dibujado en posicion / orientacion erronea. Se ve una pieza que se sale del rectangulo correcto del muro. Ver imagen de referencia: c:\temp\bug_tipo_Muro_cc.png (zona marcada como "Error en Muro").

**Resultado esperado:**
El rectangulo del muro CC deberia ser identico al muro A pero reflejado: 4 vertices alineados perfectamente con las dos lineas paralelas del muro y los puntos Verde/Blanco (o Amarillo/Cian) de la esquina L.

**Sospecha de causa:**
Posible error en la deteccion del extremo libre cuando la esquina L esta a la DERECHA y la extension va hacia la IZQUIERDA (o ARRIBA->ABAJO). El metodo ExtremoLejano puede estar devolviendo el endpoint equivocado, o el orden de los vertices del rectangulo puede estar invertido para configuraciones espejo. Revisar GenerarMurosLConExtremoLibre en LCornerDetector.cs.

**Archivos relevantes:**
- Desing/Services/LCornerDetector.cs (GenerarMurosLConExtremoLibre, ExtremoLejano)

**Origen:** US-688 T6 (#694) muros A/D con extremo libre.'

$bugBody = @(
    @{ op = 'add'; path = '/fields/System.Title';                          value = $bugTitle },
    @{ op = 'add'; path = '/fields/Microsoft.VSTS.TCM.ReproSteps';         value = $bugRepro },
    @{ op = 'add'; path = '/fields/Microsoft.VSTS.Common.Severity';        value = '3 - Medium' }
)
$bugJson = ConvertTo-Json $bugBody -Depth 10
$bugResp = Invoke-RestMethod -Uri "$baseUri/`$Bug?api-version=7.1" -Method Post -Headers $headersJsonPatch -Body $bugJson
Write-Output ("Bug #{0} creado: {1}" -f $bugResp.id, $bugTitle)
Write-Output ("URL: https://dev.azure.com/$org/$proj/_workitems/edit/{0}" -f $bugResp.id)
