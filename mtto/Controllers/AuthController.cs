using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    /// <summary>
    /// Login por token opaco. Es el único controller sin [RequiereArea]: si lo
    /// tuviera, no habría manera de autenticarse la primera vez.
    /// </summary>
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpPost, Route("login")]
        [ResponseType(typeof(LoginResultadoDto))]
        public IHttpActionResult Login(LoginInputDto entrada)
        {
            if (entrada == null ||
                string.IsNullOrWhiteSpace(entrada.UsuarioOCorreo) ||
                string.IsNullOrWhiteSpace(entrada.Password))
            {
                return BadRequest("Usuario/correo y contraseña son obligatorios.");
            }

            var valor = entrada.UsuarioOCorreo.Trim();
            var usuario = db.app_usuario
                .FirstOrDefault(u => (u.nombre_usuario == valor || u.correo == valor) && u.activo);

            // Mismo mensaje para usuario inexistente y contraseña mala: no hay
            // que decirle a quien prueba si el usuario existe o no.
            if (usuario == null || !PasswordHasher.Verificar(entrada.Password, usuario.password_hash))
            {
                return Content(HttpStatusCode.Unauthorized, new { error = "Usuario o contraseña incorrectos." });
            }

            var ahora = DateTime.Now;
            var expiraEn = ahora.Add(AuthHelper.DuracionSesion);
            var sesion = new app_sesion
            {
                token = AuthHelper.GenerarToken(),
                usuario_id = usuario.usuario_id,
                creado_en = ahora,
                expira_en = expiraEn
            };

            db.app_sesion.Add(sesion);
            usuario.ultimo_acceso = ahora;
            db.SaveChanges();

            return Ok(new LoginResultadoDto
            {
                Token = sesion.token,
                NombreUsuario = usuario.nombre_usuario,
                Area = usuario.area,
                ExpiraEn = expiraEn
            });
        }

        [HttpPost, Route("logout")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Logout()
        {
            var token = AuthHelper.ExtraerToken(Request);
            if (token != null)
            {
                var sesion = db.app_sesion.FirstOrDefault(s => s.token == token);
                if (sesion != null)
                {
                    db.app_sesion.Remove(sesion);
                    db.SaveChanges();
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// GET api/auth/quien-soy — lo llama auth.js al cargar cualquier página
        /// protegida: confirma que el token sigue vivo y de paso lo renueva.
        /// </summary>
        [HttpGet, Route("quien-soy")]
        [ResponseType(typeof(SesionActualDto))]
        public IHttpActionResult QuienSoy()
        {
            var usuario = AuthHelper.ValidarYRenovar(Request);
            if (usuario == null)
            {
                return Content(HttpStatusCode.Unauthorized, new { error = "Sesión no válida o expirada." });
            }

            var expiraEn = db.app_sesion
                .Where(s => s.usuario_id == usuario.usuario_id)
                .OrderByDescending(s => s.expira_en)
                .Select(s => s.expira_en)
                .First();

            return Ok(new SesionActualDto
            {
                NombreUsuario = usuario.nombre_usuario,
                Area = usuario.area,
                ExpiraEn = expiraEn
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
