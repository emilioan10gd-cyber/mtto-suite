(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const formateaFecha = (iso) => new Date(iso).toLocaleString("es-MX", { dateStyle: "short", timeStyle: "short" });

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  const TIPOS_CON_ORIGEN = ["Traslado a stock", "Retorno a almacén", "Salida", "Merma", "Ajuste (-)"];
  const TIPOS_CON_DESTINO = ["Traslado a stock", "Retorno a almacén", "Ajuste (+)"];
  const TIPOS_CON_REFERENCIA = ["Salida"];
  const TIPOS_CON_LOTE_ESPECIFICO = ["Traslado a stock", "Retorno a almacén", "Salida", "Merma", "Ajuste (+)", "Ajuste (-)"];

  const estado = { claves: {}, pagina: 1, tamano: 15, totalPaginas: 1, colaMovimientos: [], movimientoCancelando: null };

  function actualizarCamposPorTipo() {
    const tipo = document.getElementById("campoTipo").value;
    document.getElementById("grupoOrigen").classList.toggle("d-none", !TIPOS_CON_ORIGEN.includes(tipo));
    document.getElementById("grupoDestino").classList.toggle("d-none", !TIPOS_CON_DESTINO.includes(tipo));
    document.getElementById("grupoReferencia").classList.toggle("d-none", !TIPOS_CON_REFERENCIA.includes(tipo));

    const necesitaLote = TIPOS_CON_LOTE_ESPECIFICO.includes(tipo);
    document.getElementById("grupoLote").classList.toggle("d-none", !necesitaLote);
    if (necesitaLote) cargarLotesDeLaClave();

    // Auto-seleccionar destino si es una transferencia
    autoseleccionarDestino();
  }

  function autoseleccionarDestino() {
    const tipo = document.getElementById("campoTipo").value;
    const esTransferencia = ["Traslado a stock", "Retorno a almacén"].includes(tipo);

    if (!esTransferencia) return;

    const origen = document.getElementById("campoOrigen").value;
    const destino = document.getElementById("campoDestino");

    // Si origen es Almacén → destino Stock; si origen es Stock → destino Almacén
    if (origen === "Almacén") {
      destino.value = "Stock";
    } else if (origen === "Stock") {
      destino.value = "Almacén";
    }
  }

  // Con un <input list> se puede dejar escrito algo que no es una clave
  // válida; esto confirma qué quedó seleccionado antes de intentar guardar.
  function actualizarClaveElegida() {
    const clave = document.getElementById("campoClave").value.trim();
    const pie = document.getElementById("claveElegida");
    if (!clave) { pie.innerHTML = "&nbsp;"; pie.className = "form-text text-muted"; return; }

    const articulo = estado.claves[clave];
    if (articulo) {
      pie.textContent = `${articulo.nombre} — almacén: ${formateaNumero(articulo.enAlmacen)}, stock: ${formateaNumero(articulo.enStock)}`;
      pie.className = "form-text text-success";
    } else {
      pie.textContent = `No existe una clave activa "${clave}" en el catálogo de la clínica.`;
      pie.className = "form-text text-danger";
    }

    if (TIPOS_CON_LOTE_ESPECIFICO.includes(document.getElementById("campoTipo").value)) {
      cargarLotesDeLaClave();
    }
  }

  async function cargarLotesDeLaClave() {
    const select = document.getElementById("campoLote");
    const articulo = estado.claves[document.getElementById("campoClave").value.trim()];
    if (!articulo) { select.innerHTML = `<option value="">Elige primero una clave válida</option>`; return; }

    select.innerHTML = `<option value="">Cargando…</option>`;
    try {
      const resultado = await CateterApi.lotes.listar({ articuloId: articulo.id, tamano: 200 });
      const lotes = resultado.datos.slice();

      // Ordenar por caducidad más próxima (FEFO): nulos al final
      lotes.sort((a, b) => {
        if (!a.caducidad && !b.caducidad) return 0;
        if (!a.caducidad) return 1;
        if (!b.caducidad) return -1;
        return new Date(a.caducidad) - new Date(b.caducidad);
      });

      select.innerHTML = lotes
        .map((l, idx) => `<option value="${l.id}"${idx === 0 ? " selected" : ""}>${escapa(l.lote)} — almacén: ${formateaNumero(l.enAlmacen)}, stock: ${formateaNumero(l.enStock)}${l.caducidad ? ", caduca " + new Date(l.caducidad).toLocaleDateString("es-MX") : ""}</option>`)
        .join("") || `<option value="">Esta clave no tiene lotes; da uno de alta primero desde Inventario</option>`;
    } catch (err) {
      select.innerHTML = `<option value="">No se pudieron cargar los lotes</option>`;
    }
  }

  async function cargarClaves() {
    // soloActivos ya es el default de la API; tamano:500 cubre holgadamente
    // el catálogo actual de la clínica sin tener que paginar aquí.
    const inventario = await CateterApi.inventario.listar({ tamano: 500 });
    estado.claves = Object.fromEntries(inventario.datos.map(a => [a.clave, a]));

    document.getElementById("listaClaves").innerHTML = inventario.datos
      .map(a => `<option value="${escapa(a.clave)}" label="${escapa(a.nombre)} (almacén ${formateaNumero(a.enAlmacen)} / stock ${formateaNumero(a.enStock)})"></option>`)
      .join("");
  }

  async function cargarBitacora() {
    const tbody = document.getElementById("tablaMovimientos");
    tbody.innerHTML = `<tr><td colspan="12" class="text-center">Cargando...</td></tr>`;
    try {
      const resultado = await CateterApi.movimientos.listar({
        clave: document.getElementById("filtroClave").value.trim(),
        tipo: document.getElementById("filtroTipo").value,
        responsable: document.getElementById("filtroResponsable").value.trim(),
        estado: document.getElementById("filtroEstado").value,
        pagina: estado.pagina,
        tamano: estado.tamano
      });
      estado.totalPaginas = resultado.totalPaginas || 1;

      tbody.innerHTML = resultado.datos.map(m => {
        const esCancelado = m.estado === "Cancelado";
        const claseEstado = esCancelado ? "badge badge-danger" : "badge badge-success";
        const textoEstado = esCancelado ? "Cancelado" : "Activo";
        // data-* + delegación en vez de onclick inline: el folio y la clave
        // llevan puntos y comillas que rompen un atributo onclick.
        const btnCancelar = esCancelado
          ? `<span class="text-muted small" aria-hidden="true">—</span>`
          : `<button type="button" class="btn btn-sm btn-outline-danger btn-icono-tabla btn-cancelar-mov"
              data-id="${m.id}" data-folio="${escapa(m.folio)}" data-clave="${escapa(m.clave)}" data-cantidad="${m.cantidad}"
              title="Cancelar este movimiento"
              aria-label="Cancelar el movimiento ${escapa(m.folio)}">
              <i class="typcn typcn-trash" aria-hidden="true"></i>
            </button>`;

        return `
        <tr${esCancelado ? ' class="fila-cancelada"' : ''}>
          <td>${escapa(m.folio)}</td>
          <td class="text-nowrap">${formateaFecha(m.fecha)}</td>
          <td>${escapa(m.tipo)}</td>
          <td><span style="font-family:monospace">${escapa(m.clave)}</span></td>
          <td>${escapa(m.lote)}</td>
          <td class="text-right">${formateaNumero(m.cantidad)}</td>
          <td>${escapa(m.ubicacionOrigen) || "—"}</td>
          <td>${escapa(m.ubicacionDestino) || "—"}</td>
          <td>${escapa(m.responsable) || "—"}</td>
          <td class="small" style="max-width: 150px; overflow: hidden; text-overflow: ellipsis;">${escapa(m.observaciones) || "—"}</td>
          <td class="text-center"><span class="${claseEstado}" style="font-size: 0.75rem; padding: 0.25rem 0.5rem;">${textoEstado}</span></td>
          <td class="text-center">${btnCancelar}</td>
        </tr>`;
      }).join("") || `<tr><td colspan="12" class="text-center">Sin movimientos con este filtro.</td></tr>`;

      document.getElementById("bitacoraResumen").textContent =
        `Página ${estado.pagina} de ${estado.totalPaginas} — ${formateaNumero(resultado.total)} movimientos`;
      document.getElementById("bitacoraAnterior").disabled = estado.pagina <= 1;
      document.getElementById("bitacoraSiguiente").disabled = estado.pagina >= estado.totalPaginas;
      document.getElementById("estadoCarga").textContent = "";
    } catch (err) {
      document.getElementById("estadoCarga").textContent =
        "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      document.getElementById("estadoCarga").classList.add("text-danger");
    }
  }

  function agregarMovimientoALaCola(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorMovimiento");
    errorBox.classList.add("d-none");

    const tipo = document.getElementById("campoTipo").value;
    const clave = document.getElementById("campoClave").value.trim();
    const cantidad = parseFloat(document.getElementById("campoCantidad").value) || 0;
    const responsable = document.getElementById("campoResponsable").value.trim() || "Sin registrar";
    const observaciones = document.getElementById("campoObservaciones").value.trim() || "";

    if (!tipo || !clave || cantidad <= 0) {
      errorBox.textContent = "Falta tipo, clave o cantidad.";
      errorBox.classList.remove("d-none");
      return;
    }

    if (!estado.claves[clave]) {
      errorBox.textContent = `No existe una clave activa "${clave}" en el catálogo de la clínica.`;
      errorBox.classList.remove("d-none");
      return;
    }

    // Validaciones específicas por tipo
    if (TIPOS_CON_LOTE_ESPECIFICO.includes(tipo)) {
      const loteId = document.getElementById("campoLote").value;
      if (!loteId) {
        errorBox.textContent = "Elige el lote al que se le va a ajustar la cantidad.";
        errorBox.classList.remove("d-none");
        return;
      }
    }

    // Agregar a la cola
    const movimiento = {
      id: Date.now() + Math.random(),
      tipo,
      clave,
      cantidad,
      responsable,
      observaciones,
      origen: TIPOS_CON_ORIGEN.includes(tipo) ? document.getElementById("campoOrigen").value : null,
      destino: TIPOS_CON_DESTINO.includes(tipo) ? document.getElementById("campoDestino").value : null,
      referencia: TIPOS_CON_REFERENCIA.includes(tipo) ? (document.getElementById("campoReferencia").value.trim() || null) : null,
      loteId: TIPOS_CON_LOTE_ESPECIFICO.includes(tipo) ? parseInt(document.getElementById("campoLote").value, 10) : null
    };

    estado.colaMovimientos.push(movimiento);
    actualizarTablaCola();

    // Limpiar formulario para el siguiente
    document.getElementById("campoClave").value = "";
    document.getElementById("campoCantidad").value = "";
    document.getElementById("campoObservaciones").value = "";
    document.getElementById("campoReferencia").value = "";
    actualizarClaveElegida();
    document.getElementById("campoClave").focus();
  }

  function actualizarTablaCola() {
    const tabla = document.getElementById("tablaMovimientosPendientes");
    const seccion = document.getElementById("seccionCola");
    const contador = document.getElementById("contadorCola");

    if (estado.colaMovimientos.length === 0) {
      seccion.style.display = "none";
      tabla.innerHTML = "";
      return;
    }

    seccion.style.display = "block";
    contador.textContent = estado.colaMovimientos.length;

    const piezas = estado.colaMovimientos.reduce((suma, m) => suma + m.cantidad, 0);
    const claves = new Set(estado.colaMovimientos.map(m => m.clave)).size;
    document.getElementById("resumenCola").textContent =
      `${formateaNumero(piezas)} pieza(s) · ${claves} clave(s)`;

    // data-idx + delegación, no onclick inline: la función vive dentro de este
    // IIFE, así que un onclick="eliminarDelaCola(...)" la buscaba en window,
    // no la encontraba y el botón no hacía nada.
    tabla.innerHTML = estado.colaMovimientos.map((m, idx) => `
      <tr>
        <td>${escapa(m.tipo)}</td>
        <td><span style="font-family:monospace">${escapa(m.clave)}</span></td>
        <td class="text-right">${formateaNumero(m.cantidad)}</td>
        <td>${m.origen ? escapa(m.origen) : "—"}</td>
        <td>${m.destino ? escapa(m.destino) : "—"}</td>
        <td>${m.referencia ? escapa(m.referencia) : "—"}</td>
        <td class="small text-truncate" style="max-width: 200px;" title="${escapa(m.observaciones) || ""}">${m.observaciones ? escapa(m.observaciones) : "—"}</td>
        <td class="text-center">
          <button type="button" class="btn btn-sm btn-outline-danger btn-icono-tabla btn-quitar-cola"
                  data-idx="${idx}" title="Quitar de la lista"
                  aria-label="Quitar ${escapa(m.clave)} de la lista">
            <i class="typcn typcn-times" aria-hidden="true"></i>
          </button>
        </td>
      </tr>
    `).join("");
  }

  function eliminarDelaCola(idx) {
    estado.colaMovimientos.splice(idx, 1);
    actualizarTablaCola();
  }

  async function guardarTodosLosMovimientos() {
    if (estado.colaMovimientos.length === 0) {
      alert("No hay movimientos en la cola.");
      return;
    }

    const errorBox = document.getElementById("errorMovimiento");
    const exitoBox = document.getElementById("exitoMovimiento");
    const boton = document.getElementById("btnGuardarTodos");
    errorBox.classList.add("d-none");
    exitoBox.classList.add("d-none");

    const htmlBoton = boton.innerHTML;
    boton.disabled = true;
    boton.innerHTML = `<span class="spinner-border spinner-border-sm mr-1" role="status" aria-hidden="true"></span>Guardando…`;

    try {
      let registrados = 0;
      const errores = [];
      const fallidos = [];

      for (const movimiento of estado.colaMovimientos) {
        try {
          if (movimiento.loteId) {
            await CateterApi.movimientos.registrar({
              loteId: movimiento.loteId,
              tipo: movimiento.tipo,
              cantidad: movimiento.cantidad,
              ubicacionDestino: movimiento.destino,
              responsable: movimiento.responsable,
              observaciones: movimiento.observaciones
            });
          } else {
            await CateterApi.movimientos.surtir({
              clave: movimiento.clave,
              cantidad: movimiento.cantidad,
              tipo: movimiento.tipo,
              ubicacionOrigen: movimiento.origen,
              ubicacionDestino: movimiento.destino,
              referenciaUso: movimiento.referencia,
              responsable: movimiento.responsable,
              observaciones: movimiento.observaciones
            });
          }
          registrados++;
        } catch (err) {
          errores.push(`${movimiento.clave}: ${err.message}`);
          // Se conserva en la cola lo que no se pudo guardar: vaciarla entera
          // borraba la captura y no quedaba manera de reintentar.
          fallidos.push(movimiento);
        }
      }

      estado.colaMovimientos = fallidos;
      actualizarTablaCola();

      await cargarClaves();
      actualizarClaveElegida();
      estado.pagina = 1;
      await cargarBitacora();

      if (registrados > 0) {
        exitoBox.textContent = `✓ ${registrados} movimiento(s) guardado(s).`;
        exitoBox.classList.remove("d-none");
      }
      if (errores.length > 0) {
        errorBox.innerHTML =
          `<strong>${errores.length} movimiento(s) no se guardaron</strong> y siguen en la lista para reintentar:` +
          `<ul class="mb-0 mt-1 pl-4">${errores.map(e => `<li>${escapa(e)}</li>`).join("")}</ul>`;
        errorBox.classList.remove("d-none");
      }
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    } finally {
      boton.disabled = false;
      boton.innerHTML = htmlBoton;
    }
  }

  function limpiarCola() {
    if (confirm("¿Descartar todos los movimientos pendientes?")) {
      estado.colaMovimientos = [];
      actualizarTablaCola();
    }
  }

  // Origen y destino salen del catálogo, no de una lista fija en el HTML: si
  // la clínica da de alta un salón nuevo en Catálogos, tiene que aparecer aquí
  // sin tocar código. Los movimientos viajan con el nombre de la ubicación.
  //
  // Se conservan los valores que el HTML trae seleccionados (Stock de origen,
  // Stock de destino) cuando existen, para no cambiarle el default a quien ya
  // está acostumbrado a capturar de corrido.
  async function cargarUbicaciones() {
    const origen = document.getElementById("campoOrigen");
    const destino = document.getElementById("campoDestino");
    const previoOrigen = origen.value;
    const previoDestino = destino.value;

    let ubicaciones;
    try {
      const catalogos = await CateterApi.catalogos.todos();
      ubicaciones = catalogos.ubicaciones;
    } catch {
      return; // Si el catálogo no carga, se quedan las opciones del HTML.
    }
    if (!ubicaciones || !ubicaciones.length) return;

    const nombres = ubicaciones.map(u => u.nombre);
    // Se marca selected en el markup y no con .value porque el formulario se
    // limpia con reset() al encolar un movimiento, y reset() vuelve al atributo.
    const opciones = (preferido) => nombres.map(n =>
      `<option value="${escapa(n)}"${n === preferido ? " selected" : ""}>${escapa(n)}</option>`
    ).join("");

    origen.innerHTML = opciones(nombres.includes(previoOrigen) ? previoOrigen : nombres[0]);
    destino.innerHTML = opciones(nombres.includes(previoDestino) ? previoDestino : nombres[0]);
  }

  function abrirModalCancelar(movimientoId, folio, clave, cantidad) {
    estado.movimientoCancelando = { id: movimientoId, folio, clave, cantidad };
    document.getElementById("modalFolio").textContent = folio;
    document.getElementById("modalArticulo").textContent = clave;
    document.getElementById("modalCantidad").textContent = formateaNumero(cantidad);
    document.getElementById("modalRazon").value = "";
    const btn = document.getElementById("btnConfirmarCancelacion");
    btn.disabled = false;
    btn.innerHTML = `<i class="typcn typcn-trash mr-1"></i>Sí, cancelar movimiento`;
    $("#modalCancelarMovimiento").modal("show");
  }

  async function confirmarCancelacion() {
    if (!estado.movimientoCancelando) return;

    const btnConfirmar = document.getElementById("btnConfirmarCancelacion");
    const folio = estado.movimientoCancelando.folio;
    btnConfirmar.disabled = true;
    btnConfirmar.textContent = "Cancelando...";

    try {
      const razon = document.getElementById("modalRazon").value.trim() || null;
      await CateterApi.movimientos.cancelar(estado.movimientoCancelando.id, { razon });

      $("#modalCancelarMovimiento").modal("hide");
      estado.movimientoCancelando = null;
      estado.pagina = 1;
      await cargarBitacora();

      const exitoBox = document.getElementById("exitoMovimiento");
      exitoBox.textContent = `✓ Movimiento ${folio} cancelado. Se registró la reversión de existencias.`;
      exitoBox.classList.remove("d-none");
      setTimeout(() => exitoBox.classList.add("d-none"), 6000);
    } catch (err) {
      alert("Error al cancelar: " + err.message);
      btnConfirmar.disabled = false;
      btnConfirmar.innerHTML = `<i class="typcn typcn-trash mr-1"></i>Sí, cancelar movimiento`;
    }
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await cargarUbicaciones();
    await cargarClaves();
    await cargarBitacora();
    actualizarCamposPorTipo();

    document.getElementById("campoTipo").addEventListener("change", actualizarCamposPorTipo);
    document.getElementById("campoOrigen").addEventListener("change", autoseleccionarDestino);
    document.getElementById("formMovimiento").addEventListener("submit", agregarMovimientoALaCola);
    document.getElementById("btnGuardarTodos").addEventListener("click", guardarTodosLosMovimientos);
    document.getElementById("btnLimpiarCola").addEventListener("click", limpiarCola);

    const campoClave = document.getElementById("campoClave");
    campoClave.addEventListener("input", actualizarClaveElegida);
    campoClave.addEventListener("change", actualizarClaveElegida);
    // El escáner cierra con Enter; sin esto el Enter enviaría el formulario a
    // medio llenar. Se salta al campo de cantidad, que es el siguiente dato.
    campoClave.addEventListener("keydown", (ev) => {
      if (ev.key === "Enter") {
        ev.preventDefault();
        actualizarClaveElegida();
        document.getElementById("campoCantidad").focus();
      }
    });
    campoClave.focus();

    document.getElementById("btnBuscarBitacora").addEventListener("click", () => {
      estado.pagina = 1;
      cargarBitacora();
    });
    document.getElementById("bitacoraAnterior").addEventListener("click", () => {
      if (estado.pagina > 1) { estado.pagina--; cargarBitacora(); }
    });
    document.getElementById("bitacoraSiguiente").addEventListener("click", () => {
      if (estado.pagina < estado.totalPaginas) { estado.pagina++; cargarBitacora(); }
    });

    document.getElementById("btnConfirmarCancelacion").addEventListener("click", confirmarCancelacion);

    document.getElementById("tablaMovimientosPendientes").addEventListener("click", (ev) => {
      const btn = ev.target.closest(".btn-quitar-cola");
      if (!btn) return;
      eliminarDelaCola(parseInt(btn.dataset.idx, 10));
    });

    document.getElementById("tablaMovimientos").addEventListener("click", (ev) => {
      const btn = ev.target.closest(".btn-cancelar-mov");
      if (!btn) return;
      abrirModalCancelar(
        parseInt(btn.dataset.id, 10),
        btn.dataset.folio,
        btn.dataset.clave,
        parseFloat(btn.dataset.cantidad)
      );
    });

    // Filtro de estado: por defecto mostrar solo activos
    document.getElementById("filtroEstado").addEventListener("change", () => {
      estado.pagina = 1;
      cargarBitacora();
    });
  });
})();
