(function () {
  // Escapa antes de insertar como texto/atributo. Se usa incluso en el
  // resaltado: primero se escapa TODO el texto y solo después se envuelve la
  // coincidencia en <mark>, para que un usuario no pueda inyectar HTML
  // escribiendo algo con < o " en el buscador.
  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  function escapaRegex(texto) {
    return texto.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  }

  /** Resalta las coincidencias de `termino` dentro de `textoEscapado` (ya
   *  pasado por escapa()). Trabaja sobre texto ya escapado a propósito: así
   *  el <mark> que se inserta es el único HTML real en el resultado. */
  function resalta(textoEscapado, termino) {
    if (!termino) return textoEscapado;
    const patron = new RegExp("(" + escapaRegex(escapa(termino)) + ")", "ig");
    return textoEscapado.replace(patron, "<mark>$1</mark>");
  }

  const estado = {
    ultimaConsulta: 0,      // para descartar respuestas que llegan fuera de orden
    temporizador: null,
    pagina: 1,
    termino: "",
    categorias: []
  };

  const DEBOUNCE_MS = 300;

  async function cargarCategorias() {
    const catalogos = await CateterApi.catalogos.todos();
    estado.categorias = catalogos.categorias;
    const select = document.getElementById("agregarCategoria");
    select.innerHTML = catalogos.categorias
      .map(c => `<option value="${escapa(c.nombre)}">${escapa(c.nombre)}</option>`)
      .join("");
  }

  function pintarResultado(item, termino) {
    const desc = item.descripcion;

    const accion = item.yaEnCatalogo
      ? `<span class="badge badge-success"><i class="typcn typcn-tick"></i> Ya está en tu catálogo</span>`
      : `<button type="button" class="btn btn-sm btn-outline-primary btn-agregar-cpm"
                 data-clave="${escapa(item.clave)}" data-descripcion="${escapa(item.descripcion)}">
           <i class="typcn typcn-plus"></i> Agregar
         </button>`;

    return `
      <div class="resultado-cpm d-flex justify-content-between align-items-start">
        <div class="pr-3">
          <span class="clave-cpm">${resalta(escapa(item.clave), termino)}</span>
          ${item.cantidadMensual ? `<span class="badge badge-light ml-2">CPM: ${item.cantidadMensual}/mes</span>` : ""}
          <p class="mb-0 mt-1">${resalta(escapa(desc), termino)}</p>
        </div>
        <div class="text-nowrap pt-1">${accion}</div>
      </div>`;
  }

  async function buscar(agregarAResultadosExistentes) {
    const termino = document.getElementById("cajaBusqueda").value.trim();
    const soloCateteres = document.getElementById("filtroSoloCateteres").checked;
    const contenedor = document.getElementById("resultadosBusqueda");
    const estadoTexto = document.getElementById("estadoBusqueda");
    const cargarMasWrapper = document.getElementById("cargarMasWrapper");

    if (!termino && !soloCateteres) {
      contenedor.innerHTML = `<p class="text-muted text-center mt-4">
        Escribe algo, o activa "Solo material relacionado con catéteres" para empezar a ver resultados.
      </p>`;
      estadoTexto.textContent = "";
      cargarMasWrapper.style.setProperty("display", "none", "important");
      return;
    }

    if (!agregarAResultadosExistentes) {
      estado.pagina = 1;
      estado.termino = termino;
    }

    const miConsulta = ++estado.ultimaConsulta;
    estadoTexto.innerHTML = `<span class="spinner-inline mr-1"></span> Buscando…`;

    try {
      const resultado = await CateterApi.cuadroBasico.buscar({
        q: termino || undefined,
        soloCateteres,
        pagina: estado.pagina,
        tamano: 20
      });

      // Si el usuario ya escribió algo más nuevo mientras esta respuesta
      // viajaba, se descarta: sin esto, una respuesta lenta de una búsqueda
      // vieja podría pintarse encima de una búsqueda más reciente.
      if (miConsulta !== estado.ultimaConsulta) return;

      const html = resultado.datos.map(item => pintarResultado(item, termino)).join("");
      contenedor.innerHTML = agregarAResultadosExistentes ? contenedor.innerHTML + html : html;

      if (resultado.datos.length === 0 && !agregarAResultadosExistentes) {
        contenedor.innerHTML = `<p class="text-muted text-center mt-4">Sin resultados para "${escapa(termino)}".</p>`;
      }

      estadoTexto.textContent = `${resultado.total} resultado${resultado.total === 1 ? "" : "s"}`;
      cargarMasWrapper.style.setProperty("display", estado.pagina < resultado.totalPaginas ? "flex" : "none",
        estado.pagina < resultado.totalPaginas ? "" : "important");

      contenedor.querySelectorAll(".btn-agregar-cpm").forEach(btn => {
        btn.addEventListener("click", () => abrirModalAgregar(btn.dataset.clave, btn.dataset.descripcion));
      });
    } catch (err) {
      if (miConsulta !== estado.ultimaConsulta) return;
      estadoTexto.textContent = "";
      contenedor.innerHTML = `<p class="text-danger text-center mt-4">No se pudo buscar: ${escapa(err.message)}</p>`;
    }
  }

  function buscarConDebounce() {
    clearTimeout(estado.temporizador);
    estado.temporizador = setTimeout(() => buscar(false), DEBOUNCE_MS);
  }

  let claveEnTurno = null;

  // La descripción llega en el data-attribute del botón (ya se tenía de la
  // búsqueda): evita un segundo viaje a la API solo para reabrir lo que ya
  // se había mostrado en la tarjeta de resultado.
  function abrirModalAgregar(clave, descripcion) {
    claveEnTurno = clave;
    document.getElementById("errorAgregar").classList.add("d-none");
    document.getElementById("agregarClave").textContent = clave;
    document.getElementById("agregarDescripcion").textContent = descripcion || "";
    document.getElementById("agregarNombre").value = "";
    document.getElementById("agregarStockMinimo").value = 0;

    $("#modalAgregar").modal("show");
  }

  async function agregarAlCatalogo(ev) {
    ev.preventDefault();
    const errorBox = document.getElementById("errorAgregar");
    errorBox.classList.add("d-none");

    const entrada = {
      clave: claveEnTurno,
      nombre: document.getElementById("agregarNombre").value.trim() || null,
      categoria: document.getElementById("agregarCategoria").value,
      stockMinimo: parseFloat(document.getElementById("agregarStockMinimo").value) || 0
    };

    try {
      await CateterApi.cuadroBasico.agregar(entrada);
      $("#modalAgregar").modal("hide");
      await buscar(false); // refresca para que la clave recién agregada muestre su badge
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove("d-none");
    }
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await cargarCategorias();

    document.getElementById("resultadosBusqueda").innerHTML = `<p class="text-muted text-center mt-4">
      Escribe una clave o una palabra de la descripción para buscar en el cuadro básico.
    </p>`;

    document.getElementById("cajaBusqueda").addEventListener("input", buscarConDebounce);
    document.getElementById("filtroSoloCateteres").addEventListener("change", () => buscar(false));
    document.getElementById("btnCargarMas").addEventListener("click", () => {
      estado.pagina++;
      buscar(true);
    });
    document.getElementById("formAgregar").addEventListener("submit", agregarAlCatalogo);

    // Autofocus: es lo primero que se usa al entrar a la página.
    document.getElementById("cajaBusqueda").focus();
  });
})();
