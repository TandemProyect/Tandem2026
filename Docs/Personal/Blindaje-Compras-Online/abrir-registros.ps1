# Abre en el navegador las páginas de registro / referencia del blindaje de compras.
# Uso: powershell -File ".\abrir-registros.ps1"

$urls = @(
    "https://www.trustedshops.es/",
    "https://www.trustedshops.es/proteccion-al-comprador/",
    "https://www.escrow.com/signup-page",
    "https://www.escrow.com/fee-calculator",
    "https://www.escrow.com/support/identity-verification",
    "https://www.visa.es/paga-con-visa/seguridad-en-tus-pagos/retroceso-de-cargo.html"
)

Write-Host "Abriendo $($urls.Count) URLs para montar el blindaje de compras..." -ForegroundColor Cyan
foreach ($url in $urls) {
    Write-Host "  -> $url"
    Start-Process $url
    Start-Sleep -Milliseconds 400
}

Write-Host ""
Write-Host "Siguiente: registrate en Trusted Shops y Escrow, luego marca casillas en 04-PUESTA-EN-MARCHA.md" -ForegroundColor Green
Write-Host "Guia: 07-ALTA-ESCROW-Y-TRUSTED.md | Caso 50 EUR: 06-CASO-50-EUR.md"
