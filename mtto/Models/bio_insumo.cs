using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class bio_insumo
    {
        [Key]
        public int insumo_id { get; set; }

        [Required]
        [StringLength(20)]
        public string clave { get; set; }

        [Required]
        [StringLength(250)]
        public string nombre { get; set; }

        [StringLength(500)]
        public string descripcion { get; set; }

        public int? categoria_id { get; set; }

        [Required]
        [StringLength(40)]
        public string unidad { get; set; }

        public decimal stock_minimo { get; set; }

        public decimal? stock_maximo { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual bio_categoria_insumo bio_categoria_insumo { get; set; }
        public virtual ICollection<bio_lote> bio_lote { get; set; }

        public bio_insumo()
        {
            bio_lote = new HashSet<bio_lote>();
        }
    }
}
