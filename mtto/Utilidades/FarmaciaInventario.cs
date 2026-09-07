using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using mtto.Models;
using static mtto.Utilidades.LectorSql;

namespace mtto.Utilidades
{
    /// <summary>
    /// Reglas de inventario de Farmacia: FEFO y registro de movimientos.
    ///
    /// Todo lo que se captura en la app se escribe en farm_movimiento, nunca
    /// en farm_entrada_detalle / farm_salida_detalle: esas dos son el espejo
    /// del Access y la sincronizacion las vacia y recarga.
    /// </summary>
    public static class FarmaciaInventario
    {
        /// Un lote se identifica por (codigo de barras, lote). No hay un id
        /// numerico porque los lotes salen de una vista agregada, asi que se
        /// arma una llave de texto para poder mandarla y recibirla.
        public static string ArmarLoteId(string codigobarras, string lote)
        {
            return codigobarras + "|" + (lote ?? "");
        }

        public static bool PartirLoteId(string loteId, out string codigobarras, out string lote)
        {
            codigobarras = null;
            lote = null;
            if (string.IsNullOrWhiteSpace(loteId)) return false;

            var corte = loteId.IndexOf('|');
            if (corte < 0) return false;

            codigobarras = loteId.Substring(0, corte);
            lote = loteId.Substring(corte + 1);
            if (lote.Length == 0) lote = null;
            return codigobarras.Length > 0;
        }

        public static int SignoDe(string tipo)
        {
            switch ((tipo ?? "").Trim())
            {
                case "Entrada":
                case "Ajuste (+)":
                    return 1;
                default:
                    // Salida, Merma, Ajuste (-) y cualquier otro despacho.
                    return -1;
            }
        }

        private class LoteDisponible
        {
            public string Codigobarras;
            public string Lote;
            public DateTime? Caducidad;
            public decimal Existencia;
        }

