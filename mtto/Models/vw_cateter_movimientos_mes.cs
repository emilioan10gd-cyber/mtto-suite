using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_cateter_movimientos_mes
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int? anio { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int? mes { get; set; }

        public int? total_movimientos { get; set; }

        public decimal unidades_entrada { get; set; }

        public decimal unidades_salida { get; set; }

        public decimal unidades_merma { get; set; }

        public decimal unidades_traslado { get; set; }

        public int? entradas { get; set; }

        public int? salidas { get; set; }

        public int? mermas { get; set; }
    }
}

