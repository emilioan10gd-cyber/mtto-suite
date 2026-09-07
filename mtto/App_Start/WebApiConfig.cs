using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace mtto
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // ---------------------------------------------------------------
            // CORS: mttoweb es un sitio estático aparte (otro puerto en local,
            // y previsiblemente otro subdominio en el hosting), así que el
            // navegador lo trata como origen distinto al de esta API.
            //
            // Agrega aquí el dominio real en cuanto se publique en
            // freeasphosting.net; hasta entonces solo se permite el servidor
            // estático local de mttoweb (ver .claude/launch.json, puerto 5500).
            // ---------------------------------------------------------------
            var origenesPermitidos = string.Join(",", new[]
            {
                "http://localhost:5500",
                // 5501: puerto real con el que arranca hoy mttoweb (.claude/launch.json).
                // Se dejan los dos porque 5500 es el default de Live Server y se
                // sigue usando al abrir el sitio desde VS Code.
                "http://localhost:5501"
            });
            config.EnableCors(new EnableCorsAttribute(origenesPermitidos, headers: "*", methods: "*"));

            // Rutas por atributo ([RoutePrefix]/[Route] en los controllers).
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // ---------------------------------------------------------------
            // JSON
            // ---------------------------------------------------------------
            var json = config.Formatters.JsonFormatter;

            // camelCase: es lo que espera JavaScript.
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Red de seguridad. La API devuelve DTOs planos, pero si algún día se
            // regresa una entidad EF por descuido, esto evita el ciclo infinito
            // (categoria -> articulos -> categoria) que tumbaba los endpoints.
            json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Fechas ISO 8601 en UTC, sin el formato "/Date(...)/" de .NET.
            json.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            json.SerializerSettings.NullValueHandling = NullValueHandling.Include;

            // Que el navegador reciba JSON y no XML al pedir desde la barra de
            // direcciones (donde Accept privilegia application/xml).
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            json.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }
    }
}
