using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class bio_manual
    {
        [Key]
        public int manual_id { get; set; }

        public int equipo_id { get; set; }

        [Required]
        [StringLength(255)]
        public string nombre_archivo { get; set; }

        [Required]
        [StringLength(500)]
        public string ruta_archivo { get; set; }

        [StringLength(20)]
        public string tipo_archivo { get; set; }

        public long? tamano_bytes { get; set; }

        public string texto_extraido { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime subido_en { get; set; }

        public virtual bio_equipo bio_equipo { get; set; }
    }
}
