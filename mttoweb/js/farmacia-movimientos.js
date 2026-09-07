(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const formateaFecha = (iso) => new Date(iso).toLocaleString("es-MX", { dateStyle: "short", timeStyle: "short" });

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  // Entrada y Ajuste (+) suman existencia: no hay "el que caduca antes" que
  // elegir al sumar, así que aquí sí hace falta decir a mano a qué lote.
  // Los demás (Salida/Merma/Ajuste(-)) restan y el servidor elige por FEFO.
  const TIPOS_CON_LOTE_ESPECIFICO = ["Entrada", "Ajuste (+)"];
  const TIPOS_CON_DATOS_PACIENTE = ["Salida"];

  const estado = { codigos: {}, pagina: 1, tamano: 15, totalPaginas: 1, colaMovimientos: [] };

  function actualizarCamposPorTipo() {
    const tipo = document.getElementById("campoTipo").value;
    const necesitaLote = TIPOS_CON_LOTE_ESPECIFICO.includes(tipo);
    document.getElementById("grupoLote").classList.toggle("d-none", !necesitaLote);
    if (necesitaLote) cargarLotesDelCodigo();

    const conDatosPaciente = TIPOS_CON_DATOS_PACIENTE.includes(tipo);
    document.getElementById("grupoDatosPaciente").classList.toggle("d-none", !conDatosPaciente);
    document.getElementById("grupoDatosClinicos").classList.toggle("d-none", !conDatosPaciente);
  }

  function actualizarCodigoElegido() {
    const codigo = document.getElementById("campoCodigo").value.trim();
    const pie = document.getElementById("codigoElegido");
    if (!codigo) { pie.innerHTML = "&nbsp;"; pie.className = "form-text text-muted"; return; }

    const articulo = estado.codigos[codigo];
    if (articulo) {
      pie.textContent = `${articulo.nombre} — existencia: ${formateaNumero(articulo.total)}`;
      pie.className = "form-text text-success";
    } else {
      pie.textContent = `No existe un medicamento activo con código "${codigo}" en el catálogo.`;
      pie.className = "form-text text-danger";
    }

    if (TIPOS_CON_LOTE_ESPECIFICO.includes(document.getElementById("campoTipo").value)) {
      cargarLotesDelCodigo();
    }
  }

  async function cargarLotesDelCodigo() {
    const select = document.getElementById("campoLote");
    const articulo = estado.codigos[document.getElementById("campoCodigo").value.trim()];
    if (!articulo) { select.innerHTML = `<option value="">Elige primero un código válido</option>`; return; }

    select.innerHTML = `<option value="">Cargando…</option>`;
    try {
      const resultado = await FarmaciaApi.lotes.listar({ articuloId: articulo.id, tamano: 200 });
      select.innerHTML = resultado.datos
        .map(l => `<option value="${l.id}">${escapa(l.lote)} — existencia: ${formateaNumero(l.cantidad)}${l.caducidad ? ", caduca " + new Date(l.caducidad).toLocaleDateString("es-MX") : ""}</option>`)
        .join("") || `<option value="">Este medicamento no tiene lotes; da uno de alta primero desde Inventario</option>`;
    } catch (err) {
      select.innerHTML = `<option value="">No se pudieron cargar los lotes</option>`;
    }
  }

  async function cargarCodigos() {
    const inventario = await FarmaciaApi.inventario.listar({ tamano: 500 });
    estado.codigos = Object.fromEntries(inventario.datos.map(a => [a.codigo, a]));

    document.getElementById("listaCodigos").innerHTML = inventario.datos
      .map(a => `<option value="${escapa(a.codigo)}" label="${escapa(a.nombre)} (existencia ${formateaNumero(a.total)})"></option>`)
      .join("");
  }

  async function cargarBitacora() {
    const tbody = document.getElementById("tablaMovimientos");
    tbody.innerHTML = `<tr><td colspan="8">Cargando...</td></tr>`;
    try {
      const resultado = await FarmaciaApi.movimientos.listar({
        codigo: document.getElementById("filtroCodigo").value.trim(),
        tipo: document.getElementById("filtroTipo").value,
        pagina: estado.pagina,
        tamano: estado.tamano
      });
      estado.totalPaginas = resultado.totalPaginas || 1;

      tbody.innerHTML = resultado.datos.map(m => `
        <tr>
          <td>${escapa(m.folio)}</td>
          <td class="text-nowrap">${formateaFecha(m.fecha)}</td>
          <td>${escapa(m.tipo)}</td>
          <td><span style="font-family:monospace">${escapa(m.codigo)}</span></td>
          <td>${escapa(m.lote)}</td>
          <td class="text-right">${formateaNumero(m.cantidad)}</td>
          <td>${escapa(m.areaServicio) || "—"}</td>
          <td>${escapa(m.responsable) || "—"}</td>
        </tr>`).join("") || `<tr><td colspan="8">Sin movimientos con este filtro.</td></tr>`;

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
    const codigo = document.getElementById("campoCodigo").value.trim();
    const cantidad = parseFloat(document.getElementById("campoCantidad").value) || 0;
    const responsable = document.getElementById("campoResponsable").value.trim() || "Sin registrar";
    const observaciones = document.getElementById("campoObservaciones").value.trim() || "";

    if (!tipo || !codigo || cantidad <= 0) {
      errorBox.textContent = "Falta tipo, código o cantidad.";
      errorBox.classList.remove("d-none");
      return;
    }

    if (!estado.codigos[codigo]) {
      errorBox.textContent = `No existe un medicamento activo con código "${codigo}" en el catálogo.`;
      errorBox.classList.remove("d-none");
      return;
    }

    if (TIPOS_CON_LOTE_ESPECIFICO.includes(tipo)) {
      const loteId = document.getElementById("campoLote").value;
      if (!loteId) {
        errorBox.textContent = "Elige el lote al que se le va a agregar la cantidad.";
        errorBox.classList.remove("d-none");
        return;
      }
    }

    const conDatosPaciente = TIPOS_CON_DATOS_PACIENTE.includes(tipo);
    const movimiento = {
      id: Date.now() + Math.random(),
      tipo,
      codigo,
      cantidad,
      responsable,
      observaciones,
      loteId: TIPOS_CON_LOTE_ESPECIFICO.includes(tipo) ? parseInt(document.getElementById("campoLote").value, 10) : null,
      pacienteNombre: conDatosPaciente ? (document.getElementById("campoPaciente").value.trim() || null) : null,
      pacienteExpediente: conDatosPaciente ? (document.getElementById("campoExpediente").value.trim() || null) : null,
      diagnostico: conDatosPaciente ? (document.getElementById("campoDiagnostico").value.trim() || null) : null,
      cie: conDatosPaciente ? (document.getElementById("campoCie").value.trim() || null) : null,
      areaServicio: conDatosPaciente ? (document.getElementById("campoAreaServicio").value.trim() || null) : null
    };

    estado.colaMovimientos.push(movimiento);
    actualizarTablaCola();

    document.getElementById("campoCodigo").value = "";
    document.getElementById("campoCantidad").value = "";
    document.getElementById("campoObservaciones").value = "";
    document.getElementById("campoPaciente").value = "";
    document.getElementById("campoExpediente").value = "";
    document.getElementById("campoDiagnostico").value = "";
    document.getElementById("campoCie").value = "";
    document.getElementById("campoAreaServicio").value = "";
    actualizarCodigoElegido();
    document.getElementById("campoCodigo").focus();
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

    tabla.innerHTML = estado.colaMovimientos.map((m, idx) => `
      <tr>
        <td>${escapa(m.tipo)}</td>
        <td><span style="font-family:monospace">${escapa(m.codigo)}</span></td>
        <td class="text-right">${formateaNumero(m.cantidad)}</td>
        <td>${m.areaServicio ? escapa(m.areaServicio) : "—"}</td>
        <td class="small" style="max-width: 150px; overflow: hidden; text-overflow: ellipsis;">${m.observaciones ? escapa(m.observaciones) : "—"}</td>
        <td class="text-center">
          <button type="button" class="btn btn-sm btn-outline-danger" onclick="eliminarDelaCola(${idx})">×</button>
        </td>
      </tr>
    `).join("");
  }

  function eliminarDelaCola(idx) {
    estado.colaMovimientos.splice(idx, 1);
    actualizarTablaCola();
  }
  window.eliminarDelaCola = eliminarDelaCola;

  async function guardarTodosLosMovimientos() {
    if (estado.colaMovimientos.length === 0) {
      alert("No hay movimientos en la cola.");
      return;
    }

    const errorBox = document.getElementById("errorMovimiento");
    const exitoBox = document.getElementById("exitoMovimiento");
    errorBox.classList.add("d-none");
    exitoBox.classList.add("d-none");

    try {
      let registrados = 0;
      let errores = [];

      for (const movimiento of estado.colaMovimientos) {
        try {
          if (movimiento.loteId) {
            await FarmaciaApi.movimientos.registrar({
              loteId: movimiento.loteId,
              tipo: movimiento.tipo,
              cantidad: movimiento.cantidad,
              responsable: movimiento.responsable,
              observaciones: movimiento.observaciones
            });
          } else {
            await FarmaciaApi.movimientos.surtir({
              codigo: movimiento.codigo,
              cantidad: movimiento.cantidad,
              tipo: movimiento.tipo,
              pacienteNombre: movimiento.pacienteNombre,
              pacienteExpediente: movimiento.pacienteExpediente,
              diagnostico: movimiento.diagnostico,
              cie: movimiento.cie,
              areaServicio: movimiento.areaServicio,
              responsable: movimiento.responsable,
              observaciones: movimiento.observaciones
            });
          }
          registrados++;
        } catch (err) {
          errores.push(`${movimiento.codigo}: ${err.message}`);
        }
      }

      estado.colaMovimientos = [];
      actualizarTablaCola();

      await cargarCodigos();
      actualizarCodigoElegido();
      estado.pagina = 1;
      await cargarBitacora();

      exitoBox.textContent = `✓ ${registrados} movimiento(s) guardado(s).${errores.length > 0 ? ` (${errores.length} error(es))` : ""}`;
      exitoBox.classList.remove("d-none");
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  function limpiarCola() {
    if (confirm("¿Descartar todos los movimientos pendientes?")) {
      estado.colaMovimientos = [];
      actualizarTablaCola();
    }
  }

  function exportar() {
    const params = new URLSearchParams({
      codigo: document.getElementById("filtroCodigo").value.trim(),
      tipo: document.getElementById("filtroTipo").value
    });
    [...params.keys()].forEach(k => { if (!params.get(k)) params.delete(k); });
    if (window.Auth && window.Auth.token()) {
      params.append("token", window.Auth.token());
    }
    window.location.href = `${window.MTTO_CONFIG.API_BASE_URL}/farmacia-movimientos/exportar?${params}`;
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await cargarCodigos();
    await cargarBitacora();
    actualizarCamposPorTipo();

    document.getElementById("campoTipo").addEventListener("change", actualizarCamposPorTipo);
    document.getElementById("formMovimiento").addEventListener("submit", agregarMovimientoALaCola);
    document.getElementById("btnGuardarTodos").addEventListener("click", guardarTodosLosMovimientos);
    document.getElementById("btnLimpiarCola").addEventListener("click", limpiarCola);
    document.getElementById("btnExportar").addEventListener("click", exportar);

    const campoCodigo = document.getElementById("campoCodigo");
    campoCodigo.addEventListener("input", actualizarCodigoElegido);
    campoCodigo.addEventListener("change", actualizarCodigoElegido);
    campoCodigo.addEventListener("keydown", (ev) => {
      if (ev.key === "Enter") {
        ev.preventDefault();
        actualizarCodigoElegido();
        document.getElementById("campoCantidad").focus();
      }
    });
    campoCodigo.focus();

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
  });
})();
