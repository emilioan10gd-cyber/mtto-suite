using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace mtto.Utilidades
{
    public static class BioInventarioExcelExporter
    {
        public class FilaExportacion
        {
            public int Numero;
            public string Area;
            public string Nombre;
            public string Marca;
            public string Modelo;
            public string NumeroSerie;
            public string Estado;
            public string Ubicacion;
        }

        private static readonly Color ColorEncabezado = ColorTranslator.FromHtml("#404040");
        private static readonly Color ColorBorde = Color.Black;

        public static byte[] Generar(string hospital, DateTime fecha, IEnumerable<FilaExportacion> filas)
        {
            using (var paquete = new ExcelPackage())
            {
                var hoja = paquete.Workbook.Worksheets.Add("Equipos");
                hoja.View.ShowGridLines = false;
                hoja.Column(1).Width = 5;
                hoja.Column(2).Width = 18;
                hoja.Column(3).Width = 40;
                hoja.Column(4).Width = 18;
                hoja.Column(5).Width = 18;
                hoja.Column(6).Width = 20;
                hoja.Column(7).Width = 16;
                hoja.Column(8).Width = 20;

                hoja.Cells[1, 1, 1, 4].Merge = true;
                hoja.Cells[1, 1].Value = hospital + " — Inventario de Equipo Biomédico";
                hoja.Cells[1, 1].Style.Font.Bold = true;
                hoja.Cells[1, 6].Value = "FECHA";
                hoja.Cells[1, 6].Style.Font.Bold = true;
                hoja.Cells[1, 7].Value = fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                hoja.Row(1).Height = 22;

                string[] encabezados = { "Nº", "Área", "Equipo", "Marca", "Modelo", "Número de Serie", "Estado", "Ubicación" };
                for (var i = 0; i < encabezados.Length; i++)
                {
                    var celda = hoja.Cells[2, 1 + i];
                    celda.Value = encabezados[i];
                    celda.Style.Font.Bold = true;
                    celda.Style.Font.Color.SetColor(Color.White);
                    celda.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    celda.Style.Fill.BackgroundColor.SetColor(ColorEncabezado);
                    celda.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celda.Style.Border.BorderAround(ExcelBorderStyle.Thin, ColorBorde);
                }
                hoja.Row(2).Height = 20;

                var fila = 3;
                foreach (var f in filas)
                {
                    hoja.Cells[fila, 1].Value = f.Numero;
                    hoja.Cells[fila, 2].Value = f.Area;
                    hoja.Cells[fila, 3].Value = f.Nombre;
                    hoja.Cells[fila, 4].Value = f.Marca;
                    hoja.Cells[fila, 5].Value = f.Modelo;
                    hoja.Cells[fila, 6].Value = f.NumeroSerie;
                    hoja.Cells[fila, 7].Value = f.Estado;
                    hoja.Cells[fila, 8].Value = f.Ubicacion;
                    for (var c = 1; c <= 8; c++)
                        hoja.Cells[fila, c].Style.Border.BorderAround(ExcelBorderStyle.Thin, ColorBorde);
                    fila++;
                }

                hoja.Cells[1, 1, fila, 8].Style.Font.Name = "Calibri";
                hoja.Cells[1, 1, fila, 8].Style.Font.Size = 10;

                return paquete.GetAsByteArray();
            }
        }
    }
}
