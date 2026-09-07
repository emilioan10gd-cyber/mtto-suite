using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;

namespace mtto.Controllers
{
    /// <summary>
    /// Indicadores del tablero. Todo se calcula en las vistas vw_mtto_*, así que
    /// aquí no hay lógica de negocio duplicada.
    /// </summary>
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        private readonly Model1 db = new Model1();

        /// <summary>
        /// GET api/dashboard — todo el tablero en una sola llamada, para que la
        /// app web pinte la pantalla inicial sin encadenar 5 peticiones.
        /// </summary>
        [HttpGet, Route("")]
        [ResponseType(typeof(DashboardDto))]
        public IHttpActionResult Todo(int mesesHistorial = 6, int topBajoStock = 20)
        {
            return Ok(new DashboardDto
            {
                Kpis = LeerKpis(),
                PorCategoria = LeerPorCategoria(),
                PorEstado = LeerPorEstado(),
                PorMes = LeerPorMes(mesesHistorial),
                BajoStock = LeerBajoStock(topBajoStock)
            });
        }

        /// <summary>GET api/dashboard/kpis — las 5 tarjetas de arriba.</summary>
        [HttpGet, Route("kpis")]
        [ResponseType(typeof(KpisDto))]
        public IHttpActionResult Kpis()
        {
            return Ok(LeerKpis());
        }

        /// <summary>GET api/dashboard/por-categoria</summary>
        [HttpGet, Route("por-categoria")]
        [ResponseType(typeof(IEnumerable<ResumenCategoriaDto>))]
        public IHttpActionResult PorCategoria()
        {
            return Ok(LeerPorCategoria());
        }

        /// <summary>GET api/dashboard/por-estado</summary>
        [HttpGet, Route("por-estado")]
        [ResponseType(typeof(IEnumerable<DistribucionEstadoDto>))]
        public IHttpActionResult PorEstado()
        {
            return Ok(LeerPorEstado());
        }

        /// <summary>GET api/dashboard/por-mes?meses=12</summary>
        [HttpGet, Route("por-mes")]
        [ResponseType(typeof(IEnumerable<MovimientosMesDto>))]
        public IHttpActionResult PorMes(int meses = 6)
        {
            return Ok(LeerPorMes(meses));
        }

        /// <summary>GET api/dashboard/bajo-stock?top=0 (0 = todos)</summary>
        [HttpGet, Route("bajo-stock")]
        [ResponseType(typeof(IEnumerable<BajoStockDto>))]
        public IHttpActionResult BajoStock(int top = 0)
        {
            return Ok(LeerBajoStock(top));
        }

        // ------------------------------------------------------------ internos

        private KpisDto LeerKpis()
        {
            // vw_mtto_dashboard_kpi no está en el modelo EF (no tiene llave), así
            // que se lee con SQL directo. Los alias deben empatar con KpisDto.
            return db.Database.SqlQuery<KpisDto>(
                @"SELECT total_articulos        AS TotalArticulos,
                         valor_total_inventario AS ValorTotalInventario,
                         articulos_bajo_minimo  AS ArticulosBajoMinimo,
                         articulos_activos      AS ArticulosActivos,
                         movimientos_del_mes    AS MovimientosDelMes
                    FROM dbo.vw_mtto_dashboard_kpi").First();
        }

        private List<ResumenCategoriaDto> LeerPorCategoria()
        {
            return db.vw_mtto_resumen_categoria.AsNoTracking()
                .OrderByDescending(c => c.total_articulos)
                .ToList()
                .Select(c => new ResumenCategoriaDto
                {
                    CategoriaId = c.categoria_id,
                    Codigo = c.codigo,
                    Categoria = c.categoria,
                    TotalArticulos = c.total_articulos ?? 0,
                    TotalExistencias = c.total_existencias,
                    ValorTotal = c.valor_total,
                    BajoMinimo = c.bajo_minimo ?? 0
                })
                .ToList();
        }

        private List<DistribucionEstadoDto> LeerPorEstado()
        {
            return db.vw_mtto_distribucion_estado.AsNoTracking()
                .ToList()
                .Select(e => new DistribucionEstadoDto
                {
                    EstadoId = e.estado_id,
                    Estado = e.estado,
                    TotalArticulos = e.total_articulos ?? 0,
                    ValorTotal = e.valor_total
                })
                .OrderByDescending(e => e.TotalArticulos)
                .ToList();
        }

        private List<MovimientosMesDto> LeerPorMes(int meses)
        {
            if (meses < 1 || meses > 60) meses = 6;

            // Se ordena descendente para tomar los N más recientes y luego se
            // reordena ascendente, que es como lo espera una gráfica de línea.
            return db.vw_mtto_movimientos_mes.AsNoTracking()
                .OrderByDescending(m => m.anio).ThenByDescending(m => m.mes)
                .Take(meses)
                .ToList()
                .Select(m => new MovimientosMesDto
                {
                    Anio = m.anio ?? 0,
                    Mes = m.mes ?? 0,
                    Entradas = m.entradas ?? 0,
                    Salidas = m.salidas ?? 0,
                    TotalMovimientos = m.total_movimientos ?? 0,
                    UnidadesEntrada = m.unidades_entrada,
                    UnidadesSalida = m.unidades_salida
                })
                .OrderBy(m => m.Anio).ThenBy(m => m.Mes)
                .ToList();
        }

        private List<BajoStockDto> LeerBajoStock(int top)
        {
            var consulta = db.vw_mtto_bajo_stock.AsNoTracking()
                .OrderByDescending(b => b.faltante)
                .AsQueryable();

            if (top > 0) consulta = consulta.Take(top);

            return consulta.ToList()
                .Select(b => new BajoStockDto
                {
                    ArticuloId = b.articulo_id,
                    Codigo = b.codigo,
                    Nombre = b.nombre,
                    Categoria = b.categoria,
                    Almacen = b.almacen,
                    StockActual = b.stock_actual,
                    StockMinimo = b.stock_minimo,
                    Faltante = b.faltante ?? 0m
                })
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
