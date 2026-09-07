(function () {
  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  async function cargar() {
    const estado = document.getElementById("estadoCarga");
    try {
      const catalogos = await FarmaciaApi.catalogos.todos();

      document.getElementById("tablaCategorias").innerHTML = catalogos.categorias.map(c => `
        <tr><td><span style="font-family:monospace">${escapa(c.codigo)}</span></td><td>${escapa(c.nombre)}</td></tr>
      `).join("") || `<tr><td colspan="2">Sin categorías</td></tr>`;

      document.getElementById("tablaProveedores").innerHTML = catalogos.proveedores.map(p => `
        <tr><td><span style="font-family:monospace">${escapa(p.codigo)}</span></td><td>${escapa(p.nombre)}</td></tr>
      `).join("") || `<tr><td colspan="2">Sin proveedores</td></tr>`;

      document.getElementById("tablaTiposMovimiento").innerHTML = catalogos.tiposMovimiento.map(t => `
        <tr>
          <td>${escapa(t.nombre)}</td>
          <td>${escapa(t.clasificacion)}</td>
          <td class="text-center">${t.signo > 0 ? '<span class="text-success">+</span>' : '<span class="text-danger">−</span>'}</td>
        </tr>
      `).join("") || `<tr><td colspan="3">Sin tipos de movimiento</td></tr>`;

      estado.textContent = "";
    } catch (err) {
      estado.textContent = "No se pudo conectar con la API (" + err.message + "). ¿Está corriendo mtto en " + window.MTTO_CONFIG.API_BASE_URL + "?";
      estado.classList.add("text-danger");
    }
  }

  document.addEventListener("DOMContentLoaded", cargar);
})();
