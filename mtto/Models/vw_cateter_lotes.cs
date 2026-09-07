using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_cateter_lotes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int lote_id { get; set; }

        public int articulo_id { get; set; }

        [StringLength(20)]
        public string clave { get; set; }

        [StringLength(250)]
        public string articulo { get; set; }

        [StringLength(80)]
        public string categoria { get; set; }

        [StringLength(60)]
        public string lote { get; set; }

        [Column(TypeName = "date")]
        public DateTime? caducidad { get; set; }

        public int? dias_para_vencer { get; set; }

        [StringLength(20)]
        public string estado_caducidad { get; set; }

        public decimal en_almacen { get; set; }

        public decimal en_stock { get; set; }

        public decimal total { get; set; }

        [StringLength(255)]
        public string notas { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}

