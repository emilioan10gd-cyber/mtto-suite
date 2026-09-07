namespace mtto.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("vw_farm_inventario")]
    public partial class vw_farm_inventario
    {
        public int id { get; set; }
        public string codigobarras { get; set; }
        public string clave { get; set; }
        public string nombre { get; set; }
        public string grupo_terapeutico { get; set; }
        public string unidad_medida { get; set; }
        public int? insumo_cpm { get; set; }
        public string clasificacion_aware { get; set; }
        public bool activo { get; set; }
        public decimal total_entradas { get; set; }
        public decimal total_salidas { get; set; }
        public decimal existencia { get; set; }
        public decimal cpm_mensual { get; set; }
        public string nivel { get; set; }
        public DateTime creado_en { get; set; }
        public DateTime actualizado_en { get; set; }
    }
}
