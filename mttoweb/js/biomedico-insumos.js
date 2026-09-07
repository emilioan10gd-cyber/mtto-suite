(function () {
  let insumosCache = [];

  function escapa(t) {
    const d = document.createElement("div");
    d.textContent = t ?? "";
    return d.innerHTML;
  }

  function formateaNumero(n) {
    return new Intl.NumberFormat("es-MX", { maximumFractionDigits: 3 }).format(n || 0);
  }

  async function cargarCatalogos() {
    const catalogos = await BiomedicoApi.catalogos.todos();
    const fCategoria = document.getElementById("fCategoria");
    const listaCategoriasInsumo = document.getElementById("listaCategoriasInsumo");
    catalogos.categoriasInsumo.forEach(c => {
      fCategoria.insertAdjacentHTML("beforeend", `<option value="${escapa(c.nombre)}">${escapa(c.nombre)}</option>`);
      listaCategoriasInsumo.insertAdjacentHTML("beforeend", `<option value="${escapa(c.nombre)}">`);
    });
  }

  function filtros() {
    return {
      q: document.getElementById("fBusqueda").value.trim() || undefined,
      categoria: document.getElementById("fCategoria").value || undefined,
      soloBajoMinimo: document.getElementById("fBajoMinimo").checked || undefined
    };
  }

  async function cargarTabla() {
    const estadoEl = document.getElementById("estadoCarga");
    try {
      insumosCache = await BiomedicoApi.insumos.listar(filtros());
      const tbody = document.getElementById("tablaInsumos");
      tbody.innerHTML = insumosCache.map(i => `
        <tr>
          <td><span style="font-family:monospace">${escapa(i.clave)}</span></td>
          <td>${escapa(i.nombre)}</td>
          <td>${escapa(i.categoria)}</td>
          <td class="${i.existenciaTotal < i.stockMinimo ? "text-danger font-weight-bold" : ""}">${formateaNumero(i.existenciaTotal)} ${escapa(i.unidad)}</td>
          <td>${formateaNumero(i.stockMinimo)}</td>
          <td>${i.caducidadProxima ? new Date(i.caducidadProxima).toLocaleDateString("es-MX") : "—"}</td>
          <td class="text-right text-nowrap">
            <button type="button" class="btn btn-sm btn-outline-secondary btn-lotes" data-id="${i.id}" title="Ver lotes"><i class="typcn typcn-th-list"></i></button>
            <button type="button" class="btn btn-sm btn-outline-primary btn-editar" data-clave="${escapa(i.clave)}" title="Editar"><i class="typcn typcn-edit"></i></button>
          </td>
        </tr>`).join("") || `<tr><td colspan="7">Sin insumos que coincidan con el filtro.</td></tr>`;

      tbody.querySelectorAll(".btn-lotes").forEach(b => b.addEventListener("click", () => verLotes(parseInt(b.dataset.id, 10))));
      tbody.querySelectorAll(".btn-editar").forEach(b => b.addEventListener("click", () => abrirEdicion(b.dataset.clave)));

      const listaClaves = document.getElementById("listaClaves");
      listaClaves.innerHTML = insumosCache.map(i => `<option value="${escapa(i.clave)}">${escapa(i.nombre)}</option>`).join("");

      estadoEl.textContent = "";
    } catch (err) {
      estadoEl.textContent = "No se pudo conectar con la API (" + err.message + ").";
      estadoEl.classList.add("text-danger");
    }
  }

  async function verLotes(insumoId) {
    const lotes = await BiomedicoApi.insumos.lotes(insumoId);
    document.getElementById("tablaLotes").innerHTML = lotes.map(l => `
      <tr>
        <td>${escapa(l.lote)}</td>
        <td>${l.caducidad ? new Date(l.caducidad).toLocaleDateString("es-MX") : "Sin caducidad"}</td>
        <td>${formateaNumero(l.existencia)}</td>
        <td>${escapa(l.estadoCaducidad)}</td>
      </tr>`).join("") || `<tr><td colspan="4">Sin lotes registrados.</td></tr>`;
    $("#modalLotes").modal("show");
  }

  function limpiarFormInsumo() {
    document.getElementById("errorInsumo").classList.add("d-none");
    ["fClave", "fNombre", "fCategoriaInput", "fStockMaximo"].forEach(id => document.getElementById(id).value = "");
    document.getElementById("fUnidad").value = "Pieza";
    document.getElementById("fStockMinimo").value = "0";
    document.getElementById("fClave").disabled = false;
  }

  function abrirNuevo() {
    limpiarFormInsumo();
    $("#modalInsumo").modal("show");
  }

  function abrirEdicion(clave) {
    limpiarFormInsumo();
    const i = insumosCache.find(x => x.clave === clave);
    if (!i) return;
    document.getElementById("fClave").value = i.clave;
    document.getElementById("fClave").disabled = true;
    document.getElementById("fNombre").value = i.nombre || "";
    document.getElementById("fCategoriaInput").value = i.categoria || "";
    document.getElementById("fUnidad").value = i.unidad || "Pieza";
    document.getElementById("fStockMinimo").value = i.stockMinimo || 0;
    document.getElementById("fStockMaximo").value = i.stockMaximo || "";
    $("#modalInsumo").modal("show");
  }

  async function guardarInsumo() {
    const errorEl = document.getElementById("errorInsumo");
    errorEl.classList.add("d-none");
    const clave = document.getElementById("fClave").value.trim();
    const nombre = document.getElementById("fNombre").value.trim();
    if (!clave || !nombre) {
      errorEl.textContent = "Clave y nombre son obligatorios.";
      errorEl.classList.remove("d-none");
      return;
    }
    const entrada = {
      clave, nombre,
      categoria: document.getElementById("fCategoriaInput").value.trim() || null,
      unidad: document.getElementById("fUnidad").value.trim() || "Pieza",
      stockMinimo: parseFloat(document.getElementById("fStockMinimo").value) || 0,
      stockMaximo: document.getElementById("fStockMaximo").value ? parseFloat(document.getElementById("fStockMaximo").value) : null
    };
    try {
      await BiomedicoApi.insumos.guardar(entrada);
      $("#modalInsumo").modal("hide");
      await cargarTabla();
    } catch (err) {
      errorEl.textContent = err.message;
      errorEl.classList.remove("d-none");
    }
  }

  function abrirMovimiento() {
    document.getElementById("errorMovimiento").classList.add("d-none");
    ["mClave", "mCantidad", "mLote", "mCaducidad", "mResponsable", "mObservaciones"].forEach(id => document.getElementById(id).value = "");
    document.getElementById("mTipo").value = "Entrada";
    document.getElementById("mContieneLote").checked = false;
    document.getElementById("mTieneFecha").checked = false;
    document.getElementById("seccionLote").style.display = "none";
    document.getElementById("seccionCaducidad").style.display = "none";
    $("#modalMovimiento").modal("show");
  }

  function actualizarVisibilidadLote() {
    const seccionLote = document.getElementById("seccionLote");
    const mContieneLote = document.getElementById("mContieneLote").checked;
    seccionLote.style.display = mContieneLote ? "block" : "none";
    if (!mContieneLote) {
      document.getElementById("mTieneFecha").checked = false;
      document.getElementById("seccionCaducidad").style.display = "none";
    }
  }

  function actualizarVisibilidadCaducidad() {
    const seccionCaducidad = document.getElementById("seccionCaducidad");
    const mTieneFecha = document.getElementById("mTieneFecha").checked;
    seccionCaducidad.style.display = mTieneFecha ? "block" : "none";
  }

  async function guardarMovimiento() {
    const errorEl = document.getElementById("errorMovimiento");
    errorEl.classList.add("d-none");
    const claveInsumo = document.getElementById("mClave").value.trim();
    const cantidad = parseFloat(document.getElementById("mCantidad").value);
    if (!claveInsumo || !cantidad || cantidad <= 0) {
      errorEl.textContent = "Clave y cantidad (mayor a cero) son obligatorios.";
      errorEl.classList.remove("d-none");
      return;
    }
    const entrada = {
      claveInsumo,
      tipo: document.getElementById("mTipo").value,
      cantidad,
      lote: document.getElementById("mLote").value.trim() || null,
      caducidad: document.getElementById("mCaducidad").value || null,
      responsable: document.getElementById("mResponsable").value.trim() || null,
      observaciones: document.getElementById("mObservaciones").value.trim() || null
    };
    try {
      await BiomedicoApi.movimientos.registrar(entrada);
      $("#modalMovimiento").modal("hide");
      await cargarTabla();
    } catch (err) {
      errorEl.textContent = err.message;
      errorEl.classList.remove("d-none");
    }
  }

  async function iniciar() {
    await cargarCatalogos();
    await cargarTabla();

    document.getElementById("btnNuevo").addEventListener("click", abrirNuevo);
    document.getElementById("btnGuardarInsumo").addEventListener("click", guardarInsumo);
    document.getElementById("btnMovimiento").addEventListener("click", abrirMovimiento);
    document.getElementById("btnGuardarMovimiento").addEventListener("click", guardarMovimiento);

    document.getElementById("mContieneLote").addEventListener("change", actualizarVisibilidadLote);
    document.getElementById("mTieneFecha").addEventListener("change", actualizarVisibilidadCaducidad);

    let temporizador;
    document.getElementById("fBusqueda").addEventListener("input", () => {
      clearTimeout(temporizador);
      temporizador = setTimeout(cargarTabla, 300);
    });
    document.getElementById("fCategoria").addEventListener("change", cargarTabla);
    document.getElementById("fBajoMinimo").addEventListener("change", cargarTabla);
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", iniciar);
  else setTimeout(iniciar, 100);
})();
