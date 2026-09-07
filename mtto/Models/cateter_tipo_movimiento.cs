using System.ComponentModel.DataAnnotations;

namespace mtto.Models
{
    public class cateter_tipo_movimiento
    {
        [Key]
        public int tipo_id { get; set; }

        [Required]
        [StringLength(40)]
        public string nombre { get; set; }

        [Required]
        [StringLength(20)]
        public string clasificacion { get; set; }

        public bool requiere_origen { get; set; }

        public bool requiere_destino { get; set; }

        public short orden { get; set; }

        public bool activo { get; set; }
    }
}

