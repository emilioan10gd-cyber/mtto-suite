namespace mtto.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("vw_farm_movimientos")]
    public partial class vw_farm_movimientos
    {
        public string tipo { get; set; }
        public int movimiento_id { get; set; }
        public int? detalle_id { get; set; }
        public string codigobarras { get; set; }
        public string clave { get; set; }
        public string nombre { get; set; }
        public decimal cantidad_mov { get; set; }
        public string lote { get; set; }
        public DateTime? caducidad { get; set; }
        public DateTime fecha { get; set; }
        public string observaciones { get; set; }
        public int? medicamento_id { get; set; }
    }
}
