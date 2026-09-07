using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using mtto.Models;

namespace mtto.Utilidades
{
    /// <summary>
    /// Validación del token en cada petición. La sesión es "deslizante": cada
    /// llamada válida vuelve a empujar expira_en 90 días, que es lo que se pidió
    /// ("que no se ande cerrando"). Quien no usa el sistema en 90 días sí caduca.
    /// </summary>
    public static class AuthHelper
    {
        public static readonly TimeSpan DuracionSesion = TimeSpan.FromDays(90);

        public static app_usuario ValidarYRenovar(HttpRequestMessage request)
        {
            var token = ExtraerToken(request);
            if (token == null) return null;

            using (var db = new Model1())
            {
                var sesion = db.app_sesion
                    .Include(s => s.app_usuario)
                    .FirstOrDefault(s => s.token == token);

                if (sesion == null) return null;
                if (sesion.expira_en < DateTime.Now) return null;
                if (!sesion.app_usuario.activo) return null;

                var ahora = DateTime.Now;
                sesion.expira_en = ahora.Add(DuracionSesion);
                sesion.app_usuario.ultimo_acceso = ahora;
                db.SaveChanges();

                return sesion.app_usuario;
            }
        }

        /// El header Authorization es la vía normal; el ?token= de la query
        /// existe para las descargas (Excel), donde el navegador abre la URL
        /// directo y no hay manera de mandarle headers.
        public static string ExtraerToken(HttpRequestMessage request)
        {
            var autorizacion = request.Headers.Authorization;
            if (autorizacion != null && string.Equals(autorizacion.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return autorizacion.Parameter;
            }

            var query = request.RequestUri.Query;
            System.Diagnostics.Debug.WriteLine("Query string: " + (query ?? "(null)"));

            if (!string.IsNullOrEmpty(query) && query.Length > 1)
            {
                var pairs = query.Substring(1).Split('&');
                foreach (var pair in pairs)
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2 && parts[0] == "token")
                    {
                        var token = System.Net.WebUtility.UrlDecode(parts[1]);
                        System.Diagnostics.Debug.WriteLine("Token found in query: " + token.Substring(0, 10));
                        return token;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("No token found");
            return null;
        }

        public static string GenerarToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }
    }
}
