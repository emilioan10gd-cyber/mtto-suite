using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Web.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;

namespace mtto.Utilidades
{
    public static class CateterInventarioExcelExporter
    {
        public class FilaExportacion
        {
            public int Numero;

            public string Clave;

            public string Descripcion;

            public decimal CantidadExistente;

            public string Lote;

            public DateTime? Caducidad;

            public bool TieneLote;

            public decimal EnStock;
        }

        private static readonly Color ColorEncabezado = ColorTranslator.FromHtml("#404040");

        private static readonly Color ColorBorde = Color.Black;

        public static byte[] Generar(string hospital, DateTime fecha, IEnumerable<FilaExportacion> filas)
        {
            //IL_0000: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Expected O, but got Unknown
            ExcelPackage val = new ExcelPackage();
            try
            {
                ExcelWorksheet val2 = val.Workbook.Worksheets.Add("Inventario");
                val2.View.ShowGridLines = false;
                val2.Column(1).Width = 5.0;
                val2.Column(2).Width = 14.0;
                val2.Column(3).Width = 55.0;
                val2.Column(4).Width = 11.0;
                val2.Column(5).Width = 14.0;
                val2.Column(6).Width = 12.0;
                val2.Column(7).Width = 26.0;
                InsertarLogos(val2);
                val2.Row(1).Height = 48.0;
                ((ExcelRangeBase)val2.Cells[1, 3, 1, 7]).Merge = true;
                EscribirCeldaEncabezado(val2, 2, 1, 2, "UNIDAD HOSPITALARIA", negrita: true, (ExcelHorizontalAlignment)1);
                EscribirCeldaEncabezado(val2, 2, 3, 5, hospital, negrita: false, (ExcelHorizontalAlignment)1);
                EscribirCeldaEncabezado(val2, 2, 6, 6, "FECHA", negrita: true, (ExcelHorizontalAlignment)2);
                EscribirCeldaEncabezado(val2, 2, 7, 7, fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), negrita: false, (ExcelHorizontalAlignment)2);
                val2.Row(2).Height = 20.0;
                string[] array = new string[7] { "Nº", "CLAVE 10 DÍGITOS", "DESCRIPCIÓN ARTÍCULO COMPLETA", "CANT. EXISTENTE", "LOTE", "CADUCIDAD", "OBSERVACIONES / CANTIDAD SOLICITADA" };
                for (int i = 0; i < array.Length; i++)
                {
                    ExcelRange val3 = val2.Cells[3, 1 + i];
                    ((ExcelRangeBase)val3).Value = array[i];
                    ((ExcelRangeBase)val3).Style.Font.Bold = true;
                    ((ExcelRangeBase)val3).Style.Font.Color.SetColor(Color.White);
                    ((ExcelRangeBase)val3).Style.Fill.PatternType = (ExcelFillStyle)1;
                    ((ExcelRangeBase)val3).Style.Fill.BackgroundColor.SetColor(ColorEncabezado);
                    ((ExcelRangeBase)val3).Style.HorizontalAlignment = (ExcelHorizontalAlignment)2;
                    ((ExcelRangeBase)val3).Style.VerticalAlignment = (ExcelVerticalAlignment)1;
                    ((ExcelRangeBase)val3).Style.WrapText = true;
                    ((ExcelRangeBase)val3).Style.Border.BorderAround((ExcelBorderStyle)4, ColorBorde);
                }
                val2.Row(3).Height = 28.0;
                int num = 4;
                CultureInfo cultureInfo = new CultureInfo("es-MX");
                foreach (List<FilaExportacion> item in AgruparPorArticulo(filas))
                {
                    int filaInicio = num;
                    foreach (FilaExportacion item2 in item)
                    {
                        ((ExcelRangeBase)val2.Cells[num, 4]).Value = item2.CantidadExistente;
                        ((ExcelRangeBase)val2.Cells[num, 4]).Style.Numberformat.Format = "#,##0";
                        ((ExcelRangeBase)val2.Cells[num, 5]).Value = (item2.TieneLote ? item2.Lote : "—");
                        if (!item2.TieneLote)
                        {
                            ((ExcelRangeBase)val2.Cells[num, 6]).Value = "N/A";
                        }
                        else if (item2.Caducidad.HasValue)
                        {
                            ((ExcelRangeBase)val2.Cells[num, 6]).Value = item2.Caducidad.Value;
                            ((ExcelRangeBase)val2.Cells[num, 6]).Style.Numberformat.Format = "dd/mm/yyyy";
                        }
                        else
                        {
                            ((ExcelRangeBase)val2.Cells[num, 6]).Value = "Sin fecha";
                        }
                        ((ExcelRangeBase)val2.Cells[num, 7]).Value = DescribirObservacion(item2);
                        for (int j = 4; j <= 7; j++)
                        {
                            ((ExcelRangeBase)val2.Cells[num, j]).Style.HorizontalAlignment = (ExcelHorizontalAlignment)2;
                            ((ExcelRangeBase)val2.Cells[num, j]).Style.VerticalAlignment = (ExcelVerticalAlignment)1;
                            ((ExcelRangeBase)val2.Cells[num, j]).Style.WrapText = true;
                            ((ExcelRangeBase)val2.Cells[num, j]).Style.Border.BorderAround((ExcelBorderStyle)4, ColorBorde);
                        }
                        val2.Row(num).Height = 30.0;
                        num++;
                    }
                    int filaFin = num - 1;
                    FilaExportacion filaExportacion = item[0];
                    FusionarYEscribir(val2, filaInicio, filaFin, 1, filaExportacion.Numero.ToString(CultureInfo.InvariantCulture), (ExcelHorizontalAlignment)2);
                    FusionarYEscribir(val2, filaInicio, filaFin, 2, filaExportacion.Clave, (ExcelHorizontalAlignment)2);
                    FusionarYEscribir(val2, filaInicio, filaFin, 3, filaExportacion.Descripcion, (ExcelHorizontalAlignment)1);
                }
                num++;
                EscribirFirma(val2, num, "ELABORÓ:");
                num += 2;
                EscribirFirma(val2, num, "VALIDÓ:");
                ((ExcelRangeBase)val2.Cells[1, 1, num, 7]).Style.Font.Name = "Calibri";
                ((ExcelRangeBase)val2.Cells[1, 1, num, 7]).Style.Font.Size = 10f;
                return val.GetAsByteArray();
            }
            finally
            {
                val?.Dispose();
            }
        }

