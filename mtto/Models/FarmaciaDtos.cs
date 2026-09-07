using System;
using System.Collections.Generic;

namespace mtto.Models
{
    // Los nombres de estas propiedades estan calcados de lo que leen los JS de
    // mttoweb (farmacia-inventario.js, farmacia-movimientos.js, ...). Se
    // serializan en camelCase por el resolver de WebApiConfig: Codigo -> codigo.

    public class FarmaciaInventarioDto
    {
        public int Id { get; set; }
        /// Clave del cuadro basico (010.000.0104.00). Una misma clave puede
        /// tener varias presentaciones, cada una con su codigo de barras.
        public string Codigo { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string UnidadMedida { get; set; }
        /// Existencia neta: entradas - salidas.
        public decimal Total { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSalidas { get; set; }
        public string Nivel { get; set; }
        public string AlertaCaducidad { get; set; }
        public decimal CpmMensual { get; set; }
        public bool EnCuadroBasico { get; set; }
        public DateTime? CaducidadProxima { get; set; }
        public int LotesConStock { get; set; }
    }

    public class FarmaciaLoteDto
    {
        /// Llave de texto "codigobarras|lote": los lotes salen de una vista
        /// agregada y no tienen id numerico propio.
        public string Id { get; set; }
        public int MedicamentoId { get; set; }
        public string Codigo { get; set; }
        public string Lote { get; set; }
        public DateTime? Caducidad { get; set; }
        public string EstadoCaducidad { get; set; }
        /// Lo que queda del lote: recibido - despachado.
        public decimal Cantidad { get; set; }
        public decimal Recibido { get; set; }
        public decimal Despachado { get; set; }
        public string Notas { get; set; }
    }

    public class FarmaciaMovimientoDto
    {
        public string Tipo { get; set; }
        /// Texto literal capturado en Access (Receta, Colectivo, PISO...).
        public string TipoDetalle { get; set; }
        public string Folio { get; set; }
        public DateTime? Fecha { get; set; }
        public string Codigo { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Lote { get; set; }
        public decimal Cantidad { get; set; }
        public string AreaServicio { get; set; }
        public string Responsable { get; set; }
        public string Observaciones { get; set; }
    }

    public class FarmaciaTipoMovimientoDto
    {
        public string Nombre { get; set; }
        public string Clasificacion { get; set; }
        /// +1 suma al inventario, -1 resta.
        public int Signo { get; set; }
    }

    public class FarmaciaCatalogosDto
    {
        public List<CatalogoItemDto> Categorias { get; set; }
        public List<CatalogoItemDto> Proveedores { get; set; }
        public List<FarmaciaTipoMovimientoDto> TiposMovimiento { get; set; }
    }

    public class FarmaciaKpisDto
    {
        public int TotalMedicamentos { get; set; }
        public decimal PiezasVigentes { get; set; }
        public int MedicamentosBajoMinimo { get; set; }
        public int PiezasPorVencer90 { get; set; }
        public int MedicamentosConStock { get; set; }
        public int MedicamentosSinStock { get; set; }
        public int MedicamentosConCaducados { get; set; }
        public int MedicamentosEnCuadroBasico { get; set; }
    }

    public class FarmaciaCategoriaResumenDto
    {
        public string Categoria { get; set; }
        public int TotalArticulos { get; set; }
        public decimal TotalExistencias { get; set; }
        public int BajoMinimo { get; set; }
    }

    public class FarmaciaNivelResumenDto
    {
        public string Nivel { get; set; }
        public int TotalMedicamentos { get; set; }
    }

    public class FarmaciaMesResumenDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public decimal UnidadesEntrada { get; set; }
        public decimal UnidadesSalida { get; set; }
        /// Cuantos renglones de movimiento hubo (no piezas).
        public int TotalMovimientos { get; set; }
    }

    public class FarmaciaBajoStockDto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Total { get; set; }
        public decimal StockMinimo { get; set; }
    }

    public class FarmaciaDashboardDto
    {
        public FarmaciaKpisDto Kpis { get; set; }
        public List<FarmaciaCategoriaResumenDto> PorCategoria { get; set; }
        public List<FarmaciaNivelResumenDto> PorNivel { get; set; }
        public List<FarmaciaMesResumenDto> PorMes { get; set; }
        public List<FarmaciaBajoStockDto> BajoStock { get; set; }
    }

    // ---------------------------------------------------------------
    // Historial (Etapa 1). Los campos que dependen de un dato que el
    // Access nunca guardo son nullable a proposito: null significa "no
    // se sabe", que no es lo mismo que cero ni que 100%.
    // ---------------------------------------------------------------

    public class FarmaciaRecetaDto
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public DateTime? Fecha { get; set; }
        public string Medico { get; set; }
        public string Especialidad { get; set; }
        public string Servicio { get; set; }
        public string PacienteNombre { get; set; }
        public string PacienteExpediente { get; set; }
        public int TotalLineas { get; set; }
        public decimal TotalEntregado { get; set; }
        /// null en el historial: no se capturaba lo prescrito.
        public decimal? TotalPrescrito { get; set; }
        public string Estado { get; set; }
        /// true = viene del sistema anterior, no se puede dispensar.
        public bool Historica { get; set; }
        public string Observaciones { get; set; }
    }

    public class FarmaciaRecetaLineaDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Articulo { get; set; }
        public decimal? CantidadPrescrita { get; set; }
        public decimal CantidadEntregada { get; set; }
        public decimal Pendiente { get; set; }
        public string EstadoLinea { get; set; }
        public string Lote { get; set; }
        public DateTime? Caducidad { get; set; }
    }

    public class FarmaciaRecetaDetalleDto
    {
        public FarmaciaRecetaDto Receta { get; set; }
        public List<FarmaciaRecetaLineaDto> Detalle { get; set; }
    }

    public class FarmaciaAbastoDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        /// Todas las recetas del mes (capturadas + historicas).
        public int TotalRecetas { get; set; }
        /// Solo las capturadas en esta app: son las unicas que entran al
        /// calculo del abasto, porque de las historicas no se sabe lo prescrito.
        public int RecetasCapturadas { get; set; }
        public int TotalLineas { get; set; }
        public decimal TotalEntregado { get; set; }
        public decimal? TotalPrescrito { get; set; }
        public int? LineasConFaltante { get; set; }
        /// null mientras no haya recetas capturadas con que comparar.
        public decimal? PorcentajeAbasto { get; set; }
    }

    public class FarmaciaColectivoDto
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public DateTime? Fecha { get; set; }
        public string Servicio { get; set; }
        public string EntregadoPor { get; set; }
        public string RecibidoPor { get; set; }
        public string Turno { get; set; }
        public int TotalLineas { get; set; }
        public decimal TotalEntregado { get; set; }
        public bool Historico { get; set; }
    }

    public class FarmaciaServicioClaveDto
    {
        public string Codigo { get; set; }
        public string Articulo { get; set; }
        public decimal CantidadPeriodo { get; set; }
        public int Documentos { get; set; }
        public string Periodo { get; set; }
        /// false = es consumo historico, no un cuadro autorizado.
        public bool Autorizado { get; set; }
    }

    // ---------------------------------------------------------------
    // Etapa 2: entradas de captura y resultado del despacho.
    // ---------------------------------------------------------------

    public class FarmaciaLineaSurtidaDto
    {
        public string Codigobarras { get; set; }
        public string Lote { get; set; }
        public DateTime? Caducidad { get; set; }
        public decimal Cantidad { get; set; }
    }

    public class FarmaciaSurtidoDto
    {
        public List<FarmaciaLineaSurtidaDto> Lineas { get; set; }
        public decimal Entregado { get; set; }
        /// Lo que no alcanzo a surtirse. En una receta esto ES el faltante
        /// que alimenta el indicador de abasto.
        public decimal Faltante { get; set; }
    }

    public class FarmaciaSurtirEntradaDto
    {
        /// Clave del cuadro basico o codigo de barras.
        public string Codigo { get; set; }
        public decimal Cantidad { get; set; }
        public string Tipo { get; set; }
        public string PacienteNombre { get; set; }
        public string PacienteExpediente { get; set; }
        public string Diagnostico { get; set; }
        public string Cie { get; set; }
        public string AreaServicio { get; set; }
        public string Responsable { get; set; }
        public string Observaciones { get; set; }
    }

    public class FarmaciaMovimientoLoteEntradaDto
    {
        /// "codigobarras|lote", tal como lo entrega el listado de lotes.
        public string LoteId { get; set; }
        public string Tipo { get; set; }
        public decimal Cantidad { get; set; }
        public string Responsable { get; set; }
        public string Observaciones { get; set; }
    }

    public class FarmaciaNuevoLoteDto
    {
        public string Codigo { get; set; }
        public string Lote { get; set; }
        public DateTime? Caducidad { get; set; }
        public decimal Cantidad { get; set; }
        public string Ubicacion { get; set; }
        public string Responsable { get; set; }
        public string Observaciones { get; set; }
    }

    public class FarmaciaLineaEntradaDto
    {
        public string Codigo { get; set; }
        public decimal Cantidad { get; set; }
    }

    public class FarmaciaComEntradaDto
    {
        public string ComNumero { get; set; }
        public string Proveedor { get; set; }
        public DateTime? FechaDocumento { get; set; }
        public string Observaciones { get; set; }
        public List<FarmaciaLineaEntradaDto> Detalle { get; set; }
    }

    public class FarmaciaRecepcionEntradaDto
    {
        public string Codigo { get; set; }
        public string Lote { get; set; }
        public DateTime? Caducidad { get; set; }
        public decimal Cantidad { get; set; }
        public string ComNumero { get; set; }
        public string Responsable { get; set; }
    }

    public class FarmaciaNuevaRecetaDto
    {
        public string Medico { get; set; }
        public string PacienteNombre { get; set; }
        public string PacienteExpediente { get; set; }
        public string Servicio { get; set; }
        public DateTime? Fecha { get; set; }
        public string Observaciones { get; set; }
        public List<FarmaciaLineaEntradaDto> Detalle { get; set; }
    }

    public class FarmaciaNuevoColectivoDto
    {
        public string Servicio { get; set; }
        public string EntregadoPor { get; set; }
        public string RecibidoPor { get; set; }
        public string Turno { get; set; }
        public DateTime? Fecha { get; set; }
        public string Observaciones { get; set; }
        public List<FarmaciaLineaEntradaDto> Detalle { get; set; }
    }

    public class FarmaciaServicioClaveEntradaDto
    {
        public string Servicio { get; set; }
        public string Codigo { get; set; }
        public decimal CantidadPeriodo { get; set; }
        public string Periodo { get; set; }
    }

    public class FarmaciaAvanceCompraDto
    {
        public string ComNumero { get; set; }
        public DateTime? FechaDocumento { get; set; }
        public string Proveedor { get; set; }
        public string Codigo { get; set; }
        public string Articulo { get; set; }
        public decimal? CantidadAutorizada { get; set; }
        public decimal CantidadRecibida { get; set; }
        public decimal? Pendiente { get; set; }
        public decimal? PorcentajeSurtido { get; set; }
        public int Renglones { get; set; }
        /// true = no hay COM capturada contra la cual comparar.
        public bool SinComCapturada { get; set; }
    }
}
