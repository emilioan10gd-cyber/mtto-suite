using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class alm_lote
    {
        [Key]
        public int lote_id { get; set; }

        public int articulo_id { get; set; }

        [Required]
        [StringLength(60)]
        public string lote { get; set; }

        [Column(TypeName = "date")]
        public DateTime? caducidad { get; set; }

        [Column(TypeName = "decimal")]
        public decimal cantidad { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual alm_articulo alm_articulo { get; set; }
    }
}
