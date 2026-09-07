(function () {
  function escapa(t) {
    const d = document.createElement("div");
    d.textContent = t ?? "";
    return d.innerHTML;
  }

  async function cargarAreas() {
    const catalogos = await BiomedicoApi.catalogos.todos();
    const fArea = document.getElementById("fArea");
    catalogos.areas.forEach(a => fArea.insertAdjacentHTML("beforeend", `<option value="${escapa(a.nombre)}">${escapa(a.nombre)}</option>`));
  }

  async function reactivar(id) {
    if (!confirm("¿Reactivar este equipo como Operativo?")) return;
    await BiomedicoApi.equipos.cambiarEstado(id, "Operativo", null);
    await cargarTabla();
  }

  async function cargarTabla() {
    const estadoEl = document.getElementById("estadoCarga");
    try {
      const filtros = {
        q: document.getElementById("fBusqueda").value.trim() || undefined,
        area: document.getElementById("fArea").value || undefined
      };
      const datos = await BiomedicoApi.equipos.bajas(filtros);
      const tbody = document.getElementById("tablaBajas");
      tbody.innerHTML = datos.map(e => `
        <tr>
          <td>${escapa(e.area)}</td>
          <td>${escapa(e.nombre)}</td>
          <td>${escapa(e.marca)} ${escapa(e.modelo)}</td>
          <td>${escapa(e.numeroSerie)}</td>
          <td>${escapa(e.motivoBaja)}</td>
          <td>${new Date(e.actualizadoEn).toLocaleDateString("es-MX")}</td>
          <td class="text-right text-nowrap">
            <a class="btn btn-sm btn-outline-primary" href="biomedico-equipo.html?id=${e.id}" title="Ver expediente"><i class="typcn typcn-eye-outline"></i></a>
            <button type="button" class="btn btn-sm btn-outline-success btn-reactivar" data-id="${e.id}" title="Reactivar"><i class="typcn typcn-refresh"></i></button>
          </td>
        </tr>`).join("") || `<tr><td colspan="7">No hay equipos dados de baja.</td></tr>`;

      tbody.querySelectorAll(".btn-reactivar").forEach(b => b.addEventListener("click", () => reactivar(parseInt(b.dataset.id, 10))));
      estadoEl.textContent = "";
    } catch (err) {
      estadoEl.textContent = "No se pudo conectar con la API (" + err.message + ").";
      estadoEl.classList.add("text-danger");
    }
  }

  async function iniciar() {
    await cargarAreas();
    await cargarTabla();
    let temporizador;
    document.getElementById("fBusqueda").addEventListener("input", () => {
      clearTimeout(temporizador);
      temporizador = setTimeout(cargarTabla, 300);
    });
    document.getElementById("fArea").addEventListener("change", cargarTabla);
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", iniciar);
  else setTimeout(iniciar, 100);
})();
