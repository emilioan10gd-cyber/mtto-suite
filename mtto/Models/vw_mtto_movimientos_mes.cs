namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_mtto_movimientos_mes
    {
        public int? anio { get; set; }

        public int? mes { get; set; }

        public int? entradas { get; set; }

        public int? salidas { get; set; }

        public int? total_movimientos { get; set; }

        [Key]
        [Column(Order = 0)]
        public decimal unidades_entrada { get; set; }

        [Key]
        [Column(Order = 1)]
        public decimal unidades_salida { get; set; }
    }
}
