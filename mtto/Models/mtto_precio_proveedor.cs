namespace mtto.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class mtto_precio_proveedor
    {
        [Key]
        public int precio_id { get; set; }

        public int articulo_id { get; set; }

        public int proveedor_id { get; set; }

        public decimal cantidad_minima { get; set; }

        public decimal? cantidad_maxima { get; set; }

        public decimal precio_unitario { get; set; }

        [StringLength(255)]
        public string notas { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual mtto_articulo mtto_articulo { get; set; }

        public virtual mtto_proveedor mtto_proveedor { get; set; }
    }
}
