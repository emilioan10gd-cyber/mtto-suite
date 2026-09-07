# ============================================================================
# Habilita HTTPS para mtto (API) y mttoweb (frontend) con un certificado
# autofirmado, sin tocar los bindings http que ya funcionan (8081/8082).
#
# CORRER EN EL SERVIDOR (172.28.45.2), COMO ADMINISTRADOR.
# No se puede correr en remoto desde esta sesión: hace falta elevación local.
#
# Qué hace:
#   1. Genera un certificado autofirmado para servidor-hgr.hgreynosa.com y
#      la IP 172.28.45.2 (para que funcione entrando por cualquiera de los 2).
#   2. Abre el puerto en el firewall de Windows.
#   3. Liga el certificado al puerto con netsh http add sslcert.
#   4. Agrega el binding https al sitio de IIS correspondiente.
#
# Es seguro volver a correrlo: cada paso revisa si ya existe antes de crear.
# ============================================================================

$ErrorActionPreference = "Stop"

$hostnames = @("servidor-hgr.hgreynosa.com", "172.28.45.2")
$puertoWeb = 8443   # mttoweb (frontend)
$puertoApi = 8444   # mtto (API)
$nombreCert = "mtto-biomedico-selfsigned"

Write-Host "=== 1. Certificado ===" -ForegroundColor Cyan
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.FriendlyName -eq $nombreCert } | Select-Object -First 1
if (-not $cert) {
    $cert = New-SelfSignedCertificate `
        -DnsName $hostnames `
        -CertStoreLocation "cert:\LocalMachine\My" `
        -FriendlyName $nombreCert `
        -NotAfter (Get-Date).AddYears(5) `
        -KeyExportPolicy Exportable `
        -KeyUsage DigitalSignature, KeyEncipherment `
        -Type SSLServerAuthentication
    Write-Host "Certificado nuevo creado. Huella: $($cert.Thumbprint)"
} else {
    Write-Host "Certificado ya existía. Huella: $($cert.Thumbprint)"
}
$huella = $cert.Thumbprint

Write-Host "`n=== 2. Firewall ===" -ForegroundColor Cyan
foreach ($puerto in @($puertoWeb, $puertoApi)) {
    $regla = "mtto-biomedico-https-$puerto"
    if (-not (Get-NetFirewallRule -DisplayName $regla -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -DisplayName $regla -Direction Inbound -Protocol TCP -LocalPort $puerto -Action Allow | Out-Null
        Write-Host "Regla de firewall creada para el puerto $puerto"
    } else {
        Write-Host "Regla de firewall del puerto $puerto ya existía"
    }
}

Write-Host "`n=== 3. Ligar certificado a los puertos (netsh) ===" -ForegroundColor Cyan
foreach ($puerto in @($puertoWeb, $puertoApi)) {
    $existe = netsh http show sslcert ipport="0.0.0.0:$puerto" 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $existe) {
        $appid = [guid]::NewGuid().ToString("B")
        netsh http add sslcert ipport="0.0.0.0:$puerto" certhash=$huella appid=$appid certstorename=MY | Out-Null
        Write-Host "Certificado ligado al puerto $puerto"
    } else {
        Write-Host "El puerto $puerto ya tenía un certificado ligado (se deja como está)"
    }
}

Write-Host "`n=== 4. Bindings de IIS ===" -ForegroundColor Cyan
Import-Module WebAdministration

function Agregar-BindingHttps($sitio, $puerto) {
    $yaExiste = Get-WebBinding -Name $sitio -Protocol https -Port $puerto -ErrorAction SilentlyContinue
    if (-not $yaExiste) {
        New-WebBinding -Name $sitio -Protocol https -Port $puerto -IPAddress "*"
        Write-Host "Binding https agregado a '$sitio' en el puerto $puerto"
    } else {
        Write-Host "'$sitio' ya tenía binding https en el puerto $puerto"
    }
}

Agregar-BindingHttps -sitio "mttoweb" -puerto $puertoWeb
Agregar-BindingHttps -sitio "mtto" -puerto $puertoApi

Write-Host "`n=== Listo ===" -ForegroundColor Green
Write-Host "Frontend:  https://servidor-hgr.hgreynosa.com:$puertoWeb/login.html"
Write-Host "API:       https://servidor-hgr.hgreynosa.com:$puertoApi/api/cateter-catalogos/ubicaciones  (debe dar 401, no error de conexión)"
Write-Host ""
Write-Host "La PRIMERA vez que un navegador (compu o celular) entre, va a mostrar" -ForegroundColor Yellow
Write-Host "una advertencia de 'conexion no privada' porque el certificado es" -ForegroundColor Yellow
Write-Host "autofirmado. Hay que darle 'Avanzado' -> 'Continuar' UNA vez por" -ForegroundColor Yellow
Write-Host "dispositivo; despues de eso ya no vuelve a salir en ese navegador." -ForegroundColor Yellow
