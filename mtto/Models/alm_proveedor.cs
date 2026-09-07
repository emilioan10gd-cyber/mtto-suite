using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class alm_proveedor
    {
        [Key]
        public int proveedor_id { get; set; }

        [Required]
        [StringLength(200)]
        public string nombre { get; set; }

        [StringLength(20)]
        public string rfc { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}
