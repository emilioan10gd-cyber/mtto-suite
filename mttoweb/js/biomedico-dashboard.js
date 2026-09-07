(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);
  const PALETA = ["#2a78d6", "#eb6834", "#1baf7a", "#eda100", "#8e44ad", "#d64545", "#3d4351"];
  const GRIS_EJE = "#e1e0d9";
  const TINTA_MUTED = "#898781";

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML;
  }

  async function cargar() {
    const estado = document.getElementById("estadoCarga");
    try {
      const datos = await BiomedicoApi.dashboard.todo();
      const k = datos.kpis || {};

      document.getElementById("kpiActivos").textContent = formateaNumero(k.equiposActivos);
      document.getElementById("kpiBaja").textContent = formateaNumero(k.equiposDeBaja);
      document.getElementById("kpiVencidos").textContent = formateaNumero(k.mantenimientosVencidos);
      document.getElementById("kpiProximos").textContent = formateaNumero(k.mantenimientosProximos30d);
      document.getElementById("kpiInsumos").textContent = formateaNumero(k.insumosBajoMinimo);
      document.getElementById("kpiLotes").textContent = formateaNumero(k.lotesPorVencer);
      document.getElementById("kpiFallas").textContent = formateaNumero(k.fallasAbiertas);

      const porArea = datos.porArea || [];
      new Chart(document.getElementById("graficaPorArea"), {
        type: "bar",
        data: {
          labels: porArea.map(a => a.area),
          datasets: [{ label: "Equipos", data: porArea.map(a => a.totalEquipos), backgroundColor: PALETA[0], maxBarThickness: 28 }]
        },
        options: {
          legend: { display: false },
          scales: {
            xAxes: [{ gridLines: { display: false }, ticks: { autoSkip: false, maxRotation: 60, minRotation: 40, fontColor: TINTA_MUTED } }],
            yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, precision: 0, fontColor: TINTA_MUTED } }]
          }
        }
      });

      const porEstado = datos.porEstado || [];
      new Chart(document.getElementById("graficaPorEstado"), {
        type: "pie",
        data: {
          labels: porEstado.map(e => e.estado),
          datasets: [{ data: porEstado.map(e => e.totalEquipos), backgroundColor: porEstado.map((_, i) => PALETA[i % PALETA.length]) }]
        },
        options: { legend: { position: "bottom", labels: { fontColor: TINTA_MUTED } } }
      });

      document.getElementById("tablaMantenimientos").innerHTML = (datos.proximosMantenimientos || []).map(m => `
        <tr>
          <td><a href="biomedico-equipo.html?id=${m.equipoId}">${escapa(m.equipo)}</a></td>
          <td>${escapa(m.tipo)}</td>
          <td>${new Date(m.fechaProgramada).toLocaleDateString("es-MX")}</td>
          <td>${escapa(m.estado)}</td>
        </tr>`).join("") || `<tr><td colspan="4">Sin mantenimientos próximos</td></tr>`;

      document.getElementById("tablaInsumos").innerHTML = (datos.insumosBajoStock || []).map(i => `
        <tr>
          <td>${escapa(i.nombre)}</td>
          <td class="text-danger font-weight-bold">${formateaNumero(i.existenciaTotal)}</td>
          <td>${formateaNumero(i.stockMinimo)}</td>
        </tr>`).join("") || `<tr><td colspan="3">Sin insumos bajo mínimo</td></tr>`;

      estado.textContent = "";
    } catch (err) {
      estado.textContent = "No se pudo conectar con la API (" + err.message + ").";
      estado.classList.add("text-danger");
    }
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", cargar);
  else setTimeout(cargar, 100);
})();
