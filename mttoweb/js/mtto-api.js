// Cliente delgado sobre fetch() para la API de mtto.
// No usa jQuery a propósito: el bundle base de la plantilla ya carga jQuery
// para los plugins de UI, pero las llamadas a datos van directo con fetch.
(function () {
  const BASE = window.MTTO_CONFIG.API_BASE_URL;

  async function solicitar(metodo, ruta, cuerpo) {
    const opciones = { method: metodo, headers: {} };
    if (cuerpo !== undefined) {
      opciones.headers["Content-Type"] = "application/json";
      opciones.body = JSON.stringify(cuerpo);
    }

    const respuesta = await fetch(BASE + ruta, opciones);
    const texto = await respuesta.text();
    const datos = texto ? JSON.parse(texto) : null;

    if (!respuesta.ok) {
      const mensaje = (datos && datos.error) || `Error HTTP ${respuesta.status}`;
      throw new Error(mensaje);
    }
    return datos;
  }

  window.MttoApi = {
    dashboard: {
      todo: (params) => solicitar("GET", "/dashboard" + queryString(params)),
      kpis: () => solicitar("GET", "/dashboard/kpis"),
      porCategoria: () => solicitar("GET", "/dashboard/por-categoria"),
      porEstado: () => solicitar("GET", "/dashboard/por-estado"),
      porMes: (meses) => solicitar("GET", "/dashboard/por-mes" + queryString({ meses })),
      bajoStock: (top) => solicitar("GET", "/dashboard/bajo-stock" + queryString({ top }))
    },
    inventario: {
      listar: (params) => solicitar("GET", "/inventario" + queryString(params)),
      obtener: (codigo) => solicitar("GET", "/inventario/" + encodeURIComponent(codigo)),
      guardar: (articulo) => solicitar("POST", "/inventario", articulo),
      actualizar: (codigo, articulo) => solicitar("PUT", "/inventario/" + encodeURIComponent(codigo), articulo),
      eliminar: (codigo) => solicitar("DELETE", "/inventario/" + encodeURIComponent(codigo))
    },
    movimientos: {
      listar: (params) => solicitar("GET", "/movimientos" + queryString(params)),
      registrar: (movimiento) => solicitar("POST", "/movimientos", movimiento)
    },
    catalogos: {
      todos: () => solicitar("GET", "/catalogos")
    },
    // CRUD de catálogos (categorías, almacenes, proveedores, unidades). Nunca
    // hay "eliminar" a propósito: cambiarEstado(activo:false) da de baja sin
    // romper el historial de artículos/movimientos que los referencian.
    catalogosAdmin: {
      listar: (tipo) => solicitar("GET", `/catalogos-admin/${tipo}`),
      crear: (tipo, datos) => solicitar("POST", `/catalogos-admin/${tipo}`, datos),
      actualizar: (tipo, id, datos) => solicitar("PUT", `/catalogos-admin/${tipo}/${id}`, datos),
      cambiarEstado: (tipo, id, activo) => solicitar("POST", `/catalogos-admin/${tipo}/${id}/estado`, { activo })
    },
    precios: {
      porArticulo: (codigo, cantidad) => {
        const url = cantidad ? `/precios/por-articulo/${encodeURIComponent(codigo)}?cantidad=${cantidad}`
                             : `/precios/por-articulo/${encodeURIComponent(codigo)}`;
        return solicitar("GET", url);
      },
      crear: (datos) => solicitar("POST", "/precios", datos),
      actualizar: (id, datos) => solicitar("PUT", `/precios/${id}`, datos),
      eliminar: (id) => solicitar("DELETE", `/precios/${id}`)
    }
  };

  function queryString(params) {
    if (!params) return "";
    const limpio = Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== "");
    if (limpio.length === 0) return "";
    return "?" + limpio.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`).join("&");
  }
})();
