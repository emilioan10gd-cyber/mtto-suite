(function () {
  const formateaMoneda = (n) =>
    new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" }).format(n || 0);
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML;
  }

  // Paleta categórica validada (dataviz skill): orden fijo, nunca ciclado.
  // Solo se usan 1-4 tonos aquí, que es lo máximo que estas 3 gráficas piden.
  const PALETA = ["#2a78d6", "#eb6834", "#1baf7a", "#eda100"];
  const GRIS_EJE = "#e1e0d9";
  const TINTA_MUTED = "#898781";

  const MESES = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

  function renderizarGraficaValorCategoria(porCategoria) {
    new Chart(document.getElementById("graficaValorCategoria"), {
      type: "bar",
      data: {
        labels: porCategoria.map(c => c.categoria),
        datasets: [{
          label: "Valor total",
          data: porCategoria.map(c => c.valorTotal),
          backgroundColor: PALETA[0],
          maxBarThickness: 28
        }]
      },
      options: {
        legend: { display: false }, // una sola serie: el título ya la nombra
        scales: {
          xAxes: [{ gridLines: { display: false }, ticks: { autoSkip: false, maxRotation: 60, minRotation: 40, fontColor: TINTA_MUTED } }],
          yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, fontColor: TINTA_MUTED, callback: formateaMoneda } }]
        },
        tooltips: { callbacks: { label: (item) => formateaMoneda(item.yLabel) } }
      }
    });
  }

  function renderizarGraficaPorEstado(porEstado) {
    new Chart(document.getElementById("graficaPorEstado"), {
      type: "pie",
      data: {
        labels: porEstado.map(e => e.estado),
        datasets: [{
          data: porEstado.map(e => e.totalArticulos),
          backgroundColor: PALETA.slice(0, Math.max(porEstado.length, 1))
        }]
      },
      options: {
        // Leyenda siempre visible: en un pastel la identidad no puede depender
        // solo del color (dos de estos tonos quedan por debajo de 3:1 de contraste).
        legend: { position: "bottom", labels: { fontColor: TINTA_MUTED } },
        tooltips: {
          callbacks: {
            label: (item, data) => {
              const etiqueta = data.labels[item.index];
              const valor = data.datasets[0].data[item.index];
              return `${etiqueta}: ${formateaNumero(valor)} artículo(s)`;
            }
          }
        }
      }
    });
  }

  function renderizarGraficaMovimientosMes(porMes) {
    new Chart(document.getElementById("graficaMovimientosMes"), {
      type: "line",
      data: {
        labels: porMes.map(m => `${MESES[m.mes - 1]} ${m.anio}`),
        datasets: [
          {
            label: "Entradas", data: porMes.map(m => m.entradas),
            borderColor: PALETA[0], backgroundColor: "transparent",
            borderWidth: 2, pointRadius: 3, pointBackgroundColor: PALETA[0]
          },
          {
            label: "Salidas", data: porMes.map(m => m.salidas),
            borderColor: PALETA[1], backgroundColor: "transparent",
            borderWidth: 2, pointRadius: 3, pointBackgroundColor: PALETA[1]
          }
        ]
      },
      options: {
        legend: { position: "bottom", labels: { fontColor: TINTA_MUTED } },
        scales: {
          xAxes: [{ gridLines: { display: false }, ticks: { fontColor: TINTA_MUTED } }],
          yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, precision: 0, fontColor: TINTA_MUTED } }]
        }
      }
    });
  }

  async function cargar() {
    const estado = document.getElementById("estadoCarga");
    try {
      const datos = await MttoApi.dashboard.todo({ topBajoStock: 10, mesesHistorial: 12 });

      document.getElementById("kpiTotalArticulos").textContent = formateaNumero(datos.kpis.totalArticulos);
      document.getElementById("kpiBajoMinimo").textContent = formateaNumero(datos.kpis.articulosBajoMinimo);
      document.getElementById("kpiValorTotal").textContent = formateaMoneda(datos.kpis.valorTotalInventario);
      document.getElementById("kpiMovimientosMes").textContent = formateaNumero(datos.kpis.movimientosDelMes);

      renderizarGraficaValorCategoria(datos.porCategoria);
      renderizarGraficaPorEstado(datos.porEstado);
      renderizarGraficaMovimientosMes(datos.porMes);

      const tbodyCategorias = document.getElementById("tablaCategorias");
      tbodyCategorias.innerHTML = datos.porCategoria.map(c => `
        <tr>
          <td>${escapa(c.categoria)}</td>
          <td>${formateaNumero(c.totalArticulos)}</td>
          <td>${formateaNumero(c.totalExistencias)}</td>
          <td>${c.bajoMinimo > 0 ? `<span class="text-danger font-weight-bold">${c.bajoMinimo}</span>` : "0"}</td>
        </tr>`).join("") || `<tr><td colspan="4">Sin datos</td></tr>`;

      const tbodyBajoStock = document.getElementById("tablaBajoStock");
      tbodyBajoStock.innerHTML = datos.bajoStock.map(a => `
        <tr>
          <td>${escapa(a.codigo)}</td>
          <td>${escapa(a.nombre)}</td>
          <td class="text-danger font-weight-bold">${formateaNumero(a.stockActual)}</td>
          <td>${formateaNumero(a.stockMinimo)}</td>
        </tr>`).join("") || `<tr><td colspan="4">Sin artículos bajo mínimo</td></tr>`;

      estado.textContent = "";
    } catch (err) {
      estado.textContent = "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      estado.classList.add("text-danger");
    }
  }

  document.addEventListener("DOMContentLoaded", cargar);
})();
