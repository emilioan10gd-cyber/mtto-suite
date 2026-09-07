// Catálogos de la Clínica de Catéter: descripciones propias, ubicaciones y
// las listas fijas del módulo.
//
// La descripción del cuadro básico (cpm_clave) es un dato de gobierno y nunca
// se edita desde aquí. Lo que se administra es la descripción propia, que vive
// aparte en cateter_clave_descripcion y solo se le pone encima al mostrarla.
(function () {
  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  // { claves: [...del inventario], propias: Map clave -> descripción propia }
  const estado = {
    claves: [],
    propias: new Map(),
    ubicaciones: [],
    ubicacionEnTurno: null // null = alta; con id = edición
  };

  function avisa(idElemento, mensaje, esError) {
    const alerta = document.getElementById(idElemento);
    alerta.textContent = mensaje;
    alerta.classList.remove("d-none", "alert-danger", "alert-success");
    alerta.classList.add(esError ? "alert-danger" : "alert-success");
  }

  function ocultaAviso(idElemento) {
    document.getElementById(idElemento).classList.add("d-none");
  }

  // ------------------------------------------- claves y descripciones propias
  async function cargarDescripciones() {
    const tbody = document.getElementById("tablaDescripciones");
    try {
      // Las dos consultas son independientes: el inventario da las claves que
      // la clínica maneja y la otra las descripciones que alguien escribió.
      const [inventario, propias] = await Promise.all([
        CateterApi.inventario.listar({ pagina: 1, tamano: 500 }),
        CateterApi.cuadroBasico.descripcionesPropias()
      ]);

      estado.claves = inventario.datos;
      estado.propias = new Map(propias.map(p => [p.clave, p.descripcionPropia]));
      pintarDescripciones();
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="5" class="text-danger">No se pudieron cargar las claves: ${escapa(err.message)}</td></tr>`;
    }
  }

  function pintarDescripciones() {
    const tbody = document.getElementById("tablaDescripciones");
    const filtro = document.getElementById("filtroDescripciones").value.trim().toLowerCase();

    const visibles = estado.claves.filter(a => {
      if (!filtro) return true;
      const propia = estado.propias.get(a.clave) || "";
      return [a.clave, a.nombre, a.descripcion, a.referencia, propia]
        .some(v => (v || "").toLowerCase().includes(filtro));
    });

    tbody.innerHTML = visibles.map(a => {
      const propia = estado.propias.get(a.clave);
      return `
        <tr>
          <td class="text-nowrap"><span style="font-family:monospace">${escapa(a.clave)}</span></td>
          <td class="small">${escapa(a.referencia) || "—"}</td>
          <td class="small text-muted">${escapa(a.descripcion) || "—"}</td>
          <td class="small">${propia
            ? escapa(propia)
            : '<span class="text-muted font-italic">Usa la oficial</span>'}</td>
          <td class="text-right">
            <button type="button" class="btn btn-sm btn-outline-primary btn-editar-desc"
                    data-clave="${escapa(a.clave)}">
              <i class="typcn typcn-edit"></i> ${propia ? "Editar" : "Agregar"}
            </button>
            ${propia ? `
            <button type="button" class="btn btn-sm btn-outline-secondary btn-quitar-desc"
                    data-clave="${escapa(a.clave)}" title="Volver a mostrar la del cuadro básico">
              <i class="typcn typcn-times"></i>
            </button>` : ""}
          </td>
        </tr>`;
    }).join("") || `<tr><td colspan="5">Sin claves que coincidan.</td></tr>`;

    document.getElementById("resumenDescripciones").textContent =
      `${visibles.length} de ${estado.claves.length} claves — ${estado.propias.size} con descripción propia`;

    tbody.querySelectorAll(".btn-editar-desc").forEach(btn => {
      btn.addEventListener("click", () => abrirModalDescripcion(btn.dataset.clave));
    });
    tbody.querySelectorAll(".btn-quitar-desc").forEach(btn => {
      btn.addEventListener("click", () => quitarDescripcion(btn.dataset.clave));
    });
  }

  function abrirModalDescripcion(clave) {
    const articulo = estado.claves.find(a => a.clave === clave);
    document.getElementById("descClave").textContent = clave;
    document.getElementById("descOficial").textContent =
      (articulo && articulo.descripcion) || "(esta clave no trae descripción oficial)";
    document.getElementById("descPropia").value = estado.propias.get(clave) || "";
    document.getElementById("formDescripcion").dataset.clave = clave;
    ocultaAviso("alertaDescripcion");
    $("#modalDescripcion").modal("show");
  }

  async function guardarDescripcion(ev) {
    ev.preventDefault();
    const clave = ev.target.dataset.clave;
    const texto = document.getElementById("descPropia").value.trim();

    try {
      await CateterApi.cuadroBasico.guardarDescripcion(clave, texto);
      $("#modalDescripcion").modal("hide");
      await cargarDescripciones();
    } catch (err) {
      avisa("alertaDescripcion", err.message, true);
    }
  }

  async function quitarDescripcion(clave) {
    if (!confirm(`Quitar la descripción propia de ${clave}?\n\n` +
                 "La clave vuelve a mostrar la del cuadro básico. La oficial no se toca.")) return;
    try {
      await CateterApi.cuadroBasico.quitarDescripcion(clave);
      await cargarDescripciones();
    } catch (err) {
      alert("No se pudo quitar: " + err.message);
    }
  }

  // ---------------------------------------------------------- ubicaciones
  async function cargarUbicaciones() {
    const tbody = document.getElementById("tablaUbicaciones");
    try {
      // incluirInactivas: esta pantalla es la única desde donde se pueden
      // volver a prender, así que necesita verlas.
      estado.ubicaciones = await CateterApi.catalogos.ubicaciones(true);

      tbody.innerHTML = estado.ubicaciones.map(u => `
        <tr class="${u.activo ? "" : "text-muted"}">
          <td class="small" style="font-family:monospace">${escapa(u.codigo)}</td>
          <td>${escapa(u.nombre)}</td>
          <td class="small text-muted">${escapa(u.descripcion) || "—"}</td>
          <td><span class="badge ${u.esAlmacen ? "badge-info" : "badge-secondary"}">
            ${u.esAlmacen ? "Almacén" : "Uso inmediato"}</span></td>
          <td><span class="badge ${u.activo ? "badge-success" : "badge-dark"}">
            ${u.activo ? "Activa" : "Inactiva"}</span></td>
          <td class="text-right">
            ${u.activo ? `
              <button type="button" class="btn btn-sm btn-outline-primary btn-editar-ubi" data-id="${u.id}">
                <i class="typcn typcn-edit"></i> Editar
              </button>
              <button type="button" class="btn btn-sm btn-outline-danger btn-baja-ubi" data-id="${u.id}"
                      title="Dar de baja (no se borra)">
                <i class="typcn typcn-eject"></i>
              </button>` : `
              <button type="button" class="btn btn-sm btn-outline-success btn-alta-ubi" data-id="${u.id}">
                <i class="typcn typcn-refresh"></i> Reactivar
              </button>`}
          </td>
        </tr>`).join("") || `<tr><td colspan="6">No hay ubicaciones registradas.</td></tr>`;

      tbody.querySelectorAll(".btn-editar-ubi").forEach(btn => {
        btn.addEventListener("click", () => abrirModalUbicacion(parseInt(btn.dataset.id, 10)));
      });
      tbody.querySelectorAll(".btn-baja-ubi").forEach(btn => {
        btn.addEventListener("click", () => darDeBajaUbicacion(parseInt(btn.dataset.id, 10)));
      });
      tbody.querySelectorAll(".btn-alta-ubi").forEach(btn => {
        btn.addEventListener("click", () => reactivarUbicacion(parseInt(btn.dataset.id, 10)));
      });
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="6" class="text-danger">No se pudieron cargar las ubicaciones: ${escapa(err.message)}</td></tr>`;
    }
  }

  function abrirModalUbicacion(id) {
    const ubi = id ? estado.ubicaciones.find(u => u.id === id) : null;
    estado.ubicacionEnTurno = ubi || null;

    document.getElementById("tituloModalUbicacion").textContent =
      ubi ? `Editar ubicación — ${ubi.nombre}` : "Nueva ubicación";
    document.getElementById("ubiNombre").value = ubi ? ubi.nombre : "";
    document.getElementById("ubiDescripcion").value = (ubi && ubi.descripcion) || "";
    document.getElementById("ubiEsAlmacen").checked = ubi ? ubi.esAlmacen : false;
    ocultaAviso("alertaModalUbicacion");
    $("#modalUbicacion").modal("show");
  }

  async function guardarUbicacion(ev) {
    ev.preventDefault();
    const entrada = {
      nombre: document.getElementById("ubiNombre").value.trim(),
      descripcion: document.getElementById("ubiDescripcion").value.trim() || null,
      esAlmacen: document.getElementById("ubiEsAlmacen").checked
    };

    try {
      if (estado.ubicacionEnTurno) {
        await CateterApi.catalogos.actualizarUbicacion(estado.ubicacionEnTurno.id, entrada);
      } else {
        await CateterApi.catalogos.crearUbicacion(entrada);
      }
      $("#modalUbicacion").modal("hide");
      await cargarUbicaciones();
    } catch (err) {
      avisa("alertaModalUbicacion", err.message, true);
    }
  }

  async function darDeBajaUbicacion(id) {
    const ubi = estado.ubicaciones.find(u => u.id === id);
    if (!confirm(`Dar de baja "${ubi.nombre}"?\n\n` +
                 "No se borra: deja de aparecer en los combos, pero su historial de " +
                 "movimientos se conserva y se puede reactivar después.")) return;
    try {
      await CateterApi.catalogos.desactivarUbicacion(id);
      ocultaAviso("alertaUbicaciones");
      await cargarUbicaciones();
    } catch (err) {
      // El caso típico: todavía tiene piezas. El servidor explica cuántas.
      avisa("alertaUbicaciones", err.message, true);
    }
  }

  async function reactivarUbicacion(id) {
    try {
      await CateterApi.catalogos.reactivarUbicacion(id);
      ocultaAviso("alertaUbicaciones");
      await cargarUbicaciones();
    } catch (err) {
      avisa("alertaUbicaciones", err.message, true);
    }
  }

  // ------------------------------------------------------ listas de consulta
  async function cargarListas() {
    try {
      const catalogos = await CateterApi.catalogos.todos();

      document.getElementById("listaCategorias").innerHTML =
        catalogos.categorias.map(c => `<li>· ${escapa(c.nombre)}</li>`).join("") || "<li>—</li>";

      document.getElementById("listaTipos").innerHTML =
        catalogos.tiposMovimiento.map(t =>
          `<li>· ${escapa(t.nombre)} <span class="text-muted">(${escapa(t.clasificacion)})</span></li>`
        ).join("") || "<li>—</li>";

      document.getElementById("listaUnidades").innerHTML =
        catalogos.unidades.map(u => `<li>· ${escapa(u.nombre)}</li>`).join("") || "<li>—</li>";
    } catch (err) {
      ["listaCategorias", "listaTipos", "listaUnidades"].forEach(id => {
        document.getElementById(id).innerHTML =
          `<li class="text-danger">No se pudo cargar: ${escapa(err.message)}</li>`;
      });
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    cargarDescripciones();
    cargarUbicaciones();
    cargarListas();

    document.getElementById("filtroDescripciones").addEventListener("input", pintarDescripciones);
    document.getElementById("formDescripcion").addEventListener("submit", guardarDescripcion);
    document.getElementById("btnNuevaUbicacion").addEventListener("click", () => abrirModalUbicacion(null));
    document.getElementById("formUbicacion").addEventListener("submit", guardarUbicacion);
  });
})();
