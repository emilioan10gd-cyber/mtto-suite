namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_mtto_distribucion_estado
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int estado_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(40)]
        public string estado { get; set; }

        public int? total_articulos { get; set; }

        [Key]
        [Column(Order = 2)]
        public decimal valor_total { get; set; }
    }
}
