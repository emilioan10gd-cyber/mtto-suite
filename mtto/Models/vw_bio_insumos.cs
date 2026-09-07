using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_bio_insumos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int insumo_id { get; set; }

        [StringLength(20)]
        public string clave { get; set; }

        [StringLength(250)]
        public string nombre { get; set; }

        public string descripcion { get; set; }

        [StringLength(120)]
        public string categoria { get; set; }

        public int? categoria_id { get; set; }

        [StringLength(40)]
        public string unidad { get; set; }

        public decimal stock_minimo { get; set; }

        public decimal? stock_maximo { get; set; }

        public decimal existencia_total { get; set; }

        [Column(TypeName = "date")]
        public DateTime? caducidad_proxima { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}
