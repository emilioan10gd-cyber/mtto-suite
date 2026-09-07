using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_bio_inventario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int equipo_id { get; set; }

        [StringLength(250)]
        public string nombre { get; set; }

        public string descripcion { get; set; }

        [StringLength(120)]
        public string marca { get; set; }

        [StringLength(120)]
        public string modelo { get; set; }

        [StringLength(120)]
        public string numero_serie { get; set; }

        [StringLength(80)]
        public string area { get; set; }

        public int area_id { get; set; }

        [StringLength(120)]
        public string categoria { get; set; }

        public int? categoria_id { get; set; }

        [StringLength(40)]
        public string estado { get; set; }

        public bool es_baja { get; set; }

        public int estado_id { get; set; }

        [StringLength(150)]
        public string ubicacion { get; set; }

        public string comentarios { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fecha_adquisicion { get; set; }

        public string motivo_baja { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public int total_mantenimientos { get; set; }

        public int mantenimientos_vencidos { get; set; }

        public int total_fallas { get; set; }

        public int total_manuales { get; set; }
    }
}
