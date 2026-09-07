using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class bio_area
    {
        [Key]
        public int area_id { get; set; }

        [Required]
        [StringLength(80)]
        public string nombre { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}
