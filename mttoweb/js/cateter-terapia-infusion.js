(function () {
  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  const hoyIso = () => new Date().toISOString().split("T")[0];
  const MESES = ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];
  const MESES_CORTOS = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];
  const PALETA = ["#2a78d6", "#eb6834", "#1baf7a", "#eda100"];
  const GRIS_EJE = "#e1e0d9";
  const TINTA_MUTED = "#898781";
  const hoy = new Date();

  // ------------------------------------------------------------------
  // Selector de periodo (mes + año) compartido por las 3 pestañas: la
  // captura solo puede caer en el mes en curso, pero el HISTORIAL se puede
  // navegar mes a mes hacia atrás sin límite, para que la tabla no crezca
  // sin control a la vista.
  // ------------------------------------------------------------------
  function llenarSelectorMes(select) {
    select.innerHTML = MESES.map((nombre, idx) =>
      `<option value="${idx + 1}">${nombre}</option>`).join("");
    select.value = String(hoy.getMonth() + 1);
  }

  function llenarSelectorAnio(select, anioMinimo) {
    const actual = hoy.getFullYear();
    const opciones = [];
    for (let a = actual; a >= anioMinimo; a--) opciones.push(a);
    select.innerHTML = opciones.map(a => `<option value="${a}">${a}</option>`).join("");
    select.value = String(actual);
  }

  function leerPeriodo(selMes, selAnio) {
    const periodo = { anio: parseInt(selAnio.value, 10) };
    if (selMes) periodo.mes = parseInt(selMes.value, 10);
    return periodo;
  }

  // ------------------------------------------------------------------
  // Pestaña 1: sitios anatómicos y calibres — tabla tipo Excel, una celda
  // numérica por columna. Los totales son la suma de las celdas, no un
  // conteo de casillas marcadas: puede haber varios del mismo sitio/calibre.
  // ------------------------------------------------------------------
  function numeroCelda(input) {
    const n = parseInt(input.value, 10);
    return Number.isFinite(n) && n >= 0 ? n : 0;
  }

  function actualizarTotalSitios() {
    const celdas = document.querySelectorAll(".celda-cantidad-sitio");
    const total = Array.from(celdas).reduce((suma, c) => suma + numeroCelda(c), 0);
    document.getElementById("totalSitios").textContent = total;
  }

  function actualizarTotalCateres() {
    const celdas = document.querySelectorAll(".celda-cantidad-calibre");
    const total = Array.from(celdas).reduce((suma, c) => suma + numeroCelda(c), 0);
    document.getElementById("totalCateres").textContent = total;
  }

  function leerFilaRegistro() {
    const fila = {};
    document.querySelectorAll(".celda-cantidad-sitio, .celda-cantidad-calibre").forEach(input => {
      fila[input.dataset.campo] = numeroCelda(input);
    });
    return fila;
  }

  function limpiarFilaRegistro() {
    document.querySelectorAll(".celda-cantidad-sitio, .celda-cantidad-calibre").forEach(input => {
      input.value = "0";
    });
    document.getElementById("campoFechaRegistro").value = hoyIso();
    actualizarTotalSitios();
    actualizarTotalCateres();
  }

  // Enter en cualquier celda de la fila de captura equivale a darle clic al
  // botón ✓: es el atajo natural de una hoja de cálculo (capturar y avanzar
  // sin soltar el teclado).
  async function guardarRegistroSitios() {
    const errorBox = document.getElementById("errorRegistro");
    errorBox.classList.add("d-none");

    const fecha = document.getElementById("campoFechaRegistro").value;
    if (!fecha) {
      errorBox.textContent = "La fecha es obligatoria.";
      errorBox.classList.remove("d-none");
      document.getElementById("campoFechaRegistro").focus();
      return;
    }

    const fila = leerFilaRegistro();
    const totalGeneral = Object.values(fila).reduce((s, n) => s + n, 0);

    if (totalGeneral === 0) {
      errorBox.textContent = "Captura al menos una cantidad mayor a cero antes de agregar la fila.";
      errorBox.classList.remove("d-none");
      return;
    }

    const boton = document.getElementById("btnGuardarRegistro");
    const iconoOriginal = boton.innerHTML;
    boton.disabled = true;
    boton.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;

    try {
      const cuerpo = {
        fecha: new Date(fecha + "T00:00:00").toISOString(),
        msd: fila.msd, msi: fila.msi, mii: fila.mii,
        calibre14: fila.calibre_14, calibre16: fila.calibre_16, calibre17: fila.calibre_17,
        calibre18: fila.calibre_18, calibre19: fila.calibre_19, calibre20: fila.calibre_20,
        calibre22: fila.calibre_22, calibre24: fila.calibre_24
      };
      const creado = await CateterApi.terapiaInfusion.guardarRegistro(cuerpo);

      limpiarFilaRegistro();
      // Sólo se inserta la fila a la vista si el historial está mostrando el
      // mes en curso (que es donde siempre cae la captura); si se está
      // viendo un mes pasado, el dato ya quedó guardado, sólo no aparece
      // hasta que se regrese al mes actual.
      const periodoVisible = leerPeriodo(document.getElementById("selectorMesRegistro"), document.getElementById("selectorAnioRegistro"));
      if (periodoVisible.anio === hoy.getFullYear() && periodoVisible.mes === hoy.getMonth() + 1) {
        agregarFilaHistorial(creado);
      }

      // El renglón recién agregado hace de confirmación: no hace falta un
      // mensaje aparte que el usuario tenga que leer y descartar.
      document.getElementById("cantMSD").focus();
    } catch (err) {
      errorBox.textContent = "Error al guardar: " + err.message;
      errorBox.classList.remove("d-none");
    } finally {
      boton.disabled = false;
      boton.innerHTML = iconoOriginal;
    }
  }

  function filaHistorialHtml(r) {
    const fecha = r.fecha ? new Date(r.fecha).toLocaleString("es-MX", { dateStyle: "short", timeStyle: "short" }) : "";
    return `
      <tr class="fila-registro-nueva" data-id="${r.id}">
        <td class="text-left text-nowrap small">${escapa(fecha)}</td>
        <td>${r.msd || 0}</td><td>${r.msi || 0}</td><td>${r.mii || 0}</td>
        <td class="table-active font-weight-bold">${r.totalSitios || 0}</td>
        <td>${r.calibre14 || 0}</td><td>${r.calibre16 || 0}</td><td>${r.calibre17 || 0}</td><td>${r.calibre18 || 0}</td>
        <td>${r.calibre19 || 0}</td><td>${r.calibre20 || 0}</td><td>${r.calibre22 || 0}</td><td>${r.calibre24 || 0}</td>
        <td class="table-active font-weight-bold">${r.totalCateteres || 0}</td>
        <td>
          <button type="button" class="btn btn-sm btn-outline-danger btn-icono-tabla btn-eliminar-registro" title="Eliminar" aria-label="Eliminar registro del ${escapa(fecha)}">
            <i class="typcn typcn-trash" aria-hidden="true"></i>
          </button>
        </td>
      </tr>`;
  }

  function agregarFilaHistorial(registro) {
    const tbody = document.getElementById("tablaHistorialRegistros");
    const vacia = tbody.querySelector(".fila-vacia-historial");
    if (vacia) vacia.remove();
    tbody.insertAdjacentHTML("afterbegin", filaHistorialHtml(registro));
    // Destaca un instante la fila que se acaba de agregar, justo debajo de
    // la de captura, para que se note dónde "cayó" sin depender de un toast.
    const nueva = tbody.querySelector(".fila-registro-nueva");
    requestAnimationFrame(() => nueva.classList.add("fila-resaltada"));
    setTimeout(() => nueva.classList.remove("fila-registro-nueva", "fila-resaltada"), 1600);
  }

  async function cargarHistorialRegistros() {
    const tbody = document.getElementById("tablaHistorialRegistros");
    tbody.innerHTML = `<tr class="fila-vacia-historial"><td colspan="15" class="text-center text-muted small">Cargando…</td></tr>`;
    try {
      const periodo = leerPeriodo(document.getElementById("selectorMesRegistro"), document.getElementById("selectorAnioRegistro"));
      const resultado = await CateterApi.terapiaInfusion.listarRegistros({ ...periodo, tamano: 500 });
      const datos = resultado.datos || [];
      tbody.innerHTML = datos.length === 0
        ? `<tr class="fila-vacia-historial"><td colspan="15" class="text-center text-muted small">Sin registros en este periodo.</td></tr>`
        : datos.map(filaHistorialHtml).join("");
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="15" class="text-center text-danger small">Error al cargar historial: ${escapa(err.message)}</td></tr>`;
    }
  }

  async function eliminarRegistroGuardado(fila) {
    const id = fila.dataset.id;
    if (!confirm("¿Eliminar este registro?")) return;

    try {
      await CateterApi.terapiaInfusion.eliminarRegistro(id);
      const tbody = fila.parentElement;
      fila.remove();
      if (!tbody.querySelector("tr")) {
        tbody.innerHTML = `<tr class="fila-vacia-historial"><td colspan="15" class="text-center text-muted small">Sin registros en este periodo.</td></tr>`;
      }
    } catch (err) {
      alert("Error al eliminar: " + err.message);
    }
  }

  // ------------------------------------------------------------------
  // Pestaña 2: eventos diarios — misma mecánica que la pestaña 1 (fila de
  // captura fija arriba, Enter/✓ agrega abajo). "Personas que instalaron" es
  // sólo un número, no el detalle de quién.
  // ------------------------------------------------------------------
  function numeroCeldaEvento(input) {
    const n = parseInt(input.value, 10);
    return Number.isFinite(n) && n >= 0 ? n : 0;
  }

  function leerFilaEvento() {
    const fila = {};
    document.querySelectorAll(".celda-evento").forEach(input => {
      fila[input.dataset.campo] = numeroCeldaEvento(input);
    });
    return fila;
  }

  function limpiarFilaEvento() {
    document.querySelectorAll(".celda-evento").forEach(input => { input.value = "0"; });
    document.getElementById("campoFecha").value = hoyIso();
  }

  async function guardarEventoDiario() {
    const errorBox = document.getElementById("errorEvento");
    errorBox.classList.add("d-none");

    const fecha = document.getElementById("campoFecha").value;
    if (!fecha) {
      errorBox.textContent = "La fecha es obligatoria.";
      errorBox.classList.remove("d-none");
      document.getElementById("campoFecha").focus();
      return;
    }

    const fila = leerFilaEvento();
    if (fila.totalEventos === 0 && fila.totalIntervenciones === 0) {
      errorBox.textContent = "Captura al menos el total de eventos o de intervenciones.";
      errorBox.classList.remove("d-none");
      return;
    }

    const boton = document.getElementById("btnGuardarEventos");
    const iconoOriginal = boton.innerHTML;
    boton.disabled = true;
    boton.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;

    try {
      const cuerpo = {
        fecha: new Date(fecha + "T00:00:00").toISOString(),
        totalEventos: fila.totalEventos,
        totalCateterColocados: fila.totalCateterColocados,
        personasInstalaron: fila.personasInstalaron,
        totalIntervenciones: fila.totalIntervenciones
      };
      const creado = await CateterApi.terapiaInfusion.guardarEvento(cuerpo);

      limpiarFilaEvento();
      const periodoVisible = leerPeriodo(document.getElementById("selectorMesEvento"), document.getElementById("selectorAnioEvento"));
      if (periodoVisible.anio === hoy.getFullYear() && periodoVisible.mes === hoy.getMonth() + 1) {
        agregarFilaEventoHistorial(creado);
      }
      document.getElementById("campoTotalEventos").focus();
    } catch (err) {
      errorBox.textContent = "Error al guardar: " + err.message;
      errorBox.classList.remove("d-none");
    } finally {
      boton.disabled = false;
      boton.innerHTML = iconoOriginal;
    }
  }

  function filaEventoHistorialHtml(e) {
    const fecha = e.fecha ? new Date(e.fecha).toLocaleDateString("es-MX") : "";
    return `
      <tr class="fila-evento-nueva" data-id="${e.id}">
        <td class="text-left small">${escapa(fecha)}</td>
        <td>${e.totalEventos || 0}</td>
        <td>${e.totalCateterColocados || 0}</td>
        <td>${e.personasInstalaron || 0}</td>
        <td>${e.totalIntervenciones || 0}</td>
        <td>
          <button type="button" class="btn btn-sm btn-outline-danger btn-icono-tabla btn-eliminar-evento" title="Eliminar" aria-label="Eliminar evento del ${escapa(fecha)}">
            <i class="typcn typcn-trash" aria-hidden="true"></i>
          </button>
        </td>
      </tr>`;
  }

  function agregarFilaEventoHistorial(evento) {
    const tbody = document.getElementById("tablaEventosGuardados");
    const vacia = tbody.querySelector(".fila-vacia");
    if (vacia) vacia.remove();
    tbody.insertAdjacentHTML("afterbegin", filaEventoHistorialHtml(evento));
    const nueva = tbody.querySelector(".fila-evento-nueva");
    requestAnimationFrame(() => nueva.classList.add("fila-resaltada"));
    setTimeout(() => nueva.classList.remove("fila-evento-nueva", "fila-resaltada"), 1600);
  }

  async function cargarEventosGuardados() {
    const tbody = document.getElementById("tablaEventosGuardados");
    tbody.innerHTML = `<tr class="fila-vacia"><td colspan="6" class="text-center text-muted small">Cargando…</td></tr>`;
    try {
      const periodo = leerPeriodo(document.getElementById("selectorMesEvento"), document.getElementById("selectorAnioEvento"));
      const resultado = await CateterApi.terapiaInfusion.listarEventos({ ...periodo, tamano: 500 });
      const datos = resultado.datos || [];
      tbody.innerHTML = datos.length === 0
        ? `<tr class="fila-vacia"><td colspan="6" class="text-center text-muted small">Sin eventos en este periodo.</td></tr>`
        : datos.map(filaEventoHistorialHtml).join("");
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger small">Error al cargar eventos: ${escapa(err.message)}</td></tr>`;
    }
  }

  async function eliminarEventoGuardado(fila) {
    const id = fila.dataset.id;
    if (!confirm("¿Eliminar este evento?")) return;

    try {
      await CateterApi.terapiaInfusion.eliminarEvento(id);
      const tbody = fila.parentElement;
      fila.remove();
      if (!tbody.querySelector("tr")) {
        tbody.innerHTML = `<tr class="fila-vacia"><td colspan="6" class="text-center text-muted small">Sin eventos todavía.</td></tr>`;
      }
    } catch (err) {
      alert("Error al eliminar: " + err.message);
    }
  }

  // ------------------------------------------------------------------
  // Pestaña 3: Reportes — gráficas dinámicas sobre lo capturado en las
  // otras 2 pestañas. Se agrupa por fecha en el navegador (los datos ya
  // vienen filtrados por año desde /reportes, así que no es mucho volumen).
  // ------------------------------------------------------------------
  const graficas = {};

  function destruirGrafica(id) {
    if (graficas[id]) { graficas[id].destroy(); graficas[id] = null; }
  }

  function agruparPorFecha(items, campos) {
    const porFecha = new Map();
    items.forEach(item => {
      const clave = (item.fecha || "").split("T")[0];
      if (!clave) return;
      if (!porFecha.has(clave)) {
        const inicial = { fecha: clave };
        campos.forEach(c => { inicial[c] = 0; });
        porFecha.set(clave, inicial);
      }
      const acumulado = porFecha.get(clave);
      campos.forEach(c => { acumulado[c] += item[c] || 0; });
    });
    return Array.from(porFecha.values()).sort((a, b) => a.fecha.localeCompare(b.fecha));
  }

  function etiquetaFechaCorta(iso) {
    const [anio, mes, dia] = iso.split("-").map(Number);
    return `${dia} ${MESES_CORTOS[mes - 1]}`;
  }

  function actualizarKpis(registros, eventos) {
    const totalSitios = registros.reduce((s, r) => s + (r.totalSitios || 0), 0);
    const totalCateteres = registros.reduce((s, r) => s + (r.totalCateteres || 0), 0);
    const totalEventos = eventos.reduce((s, e) => s + (e.totalEventos || 0), 0);
    const totalIntervenciones = eventos.reduce((s, e) => s + (e.totalIntervenciones || 0), 0);
    document.getElementById("kpiTotalSitios").textContent = totalSitios;
    document.getElementById("kpiTotalCateteres").textContent = totalCateteres;
    document.getElementById("kpiTotalEventos").textContent = totalEventos;
    document.getElementById("kpiTotalIntervenciones").textContent = totalIntervenciones;
  }

  function renderizarGraficaEventosTiempo(eventos) {
    destruirGrafica("eventosTiempo");
    const porDia = agruparPorFecha(eventos, ["totalEventos", "totalCateterColocados", "totalIntervenciones"]);
    const ctx = document.getElementById("graficaEventosTiempo");
    if (porDia.length === 0) { return; }

    graficas.eventosTiempo = new Chart(ctx, {
      type: "line",
      data: {
        labels: porDia.map(d => etiquetaFechaCorta(d.fecha)),
        datasets: [
          { label: "Total eventos", data: porDia.map(d => d.totalEventos), borderColor: PALETA[0], backgroundColor: "transparent", borderWidth: 2, pointRadius: 3 },
          { label: "Catéteres colocados", data: porDia.map(d => d.totalCateterColocados), borderColor: PALETA[1], backgroundColor: "transparent", borderWidth: 2, borderDash: [6, 3], pointRadius: 3 },
          { label: "Intervenciones", data: porDia.map(d => d.totalIntervenciones), borderColor: PALETA[2], backgroundColor: "transparent", borderWidth: 2, borderDash: [2, 2], pointRadius: 3 }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        legend: { position: "bottom", labels: { fontColor: TINTA_MUTED } },
        scales: {
          xAxes: [{ gridLines: { display: false }, ticks: { fontColor: TINTA_MUTED, maxRotation: 0 } }],
          yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, precision: 0, fontColor: TINTA_MUTED } }]
        }
      }
    });
  }

  function renderizarGraficaSitiosCateteresTiempo(registros) {
    destruirGrafica("sitiosCateteresTiempo");
    const porDia = agruparPorFecha(registros, ["totalSitios", "totalCateteres"]);
    const ctx = document.getElementById("graficaSitiosCateteresTiempo");
    if (porDia.length === 0) { return; }

    graficas.sitiosCateteresTiempo = new Chart(ctx, {
      type: "line",
      data: {
        labels: porDia.map(d => etiquetaFechaCorta(d.fecha)),
        datasets: [
          { label: "Sitios", data: porDia.map(d => d.totalSitios), borderColor: PALETA[0], backgroundColor: "transparent", borderWidth: 2, pointRadius: 3 },
          { label: "Catéteres", data: porDia.map(d => d.totalCateteres), borderColor: PALETA[1], backgroundColor: "transparent", borderWidth: 2, borderDash: [6, 3], pointRadius: 3 }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        legend: { position: "bottom", labels: { fontColor: TINTA_MUTED } },
        scales: {
          xAxes: [{ gridLines: { display: false }, ticks: { fontColor: TINTA_MUTED, maxRotation: 0 } }],
          yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, precision: 0, fontColor: TINTA_MUTED } }]
        }
      }
    });
  }

  function renderizarGraficaPorSitio(registros) {
    destruirGrafica("porSitio");
    const totales = { MSD: 0, MSI: 0, MII: 0 };
    registros.forEach(r => { totales.MSD += r.msd || 0; totales.MSI += r.msi || 0; totales.MII += r.mii || 0; });

    graficas.porSitio = new Chart(document.getElementById("graficaPorSitio"), {
      type: "bar",
      data: {
        labels: Object.keys(totales),
        datasets: [{ label: "Catéteres", data: Object.values(totales), backgroundColor: PALETA[0], maxBarThickness: 42 }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        legend: { display: false },
        scales: {
          xAxes: [{ gridLines: { display: false }, ticks: { fontColor: TINTA_MUTED } }],
          yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, precision: 0, fontColor: TINTA_MUTED } }]
        }
      }
    });
  }

  function renderizarGraficaPorCalibre(registros) {
    destruirGrafica("porCalibre");
    const claves = ["calibre14", "calibre16", "calibre17", "calibre18", "calibre19", "calibre20", "calibre22", "calibre24"];
    const etiquetas = ["14", "16", "17", "18", "19", "20", "22", "24"];
    const valores = claves.map(c => registros.reduce((s, r) => s + (r[c] || 0), 0));

    graficas.porCalibre = new Chart(document.getElementById("graficaPorCalibre"), {
      type: "bar",
      data: {
        labels: etiquetas,
        datasets: [{ label: "Catéteres", data: valores, backgroundColor: PALETA[3], maxBarThickness: 32 }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        legend: { display: false },
        scales: {
          xAxes: [{ gridLines: { display: false }, ticks: { fontColor: TINTA_MUTED } }],
          yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, precision: 0, fontColor: TINTA_MUTED } }]
        }
      }
    });
  }

  async function cargarReportes() {
    const mensajeVacio = document.getElementById("mensajeSinDatosReportes");
    try {
      const anio = parseInt(document.getElementById("selectorAnioReportes").value, 10);
      const resultado = await CateterApi.terapiaInfusion.reportes({ anio });
      const registros = resultado.registros || [];
      const eventos = resultado.eventos || [];

      actualizarKpis(registros, eventos);
      renderizarGraficaEventosTiempo(eventos);
      renderizarGraficaSitiosCateteresTiempo(registros);
      renderizarGraficaPorSitio(registros);
      renderizarGraficaPorCalibre(registros);

      mensajeVacio.style.display = (registros.length === 0 && eventos.length === 0) ? "" : "none";
    } catch (err) {
      mensajeVacio.textContent = "No se pudieron cargar los reportes: " + err.message;
      mensajeVacio.style.display = "";
    }
  }

  // Ambos campos de fecha de captura quedan acotados al mes en curso (ni
  // pasado ni futuro): min = día 1 del mes, max = hoy. El backend valida lo
  // mismo, esto es sólo para no dejar elegir una fecha inválida desde el
  // calendario nativo.
  function restringirFechaAlMesActual(input) {
    const primerDia = new Date(hoy.getFullYear(), hoy.getMonth(), 1).toISOString().split("T")[0];
    input.min = primerDia;
    input.max = hoyIso();
  }

  document.addEventListener("DOMContentLoaded", async () => {
    document.querySelectorAll(".celda-cantidad-sitio").forEach(c => c.addEventListener("input", actualizarTotalSitios));
    document.querySelectorAll(".celda-cantidad-calibre").forEach(c => c.addEventListener("input", actualizarTotalCateres));
    document.getElementById("btnGuardarRegistro").addEventListener("click", guardarRegistroSitios);
    document.getElementById("campoFechaRegistro").value = hoyIso();
    restringirFechaAlMesActual(document.getElementById("campoFechaRegistro"));
    // Enter en cualquier celda de la fila de captura guarda, como en Excel;
    // Tab sigue moviéndose libremente entre celdas para capturar de corrido.
    document.getElementById("filaCapturaRegistro").addEventListener("keydown", (ev) => {
      if (ev.key === "Enter" && ev.target.matches("input")) {
        ev.preventDefault();
        guardarRegistroSitios();
      }
    });

    llenarSelectorMes(document.getElementById("selectorMesRegistro"));
    llenarSelectorAnio(document.getElementById("selectorAnioRegistro"), hoy.getFullYear() - 4);
    document.getElementById("selectorMesRegistro").addEventListener("change", cargarHistorialRegistros);
    document.getElementById("selectorAnioRegistro").addEventListener("change", cargarHistorialRegistros);
    document.getElementById("btnExportarRegistros").addEventListener("click", () => {
      const periodo = leerPeriodo(document.getElementById("selectorMesRegistro"), document.getElementById("selectorAnioRegistro"));
      window.open(CateterApi.terapiaInfusion.urlExportarRegistros(periodo), "_blank");
    });
    await cargarHistorialRegistros();
    document.getElementById("tablaHistorialRegistros").addEventListener("click", (ev) => {
      const btn = ev.target.closest(".btn-eliminar-registro");
      if (btn) eliminarRegistroGuardado(btn.closest("tr"));
    });

    document.getElementById("campoFecha").value = hoyIso();
    restringirFechaAlMesActual(document.getElementById("campoFecha"));
    document.getElementById("btnGuardarEventos").addEventListener("click", guardarEventoDiario);
    document.getElementById("filaCapturaEvento").addEventListener("keydown", (ev) => {
      if (ev.key === "Enter" && ev.target.matches("input")) {
        ev.preventDefault();
        guardarEventoDiario();
      }
    });

    document.getElementById("tablaEventosGuardados").addEventListener("click", (ev) => {
      const btn = ev.target.closest(".btn-eliminar-evento");
      if (btn) eliminarEventoGuardado(btn.closest("tr"));
    });

    llenarSelectorMes(document.getElementById("selectorMesEvento"));
    llenarSelectorAnio(document.getElementById("selectorAnioEvento"), hoy.getFullYear() - 4);
    document.getElementById("selectorMesEvento").addEventListener("change", cargarEventosGuardados);
    document.getElementById("selectorAnioEvento").addEventListener("change", cargarEventosGuardados);
    document.getElementById("btnExportarEventos").addEventListener("click", () => {
      const periodo = leerPeriodo(document.getElementById("selectorMesEvento"), document.getElementById("selectorAnioEvento"));
      window.open(CateterApi.terapiaInfusion.urlExportarEventos(periodo), "_blank");
    });
    await cargarEventosGuardados();

    // Reportes: se carga hasta que se ve la pestaña (Chart.js no dibuja bien
    // en un canvas que está dentro de un tab-pane oculto con display:none —
    // el lienzo mide 0x0 en ese momento), y de nuevo cada vez que se vuelve
    // a mostrar tras cambiar el año.
    llenarSelectorAnio(document.getElementById("selectorAnioReportes"), hoy.getFullYear() - 4);
    let reportesCargados = false;
    $('a[href="#pestaña-reportes"]').on("shown.bs.tab", () => {
      if (!reportesCargados) { reportesCargados = true; cargarReportes(); }
    });
    document.getElementById("selectorAnioReportes").addEventListener("change", cargarReportes);
    document.getElementById("btnActualizarReportes").addEventListener("click", cargarReportes);
  });
})();
