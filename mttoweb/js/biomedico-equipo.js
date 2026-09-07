(function () {
  const qp = new URLSearchParams(location.search);
  const equipoId = parseInt(qp.get("id"), 10);

  function escapa(t) {
    const d = document.createElement("div");
    d.textContent = t ?? "";
    return d.innerHTML;
  }

  function fecha(f) {
    return f ? new Date(f).toLocaleDateString("es-MX") : "—";
  }

  if (!equipoId) {
    document.getElementById("estadoCarga").textContent = "Falta el id del equipo en la URL (?id=...).";
    document.getElementById("estadoCarga").classList.add("text-danger");
    return;
  }

  async function cargarDatos() {
    const e = await BiomedicoApi.equipos.obtener(equipoId);
    document.getElementById("tituloEquipo").textContent = e.nombre;
    document.getElementById("subtituloEquipo").textContent = `${e.area} · ${e.estado}`;

    document.getElementById("dgArea").textContent = e.area || "—";
    document.getElementById("dgCategoria").textContent = e.categoria || "—";
    document.getElementById("dgEstado").textContent = e.estado || "—";
    document.getElementById("dgMarca").textContent = e.marca || "—";
    document.getElementById("dgModelo").textContent = e.modelo || "—";
    document.getElementById("dgSerie").textContent = e.numeroSerie || "—";
    document.getElementById("dgUbicacion").textContent = e.ubicacion || "—";
    document.getElementById("dgFechaAdq").textContent = fecha(e.fechaAdquisicion);
    document.getElementById("dgComentarios").textContent = e.comentarios || "—";

    document.getElementById("linkEtiqueta").href = "biomedico-etiquetas.html?id=" + equipoId;

    // Formulario de edición
    document.getElementById("eNombre").value = e.nombre || "";
    document.getElementById("eArea").value = e.area || "";
    document.getElementById("eCategoria").value = e.categoria || "";
    document.getElementById("eMarca").value = e.marca || "";
    document.getElementById("eModelo").value = e.modelo || "";
    document.getElementById("eSerie").value = e.numeroSerie || "";
    document.getElementById("eUbicacion").value = e.ubicacion || "";
    document.getElementById("eFechaAdq").value = e.fechaAdquisicion ? e.fechaAdquisicion.substring(0, 10) : "";
    document.getElementById("eComentarios").value = e.comentarios || "";

    return e;
  }

  function dibujarQr() {
    const url = location.origin + location.pathname.replace(/[^/]*$/, "") + "biomedico-equipo.html?id=" + equipoId;
    const qr = qrcode(0, "M");
    qr.addData(url);
    qr.make();
    document.getElementById("qrEquipo").innerHTML = qr.createSvgTag({ cellSize: 4, margin: 2 });
  }

  async function cargarCatalogosEdicion() {
    const catalogos = await BiomedicoApi.catalogos.todos();
    document.getElementById("listaAreasEdit").innerHTML = catalogos.areas.map(a => `<option value="${escapa(a.nombre)}">`).join("");
    document.getElementById("listaCategoriasEdit").innerHTML = catalogos.categoriasEquipo.map(c => `<option value="${escapa(c.nombre)}">`).join("");
  }

  async function guardarDatos() {
    const errorEl = document.getElementById("errorDatos");
    errorEl.classList.add("d-none");
    const nombre = document.getElementById("eNombre").value.trim();
    const area = document.getElementById("eArea").value.trim();
    if (!nombre || !area) {
      errorEl.textContent = "Nombre y área son obligatorios.";
      errorEl.classList.remove("d-none");
      return;
    }
    const entrada = {
      id: equipoId, nombre, area,
      categoria: document.getElementById("eCategoria").value.trim() || null,
      marca: document.getElementById("eMarca").value.trim() || null,
      modelo: document.getElementById("eModelo").value.trim() || null,
      numeroSerie: document.getElementById("eSerie").value.trim() || null,
      ubicacion: document.getElementById("eUbicacion").value.trim() || null,
      fechaAdquisicion: document.getElementById("eFechaAdq").value || null,
      comentarios: document.getElementById("eComentarios").value.trim() || null
    };
    try {
      await BiomedicoApi.equipos.guardar(entrada);
      $("#modalDatos").modal("hide");
      await cargarDatos();
    } catch (err) {
      errorEl.textContent = err.message;
      errorEl.classList.remove("d-none");
    }
  }

  // -------------------- Mantenimientos --------------------
  async function cargarMantenimientos() {
    const datos = await BiomedicoApi.mantenimiento.listar({ equipoId });
    document.getElementById("tablaMantenimientos").innerHTML = datos.map(m => `
      <tr>
        <td>${escapa(m.tipo)}</td>
        <td>${fecha(m.fechaProgramada)}</td>
        <td>${fecha(m.fechaRealizada)}</td>
        <td>${escapa(m.estado)}</td>
        <td>${escapa(m.tecnicoResponsable)}</td>
        <td class="text-right text-nowrap">
          ${m.estado !== "Realizado" && m.estado !== "Cancelado" ? `
            <button type="button" class="btn btn-sm btn-outline-success btn-mtto-realizado" data-id="${m.id}">Realizado</button>
            <button type="button" class="btn btn-sm btn-outline-danger btn-mtto-cancelar" data-id="${m.id}">Cancelar</button>` : ""}
        </td>
      </tr>`).join("") || `<tr><td colspan="6">Sin mantenimientos registrados.</td></tr>`;

    document.querySelectorAll(".btn-mtto-realizado").forEach(b => b.addEventListener("click", async () => {
      await BiomedicoApi.mantenimiento.marcarRealizado(parseInt(b.dataset.id, 10));
      await cargarMantenimientos();
    }));
    document.querySelectorAll(".btn-mtto-cancelar").forEach(b => b.addEventListener("click", async () => {
      await BiomedicoApi.mantenimiento.cancelar(parseInt(b.dataset.id, 10));
      await cargarMantenimientos();
    }));
  }

  function abrirModalMtto() {
    document.getElementById("errorMtto").classList.add("d-none");
    ["mTecnico", "mProveedor", "mFrecuencia", "mDescripcion"].forEach(id => document.getElementById(id).value = "");
    document.getElementById("mFecha").value = new Date().toISOString().substring(0, 10);
    document.getElementById("mTipo").value = "Preventivo";
    $("#modalMtto").modal("show");
  }

  async function guardarMtto() {
    const errorEl = document.getElementById("errorMtto");
    errorEl.classList.add("d-none");
    const f = document.getElementById("mFecha").value;
    if (!f) {
      errorEl.textContent = "La fecha es obligatoria.";
      errorEl.classList.remove("d-none");
      return;
    }
    try {
      await BiomedicoApi.mantenimiento.programar({
        equipoId,
        tipo: document.getElementById("mTipo").value,
        fechaProgramada: f,
        tecnicoResponsable: document.getElementById("mTecnico").value.trim() || null,
        proveedor: document.getElementById("mProveedor").value.trim() || null,
        frecuenciaMeses: document.getElementById("mFrecuencia").value ? parseInt(document.getElementById("mFrecuencia").value, 10) : null,
        descripcion: document.getElementById("mDescripcion").value.trim() || null
      });
      $("#modalMtto").modal("hide");
      await cargarMantenimientos();
    } catch (err) {
      errorEl.textContent = err.message;
      errorEl.classList.remove("d-none");
    }
  }

  // -------------------- Manuales --------------------
  async function cargarManuales() {
    const manuales = await BiomedicoApi.manuales.listar(equipoId);
    document.getElementById("listaManuales").innerHTML = manuales.map(m => `
      <li class="list-group-item d-flex justify-content-between align-items-center">
        <span><i class="typcn typcn-document-text"></i> ${escapa(m.nombreArchivo)}</span>
        <span>
          <a class="btn btn-sm btn-outline-secondary" href="${BiomedicoApi.manuales.descargarUrl(m.id)}" target="_blank"><i class="typcn typcn-download"></i></a>
          <button type="button" class="btn btn-sm btn-outline-danger btn-borrar-manual" data-id="${m.id}"><i class="typcn typcn-trash"></i></button>
        </span>
      </li>`).join("") || `<li class="list-group-item text-muted">Sin manuales adjuntos.</li>`;

    document.querySelectorAll(".btn-borrar-manual").forEach(b => b.addEventListener("click", async () => {
      if (!confirm("¿Eliminar este manual?")) return;
      await BiomedicoApi.manuales.eliminar(parseInt(b.dataset.id, 10));
      await cargarManuales();
    }));
  }

  async function subirManual() {
    const input = document.getElementById("inputManual");
    const errorEl = document.getElementById("errorManual");
    errorEl.classList.add("d-none");
    if (!input.files.length) return;
    try {
      await BiomedicoApi.manuales.subir(equipoId, input.files[0]);
      input.value = "";
      document.querySelector('label[for="inputManual"]').textContent = "Elegir archivo (PDF, DOC)";
      await cargarManuales();
    } catch (err) {
      errorEl.textContent = err.message;
      errorEl.classList.remove("d-none");
    }
  }

  // -------------------- Fallas + sugerencias --------------------
  async function cargarFallas() {
    const fallas = await BiomedicoApi.fallas.listar(equipoId);
    document.getElementById("tablaFallas").innerHTML = fallas.map(f => `
      <tr>
        <td>${fecha(f.fecha)}</td>
        <td>${escapa(f.sintoma)}</td>
        <td>${escapa(f.causa) || "—"}</td>
        <td>${escapa(f.solucion) || "—"}</td>
        <td>${escapa(f.estado)}</td>
        <td class="text-right">
          ${f.estado === "Abierta" ? `<button type="button" class="btn btn-sm btn-outline-primary btn-resolver" data-id="${f.id}">Resolver</button>` : ""}
        </td>
      </tr>`).join("") || `<tr><td colspan="6">Sin fallas registradas.</td></tr>`;

    document.querySelectorAll(".btn-resolver").forEach(b => b.addEventListener("click", () => abrirResolver(parseInt(b.dataset.id, 10))));
  }

  function renderSugerencias(sugerencias) {
    const box = document.getElementById("sugerenciasBox");
    if (!sugerencias.length) { box.innerHTML = ""; return; }
    box.innerHTML = `<div class="small text-muted mb-1">Coincidencias con el historial y los manuales:</div>` +
      sugerencias.map(s => `
        <div class="sugerencia ${s.origen === "manual" ? "manual" : ""}">
          <div class="origen">${s.origen === "manual" ? "Manual" : "Falla anterior"} ${s.mismoEquipo ? "· este equipo" : "· " + escapa(s.equipoNombre)}</div>
          ${s.origen === "manual"
            ? `<div><strong>${escapa(s.titulo)}</strong></div><div>${escapa(s.fragmento)}</div>`
            : `<div><strong>Síntoma:</strong> ${escapa(s.titulo)}</div>
               ${s.causa ? `<div><strong>Causa:</strong> ${escapa(s.causa)}</div>` : ""}
               <div><strong>Solución:</strong> ${escapa(s.solucion)}</div>`}
        </div>`).join("");
  }

  let temporizadorSugerencias;
  function onCambioSintoma() {
    clearTimeout(temporizadorSugerencias);
    const texto = document.getElementById("fSintoma").value.trim();
    if (texto.length < 4) { document.getElementById("sugerenciasBox").innerHTML = ""; return; }
    temporizadorSugerencias = setTimeout(async () => {
      try {
        const resultado = await BiomedicoApi.fallas.sugerencias(equipoId, texto);
        renderSugerencias(resultado.sugerencias || []);
      } catch { /* búsqueda best-effort, no interrumpe la captura */ }
    }, 400);
  }

  async function reportarFalla() {
    const sintoma = document.getElementById("fSintoma").value.trim();
    if (!sintoma) return;
    await BiomedicoApi.fallas.reportar({
      equipoId, sintoma,
      tecnico: document.getElementById("fTecnico").value.trim() || null
    });
    document.getElementById("fSintoma").value = "";
    document.getElementById("fTecnico").value = "";
    document.getElementById("sugerenciasBox").innerHTML = "";
    await cargarFallas();
  }

  function abrirResolver(id) {
    document.getElementById("errorResolver").classList.add("d-none");
    document.getElementById("rFallaId").value = id;
    document.getElementById("rCausa").value = "";
    document.getElementById("rSolucion").value = "";
    $("#modalResolver").modal("show");
  }

  async function guardarResolver() {
    const errorEl = document.getElementById("errorResolver");
    errorEl.classList.add("d-none");
    const solucion = document.getElementById("rSolucion").value.trim();
    if (!solucion) {
      errorEl.textContent = "La solución es obligatoria.";
      errorEl.classList.remove("d-none");
      return;
    }
    try {
      const id = parseInt(document.getElementById("rFallaId").value, 10);
      await BiomedicoApi.fallas.resolver(id, document.getElementById("rCausa").value.trim() || null, solucion);
      $("#modalResolver").modal("hide");
      await cargarFallas();
    } catch (err) {
      errorEl.textContent = err.message;
      errorEl.classList.remove("d-none");
    }
  }

  async function iniciar() {
    const estado = document.getElementById("estadoCarga");
    try {
      await Promise.all([cargarDatos(), cargarCatalogosEdicion(), cargarMantenimientos(), cargarManuales(), cargarFallas()]);
      dibujarQr();
      estado.textContent = "";
    } catch (err) {
      estado.textContent = "No se pudo cargar el expediente (" + err.message + ").";
      estado.classList.add("text-danger");
      return;
    }

    document.getElementById("btnEditarDatos").addEventListener("click", () => $("#modalDatos").modal("show"));
    document.getElementById("btnGuardarDatos").addEventListener("click", guardarDatos);
    document.getElementById("btnAgregarMtto").addEventListener("click", abrirModalMtto);
    document.getElementById("btnGuardarMtto").addEventListener("click", guardarMtto);
    document.getElementById("inputManual").addEventListener("change", (ev) => {
      const label = document.querySelector('label[for="inputManual"]');
      if (ev.target.files.length) label.textContent = ev.target.files[0].name;
      subirManual();
    });
    document.getElementById("fSintoma").addEventListener("input", onCambioSintoma);
    document.getElementById("btnReportarFalla").addEventListener("click", reportarFalla);
    document.getElementById("btnGuardarResolver").addEventListener("click", guardarResolver);
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", iniciar);
  else setTimeout(iniciar, 100);
})();
