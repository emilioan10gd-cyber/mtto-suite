using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    /// <summary>
    /// Cuenta de acceso. El "area" (mtto | cateter | farmacia) es lo que separa
    /// los módulos: lo usa RequiereAreaAttribute en la API y auth.js para
    /// esconder del sidebar lo que no le toca a esa cuenta.
    /// </summary>
    public class app_usuario
    {
        [Key]
        public int usuario_id { get; set; }

        [Required]
        [StringLength(40)]
        public string nombre_usuario { get; set; }

        [Required]
        [StringLength(120)]
        public string correo { get; set; }

        [Required]
        [StringLength(200)]
        public string password_hash { get; set; }

        [Required]
        [StringLength(20)]
        public string area { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ultimo_acceso { get; set; }
    }

    /// <summary>
    /// Token opaco (no JWT) guardado en servidor: así se puede revocar de
    /// verdad borrando el renglón, cosa que un JWT autocontenido no permite.
    /// </summary>
    public class app_sesion
    {
        [Key]
        public int sesion_id { get; set; }

        [Required]
        [StringLength(64)]
        public string token { get; set; }

        public int usuario_id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime expira_en { get; set; }

        public virtual app_usuario app_usuario { get; set; }
    }
}
