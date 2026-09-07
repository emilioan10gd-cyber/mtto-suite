namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class mtto_proveedor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mtto_proveedor()
        {
            mtto_articulo = new HashSet<mtto_articulo>();
        }

        [Key]
        public int proveedor_id { get; set; }

        [Required]
        [StringLength(20)]
        public string codigo { get; set; }

        [Required]
        [StringLength(150)]
        public string nombre { get; set; }

        [StringLength(120)]
        public string contacto { get; set; }

        [StringLength(40)]
        public string telefono { get; set; }

        [StringLength(120)]
        public string correo { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mtto_articulo> mtto_articulo { get; set; }
    }
}
