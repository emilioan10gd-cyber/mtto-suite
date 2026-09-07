using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace mtto.Utilidades
{
    /// <summary>
    /// Reproduce el formato visual de MTTO_COMPLETE_DASHBOARD.xlsx (el Excel del que
    /// nació esta app): banda de color con título grande, subtítulo, encabezados
    /// blancos en negrita sobre la misma banda, filas de datos en cursiva gris.
    /// Cada hoja exportada solo cambia el color de banda, el título y las columnas.
    /// </summary>
    public static class ExcelExportHelper
    {
        private static readonly Color ColorBordeEncabezado = ColorTranslator.FromHtml("#1F3864");
        private static readonly Color ColorTextoSubtitulo = ColorTranslator.FromHtml("#D9E2F3");
        private static readonly Color ColorTextoDatos = ColorTranslator.FromHtml("#808080");

        /// <summary>
        /// Crea la hoja con título (fila 1), subtítulo (fila 2), fila 3 vacía de
        /// respiro y encabezados (fila 4, arrancando en columna B como el original,
        /// dejando A como margen angosto). Los datos se escriben desde la fila 5.
        /// </summary>
        public static ExcelWorksheet CrearHojaConEncabezado(
            ExcelPackage paquete, string nombreHoja, string tituloGrande, string subtitulo,
            string colorHex, (string Encabezado, double Ancho)[] columnas)
        {
            var hoja = paquete.Workbook.Worksheets.Add(nombreHoja);
            var colorBanda = ColorTranslator.FromHtml(colorHex);
            var ultimaColumna = columnas.Length + 1; // +1 porque arranca en B

            hoja.Column(1).Width = 2.43;

            var celdaTitulo = hoja.Cells[1, 1, 1, ultimaColumna];
            celdaTitulo.Merge = true;
            hoja.Cells[1, 1].Value = tituloGrande;
            celdaTitulo.Style.Font.Size = 18;
            celdaTitulo.Style.Font.Bold = true;
            celdaTitulo.Style.Font.Color.SetColor(Color.White);
            celdaTitulo.Style.Fill.PatternType = ExcelFillStyle.Solid;
            celdaTitulo.Style.Fill.BackgroundColor.SetColor(colorBanda);
            celdaTitulo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            celdaTitulo.Style.Indent = 1;
            hoja.Row(1).Height = 31.5;

            var celdaSubtitulo = hoja.Cells[2, 1, 2, ultimaColumna];
            celdaSubtitulo.Merge = true;
            if (!string.IsNullOrEmpty(subtitulo)) hoja.Cells[2, 1].Value = subtitulo;
            celdaSubtitulo.Style.Font.Size = 10;
            celdaSubtitulo.Style.Font.Color.SetColor(ColorTextoSubtitulo);
            celdaSubtitulo.Style.Fill.PatternType = ExcelFillStyle.Solid;
            celdaSubtitulo.Style.Fill.BackgroundColor.SetColor(colorBanda);
            celdaSubtitulo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            celdaSubtitulo.Style.Indent = 1;
            hoja.Row(2).Height = 18;

            for (var i = 0; i < columnas.Length; i++)
            {
                var columna = i + 2; // B en adelante
                var celda = hoja.Cells[4, columna];
                celda.Value = columnas[i].Encabezado;
                celda.Style.Font.Bold = true;
                celda.Style.Font.Size = 10;
                celda.Style.Font.Color.SetColor(Color.White);
                celda.Style.Fill.PatternType = ExcelFillStyle.Solid;
                celda.Style.Fill.BackgroundColor.SetColor(colorBanda);
                celda.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                celda.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                celda.Style.WrapText = true;
                celda.Style.Border.BorderAround(ExcelBorderStyle.Medium, ColorBordeEncabezado);

                hoja.Column(columna).Width = columnas[i].Ancho;
            }
            hoja.Row(4).Height = 30;

            hoja.View.FreezePanes(5, 3);

            return hoja;
        }

        /// <summary>Cursiva gris: el estilo que el Excel original usa para cada fila de datos.</summary>
        public static void EstiloFilaDatos(ExcelRange rango)
        {
            rango.Style.Font.Italic = true;
            rango.Style.Font.Size = 10;
            rango.Style.Font.Color.SetColor(ColorTextoDatos);
        }
    }
}
