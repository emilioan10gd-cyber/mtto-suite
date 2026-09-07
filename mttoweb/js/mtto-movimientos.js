(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const formateaFecha = (iso) => new Date(iso).toLocaleString("es-MX", { dateStyle: "short", timeStyle: "short" });

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML;
  }

  let tiposPorNombre = {};

  async function cargarCatalogos() {
    const catalogos = await MttoApi.catalogos.todos();

    const selectTipo = document.getElementById("campoTipo");
    selectTipo.innerHTML = catalogos.tiposMovimiento
      .map(t => `<option value="${escapa(t.nombre)}">${escapa(t.nombre)}</option>`).join("");
    tiposPorNombre = Object.fromEntries(catalogos.tiposMovimiento.map(t => [t.nombre, t]));

    const selectDestino = document.getElementById("campoDestino");
    selectDestino.innerHTML = catalogos.almacenes
      .map(a => `<option value="${escapa(a.nombre)}">${escapa(a.nombre)}</option>`).join("");

    // Lista de artículos para el select. tamano=500 cubre holgadamente el catálogo actual.
    const inventario = await MttoApi.inventario.listar({ tamano: 500 });
    const selectArticulo = document.getElementById("campoArticulo");
    selectArticulo.insertAdjacentHTML("beforeend",
      inventario.datos.map(a => `<option value="${escapa(a.codigo)}">${escapa(a.codigo)} — ${escapa(a.nombre)} (stock: ${formateaNumero(a.stockActual)})</option>`).join(""));

    actualizarVisibilidadDestino();
  }

  function actualizarVisibilidadDestino() {
    const tipo = tiposPorNombre[document.getElementById("campoTipo").value];
    const requiereDestino = !!(tipo && tipo.requiereDestino);
    document.getElementById("grupoDestino").style.display = requiereDestino ? "" : "none";
    document.getElementById("campoDestino").required = requiereDestino;
  }

  async function cargarBitacora() {
    const tbody = document.getElementById("tablaMovimientos");
    try {
      const resultado = await MttoApi.movimientos.listar({ tamano: 15 });
      tbody.innerHTML = resultado.datos.map(m => `
        <tr>
          <td>${escapa(m.folio)}</td>
          <td>${formateaFecha(m.fecha)}</td>
          <td>${escapa(m.tipo)}</td>
          <td>${escapa(m.codigoArticulo)} — ${escapa(m.nombreArticulo)}</td>
          <td class="text-right">${formateaNumero(m.cantidad)}</td>
          <td class="text-right">${m.stockResultante != null ? formateaNumero(m.stockResultante) : "—"}</td>
          <td>${escapa(m.responsable) || "—"}</td>
        </tr>`).join("") || `<tr><td colspan="7">Sin movimientos registrados</td></tr>`;
      document.getElementById("estadoCarga").textContent = "";
    } catch (err) {
      document.getElementById("estadoCarga").textContent =
        "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      document.getElementById("estadoCarga").classList.add("text-danger");
    }
  }

  async function registrarMovimiento(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorMovimiento");
    const exitoBox = document.getElementById("exitoMovimiento");
    errorBox.classList.add("d-none");
    exitoBox.classList.add("d-none");

    const entrada = {
      codigoArticulo: document.getElementById("campoArticulo").value,
      tipo: document.getElementById("campoTipo").value,
      cantidad: parseFloat(document.getElementById("campoCantidad").value),
      responsable: document.getElementById("campoResponsable").value.trim() || null,
      observaciones: document.getElementById("campoObservaciones").value.trim() || null,
      almacenDestino: document.getElementById("campoDestino").style.display === "none" ? null : document.getElementById("campoDestino").value
    };

    try {
      const resultado = await MttoApi.movimientos.registrar(entrada);
      exitoBox.textContent = `Movimiento ${resultado.folio} registrado. Stock: ${formateaNumero(resultado.stockPrevio)} → ${formateaNumero(resultado.stockResultante)}.`;
      exitoBox.classList.remove("d-none");

      document.getElementById("campoCantidad").value = "";
      document.getElementById("campoObservaciones").value = "";

      await cargarBitacora();
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await cargarCatalogos();
    await cargarBitacora();

    document.getElementById("campoTipo").addEventListener("change", actualizarVisibilidadDestino);
    document.getElementById("formMovimiento").addEventListener("submit", registrarMovimiento);
    document.getElementById("btnExportar").addEventListener("click", exportar);
  });

  // Exporta la bitácora COMPLETA (no solo los últimos 15 que se ven en pantalla),
  // con el mismo formato que la hoja "Entradas y Salidas" del Excel original.
  function exportar() {
    window.location.href = `${window.MTTO_CONFIG.API_BASE_URL}/movimientos/exportar`;
  }
})();
