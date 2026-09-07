using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class vw_cateter_movimientos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int movimiento_id { get; set; }

        [StringLength(20)]
        public string folio { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime fecha { get; set; }

        [StringLength(40)]
        public string tipo { get; set; }

        [StringLength(20)]
        public string clasificacion { get; set; }

        [StringLength(20)]
        public string clave { get; set; }

        [StringLength(250)]
        public string articulo { get; set; }

        [StringLength(60)]
        public string lote { get; set; }

        [Column(TypeName = "date")]
        public DateTime? caducidad { get; set; }

        public decimal cantidad { get; set; }

        [StringLength(80)]
        public string ubicacion_origen { get; set; }

        [StringLength(80)]
        public string ubicacion_destino { get; set; }

        [StringLength(120)]
        public string referencia_uso { get; set; }

        [StringLength(120)]
        public string responsable { get; set; }

        [StringLength(500)]
        public string observaciones { get; set; }

        public decimal? saldo_origen_previo { get; set; }

        public decimal? saldo_origen_resultante { get; set; }

        public decimal? saldo_destino_previo { get; set; }

        public decimal? saldo_destino_resultante { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [StringLength(120)]
        public string creado_por { get; set; }

        public int lote_id { get; set; }

        public int tipo_id { get; set; }

        public int articulo_id { get; set; }
    }
}

