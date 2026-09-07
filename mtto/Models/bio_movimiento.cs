using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class bio_movimiento
    {
        [Key]
        public int movimiento_id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime fecha { get; set; }

        public int tipo_id { get; set; }

        public int insumo_id { get; set; }

        public int lote_id { get; set; }

        public decimal cantidad { get; set; }

        [StringLength(120)]
        public string responsable { get; set; }

        [StringLength(500)]
        public string observaciones { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual bio_tipo_movimiento bio_tipo_movimiento { get; set; }
        public virtual bio_insumo bio_insumo { get; set; }
        public virtual bio_lote bio_lote { get; set; }
    }
}
