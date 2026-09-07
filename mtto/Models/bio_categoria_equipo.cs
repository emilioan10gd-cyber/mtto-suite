using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class bio_categoria_equipo
    {
        [Key]
        public int categoria_id { get; set; }

        [Required]
        [StringLength(120)]
        public string nombre { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}
