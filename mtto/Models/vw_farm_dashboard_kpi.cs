namespace mtto.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("vw_farm_dashboard_kpi")]
    public partial class vw_farm_dashboard_kpi
    {
        public int total_medicamentos { get; set; }
        public decimal total_existencia { get; set; }
        public int medicamentos_bajo_stock { get; set; }
    }
}
