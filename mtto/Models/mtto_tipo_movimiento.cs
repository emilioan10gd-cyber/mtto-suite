namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class mtto_tipo_movimiento
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mtto_tipo_movimiento()
        {
            mtto_movimiento = new HashSet<mtto_movimiento>();
        }

        [Key]
        public int tipo_id { get; set; }

        [Required]
        [StringLength(40)]
        public string nombre { get; set; }

        public short signo { get; set; }

        public bool requiere_destino { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mtto_movimiento> mtto_movimiento { get; set; }
    }
}
