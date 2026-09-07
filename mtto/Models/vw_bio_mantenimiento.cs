using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_bio_mantenimiento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int mantenimiento_id { get; set; }

        public int equipo_id { get; set; }

        [StringLength(250)]
        public string equipo { get; set; }

        [StringLength(80)]
        public string area { get; set; }

        [StringLength(20)]
        public string tipo { get; set; }

        [Column(TypeName = "date")]
        public DateTime fecha_programada { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fecha_realizada { get; set; }

        [StringLength(20)]
        public string estado { get; set; }

        [StringLength(120)]
        public string tecnico_responsable { get; set; }

        [StringLength(150)]
        public string proveedor { get; set; }

        public string descripcion { get; set; }

        public int? frecuencia_meses { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}
