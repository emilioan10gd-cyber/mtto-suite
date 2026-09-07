using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class cateter_articulo
    {
        [Key]
        public int articulo_id { get; set; }

        [Required]
        [StringLength(20)]
        public string clave { get; set; }

        [Required]
        [StringLength(250)]
        public string nombre { get; set; }

        public string descripcion { get; set; }

        public int categoria_id { get; set; }

        public int unidad_id { get; set; }

        [StringLength(120)]
        public string marca { get; set; }

        [StringLength(80)]
        public string referencia { get; set; }

        [StringLength(40)]
        public string calibre { get; set; }

        public short? lumenes { get; set; }

        [StringLength(120)]
        public string presentacion { get; set; }

        public decimal stock_minimo { get; set; }

        public decimal? stock_maximo { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual cateter_categoria cateter_categoria { get; set; }

        public virtual mtto_unidad_medida mtto_unidad_medida { get; set; }

        public virtual ICollection<cateter_lote> cateter_lote { get; set; }

        public cateter_articulo()
        {
            cateter_lote = new HashSet<cateter_lote>();
        }
    }
}

