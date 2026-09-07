namespace mtto.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("vw_farm_lotes")]
    public partial class vw_farm_lotes
    {
        public int lote_id { get; set; }
        public int id_entrada { get; set; }
        public int? medicamento_id { get; set; }
        public string codigobarras { get; set; }
        public string clave { get; set; }
        public string descripcion_articulo_completa { get; set; }
        public string lote { get; set; }
        public DateTime? fecha_caducidad { get; set; }
        public decimal cantidad { get; set; }
        public string ubicacion { get; set; }
        public DateTime fecha_entrada { get; set; }
        public string estado_caducidad { get; set; }
        public DateTime creado_en { get; set; }
    }
}
