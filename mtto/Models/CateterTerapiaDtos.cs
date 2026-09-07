using System;
using System.Collections.Generic;

// DTOs de Terapia de Infusión Intravascular y de cancelación de movimientos
// de la Clínica de Catéter.
namespace mtto.Models
{
    /// <summary>
    /// Una fila tipo Excel: cantidad por cada sitio anatómico y cada calibre,
    /// no solo si se usó o no. Cero = no se usó ese sitio/calibre.
    /// </summary>
    public class TerapiaRegistroInputDto
    {
        public int Msd { get; set; }
        public int Msi { get; set; }
        public int Mii { get; set; }

        public int Calibre14 { get; set; }
        public int Calibre16 { get; set; }
        public int Calibre17 { get; set; }
        public int Calibre18 { get; set; }
        public int Calibre19 { get; set; }
        public int Calibre20 { get; set; }
        public int Calibre22 { get; set; }
        public int Calibre24 { get; set; }

        public DateTime? Fecha { get; set; }
    }

    public class TerapiaRegistroDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int Msd { get; set; }
        public int Msi { get; set; }
        public int Mii { get; set; }

        public int Calibre14 { get; set; }
        public int Calibre16 { get; set; }
        public int Calibre17 { get; set; }
        public int Calibre18 { get; set; }
        public int Calibre19 { get; set; }
        public int Calibre20 { get; set; }
        public int Calibre22 { get; set; }
        public int Calibre24 { get; set; }

        public int TotalSitios { get; set; }

        public int TotalCateteres { get; set; }

        public string CreadoPor { get; set; }
    }

    /// <summary>
    /// Fila tipo Excel de la pestaña "Eventos Diarios": PersonasInstalaron es
    /// solo el número de personas que instalaron catéteres ese día, no quiénes.
    /// </summary>
    public class TerapiaEventoDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int TotalEventos { get; set; }

        public int TotalCateterColocados { get; set; }

        public int PersonasInstalaron { get; set; }

        public int TotalIntervenciones { get; set; }
    }

    public class TerapiaEventoInputDto
    {
        public DateTime Fecha { get; set; }

        public int TotalEventos { get; set; }

        public int TotalCateterColocados { get; set; }

        public int PersonasInstalaron { get; set; }

        public int TotalIntervenciones { get; set; }
    }

    /// <summary>Cuerpo del POST de cancelación: la razón es opcional.</summary>
    public class CancelarMovimientoDto
    {
        public string Razon { get; set; }
    }
}
