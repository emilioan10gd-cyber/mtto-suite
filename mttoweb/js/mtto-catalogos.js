(function () {
  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML;
  }

  // Un solo lugar define qué campos tiene cada catálogo: la tabla, el
  // formulario del modal y el guardado se generan a partir de esto, en vez
  // de repetir el mismo código 4 veces para categorías/almacenes/proveedores/unidades.
  const CATALOGOS = {
    categorias: {
      etiqueta: "categoría",
      tituloPlural: "Categorías",
      campos: [
        { clave: "codigo", label: "Código", tipo: "text", maxlength: 20, requerido: true },
        { clave: "nombre", label: "Nombre", tipo: "text", maxlength: 80, requerido: true },
        { clave: "descripcion", label: "Descripción", tipo: "text", maxlength: 255, requerido: false }
      ]
    },
    almacenes: {
      etiqueta: "almacén",
      tituloPlural: "Almacenes",
      campos: [
        { clave: "codigo", label: "Código", tipo: "text", maxlength: 20, requerido: true },
        { clave: "nombre", label: "Nombre", tipo: "text", maxlength: 80, requerido: true },
        { clave: "ubicacion", label: "Ubicación", tipo: "text", maxlength: 150, requerido: false },
        { clave: "responsable", label: "Responsable", tipo: "text", maxlength: 120, requerido: false }
      ]
    },
    proveedores: {
      etiqueta: "proveedor",
      tituloPlural: "Proveedores",
      campos: [
        { clave: "codigo", label: "Código", tipo: "text", maxlength: 20, requerido: true },
        { clave: "nombre", label: "Nombre", tipo: "text", maxlength: 150, requerido: true },
        { clave: "contacto", label: "Contacto", tipo: "text", maxlength: 120, requerido: false },
        { clave: "telefono", label: "Teléfono", tipo: "text", maxlength: 40, requerido: false },
        { clave: "correo", label: "Correo", tipo: "email", maxlength: 120, requerido: false }
      ]
    },
    unidades: {
      etiqueta: "unidad",
      tituloPlural: "Unidades",
      campos: [
        { clave: "nombre", label: "Nombre", tipo: "text", maxlength: 40, requerido: true },
        { clave: "abreviatura", label: "Abreviatura", tipo: "text", maxlength: 10, requerido: false },
        { clave: "permiteDecimal", label: "Permite decimales", tipo: "checkbox", requerido: false }
      ]
    }
  };

  const estado = { tipo: null, id: null }; // id null = alta

  async function cargarTabla(tipo) {
    const config = CATALOGOS[tipo];
    const tbody = document.getElementById("tabla-" + tipo);
    const filas = await MttoApi.catalogosAdmin.listar(tipo);

    const encabezado = `<tr class="text-muted small">
      ${config.campos.map(c => `<th>${escapa(c.label)}</th>`).join("")}
      <th>Estado</th><th></th>
    </tr>`;

    const cuerpo = filas.map(fila => {
      const celdas = config.campos.map(c => {
        if (c.tipo === "checkbox") return `<td>${fila[c.clave] ? "Sí" : "No"}</td>`;
        return `<td>${escapa(fila[c.clave])}</td>`;
      }).join("");
      const insignia = fila.activo
        ? `<span class="badge badge-success">Activo</span>`
        : `<span class="badge badge-secondary">Dado de baja</span>`;
      const botonEstado = fila.activo
        ? `<button type="button" class="btn btn-sm btn-outline-danger btn-toggle" data-tipo="${tipo}" data-id="${fila.id}" data-activo="false">Dar de baja</button>`
        : `<button type="button" class="btn btn-sm btn-outline-success btn-toggle" data-tipo="${tipo}" data-id="${fila.id}" data-activo="true">Reactivar</button>`;

      return `<tr>
        ${celdas}
        <td>${insignia}</td>
        <td class="text-nowrap">
          <button type="button" class="btn btn-sm btn-outline-primary btn-editar" data-tipo="${tipo}" data-id="${fila.id}">
            <i class="typcn typcn-edit"></i>
          </button>
          ${botonEstado}
        </td>
      </tr>`;
    }).join("") || `<tr><td colspan="${config.campos.length + 2}">Sin registros</td></tr>`;

    tbody.innerHTML = encabezado + cuerpo;

    tbody.querySelectorAll(".btn-editar").forEach(btn =>
      btn.addEventListener("click", () => abrirModal(tipo, btn.dataset.id, filas)));
    tbody.querySelectorAll(".btn-toggle").forEach(btn =>
      btn.addEventListener("click", () => cambiarEstado(tipo, btn.dataset.id, btn.dataset.activo === "true")));
  }

  function renderizarCampoFormulario(campo, valor) {
    if (campo.tipo === "checkbox") {
      return `
        <div class="form-group form-check">
          <input type="checkbox" class="form-check-input" id="campo-${campo.clave}" ${valor ? "checked" : ""}>
          <label class="form-check-label" for="campo-${campo.clave}">${escapa(campo.label)}</label>
        </div>`;
    }
    return `
      <div class="form-group">
        <label>${escapa(campo.label)}${campo.requerido ? " *" : ""}</label>
        <input type="${campo.tipo}" class="form-control" id="campo-${campo.clave}"
               maxlength="${campo.maxlength || 255}" ${campo.requerido ? "required" : ""}
               value="${escapa(valor ?? "")}">
      </div>`;
  }

  function abrirModal(tipo, id, filasCache) {
    const config = CATALOGOS[tipo];
    estado.tipo = tipo;
    estado.id = id || null;

    const fila = id ? filasCache.find(f => String(f.id) === String(id)) : null;

    document.getElementById("tituloModalCatalogo").textContent =
      (id ? "Editar " : "Nueva/o ") + config.etiqueta;
    document.getElementById("errorCatalogo").classList.add("d-none");
    document.getElementById("camposCatalogo").innerHTML =
      config.campos.map(c => renderizarCampoFormulario(c, fila ? fila[c.clave] : null)).join("");

    $("#modalCatalogo").modal("show");
  }

  async function guardar(ev) {
    ev.preventDefault();
    const config = CATALOGOS[estado.tipo];
    const errorBox = document.getElementById("errorCatalogo");
    errorBox.classList.add("d-none");

    const entrada = {};
    config.campos.forEach(c => {
      const el = document.getElementById("campo-" + c.clave);
      entrada[c.clave.charAt(0).toUpperCase() + c.clave.slice(1)] =
        c.tipo === "checkbox" ? el.checked : (el.value.trim() || null);
    });

    try {
      if (estado.id) {
        await MttoApi.catalogosAdmin.actualizar(estado.tipo, estado.id, entrada);
      } else {
        await MttoApi.catalogosAdmin.crear(estado.tipo, entrada);
      }
      $("#modalCatalogo").modal("hide");
      await cargarTabla(estado.tipo);
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  async function cambiarEstado(tipo, id, activo) {
    const accion = activo ? "reactivar" : "dar de baja a";
    if (!confirm(`¿Seguro que quieres ${accion} este registro?`)) return;
    await MttoApi.catalogosAdmin.cambiarEstado(tipo, id, activo);
    await cargarTabla(tipo);
  }

  document.addEventListener("DOMContentLoaded", () => {
    Object.keys(CATALOGOS).forEach(cargarTabla);

    document.querySelectorAll(".btn-nuevo").forEach(btn =>
      btn.addEventListener("click", () => abrirModal(btn.dataset.tipo, null, null)));

    document.getElementById("formCatalogo").addEventListener("submit", guardar);
  });
})();
