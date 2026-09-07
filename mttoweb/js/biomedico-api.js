// Cliente delgado sobre fetch() para los endpoints de Biomédico.
// Misma API que mtto-api.js/cateter-api.js (mismo backend, mismo
// API_BASE_URL de mtto-config.js).
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

  // Subida de archivos (manuales): multipart/form-data, sin Content-Type
  // manual (el navegador pone el boundary solo).
  async function subirArchivo(ruta, archivo) {
    const formData = new FormData();
    formData.append("archivo", archivo, archivo.name);

    const opciones = { method: "POST", headers: {}, body: formData };
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

  function urlDescarga(ruta) {
    // Para <a href> directo (descarga de manual): el token va en query string
    // porque un link normal no puede mandar el header Authorization.
    const token = window.Auth ? window.Auth.token() : null;
    return BASE + ruta + queryString({ token });
  }

  window.BiomedicoApi = {
    equipos: {
      listar: (params) => solicitar("GET", "/biomedico" + queryString(params)),
      bajas: (params) => solicitar("GET", "/biomedico/bajas" + queryString(params)),
      obtener: (id) => solicitar("GET", "/biomedico/" + id),
      guardar: (equipo) => solicitar("POST", "/biomedico", equipo),
      cambiarEstado: (id, estado, motivoBaja) =>
        solicitar("POST", `/biomedico/${id}/estado`, { estado, motivoBaja }),
      eliminar: (id) => solicitar("DELETE", "/biomedico/" + id),
      exportarUrl: (params) => BASE + "/biomedico/exportar" + queryString(params)
    },
    catalogos: {
      todos: () => solicitar("GET", "/biomedico-catalogos"),
      areas: () => solicitar("GET", "/biomedico-catalogos/areas"),
      categoriasEquipo: () => solicitar("GET", "/biomedico-catalogos/categorias-equipo"),
      categoriasInsumo: () => solicitar("GET", "/biomedico-catalogos/categorias-insumo"),
      estadosEquipo: () => solicitar("GET", "/biomedico-catalogos/estados-equipo"),
      crearArea: (nombre) => solicitar("POST", "/biomedico-catalogos/areas", { nombre }),
      crearCategoriaEquipo: (nombre) => solicitar("POST", "/biomedico-catalogos/categorias-equipo", { nombre }),
      crearCategoriaInsumo: (nombre) => solicitar("POST", "/biomedico-catalogos/categorias-insumo", { nombre })
    },
    insumos: {
      listar: (params) => solicitar("GET", "/biomedico-insumos" + queryString(params)),
      obtener: (id) => solicitar("GET", "/biomedico-insumos/" + id),
      guardar: (insumo) => solicitar("POST", "/biomedico-insumos", insumo),
      lotes: (id, params) => solicitar("GET", `/biomedico-insumos/${id}/lotes` + queryString(params)),
      porVencer: (dias) => solicitar("GET", "/biomedico-insumos/lotes/por-vencer" + queryString({ dias }))
    },
    movimientos: {
      registrar: (movimiento) => solicitar("POST", "/biomedico-movimientos", movimiento)
    },
    mantenimiento: {
      listar: (params) => solicitar("GET", "/biomedico-mantenimiento" + queryString(params)),
      obtener: (id) => solicitar("GET", "/biomedico-mantenimiento/" + id),
      programar: (mtto) => solicitar("POST", "/biomedico-mantenimiento", mtto),
      marcarRealizado: (id, datos) => solicitar("POST", `/biomedico-mantenimiento/${id}/realizado`, datos || {}),
      cancelar: (id) => solicitar("POST", `/biomedico-mantenimiento/${id}/cancelar`)
    },
    manuales: {
      listar: (equipoId) => solicitar("GET", "/biomedico-manuales/equipo/" + equipoId),
      subir: (equipoId, archivo) => subirArchivo("/biomedico-manuales/equipo/" + equipoId, archivo),
      descargarUrl: (id) => urlDescarga(`/biomedico-manuales/${id}/descargar`),
      eliminar: (id) => solicitar("DELETE", "/biomedico-manuales/" + id)
    },
    fallas: {
      listar: (equipoId) => solicitar("GET", "/biomedico-fallas/equipo/" + equipoId),
      reportar: (falla) => solicitar("POST", "/biomedico-fallas", falla),
      resolver: (id, causa, solucion) => solicitar("POST", `/biomedico-fallas/${id}/resolver`, { causa, solucion }),
      sugerencias: (equipoId, sintoma) =>
        solicitar("GET", "/biomedico-fallas/sugerencias" + queryString({ equipoId, sintoma }))
    },
    dashboard: {
      todo: () => solicitar("GET", "/biomedico-dashboard")
    }
  };
})();
