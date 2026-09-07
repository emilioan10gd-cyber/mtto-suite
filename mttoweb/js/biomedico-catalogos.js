(function () {
  function escapa(t) {
    const d = document.createElement("div");
    d.textContent = t ?? "";
    return d.innerHTML;
  }

  function render(id, items) {
    document.getElementById(id).innerHTML = items.map(i =>
      `<li class="list-group-item">${escapa(i.nombre)}</li>`).join("") || `<li class="list-group-item text-muted">Sin registros</li>`;
  }

  async function cargar() {
    const estadoEl = document.getElementById("estadoCarga");
    try {
      const datos = await BiomedicoApi.catalogos.todos();
      render("listaAreas", datos.areas);
      render("listaCategoriasEquipo", datos.categoriasEquipo);
      render("listaCategoriasInsumo", datos.categoriasInsumo);
      estadoEl.textContent = "";
    } catch (err) {
      estadoEl.textContent = "No se pudo conectar con la API (" + err.message + ").";
      estadoEl.classList.add("text-danger");
    }
  }

  async function agregar(inputId, fn) {
    const input = document.getElementById(inputId);
    const nombre = input.value.trim();
    if (!nombre) return;
    try {
      await fn(nombre);
      input.value = "";
      await cargar();
    } catch (err) {
      alert(err.message);
    }
  }

  function iniciar() {
    cargar();
    document.getElementById("btnAgregarArea").addEventListener("click", () =>
      agregar("nuevaArea", (n) => BiomedicoApi.catalogos.crearArea(n)));
    document.getElementById("btnAgregarCategoriaEquipo").addEventListener("click", () =>
      agregar("nuevaCategoriaEquipo", (n) => BiomedicoApi.catalogos.crearCategoriaEquipo(n)));
    document.getElementById("btnAgregarCategoriaInsumo").addEventListener("click", () =>
      agregar("nuevaCategoriaInsumo", (n) => BiomedicoApi.catalogos.crearCategoriaInsumo(n)));
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", iniciar);
  else setTimeout(iniciar, 100);
})();
