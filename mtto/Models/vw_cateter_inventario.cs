using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_cateter_inventario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int articulo_id { get; set; }

        [StringLength(20)]
        public string clave { get; set; }

        [StringLength(250)]
        public string nombre { get; set; }

        public string descripcion { get; set; }

        [StringLength(80)]
        public string categoria { get; set; }

        [StringLength(40)]
        public string unidad { get; set; }

        [StringLength(120)]
        public string marca { get; set; }

        [StringLength(80)]
        public string referencia { get; set; }

        [StringLength(40)]
        public string calibre { get; set; }

        public short? lumenes { get; set; }

        [StringLength(120)]
        public string presentacion { get; set; }

        public decimal en_almacen { get; set; }

        public decimal en_stock { get; set; }

        public decimal total { get; set; }

        public decimal total_vigente { get; set; }

        public decimal total_vencido { get; set; }

        public decimal stock_minimo { get; set; }

        public decimal? stock_maximo { get; set; }

        public decimal? cpm_mensual { get; set; }

        public int? lotes_con_existencia { get; set; }

        [Column(TypeName = "date")]
        public DateTime? caducidad_proxima { get; set; }

        [StringLength(20)]
        public string nivel { get; set; }

        [StringLength(20)]
        public string alerta_caducidad { get; set; }

        public bool activo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }

        public int categoria_id { get; set; }

        public int unidad_id { get; set; }
    }
}

