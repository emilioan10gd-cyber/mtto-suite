using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class cateter_ubicacion
    {
        [Key]
        public int ubicacion_id { get; set; }

        [Required]
        [StringLength(20)]
        public string codigo { get; set; }

        [Required]
        [StringLength(80)]
        public string nombre { get; set; }

        [StringLength(255)]
        public string descripcion { get; set; }

        public bool es_almacen { get; set; }

        public short orden { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}

