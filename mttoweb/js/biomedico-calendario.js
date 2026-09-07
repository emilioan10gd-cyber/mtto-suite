(function () {
  const MESES = ["enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];
  const DIAS = ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
  let fechaActual = new Date();
  let mantenimientosMes = [];
  let equipos = [];

  function escapa(t) {
    const d = document.createElement("div");
    d.textContent = t ?? "";
    return d.innerHTML;
  }

  function claseDia(m) {
    if (m.estado === "Vencido") return "vencido";
    return m.tipo === "Preventivo" ? "preventivo" : "correctivo";
  }

  async function cargarMes() {
    const estadoEl = document.getElementById("estadoCarga");
    const anio = fechaActual.getFullYear();
    const mes = fechaActual.getMonth();
    document.getElementById("tituloMes").textContent = MESES[mes].charAt(0).toUpperCase() + MESES[mes].slice(1) + " " + anio;

    const primerDiaMes = new Date(anio, mes, 1);
    const ultimoDiaMes = new Date(anio, mes + 1, 0);
    const inicioGrilla = new Date(primerDiaMes);
    inicioGrilla.setDate(inicioGrilla.getDate() - primerDiaMes.getDay());
    const finGrilla = new Date(ultimoDiaMes);
    finGrilla.setDate(finGrilla.getDate() + (6 - ultimoDiaMes.getDay()));

    try {
      mantenimientosMes = await BiomedicoApi.mantenimiento.listar({
        desde: inicioGrilla.toISOString().substring(0, 10),
        hasta: finGrilla.toISOString().substring(0, 10)
      });

      const porDia = {};
      mantenimientosMes.forEach(m => {
        const clave = m.fechaProgramada.substring(0, 10);
        (porDia[clave] = porDia[clave] || []).push(m);
      });

      const grilla = document.getElementById("grillaCalendario");
      let html = DIAS.map(d => `<div class="cal-encabezado">${d}</div>`).join("");

      const hoyStr = new Date().toISOString().substring(0, 10);
      for (let d = new Date(inicioGrilla); d <= finGrilla; d.setDate(d.getDate() + 1)) {
        const claveDia = d.toISOString().substring(0, 10);
        const items = porDia[claveDia] || [];
        const fueraMes = d.getMonth() !== mes;
        html += `<div class="cal-dia ${fueraMes ? "fuera-mes" : ""}" data-fecha="${claveDia}" style="${claveDia === hoyStr ? "border-color:#2a78d6;" : ""}">
          <div class="num">${d.getDate()}</div>
          ${items.slice(0, 3).map(m => `<div class="item ${claseDia(m)}" title="${escapa(m.equipo)}">${escapa(m.equipo)}</div>`).join("")}
          ${items.length > 3 ? `<div class="text-muted" style="font-size:10px;">+${items.length - 3} más</div>` : ""}
        </div>`;
      }
      grilla.innerHTML = html;

      grilla.querySelectorAll(".cal-dia").forEach(el => el.addEventListener("click", () => abrirDia(el.dataset.fecha, porDia[el.dataset.fecha] || [])));

      estadoEl.textContent = "";
    } catch (err) {
      estadoEl.textContent = "No se pudo conectar con la API (" + err.message + ").";
      estadoEl.classList.add("text-danger");
    }
  }

  function abrirDia(fecha, items) {
    document.getElementById("modalDiaTitulo").textContent = "Mantenimientos — " + new Date(fecha + "T00:00:00").toLocaleDateString("es-MX", { weekday: "long", year: "numeric", month: "long", day: "numeric" });
    document.getElementById("tablaDia").innerHTML = items.map(m => `
      <tr>
        <td><a href="biomedico-equipo.html?id=${m.equipoId}">${escapa(m.equipo)}</a></td>
        <td>${escapa(m.tipo)}</td>
        <td>${escapa(m.estado)}</td>
        <td>${escapa(m.tecnicoResponsable)}</td>
        <td class="text-right text-nowrap">
          ${m.estado !== "Realizado" && m.estado !== "Cancelado" ? `
            <button type="button" class="btn btn-sm btn-outline-success btn-realizado" data-id="${m.id}">Realizado</button>
            <button type="button" class="btn btn-sm btn-outline-danger btn-cancelar" data-id="${m.id}">Cancelar</button>
          ` : ""}
        </td>
      </tr>`).join("") || `<tr><td colspan="5">Sin mantenimientos programados este día.</td></tr>`;

    document.getElementById("tablaDia").querySelectorAll(".btn-realizado").forEach(b =>
      b.addEventListener("click", async () => { await BiomedicoApi.mantenimiento.marcarRealizado(parseInt(b.dataset.id, 10)); $("#modalDia").modal("hide"); await cargarMes(); }));
    document.getElementById("tablaDia").querySelectorAll(".btn-cancelar").forEach(b =>
      b.addEventListener("click", async () => { await BiomedicoApi.mantenimiento.cancelar(parseInt(b.dataset.id, 10)); $("#modalDia").modal("hide"); await cargarMes(); }));

    $("#modalDia").modal("show");
  }

  async function cargarEquipos() {
    const pagina = await BiomedicoApi.equipos.listar({ tamano: 500 });
    equipos = pagina.datos;
    document.getElementById("pEquipo").innerHTML = equipos.map(e => `<option value="${e.id}">${escapa(e.nombre)} (${escapa(e.area)})</option>`).join("");
  }

  function abrirProgramar() {
    document.getElementById("errorProgramar").classList.add("d-none");
    ["pTecnico", "pProveedor", "pFrecuencia", "pDescripcion"].forEach(id => document.getElementById(id).value = "");
    document.getElementById("pFecha").value = new Date().toISOString().substring(0, 10);
    document.getElementById("pTipo").value = "Preventivo";
    $("#modalProgramar").modal("show");
  }

  async function guardarProgramacion() {
    const errorEl = document.getElementById("errorProgramar");
    errorEl.classList.add("d-none");
    const equipoId = parseInt(document.getElementById("pEquipo").value, 10);
    const fecha = document.getElementById("pFecha").value;
    if (!equipoId || !fecha) {
      errorEl.textContent = "Equipo y fecha son obligatorios.";
      errorEl.classList.remove("d-none");
      return;
    }
    const entrada = {
      equipoId,
      tipo: document.getElementById("pTipo").value,
      fechaProgramada: fecha,
      tecnicoResponsable: document.getElementById("pTecnico").value.trim() || null,
      proveedor: document.getElementById("pProveedor").value.trim() || null,
      frecuenciaMeses: document.getElementById("pFrecuencia").value ? parseInt(document.getElementById("pFrecuencia").value, 10) : null,
      descripcion: document.getElementById("pDescripcion").value.trim() || null
    };
    try {
      await BiomedicoApi.mantenimiento.programar(entrada);
      $("#modalProgramar").modal("hide");
      await cargarMes();
    } catch (err) {
      errorEl.textContent = err.message;
      errorEl.classList.remove("d-none");
    }
  }

  async function iniciar() {
    await cargarEquipos();
    await cargarMes();

    document.getElementById("btnMesAnterior").addEventListener("click", () => { fechaActual.setMonth(fechaActual.getMonth() - 1); cargarMes(); });
    document.getElementById("btnMesSiguiente").addEventListener("click", () => { fechaActual.setMonth(fechaActual.getMonth() + 1); cargarMes(); });
    document.getElementById("btnHoy").addEventListener("click", () => { fechaActual = new Date(); cargarMes(); });
    document.getElementById("btnProgramar").addEventListener("click", abrirProgramar);
    document.getElementById("btnGuardarProgramacion").addEventListener("click", guardarProgramacion);
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", iniciar);
  else setTimeout(iniciar, 100);
})();