        private static List<List<FilaExportacion>> AgruparPorArticulo(IEnumerable<FilaExportacion> filas)
        {
            List<List<FilaExportacion>> list = new List<List<FilaExportacion>>();
            List<FilaExportacion> list2 = null;
            string text = null;
            foreach (FilaExportacion fila in filas)
            {
                if (text != fila.Clave)
                {
                    list2 = new List<FilaExportacion>();
                    list.Add(list2);
                    text = fila.Clave;
                }
                list2.Add(fila);
            }
            return list;
        }

        private static string DescribirObservacion(FilaExportacion f)
        {
            if (!f.TieneLote)
            {
                return "0";
            }
            string text = FormatearCantidad(f.CantidadExistente);
            if (f.EnStock > 0m)
            {
                return text + " en total (" + FormatearCantidad(f.EnStock) + " en stock)";
            }
            return text + " en total";
        }

        private static string FormatearCantidad(decimal n)
        {
            if (!(n == Math.Truncate(n)))
            {
                return n.ToString("0.###", CultureInfo.InvariantCulture);
            }
            return ((long)n).ToString(CultureInfo.InvariantCulture);
        }

        private static void FusionarYEscribir(ExcelWorksheet hoja, int filaInicio, int filaFin, int columna, string valor, ExcelHorizontalAlignment alineacion)
        {
            //IL_003d: Unknown result type (might be due to invalid IL or missing references)
            ExcelRange val = hoja.Cells[filaInicio, columna, filaFin, columna];
            ((ExcelRangeBase)val).Merge = true;
            ((ExcelRangeBase)hoja.Cells[filaInicio, columna]).Value = valor;
            ((ExcelRangeBase)val).Style.WrapText = true;
            ((ExcelRangeBase)val).Style.HorizontalAlignment = alineacion;
            ((ExcelRangeBase)val).Style.VerticalAlignment = (ExcelVerticalAlignment)1;
            ((ExcelRangeBase)val).Style.Border.BorderAround((ExcelBorderStyle)4, ColorBorde);
            if (columna == 1)
            {
                ((ExcelRangeBase)val).Style.Font.Bold = true;
            }
        }

        private static void EscribirCeldaEncabezado(ExcelWorksheet hoja, int fila, int colInicio, int colFin, string valor, bool negrita, ExcelHorizontalAlignment alineacion)
        {
            //IL_0047: Unknown result type (might be due to invalid IL or missing references)
            //IL_0060: Unknown result type (might be due to invalid IL or missing references)
            //IL_0063: Invalid comparison between Unknown and I4
            ExcelRange val = hoja.Cells[fila, colInicio, fila, colFin];
            if (colFin > colInicio)
            {
                ((ExcelRangeBase)val).Merge = true;
            }
            ((ExcelRangeBase)hoja.Cells[fila, colInicio]).Value = valor;
            ((ExcelRangeBase)val).Style.Font.Bold = negrita;
            ((ExcelRangeBase)val).Style.HorizontalAlignment = alineacion;
            ((ExcelRangeBase)val).Style.VerticalAlignment = (ExcelVerticalAlignment)1;
            ((ExcelRangeBase)val).Style.Indent = (((int)alineacion == 1) ? 1 : 0);
            ((ExcelRangeBase)val).Style.Border.BorderAround((ExcelBorderStyle)4, ColorBorde);
        }

        private static void EscribirFirma(ExcelWorksheet hoja, int fila, string etiqueta)
        {
            ((ExcelRangeBase)hoja.Cells[fila, 1]).Value = etiqueta;
            ((ExcelRangeBase)hoja.Cells[fila, 1]).Style.Font.Bold = true;
            ((ExcelRangeBase)hoja.Cells[fila, 2, fila, 4]).Merge = true;
            ((ExcelRangeBase)hoja.Cells[fila, 2, fila, 4]).Style.Border.Bottom.Style = (ExcelBorderStyle)4;
        }

        private static void InsertarLogos(ExcelWorksheet hoja)
        {
            string text = HostingEnvironment.MapPath("~/Content/Logos/");
            if (text != null)
            {
                AgregarImagen(hoja, Path.Combine(text, "gobierno-mexico.png"), "logoGobierno", 2, 2, 130, 42);
                AgregarImagen(hoja, Path.Combine(text, "imss-bienestar.png"), "logoImss", 2, 165, 150, 42);
            }
        }

        private static void AgregarImagen(ExcelWorksheet hoja, string ruta, string nombre, int fila0, int offsetXpx, int ancho, int alto)
        {
            if (!File.Exists(ruta))
            {
                return;
            }
            using (var imagen = Image.FromFile(ruta))
            {
                var dibujo = hoja.Drawings.AddPicture(nombre, imagen);
                dibujo.SetPosition(0, 4, 0, offsetXpx);
                dibujo.SetSize(ancho, alto);
            }
        }
    }
}

