using System;
using System.Data;
using System.Data.SqlClient;

namespace mtto.Utilidades
{
    /// <summary>
    /// Lectura defensiva de un DataReader para los endpoints de Farmacia.
    ///
    /// Se lee por NOMBRE de columna y con Convert.ToXxx a proposito: los
    /// origenes son vistas que cambian de forma, y con GetDecimal(0) cualquier
    /// reordenamiento o un int donde se esperaba decimal tumbaba el endpoint
    /// con un 500 sin pista de cual columna fallo.
    /// </summary>
    public static class LectorSql
    {
        public static string Txt(IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? null : Convert.ToString(v);
        }

        public static decimal Num(IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? 0m : Convert.ToDecimal(v);
        }

        public static int Ent(IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? 0 : Convert.ToInt32(v);
        }

        public static bool Si(IDataRecord r, string col)
        {
            var v = r[col];
            return v != DBNull.Value && Convert.ToBoolean(v);
        }

        public static DateTime? Fecha(IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v);
        }

        /// Convierte "" en NULL para que los filtros opcionales se apaguen
        /// solos en el WHERE (@x IS NULL OR columna = @x).
        public static object Opcional(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor.Trim();
        }

        public static object ONulo(object valor)
        {
            return valor ?? DBNull.Value;
        }

        public static void Paginar(ref int pagina, ref int tamano, int tamanoMaximo = 500)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1) tamano = 20;
            if (tamano > tamanoMaximo) tamano = tamanoMaximo;
        }

        public static int TotalPaginas(int total, int tamano)
        {
            return tamano > 0 ? (total + tamano - 1) / tamano : 0;
        }

        public static SqlCommand Comando(SqlConnection cn, string sql)
        {
            var cmd = new SqlCommand(sql, cn);
            cmd.CommandTimeout = 120;
            return cmd;
        }
    }
}
