$pat = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $pat))
$headers = @{
    Authorization  = 'Basic ' + $base64Auth
    'Content-Type' = 'application/json-patch+json'
}

$usId = 688

$title = 'T5: Muros B y C - Detectar tramos rectos entre dos esquinas L adyacentes'
$desc  = 'Detectar pares de esquinas L cuyos puntos Verde/Blanco estan alineados (comparten eje horizontal o vertical) y generar una polilinea rectangular cerrada por cada tramo recto.

**Alcance:**
- Detectar pares de esquinas L adyacentes (colineales en el mismo eje)
- Por cada par, construir rectangulo con los 4 puntos existentes: Verde_izq, Blanco_izq, Blanco_der, Verde_der (o equivalente segun orientacion)
- Emitir 2 PolilineaDTO por muro (patron US-679): 
   - Capa ObjetoDB2d, AlturaExtrusion=0
   - Capa ModelDesing, AlturaExtrusion=2700
- NO implementa extremos libres (eso es T2 - muros A/D)
- NO implementa muros sin esquina (eso es T3 - muros E/F)

**Archivos esperados:**
- Desing/Services/LCornerDetector.cs (metodo DetectarMurosEntreEsquinas)
- Sin cambios en Commands.cs (ya dibuja polilineas genericas)

**Referencia:** C:\temp\Muro_Recto3.png - Muro B (21/6/7/5) y Muro C (8/10/11/9)'

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
