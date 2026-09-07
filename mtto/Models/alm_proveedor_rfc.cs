using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class alm_proveedor_rfc
    {
        [Key]
        public int proveedor_rfc_id { get; set; }

        [Required]
        public int proveedor_id { get; set; }

        [Required]
        [StringLength(20)]
        public string rfc { get; set; }

        [Required]
        public bool es_activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        // Relación con proveedor
        [ForeignKey("proveedor_id")]
        public virtual alm_proveedor alm_proveedor { get; set; }
    }
}
