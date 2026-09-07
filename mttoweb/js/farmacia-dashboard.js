(function () {
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

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
          label: "Total de piezas",
          data: porCategoria.map(c => c.totalExistencias),
          backgroundColor: PALETA[0],
          maxBarThickness: 28
        }]
      },
      options: {
        legend: { display: false },
        scales: {
          xAxes: [{ gridLines: { display: false }, ticks: { autoSkip: false, maxRotation: 60, minRotation: 40, fontColor: TINTA_MUTED } }],
          yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, fontColor: TINTA_MUTED } }]
        }
      }
    });
  }

  const COLOR_NIVEL = {
    "Sin stock": "#3d4351", "Bajo minimo": "#d64545",
    "Normal": "#1baf7a", "Sobre maximo": "#eda100"
  };
  const ETIQUETA_NIVEL = {
    "Sin stock": "Sin stock", "Bajo minimo": "Bajo mínimo",
    "Normal": "Normal", "Sobre maximo": "Sobre máximo"
  };

  function renderizarGraficaPorNivel(porNivel) {
    const conDatos = porNivel.filter(n => n.totalMedicamentos > 0);
    const total = conDatos.reduce((s, n) => s + n.totalMedicamentos, 0);

    new Chart(document.getElementById("graficaPorNivel"), {
      type: "pie",
      data: {
        labels: conDatos.map(n => ETIQUETA_NIVEL[n.nivel] || n.nivel),
        datasets: [{
          data: conDatos.map(n => n.totalMedicamentos),
          backgroundColor: conDatos.map(n => COLOR_NIVEL[n.nivel] || PALETA[0])
        }]
      },
      options: {
        legend: { position: "bottom", labels: { fontColor: TINTA_MUTED } },
        tooltips: {
          callbacks: {
            label: (item, data) => {
              const etiqueta = data.labels[item.index];
              const valor = data.datasets[0].data[item.index];
              const pct = total ? Math.round(valor * 1000 / total) / 10 : 0;
              return `${etiqueta}: ${formateaNumero(valor)} medicamento(s) (${pct}%)`;
            }
          }
        }
      }
    });
  }

  let graficaMeses = null;
  let datosPorMes = [];

  function renderizarGraficaMovimientosMes(porMes, indiceResaltado) {
    const radios = porMes.map((_, i) => (i === indiceResaltado ? 7 : 3));

    const config = {
      type: "line",
      data: {
        labels: porMes.map(m => `${MESES[m.mes - 1]} ${m.anio}`),
        datasets: [
          {
            label: "Entradas", data: porMes.map(m => m.unidadesEntrada),
            borderColor: PALETA[0], backgroundColor: "transparent",
            borderWidth: 2, pointRadius: radios, pointBackgroundColor: PALETA[0]
          },
          {
            label: "Salidas", data: porMes.map(m => m.unidadesSalida),
            borderColor: PALETA[1], backgroundColor: "transparent",
            borderWidth: 2, pointRadius: radios, pointBackgroundColor: PALETA[1]
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
    };

    if (graficaMeses) {
      graficaMeses.data.datasets.forEach(ds => { ds.pointRadius = radios; });
      graficaMeses.update();
    } else {
      graficaMeses = new Chart(document.getElementById("graficaMovimientosMes"), config);
    }
  }

  function mostrarDetalleMes(indice) {
    const m = datosPorMes[indice];
    if (!m) return;
    document.getElementById("mesEntradas").textContent = formateaNumero(m.unidadesEntrada);
    document.getElementById("mesSalidas").textContent = formateaNumero(m.unidadesSalida);
    document.getElementById("mesTotal").textContent = formateaNumero(m.totalMovimientos);
    document.getElementById("mesUnidadesEntrada").textContent =
      m.unidadesEntrada ? `(${formateaNumero(m.unidadesEntrada)} unidades)` : "";
    document.getElementById("mesUnidadesSalida").textContent =
      m.unidadesSalida ? `(${formateaNumero(m.unidadesSalida)} unidades)` : "";
    renderizarGraficaMovimientosMes(datosPorMes, indice);
  }

  function configurarSelectorMes(porMes) {
    datosPorMes = porMes;
    const selector = document.getElementById("selectorMes");
    selector.innerHTML = porMes
      .map((m, i) => `<option value="${i}">${MESES[m.mes - 1]} ${m.anio}</option>`)
      .join("");

    const actual = porMes.length - 1;
    selector.value = actual;
    selector.addEventListener("change", () => mostrarDetalleMes(parseInt(selector.value, 10)));
    mostrarDetalleMes(actual);
  }

  async function cargar() {
    const estado = document.getElementById("estadoCarga");
    try {
      const datos = await FarmaciaApi.dashboard.todo({ topBajoStock: 10, mesesHistorial: 12 });

      document.getElementById("kpiTotal").textContent = formateaNumero(datos.kpis.totalMedicamentos);
      document.getElementById("kpiVigentes").textContent = formateaNumero(datos.kpis.piezasVigentes);
      document.getElementById("kpiBajoMinimo").textContent = formateaNumero(datos.kpis.medicamentosBajoMinimo);
      document.getElementById("kpiPorCaducar").textContent = formateaNumero(datos.kpis.piezasPorVencer90);

      renderizarGraficaValorCategoria(datos.porCategoria);
      renderizarGraficaPorNivel(datos.porNivel);
      renderizarGraficaMovimientosMes(datos.porMes, datos.porMes.length - 1);
      configurarSelectorMes(datos.porMes);

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
          <td class="text-nowrap"><span style="font-family:monospace">${escapa(a.codigo)}</span></td>
          <td>${escapa(a.nombre)}</td>
          <td class="text-danger font-weight-bold">${formateaNumero(a.total)}</td>
          <td>${formateaNumero(a.stockMinimo)}</td>
        </tr>`).join("") || `<tr><td colspan="4">Sin medicamentos bajo mínimo</td></tr>`;

      estado.textContent = "";
    } catch (err) {
      estado.textContent = "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      estado.classList.add("text-danger");
    }
  }

  document.addEventListener("DOMContentLoaded", cargar);
})();
