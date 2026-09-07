using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class bio_equipo
    {
        [Key]
        public int equipo_id { get; set; }

        [Required]
        [StringLength(250)]
        public string nombre { get; set; }

        [StringLength(500)]
        public string descripcion { get; set; }

        [StringLength(120)]
        public string marca { get; set; }

        [StringLength(120)]
        public string modelo { get; set; }

        [StringLength(120)]
        public string numero_serie { get; set; }

        public int area_id { get; set; }

        public int? categoria_id { get; set; }

        public int estado_id { get; set; }

        [StringLength(150)]
        public string ubicacion { get; set; }

        [StringLength(500)]
        public string comentarios { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fecha_adquisicion { get; set; }

        [StringLength(500)]
        public string motivo_baja { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual bio_area bio_area { get; set; }
        public virtual bio_categoria_equipo bio_categoria_equipo { get; set; }
        public virtual bio_estado_equipo bio_estado_equipo { get; set; }

        public virtual ICollection<bio_mantenimiento> bio_mantenimiento { get; set; }
        public virtual ICollection<bio_manual> bio_manual { get; set; }
        public virtual ICollection<bio_falla> bio_falla { get; set; }

        public bio_equipo()
        {
            bio_mantenimiento = new HashSet<bio_mantenimiento>();
            bio_manual = new HashSet<bio_manual>();
            bio_falla = new HashSet<bio_falla>();
        }
    }
}
