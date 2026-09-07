using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class alm_articulo
    {
        [Key]
        public int articulo_id { get; set; }

        [Required]
        [StringLength(20)]
        public string clave_ssa { get; set; }

        [Required]
        [StringLength(500)]
        public string nombre { get; set; }

        [Required]
        [StringLength(80)]
        public string unidad_medida { get; set; }

        public bool es_cpm { get; set; }

        [Column(TypeName = "decimal")]
        public decimal? precio_unitario { get; set; }

        [Column(TypeName = "decimal")]
        public decimal stock_minimo { get; set; }

        [Column(TypeName = "decimal")]
        public decimal? stock_maximo { get; set; }

        [StringLength(40)]
        public string programa { get; set; }

        public bool activo { get; set; }

        public string imagen_base64 { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }
    }
}
