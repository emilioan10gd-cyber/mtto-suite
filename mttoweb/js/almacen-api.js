// Cliente HTTP para el módulo Almacén Central de Material de Curación.
(function () {
  const BASE = window.MTTO_CONFIG.API_BASE_URL;

  async function solicitar(metodo, ruta, cuerpo) {
    const opciones = { method: metodo, headers: {} };
    if (cuerpo !== undefined) {
      opciones.headers["Content-Type"] = "application/json";
      opciones.body = JSON.stringify(cuerpo);
    }
    if (window.Auth && window.Auth.token())
      opciones.headers["Authorization"] = "Bearer " + window.Auth.token();

    const respuesta = await fetch(BASE + ruta, opciones);
    if (respuesta.status === 401 && window.Auth) {
      window.Auth.logout();
      throw new Error("Sesión expirada.");
    }
    const texto = await respuesta.text();
    const datos = texto ? JSON.parse(texto) : null;
    if (!respuesta.ok) {
      // El endpoint de caja devuelve { errores: [...] } con el detalle por
      // renglon; el resto devuelve { error: "..." }. Se conserva el cuerpo
      // completo en el Error para que quien llama pueda mostrarlo entero.
      const detalle = (datos && datos.errores && datos.errores.length)
        ? datos.errores.join(" · ")
        : (datos && datos.error);
      const err = new Error(detalle || `Error HTTP ${respuesta.status}`);
      err.status = respuesta.status;
      err.datos  = datos;
      throw err;
    }
    return datos;
  }

  function qs(params) {
    if (!params) return "";
    const limpio = Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== "");
    return limpio.length ? "?" + limpio.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`).join("&") : "";
  }

  async function descargar(ruta, params, nombrePorDefecto) {
    const opciones = { headers: {} };
    if (window.Auth && window.Auth.token())
      opciones.headers["Authorization"] = "Bearer " + window.Auth.token();

    const respuesta = await fetch(BASE + ruta + qs(params), opciones);
    if (respuesta.status === 401 && window.Auth) {
      window.Auth.logout();
      throw new Error("Sesión expirada.");
    }
    if (!respuesta.ok) throw new Error(`Error HTTP ${respuesta.status}`);

    const blob = await respuesta.blob();
    const cd = respuesta.headers.get("Content-Disposition") || "";
    const m = cd.match(/filename="?([^";]+)"?/i);
    const nombre = m ? m[1] : nombrePorDefecto;

    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url; a.download = nombre;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(() => URL.revokeObjectURL(url), 2000);
  }

  window.AlmacenApi = {
    inventario: {
      listar:    (p)   => solicitar("GET",  "/almacen" + qs(p)),
      obtener:   (id)  => solicitar("GET",  `/almacen/${id}`),
      porClave:  (c)   => solicitar("GET",  "/almacen/por-clave" + qs({ clave: c })),
      buscarClaves: (q, limite) => solicitar("GET", "/almacen/claves" + qs({ q, limite })),
      programas: ()    => solicitar("GET",  "/almacen/programas"),
      fuentesFinanciamiento:   () => solicitar("GET", "/almacen/fuentes-financiamiento"),
      partidasPresupuestales:  () => solicitar("GET", "/almacen/partidas-presupuestales"),
      cluesOrigen:             () => solicitar("GET", "/almacen/clues-origen"),
      areas:     ()    => solicitar("GET",  "/almacen/areas"),
      exportar:  (p)   => descargar("/almacen/exportar", p, "Almacen-Inventario.xlsx"),
      exportarClues: () => descargar("/almacen/exportar-clues", null, "Inventario-Semanal-CLUES.xlsx"),
      dashboard: ()    => solicitar("GET",  "/almacen/dashboard"),
      crear:     (dto) => solicitar("POST", "/almacen", dto),
      actualizar:(id, dto) => solicitar("PUT", `/almacen/${id}`, dto),
      desactivar:(id)  => solicitar("PUT", `/almacen/${id}/desactivar`),
      actualizarLimites:(id, min, max) =>
        solicitar("PUT", `/almacen/${id}/stock-limites`, { stockMinimo: min, stockMaximo: max })
    },
    lotes: {
      porArticulo:     (id, p) => solicitar("GET", `/almacen/${id}/lotes` + qs(p)),
      proximosCaducar: (dias)  => solicitar("GET", "/almacen/lotes/proximos-caducar" + qs({ dias }))
    },
    movimientos: {
      listar:    (p)   => solicitar("GET",  "/almacen/movimientos" + qs(p)),
      registrar: (dto) => solicitar("POST", "/almacen-movimientos", dto),
      // Caja: varios articulos, un solo destino, una sola transaccion.
      caja:      (dto) => solicitar("POST", "/almacen-movimientos/caja", dto),
      exportar:  (p)   => descargar("/almacen/movimientos/exportar", p, "Almacen-Movimientos.xlsx")
    },
    proveedores: {
      listar: (q) => solicitar("GET", "/almacen/proveedores" + qs({ q })),
      crear: (nombre, rfc) => solicitar("POST", "/almacen/proveedores", { nombre, rfc }),
      obtenerConRfc: (id) => solicitar("GET", `/almacen/proveedores/${id}/rfc`),
      agregarRfc: (id, rfc) => solicitar("POST", `/almacen/proveedores/${id}/rfc`, { rfc }),
      activarRfc: (id, rfcId) => solicitar("PUT", `/almacen/proveedores/${id}/rfc/${rfcId}/activar`),
      inactivarRfc: (id, rfcId) => solicitar("PUT", `/almacen/proveedores/${id}/rfc/${rfcId}/inactivar`)
    },
    config: {
      obtener: ()    => solicitar("GET", "/almacen/config"),
      guardar: (dto) => solicitar("PUT", "/almacen/config", dto)
    }
  };
})();
