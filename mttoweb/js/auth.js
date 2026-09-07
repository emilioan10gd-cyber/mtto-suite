// Guardia de sesión + helpers compartidos por MTTO y Clínica de Catéter.
//
// El token vive en localStorage (no sessionStorage ni cookie): localStorage
// sobrevive a cerrar el navegador y reiniciar la PC, que es justo lo que se
// pidió ("que no se ande cerrando"). Una cookie hubiera necesitado credentials
// en el CORS, que hoy es Access-Control-Allow-Origin: * (no admite cookies).
//
// Debe cargarse ANTES que mtto-api.js / cateter-api.js en cada página (ambos
// leen Auth.token() para poner el header Authorization), y este script debe
// incluirse en TODA página protegida, justo después de mtto-config.js.
(function () {
  const CLAVE_TOKEN = "mtto_auth_token";
  const CLAVE_AREA = "mtto_auth_area";
  const CLAVE_USUARIO = "mtto_auth_usuario";
  const CLAVE_EXPIRA = "mtto_auth_expira";

  function guardar(sesion) {
    localStorage.setItem(CLAVE_TOKEN, sesion.token);
    localStorage.setItem(CLAVE_AREA, sesion.area);
    localStorage.setItem(CLAVE_USUARIO, sesion.nombreUsuario);
    localStorage.setItem(CLAVE_EXPIRA, sesion.expiraEn);
  }

  function limpiar() {
    [CLAVE_TOKEN, CLAVE_AREA, CLAVE_USUARIO, CLAVE_EXPIRA].forEach(k => localStorage.removeItem(k));
  }

  function token() { return localStorage.getItem(CLAVE_TOKEN); }
  function area() { return localStorage.getItem(CLAVE_AREA); }
  function nombreUsuario() { return localStorage.getItem(CLAVE_USUARIO); }

  /// Página de aterrizaje según el área: cada cuenta cae directo a SU módulo.
  function paginaInicio(areaUsuario) {
    if (areaUsuario === "cateter")  return "cateter-inventario.html";
    if (areaUsuario === "farmacia") return "farmacia-inventario.html";
    if (areaUsuario === "biomedico") return "biomedico-dashboard.html";
    if (areaUsuario === "almacen")  return "almacen-dashboard.html";
    return "index.html";
  }

  const NOMBRE_AREA = {
    mtto:     "Almacén de Mantenimiento",
    cateter:  "Clínica de Catéter",
    farmacia: "Farmacia",
    biomedico:"Biomédico",
    almacen:  "Almacén Central"
  };
  // Texto corto: el espacio del logo está pensado para 4-5 letras ("MTTO"),
  // no para el nombre completo del módulo.
  const MARCA = {
    mtto:     { texto: "MTTO",     mini: "M" },
    cateter:  { texto: "CATÉTER",  mini: "C" },
    farmacia: { texto: "FARMACIA", mini: "F" },
    biomedico:{ texto: "BIOMED",   mini: "B" },
    almacen:  { texto: "ALMACÉN",  mini: "A" }
  };

  /// Oculta del sidebar la sección del área que no es la de la sesión actual
  /// (los <li data-area="..."> se marcan en cada página) y llena el nombre
  /// de usuario/área en el perfil. Así "separar las áreas" no depende solo
  /// de la contraseña: el usuario de mtto nunca ve ni puede tocar un enlace
  /// a Clínica de Catéter, y viceversa.
  function aplicarVisibilidadPorArea(sesion) {
    document.querySelectorAll("[data-area]").forEach(el => {
      el.style.display = el.dataset.area === sesion.area ? "" : "none";
    });

    const nombreEl = document.getElementById("sidebarNombreArea");
    const designacionEl = document.getElementById("sidebarDesignacion");
    if (nombreEl) nombreEl.textContent = NOMBRE_AREA[sesion.area] || sesion.area;
    if (designacionEl) designacionEl.textContent = sesion.nombreUsuario;

    const perfilEl = document.querySelector(".nav-profile-name");
    if (perfilEl) perfilEl.textContent = sesion.nombreUsuario;

    // Logo/marca de arriba a la izquierda: debe decir a qué módulo entraste,
    // no siempre "MTTO" aunque estés en Clínica de Catéter. El link también
    // se ajusta para que llevar a CASA lleve a la página de inicio propia.
    const marca = MARCA[sesion.area] || MARCA.mtto;
    const inicio = paginaInicio(sesion.area);
    const textoEl = document.getElementById("brandLogoTexto");
    const textoMiniEl = document.getElementById("brandLogoTextoMini");
    const linkEl = document.getElementById("brandLogoLink");
    const linkMiniEl = document.getElementById("brandLogoLinkMini");
    if (textoEl) textoEl.textContent = marca.texto;
    if (textoMiniEl) textoMiniEl.textContent = marca.mini;
    if (linkEl) linkEl.setAttribute("href", inicio);
    if (linkMiniEl) linkMiniEl.setAttribute("href", inicio);
  }

  async function login(usuarioOCorreo, password) {
    const respuesta = await fetch(window.MTTO_CONFIG.API_BASE_URL + "/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ usuarioOCorreo, password })
    });
    const datos = await respuesta.json().catch(() => null);
    if (!respuesta.ok) throw new Error((datos && datos.error) || "No se pudo iniciar sesión.");
    guardar(datos);
    return datos;
  }

  async function logout() {
    const t = token();
    limpiar();
    if (t) {
      // Best-effort: si falla (sin red, servidor caído), la sesión local ya
      // se borró de todos modos, que es lo que le importa a quien cierra sesión.
      try {
        await fetch(window.MTTO_CONFIG.API_BASE_URL + "/auth/logout", {
          method: "POST",
          headers: { Authorization: "Bearer " + t }
        });
      } catch { /* ignorado a propósito */ }
    }
    window.location.href = "login.html";
  }

  /// Llamar al cargar cualquier página protegida. Si no hay token redirige a
  /// login; si el servidor lo rechaza (expiró, se revocó) también. De paso
  /// confirma el nombre en pantalla y renueva la sesión otros 90 días.
  async function exigirSesion(areaEsperada) {
    if (!token()) {
      window.location.href = "login.html";
      return null;
    }
    try {
      const respuesta = await fetch(window.MTTO_CONFIG.API_BASE_URL + "/auth/quien-soy", {
        headers: { Authorization: "Bearer " + token() }
      });
      if (!respuesta.ok) throw new Error("sesión inválida");
      const sesion = await respuesta.json();

      // Si el área no coincide con la de esta página, no se le muestra un 403
      // en seco: se le manda a su propio inicio, que es lo útil.
      if (areaEsperada && sesion.area !== areaEsperada) {
        window.location.href = paginaInicio(sesion.area);
        return null;
      }

      localStorage.setItem(CLAVE_EXPIRA, sesion.expiraEn);
      localStorage.setItem(CLAVE_USUARIO, sesion.nombreUsuario);
      aplicarVisibilidadPorArea(sesion);
      return sesion;
    } catch {
      limpiar();
      window.location.href = "login.html";
      return null;
    }
  }

  window.Auth = { login, logout, exigirSesion, token, area, nombreUsuario, paginaInicio };

  // El botón vive en el dropdown del perfil de cada página protegida; se
  // conecta aquí, una sola vez, para no repetir este listener en cada script
  // de página (login.html no tiene el botón, y el guardián no hace nada ahí).
  document.addEventListener("DOMContentLoaded", () => {
    const boton = document.getElementById("btnLogout");
    if (boton) boton.addEventListener("click", (ev) => { ev.preventDefault(); logout(); });
  });
})();
