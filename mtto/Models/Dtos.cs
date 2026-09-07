using System;
using System.Collections.Generic;

namespace mtto.Models
{
    // ========================================================================
    // DTOs: lo que la API expone hacia la app web.
    // Nunca devolvemos entidades EF directamente: arrastran propiedades de
    // navegación que provocan ciclos al serializar y filtran la forma interna
    // de la base de datos hacia el cliente.
    // ========================================================================

    /// <summary>Sobre genérico para listados paginados.</summary>
    public class PaginaDto<T>
    {
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int Tamano { get; set; }
        public int TotalPaginas { get; set; }
        public IEnumerable<T> Datos { get; set; }
    }

    /// <summary>Fila del inventario, lista para pintarse en una tabla.</summary>
    public class ArticuloDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }

        // Nombres resueltos: la app web no debería tener que buscar el id.
        public string Categoria { get; set; }
        public string Unidad { get; set; }
        public string Almacen { get; set; }
        public string Proveedor { get; set; }
        public string Estado { get; set; }

        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal? StockMaximo { get; set; }
        public decimal? CostoUnitario { get; set; }
        public decimal ValorTotal { get; set; }

        /// <summary>Sin stock | Bajo minimo | Normal | Sobre maximo</summary>
        public string Nivel { get; set; }
        public DateTime UltimaActualizacion { get; set; }

        // Ids incluidos aparte para precargar los combos al editar.
        public int CategoriaId { get; set; }
        public int UnidadId { get; set; }
        public int AlmacenId { get; set; }
        public int? ProveedorId { get; set; }
        public int EstadoId { get; set; }
    }

    /// <summary>Alta o edición de artículo. Se resuelve por nombre de catálogo.</summary>
    public class ArticuloInputDto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string Unidad { get; set; }
        public string Almacen { get; set; }
        public string Estado { get; set; }
        public string Proveedor { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal? StockMaximo { get; set; }
        public decimal? CostoUnitario { get; set; }
    }

    /// <summary>Renglón de la bitácora de entradas y salidas.</summary>
    public class MovimientoDto
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string CodigoArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public decimal Cantidad { get; set; }
        public string Almacen { get; set; }
        public string AlmacenDestino { get; set; }
        public string Responsable { get; set; }
        public string Observaciones { get; set; }
        public decimal? StockPrevio { get; set; }
        public decimal? StockResultante { get; set; }
        /// <summary>+1 suma existencia, -1 resta.</summary>
        public short Signo { get; set; }
    }

    /// <summary>Petición para registrar un movimiento.</summary>
    public class MovimientoInputDto
    {
        public string CodigoArticulo { get; set; }
        /// <summary>Entrada | Salida | Ajuste (+) | Ajuste (-) | Transferencia</summary>
        public string Tipo { get; set; }
        public decimal Cantidad { get; set; }
        public string Responsable { get; set; }
        public string Observaciones { get; set; }
        /// <summary>Solo para Transferencia: nombre o código del almacén destino.</summary>
        public string AlmacenDestino { get; set; }
        public DateTime? Fecha { get; set; }
    }

    /// <summary>Resultado de registrar un movimiento.</summary>
    public class MovimientoResultadoDto
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public decimal StockPrevio { get; set; }
        public decimal StockResultante { get; set; }
    }

    // ---------------------------------------------------------------- Dashboard

    public class KpisDto
    {
        public int TotalArticulos { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public int ArticulosBajoMinimo { get; set; }
        public int ArticulosActivos { get; set; }
        public int MovimientosDelMes { get; set; }
    }

    public class ResumenCategoriaDto
    {
        public int CategoriaId { get; set; }
        public string Codigo { get; set; }
        public string Categoria { get; set; }
        public int TotalArticulos { get; set; }
        public decimal TotalExistencias { get; set; }
        public decimal ValorTotal { get; set; }
        public int BajoMinimo { get; set; }
    }

    public class DistribucionEstadoDto
    {
        public int EstadoId { get; set; }
        public string Estado { get; set; }
        public int TotalArticulos { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class MovimientosMesDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Entradas { get; set; }
        public int Salidas { get; set; }
        public int TotalMovimientos { get; set; }
        public decimal UnidadesEntrada { get; set; }
        public decimal UnidadesSalida { get; set; }
    }

    public class BajoStockDto
    {
        public int ArticuloId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Almacen { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal Faltante { get; set; }
    }

    /// <summary>Todo lo que el dashboard necesita en una sola llamada.</summary>
    public class DashboardDto
    {
        public KpisDto Kpis { get; set; }
        public IEnumerable<ResumenCategoriaDto> PorCategoria { get; set; }
        public IEnumerable<DistribucionEstadoDto> PorEstado { get; set; }
        public IEnumerable<MovimientosMesDto> PorMes { get; set; }
        public IEnumerable<BajoStockDto> BajoStock { get; set; }
    }

    // ---------------------------------------------------------------- Catálogos

    public class CatalogoItemDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
    }

    public class TipoMovimientoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public short Signo { get; set; }
        public bool RequiereDestino { get; set; }
    }

    // ---------------------------------------------------------- Administración de catálogos

    /// <summary>
    /// Fila de un catálogo (categoría/almacén/proveedor/unidad) para la pantalla de
    /// administración. Trae todos los campos posibles; cada catálogo usa solo los
    /// suyos (p. ej. Unidad no tiene Codigo, Categoría no tiene Telefono).
    /// </summary>
    public class CatalogoAdminDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public string Descripcion { get; set; }   // categoría
        public string Ubicacion { get; set; }     // almacén
        public string Responsable { get; set; }   // almacén
        public string Contacto { get; set; }      // proveedor
        public string Telefono { get; set; }      // proveedor
        public string Correo { get; set; }        // proveedor
        public string Abreviatura { get; set; }   // unidad
        public bool? PermiteDecimal { get; set; } // unidad
    }

    /// <summary>Alta o edición de una fila de catálogo.</summary>
    public class CatalogoAdminInputDto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public string Responsable { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Abreviatura { get; set; }
        public bool? PermiteDecimal { get; set; }
    }

    /// <summary>Activar / dar de baja una fila de catálogo. Nunca se borra: protege el historial.</summary>
    public class EstadoActivoDto
    {
        public bool Activo { get; set; }
    }

    /// <summary>Todos los combos de los formularios en una sola llamada.</summary>
    public class CatalogosDto
    {
        public IEnumerable<CatalogoItemDto> Categorias { get; set; }
        public IEnumerable<CatalogoItemDto> Almacenes { get; set; }
        public IEnumerable<CatalogoItemDto> Proveedores { get; set; }
        public IEnumerable<CatalogoItemDto> Unidades { get; set; }
        public IEnumerable<CatalogoItemDto> Estados { get; set; }
        public IEnumerable<TipoMovimientoDto> TiposMovimiento { get; set; }
    }

    // ---------------------------------------------------------- Comparativa de precios

    /// <summary>Un precio que un proveedor ofrece para un artículo en un rango de cantidad.</summary>
    public class PrecioProveedorDto
    {
        public int Id { get; set; }
        public string ArticuloCodigo { get; set; }
        public string ArticuloNombre { get; set; }
        public string Proveedor { get; set; }
        /// <summary>Menudeo | Pequeño | Mayoreo | Gran Volumen (deducido del rango)</summary>
        public string TipoPrecio { get; set; }
        /// <summary>Cantidad a partir de la cual aplica este precio.</summary>
        public decimal CantidadMinima { get; set; }
        /// <summary>Cantidad hasta la cual aplica este precio (null = sin límite superior).</summary>
        public decimal? CantidadMaxima { get; set; }
        /// <summary>Precio por unidad individual.</summary>
        public decimal PrecioUnitario { get; set; }
        /// <summary>Costo total = precio_unitario × cantidad_consultada (solo si se especificó cantidad en la query).</summary>
        public decimal? CostoTotal { get; set; }
        public string Notas { get; set; }
        public DateTime ActualizadoEn { get; set; }
        /// <summary>true si es el precio más bajo para esta cantidad/comparativa.</summary>
        public bool EsMasBarato { get; set; }
    }

    /// <summary>Alta o edición de un precio de proveedor.</summary>
    public class PrecioProveedorInputDto
    {
        public string ArticuloCodigo { get; set; }
        public string Proveedor { get; set; }
        public decimal? CantidadMinima { get; set; }
        public decimal? CantidadMaxima { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Notas { get; set; }
    }

    /// <summary>Todos los precios registrados de un artículo, listos para tabla + gráfica.</summary>
    public class ComparativaPreciosDto
    {
        public string ArticuloCodigo { get; set; }
        public string ArticuloNombre { get; set; }
        /// <summary>Cantidad consultada (si se especificó en la query).</summary>
        public decimal? CantidadConsultada { get; set; }
        public IEnumerable<PrecioProveedorDto> Precios { get; set; }
    }
}
