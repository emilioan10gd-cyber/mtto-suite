namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_mtto_bajo_stock
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int articulo_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string codigo { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(250)]
        public string nombre { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(80)]
        public string categoria { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(80)]
        public string almacen { get; set; }

        [Key]
        [Column(Order = 5)]
        public decimal stock_actual { get; set; }

        [Key]
        [Column(Order = 6)]
        public decimal stock_minimo { get; set; }

        public decimal? faltante { get; set; }
    }
}
