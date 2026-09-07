using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class cpm_clave
    {
        [Key]
        [StringLength(20)]
        public string clave { get; set; }

        public string descripcion { get; set; }

        [StringLength(10)]
        public string grupo { get; set; }

        [StringLength(80)]
        public string grupo_terapeutico { get; set; }

        public bool? primer_nivel { get; set; }

        public bool? segundo_nivel { get; set; }

        public bool? tercer_nivel { get; set; }

        public decimal? cantidad_mensual { get; set; }

        [StringLength(120)]
        public string fuente { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}

