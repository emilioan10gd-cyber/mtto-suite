using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    /// <summary>
    /// Mapeo parcial de la tabla existente dbo.cateter_movimiento: solo las
    /// columnas de cancelación agregadas por
    /// Cateter_060_TERAPIA_INFUSION_Y_CANCELACION.sql. A propósito NO se
    /// declaran las demás columnas de la tabla (folio, tipo_id, lote_id,
    /// cantidad, etc.): esas ya las expone vw_cateter_movimientos y esta
    /// clase solo se usa para leer/actualizar el estado de cancelación sin
    /// tocar el resto de la fila.
    /// </summary>
    [Table("cateter_movimiento")]
    public class cateter_movimiento_estado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int movimiento_id { get; set; }

        [Required]
        [StringLength(20)]
        public string estado { get; set; }

        [StringLength(500)]
        public string razon_cancelacion { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? cancelado_en { get; set; }

        [StringLength(120)]
        public string cancelado_por { get; set; }

        public bool es_reversion { get; set; }

        public int? reversion_de_id { get; set; }
    }
}
