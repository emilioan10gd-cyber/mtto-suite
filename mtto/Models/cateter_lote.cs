using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class cateter_lote
    {
        [Key]
        public int lote_id { get; set; }

        public int articulo_id { get; set; }

        [Required]
        [StringLength(60)]
        public string lote { get; set; }

        [Column(TypeName = "date")]
        public DateTime? caducidad { get; set; }

        [StringLength(255)]
        public string notas { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual cateter_articulo cateter_articulo { get; set; }
    }
}

