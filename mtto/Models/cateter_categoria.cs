using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class cateter_categoria
    {
        [Key]
        public int categoria_id { get; set; }

        [Required]
        [StringLength(20)]
        public string codigo { get; set; }

        [Required]
        [StringLength(80)]
        public string nombre { get; set; }

        [StringLength(255)]
        public string descripcion { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual ICollection<cateter_articulo> cateter_articulo { get; set; }

        public cateter_categoria()
        {
            cateter_articulo = new HashSet<cateter_articulo>();
        }
    }
}

