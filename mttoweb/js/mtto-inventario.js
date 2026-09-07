(function () {
  const formateaMoneda = (n) =>
    new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" }).format(n || 0);
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML;
  }

  const CLASE_NIVEL = {
    "Sin stock": "badge-dark",
    "Bajo minimo": "badge-danger",
    "Normal": "badge-success",
    "Sobre maximo": "badge-warning"
  };

  const estado = {
    pagina: 1,
    tamano: 20,
    totalPaginas: 1,
    catalogos: null,
    edicion: null // codigo del artículo en edición, null = alta
  };

  async function cargarCatalogos() {
    estado.catalogos = await MttoApi.catalogos.todos();

    const selectCategoria = document.getElementById("filtroCategoria");
    estado.catalogos.categorias.forEach(c => {
      selectCategoria.insertAdjacentHTML("beforeend", `<option value="${escapa(c.nombre)}">${escapa(c.nombre)}</option>`);
    });

    llenarSelect("campoCategoria", estado.catalogos.categorias);
    llenarSelect("campoUnidad", estado.catalogos.unidades);
    llenarSelect("campoAlmacen", estado.catalogos.almacenes);
    llenarSelect("campoEstado", estado.catalogos.estados);
  }

  function llenarSelect(idSelect, items) {
    const select = document.getElementById(idSelect);
    select.innerHTML = items.map(i => `<option value="${escapa(i.nombre)}">${escapa(i.nombre)}</option>`).join("");
  }

  async function cargarTabla() {
    const tbody = document.getElementById("tablaInventario");
    const params = {
      q: document.getElementById("filtroTexto").value.trim(),
      categoria: document.getElementById("filtroCategoria").value,
      nivel: document.getElementById("filtroNivel").value,
      pagina: estado.pagina,
      tamano: estado.tamano
    };

    try {
      const resultado = await MttoApi.inventario.listar(params);
      estado.totalPaginas = resultado.totalPaginas || 1;

      tbody.innerHTML = resultado.datos.map(a => `
        <tr>
          <td>${escapa(a.codigo)}</td>
          <td>${escapa(a.nombre)}</td>
          <td>${escapa(a.categoria)}</td>
          <td>${escapa(a.almacen)}</td>
          <td class="text-right">${formateaNumero(a.stockActual)}</td>
          <td class="text-right">${formateaNumero(a.stockMinimo)}</td>
          <td><span class="badge ${CLASE_NIVEL[a.nivel] || "badge-secondary"}">${escapa(a.nivel)}</span></td>
          <td class="text-right">${formateaMoneda(a.valorTotal)}</td>
          <td class="text-nowrap">
            <button type="button" class="btn btn-sm btn-outline-info btn-precios" data-codigo="${escapa(a.codigo)}" title="Comparar precios de proveedores">
              <i class="typcn typcn-chart-bar"></i>
            </button>
            <button type="button" class="btn btn-sm btn-outline-primary btn-editar" data-codigo="${escapa(a.codigo)}" title="Editar">
              <i class="typcn typcn-edit"></i>
            </button>
            <button type="button" class="btn btn-sm btn-outline-danger btn-eliminar" data-codigo="${escapa(a.codigo)}" title="Eliminar">
              <i class="typcn typcn-trash"></i>
            </button>
          </td>
        </tr>`).join("") || `<tr><td colspan="9">Sin resultados</td></tr>`;

      document.getElementById("resumenPagina").textContent =
        `Página ${estado.pagina} de ${estado.totalPaginas} — ${formateaNumero(resultado.total)} artículos`;
      document.getElementById("btnPagAnterior").disabled = estado.pagina <= 1;
      document.getElementById("btnPagSiguiente").disabled = estado.pagina >= estado.totalPaginas;
      document.getElementById("estadoCarga").textContent = "";

      tbody.querySelectorAll(".btn-precios").forEach(btn => {
        btn.addEventListener("click", () => MttoPreciosModal.abrir(btn.dataset.codigo));
      });
      tbody.querySelectorAll(".btn-editar").forEach(btn => {
        btn.addEventListener("click", () => abrirModalEdicion(btn.dataset.codigo));
      });
      tbody.querySelectorAll(".btn-eliminar").forEach(btn => {
        btn.addEventListener("click", () => eliminarArticulo(btn.dataset.codigo));
      });
    } catch (err) {
      document.getElementById("estadoCarga").textContent =
        "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      document.getElementById("estadoCarga").classList.add("text-danger");
    }
  }

  async function eliminarArticulo(codigo) {
    if (!confirm(`¿Eliminar el artículo ${codigo}? Solo se puede eliminar si nunca tuvo movimientos registrados.`)) return;
    try {
      await MttoApi.inventario.eliminar(codigo);
      await cargarTabla();
    } catch (err) {
      alert(err.message);
    }
  }

  async function abrirModalEdicion(codigo) {
    const articulo = await MttoApi.inventario.obtener(codigo);
    estado.edicion = codigo;

    document.getElementById("tituloModalArticulo").textContent = "Editar artículo — " + codigo;
    document.getElementById("campoCodigo").value = articulo.codigo;
    document.getElementById("campoCodigo").disabled = true;
    document.getElementById("campoNombre").value = articulo.nombre;
    document.getElementById("campoDescripcion").value = "";
    document.getElementById("campoCategoria").value = articulo.categoria;
    document.getElementById("campoUnidad").value = articulo.unidad;
    document.getElementById("campoAlmacen").value = articulo.almacen;
    document.getElementById("campoEstado").value = articulo.estado;
    document.getElementById("campoStockActual").value = articulo.stockActual;
    document.getElementById("campoStockMinimo").value = articulo.stockMinimo;
    document.getElementById("campoStockMaximo").value = articulo.stockMaximo ?? "";
    document.getElementById("errorArticulo").classList.add("d-none");

    $("#modalArticulo").modal("show");
  }

  function abrirModalAlta() {
    estado.edicion = null;
    document.getElementById("formArticulo").reset();
    document.getElementById("tituloModalArticulo").textContent = "Nuevo artículo";
    document.getElementById("campoCodigo").disabled = false;
    document.getElementById("errorArticulo").classList.add("d-none");
  }

  async function guardarArticulo(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorArticulo");
    errorBox.classList.add("d-none");

    const entrada = {
      codigo: document.getElementById("campoCodigo").value.trim(),
      nombre: document.getElementById("campoNombre").value.trim(),
      descripcion: document.getElementById("campoDescripcion").value.trim() || null,
      categoria: document.getElementById("campoCategoria").value,
      unidad: document.getElementById("campoUnidad").value,
      almacen: document.getElementById("campoAlmacen").value,
      estado: document.getElementById("campoEstado").value,
      stockActual: parseFloat(document.getElementById("campoStockActual").value),
      stockMinimo: parseFloat(document.getElementById("campoStockMinimo").value),
      stockMaximo: document.getElementById("campoStockMaximo").value ? parseFloat(document.getElementById("campoStockMaximo").value) : null
    };

    try {
      if (estado.edicion) {
        await MttoApi.inventario.actualizar(estado.edicion, entrada);
      } else {
        await MttoApi.inventario.guardar(entrada);
      }
      $("#modalArticulo").modal("hide");
      await cargarTabla();
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await cargarCatalogos();
    await cargarTabla();

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
    document.getElementById("btnNuevoArticulo").addEventListener("click", abrirModalAlta);
    document.getElementById("formArticulo").addEventListener("submit", guardarArticulo);
    document.getElementById("btnExportar").addEventListener("click", exportar);
  });

  // Exporta TODO lo que cumpla el filtro actual (no solo la página visible),
  // con el mismo formato que la hoja "Inventario" del Excel original.
  function exportar() {
    const params = new URLSearchParams({
      q: document.getElementById("filtroTexto").value.trim(),
      categoria: document.getElementById("filtroCategoria").value,
      nivel: document.getElementById("filtroNivel").value
    });
    [...params.keys()].forEach(k => { if (!params.get(k)) params.delete(k); });
    window.location.href = `${window.MTTO_CONFIG.API_BASE_URL}/inventario/exportar?${params}`;
  }
})();
