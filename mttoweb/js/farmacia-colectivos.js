(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const formateaFecha = (iso) => iso ? new Date(iso).toLocaleDateString("es-MX") : "—";

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  const estado = { codigos: {}, detalleColectivo: [] };

  async function cargarCodigos() {
    const inventario = await FarmaciaApi.inventario.listar({ tamano: 500 });
    estado.codigos = Object.fromEntries(inventario.datos.map(a => [a.codigo, a]));
    document.getElementById("listaCodigosColectivo").innerHTML = inventario.datos
      .map(a => `<option value="${escapa(a.codigo)}" label="${escapa(a.nombre)}"></option>`).join("");
  }

  async function cargarServicios() {
    const servicios = await FarmaciaApi.colectivos.servicios(true);
    const opciones = servicios.map(s => `<option value="${escapa(s.nombre)}">${escapa(s.nombre)}</option>`).join("");
    document.getElementById("cuadroServicio").innerHTML = opciones;
    document.getElementById("colectivoServicio").innerHTML = opciones;
    document.getElementById("filtroServicioHistorial").insertAdjacentHTML("beforeend", opciones);
  }

  async function cargarCuadroServicio() {
    const servicio = document.getElementById("cuadroServicio").value;
    const tbody = document.getElementById("tablaCuadroServicio");
    if (!servicio) { tbody.innerHTML = `<tr><td colspan="4">Selecciona un servicio</td></tr>`; return; }
    tbody.innerHTML = `<tr><td colspan="4">Cargando...</td></tr>`;
    try {
      const claves = await FarmaciaApi.colectivos.clavesDelServicio(servicio);
      tbody.innerHTML = claves.map(c => `
        <tr>
          <td><span style="font-family:monospace">${escapa(c.codigo)}</span></td>
          <td>${escapa(c.articulo)}</td>
          <td class="text-right">${formateaNumero(c.cantidadPeriodo)}</td>
          <td>${escapa(c.periodo)}</td>
        </tr>`).join("") || `<tr><td colspan="4">Este servicio todavía no tiene claves autorizadas.</td></tr>`;
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="4" class="text-danger">${escapa(err.message)}</td></tr>`;
    }
  }

  async function asignarClave(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorClave");
    const exitoBox = document.getElementById("exitoClave");
    errorBox.classList.add("d-none");
    exitoBox.classList.add("d-none");

    const entrada = {
      servicio: document.getElementById("cuadroServicio").value,
      codigo: document.getElementById("claveCodigo").value.trim(),
      cantidadPeriodo: parseFloat(document.getElementById("claveCantidad").value),
      periodo: document.getElementById("clavePeriodo").value
    };

    if (!entrada.servicio) {
      errorBox.textContent = "Elige un servicio primero.";
      errorBox.classList.remove("d-none");
      return;
    }

    try {
      await FarmaciaApi.colectivos.asignarClave(entrada);
      exitoBox.textContent = `"${entrada.codigo}" autorizado para ${entrada.servicio}.`;
      exitoBox.classList.remove("d-none");
      document.getElementById("claveCodigo").value = "";
      document.getElementById("claveCantidad").value = "";
      await cargarCuadroServicio();
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  function agregarDetalle() {
    const codigo = document.getElementById("detalleCodigo").value.trim();
    const cantidad = parseFloat(document.getElementById("detalleCantidad").value);

    if (!codigo || !estado.codigos[codigo]) {
      alert(`No existe un medicamento activo con código "${codigo}".`);
      return;
    }
    if (!cantidad || cantidad <= 0) {
      alert("La cantidad debe ser mayor a cero.");
      return;
    }
    if (estado.detalleColectivo.some(d => d.codigo === codigo)) {
      alert("Ese código ya está en el detalle de este colectivo.");
      return;
    }

    estado.detalleColectivo.push({ codigo, cantidad, nombre: estado.codigos[codigo].nombre });
    renderizarDetalleColectivo();

    document.getElementById("detalleCodigo").value = "";
    document.getElementById("detalleCantidad").value = "";
    document.getElementById("detalleCodigo").focus();
  }

  function renderizarDetalleColectivo() {
    const wrap = document.getElementById("wrapDetalleColectivo");
    const tbody = document.getElementById("tablaDetalleColectivo");
    wrap.style.display = estado.detalleColectivo.length ? "" : "none";
    tbody.innerHTML = estado.detalleColectivo.map((d, idx) => `
      <tr>
        <td><span style="font-family:monospace">${escapa(d.codigo)}</span> <span class="small text-muted">${escapa(d.nombre)}</span></td>
        <td class="text-right">${formateaNumero(d.cantidad)}</td>
        <td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger" onclick="window.__farmQuitarDetalleColectivo(${idx})">×</button></td>
      </tr>`).join("");
  }
  window.__farmQuitarDetalleColectivo = (idx) => { estado.detalleColectivo.splice(idx, 1); renderizarDetalleColectivo(); };

  async function guardarColectivo(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorColectivo");
    const exitoBox = document.getElementById("exitoColectivo");
    errorBox.classList.add("d-none");
    exitoBox.classList.add("d-none");

    if (estado.detalleColectivo.length === 0) {
      errorBox.textContent = "Agrega al menos una clave al detalle del colectivo.";
      errorBox.classList.remove("d-none");
      return;
    }

    const entrada = {
      servicio: document.getElementById("colectivoServicio").value,
      entregadoPor: document.getElementById("colectivoEntregaPor").value.trim(),
      recibidoPor: document.getElementById("colectivoRecibePor").value.trim(),
      fecha: document.getElementById("colectivoFecha").value || null,
      observaciones: document.getElementById("colectivoObservaciones").value.trim() || null,
      detalle: estado.detalleColectivo.map(d => ({ codigo: d.codigo, cantidad: d.cantidad }))
    };

    try {
      const resultado = await FarmaciaApi.colectivos.guardar(entrada);
      exitoBox.textContent = `Colectivo ${resultado.colectivo.folio} entregado a ${escapa(entrada.servicio)}.`;
      exitoBox.classList.remove("d-none");
      document.getElementById("formColectivo").reset();
      estado.detalleColectivo = [];
      renderizarDetalleColectivo();
      await cargarHistorial();
    } catch (err) {
      // El SP es todo-o-nada: si una clave no alcanza, ninguna se entrega.
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  async function cargarHistorial() {
    const tbody = document.getElementById("tablaHistorial");
    tbody.innerHTML = `<tr><td colspan="7">Cargando...</td></tr>`;
    try {
      const resultado = await FarmaciaApi.colectivos.listar({
        servicio: document.getElementById("filtroServicioHistorial").value || null,
        tamano: 30
      });
      tbody.innerHTML = resultado.datos.map(c => `
        <tr>
          <td>${escapa(c.folio)}</td>
          <td>${formateaFecha(c.fecha)}</td>
          <td>${escapa(c.servicio)}</td>
          <td>${escapa(c.entregadoPor)}</td>
          <td>${escapa(c.recibidoPor)}</td>
          <td class="text-right">${formateaNumero(c.totalLineas)}</td>
          <td class="text-right">${formateaNumero(c.totalEntregado)}</td>
        </tr>`).join("") || `<tr><td colspan="7">Sin colectivos registrados todavía.</td></tr>`;
      document.getElementById("estadoCarga").textContent = "";
    } catch (err) {
      document.getElementById("estadoCarga").textContent =
        "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      document.getElementById("estadoCarga").classList.add("text-danger");
    }
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await Promise.all([cargarCodigos(), cargarServicios()]);
    await Promise.all([cargarCuadroServicio(), cargarHistorial()]);

    document.getElementById("cuadroServicio").addEventListener("change", cargarCuadroServicio);
    document.getElementById("formClave").addEventListener("submit", asignarClave);
    document.getElementById("btnAgregarDetalle").addEventListener("click", agregarDetalle);
    document.getElementById("formColectivo").addEventListener("submit", guardarColectivo);
    document.getElementById("btnBuscarHistorial").addEventListener("click", cargarHistorial);
  });
})();
