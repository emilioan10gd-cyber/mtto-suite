namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class mtto_articulo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mtto_articulo()
        {
            mtto_movimiento = new HashSet<mtto_movimiento>();
        }

        [Key]
        public int articulo_id { get; set; }

        [Required]
        [StringLength(20)]
        public string codigo { get; set; }

        [Required]
        [StringLength(250)]
        public string nombre { get; set; }

        [StringLength(500)]
        public string descripcion { get; set; }

        public int categoria_id { get; set; }

        public int unidad_id { get; set; }

        public int almacen_id { get; set; }

        public int? proveedor_id { get; set; }

        public int estado_id { get; set; }

        public decimal stock_actual { get; set; }

        public decimal stock_minimo { get; set; }

        public decimal? stock_maximo { get; set; }

        public decimal? costo_unitario { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? valor_total { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        public virtual mtto_almacen mtto_almacen { get; set; }

        public virtual mtto_categoria mtto_categoria { get; set; }

        public virtual mtto_estado_articulo mtto_estado_articulo { get; set; }

        public virtual mtto_proveedor mtto_proveedor { get; set; }

        public virtual mtto_unidad_medida mtto_unidad_medida { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mtto_movimiento> mtto_movimiento { get; set; }
    }
}
