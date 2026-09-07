// Configuración de entorno. Cambiar API_BASE_URL al desplegar en el hosting.
//
// Elige http o https según cómo se cargó ESTA página: si mttoweb se abrió
// por https (necesario para que el navegador deje usar la cámara — ver
// biomedico-escanear.html), pegarle a la API por http rompería con "mixed
// content" (el navegador bloquea llamadas http desde una página https).
// Los visitantes que sigan entrando por http seguro no ven ningún cambio.
window.MTTO_CONFIG = {
  API_BASE_URL: location.hostname.endsWith("trycloudflare.com")
    ? "https://survey-der-albuquerque-apnic.trycloudflare.com/api"
    : location.protocol === "https:"
    ? "https://servidor-hgr.hgreynosa.com:8444/api"
    : "http://172.28.45.2:8081/api"
};
