namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class mtto_estado_articulo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mtto_estado_articulo()
        {
            mtto_articulo = new HashSet<mtto_articulo>();
        }

        [Key]
        public int estado_id { get; set; }

        [Required]
        [StringLength(40)]
        public string nombre { get; set; }

        public bool es_activo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mtto_articulo> mtto_articulo { get; set; }
    }
}
