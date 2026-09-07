(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const formateaFecha = (iso) => iso ? new Date(iso).toLocaleDateString("es-MX") : "—";
  const formateaFechaHora = (iso) =>
    iso ? new Date(iso).toLocaleString("es-MX", { dateStyle: "short", timeStyle: "short" }) : "—";

  // Mismo patrón que mtto-inventario.js: escapar también comillas porque el
  // resultado se inserta en atributos (data-*), no solo en texto.
  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  const CLASE_NIVEL = {
    "Sin stock": "badge-dark",
    "Bajo minimo": "badge-danger",
    "Normal": "badge-success",
    "Sobre maximo": "badge-warning"
  };

  const CLASE_ALERTA = {
    "Con caducados": "badge-danger",
    "Critico": "badge-danger",
    "Por vencer": "badge-warning",
    "Vigente": "badge-success",
    "Sin fecha": "badge-secondary"
  };

  const CLASE_ESTADO_LOTE = {
    "Vencido": "badge-danger",
    "Critico": "badge-danger",
    "Por vencer": "badge-warning",
    "Vigente": "badge-success",
    "Sin fecha": "badge-secondary"
  };

  const estado = {
    pagina: 1,
    tamano: 20,
    totalPaginas: 1,
    // { id, clave, nombre, referencia } del modal de lotes abierto. La
    // referencia viaja porque una clave puede tener varias presentaciones y
    // el alta de lote necesita saber a cuál de ellas pertenece.
    articuloEnTurno: null
  };

  // Paginación propia de "Movimientos recientes" dentro del modal: la
  // bitácora de una clave con muchos lotes puede crecer bastante, y cargarla
  // completa de un jalón sería lo que tira de peso al modal.
  const estadoHistorial = {
    pagina: 1,
    tamano: 10,
    totalPaginas: 1
  };

  async function cargarCatalogos() {
    const catalogos = await CateterApi.catalogos.todos();
    const select = document.getElementById("filtroCategoria");
    catalogos.categorias.forEach(c => {
      select.insertAdjacentHTML("beforeend", `<option value="${escapa(c.nombre)}">${escapa(c.nombre)}</option>`);
    });

    // Origen y destino del modal salen del catálogo, no de una lista fija:
    // una ubicación dada de alta en Catálogos tiene que poder usarse aquí.
    // Se respeta el default del HTML (Stock) si sigue existiendo.
    if (catalogos.ubicaciones && catalogos.ubicaciones.length) {
      const nombres = catalogos.ubicaciones.map(u => u.nombre);
      ["surtirOrigen", "surtirDestino"].forEach(id => {
        const combo = document.getElementById(id);
        // El modal hace form.reset() cada vez que se abre, y reset() vuelve al
        // atributo selected del HTML, no al valor actual: por eso el default se
        // marca en el propio markup en vez de asignar combo.value.
        const preferido = nombres.includes(combo.value) ? combo.value : nombres[0];
        combo.innerHTML = nombres.map(n =>
          `<option value="${escapa(n)}"${n === preferido ? " selected" : ""}>${escapa(n)}</option>`
        ).join("");
      });
    }
  }

  async function cargarKpis() {
    try {
      const kpis = await CateterApi.dashboard.kpis();
      document.getElementById("kpiClaves").textContent = formateaNumero(kpis.totalClaves);
      document.getElementById("kpiAlmacen").textContent = formateaNumero(kpis.piezasEnAlmacen);
      document.getElementById("kpiStock").textContent = formateaNumero(kpis.piezasEnStock);
      document.getElementById("kpiPorCaducar").textContent = formateaNumero(kpis.piezasPorVencer90);
    } catch { /* los KPIs son un extra; si fallan, la tabla sigue funcionando */ }
  }

  async function cargarTabla() {
    const tbody = document.getElementById("tablaInventario");
    const params = {
      q: document.getElementById("filtroTexto").value.trim(),
      categoria: document.getElementById("filtroCategoria").value,
      nivel: document.getElementById("filtroNivel").value,
      alerta: document.getElementById("filtroAlerta").value,
      pagina: estado.pagina,
      tamano: estado.tamano
    };

    try {
      const resultado = await CateterApi.inventario.listar(params);
      estado.totalPaginas = resultado.totalPaginas || 1;

      tbody.innerHTML = resultado.datos.map(a => `
        <tr>
          <td class="text-nowrap"><span style="font-family:monospace">${escapa(a.clave)}</span></td>
          <td>${escapa(a.nombre)}</td>
          <td>${escapa(a.categoria)}</td>
          <td class="text-right">${formateaNumero(a.enAlmacen)}</td>
          <td class="text-right">${formateaNumero(a.enStock)}</td>
          <td><span class="badge ${CLASE_NIVEL[a.nivel] || "badge-secondary"}">${escapa(a.nivel)}</span></td>
          <td><span class="badge ${CLASE_ALERTA[a.alertaCaducidad] || "badge-secondary"}">${escapa(a.alertaCaducidad)}</span></td>
          <td class="acciones-fila">
            <button type="button" class="btn btn-sm btn-outline-primary btn-lotes"
                    data-id="${a.id}" data-clave="${escapa(a.clave)}" data-nombre="${escapa(a.nombre)}"
                    data-referencia="${escapa(a.referencia)}"
                    title="Ver lotes, caducidad y movimientos">
              <i class="typcn typcn-time"></i> Lotes
            </button>
            <button type="button" class="btn btn-sm btn-outline-secondary btn-etiqueta"
                    data-clave="${escapa(a.clave)}" title="Imprimir la etiqueta de esta clave">
              <i class="typcn typcn-tags"></i>
            </button>
          </td>
        </tr>`).join("") || `<tr><td colspan="9">Sin resultados</td></tr>`;

      document.getElementById("resumenPagina").textContent =
        `Página ${estado.pagina} de ${estado.totalPaginas} — ${formateaNumero(resultado.total)} claves`;
      document.getElementById("btnPagAnterior").disabled = estado.pagina <= 1;
      document.getElementById("btnPagSiguiente").disabled = estado.pagina >= estado.totalPaginas;
      document.getElementById("estadoCarga").textContent = "";

      tbody.querySelectorAll(".btn-lotes").forEach(btn => {
        btn.addEventListener("click", () => abrirModalLotes(
          parseInt(btn.dataset.id, 10), btn.dataset.clave, btn.dataset.nombre,
          btn.dataset.referencia));
      });
      tbody.querySelectorAll(".btn-etiqueta").forEach(btn => {
        btn.addEventListener("click", () => abrirEtiquetas({ clave: btn.dataset.clave }));
      });
    } catch (err) {
      document.getElementById("estadoCarga").textContent =
        "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      document.getElementById("estadoCarga").classList.add("text-danger");
    }
  }

  // -------------------------------------------------------- modal de lotes
  async function abrirModalLotes(articuloId, clave, nombre, referencia) {
    estado.articuloEnTurno = { id: articuloId, clave, nombre, referencia: referencia || null };
    document.getElementById("lotesClave").textContent = clave;
    document.getElementById("lotesNombre").textContent = nombre;
    document.getElementById("formSurtir").reset();
    document.getElementById("formNuevoLote").reset();
    document.getElementById("alertaSurtir").classList.add("d-none");
    document.getElementById("alertaLote").classList.add("d-none");
    document.getElementById("histTipo").value = "";
    estadoHistorial.pagina = 1;
    actualizarCamposSurtir();

    // Siempre arranca en la pestaña "Movimientos": es lo primero que se usa
    // al abrir el detalle de una clave (surtir/consumir/mermar), no el
    // historial. Bootstrap no reinicia la pestaña activa entre aperturas.
    $('#tabsLote a[href="#tabMovimientos"]').tab("show");

    $("#modalLotes").modal("show");
    await Promise.all([cargarLotes(), cargarMovimientosDeLaClave()]);
  }

  async function cargarLotes() {
    const tbody = document.getElementById("tablaLotes");
    const selectLote = document.getElementById("surtirLote");
    tbody.innerHTML = `<tr><td colspan="7">Cargando...</td></tr>`;
    selectLote.innerHTML = `<option value="">Cargando…</option>`;
    try {
      const resultado = await CateterApi.lotes.listar({ articuloId: estado.articuloEnTurno.id, tamano: 200 });
      tbody.innerHTML = resultado.datos.map(l => `
        <tr>
          <td>${escapa(l.lote)}</td>
          <td>${formateaFecha(l.caducidad)}</td>
          <td><span class="badge ${CLASE_ESTADO_LOTE[l.estadoCaducidad] || "badge-secondary"}">${escapa(l.estadoCaducidad)}</span></td>
          <td class="text-right">${formateaNumero(l.enAlmacen)}</td>
          <td class="text-right">${formateaNumero(l.enStock)}</td>
          <td class="text-right font-weight-bold">${formateaNumero(l.total)}</td>
          <td class="small text-muted">${escapa(l.notas) || "—"}</td>
        </tr>`).join("") || `<tr><td colspan="7">Esta clave todavía no tiene lotes registrados. Da de alta el primero abajo.</td></tr>`;

      // Ordenar por caducidad más próxima (FEFO): nulos al final. El primero
      // queda preseleccionado, pero el usuario puede elegir otro lote.
      const lotesOrdenados = resultado.datos.slice().sort((a, b) => {
        if (!a.caducidad && !b.caducidad) return 0;
        if (!a.caducidad) return 1;
        if (!b.caducidad) return -1;
        return new Date(a.caducidad) - new Date(b.caducidad);
      });
      selectLote.innerHTML = lotesOrdenados
        .map((l, idx) => `<option value="${l.id}"${idx === 0 ? " selected" : ""}>${escapa(l.lote)} — almacén: ${formateaNumero(l.enAlmacen)}, stock: ${formateaNumero(l.enStock)}${l.caducidad ? ", caduca " + formateaFecha(l.caducidad) : ""}</option>`)
        .join("") || `<option value="">Esta clave no tiene lotes; da uno de alta abajo</option>`;
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="7" class="text-danger">No se pudieron cargar los lotes: ${escapa(err.message)}</td></tr>`;
      selectLote.innerHTML = `<option value="">No se pudieron cargar los lotes</option>`;
    }
  }

  async function cargarMovimientosDeLaClave() {
    const tbody = document.getElementById("tablaMovimientosLote");
    tbody.innerHTML = `<tr><td colspan="8">Cargando...</td></tr>`;
    try {
      const resultado = await CateterApi.movimientos.listar({
        clave: estado.articuloEnTurno.clave,
        tipo: document.getElementById("histTipo").value,
        pagina: estadoHistorial.pagina,
        tamano: estadoHistorial.tamano
      });
      estadoHistorial.totalPaginas = resultado.totalPaginas || 1;

      tbody.innerHTML = resultado.datos.map(m => `
        <tr>
          <td>${escapa(m.folio)}</td>
          <td class="text-nowrap">${formateaFechaHora(m.fecha)}</td>
          <td>${escapa(m.tipo)}</td>
          <td>${escapa(m.lote)}</td>
          <td class="text-right">${formateaNumero(m.cantidad)}</td>
          <td>${escapa(m.ubicacionOrigen) || "—"}</td>
          <td>${escapa(m.ubicacionDestino) || "—"}</td>
          <td>${escapa(m.responsable) || "—"}</td>
        </tr>`).join("") || `<tr><td colspan="8">Sin movimientos con este filtro.</td></tr>`;

      document.getElementById("histResumen").textContent =
        `Página ${estadoHistorial.pagina} de ${estadoHistorial.totalPaginas} — ${formateaNumero(resultado.total)} movimientos`;
      document.getElementById("histAnterior").disabled = estadoHistorial.pagina <= 1;
      document.getElementById("histSiguiente").disabled = estadoHistorial.pagina >= estadoHistorial.totalPaginas;
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="8" class="text-danger">No se pudo cargar la bitácora: ${escapa(err.message)}</td></tr>`;
    }
  }

  // Solo "Traslado a stock" mueve el material a otro lado; Salida y Merma
  // lo sacan del inventario sin destino, y Salida es el único que se anota
  // con una referencia (a qué paciente/caso se usó).
  const TIPOS_CON_DESTINO = ["Traslado a stock"];
  const TIPOS_CON_REFERENCIA = ["Salida"];

  function actualizarCamposSurtir() {
    const tipo = document.getElementById("surtirTipo").value;
    document.getElementById("grupoSurtirDestino").classList.toggle("d-none", !TIPOS_CON_DESTINO.includes(tipo));
    document.getElementById("grupoSurtirReferencia").classList.toggle("d-none", !TIPOS_CON_REFERENCIA.includes(tipo));
    autoseleccionarDestinoSurtir();
  }

  // Solo hay dos ubicaciones (Almacén/Stock), así que el destino de un
  // traslado siempre es la otra: elegir origen ya contesta la pregunta.
  function autoseleccionarDestinoSurtir() {
    const tipo = document.getElementById("surtirTipo").value;
    if (!TIPOS_CON_DESTINO.includes(tipo)) return;

    const origen = document.getElementById("surtirOrigen").value;
    const destino = document.getElementById("surtirDestino");
    if (origen === "Almacén") destino.value = "Stock";
    else if (origen === "Stock") destino.value = "Almacén";
  }

  async function surtir(ev) {
    ev.preventDefault();
    const alerta = document.getElementById("alertaSurtir");
    alerta.classList.add("d-none");

    const tipo = document.getElementById("surtirTipo").value;
    const loteId = parseInt(document.getElementById("surtirLote").value, 10);
    if (!loteId) {
      alerta.textContent = "Elige el lote del que se va a descontar.";
      alerta.classList.remove("d-none", "alert-success");
      alerta.classList.add("alert-danger");
      return;
    }

    const entrada = {
      loteId,
      cantidad: parseFloat(document.getElementById("surtirCantidad").value),
      tipo,
      ubicacionOrigen: document.getElementById("surtirOrigen").value,
      // El servidor ignora el destino en Salida/Merma, pero no se manda
      // salvo que aplique: así el formulario refleja lo que en verdad pasa.
      ubicacionDestino: TIPOS_CON_DESTINO.includes(tipo) ? document.getElementById("surtirDestino").value : null,
      referenciaUso: TIPOS_CON_REFERENCIA.includes(tipo)
        ? (document.getElementById("surtirReferencia").value.trim() || null) : null,
      responsable: document.getElementById("surtirResponsable").value.trim() || null
    };

    try {
      const resultado = await CateterApi.movimientos.registrar(entrada);
      alerta.textContent = `Registrado (${escapa(tipo)}): ${formateaNumero(resultado.cantidad)} del lote ${escapa(resultado.lote)}${resultado.caducidad ? " (caduca " + formateaFecha(resultado.caducidad) + ")" : ""}.`;
      alerta.classList.remove("d-none", "alert-danger");
      alerta.classList.add("alert-success");
      document.getElementById("formSurtir").reset();
      actualizarCamposSurtir();
      await Promise.all([cargarLotes(), cargarMovimientosDeLaClave(), cargarTabla(), cargarKpis()]);
    } catch (err) {
      alerta.textContent = err.message;
      alerta.classList.remove("d-none", "alert-success");
      alerta.classList.add("alert-danger");
    }
  }

  async function darDeAltaLote(ev) {
    ev.preventDefault();
    const alerta = document.getElementById("alertaLote");
    alerta.classList.add("d-none");

    const entrada = {
      clave: estado.articuloEnTurno.clave,
      // El modal se abrió desde una presentación concreta; mandarla evita que
      // el servidor rechace el alta cuando la clave tiene más de una.
      referencia: estado.articuloEnTurno.referencia,
      lote: document.getElementById("loteNumero").value.trim(),
      caducidad: document.getElementById("loteCaducidad").value || null,
      cantidadInicial: parseFloat(document.getElementById("loteCantidad").value) || 0
    };

    try {
      await CateterApi.lotes.guardar(entrada);
      alerta.textContent = "Lote dado de alta.";
      alerta.classList.remove("d-none", "alert-danger");
      alerta.classList.add("alert-success");
      document.getElementById("formNuevoLote").reset();
      await Promise.all([cargarLotes(), cargarMovimientosDeLaClave(), cargarTabla(), cargarKpis()]);
    } catch (err) {
      alerta.textContent = err.message;
      alerta.classList.remove("d-none", "alert-success");
      alerta.classList.add("alert-danger");
    }
  }

  // La hoja de etiquetas se abre en otra pestaña y allá vuelve a pedir los
  // datos: así lo impreso corresponde al filtro, no a la página que se ve.
  // Sin argumento imprime todo lo que cumple los filtros actuales.
  function abrirEtiquetas(extra) {
    const params = new URLSearchParams(extra || {
      q: document.getElementById("filtroTexto").value.trim(),
      categoria: document.getElementById("filtroCategoria").value,
      nivel: document.getElementById("filtroNivel").value,
      alerta: document.getElementById("filtroAlerta").value
    });
    [...params.keys()].forEach(k => { if (!params.get(k)) params.delete(k); });
    window.open("cateter-etiquetas.html?" + params, "_blank");
  }

  function exportar() {
    const params = new URLSearchParams({
      q: document.getElementById("filtroTexto").value.trim(),
      categoria: document.getElementById("filtroCategoria").value,
      nivel: document.getElementById("filtroNivel").value,
      alerta: document.getElementById("filtroAlerta").value
    });
    [...params.keys()].forEach(k => { if (!params.get(k)) params.delete(k); });
    if (window.Auth && window.Auth.token()) {
      params.append("token", window.Auth.token());
    }
    window.location.href = `${window.MTTO_CONFIG.API_BASE_URL}/cateter/exportar?${params}`;
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await cargarCatalogos();
    await Promise.all([cargarTabla(), cargarKpis()]);

    document.getElementById("formFiltros").addEventListener("submit", (ev) => {
      ev.preventDefault();
      estado.pagina = 1;
      cargarTabla();
    });
    document.getElementById("btnPagAnterior").addEventListener("click", () => {
      if (estado.pagina > 1) { estado.pagina--; cargarTabla(); }
    });
    document.getElementById("btnPagSiguiente").addEventListener("click", () => {
      if (estado.pagina < estado.totalPaginas) { estado.pagina++; cargarTabla(); }
    });
    document.getElementById("btnExportar").addEventListener("click", exportar);
    document.getElementById("btnEtiquetas").addEventListener("click", () => abrirEtiquetas(null));
    document.getElementById("formSurtir").addEventListener("submit", surtir);
    document.getElementById("surtirTipo").addEventListener("change", actualizarCamposSurtir);
    document.getElementById("surtirOrigen").addEventListener("change", autoseleccionarDestinoSurtir);
    document.getElementById("formNuevoLote").addEventListener("submit", darDeAltaLote);

    document.getElementById("btnHistBuscar").addEventListener("click", () => {
      estadoHistorial.pagina = 1;
      cargarMovimientosDeLaClave();
    });
    document.getElementById("histAnterior").addEventListener("click", () => {
      if (estadoHistorial.pagina > 1) { estadoHistorial.pagina--; cargarMovimientosDeLaClave(); }
    });
    document.getElementById("histSiguiente").addEventListener("click", () => {
      if (estadoHistorial.pagina < estadoHistorial.totalPaginas) { estadoHistorial.pagina++; cargarMovimientosDeLaClave(); }
    });
  });
})();
