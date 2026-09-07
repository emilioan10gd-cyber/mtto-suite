using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    /// <summary>
    /// Un registro de la pestaña "Registro de Sitios y Catéteres" de Terapia
    /// de Infusión Intravascular: cuántos catéteres se colocaron por sitio
    /// anatómico y por calibre (no solo sí/no: puede haber varios del mismo).
    /// No lleva paciente asociado, es un conteo/tally de la clínica, no un
    /// expediente clínico.
    /// </summary>
    public class cateter_terapia_registro
    {
        [Key]
        public int registro_id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime fecha { get; set; }

        public int msd { get; set; }
        public int msi { get; set; }
        public int mii { get; set; }

        public int calibre_14 { get; set; }
        public int calibre_16 { get; set; }
        public int calibre_17 { get; set; }
        public int calibre_18 { get; set; }
        public int calibre_19 { get; set; }
        public int calibre_20 { get; set; }
        public int calibre_22 { get; set; }
        public int calibre_24 { get; set; }

        [StringLength(120)]
        public string creado_por { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }
    }
}
