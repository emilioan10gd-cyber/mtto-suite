using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/cateter-dashboard")]
    [RequiereArea("cateter")]
    public class CateterDashboardController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(CateterDashboardDto))]
        public IHttpActionResult Todo(int mesesHistorico = 12, int topBajoStock = 10, int topPorCaducar = 10)
        {
            return Ok<CateterDashboardDto>(new CateterDashboardDto
            {
                Kpis = LeerKpis(),
                PorCategoria = LeerPorCategoria(),
                PorNivel = LeerPorNivel(),
                PorMes = LeerPorMes(mesesHistorico),
                PorCaducar = LeerPorCaducar(topPorCaducar),
                BajoStock = LeerBajoStock(topBajoStock)
            });
        }

        [HttpGet]
        [Route("kpis")]
        [ResponseType(typeof(CateterKpisDto))]
        public IHttpActionResult Kpis()
        {
            return Ok<CateterKpisDto>(LeerKpis());
        }

        [HttpGet]
        [Route("por-categoria")]
        public IHttpActionResult PorCategoria()
        {
            return Ok<List<CateterResumenCategoriaDto>>(LeerPorCategoria());
        }

        [HttpGet]
        [Route("por-mes")]
        public IHttpActionResult PorMes(int meses = 12)
        {
            return Ok<List<CateterMovimientosMesDto>>(LeerPorMes(meses));
        }

        [HttpGet]
        [Route("por-caducar")]
        public IHttpActionResult PorCaducar(int top = 10)
        {
            return Ok<List<CateterLoteDto>>(LeerPorCaducar(top));
        }

        [HttpGet]
        [Route("bajo-stock")]
        public IHttpActionResult BajoStock(int top = 10)
        {
            return Ok<List<CateterArticuloDto>>(LeerBajoStock(top));
        }

        [HttpGet]
        [Route("por-nivel")]
        public IHttpActionResult PorNivel()
        {
            return Ok<List<DistribucionNivelCateterDto>>(LeerPorNivel());
        }

        private CateterKpisDto LeerKpis()
        {
            vw_cateter_dashboard_kpi vw_cateter_dashboard_kpi2 = (db.vw_cateter_dashboard_kpi.AsNoTracking()).FirstOrDefault();
            if (vw_cateter_dashboard_kpi2 == null)
            {
                return new CateterKpisDto();
            }
            return new CateterKpisDto
            {
                TotalClaves = vw_cateter_dashboard_kpi2.total_claves.GetValueOrDefault(),
                TotalPiezas = vw_cateter_dashboard_kpi2.total_piezas.GetValueOrDefault(),
                PiezasVigentes = vw_cateter_dashboard_kpi2.piezas_vigentes.GetValueOrDefault(),
                PiezasVencidas = vw_cateter_dashboard_kpi2.piezas_vencidas.GetValueOrDefault(),
                PiezasEnAlmacen = vw_cateter_dashboard_kpi2.piezas_en_almacen.GetValueOrDefault(),
                PiezasEnStock = vw_cateter_dashboard_kpi2.piezas_en_stock.GetValueOrDefault(),
                ClavesBajoMinimo = vw_cateter_dashboard_kpi2.claves_bajo_minimo.GetValueOrDefault(),
                PiezasPorVencer30 = vw_cateter_dashboard_kpi2.piezas_por_vencer_30.GetValueOrDefault(),
                PiezasPorVencer90 = vw_cateter_dashboard_kpi2.piezas_por_vencer_90.GetValueOrDefault(),
                MovimientosDelMes = vw_cateter_dashboard_kpi2.movimientos_del_mes.GetValueOrDefault()
            };
        }

        private List<CateterResumenCategoriaDto> LeerPorCategoria()
        {
            return (from c in db.vw_cateter_resumen_categoria.AsNoTracking()
                orderby c.categoria
                select new CateterResumenCategoriaDto
                {
                    Codigo = c.codigo,
                    Categoria = c.categoria,
                    TotalArticulos = (c.total_articulos ?? 0),
                    TotalExistencias = c.total_existencias,
                    TotalVigente = c.total_vigente,
                    TotalVencido = c.total_vencido,
                    EnAlmacen = c.en_almacen,
                    EnStock = c.en_stock,
                    BajoMinimo = (c.bajo_minimo ?? 0)
                }).ToList();
        }

        private List<CateterMovimientosMesDto> LeerPorMes(int meses)
        {
            if (meses < 1 || meses > 60)
            {
                meses = 12;
            }
            DateTime corte = DateTime.Now.AddMonths(-meses);
            return (from m in db.vw_cateter_movimientos_mes.AsNoTracking()
                where m.anio > (int?)((DateTime)corte).Year || (m.anio == (int?)((DateTime)corte).Year && m.mes >= (int?)((DateTime)corte).Month)
                orderby m.anio, m.mes
                select new CateterMovimientosMesDto
                {
                    Anio = (m.anio ?? 0),
                    Mes = (m.mes ?? 0),
                    TotalMovimientos = (m.total_movimientos ?? 0),
                    UnidadesEntrada = m.unidades_entrada,
                    UnidadesSalida = m.unidades_salida,
                    UnidadesMerma = m.unidades_merma,
                    UnidadesTraslado = m.unidades_traslado
                }).ToList();
        }

        private List<CateterLoteDto> LeerPorCaducar(int top)
        {
            if (top < 1 || top > 200)
            {
                top = 10;
            }
            return (from l in (from l in db.vw_cateter_lotes.AsNoTracking()
                    where l.total > 0m && (l.estado_caducidad == "Vencido" || l.estado_caducidad == "Critico" || l.estado_caducidad == "Por vencer")
                    orderby l.caducidad
                    select l).Take(top)
                select new CateterLoteDto
                {
                    Id = l.lote_id,
                    ArticuloId = l.articulo_id,
                    Clave = l.clave,
                    Articulo = l.articulo,
                    Categoria = l.categoria,
                    Lote = l.lote,
                    Caducidad = l.caducidad,
                    DiasParaVencer = l.dias_para_vencer,
                    EstadoCaducidad = l.estado_caducidad,
                    EnAlmacen = l.en_almacen,
                    EnStock = l.en_stock,
                    Total = l.total,
                    Notas = l.notas
                }).ToList();
        }

        private List<DistribucionNivelCateterDto> LeerPorNivel()
        {
            return (from a in db.vw_cateter_inventario.AsNoTracking()
                where a.activo
                group a by a.nivel into g
                select new DistribucionNivelCateterDto
                {
                    Nivel = g.Key,
                    TotalClaves = g.Count()
                }).ToList();
        }

        private List<CateterArticuloDto> LeerBajoStock(int top)
        {
            if (top < 1 || top > 200)
            {
                top = 10;
            }
            return (from a in db.vw_cateter_inventario.AsNoTracking()
                where a.activo && (a.nivel == "Bajo minimo" || a.nivel == "Sin stock")
                orderby a.total_vigente
                select a).Take(top).ToList().Select(CateterController.Mapear)
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

