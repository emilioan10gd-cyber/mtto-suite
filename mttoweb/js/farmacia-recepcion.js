(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const formateaFecha = (iso) => iso ? new Date(iso).toLocaleDateString("es-MX") : "—";

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  const estado = { detalleCom: [], codigos: {} };

  async function cargarCodigos() {
    const inventario = await FarmaciaApi.inventario.listar({ tamano: 500 });
    estado.codigos = Object.fromEntries(inventario.datos.map(a => [a.codigo, a]));
    const opciones = inventario.datos
      .map(a => `<option value="${escapa(a.codigo)}" label="${escapa(a.nombre)}"></option>`).join("");
    document.getElementById("listaCodigosRecepcion").innerHTML = opciones;
  }

  async function cargarProveedores() {
    const catalogos = await FarmaciaApi.catalogos.todos();
    const select = document.getElementById("comProveedor");
    catalogos.proveedores.forEach(p => {
      select.insertAdjacentHTML("beforeend", `<option value="${escapa(p.nombre)}">${escapa(p.nombre)}</option>`);
    });
  }

  function agregarDetalle() {
    const codigo = document.getElementById("detalleCodigo").value.trim();
    const cantidad = parseFloat(document.getElementById("detalleCantidad").value);

    if (!codigo || !estado.codigos[codigo]) {
      alert(`No existe un medicamento activo con código "${codigo}".`);
      return;
    }
    if (!cantidad || cantidad <= 0) {
      alert("La cantidad autorizada debe ser mayor a cero.");
      return;
    }
    if (estado.detalleCom.some(d => d.codigo === codigo)) {
      alert("Ese código ya está en el detalle de esta COM.");
      return;
    }

    estado.detalleCom.push({ codigo, cantidad, nombre: estado.codigos[codigo].nombre });
    renderizarDetalleCom();

    document.getElementById("detalleCodigo").value = "";
    document.getElementById("detalleCantidad").value = "";
    document.getElementById("detalleCodigo").focus();
  }

  function renderizarDetalleCom() {
    const wrap = document.getElementById("wrapDetalleCom");
    const tbody = document.getElementById("tablaDetalleCom");
    wrap.style.display = estado.detalleCom.length ? "" : "none";
    tbody.innerHTML = estado.detalleCom.map((d, idx) => `
      <tr>
        <td><span style="font-family:monospace">${escapa(d.codigo)}</span> <span class="small text-muted">${escapa(d.nombre)}</span></td>
        <td class="text-right">${formateaNumero(d.cantidad)}</td>
        <td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger" onclick="window.__farmQuitarDetalle(${idx})">×</button></td>
      </tr>`).join("");
  }
  window.__farmQuitarDetalle = (idx) => { estado.detalleCom.splice(idx, 1); renderizarDetalleCom(); };

  async function guardarCom(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorCom");
    const exitoBox = document.getElementById("exitoCom");
    errorBox.classList.add("d-none");
    exitoBox.classList.add("d-none");

    if (estado.detalleCom.length === 0) {
      errorBox.textContent = "Agrega al menos una clave al detalle de la COM.";
      errorBox.classList.remove("d-none");
      return;
    }

    const entrada = {
      comNumero: document.getElementById("comNumero").value.trim(),
      proveedor: document.getElementById("comProveedor").value || null,
      fechaDocumento: document.getElementById("comFecha").value || null,
      observaciones: document.getElementById("comObservaciones").value.trim() || null,
      detalle: estado.detalleCom.map(d => ({ codigo: d.codigo, cantidad: d.cantidad }))
    };

    try {
      await FarmaciaApi.compras.guardar(entrada);
      exitoBox.textContent = `COM "${entrada.comNumero}" guardada con ${entrada.detalle.length} clave(s).`;
      exitoBox.classList.remove("d-none");
      document.getElementById("formCom").reset();
      estado.detalleCom = [];
      renderizarDetalleCom();
      await Promise.all([cargarAvance(), cargarComs()]);
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  async function registrarRecepcion(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorRecepcion");
    const exitoBox = document.getElementById("exitoRecepcion");
    errorBox.classList.add("d-none");
    exitoBox.classList.add("d-none");

    const entrada = {
      codigo: document.getElementById("recepcionCodigo").value.trim(),
      lote: document.getElementById("recepcionLote").value.trim(),
      caducidad: document.getElementById("recepcionCaducidad").value || null,
      cantidad: parseFloat(document.getElementById("recepcionCantidad").value),
      comNumero: document.getElementById("recepcionCom").value.trim() || null,
      responsable: document.getElementById("recepcionResponsable").value.trim() || null
    };

    try {
      await FarmaciaApi.compras.recepcion(entrada);
      exitoBox.textContent = `Recepción registrada: ${formateaNumero(entrada.cantidad)} de ${escapa(entrada.codigo)} (lote ${escapa(entrada.lote)}).`;
      exitoBox.classList.remove("d-none");
      document.getElementById("formRecepcion").reset();
      await cargarAvance();
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  function barraColor(pct) {
    if (pct >= 100) return "bg-success";
    if (pct >= 50) return "bg-warning";
    return "bg-danger";
  }

  async function cargarAvance() {
    const tbody = document.getElementById("tablaAvanceCompra");
    tbody.innerHTML = `<tr><td colspan="9">Cargando...</td></tr>`;
    try {
      const filtro = document.getElementById("filtroCom").value.trim() || null;
      const datos = await FarmaciaApi.compras.listar(filtro);
      tbody.innerHTML = datos.map(a => `
        <tr>
          <td>${escapa(a.comNumero)}</td>
          <td>${formateaFecha(a.fechaDocumento)}</td>
          <td>${escapa(a.proveedor) || "—"}</td>
          <td><span style="font-family:monospace">${escapa(a.codigo)}</span></td>
          <td>${escapa(a.articulo)}</td>
          <td class="text-right">${a.cantidadAutorizada === null || a.cantidadAutorizada === undefined ? '<span class="text-muted">—</span>' : formateaNumero(a.cantidadAutorizada)}</td>
          <td class="text-right">${formateaNumero(a.cantidadRecibida)}</td>
          <td class="text-right">${a.pendiente === null || a.pendiente === undefined ? '<span class="text-muted">—</span>' : (a.pendiente > 0 ? `<span class="text-danger">${formateaNumero(a.pendiente)}</span>` : "0")}</td>
          <td>
            ${a.porcentajeSurtido === null || a.porcentajeSurtido === undefined
              ? `<span class="small text-muted">Sin COM capturada</span>`
              : `<div class="progress" style="height: 18px;">
              <div class="progress-bar ${barraColor(a.porcentajeSurtido)}" role="progressbar"
                   style="width: ${Math.min(a.porcentajeSurtido, 100)}%;">
                ${a.porcentajeSurtido}%
              </div>
            </div>`}
          </td>
        </tr>`).join("") || `<tr><td colspan="9">Sin COM registradas todavía.</td></tr>`;
      document.getElementById("estadoCarga").textContent = "";
    } catch (err) {
      document.getElementById("estadoCarga").textContent =
        "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      document.getElementById("estadoCarga").classList.add("text-danger");
    }
  }

  async function cargarComs() {
    try {
      const datos = await FarmaciaApi.compras.listar(null);
      const nums = [...new Set(datos.map(a => a.comNumero))];
      document.getElementById("listaComs").innerHTML = nums.map(n => `<option value="${escapa(n)}"></option>`).join("");
    } catch { /* opcional */ }
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await Promise.all([cargarCodigos(), cargarProveedores(), cargarAvance(), cargarComs()]);

    document.getElementById("btnAgregarDetalle").addEventListener("click", agregarDetalle);
    document.getElementById("formCom").addEventListener("submit", guardarCom);
    document.getElementById("formRecepcion").addEventListener("submit", registrarRecepcion);
    document.getElementById("btnFiltrarCom").addEventListener("click", cargarAvance);
  });
})();
