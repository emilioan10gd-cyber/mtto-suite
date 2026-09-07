using System;
using System.Collections.Generic;

// DTOs del área de Biomédico (equipo, insumos, mantenimiento, manuales, fallas).
namespace mtto.Models
{
    public class BioEquipoDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Marca { get; set; }

        public string Modelo { get; set; }

        public string NumeroSerie { get; set; }

        public string Area { get; set; }

        public int AreaId { get; set; }

        public string Categoria { get; set; }

        public int? CategoriaId { get; set; }

        public string Estado { get; set; }

        public bool EsBaja { get; set; }

        public int EstadoId { get; set; }

        public string Ubicacion { get; set; }

        public string Comentarios { get; set; }

        public DateTime? FechaAdquisicion { get; set; }

        public string MotivoBaja { get; set; }

        public bool Activo { get; set; }

        public DateTime ActualizadoEn { get; set; }

        public DateTime CreadoEn { get; set; }

        public int TotalMantenimientos { get; set; }

        public int MantenimientosVencidos { get; set; }

        public int TotalFallas { get; set; }

        public int TotalManuales { get; set; }
    }

    public class BioEquipoInputDto
    {
        public int? Id { get; set; }

        public string Nombre { get; set; }

        public string Area { get; set; }

        public string Categoria { get; set; }

        public string Estado { get; set; } = "Operativo";

        public string Descripcion { get; set; }

        public string Marca { get; set; }

        public string Modelo { get; set; }

        public string NumeroSerie { get; set; }

        public string Ubicacion { get; set; }

        public string Comentarios { get; set; }

        public DateTime? FechaAdquisicion { get; set; }
    }

    public class BioCambioEstadoDto
    {
        public string Estado { get; set; }

        public string MotivoBaja { get; set; }
    }

    public class BioCatalogosDto
    {
        public IEnumerable<CatalogoItemDto> Areas { get; set; }

        public IEnumerable<CatalogoItemDto> CategoriasEquipo { get; set; }

        public IEnumerable<CatalogoItemDto> CategoriasInsumo { get; set; }

        public IEnumerable<CatalogoItemDto> EstadosEquipo { get; set; }
    }

    public class BioCatalogoCreateDto
    {
        public string Nombre { get; set; }
    }

    public class BioInsumoDto
    {
        public int Id { get; set; }

        public string Clave { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Categoria { get; set; }

        public int? CategoriaId { get; set; }

        public string Unidad { get; set; }

        public decimal StockMinimo { get; set; }

        public decimal? StockMaximo { get; set; }

        public decimal ExistenciaTotal { get; set; }

        public DateTime? CaducidadProxima { get; set; }

        public bool Activo { get; set; }

        public DateTime ActualizadoEn { get; set; }
    }

    public class BioInsumoInputDto
    {
        public string Clave { get; set; }

        public string Nombre { get; set; }

        public string Categoria { get; set; }

        public string Unidad { get; set; } = "Pieza";

        public string Descripcion { get; set; }

        public decimal StockMinimo { get; set; }

        public decimal? StockMaximo { get; set; }
    }

    public class BioLoteDto
    {
        public int Id { get; set; }

        public int InsumoId { get; set; }

        public string Clave { get; set; }

        public string Insumo { get; set; }

        public string Categoria { get; set; }

        public string Lote { get; set; }

        public DateTime? Caducidad { get; set; }

        public int? DiasParaVencer { get; set; }

        public string EstadoCaducidad { get; set; }

        public decimal Existencia { get; set; }

        public string Notas { get; set; }
    }

    public class BioMovimientoInputDto
    {
        public string ClaveInsumo { get; set; }

        public string Lote { get; set; }

        public string Tipo { get; set; }

        public decimal Cantidad { get; set; }

        public DateTime? Caducidad { get; set; }

        public string Responsable { get; set; }

        public string Observaciones { get; set; }
    }

    public class BioMantenimientoDto
    {
        public int Id { get; set; }

        public int EquipoId { get; set; }

        public string Equipo { get; set; }

        public string Area { get; set; }

        public string Tipo { get; set; }

        public DateTime FechaProgramada { get; set; }

        public DateTime? FechaRealizada { get; set; }

        public string Estado { get; set; }

        public string TecnicoResponsable { get; set; }

        public string Proveedor { get; set; }

        public string Descripcion { get; set; }

        public int? FrecuenciaMeses { get; set; }
    }

    public class BioMantenimientoInputDto
    {
        public int EquipoId { get; set; }

        public string Tipo { get; set; }

        public DateTime FechaProgramada { get; set; }

        public string TecnicoResponsable { get; set; }

        public string Proveedor { get; set; }

        public string Descripcion { get; set; }

        public int? FrecuenciaMeses { get; set; }
    }

    public class BioMantenimientoRealizadoDto
    {
        public DateTime? FechaRealizada { get; set; }

        public string TecnicoResponsable { get; set; }

        public string Descripcion { get; set; }
    }

    public class BioManualDto
    {
        public int Id { get; set; }

        public int EquipoId { get; set; }

        public string NombreArchivo { get; set; }

        public string TipoArchivo { get; set; }

        public long? TamanoBytes { get; set; }

        public DateTime SubidoEn { get; set; }
    }

    public class BioFallaDto
    {
        public int Id { get; set; }

        public int EquipoId { get; set; }

        public DateTime Fecha { get; set; }

        public string Sintoma { get; set; }

        public string Causa { get; set; }

        public string Solucion { get; set; }

        public string Tecnico { get; set; }

        public string Estado { get; set; }
    }

    public class BioFallaInputDto
    {
        public int EquipoId { get; set; }

        public string Sintoma { get; set; }

        public string Tecnico { get; set; }
    }

    public class BioFallaResolverDto
    {
        public string Causa { get; set; }

        public string Solucion { get; set; }
    }

    /// <summary>
    /// Resultado de "Sugerencias": coincidencias por palabras clave contra
    /// fallas ya resueltas (de este equipo o de otros del mismo modelo) y
    /// contra el texto de los manuales. Búsqueda local, sin IA externa.
    /// </summary>
    public class BioSugerenciaDto
    {
        /// <summary>"falla" o "manual".</summary>
        public string Origen { get; set; }

        public int? FallaId { get; set; }

        public int? ManualId { get; set; }

        public string EquipoNombre { get; set; }

        public bool MismoEquipo { get; set; }

        public string Titulo { get; set; }

        public string Fragmento { get; set; }

        public string Causa { get; set; }

        public string Solucion { get; set; }

        public DateTime? Fecha { get; set; }

        public int Relevancia { get; set; }
    }

    public class BioSugerenciasResultadoDto
    {
        public IEnumerable<BioSugerenciaDto> Sugerencias { get; set; }
    }

    public class BioDashboardDto
    {
        public BioKpisDto Kpis { get; set; }

        public IEnumerable<BioResumenAreaDto> PorArea { get; set; }

        public IEnumerable<BioResumenEstadoDto> PorEstado { get; set; }

        public IEnumerable<BioMantenimientoDto> ProximosMantenimientos { get; set; }

        public IEnumerable<BioInsumoDto> InsumosBajoStock { get; set; }

        public IEnumerable<BioLoteDto> LotesPorVencer { get; set; }
    }

    public class BioKpisDto
    {
        public int TotalEquipos { get; set; }

        public int EquiposActivos { get; set; }

        public int EquiposDeBaja { get; set; }

        public int MantenimientosVencidos { get; set; }

        public int MantenimientosProximos30d { get; set; }

        public int InsumosBajoMinimo { get; set; }

        public int LotesPorVencer { get; set; }

        public int FallasAbiertas { get; set; }
    }

    public class BioResumenAreaDto
    {
        public int AreaId { get; set; }

        public string Area { get; set; }

        public int TotalEquipos { get; set; }
    }

    public class BioResumenEstadoDto
    {
        public int EstadoId { get; set; }

        public string Estado { get; set; }

        public int TotalEquipos { get; set; }
    }

    public class BioFrecuenciaDto
    {
        public int FrecuenciaMeses { get; set; }
    }
}
