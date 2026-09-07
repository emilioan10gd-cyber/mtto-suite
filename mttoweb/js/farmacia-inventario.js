(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const formateaFecha = (iso) => iso ? new Date(iso).toLocaleDateString("es-MX") : "—";
  const formateaFechaHora = (iso) =>
    iso ? new Date(iso).toLocaleString("es-MX", { dateStyle: "short", timeStyle: "short" }) : "—";

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

  // Solo Salida pide datos de paciente; el servidor los ignora en los demás
  // tipos, pero ocultarlos evita que se capturen sin que sirvan de nada.
  const TIPOS_CON_DATOS_PACIENTE = ["Salida"];

  const estado = {
    pagina: 1,
    tamano: 20,
    totalPaginas: 1,
    articuloEnTurno: null // { id, codigo, nombre }
  };

  const estadoHistorial = { pagina: 1, tamano: 10, totalPaginas: 1 };

  async function cargarCatalogos() {
    const catalogos = await FarmaciaApi.catalogos.todos();
    const categorias = catalogos.categorias || [];
    const select = document.getElementById("filtroCategoria");
    // El value es el codigo (la categoria limpia); el nombre trae el conteo.
    categorias.forEach(c => {
      select.insertAdjacentHTML("beforeend", `<option value="${escapa(c.codigo)}">${escapa(c.nombre)}</option>`);
    });

    const selectNuevo = document.getElementById("nuevoCategoria");
    if (selectNuevo) {
      selectNuevo.innerHTML = categorias
        .map(c => `<option value="${escapa(c.codigo)}">${escapa(c.codigo)}</option>`).join("");
    }
  }

  async function cargarKpis() {
    try {
      const kpis = await FarmaciaApi.dashboard.kpis();
      document.getElementById("kpiTotal").textContent = formateaNumero(kpis.totalMedicamentos);
      document.getElementById("kpiVigentes").textContent = formateaNumero(kpis.piezasVigentes);
      document.getElementById("kpiBajoMinimo").textContent = formateaNumero(kpis.medicamentosBajoMinimo);
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
      const resultado = await FarmaciaApi.inventario.listar(params);
      estado.totalPaginas = resultado.totalPaginas || 1;

      tbody.innerHTML = resultado.datos.map(a => `
        <tr>
          <td class="text-nowrap"><span style="font-family:monospace">${escapa(a.codigo)}</span></td>
          <td>${escapa(a.nombre)}</td>
          <td>${escapa(a.categoria)}</td>
          <td class="text-right">${formateaNumero(a.total)}</td>
          <td><span class="badge ${CLASE_NIVEL[a.nivel] || "badge-secondary"}">${escapa(a.nivel)}</span></td>
          <td><span class="badge ${CLASE_ALERTA[a.alertaCaducidad] || "badge-secondary"}">${escapa(a.alertaCaducidad)}</span></td>
          <td class="acciones-fila">
            <button type="button" class="btn btn-sm btn-outline-primary btn-lotes"
                    data-id="${a.id}" data-codigo="${escapa(a.codigo)}" data-nombre="${escapa(a.nombre)}"
                    title="Ver lotes, caducidad y movimientos">
              <i class="typcn typcn-time"></i> Lotes
            </button>
          </td>
        </tr>`).join("") || `<tr><td colspan="7">Sin resultados</td></tr>`;

      document.getElementById("resumenPagina").textContent =
        `Página ${estado.pagina} de ${estado.totalPaginas} — ${formateaNumero(resultado.total)} medicamentos`;
      document.getElementById("btnPagAnterior").disabled = estado.pagina <= 1;
      document.getElementById("btnPagSiguiente").disabled = estado.pagina >= estado.totalPaginas;
      document.getElementById("estadoCarga").textContent = "";

      tbody.querySelectorAll(".btn-lotes").forEach(btn => {
        btn.addEventListener("click", () => abrirModalLotes(
          parseInt(btn.dataset.id, 10), btn.dataset.codigo, btn.dataset.nombre));
      });
    } catch (err) {
      document.getElementById("estadoCarga").textContent =
        "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      document.getElementById("estadoCarga").classList.add("text-danger");
    }
  }

  // -------------------------------------------------------- modal de lotes
  async function abrirModalLotes(articuloId, codigo, nombre) {
    estado.articuloEnTurno = { id: articuloId, codigo, nombre };
    document.getElementById("lotesCodigo").textContent = codigo;
    document.getElementById("lotesNombre").textContent = nombre;
    document.getElementById("formSurtir").reset();
    document.getElementById("formNuevoLote").reset();
    document.getElementById("alertaSurtir").classList.add("d-none");
    document.getElementById("alertaLote").classList.add("d-none");
    document.getElementById("histTipo").value = "";
    estadoHistorial.pagina = 1;
    actualizarCamposSurtir();

    $('#tabsLote a[href="#tabMovimientos"]').tab("show");

    $("#modalLotes").modal("show");
    await Promise.all([cargarLotes(), cargarMovimientosDeLaClave()]);
  }

  async function cargarLotes() {
    const tbody = document.getElementById("tablaLotes");
    tbody.innerHTML = `<tr><td colspan="5">Cargando...</td></tr>`;
    try {
      const resultado = await FarmaciaApi.lotes.listar({ articuloId: estado.articuloEnTurno.id, tamano: 200 });
      tbody.innerHTML = resultado.datos.map(l => `
        <tr>
          <td>${escapa(l.lote)}</td>
          <td>${formateaFecha(l.caducidad)}</td>
          <td><span class="badge ${CLASE_ESTADO_LOTE[l.estadoCaducidad] || "badge-secondary"}">${escapa(l.estadoCaducidad)}</span></td>
          <td class="text-right font-weight-bold">${formateaNumero(l.cantidad)}</td>
          <td class="small text-muted">${escapa(l.notas) || "—"}</td>
        </tr>`).join("") || `<tr><td colspan="5">Este medicamento todavía no tiene lotes registrados. Da de alta el primero abajo.</td></tr>`;
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="5" class="text-danger">No se pudieron cargar los lotes: ${escapa(err.message)}</td></tr>`;
    }
  }

  async function cargarMovimientosDeLaClave() {
    const tbody = document.getElementById("tablaMovimientosLote");
    tbody.innerHTML = `<tr><td colspan="7">Cargando...</td></tr>`;
    try {
      const resultado = await FarmaciaApi.movimientos.listar({
        // Por articuloId y no por clave: una misma clave del cuadro basico
        // tiene varias presentaciones, y la pestana de lotes muestra solo una.
        articuloId: estado.articuloEnTurno.id,
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
          <td>${escapa(m.areaServicio) || "—"}</td>
          <td>${escapa(m.responsable) || "—"}</td>
        </tr>`).join("") || `<tr><td colspan="7">Sin movimientos con este filtro.</td></tr>`;

      document.getElementById("histResumen").textContent =
        `Página ${estadoHistorial.pagina} de ${estadoHistorial.totalPaginas} — ${formateaNumero(resultado.total)} movimientos`;
      document.getElementById("histAnterior").disabled = estadoHistorial.pagina <= 1;
      document.getElementById("histSiguiente").disabled = estadoHistorial.pagina >= estadoHistorial.totalPaginas;
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="7" class="text-danger">No se pudo cargar la bitácora: ${escapa(err.message)}</td></tr>`;
    }
  }

  function actualizarCamposSurtir() {
    const tipo = document.getElementById("surtirTipo").value;
    const muestraDatosPaciente = TIPOS_CON_DATOS_PACIENTE.includes(tipo);
    document.getElementById("grupoDatosPaciente").classList.toggle("d-none", !muestraDatosPaciente);
    document.getElementById("grupoDatosClinicos").classList.toggle("d-none", !muestraDatosPaciente);
  }

  async function surtir(ev) {
    ev.preventDefault();
    const alerta = document.getElementById("alertaSurtir");
    alerta.classList.add("d-none");

    const tipo = document.getElementById("surtirTipo").value;
    const conDatosPaciente = TIPOS_CON_DATOS_PACIENTE.includes(tipo);
    const entrada = {
      codigo: estado.articuloEnTurno.codigo,
      cantidad: parseFloat(document.getElementById("surtirCantidad").value),
      tipo,
      pacienteNombre: conDatosPaciente ? (document.getElementById("surtirPaciente").value.trim() || null) : null,
      pacienteExpediente: conDatosPaciente ? (document.getElementById("surtirExpediente").value.trim() || null) : null,
      diagnostico: conDatosPaciente ? (document.getElementById("surtirDiagnostico").value.trim() || null) : null,
      cie: conDatosPaciente ? (document.getElementById("surtirCie").value.trim() || null) : null,
      areaServicio: conDatosPaciente ? (document.getElementById("surtirAreaServicio").value.trim() || null) : null,
      responsable: document.getElementById("surtirResponsable").value.trim() || null,
      observaciones: document.getElementById("surtirObservaciones").value.trim() || null
    };

    try {
      const resultado = await FarmaciaApi.movimientos.surtir(entrada);
      const detalle = resultado.lineas
        .map(l => `${formateaNumero(l.cantidad)} del lote ${escapa(l.lote)}${l.caducidad ? " (caduca " + formateaFecha(l.caducidad) + ")" : ""}`)
        .join("; ");
      alerta.textContent = `Registrado (${escapa(tipo)}): ${detalle}.`;
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
      codigo: estado.articuloEnTurno.codigo,
      lote: document.getElementById("loteNumero").value.trim(),
      caducidad: document.getElementById("loteCaducidad").value || null,
      cantidadInicial: parseFloat(document.getElementById("loteCantidad").value) || 0
    };

    try {
      await FarmaciaApi.lotes.guardar(entrada);
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

  async function guardarNuevoMedicamento(ev) {
    ev.preventDefault();
    const alerta = document.getElementById("alertaNuevoMedicamento");
    alerta.classList.add("d-none");

    const entrada = {
      codigo: document.getElementById("nuevoCodigo").value.trim(),
      nombre: document.getElementById("nuevoNombre").value.trim(),
      categoria: document.getElementById("nuevoCategoria").value,
      presentacion: document.getElementById("nuevoPresentacion").value.trim() || null,
      stockMinimo: parseFloat(document.getElementById("nuevoStockMinimo").value) || 0,
      stockMaximo: document.getElementById("nuevoStockMaximo").value
        ? parseFloat(document.getElementById("nuevoStockMaximo").value) : null,
      activo: true
    };

    try {
      await FarmaciaApi.inventario.guardar(entrada);
      alerta.textContent = "Medicamento guardado.";
      alerta.classList.remove("d-none", "alert-danger");
      alerta.classList.add("alert-success");
      document.getElementById("formNuevoMedicamento").reset();
      await Promise.all([cargarTabla(), cargarKpis()]);
      setTimeout(() => $("#modalNuevoMedicamento").modal("hide"), 800);
    } catch (err) {
      alerta.textContent = err.message;
      alerta.classList.remove("d-none", "alert-success");
      alerta.classList.add("alert-danger");
    }
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
    window.location.href = `${window.MTTO_CONFIG.API_BASE_URL}/farmacia/exportar?${params}`;
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
    document.getElementById("btnNuevoMedicamento").addEventListener("click", () => {
      document.getElementById("formNuevoMedicamento").reset();
      document.getElementById("alertaNuevoMedicamento").classList.add("d-none");
      $("#modalNuevoMedicamento").modal("show");
    });
    document.getElementById("formNuevoMedicamento").addEventListener("submit", guardarNuevoMedicamento);

    document.getElementById("formSurtir").addEventListener("submit", surtir);
    document.getElementById("surtirTipo").addEventListener("change", actualizarCamposSurtir);
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
