using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using mtto.Models;

namespace mtto.Utilidades
{
    /// <summary>
    /// Separa los módulos del lado del servidor. Esconder enlaces en el sidebar
    /// es cosmético: sin esto, la cuenta de mantenimiento podría pegarle a mano
    /// a /api/farmacia. Se pone a nivel de controller.
    /// </summary>
    public class RequiereAreaAttribute : AuthorizationFilterAttribute
    {
        private readonly string area;

        public RequiereAreaAttribute(string area)
        {
            this.area = area;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var usuario = AuthHelper.ValidarYRenovar(actionContext.Request);

            if (usuario == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, "Sesión no válida o expirada. Inicia sesión de nuevo.");
                return;
            }

            if (!string.Equals(usuario.area, area, StringComparison.OrdinalIgnoreCase))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Forbidden, "Esta cuenta no tiene acceso a este módulo.");
                return;
            }

            // Los controllers lo leen de aquí cuando necesitan saber quién hizo
            // el movimiento, sin volver a consultar la sesión.
            actionContext.Request.Properties["UsuarioActual"] = usuario;
        }
    }
}
