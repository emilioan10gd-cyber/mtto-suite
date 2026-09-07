using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    public class alm_movimiento
    {
        [Key]
        public int movimiento_id { get; set; }

        [Required]
        [StringLength(20)]
        public string tipo { get; set; }

        public int lote_id { get; set; }

        public int articulo_id { get; set; }

        [Column(TypeName = "decimal")]
        public decimal cantidad { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime fecha { get; set; }

        public int? proveedor_id { get; set; }

        [StringLength(60)]
        public string vale { get; set; }

        [StringLength(40)]
        public string programa { get; set; }

        [StringLength(120)]
        public string orden_suministro { get; set; }

        [StringLength(80)]
        public string area_destino { get; set; }

        [StringLength(160)]
        public string entregado_a { get; set; }   // persona que recibe el material

        [StringLength(500)]
        public string notas { get; set; }

        public int usuario_id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [StringLength(10)]
        public string ajuste_subtipo { get; set; }  // "Agregar"|"Reducir"|"Exacto" — solo para tipo Ajuste

        public string imagen_base64 { get; set; }

        // Solo aplican a Entrada — datos institucionales del formato SSA de
        // inventario semanal (CLUES/fuente de financiamiento/partida).
        [StringLength(20)]
        public string clues_origen { get; set; }

        [StringLength(120)]
        public string fuente_financiamiento { get; set; }

        [StringLength(40)]
        public string partida_presupuestal { get; set; }

        public virtual alm_lote alm_lote { get; set; }
        public virtual alm_articulo alm_articulo { get; set; }
        public virtual alm_proveedor alm_proveedor { get; set; }
    }
}
