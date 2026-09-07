using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_cateter_resumen_categoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int categoria_id { get; set; }

        [StringLength(20)]
        public string codigo { get; set; }

        [StringLength(80)]
        public string categoria { get; set; }

        public int? total_articulos { get; set; }

        public decimal total_existencias { get; set; }

        public decimal total_vigente { get; set; }

        public decimal total_vencido { get; set; }

        public decimal en_almacen { get; set; }

        public decimal en_stock { get; set; }

        public int? bajo_minimo { get; set; }
    }
}

