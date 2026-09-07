using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace mtto.Utilidades
{
    /// <summary>
    /// Extractor de texto plano de PDF, sin dependencias externas: mismo
    /// criterio que mtto-barcode.js (implementación propia en vez de una
    /// librería de terceros). No es perfecto (fuentes CID/Type0 pueden salir
    /// incompletas), pero es suficiente para búsqueda por palabras clave, que
    /// es su único uso (sugerencias de fallas a partir del manual).
    /// </summary>
    public static class PdfTextExtractor
    {
        private static readonly Regex StreamRegex = new Regex(
            @"<<(?<dict>[^>]*?)>>\s*stream\r?\n(?<data>.*?)endstream",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex TextShowRegex = new Regex(
            @"\((?<txt>(?:\\.|[^()\\])*)\)\s*Tj|\[(?<arr>(?:\\.|[^\[\]\\]|\((?:\\.|[^()\\])*\))*)\]\s*TJ",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex ArrayLiteralRegex = new Regex(
            @"\((?<txt>(?:\\.|[^()\\])*)\)", RegexOptions.Compiled);

        public static string ExtraerTexto(byte[] pdfBytes, int maxCaracteres = 2_000_000)
        {
            if (pdfBytes == null || pdfBytes.Length == 0) return null;

            // El PDF es texto ASCII para su estructura (dicts, operadores) con
            // streams binarios embebidos: Latin1 preserva 1 byte = 1 char, que es
            // lo que necesita el regex de la estructura para no corromper offsets.
            var raw = Encoding.GetEncoding("ISO-8859-1").GetString(pdfBytes);
            var sb = new StringBuilder();

            foreach (Match m in StreamRegex.Matches(raw))
            {
                var dict = m.Groups["dict"].Value;
                var data = m.Groups["data"].Value;

                if (!dict.Contains("/FlateDecode")) continue;
                // Streams de imagen no tienen texto que buscar; procesarlos solo
                // agrega ruido/tiempo.
                if (dict.Contains("/Image")) continue;

                var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(data);
                var descomprimido = DescomprimirFlate(bytes);
                if (descomprimido == null) continue;

                var contenido = Encoding.GetEncoding("ISO-8859-1").GetString(descomprimido);
                ExtraerDeContenido(contenido, sb);

                if (sb.Length > maxCaracteres) break;
            }

            var texto = sb.ToString();
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Length > maxCaracteres ? texto.Substring(0, maxCaracteres) : texto;
        }

        private static void ExtraerDeContenido(string contenido, StringBuilder sb)
        {
            foreach (Match tm in TextShowRegex.Matches(contenido))
            {
                if (tm.Groups["txt"].Success)
                {
                    sb.Append(DecodificarLiteral(tm.Groups["txt"].Value));
                    sb.Append(' ');
                }
                else if (tm.Groups["arr"].Success)
                {
                    foreach (Match lit in ArrayLiteralRegex.Matches(tm.Groups["arr"].Value))
                    {
                        sb.Append(DecodificarLiteral(lit.Groups["txt"].Value));
                    }
                    sb.Append(' ');
                }
            }
            sb.Append('\n');
        }

        /// <summary>Desescapa \), \(, \\, \n, \r, \t y secuencias octales \ddd de un literal PDF.</summary>
        private static string DecodificarLiteral(string literal)
        {
            var sb = new StringBuilder(literal.Length);
            for (var i = 0; i < literal.Length; i++)
            {
                var c = literal[i];
                if (c != '\\' || i == literal.Length - 1) { sb.Append(c); continue; }

                var next = literal[++i];
                switch (next)
                {
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case '(': sb.Append('('); break;
                    case ')': sb.Append(')'); break;
                    case '\\': sb.Append('\\'); break;
                    default:
                        if (char.IsDigit(next))
                        {
                            var octal = new string(new[] { next });
                            var consumidos = 0;
                            while (consumidos < 2 && i + 1 < literal.Length && char.IsDigit(literal[i + 1]))
                            {
                                octal += literal[++i];
                                consumidos++;
                            }
                            try { sb.Append((char)Convert.ToInt32(octal, 8)); } catch { /* secuencia inválida, se ignora */ }
                        }
                        else sb.Append(next);
                        break;
                }
            }
            return sb.ToString();
        }

        private static byte[] DescomprimirFlate(byte[] datos)
        {
            try
            {
                // zlib = 2 bytes de encabezado + deflate crudo + 4 bytes Adler32;
                // DeflateStream solo entiende el deflate crudo de en medio.
                using (var entrada = new MemoryStream(datos, 2, datos.Length - 2))
                using (var deflate = new DeflateStream(entrada, CompressionMode.Decompress))
                using (var salida = new MemoryStream())
                {
                    deflate.CopyTo(salida);
                    return salida.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
