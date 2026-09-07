using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    /// <summary>
    /// Registro diario de eventos de Terapia de Infusión Intravascular:
    /// total de eventos, total de catéteres colocados, cuántas personas
    /// instalaron (solo el número, no quiénes) y total de intervenciones.
    /// </summary>
    public class cateter_terapia_evento
    {
        [Key]
        public int evento_id { get; set; }

        [Column(TypeName = "date")]
        public DateTime fecha { get; set; }

        public int total_eventos { get; set; }

        public int total_cateteres_colocados { get; set; }

        public int personas_instalaron { get; set; }

        public int total_intervenciones { get; set; }

        [StringLength(120)]
        public string creado_por { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? actualizado_en { get; set; }
    }
}
