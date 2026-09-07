(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const formateaFecha = (iso) => iso ? new Date(iso).toLocaleDateString("es-MX") : "—";

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  const CLASE_ESTADO = {
    "Pendiente": "badge-secondary",
    "Parcial": "badge-warning",
    "Completa": "badge-success",
    "Sin detalle": "badge-dark"
  };

  const estado = { detalleReceta: [], codigos: {}, pagina: 1, tamano: 15, totalPaginas: 1, recetaEnTurno: null };

  async function cargarCodigos() {
    const inventario = await FarmaciaApi.inventario.listar({ tamano: 500 });
    estado.codigos = Object.fromEntries(inventario.datos.map(a => [a.codigo, a]));
    document.getElementById("listaCodigosReceta").innerHTML = inventario.datos
      .map(a => `<option value="${escapa(a.codigo)}" label="${escapa(a.nombre)}"></option>`).join("");
  }

  function agregarDetalle() {
    const codigo = document.getElementById("detalleCodigo").value.trim();
    const cantidad = parseFloat(document.getElementById("detalleCantidad").value);

    if (!codigo || !estado.codigos[codigo]) {
      alert(`No existe un medicamento activo con código "${codigo}".`);
      return;
    }
    if (!cantidad || cantidad <= 0) {
      alert("La cantidad prescrita debe ser mayor a cero.");
      return;
    }
    if (estado.detalleReceta.some(d => d.codigo === codigo)) {
      alert("Ese código ya está en el detalle de esta receta.");
      return;
    }

    estado.detalleReceta.push({ codigo, cantidad, nombre: estado.codigos[codigo].nombre });
    renderizarDetalleReceta();

    document.getElementById("detalleCodigo").value = "";
    document.getElementById("detalleCantidad").value = "";
    document.getElementById("detalleCodigo").focus();
  }

  function renderizarDetalleReceta() {
    const wrap = document.getElementById("wrapDetalleReceta");
    const tbody = document.getElementById("tablaDetalleReceta");
    wrap.style.display = estado.detalleReceta.length ? "" : "none";
    tbody.innerHTML = estado.detalleReceta.map((d, idx) => `
      <tr>
        <td><span style="font-family:monospace">${escapa(d.codigo)}</span> <span class="small text-muted">${escapa(d.nombre)}</span></td>
        <td class="text-right">${formateaNumero(d.cantidad)}</td>
        <td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger" onclick="window.__farmQuitarDetalleReceta(${idx})">×</button></td>
      </tr>`).join("");
  }
  window.__farmQuitarDetalleReceta = (idx) => { estado.detalleReceta.splice(idx, 1); renderizarDetalleReceta(); };

  async function guardarReceta(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorReceta");
    const exitoBox = document.getElementById("exitoReceta");
    errorBox.classList.add("d-none");
    exitoBox.classList.add("d-none");

    if (estado.detalleReceta.length === 0) {
      errorBox.textContent = "Agrega al menos una clave al detalle de la receta.";
      errorBox.classList.remove("d-none");
      return;
    }

    const entrada = {
      medico: document.getElementById("recetaMedico").value.trim(),
      pacienteNombre: document.getElementById("recetaPaciente").value.trim(),
      pacienteExpediente: document.getElementById("recetaExpediente").value.trim() || null,
      fecha: document.getElementById("recetaFecha").value || null,
      observaciones: document.getElementById("recetaObservaciones").value.trim() || null,
      detalle: estado.detalleReceta.map(d => ({ codigo: d.codigo, cantidad: d.cantidad }))
    };

    try {
      const resultado = await FarmaciaApi.recetas.guardar(entrada);
      exitoBox.textContent = `Receta ${resultado.receta.folio} guardada con ${entrada.detalle.length} clave(s).`;
      exitoBox.classList.remove("d-none");
      document.getElementById("formReceta").reset();
      estado.detalleReceta = [];
      renderizarDetalleReceta();
      estado.pagina = 1;
      await Promise.all([cargarRecetas(), cargarKpis()]);
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  async function cargarRecetas() {
    const tbody = document.getElementById("tablaRecetas");
    tbody.innerHTML = `<tr><td colspan="8">Cargando...</td></tr>`;
    try {
      const resultado = await FarmaciaApi.recetas.listar({
        expediente: document.getElementById("filtroExpediente").value.trim(),
        estado: document.getElementById("filtroEstado").value,
        pagina: estado.pagina,
        tamano: estado.tamano
      });
      estado.totalPaginas = resultado.totalPaginas || 1;

      tbody.innerHTML = resultado.datos.map(r => `
        <tr>
          <td>${escapa(r.folio)}</td>
          <td>${formateaFecha(r.fecha)}</td>
          <td>${escapa(r.pacienteNombre)}</td>
          <td>${escapa(r.medico)}</td>
          <td class="text-right">${r.totalPrescrito === null || r.totalPrescrito === undefined ? '<span class="text-muted" title="El sistema anterior no capturaba lo prescrito">—</span>' : formateaNumero(r.totalPrescrito)}</td>
          <td class="text-right">${formateaNumero(r.totalEntregado)}</td>
          <td><span class="badge ${CLASE_ESTADO[r.estado] || "badge-secondary"}">${escapa(r.estado)}</span></td>
          <td>
            <button type="button" class="btn btn-sm btn-outline-primary btn-detalle" data-id="${r.id}">
              <i class="typcn typcn-eye-outline"></i> Ver / dispensar
            </button>
          </td>
        </tr>`).join("") || `<tr><td colspan="8">Sin recetas con este filtro.</td></tr>`;

      document.getElementById("resumenPagina").textContent =
        `Página ${estado.pagina} de ${estado.totalPaginas} — ${formateaNumero(resultado.total)} recetas`;
      document.getElementById("btnPagAnterior").disabled = estado.pagina <= 1;
      document.getElementById("btnPagSiguiente").disabled = estado.pagina >= estado.totalPaginas;
      document.getElementById("estadoCarga").textContent = "";

      tbody.querySelectorAll(".btn-detalle").forEach(btn => {
        btn.addEventListener("click", () => abrirModalReceta(parseInt(btn.dataset.id, 10)));
      });
    } catch (err) {
      document.getElementById("estadoCarga").textContent =
        "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      document.getElementById("estadoCarga").classList.add("text-danger");
    }
  }

  async function cargarKpis() {
    try {
      const datos = await FarmaciaApi.recetas.indicadorAbasto(1);
      const actual = datos[datos.length - 1];
      if (!actual) {
        document.getElementById("kpiRecetas").textContent = "0";
        document.getElementById("kpiAbasto").textContent = "—";
        document.getElementById("kpiFaltante").textContent = "0";
        document.getElementById("kpiPrescritoEntregado").textContent = "—";
        return;
      }
      // porcentajeAbasto y totalPrescrito llegan en null cuando la receta
      // viene del sistema anterior: ahi solo se registro lo entregado, y
      // mostrar 100% seria falso por construccion.
      document.getElementById("kpiRecetas").textContent = formateaNumero(actual.totalRecetas);
      document.getElementById("kpiAbasto").textContent =
        actual.porcentajeAbasto === null || actual.porcentajeAbasto === undefined
          ? "—" : `${actual.porcentajeAbasto}%`;
      document.getElementById("kpiFaltante").textContent =
        actual.lineasConFaltante === null || actual.lineasConFaltante === undefined
          ? "—" : formateaNumero(actual.lineasConFaltante);
      document.getElementById("kpiPrescritoEntregado").textContent =
        `${actual.totalPrescrito === null || actual.totalPrescrito === undefined ? "—" : formateaNumero(actual.totalPrescrito)}`
        + ` / ${formateaNumero(actual.totalEntregado)}`;
    } catch { /* los KPIs son un extra */ }
  }

  async function abrirModalReceta(recetaId) {
    estado.recetaEnTurno = recetaId;
    document.getElementById("alertaDispensar").classList.add("d-none");
    $("#modalReceta").modal("show");
    await cargarDetalleModal();
  }

  async function cargarDetalleModal() {
    const tbody = document.getElementById("tablaDetalleModal");
    tbody.innerHTML = `<tr><td colspan="7">Cargando...</td></tr>`;
    try {
      const resultado = await FarmaciaApi.recetas.obtener(estado.recetaEnTurno);
      document.getElementById("modalFolio").textContent = resultado.receta.folio;
      document.getElementById("modalPaciente").textContent = resultado.receta.pacienteNombre;

      tbody.innerHTML = resultado.detalle.map(d => `
        <tr>
          <td><span style="font-family:monospace">${escapa(d.codigo)}</span></td>
          <td>${escapa(d.articulo)}</td>
          <td class="text-right">${d.cantidadPrescrita === null || d.cantidadPrescrita === undefined ? '<span class="text-muted">—</span>' : formateaNumero(d.cantidadPrescrita)}</td>
          <td class="text-right">${formateaNumero(d.cantidadEntregada)}</td>
          <td class="text-right">${d.pendiente > 0 ? `<span class="text-danger">${formateaNumero(d.pendiente)}</span>` : "0"}</td>
          <td><span class="badge ${CLASE_ESTADO[d.estadoLinea] || "badge-secondary"}">${escapa(d.estadoLinea)}</span></td>
          <td>
            ${d.pendiente > 0
              ? `<button type="button" class="btn btn-sm btn-primary btn-dispensar" data-id="${d.id}">Dispensar</button>`
              : `<span class="text-success"><i class="typcn typcn-tick"></i></span>`}
          </td>
        </tr>`).join("") || `<tr><td colspan="7">Sin detalle.</td></tr>`;

      tbody.querySelectorAll(".btn-dispensar").forEach(btn => {
        btn.addEventListener("click", () => dispensarLinea(parseInt(btn.dataset.id, 10)));
      });
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="7" class="text-danger">No se pudo cargar la receta: ${escapa(err.message)}</td></tr>`;
    }
  }

  async function dispensarLinea(recetaDetalleId) {
    const alerta = document.getElementById("alertaDispensar");
    alerta.classList.add("d-none");
    try {
      const resultado = await FarmaciaApi.recetas.dispensarLinea(recetaDetalleId, {
        responsable: document.getElementById("dispensarResponsable").value.trim() || null
      });
      if (resultado.faltanteEstaVez > 0) {
        alerta.textContent = `Se entregaron ${formateaNumero(resultado.entregadoEstaVez)} de ${escapa(resultado.linea.codigo)}. ` +
          `Faltan ${formateaNumero(resultado.faltanteEstaVez)} por no haber existencia suficiente.`;
        alerta.classList.remove("d-none", "alert-success");
        alerta.classList.add("alert-warning");
      } else {
        alerta.textContent = `Se entregaron ${formateaNumero(resultado.entregadoEstaVez)} de ${escapa(resultado.linea.codigo)} completo.`;
        alerta.classList.remove("d-none", "alert-warning");
        alerta.classList.add("alert-success");
      }
      await Promise.all([cargarDetalleModal(), cargarRecetas(), cargarKpis()]);
    } catch (err) {
      alerta.textContent = err.message;
      alerta.classList.remove("d-none", "alert-success", "alert-warning");
      alerta.classList.add("alert-danger");
    }
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await cargarCodigos();
    await Promise.all([cargarRecetas(), cargarKpis()]);

    document.getElementById("btnAgregarDetalle").addEventListener("click", agregarDetalle);
    document.getElementById("formReceta").addEventListener("submit", guardarReceta);

    document.getElementById("btnBuscarRecetas").addEventListener("click", () => {
      estado.pagina = 1;
      cargarRecetas();
    });
    document.getElementById("btnPagAnterior").addEventListener("click", () => {
      if (estado.pagina > 1) { estado.pagina--; cargarRecetas(); }
    });
    document.getElementById("btnPagSiguiente").addEventListener("click", () => {
      if (estado.pagina < estado.totalPaginas) { estado.pagina++; cargarRecetas(); }
    });
  });
})();
