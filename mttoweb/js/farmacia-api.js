// Cliente delgado sobre fetch() para los endpoints de Farmacia.
// Misma API que mtto-api.js/cateter-api.js (mismo backend, mismo
// API_BASE_URL de mtto-config.js): son módulos del mismo proyecto de
// inventarios, no servidores distintos.
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

  window.FarmaciaApi = {
    inventario: {
      listar: (params) => solicitar("GET", "/farmacia" + queryString(params)),
      porCodigo: (codigo) => solicitar("GET", "/farmacia/buscar" + queryString({ codigo })),
      obtener: (id) => solicitar("GET", "/farmacia/" + id),
      guardar: (articulo) => solicitar("POST", "/farmacia", articulo),
      cambiarEstado: (id, activo) => solicitar("POST", `/farmacia/${id}/estado`, { activo }),
      eliminar: (id) => solicitar("DELETE", "/farmacia/" + id)
    },
    lotes: {
      listar: (params) => solicitar("GET", "/farmacia/lotes" + queryString(params)),
      porCaducar: (top) => solicitar("GET", "/farmacia/lotes/por-caducar" + queryString({ top })),
      guardar: (lote) => solicitar("POST", "/farmacia/lotes", lote)
    },
    movimientos: {
      listar: (params) => solicitar("GET", "/farmacia/movimientos" + queryString(params)),
      registrar: (movimiento) => solicitar("POST", "/farmacia/movimientos", movimiento),
      surtir: (entrada) => solicitar("POST", "/farmacia/movimientos/surtir", entrada)
    },
    catalogos: {
      todos: () => solicitar("GET", "/farmacia/catalogos")
    },
    // Etapa 1: las consultas leen el historial real del sistema anterior.
    // Las altas (guardar/dispensar/asignar) responden 501 hasta que existan
    // las tablas de captura; el mensaje del servidor explica por que.
    compras: {
      listar: (comNumero) => solicitar("GET", "/farmacia/compras" + queryString({ comNumero })),
      obtener: (comNumero) => solicitar("GET", "/farmacia/compras" + queryString({ comNumero })),
      guardar: (compra) => solicitar("POST", "/farmacia/compras", compra),
      recepcion: (entrada) => solicitar("POST", "/farmacia/compras/recepcion", entrada)
    },
    recetas: {
      listar: (params) => solicitar("GET", "/farmacia/recetas" + queryString(params)),
      obtener: (id) => solicitar("GET", "/farmacia/recetas/" + id),
      guardar: (receta) => solicitar("POST", "/farmacia/recetas", receta),
      dispensarLinea: (recetaDetalleId, entrada) =>
        solicitar("POST", `/farmacia/recetas/lineas/${recetaDetalleId}/dispensar`, entrada || {}),
      indicadorAbasto: (meses) => solicitar("GET", "/farmacia/recetas/indicador-abasto" + queryString({ meses }))
    },
    colectivos: {
      listar: (params) => solicitar("GET", "/farmacia/colectivos" + queryString(params)),
      obtener: (id) => solicitar("GET", "/farmacia/colectivos/" + id),
      guardar: (colectivo) => solicitar("POST", "/farmacia/colectivos", colectivo),
      servicios: (soloActivos) => solicitar("GET", "/farmacia/colectivos/servicios" + queryString({ soloActivos })),
      clavesDelServicio: (servicio) =>
        solicitar("GET", `/farmacia/colectivos/servicios/${encodeURIComponent(servicio)}/claves`),
      asignarClave: (entrada) => solicitar("POST", "/farmacia/colectivos/claves", entrada)
    },
    dashboard: {
      todo: (params) => {
        const p = { ...params };
        if (p.mesesHistorial !== undefined) {
          p.mesesHistorico = p.mesesHistorial;
          delete p.mesesHistorial;
        }
        return solicitar("GET", "/farmacia/dashboard" + queryString(p));
      },
      kpis: () => solicitar("GET", "/farmacia/dashboard/kpis")
    }
  };
})();
