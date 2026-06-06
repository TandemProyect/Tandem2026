$pat = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $pat))
$headers = @{
    Authorization  = 'Basic ' + $base64Auth
    'Content-Type' = 'application/json-patch+json'
}

$usId = 688

$title = 'T6: Muros A y D - Detectar muros rectos con una esquina L y un extremo libre'
$desc  = 'Detectar muros rectos donde UN extremo nace de una esquina L y el OTRO extremo es libre (sin conexion). Cubre los muros A, CC (espejo de A), D y Cara E (espejo de D) del documento Muro_Recto 1 esquina.png.

**Algoritmo:**
1. Para cada PanelInfoMuro (T5) y cada eje (H, V):
   a. Identificar extremo L (cerca de ptAzul/ptRojo) vs extremo libre (lejos)
   b. Verificar que el extremo libre inner Y el extremo libre outer NO conectan con ningun otro segmento del input (tolerancia ~ 1 mm)
   c. Verificar que la longitud del muro mas alla del panel supera un umbral minimo (p.ej. 600 mm) para descartar el brazo corto de la propia L
2. Si ambos checks pasan, construir rectangulo:
   - Eje H: [Verde, free_end_inner, free_end_outer, Blanco]
   - Eje V: [Amarillo, free_end_inner, free_end_outer, Cian]
3. Emitir 2 PolilineaDTO (patron US-679: ObjetoDB2d + ModelDesing 2700) + 4 cuadrados marcadores (T1).

**Archivos esperados:**
- Desing/Services/LCornerDetector.cs (metodo GenerarMurosLConExtremoLibre + helpers)
- Sin cambios en plugin (DTOs ya cubiertos en T5/T1)

**Predecesoras:** US-688 T5 (#693) y T1 (cuadrados, #689 cubierto en T5)
**Referencia:** C:\temp\Muro_Recto 1 esquina.png'

$taskBody = @(
    @{ op = 'add'; path = '/fields/System.Title'; value = $title },
    @{ op = 'add'; path = '/fields/System.Description'; value = $desc },
    @{
        op    = 'add'
        path  = '/relations/-'
        value = @{
            rel = 'System.LinkTypes.Hierarchy-Reverse'
            url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workItems/$usId"
        }
    }
)

$taskJson = ConvertTo-Json $taskBody -Depth 10
$taskResponse = Invoke-RestMethod -Uri 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$Task?api-version=7.1' -Method Post -Headers $headers -Body $taskJson
Write-Output ("Task #{0} creada bajo US #{1}" -f $taskResponse.id, $usId)
Write-Output ("URL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/{0}" -f $taskResponse.id)