        /// <summary>
        /// Lotes con existencia de una clave del cuadro basico (o de un codigo
        /// de barras suelto), ordenados FEFO: primero lo que caduca antes.
        /// Una clave puede tener varias presentaciones y todas entran al
        /// reparto, que es justo lo que se busca al despachar por caducidad.
        /// </summary>
        private static List<LoteDisponible> LotesFefo(SqlConnection cn, SqlTransaction tx, string codigo)
        {
            var lista = new List<LoteDisponible>();

            var sql = @"
                SELECT codigobarras, lote, fecha_caducidad, existencia
                FROM vw_farm_lotes
                WHERE (clave = @codigo OR codigobarras = @codigo)
                  AND existencia > 0
                ORDER BY CASE WHEN fecha_caducidad IS NULL THEN 1 ELSE 0 END,
                         fecha_caducidad,
                         existencia DESC";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@codigo", codigo ?? "");
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new LoteDisponible
                        {
                            Codigobarras = Txt(r, "codigobarras"),
                            Lote = Txt(r, "lote"),
                            Caducidad = Fecha(r, "fecha_caducidad"),
                            Existencia = Num(r, "existencia")
                        });
                    }
                }
            }

            return lista;
        }

        public class Movimiento
        {
            public string Tipo = "Salida";
            public string Folio;
            public string AreaServicio;
            public string Responsable;
            public string Observaciones;
            public string Origen = "manual";
            public int? OrigenId;
            public string Usuario;
            public DateTime? Fecha;
        }

        /// <summary>
        /// Descuenta <paramref name="cantidad"/> repartiendola entre los lotes
        /// por FEFO y deja un renglon en farm_movimiento por cada lote tocado.
        ///
        /// Si no alcanza: con permiteParcial=true entrega lo que hay y reporta
        /// el faltante (asi funciona la receta, donde el faltante ES el
        /// indicador de abasto); con false no entrega nada y avisa.
        /// </summary>
        public static FarmaciaSurtidoDto Despachar(SqlConnection cn, SqlTransaction tx,
                                                   string codigo, decimal cantidad,
                                                   Movimiento datos, bool permiteParcial)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero.");

            var disponibles = LotesFefo(cn, tx, codigo);

            decimal enLotesPositivos = 0;
            foreach (var l in disponibles) enLotesPositivos += l.Existencia;

            /* El neto del medicamento puede ser MENOR que la suma de los lotes
               con saldo, porque hay lotes en negativo: salidas capturadas con
               un lote que no empata con ninguna entrada (en el Access se
               escribio la caducidad en el campo del lote). Repartir sobre los
               lotes positivos sin mirar el neto deja la existencia negativa,
               asi que el neto manda. */
            var neto = NetoDe(cn, tx, codigo);
            var disponible = Math.Min(enLotesPositivos, neto);
            if (disponible < 0) disponible = 0;

            if (!permiteParcial && disponible < cantidad)
            {
                throw new InvalidOperationException(
                    "No hay existencia suficiente de " + codigo + ": se piden " +
                    cantidad.ToString("0.##") + " y solo hay " + disponible.ToString("0.##") + ".");
            }

            var resultado = new FarmaciaSurtidoDto { Lineas = new List<FarmaciaLineaSurtidaDto>() };
            var porRepartir = Math.Min(cantidad, disponible);
            var noSurtido = cantidad - porRepartir;
            var signo = SignoDe(datos.Tipo);

            foreach (var lote in disponibles)
            {
                if (porRepartir <= 0) break;

                var toma = Math.Min(lote.Existencia, porRepartir);
                porRepartir -= toma;

                Registrar(cn, tx, lote.Codigobarras, lote.Lote, lote.Caducidad, toma, signo, datos);

                resultado.Lineas.Add(new FarmaciaLineaSurtidaDto
                {
                    Codigobarras = lote.Codigobarras,
                    Lote = lote.Lote,
                    Caducidad = lote.Caducidad,
                    Cantidad = toma
                });
            }

            resultado.Entregado = cantidad - noSurtido - porRepartir;
            resultado.Faltante = noSurtido + porRepartir;
            return resultado;
        }

        /// <summary>
        /// Existencia neta de la clave sumando TODOS sus lotes, incluidos los
        /// que estan en negativo. Es el techo real de lo que se puede entregar.
        /// </summary>
        private static decimal NetoDe(SqlConnection cn, SqlTransaction tx, string codigo)
        {
            var sql = @"
                SELECT ISNULL(SUM(existencia), 0)
                FROM vw_farm_lotes
                WHERE clave = @codigo OR codigobarras = @codigo";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@codigo", codigo ?? "");
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        /// <summary>Inserta un renglon en farm_movimiento.</summary>
        public static int Registrar(SqlConnection cn, SqlTransaction tx,
                                    string codigobarras, string lote, DateTime? caducidad,
                                    decimal cantidad, int signo, Movimiento datos)
        {
            var sql = @"
                INSERT INTO dbo.farm_movimiento
                    (tipo, signo, codigobarras, lote, caducidad, cantidad, fecha,
                     folio, area_servicio, responsable, observaciones, origen, origen_id, usuario)
                OUTPUT INSERTED.movimiento_id
                VALUES
                    (@tipo, @signo, @cb, @lote, @cad, @cant, @fecha,
                     @folio, @area, @resp, @obs, @origen, @origenId, @usuario)";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@tipo", datos.Tipo ?? "Salida");
                cmd.Parameters.AddWithValue("@signo", signo);
                cmd.Parameters.AddWithValue("@cb", codigobarras);
                cmd.Parameters.AddWithValue("@lote", ONulo(lote));
                cmd.Parameters.AddWithValue("@cad", ONulo(caducidad));
                cmd.Parameters.AddWithValue("@cant", cantidad);
                cmd.Parameters.AddWithValue("@fecha", datos.Fecha ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@folio", ONulo(datos.Folio));
                cmd.Parameters.AddWithValue("@area", ONulo(datos.AreaServicio));
                cmd.Parameters.AddWithValue("@resp", ONulo(datos.Responsable));
                cmd.Parameters.AddWithValue("@obs", ONulo(datos.Observaciones));
                cmd.Parameters.AddWithValue("@origen", ONulo(datos.Origen));
                cmd.Parameters.AddWithValue("@origenId", ONulo(datos.OrigenId));
                cmd.Parameters.AddWithValue("@usuario", ONulo(datos.Usuario));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Resuelve una clave del cuadro basico a un codigo de barras concreto.
        /// Si la clave tiene varias presentaciones se toma la que tenga mas
        /// existencia; sirve para dar entrada cuando el usuario tecleo la clave.
        /// </summary>
        public static string ResolverCodigoBarras(SqlConnection cn, SqlTransaction tx, string codigo)
        {
            var sql = @"
                SELECT TOP 1 m.codigobarras
                FROM dbo.farm_medicamento m
                LEFT JOIN dbo.vw_farm_inventario i ON i.codigobarras = m.codigobarras
                WHERE m.clave = @codigo OR m.codigobarras = @codigo
                ORDER BY CASE WHEN m.codigobarras = @codigo THEN 0 ELSE 1 END,
                         ISNULL(i.existencia, 0) DESC";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@codigo", codigo ?? "");
                var v = cmd.ExecuteScalar();
                return v == null || v == DBNull.Value ? null : Convert.ToString(v);
            }
        }

        /// <summary>Folio consecutivo por prefijo (RX-000123, COL-000045...).</summary>
        public static string SiguienteFolio(SqlConnection cn, SqlTransaction tx, string tabla, string columna, string prefijo)
        {
            var sql = "SELECT COUNT(*) FROM dbo." + tabla;
            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                var n = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                return prefijo + "-" + n.ToString("000000");
            }
        }
    }
}
