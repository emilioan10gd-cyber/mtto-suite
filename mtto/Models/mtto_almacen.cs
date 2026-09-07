namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class mtto_almacen
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mtto_almacen()
        {
            mtto_articulo = new HashSet<mtto_articulo>();
            mtto_movimiento = new HashSet<mtto_movimiento>();
            mtto_movimiento1 = new HashSet<mtto_movimiento>();
        }

        [Key]
        public int almacen_id { get; set; }

        [Required]
        [StringLength(20)]
        public string codigo { get; set; }

        [Required]
        [StringLength(80)]
        public string nombre { get; set; }

        [StringLength(150)]
        public string ubicacion { get; set; }

        [StringLength(120)]
        public string responsable { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mtto_articulo> mtto_articulo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mtto_movimiento> mtto_movimiento { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mtto_movimiento> mtto_movimiento1 { get; set; }
    }
}
