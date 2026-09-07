(function () {
  const formateaMoneda = (n) =>
    new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" }).format(n || 0);
  const formateaNumero = (n) => new Intl.NumberFormat("es-MX").format(n || 0);

  // Escapa también comillas: este resultado se inserta en ATRIBUTOS
  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  // Paleta categórica validada
  const PALETA = ["#2a78d6", "#eb6834", "#1baf7a", "#eda100"];
  const GRIS_EJE = "#e1e0d9";
  const TINTA_MUTED = "#898781";

  const MESES = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

  function renderizarGraficaValorCategoria(porCategoria) {
    new Chart(document.getElementById("graficaValorCategoria"), {
      type: "bar",
      data: {
        labels: porCategoria.map(c => c.Categoria),
        datasets: [{
          label: "Total de piezas",
          data: porCategoria.map(c => c.TotalExistencias),
          backgroundColor: PALETA[0],
          maxBarThickness: 28
        }]
      },
      options: {
        legend: { display: false },
        scales: {
          xAxes: [{ gridLines: { display: false }, ticks: { autoSkip: false, maxRotation: 60, minRotation: 40, fontColor: TINTA_MUTED } }],
          yAxes: [{ gridLines: { color: GRIS_EJE }, ticks: { beginAtZero: true, fontColor: TINTA_MUTED, callback: formateaMoneda } }]
        },
        tooltips: { callbacks: { label: (item) => formateaMoneda(item.yLabel) } }
      }
    });
  }

  const COLOR_NIVEL = {
    "Sin stock":    "#3d4351",
    "Bajo minimo":  "#d64545",
    "Normal":       "#1baf7a",
    "Sobre maximo": "#eda100"
  };
  const ETIQUETA_NIVEL = {
    "Sin stock": "Sin stock",
    "Bajo minimo": "Bajo mínimo",
    "Normal": "Normal",
    "Sobre maximo": "Sobre máximo"
  };

  function renderizarGraficaPorNivel(porNivel) {
    const conDatos = porNivel.filter(n => n.TotalClaves > 0);
    const total = conDatos.reduce((s, n) => s + n.TotalClaves, 0);

    new Chart(document.getElementById("graficaPorNivel"), {
      type: "pie",
      data: {
        labels: conDatos.map(n => ETIQUETA_NIVEL[n.Nivel] || n.Nivel),
        datasets: [{
          data: conDatos.map(n => n.TotalClaves),
          backgroundColor: conDatos.map(n => COLOR_NIVEL[n.Nivel] || PALETA[0])
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
              return `${etiqueta}: ${formateaNumero(valor)} clave(s) (${pct}%)`;
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
        labels: porMes.map(m => `${MESES[m.Mes - 1]} ${m.Anio}`),
        datasets: [
          {
            label: "Entradas", data: porMes.map(m => m.UnidadesEntrada),
            borderColor: PALETA[0], backgroundColor: "transparent",
            borderWidth: 2, pointRadius: radios, pointBackgroundColor: PALETA[0]
          },
          {
            label: "Salidas", data: porMes.map(m => m.UnidadesSalida),
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
    document.getElementById("mesEntradas").textContent = formateaNumero(m.UnidadesEntrada);
    document.getElementById("mesSalidas").textContent = formateaNumero(m.UnidadesSalida);
    document.getElementById("mesTotal").textContent = formateaNumero(m.TotalMovimientos);
    document.getElementById("mesUnidadesEntrada").textContent =
      m.UnidadesEntrada ? `(${formateaNumero(m.UnidadesEntrada)} unidades)` : "";
    document.getElementById("mesUnidadesSalida").textContent =
      m.UnidadesSalida ? `(${formateaNumero(m.UnidadesSalida)} unidades)` : "";
    renderizarGraficaMovimientosMes(datosPorMes, indice);
  }

  function configurarSelectorMes(porMes) {
    datosPorMes = porMes;
    const selector = document.getElementById("selectorMes");
    selector.innerHTML = porMes
      .map((m, i) => `<option value="${i}">${MESES[m.Mes - 1]} ${m.Anio}</option>`)
      .join("");

    const actual = porMes.length - 1;
    selector.value = actual;
    selector.addEventListener("change", () => mostrarDetalleMes(parseInt(selector.value, 10)));
    mostrarDetalleMes(actual);
  }

  async function cargar() {
    const estado = document.getElementById("estadoCarga");
    try {
      const datos = await CateterApi.dashboard.todo({ topBajoStock: 10, mesesHistorial: 12 });

      document.getElementById("kpiClaves").textContent = formateaNumero(datos.kpis.totalClaves);
      document.getElementById("kpiAlmacen").textContent = formateaNumero(datos.kpis.piezasEnAlmacen);
      document.getElementById("kpiStock").textContent = formateaNumero(datos.kpis.piezasEnStock);
      document.getElementById("kpiPorCaducar").textContent = formateaNumero(datos.kpis.piezasPorVencer90);

      renderizarGraficaValorCategoria(datos.porCategoria);
      renderizarGraficaPorNivel(datos.porNivel);
      renderizarGraficaMovimientosMes(datos.porMes, datos.porMes.length - 1);
      configurarSelectorMes(datos.porMes);

      const tbodyCategorias = document.getElementById("tablaCategorias");
      tbodyCategorias.innerHTML = datos.porCategoria.map(c => `
        <tr>
          <td>${escapa(c.Categoria)}</td>
          <td>${formateaNumero(c.TotalArticulos)}</td>
          <td>${formateaNumero(c.TotalExistencias)}</td>
          <td>${c.BajoMinimo > 0 ? `<span class="text-danger font-weight-bold">${c.BajoMinimo}</span>` : "0"}</td>
        </tr>`).join("") || `<tr><td colspan="4">Sin datos</td></tr>`;

      const tbodyBajoStock = document.getElementById("tablaBajoStock");
      tbodyBajoStock.innerHTML = datos.bajoStock.map(a => `
        <tr>
          <td class="text-nowrap"><span style="font-family:monospace">${escapa(a.Clave)}</span></td>
          <td>${escapa(a.Nombre)}</td>
          <td class="text-danger font-weight-bold">${formateaNumero(a.Total)}</td>
          <td>${formateaNumero(a.StockMinimo)}</td>
        </tr>`).join("") || `<tr><td colspan="4">Sin claves bajo mínimo</td></tr>`;

      estado.textContent = "";
    } catch (err) {
      estado.textContent = "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      estado.classList.add("text-danger");
    }
  }

  document.addEventListener("DOMContentLoaded", cargar);
})();
