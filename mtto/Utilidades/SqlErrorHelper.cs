using System;
using System.Data.SqlClient;

namespace mtto.Utilidades
{
    public static class SqlErrorHelper
    {
        /// <summary>
        /// EF6 envuelve el SqlException real dentro de un UpdateException
        /// intermedio (DbUpdateException.InnerException.InnerException), así
        /// que hay que recorrer la cadena en vez de mirar solo un nivel.
        /// </summary>
        public static bool EsViolacionDeUnicidad(Exception ex)
        {
            for (var actual = ex; actual != null; actual = actual.InnerException)
            {
                if (actual is SqlException sql && (sql.Number == 2627 || sql.Number == 2601))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Errores que los procedimientos lanzan a propósito con THROW/RAISERROR
        /// (rango 50000-51999): son reglas de negocio, no fallas del sistema, así
        /// que el mensaje sí se le puede mostrar tal cual al usuario.
        /// Devuelve null si la excepción no es de esas.
        /// </summary>
        public static SqlException ErrorDeValidacion(Exception ex)
        {
            for (var actual = ex; actual != null; actual = actual.InnerException)
            {
                if (actual is SqlException sql && sql.Number >= 50000 && sql.Number < 52000)
                    return sql;
            }
            return null;
        }
    }
}
