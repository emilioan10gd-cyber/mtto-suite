using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class bio_falla
    {
        [Key]
        public int falla_id { get; set; }

        public int equipo_id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime fecha { get; set; }

        [Required]
        [StringLength(1000)]
        public string sintoma { get; set; }

        [StringLength(1000)]
        public string causa { get; set; }

        [StringLength(1000)]
        public string solucion { get; set; }

        [StringLength(120)]
        public string tecnico { get; set; }

        [Required]
        [StringLength(20)]
        public string estado { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual bio_equipo bio_equipo { get; set; }
    }
}
