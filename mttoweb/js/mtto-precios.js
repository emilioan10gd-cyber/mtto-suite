(function () {
  // Inyector: asegurar que window.MttoApi.precios existe
  if (!window.MttoApi?.precios) {
    if (!window.MttoApi) window.MttoApi = {};
    window.MttoApi.precios = {
      porArticulo: (codigo, cantidad) => {
        const url = cantidad
          ? `/precios/por-articulo/${encodeURIComponent(codigo)}?cantidad=${cantidad}`
          : `/precios/por-articulo/${encodeURIComponent(codigo)}`;
        return fetch(window.MTTO_CONFIG.API_BASE_URL + url)
          .then(r => r.text()).then(t => t ? JSON.parse(t) : null);
      },
      crear: (datos) => fetch(window.MTTO_CONFIG.API_BASE_URL + '/precios',
        {method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(datos)})
        .then(r => r.text()).then(t => JSON.parse(t)),
      actualizar: (id, datos) => fetch(window.MTTO_CONFIG.API_BASE_URL + '/precios/' + id,
        {method: 'PUT', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(datos)})
        .then(r => r.text()).then(t => t ? JSON.parse(t) : null),
      eliminar: (id) => fetch(window.MTTO_CONFIG.API_BASE_URL + '/precios/' + id, {method: 'DELETE'})
        .then(r => r.text()).then(t => t ? JSON.parse(t) : null)
    };
  }

  const formateaMoneda = (n) =>
    new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" }).format(n || 0);

  function escapa(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML;
  }

  window.MttoPreciosModal = {
    chartInstance: null,
    articuloActual: null,

    async abrir(codigoArticulo) {
      window.MttoPreciosModal.articuloActual = codigoArticulo;
      document.getElementById("campoArticuloCodigo").value = codigoArticulo;
      document.getElementById("titleArticuloCodigo").textContent = codigoArticulo;
      document.getElementById("cantidadConsulta").value = "";

      document.getElementById("formNuevoPrecio").reset();
      document.getElementById("alertaNuevoPrecio").classList.add("d-none");

      await llenarProveedoresEnFormulario();
      await cargarComparativa(codigoArticulo, null);

      $('#modalPrecios').modal('show');
    },

    async refrescar(codigoArticulo) {
      const cantidad = document.getElementById("cantidadConsulta").value;
      await cargarComparativa(codigoArticulo, cantidad ? parseFloat(cantidad) : null);
    }
  };

  async function llenarProveedoresEnFormulario() {
    try {
      const catalogos = await window.MttoApi.catalogos.todos();
      const selectProveedor = document.getElementById("campoProveedorPrecio");
      selectProveedor.innerHTML = '<option value="">Seleccionar proveedor...</option>';
      catalogos.proveedores.forEach(p => {
        selectProveedor.insertAdjacentHTML("beforeend",
          `<option value="${escapa(p.nombre)}">${escapa(p.nombre)}</option>`);
      });
    } catch (err) {
      console.error("Error al cargar proveedores:", err);
    }
  }

  async function cargarComparativa(codigoArticulo, cantidad) {
    try {
      const url = cantidad ? `/precios/por-articulo/${encodeURIComponent(codigoArticulo)}?cantidad=${cantidad}`
                           : `/precios/por-articulo/${encodeURIComponent(codigoArticulo)}`;
      const comparativa = await window.MttoApi.precios.porArticulo(codigoArticulo, cantidad);

      // Fallback: si el servidor no devuelve costoTotal, calcularlo localmente
      if (cantidad && comparativa.precios) {
        comparativa.precios.forEach(p => {
          if (!p.costoTotal && p.precioUnitario) {
            p.costoTotal = p.precioUnitario * cantidad;
          }
        });
      }
      // Fallback: si el servidor no devuelve cantidadConsultada, agregarlo
      if (!comparativa.cantidadConsultada && cantidad) {
        comparativa.cantidadConsultada = cantidad;
      }

      // Mostrar/ocultar columna de costo total
      const headerCostoTotal = document.getElementById("headerCostoTotal");
      const mostrarCosto = cantidad > 0;
      headerCostoTotal.style.display = mostrarCosto ? "table-cell" : "none";

      // Llenar resumen
      const resumenDiv = document.getElementById("resumenPrecios");
      if (comparativa.precios && comparativa.precios.length > 0) {
        // Si hay cantidad, buscar el que tiene menor costoTotal; si no, usar el que tiene menor precioUnitario
        let masBarato;
        if (cantidad > 0) {
          masBarato = comparativa.precios.reduce((a, b) =>
            (a.costoTotal || 0) < (b.costoTotal || 0) ? a : b
          );
        } else {
          masBarato = comparativa.precios[0]; // Ya viene ordenado por precioUnitario
        }

        const html = `
          <div style="font-size: 13px; line-height: 1.6;">
            <div style="margin-bottom: 12px;">
              <strong style="color: #333;">Más económico:</strong><br>
              <span style="color: #28a745; font-weight: bold; font-size: 14px;">${escapa(masBarato.proveedor)}</span>
              <span style="display: block; color: #666; font-size: 12px; margin-top: 2px;">${escapa(masBarato.tipoPrecio)}</span>
            </div>
            <div style="background: #f5f5f5; padding: 8px; border-radius: 4px; margin-bottom: 8px;">
              <div style="font-size: 11px; color: #666; margin-bottom: 4px;">Precio por unidad</div>
              <div style="font-size: 16px; font-weight: bold; color: #28a745;">
                ${formateaMoneda(masBarato.precioUnitario)}
              </div>
              ${cantidad > 0 ? `
                <div style="font-size: 11px; color: #666; margin-top: 8px; border-top: 1px solid #ddd; padding-top: 8px;">
                  Costo para ${cantidad} unidades
                </div>
                <div style="font-size: 16px; font-weight: bold; color: #28a745;">
                  ${formateaMoneda(masBarato.costoTotal || 0)}
                </div>
              ` : ''}
            </div>
            <div style="font-size: 12px; color: #666;">
              ${comparativa.precios.length} opción(es) disponible(s)
            </div>
          </div>
        `;
        resumenDiv.innerHTML = html;
      }

      // Fallback: si el servidor no devuelve costoTotal, calcularlo localmente
      if (cantidad && comparativa.precios) {
        comparativa.precios.forEach(p => {
          if (!p.costoTotal && p.precioUnitario) {
            p.costoTotal = p.precioUnitario * cantidad;
          }
        });
      }
      // Fallback: si el servidor no devuelve cantidadConsultada, agregarlo
      if (!comparativa.cantidadConsultada && cantidad) {
        comparativa.cantidadConsultada = cantidad;
      }

      // Llenar tabla
      const tbody = document.getElementById("bodyTablaPreciosComparativa");
      if (!comparativa.precios || comparativa.precios.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin precios aplicables</td></tr>';
      } else {
        tbody.innerHTML = comparativa.precios.map(p => `
          <tr ${p.esMasBarato ? 'class="table-success font-weight-bold"' : ''}>
            <td>${escapa(p.proveedor)}</td>
            <td><span class="badge badge-info">${escapa(p.tipoPrecio)}</span></td>
            <td class="text-right">${p.cantidadMinima.toLocaleString()}${p.cantidadMaxima ? '-' + p.cantidadMaxima.toLocaleString() : '+'}</td>
            <td class="text-right">${formateaMoneda(p.precioUnitario)}</td>
            ${mostrarCosto ? `<td class="text-right">${p.costoTotal ? formateaMoneda(p.costoTotal) : '—'}</td>` : ''}
            <td>${escapa(p.notas || '—')}</td>
            <td>
              <button type="button" class="btn btn-xs btn-outline-primary btn-editar-precio" data-id="${p.id}" data-proveedor="${escapa(p.proveedor)}" data-cantmin="${p.cantidadMinima}" data-cantmax="${p.cantidadMaxima || ''}" data-precio="${p.precioUnitario}" title="Editar">
                <i class="typcn typcn-pencil"></i>
              </button>
              <button type="button" class="btn btn-xs btn-outline-danger btn-eliminar-precio" data-id="${p.id}" title="Eliminar">
                <i class="typcn typcn-trash"></i>
              </button>
            </td>
          </tr>
        `).join("");

        tbody.querySelectorAll(".btn-editar-precio").forEach(btn => {
          btn.addEventListener("click", () => editarPrecio(btn.dataset.id, btn.dataset.proveedor, btn.dataset.cantmin, btn.dataset.cantmax, btn.dataset.precio, codigoArticulo));
        });

        tbody.querySelectorAll(".btn-eliminar-precio").forEach(btn => {
          btn.addEventListener("click", () => eliminarPrecio(btn.dataset.id, codigoArticulo));
        });
      }

      dibujarGrafica(comparativa, cantidad);
    } catch (err) {
      console.error("Error al cargar comparativa:", err);
      document.getElementById("bodyTablaPreciosComparativa").innerHTML =
        '<tr><td colspan="7" class="text-danger">Error al cargar: ' + escapa(err.message) + '</td></tr>';
    }
  }

  function dibujarGrafica(comparativa, cantidad) {
    const contenedor = document.getElementById("chartPreciosComparativa");
    if (!contenedor) return;

    if (!comparativa.precios || comparativa.precios.length === 0) return;

    try {
      // Limpiar contenedor
      contenedor.innerHTML = '';

      const precios = comparativa.precios.slice(0, 6);

      // Usar costo total si hay cantidad, si no usar precio unitario
      const valoresParaGrafica = precios.map(p => cantidad && p.costoTotal ? p.costoTotal : p.precioUnitario);
      const preciosMax = Math.max(...valoresParaGrafica);
      const preciosMin = Math.min(...valoresParaGrafica);
      const rango = preciosMax - preciosMin || 1;

      // Crear visualización con barras HTML/CSS
      const html = `
        <div style="padding: 10px 0;">
          <div style="display: flex; flex-direction: column; gap: 12px;">
            ${precios.map((p, i) => {
              const valor = cantidad && p.costoTotal ? p.costoTotal : p.precioUnitario;
              const pct = ((valor - preciosMin) / rango * 100) || 0;
              const esBarato = p.esMasBarato ? 'rgb(40, 167, 69)' : 'rgb(0, 123, 255)';
              const nombreCorto = p.proveedor.substring(0, 12);
              return `
                <div style="display: flex; align-items: center; gap: 8px;">
                  <div style="min-width: 80px; font-size: 12px; font-weight: 500; text-align: right;">
                    ${nombreCorto}
                  </div>
                  <div style="flex: 1; display: flex; align-items: center;">
                    <div style="background: ${esBarato}; height: 24px; border-radius: 3px; width: ${Math.max(pct, 5)}%; display: flex; align-items: center; justify-content: flex-end; padding-right: 6px; color: white; font-size: 11px; font-weight: bold;">
                      ${pct > 15 ? formateaMoneda(valor) : ''}
                    </div>
                    ${pct <= 15 ? `<span style="margin-left: 6px; font-size: 11px; font-weight: bold;">${formateaMoneda(valor)}</span>` : ''}
                  </div>
                  ${p.esMasBarato ? '<span style="color: #28a745; font-weight: bold; font-size: 12px;">✓ Mejor</span>' : ''}
                </div>
              `;
            }).join('')}
          </div>
          <div style="margin-top: 12px; font-size: 12px; color: #666; text-align: center;">
            ${cantidad ? `Costo total para ${cantidad} unidades` : 'Precio por unidad'}
          </div>
        </div>
      `;

      contenedor.innerHTML = html;
    } catch (e) {
      console.error("Error dibujando gráfica:", e.message);
    }
  }

  async function editarPrecio(precioId, proveedor, cantMin, cantMax, precio, codigoArticulo) {
    // Prellenar el formulario con los datos actuales
    document.getElementById("campoProveedorPrecio").value = proveedor;
    document.getElementById("campoCantidadMinimaP").value = cantMin;
    document.getElementById("campoCantidadMaximaP").value = cantMax || '';
    document.getElementById("campoPrecioUnitarioP").value = precio;

    // Cambiar el texto del botón a "Actualizar"
    const form = document.getElementById("formNuevoPrecio");
    const btnEnviar = form.querySelector('button[type="submit"]');
    const textoOriginal = btnEnviar.textContent;
    btnEnviar.textContent = "Actualizar precio";

    // Guardar el ID del precio siendo editado
    form.dataset.editandoId = precioId;

    // Modificar el submit del formulario para hacer PUT en lugar de POST
    const handleSubmitEditado = async (e) => {
      e.preventDefault();
      const codigo = document.getElementById("campoArticuloCodigo").value;
      const proveedorForm = document.getElementById("campoProveedorPrecio").value;
      const cantMinForm = parseFloat(document.getElementById("campoCantidadMinimaP").value);
      const cantMaxForm = document.getElementById("campoCantidadMaximaP").value ?
                          parseFloat(document.getElementById("campoCantidadMaximaP").value) : null;
      const precioForm = parseFloat(document.getElementById("campoPrecioUnitarioP").value);

      if (!codigo || !proveedorForm || isNaN(cantMinForm) || isNaN(precioForm)) {
        alert("Complete todos los campos obligatorios.");
        return;
      }

      try {
        await window.MttoApi.precios.actualizar(precioId, {
          articuloCodigo: codigo,
          proveedor: proveedorForm,
          cantidadMinima: cantMinForm,
          cantidadMaxima: cantMaxForm,
          precioUnitario: precioForm,
          notas: ""
        });

        form.reset();
        btnEnviar.textContent = textoOriginal;
        delete form.dataset.editandoId;
        form.removeEventListener("submit", handleSubmitEditado);
        form.addEventListener("submit", guardarNuevoPrecio);

        const alerta = document.getElementById("alertaNuevoPrecio");
        alerta.className = "alert alert-success";
        alerta.textContent = "Precio actualizado correctamente. Actualizando comparativa...";
        alerta.classList.remove("d-none");

        const cantidad = document.getElementById("cantidadConsulta").value;
        await cargarComparativa(codigo, cantidad ? parseFloat(cantidad) : null);
        setTimeout(() => alerta.classList.add("d-none"), 3000);
      } catch (err) {
        alert("Error al actualizar: " + err.message);
      }
    };

    form.removeEventListener("submit", guardarNuevoPrecio);
    form.addEventListener("submit", handleSubmitEditado);

    // Scroll al formulario para que sea visible
    form.scrollIntoView({ behavior: "smooth", block: "nearest" });
  }

  async function eliminarPrecio(precioId, codigoArticulo) {
    if (!confirm("¿Eliminar este precio de proveedor?")) return;

    try {
      await window.MttoApi.precios.eliminar(precioId);

      const alerta = document.getElementById("alertaNuevoPrecio");
      alerta.className = "alert alert-success";
      alerta.textContent = "Precio eliminado correctamente.";
      alerta.classList.remove("d-none");

      const cantidad = document.getElementById("cantidadConsulta").value;
      await cargarComparativa(codigoArticulo, cantidad ? parseFloat(cantidad) : null);
      setTimeout(() => alerta.classList.add("d-none"), 3000);
    } catch (err) {
      alert("Error al eliminar: " + err.message);
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("formNuevoPrecio");
    if (form) {
      form.addEventListener("submit", async (e) => {
        e.preventDefault();
        await guardarNuevoPrecio();
      });
    }

    const btnActualizar = document.getElementById("btnActualizar");
    if (btnActualizar) {
      btnActualizar.addEventListener("click", () => {
        if (window.MttoPreciosModal.articuloActual) {
          window.MttoPreciosModal.refrescar(window.MttoPreciosModal.articuloActual);
        }
      });
    }

    // Actualizar al cambiar cantidad (en tiempo real)
    const cantidadConsulta = document.getElementById("cantidadConsulta");
    if (cantidadConsulta) {
      cantidadConsulta.addEventListener("input", () => {
        if (window.MttoPreciosModal.articuloActual) {
          window.MttoPreciosModal.refrescar(window.MttoPreciosModal.articuloActual);
        }
      });
    }
  });

  async function guardarNuevoPrecio() {
    const alerta = document.getElementById("alertaNuevoPrecio");
    alerta.classList.add("d-none");

    try {
      const codigo = document.getElementById("campoArticuloCodigo").value;
      const proveedor = document.getElementById("campoProveedorPrecio").value;
      const cantMin = parseFloat(document.getElementById("campoCantidadMinimaP").value);
      const cantMax = document.getElementById("campoCantidadMaximaP").value ?
                      parseFloat(document.getElementById("campoCantidadMaximaP").value) : null;
      const precio = parseFloat(document.getElementById("campoPrecioUnitarioP").value);

      if (!codigo || !proveedor || isNaN(cantMin) || isNaN(precio)) {
        throw new Error("Complete todos los campos obligatorios.");
      }

      await window.MttoApi.precios.crear({
        articuloCodigo: codigo,
        proveedor,
        cantidadMinima: cantMin,
        cantidadMaxima: cantMax,
        precioUnitario: precio,
        notas: ""
      });

      document.getElementById("formNuevoPrecio").reset();

      alerta.className = "alert alert-success";
      alerta.textContent = "Precio registrado correctamente. Actualizando comparativa...";
      alerta.classList.remove("d-none");

      const cantidad = document.getElementById("cantidadConsulta").value;
      await cargarComparativa(codigo, cantidad ? parseFloat(cantidad) : null);
      setTimeout(() => alerta.classList.add("d-none"), 3000);
    } catch (err) {
      alerta.className = "alert alert-danger";
      alerta.textContent = "Error: " + escapa(err.message);
      alerta.classList.remove("d-none");
    }
  }
})();
