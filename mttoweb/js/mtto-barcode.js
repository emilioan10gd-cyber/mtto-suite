// Generador de códigos de barras Code 128 (subconjunto B).
//
// Se escribió a mano en vez de traer una librería externa por dos razones: el
// servidor no tiene salida a internet para un CDN, y Code 128B es un formato
// corto y cerrado (una tabla de patrones + una suma de control). Cubre todo el
// ASCII imprimible, así que sirve para códigos tipo "MT-0122".
//
// Se dibuja en SVG y no en canvas: la etiqueta se imprime, y el SVG sale nítido
// a cualquier resolución de impresora mientras que un canvas se pixelea.
(function () {
  // Cada patrón describe 6 anchos alternando barra/espacio, empezando en barra.
  // El índice es el valor Code 128; el 106 (Stop) lleva 7 por definición.
  const PATRONES = [
    "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312",
    "132212", "221213", "221312", "231212", "112232", "122132", "122231", "113222",
    "123122", "123221", "223211", "221132", "221231", "213212", "223112", "312131",
    "311222", "321122", "321221", "312212", "322112", "322211", "212123", "212321",
    "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313",
    "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121",
    "313121", "211331", "231131", "213113", "213311", "213131", "311123", "311321",
    "331121", "312113", "312311", "332111", "314111", "221411", "431111", "111224",
    "111422", "121124", "121421", "141122", "141221", "112214", "112412", "122114",
    "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111",
    "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112",
    "421211", "212141", "214121", "412121", "111143", "111341", "131141", "114113",
    "114311", "411113", "411311", "113141", "114131", "311141", "411131", "211412",
    "211214", "211232", "2331112"
  ];

  const INICIO_B = 104;
  const STOP = 106;

  // Code 128B cubre ASCII 32..126. Fuera de ese rango no hay representación.
  function esCodificable(texto) {
    if (!texto) return false;
    for (let i = 0; i < texto.length; i++) {
      const c = texto.charCodeAt(i);
      if (c < 32 || c > 126) return false;
    }
    return true;
  }

  // Devuelve la lista de valores Code 128 (con inicio, control y stop).
  function calcularValores(texto) {
    const valores = [INICIO_B];
    for (let i = 0; i < texto.length; i++) {
      valores.push(texto.charCodeAt(i) - 32);
    }
    // Suma de control: inicio + Σ(posición × valor), módulo 103. La posición
    // arranca en 1 para el primer carácter de datos.
    let suma = INICIO_B;
    for (let i = 0; i < texto.length; i++) {
      suma += (i + 1) * (texto.charCodeAt(i) - 32);
    }
    valores.push(suma % 103);
    valores.push(STOP);
    return valores;
  }

  // Convierte los valores en una lista de barras {x, ancho} en módulos.
  function calcularBarras(valores) {
    const barras = [];
    let x = 0;
    valores.forEach(valor => {
      const patron = PATRONES[valor];
      for (let i = 0; i < patron.length; i++) {
        const ancho = parseInt(patron[i], 10);
        if (i % 2 === 0) barras.push({ x: x, ancho: ancho }); // par = barra
        x += ancho;
      }
    });
    return { barras: barras, anchoTotal: x };
  }

  function escaparXml(texto) {
    return String(texto)
      .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  /**
   * Genera el SVG del código de barras.
   * @param {string} texto        Lo que se va a codificar (ej. "MT-0122").
   * @param {object} [opciones]
   * @param {number} [opciones.moduloPx=2]   Ancho en px de la barra más delgada.
   * @param {number} [opciones.altoPx=50]    Alto de las barras.
   * @param {boolean}[opciones.mostrarTexto=true] Imprime el código bajo las barras.
   * @param {number} [opciones.margenPx=10]  Zona muda a los lados (la necesita el escáner).
   * @returns {string} SVG listo para insertar en el DOM.
   */
  function svg(texto, opciones) {
    const o = opciones || {};
    const modulo = o.moduloPx || 2;
    const alto = o.altoPx || 50;
    const margen = o.margenPx != null ? o.margenPx : 10;
    const mostrarTexto = o.mostrarTexto !== false;
    const altoTexto = mostrarTexto ? 14 : 0;

    if (!esCodificable(texto)) {
      return '<svg xmlns="http://www.w3.org/2000/svg" width="120" height="30">' +
             '<text x="0" y="20" font-size="11" fill="#c00">Código no válido</text></svg>';
    }

    const calc = calcularBarras(calcularValores(texto));
    const anchoSvg = calc.anchoTotal * modulo + margen * 2;
    const altoSvg = alto + altoTexto + 4;

    let rects = "";
    calc.barras.forEach(b => {
      rects += '<rect x="' + (margen + b.x * modulo) + '" y="0" width="' +
               (b.ancho * modulo) + '" height="' + alto + '" fill="#000"/>';
    });

    let etiqueta = "";
    if (mostrarTexto) {
      etiqueta = '<text x="' + (anchoSvg / 2) + '" y="' + (alto + 12) +
                 '" text-anchor="middle" font-family="monospace" font-size="12" fill="#000">' +
                 escaparXml(texto) + '</text>';
    }

    return '<svg xmlns="http://www.w3.org/2000/svg" width="' + anchoSvg + '" height="' + altoSvg +
           '" viewBox="0 0 ' + anchoSvg + ' ' + altoSvg + '">' + rects + etiqueta + '</svg>';
  }

  window.MttoBarcode = {
    svg: svg,
    esCodificable: esCodificable,
    // Se exponen para poder verificar la codificación desde pruebas.
    _valores: calcularValores,
    _patrones: PATRONES
  };
})();
