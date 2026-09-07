using System.Collections.Generic;
using System;

// DTOs de Clinica de Cateter y Cuadro Basico (CPM).
namespace mtto.Models
{
    public class CateterArticuloDto
    {
        public int Id { get; set; }

        public string Clave { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Categoria { get; set; }

        public string Unidad { get; set; }

        public string Marca { get; set; }

        public string Referencia { get; set; }

        public string Calibre { get; set; }

        public short? Lumenes { get; set; }

        public string Presentacion { get; set; }

        public decimal EnAlmacen { get; set; }

        public decimal EnStock { get; set; }

        public decimal Total { get; set; }

        public decimal TotalVigente { get; set; }

        public decimal TotalVencido { get; set; }

        public decimal StockMinimo { get; set; }

        public decimal? StockMaximo { get; set; }

        public decimal? CpmMensual { get; set; }

        public int LotesConExistencia { get; set; }

        public DateTime? CaducidadProxima { get; set; }

        public string Nivel { get; set; }

        public string AlertaCaducidad { get; set; }

        public bool Activo { get; set; }

        public DateTime UltimaActualizacion { get; set; }

        public int CategoriaId { get; set; }

        public int UnidadId { get; set; }
    }

    public class CateterArticuloInputDto
    {
        public string Clave { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Categoria { get; set; }

        public string Unidad { get; set; }

        public string Marca { get; set; }

        public string Referencia { get; set; }

        public string Calibre { get; set; }

        public short? Lumenes { get; set; }

        public string Presentacion { get; set; }

        public decimal StockMinimo { get; set; }

        public decimal? StockMaximo { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class CateterCatalogosDto
    {
        public IEnumerable<CatalogoItemDto> Categorias { get; set; }

        public IEnumerable<CateterUbicacionDto> Ubicaciones { get; set; }

        public IEnumerable<CateterTipoMovimientoDto> TiposMovimiento { get; set; }

        public IEnumerable<CatalogoItemDto> Unidades { get; set; }
    }

    public class CateterDashboardDto
    {
        public CateterKpisDto Kpis { get; set; }

        public IEnumerable<CateterResumenCategoriaDto> PorCategoria { get; set; }

        public IEnumerable<DistribucionNivelCateterDto> PorNivel { get; set; }

        public IEnumerable<CateterMovimientosMesDto> PorMes { get; set; }

        public IEnumerable<CateterLoteDto> PorCaducar { get; set; }

        public IEnumerable<CateterArticuloDto> BajoStock { get; set; }
    }

    public class CateterKpisDto
    {
        public int TotalClaves { get; set; }

        public decimal TotalPiezas { get; set; }

        public decimal PiezasVigentes { get; set; }

        public decimal PiezasVencidas { get; set; }

        public decimal PiezasEnAlmacen { get; set; }

        public decimal PiezasEnStock { get; set; }

        public int ClavesBajoMinimo { get; set; }

        public decimal PiezasPorVencer30 { get; set; }

        public decimal PiezasPorVencer90 { get; set; }

        public int MovimientosDelMes { get; set; }
    }

    public class CateterLoteDto
    {
        public int Id { get; set; }

        public int ArticuloId { get; set; }

        public string Clave { get; set; }

        public string Articulo { get; set; }

        public string Categoria { get; set; }

        public string Lote { get; set; }

        public DateTime? Caducidad { get; set; }

        public int? DiasParaVencer { get; set; }

        public string EstadoCaducidad { get; set; }

        public decimal EnAlmacen { get; set; }

        public decimal EnStock { get; set; }

        public decimal Total { get; set; }

        public string Notas { get; set; }
    }

    public class CateterLoteInputDto
    {
        public string Clave { get; set; }

        public string Lote { get; set; }

        public DateTime? Caducidad { get; set; }

        public decimal CantidadInicial { get; set; }

        public string Ubicacion { get; set; }

        public string Responsable { get; set; }

        public string Observaciones { get; set; }

        public string Notas { get; set; }

        /// <summary>
        /// Presentación del fabricante. Una misma clave del cuadro básico puede
        /// tener varias (distinta marca o referencia), y entonces el lote no se
        /// puede asignar sin saber a cuál pertenece.
        /// </summary>
        public string Referencia { get; set; }
    }

    public class CateterMovimientoDto
    {
        public int Id { get; set; }

        public string Folio { get; set; }

        public DateTime Fecha { get; set; }

        public string Tipo { get; set; }

        public string Clasificacion { get; set; }

        public string Clave { get; set; }

        public string Articulo { get; set; }

        public string Lote { get; set; }

        public DateTime? Caducidad { get; set; }

        public decimal Cantidad { get; set; }

        public string UbicacionOrigen { get; set; }

        public string UbicacionDestino { get; set; }

        public string ReferenciaUso { get; set; }

        public string Responsable { get; set; }

        public string Observaciones { get; set; }

        public decimal? SaldoOrigenResultante { get; set; }

        public decimal? SaldoDestinoResultante { get; set; }

        public string Estado { get; set; }

        public string RazonCancelacion { get; set; }

        public DateTime? CanceladoEn { get; set; }

        public string CanceladoPor { get; set; }

        public bool EsReversion { get; set; }
    }

    public class CateterMovimientoInputDto
    {
        public int LoteId { get; set; }

        public string Tipo { get; set; }

        public decimal Cantidad { get; set; }

        public string UbicacionOrigen { get; set; }

        public string UbicacionDestino { get; set; }

        public string ReferenciaUso { get; set; }

        public string Responsable { get; set; }

        public string Observaciones { get; set; }

        public DateTime? Fecha { get; set; }
    }

    public class CateterMovimientosMesDto
    {
        public int Anio { get; set; }

        public int Mes { get; set; }

        public int TotalMovimientos { get; set; }

        public decimal UnidadesEntrada { get; set; }

        public decimal UnidadesSalida { get; set; }

        public decimal UnidadesMerma { get; set; }

        public decimal UnidadesTraslado { get; set; }
    }

    public class CateterResumenCategoriaDto
    {
        public string Codigo { get; set; }

        public string Categoria { get; set; }

        public int TotalArticulos { get; set; }

        public decimal TotalExistencias { get; set; }

        public decimal TotalVigente { get; set; }

        public decimal TotalVencido { get; set; }

        public decimal EnAlmacen { get; set; }

        public decimal EnStock { get; set; }

        public int BajoMinimo { get; set; }
    }

    public class CateterSurtidoInputDto
    {
        public string Clave { get; set; }

        public decimal Cantidad { get; set; }

        public string Tipo { get; set; }

        public string UbicacionOrigen { get; set; }

        public string UbicacionDestino { get; set; }

        public string ReferenciaUso { get; set; }

        public string Responsable { get; set; }

        public string Observaciones { get; set; }
    }

    public class CateterSurtidoLineaDto
    {
        public int LoteId { get; set; }

        public string Lote { get; set; }

        public DateTime? Caducidad { get; set; }

        public decimal Cantidad { get; set; }

        public string Folio { get; set; }
    }

    public class CateterSurtidoResultadoDto
    {
        public string Clave { get; set; }

        public decimal CantidadSolicitada { get; set; }

        public IEnumerable<CateterSurtidoLineaDto> Lineas { get; set; }
    }

    public class CateterTipoMovimientoDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Clasificacion { get; set; }

        public bool RequiereOrigen { get; set; }

        public bool RequiereDestino { get; set; }
    }

    public class CateterUbicacionDto
    {
        public int Id { get; set; }

        public string Codigo { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public bool EsAlmacen { get; set; }

        public bool Activo { get; set; }
    }

    /// <summary>
    /// Alta o edición de una ubicación. El código se genera solo (UBI-00N):
    /// es una llave interna y pedírselo a quien captura solo da oportunidad
    /// de chocar contra el UNIQUE.
    /// </summary>
    public class CateterUbicacionCreateDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        /// <summary>true = resguardo. Decide de dónde puede salir el material.</summary>
        public bool EsAlmacen { get; set; }
    }

    public class DistribucionNivelCateterDto
    {
        public string Nivel { get; set; }

        public int TotalClaves { get; set; }
    }

    public class DistribucionNivelDto
    {
        public int NivelId { get; set; }

        public string Nivel { get; set; }

        public int TotalArticulos { get; set; }

        public decimal ValorTotal { get; set; }
    }

    public class CuadroBasicoItemDto
    {
        public string Clave { get; set; }

        public string Descripcion { get; set; }

        public string Grupo { get; set; }

        public string GrupoTerapeutico { get; set; }

        public decimal? CantidadMensual { get; set; }

        public bool YaEnCatalogo { get; set; }
    }

    public class CuadroBasicoAltaDto
    {
        public string Clave { get; set; }

        public string Nombre { get; set; }

        public string Categoria { get; set; }

        public decimal StockMinimo { get; set; }

        public decimal? StockMaximo { get; set; }

        public string DescripcionPropia { get; set; }
    }

    /// <summary>
    /// Una descripción propia junto a la oficial del CPM, para poder
    /// compararlas lado a lado en la pantalla de catálogos.
    /// </summary>
    public class DescripcionPropiaDto
    {
        public string Clave { get; set; }
        public string DescripcionPropia { get; set; }
        /// <summary>La del cuadro básico. Null si la clave ya no existe en el CPM.</summary>
        public string DescripcionOficial { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        /// <summary>true si la clínica ya maneja esa clave en su inventario.</summary>
        public bool EnCatalogo { get; set; }
    }

    public class ActualizarDescripcionPropiaDto
    {
        public string Clave { get; set; }

        public string DescripcionPropia { get; set; }
    }
}

