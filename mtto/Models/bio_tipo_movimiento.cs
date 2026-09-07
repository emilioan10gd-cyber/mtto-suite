using System.ComponentModel.DataAnnotations;

namespace mtto.Models
{
    public class bio_tipo_movimiento
    {
        [Key]
        public int tipo_id { get; set; }

        [Required]
        [StringLength(40)]
        public string nombre { get; set; }

        public short signo { get; set; }
    }
}
