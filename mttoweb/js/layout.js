// Navbar + sidebar compartidos por todas las páginas. Antes cada página traía
// su propia copia pegada del menú completo (~200 líneas), y agregar un enlace
// nuevo significaba tocar 30+ archivos a mano — así se nos olvidó "Terapia de
// Infusión" en varias páginas la primera vez. Ahora se agrega aquí una sola
// vez y aparece en todos lados.
//
// Debe cargarse justo después de mtto-config.js y ANTES de auth.js: Layout.inyectar()
// tiene que insertar los <li data-area="..."> en el DOM antes de que
// Auth.exigirSesion() llegue a aplicarVisibilidadPorArea(), que es quien los
// muestra/oculta según el área de la sesión.
(function () {
  const NAVBAR_HTML = `
    <nav class="navbar col-lg-12 col-12 p-0 fixed-top d-flex flex-row">
      <div class="text-center navbar-brand-wrapper d-flex align-items-center justify-content-center">
        <a class="navbar-brand brand-logo" href="index.html" id="brandLogoLink"><span class="mtto-brand" id="brandLogoTexto">MTTO</span></a>
        <a class="navbar-brand brand-logo-mini" href="index.html" id="brandLogoLinkMini"><span class="mtto-brand" id="brandLogoTextoMini">M</span></a>
        <button class="navbar-toggler navbar-toggler align-self-center d-none d-lg-flex" type="button" data-toggle="minimize">
          <span class="typcn typcn-th-menu"></span>
        </button>
      </div>
      <div class="navbar-menu-wrapper d-flex align-items-center justify-content-end">
        <ul class="navbar-nav navbar-nav-right">
          <li class="nav-item nav-profile dropdown">
            <a class="nav-link dropdown-toggle pl-0 pr-0" href="#" data-toggle="dropdown" id="profileDropdown">
              <i class="typcn typcn-user-outline mr-1"></i>
              <span class="nav-profile-name">Clínica de Catéter</span>
            </a>
            <div class="dropdown-menu dropdown-menu-right navbar-dropdown" aria-labelledby="profileDropdown">
              <a class="dropdown-item disabled">
                <i class="typcn typcn-cog text-primary"></i>
                Configuración (próximamente)
              </a>
              <a class="dropdown-item" href="#" id="btnLogout">
                <i class="typcn typcn-power-outline text-danger"></i>
                Cerrar sesión
              </a>
            </div>
          </li>
        </ul>
        <button class="navbar-toggler navbar-toggler-right d-lg-none align-self-center" type="button" data-toggle="offcanvas">
          <span class="typcn typcn-th-menu"></span>
        </button>
      </div>
    </nav>`;

  const SIDEBAR_HTML = `
    <nav class="sidebar sidebar-offcanvas" id="sidebar">
      <ul class="nav">
        <li class="nav-item">
          <div class="d-flex sidebar-profile">
            <div class="sidebar-profile-name">
              <p class="sidebar-name" id="sidebarNombreArea">Panel operativo</p>
              <p class="sidebar-designation" id="sidebarDesignacion"></p>
            </div>
          </div>
          <p class="sidebar-menu-title">Menú</p>
        </li>
        <li class="nav-item" data-area="mtto">
          <a class="nav-link" href="index.html">
            <i class="typcn typcn-device-desktop menu-icon"></i>
            <span class="menu-title">Dashboard</span>
          </a>
        </li>
        <li class="nav-item" data-area="mtto">
          <a class="nav-link" href="inventario.html">
            <i class="typcn typcn-archive menu-icon"></i>
            <span class="menu-title">Inventario</span>
          </a>
        </li>
        <li class="nav-item" data-area="mtto">
          <a class="nav-link" href="movimientos.html">
            <i class="typcn typcn-arrow-repeat menu-icon"></i>
            <span class="menu-title">Movimientos</span>
          </a>
        </li>
        <li class="nav-item" data-area="mtto">
          <a class="nav-link" href="catalogos.html">
            <i class="typcn typcn-th-list menu-icon"></i>
            <span class="menu-title">Catálogos</span>
          </a>
        </li>
        <li class="nav-item" data-area="cateter">
          <p class="sidebar-menu-title">Clínica de Catéter</p>
        </li>
        <li class="nav-item" data-area="cateter">
          <a class="nav-link" href="cateter-dashboard.html">
            <i class="typcn typcn-device-desktop menu-icon"></i>
            <span class="menu-title">Dashboard</span>
          </a>
        </li>
        <li class="nav-item" data-area="cateter">
          <a class="nav-link" href="cateter-inventario.html">
            <i class="typcn typcn-heart-outline menu-icon"></i>
            <span class="menu-title">Inventario</span>
          </a>
        </li>
        <li class="nav-item" data-area="cateter">
          <a class="nav-link" href="cateter-movimientos.html">
            <i class="typcn typcn-arrow-repeat menu-icon"></i>
            <span class="menu-title">Movimientos</span>
          </a>
        </li>
        <li class="nav-item" data-area="cateter">
          <a class="nav-link" href="cateter-terapia-infusion.html">
            <i class="typcn typcn-beaker menu-icon"></i>
            <span class="menu-title">Terapia de Infusión</span>
          </a>
        </li>
        <li class="nav-item" data-area="cateter">
          <a class="nav-link" href="cateter-cuadro-basico.html">
            <i class="typcn typcn-zoom menu-icon"></i>
            <span class="menu-title">Cuadro básico</span>
          </a>
        </li>
        <li class="nav-item" data-area="cateter">
          <a class="nav-link" href="cateter-catalogos.html">
            <i class="typcn typcn-th-list menu-icon"></i>
            <span class="menu-title">Catálogos</span>
          </a>
        </li>
        <li class="nav-item" data-area="farmacia">
          <p class="sidebar-menu-title">Farmacia</p>
        </li>
        <li class="nav-item" data-area="farmacia">
          <a class="nav-link" href="farmacia-dashboard.html">
            <i class="typcn typcn-device-desktop menu-icon"></i>
            <span class="menu-title">Dashboard</span>
          </a>
        </li>
        <li class="nav-item" data-area="farmacia">
          <a class="nav-link" href="farmacia-inventario.html">
            <i class="typcn typcn-plus menu-icon"></i>
            <span class="menu-title">Inventario</span>
          </a>
        </li>
        <li class="nav-item" data-area="farmacia">
          <a class="nav-link" href="farmacia-movimientos.html">
            <i class="typcn typcn-arrow-repeat menu-icon"></i>
            <span class="menu-title">Movimientos</span>
          </a>
        </li>
        <li class="nav-item" data-area="farmacia">
          <a class="nav-link" href="farmacia-recepcion.html">
            <i class="typcn typcn-input-checked menu-icon"></i>
            <span class="menu-title">Recepción</span>
          </a>
        </li>
        <li class="nav-item" data-area="farmacia">
          <a class="nav-link" href="farmacia-recetas.html">
            <i class="typcn typcn-clipboard menu-icon"></i>
            <span class="menu-title">Recetas</span>
          </a>
        </li>
        <li class="nav-item" data-area="farmacia">
          <a class="nav-link" href="farmacia-colectivos.html">
            <i class="typcn typcn-group menu-icon"></i>
            <span class="menu-title">Colectivos</span>
          </a>
        </li>
        <li class="nav-item" data-area="farmacia">
          <a class="nav-link" href="farmacia-catalogos.html">
            <i class="typcn typcn-th-list menu-icon"></i>
            <span class="menu-title">Catálogos</span>
          </a>
        </li>
        <li class="nav-item" data-area="biomedico">
          <p class="sidebar-menu-title">Biomédico</p>
        </li>
        <li class="nav-item" data-area="biomedico">
          <a class="nav-link" href="biomedico-dashboard.html">
            <i class="typcn typcn-device-desktop menu-icon"></i>
            <span class="menu-title">Dashboard</span>
          </a>
        </li>
        <li class="nav-item" data-area="biomedico">
          <a class="nav-link" href="biomedico-equipos.html">
            <i class="typcn typcn-heart-outline menu-icon"></i>
            <span class="menu-title">Equipos</span>
          </a>
        </li>
        <li class="nav-item" data-area="biomedico">
          <a class="nav-link" href="biomedico-bajas.html">
            <i class="typcn typcn-warning-outline menu-icon"></i>
            <span class="menu-title">Bajas</span>
          </a>
        </li>
        <li class="nav-item" data-area="biomedico">
          <a class="nav-link" href="biomedico-insumos.html">
            <i class="typcn typcn-beaker menu-icon"></i>
            <span class="menu-title">Insumos</span>
          </a>
        </li>
        <li class="nav-item" data-area="biomedico">
          <a class="nav-link" href="biomedico-calendario.html">
            <i class="typcn typcn-calendar-outline menu-icon"></i>
            <span class="menu-title">Calendario</span>
          </a>
        </li>
        <li class="nav-item" data-area="biomedico">
          <a class="nav-link" href="biomedico-etiquetas.html">
            <i class="typcn typcn-tags menu-icon"></i>
            <span class="menu-title">Etiquetas</span>
          </a>
        </li>
        <li class="nav-item" data-area="biomedico">
          <a class="nav-link" href="biomedico-escanear.html">
            <i class="typcn typcn-camera-outline menu-icon"></i>
            <span class="menu-title">Escanear</span>
          </a>
        </li>
        <li class="nav-item" data-area="biomedico">
          <a class="nav-link" href="biomedico-catalogos.html">
            <i class="typcn typcn-th-list menu-icon"></i>
            <span class="menu-title">Catálogos</span>
          </a>
        </li>
        <li class="nav-item" data-area="almacen">
          <p class="sidebar-menu-title">Almac&eacute;n Central</p>
        </li>
        <li class="nav-item" data-area="almacen">
          <a class="nav-link" href="almacen-dashboard.html">
            <i class="typcn typcn-device-desktop menu-icon"></i>
            <span class="menu-title">Dashboard</span>
          </a>
        </li>
        <li class="nav-item" data-area="almacen">
          <a class="nav-link" href="almacen-inventario.html">
            <i class="typcn typcn-archive menu-icon"></i>
            <span class="menu-title">Inventario</span>
          </a>
        </li>
        <li class="nav-item" data-area="almacen">
          <a class="nav-link" href="almacen-movimientos.html">
            <i class="typcn typcn-shopping-cart menu-icon"></i>
            <span class="menu-title">Salida</span>
          </a>
        </li>
        <li class="nav-item" data-area="almacen">
          <a class="nav-link" href="almacen-recepcion.html">
            <i class="typcn typcn-download-outline menu-icon"></i>
            <span class="menu-title">Entrada</span>
          </a>
        </li>
        <li class="nav-item" data-area="almacen">
          <a class="nav-link" href="almacen-etiquetas.html">
            <i class="typcn typcn-tags menu-icon"></i>
            <span class="menu-title">Etiquetas</span>
          </a>
        </li>
        <li class="nav-item" data-area="almacen">
          <a class="nav-link" href="almacen-historial.html">
            <i class="typcn typcn-time menu-icon"></i>
            <span class="menu-title">Historial</span>
          </a>
        </li>
        <li class="nav-item" data-area="almacen">
          <a class="nav-link" href="almacen-escanear.html">
            <i class="typcn typcn-camera-outline menu-icon"></i>
            <span class="menu-title">Escanear</span>
          </a>
        </li>
        <li class="nav-item" data-area="almacen">
          <a class="nav-link" href="almacen-catalogos.html">
            <i class="typcn typcn-th-list menu-icon"></i>
            <span class="menu-title">Cat&aacute;logos</span>
          </a>
        </li>
      </ul>
    </nav>`;

  /// Marca como activo el <li> cuyo enlace apunta a la página actual,
  /// comparando solo el nombre de archivo (ignora query string y ancla).
  function marcarPaginaActiva(sidebarEl) {
    const archivoActual = window.location.pathname.split("/").pop() || "index.html";
    sidebarEl.querySelectorAll("a.nav-link[href]").forEach(enlace => {
      const archivoEnlace = enlace.getAttribute("href").split("?")[0].split("#")[0];
      if (archivoEnlace === archivoActual) {
        enlace.closest("li.nav-item").classList.add("active");
      }
    });
  }

  function inyectar() {
    const navbarDestino = document.getElementById("navbarInyectado");
    const sidebarDestino = document.getElementById("sidebarInyectado");
    if (navbarDestino) navbarDestino.outerHTML = NAVBAR_HTML;
    if (sidebarDestino) {
      sidebarDestino.outerHTML = SIDEBAR_HTML;
      marcarPaginaActiva(document.getElementById("sidebar"));
    }
  }

  window.Layout = { inyectar };
})();
