(function () {
  let catalogos = { areas: [], categoriasEquipo: [], estadosEquipo: [] };

  function escapa(t) {
    const d = document.createElement("div");
    d.textContent = t ?? "";
    return d.innerHTML;
  }

  function badgeEstado(estado, esBaja) {
    const clase = esBaja ? "badge-secondary" : estado === "Operativo" ? "badge-success" : estado === "En mantenimiento" ? "badge-warning" : "badge-danger";
    return `<span class="badge ${clase}">${escapa(estado)}</span>`;
  }

  async function cargarCatalogos() {
    catalogos = await BiomedicoApi.catalogos.todos();
    const fArea = document.getElementById("fArea");
    const fCategoria = document.getElementById("fCategoria");
    const fEstado = document.getElementById("fEstado");
    const listaAreas = document.getElementById("listaAreas");
    const listaCategorias = document.getElementById("listaCategorias");

    catalogos.areas.forEach(a => {
      fArea.insertAdjacentHTML("beforeend", `<option value="${escapa(a.nombre)}">${escapa(a.nombre)}</option>`);
      listaAreas.insertAdjacentHTML("beforeend", `<option value="${escapa(a.nombre)}">`);
    });
    catalogos.categoriasEquipo.forEach(c => {
      fCategoria.insertAdjacentHTML("beforeend", `<option value="${escapa(c.nombre)}">${escapa(c.nombre)}</option>`);
      listaCategorias.insertAdjacentHTML("beforeend", `<option value="${escapa(c.nombre)}">`);
    });
    catalogos.estadosEquipo.forEach(e => {
      fEstado.insertAdjacentHTML("beforeend", `<option value="${escapa(e.nombre)}">${escapa(e.nombre)}</option>`);
    });
  }

  function filtros() {
    return {
      q: document.getElementById("fBusqueda").value.trim() || undefined,
      area: document.getElementById("fArea").value || undefined,
      categoria: document.getElementById("fCategoria").value || undefined,
      estado: document.getElementById("fEstado").value || undefined,
      tamano: 500
    };
  }

  function actualizarLinkExportar() {
    document.getElementById("btnExportar").href = BiomedicoApi.equipos.exportarUrl(filtros());
  }

  async function cargarTabla() {
    const estadoEl = document.getElementById("estadoCarga");
    try {
      const pagina = await BiomedicoApi.equipos.listar(filtros());
      const tbody = document.getElementById("tablaEquipos");
      tbody.innerHTML = pagina.datos.map(e => `
        <tr>
          <td>${escapa(e.area)}</td>
          <td>${escapa(e.nombre)}</td>
          <td>${escapa(e.marca)} ${escapa(e.modelo)}</td>
          <td>${escapa(e.numeroSerie)}</td>
          <td>${badgeEstado(e.estado, e.esBaja)}</td>
          <td>${escapa(e.ubicacion)}</td>
          <td>${e.mantenimientosVencidos > 0 ? `<span class="text-danger font-weight-bold">${e.mantenimientosVencidos} vencido(s)</span>` : `${e.totalMantenimientos} registrado(s)`}</td>
          <td class="text-right text-nowrap">
            <a class="btn btn-sm btn-outline-primary" href="biomedico-equipo.html?id=${e.id}" title="Ver expediente"><i class="typcn typcn-eye-outline"></i></a>
            <button type="button" class="btn btn-sm btn-outline-secondary btn-editar" data-id="${e.id}" title="Editar"><i class="typcn typcn-edit"></i></button>
            ${!e.esBaja ? `<button type="button" class="btn btn-sm btn-outline-danger btn-baja" data-id="${e.id}" title="Dar de baja"><i class="typcn typcn-warning-outline"></i></button>` : ""}
          </td>
        </tr>`).join("") || `<tr><td colspan="8">Sin equipos que coincidan con el filtro.</td></tr>`;

      tbody.querySelectorAll(".btn-editar").forEach(b => b.addEventListener("click", () => abrirEdicion(parseInt(b.dataset.id, 10))));
      tbody.querySelectorAll(".btn-baja").forEach(b => b.addEventListener("click", () => abrirBaja(parseInt(b.dataset.id, 10))));

      actualizarLinkExportar();
      estadoEl.textContent = "";
    } catch (err) {
      estadoEl.textContent = "No se pudo conectar con la API (" + err.message + ").";
      estadoEl.classList.add("text-danger");
    }
  }

  function limpiarForm() {
    document.getElementById("errorEquipo").classList.add("d-none");
    document.getElementById("fEquipoId").value = "";
    ["fNombre", "fAreaInput", "fCategoriaInput", "fMarca", "fModelo", "fSerie", "fUbicacion", "fFechaAdq", "fComentarios"].forEach(id => document.getElementById(id).value = "");
    document.getElementById("modalEquipoTitulo").textContent = "Agregar equipo";
  }

  function abrirNuevo() {
    limpiarForm();
    $("#modalEquipo").modal("show");
  }

  async function abrirEdicion(id) {
    limpiarForm();
    const e = await BiomedicoApi.equipos.obtener(id);
    document.getElementById("modalEquipoTitulo").textContent = "Editar equipo";
    document.getElementById("fEquipoId").value = e.id;
    document.getElementById("fNombre").value = e.nombre || "";
    document.getElementById("fAreaInput").value = e.area || "";
    document.getElementById("fCategoriaInput").value = e.categoria || "";
    document.getElementById("fMarca").value = e.marca || "";
    document.getElementById("fModelo").value = e.modelo || "";
    document.getElementById("fSerie").value = e.numeroSerie || "";
    document.getElementById("fUbicacion").value = e.ubicacion || "";
    document.getElementById("fFechaAdq").value = e.fechaAdquisicion ? e.fechaAdquisicion.substring(0, 10) : "";
    document.getElementById("fComentarios").value = e.comentarios || "";
    $("#modalEquipo").modal("show");
  }

  async function guardarEquipo() {
    const errorEl = document.getElementById("errorEquipo");
    errorEl.classList.add("d-none");
    const nombre = document.getElementById("fNombre").value.trim();
    const area = document.getElementById("fAreaInput").value.trim();
    if (!nombre || !area) {
      errorEl.textContent = "Nombre y área son obligatorios.";
      errorEl.classList.remove("d-none");
      return;
    }
    const idVal = document.getElementById("fEquipoId").value;
    const entrada = {
      id: idVal ? parseInt(idVal, 10) : null,
      nombre,
      area,
      categoria: document.getElementById("fCategoriaInput").value.trim() || null,
      marca: document.getElementById("fMarca").value.trim() || null,
      modelo: document.getElementById("fModelo").value.trim() || null,
      numeroSerie: document.getElementById("fSerie").value.trim() || null,
      ubicacion: document.getElementById("fUbicacion").value.trim() || null,
      fechaAdquisicion: document.getElementById("fFechaAdq").value || null,
      comentarios: document.getElementById("fComentarios").value.trim() || null
    };
    try {
      await BiomedicoApi.equipos.guardar(entrada);
      $("#modalEquipo").modal("hide");
      await cargarTabla();
    } catch (err) {
      errorEl.textContent = err.message;
      errorEl.classList.remove("d-none");
    }
  }

  function abrirBaja(id) {
    document.getElementById("errorBaja").classList.add("d-none");
    document.getElementById("bajaEquipoId").value = id;
    document.getElementById("bajaMotivo").value = "";
    $("#modalBaja").modal("show");
  }

  async function confirmarBaja() {
    const errorEl = document.getElementById("errorBaja");
    errorEl.classList.add("d-none");
    const id = parseInt(document.getElementById("bajaEquipoId").value, 10);
    const motivo = document.getElementById("bajaMotivo").value.trim();
    if (!motivo) {
      errorEl.textContent = "El motivo es obligatorio.";
      errorEl.classList.remove("d-none");
      return;
    }
    try {
      await BiomedicoApi.equipos.cambiarEstado(id, "De baja", motivo);
      $("#modalBaja").modal("hide");
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
    document.getElementById("btnGuardarEquipo").addEventListener("click", guardarEquipo);
    document.getElementById("btnConfirmarBaja").addEventListener("click", confirmarBaja);

    let temporizador;
    ["fBusqueda"].forEach(id => document.getElementById(id).addEventListener("input", () => {
      clearTimeout(temporizador);
      temporizador = setTimeout(cargarTabla, 300);
    }));
    ["fArea", "fCategoria", "fEstado"].forEach(id => document.getElementById(id).addEventListener("change", cargarTabla));
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", iniciar);
  else setTimeout(iniciar, 100);
})();
