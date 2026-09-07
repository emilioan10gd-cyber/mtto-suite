using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    [Table("cateter_clave_descripcion")]
    public class cateter_clave_descripcion
    {
        [Key]
        [Column("clave")]
        [StringLength(20)]
        public string Clave { get; set; }

        [Column("descripcion_propia")]
        public string DescripcionPropia { get; set; }

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; }
    }
}

