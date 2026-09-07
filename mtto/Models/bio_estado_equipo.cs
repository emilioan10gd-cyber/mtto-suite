using System.ComponentModel.DataAnnotations;

namespace mtto.Models
{
    public class bio_estado_equipo
    {
        [Key]
        public int estado_id { get; set; }

        [Required]
        [StringLength(40)]
        public string nombre { get; set; }

        public bool es_baja { get; set; }
    }
}
