// Cliente delgado sobre fetch() para los endpoints de la Clínica de Catéter.
// Misma API que mtto-api.js (mismo backend, mismo API_BASE_URL de
// mtto-config.js): son dos módulos del mismo proyecto de inventarios, no dos
// servidores distintos.
(function () {
  const BASE = window.MTTO_CONFIG.API_BASE_URL;

  async function solicitar(metodo, ruta, cuerpo) {
    const opciones = { method: metodo, headers: {} };
    if (cuerpo !== undefined) {
      opciones.headers["Content-Type"] = "application/json";
      opciones.body = JSON.stringify(cuerpo);
    }
    if (window.Auth && window.Auth.token()) {
      opciones.headers["Authorization"] = "Bearer " + window.Auth.token();
    }

    const respuesta = await fetch(BASE + ruta, opciones);

    if (respuesta.status === 401 && window.Auth) {
      window.Auth.logout();
      throw new Error("Sesión expirada. Inicia sesión de nuevo.");
    }

    const texto = await respuesta.text();
    const datos = texto ? JSON.parse(texto) : null;

    if (!respuesta.ok) {
      const mensaje = (datos && datos.error) || `Error HTTP ${respuesta.status}`;
      throw new Error(mensaje);
    }
    return datos;
  }

  function queryString(params) {
    if (!params) return "";
    const limpio = Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== "");
    if (limpio.length === 0) return "";
    return "?" + limpio.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`).join("&");
  }

  window.CateterApi = {
    inventario: {
      listar: (params) => solicitar("GET", "/cateter" + queryString(params)),
      // La clave lleva puntos (060.172.0493): nunca va en el path, IIS la
      // confundiría con una extensión de archivo. Siempre por query string.
      porClave: (clave) => solicitar("GET", "/cateter/buscar" + queryString({ clave })),
      obtener: (id) => solicitar("GET", "/cateter/" + id),
      guardar: (articulo) => solicitar("POST", "/cateter", articulo),
      cambiarEstado: (id, activo) => solicitar("POST", `/cateter/${id}/estado`, { activo }),
      eliminar: (id) => solicitar("DELETE", "/cateter/" + id)
    },
    lotes: {
      listar: (params) => solicitar("GET", "/cateter-lotes" + queryString(params)),
      porCaducar: (top) => solicitar("GET", "/cateter-lotes/por-caducar" + queryString({ top })),
      guardar: (lote) => solicitar("POST", "/cateter-lotes", lote)
    },
    movimientos: {
      listar: (params) => solicitar("GET", "/cateter-movimientos" + queryString(params)),
      registrar: (movimiento) => solicitar("POST", "/cateter-movimientos", movimiento),
      surtir: (entrada) => solicitar("POST", "/cateter-movimientos/surtir", entrada),
      // No borra el movimiento: lo marca como Cancelado y registra una
      // reversión enlazada, para que la bitácora siga siendo auditable.
      cancelar: (id, datos) => solicitar("POST", `/cateter-movimientos/${id}/cancelar`, datos || {})
    },
    terapiaInfusion: {
      guardarRegistro: (registro) => solicitar("POST", "/cateter-terapia-infusion/registros", registro),
      listarRegistros: (params) => solicitar("GET", "/cateter-terapia-infusion/registros" + queryString(params)),
      eliminarRegistro: (id) => solicitar("DELETE", `/cateter-terapia-infusion/registros/${id}`),
      listarEventos: (params) => solicitar("GET", "/cateter-terapia-infusion/eventos" + queryString(params)),
      guardarEvento: (evento) => solicitar("POST", "/cateter-terapia-infusion/eventos", evento),
      eliminarEvento: (id) => solicitar("DELETE", `/cateter-terapia-infusion/eventos/${id}`),
      reportes: (params) => solicitar("GET", "/cateter-terapia-infusion/reportes" + queryString(params)),
      // Descargas: van por navegación directa del navegador (no fetch), así
      // que el token viaja en la URL en vez de en el header Authorization.
      urlExportarRegistros: (params) =>
        BASE + "/cateter-terapia-infusion/registros/exportar" + queryString({ ...params, token: window.Auth && window.Auth.token() }),
      urlExportarEventos: (params) =>
        BASE + "/cateter-terapia-infusion/eventos/exportar" + queryString({ ...params, token: window.Auth && window.Auth.token() })
    },
    catalogos: {
      todos: () => solicitar("GET", "/cateter-catalogos"),
      ubicaciones: (incluirInactivas) =>
        solicitar("GET", "/cateter-catalogos/ubicaciones" + queryString({ incluirInactivas })),
      crearUbicacion: (ubicacion) => solicitar("POST", "/cateter-catalogos/ubicaciones", ubicacion),
      actualizarUbicacion: (id, ubicacion) => solicitar("PUT", `/cateter-catalogos/ubicaciones/${id}`, ubicacion),
      // No borra: apaga el registro. El id sigue colgando de movimientos y
      // existencias históricos, así que eliminarlo rompería la trazabilidad.
      desactivarUbicacion: (id) => solicitar("DELETE", `/cateter-catalogos/ubicaciones/${id}`),
      reactivarUbicacion: (id) => solicitar("POST", `/cateter-catalogos/ubicaciones/${id}/estado`, { activo: true })
    },
    dashboard: {
      todo: (params) => {
        const p = { ...params };
        if (p.mesesHistorial !== undefined) {
          p.mesesHistorico = p.mesesHistorial;
          delete p.mesesHistorial;
        }
        return solicitar("GET", "/cateter-dashboard" + queryString(p));
      },
      kpis: () => solicitar("GET", "/cateter-dashboard/kpis")
    },
    cuadroBasico: {
      buscar: (params) => solicitar("GET", "/cuadro-basico/buscar" + queryString(params)),
      agregar: (entrada) => solicitar("POST", "/cuadro-basico/agregar", entrada),
      descripcionesPropias: () => solicitar("GET", "/cuadro-basico/descripciones-propias"),
      guardarDescripcion: (clave, descripcionPropia) =>
        solicitar("PUT", "/cuadro-basico/descripcion-propia", { clave, descripcionPropia }),
      // Mandar la descripción vacía borra la propia y deja que vuelva a mandar
      // la oficial del CPM. La del cuadro básico nunca se toca.
      quitarDescripcion: (clave) =>
        solicitar("PUT", "/cuadro-basico/descripcion-propia", { clave, descripcionPropia: "" })
    }
  };
})();
