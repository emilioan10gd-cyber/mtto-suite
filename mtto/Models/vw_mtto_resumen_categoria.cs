namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_mtto_resumen_categoria
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int categoria_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string codigo { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(80)]
        public string categoria { get; set; }

        public int? total_articulos { get; set; }

        [Key]
        [Column(Order = 3)]
        public decimal total_existencias { get; set; }

        [Key]
        [Column(Order = 4)]
        public decimal valor_total { get; set; }

        public int? bajo_minimo { get; set; }
    }
}
