using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    // Fila única con los datos institucionales fijos del almacén (entidad
    // federativa, CLUES destino) que necesita el formato de exportación
    // SSA/CLUES — se capturan una vez en vez de repetirlas en cada entrada.
    public class alm_config
    {
        [Key]
        public int config_id { get; set; }

        [StringLength(60)]
        public string entidad_federativa { get; set; }

        [StringLength(20)]
        public string clues_destino { get; set; }

        // Contra la evidencia real (1711 filas del formato de referencia):
        // el CLUES de origen es igual de fijo que el de destino, no varía
        // por remisión, así que vive aquí y no en cada Entrada.
        [StringLength(20)]
        public string clues_origen { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime actualizado_en { get; set; }
    }
}
