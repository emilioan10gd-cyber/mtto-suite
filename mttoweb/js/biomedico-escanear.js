(function () {
  let lector = null;
  let escaneando = false;

  function escapa(t) {
    const d = document.createElement("div");
    d.textContent = t ?? "";
    return d.innerHTML;
  }

  function mostrarError(msg, idCaja) {
    const el = document.getElementById(idCaja || "errorCamara");
    el.textContent = msg;
    el.classList.remove("d-none");
  }

  function ocultarError(idCaja) {
    document.getElementById(idCaja || "errorCamara").classList.add("d-none");
  }

  function mostrarExito(msg) {
    const el = document.getElementById("exitoEscaneo");
    el.textContent = msg;
    el.classList.remove("d-none");
  }

  // Un mismo código sirve para las dos etiquetas que genera biomedico-etiquetas.html
  // (el QR trae la URL completa del expediente; la barra trae "BIO-000187") y para
  // lo que alguien teclee a mano: solo el número también cuenta.
  function idDesdeTexto(texto) {
    const limpio = (texto || "").trim();

    const comoUrl = limpio.match(/biomedico-equipo\.html\?id=(\d+)/i);
    if (comoUrl) return parseInt(comoUrl[1], 10);

    const comoBarra = limpio.match(/^BIO-0*(\d+)$/i);
    if (comoBarra) return parseInt(comoBarra[1], 10);

    if (/^\d+$/.test(limpio)) return parseInt(limpio, 10);

    return null;
  }

  async function manejarResultado(texto, idCajaError) {
    const id = idDesdeTexto(texto);
    if (id == null) {
      mostrarError("Ese código no corresponde a ningún equipo de Biomédico: " + texto, idCajaError);
      return;
    }

    detener();
    try {
      const equipo = await BiomedicoApi.equipos.obtener(id);
      mostrarExito("Encontrado: " + equipo.nombre + " — abriendo expediente…");
      setTimeout(() => { location.href = "biomedico-equipo.html?id=" + id; }, 500);
    } catch (err) {
      mostrarError("El código apunta a un equipo (" + id + ") que ya no existe o no se pudo cargar: " + err.message, idCajaError);
    }
  }

  async function listarCamaras() {
    const select = document.getElementById("fCamara");
    try {
      const dispositivos = await navigator.mediaDevices.enumerateDevices();
      const camaras = dispositivos.filter(d => d.kind === "videoinput");
      select.innerHTML = camaras.map((c, i) =>
        `<option value="${escapa(c.deviceId)}">${escapa(c.label || "Cámara " + (i + 1))}</option>`).join("");
      // Prioriza la trasera en celular: suele traer "back"/"trasera"/"rear" en el label.
      const trasera = camaras.find(c => /back|trasera|rear|environment/i.test(c.label));
      if (trasera) select.value = trasera.deviceId;
    } catch { /* enumerar cámaras es best-effort; sin permiso todavía no hay labels */ }
  }

  async function iniciar() {
    ocultarError();
    document.getElementById("exitoEscaneo").classList.add("d-none");

    if (!window.ZXing) {
      mostrarError("No se pudo cargar el lector de códigos.");
      return;
    }

    // getUserMedia (acceso a cámara) solo lo permiten los navegadores en un
    // "contexto seguro": https, o localhost. Una IP de red local por http
    // (como esta) NO cuenta, así que en celular la cámara ni intenta abrir.
    // Se detecta antes de llamar a ZXing porque, si no, el error que tira el
    // navegador es un TypeError críptico ("no se puede leer mediaDevices").
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
      mostrarError(
        "Este navegador no deja abrir la cámara porque el sitio no usa una conexión segura (https). " +
        "Mientras el servidor no tenga certificado, usa la opción de escribir el código a mano, aquí abajo."
      );
      return;
    }

    const hints = new Map();
    hints.set(ZXing.DecodeHintType.POSSIBLE_FORMATS, [ZXing.BarcodeFormat.QR_CODE, ZXing.BarcodeFormat.CODE_128]);
    lector = new ZXing.BrowserMultiFormatReader(hints);

    const deviceId = document.getElementById("fCamara").value;
    const restricciones = deviceId
      ? { video: { deviceId: { exact: deviceId } } }
      : { video: { facingMode: "environment" } };

    try {
      await lector.decodeFromConstraints(restricciones, "video", (resultado, error) => {
        if (resultado && escaneando) {
          escaneando = false;
          manejarResultado(resultado.getText());
        }
        // NotFoundException se dispara en CADA cuadro sin código: es ruido normal, no un error real.
      });
      escaneando = true;
      document.getElementById("btnIniciar").classList.add("d-none");
      document.getElementById("btnDetener").classList.remove("d-none");
      await listarCamaras();
    } catch (err) {
      if (err && err.name === "NotAllowedError") {
        mostrarError("Se necesita permiso de cámara. Revisa los permisos del sitio en tu navegador.");
      } else if (err && err.name === "NotFoundError") {
        mostrarError("No se encontró ninguna cámara en este dispositivo.");
      } else {
        mostrarError("No se pudo iniciar la cámara: " + (err && err.message ? err.message : err));
      }
    }
  }

  function detener() {
    escaneando = false;
    if (lector) { lector.reset(); lector = null; }
    document.getElementById("btnIniciar").classList.remove("d-none");
    document.getElementById("btnDetener").classList.add("d-none");
  }

  function iniciarPagina() {
    document.getElementById("btnIniciar").addEventListener("click", iniciar);
    document.getElementById("btnDetener").addEventListener("click", detener);
    document.getElementById("fCamara").addEventListener("change", () => {
      if (escaneando) { detener(); iniciar(); }
    });
    listarCamaras();
    // Detener la cámara si el usuario navega a otra página sin darle "Detener".
    window.addEventListener("beforeunload", detener);

    document.getElementById("formManual").addEventListener("submit", (ev) => {
      ev.preventDefault();
      ocultarError("errorManual");
      const valor = document.getElementById("fCodigoManual").value;
      if (!valor.trim()) return;
      manejarResultado(valor, "errorManual");
    });
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", iniciarPagina);
  else setTimeout(iniciarPagina, 100);
})();
