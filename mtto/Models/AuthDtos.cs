using System;

namespace mtto.Models
{
    public class LoginInputDto
    {
        public string UsuarioOCorreo { get; set; }
        public string Password { get; set; }
    }

    public class LoginResultadoDto
    {
        public string Token { get; set; }
        public string NombreUsuario { get; set; }
        public string Area { get; set; }
        public DateTime ExpiraEn { get; set; }
    }

    public class SesionActualDto
    {
        public string NombreUsuario { get; set; }
        public string Area { get; set; }
        public DateTime ExpiraEn { get; set; }
    }
}
