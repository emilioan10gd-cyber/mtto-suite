using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_cateter_dashboard_kpi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int? total_claves { get; set; }

        public decimal? total_piezas { get; set; }

        public decimal? piezas_vigentes { get; set; }

        public decimal? piezas_vencidas { get; set; }

        public decimal? piezas_en_almacen { get; set; }

        public decimal? piezas_en_stock { get; set; }

        public int? claves_bajo_minimo { get; set; }

        public decimal? piezas_por_vencer_30 { get; set; }

        public decimal? piezas_por_vencer_90 { get; set; }

        public int? movimientos_del_mes { get; set; }
    }
}

