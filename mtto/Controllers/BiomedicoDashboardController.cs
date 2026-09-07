using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/biomedico-dashboard")]
    [RequiereArea("biomedico")]
    public class BiomedicoDashboardController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(BioDashboardDto))]
        public IHttpActionResult Todo()
        {
            return Ok(new BioDashboardDto
            {
                Kpis = LeerKpis(),
                PorArea = LeerPorArea(),
                PorEstado = LeerPorEstado(),
                ProximosMantenimientos = LeerProximosMantenimientos(),
                InsumosBajoStock = LeerInsumosBajoStock(),
                LotesPorVencer = LeerLotesPorVencer()
            });
        }

        private BioKpisDto LeerKpis()
        {
            // vw_bio_dashboard_kpi no está en el modelo EF (fila única sin
            // llave), así que se lee con SQL directo, igual que vw_mtto_dashboard_kpi.
            return db.Database.SqlQuery<BioKpisDto>(
                @"SELECT total_equipos               AS TotalEquipos,
                         equipos_activos              AS EquiposActivos,
                         equipos_de_baja              AS EquiposDeBaja,
                         mantenimientos_vencidos      AS MantenimientosVencidos,
                         mantenimientos_proximos_30d  AS MantenimientosProximos30d,
                         insumos_bajo_minimo          AS InsumosBajoMinimo,
                         lotes_por_vencer             AS LotesPorVencer,
                         fallas_abiertas              AS FallasAbiertas
                    FROM dbo.vw_bio_dashboard_kpi").First();
        }

        private List<BioResumenAreaDto> LeerPorArea()
        {
            return db.vw_bio_inventario.AsNoTracking()
                .GroupBy(e => new { e.area_id, e.area })
                .Select(g => new { g.Key.area_id, g.Key.area, total = g.Count() })
                .ToList()
                .Select(g => new BioResumenAreaDto { AreaId = g.area_id, Area = g.area, TotalEquipos = g.total })
                .OrderByDescending(a => a.TotalEquipos)
                .ToList();
        }

        private List<BioResumenEstadoDto> LeerPorEstado()
        {
            return db.vw_bio_inventario.AsNoTracking()
                .GroupBy(e => new { e.estado_id, e.estado })
                .Select(g => new { g.Key.estado_id, g.Key.estado, total = g.Count() })
                .ToList()
                .Select(g => new BioResumenEstadoDto { EstadoId = g.estado_id, Estado = g.estado, TotalEquipos = g.total })
                .OrderByDescending(e => e.TotalEquipos)
                .ToList();
        }

        private List<BioMantenimientoDto> LeerProximosMantenimientos()
        {
            var limite = DateTime.Today.AddDays(30);
            return db.vw_bio_mantenimiento.AsNoTracking()
                .Where(m => m.estado == "Programado" && m.fecha_programada <= limite)
                .OrderBy(m => m.fecha_programada)
                .ToList()
                .Select(v => new BioMantenimientoDto
                {
                    Id = v.mantenimiento_id,
                    EquipoId = v.equipo_id,
                    Equipo = v.equipo,
                    Area = v.area,
                    Tipo = v.tipo,
                    FechaProgramada = v.fecha_programada,
                    FechaRealizada = v.fecha_realizada,
                    Estado = v.estado,
                    TecnicoResponsable = v.tecnico_responsable,
                    Proveedor = v.proveedor,
                    Descripcion = v.descripcion,
                    FrecuenciaMeses = v.frecuencia_meses
                })
                .ToList();
        }

        private List<BioInsumoDto> LeerInsumosBajoStock()
        {
            return db.vw_bio_insumos.AsNoTracking()
                .Where(i => i.existencia_total < i.stock_minimo)
                .OrderBy(i => i.nombre)
                .ToList()
                .Select(v => new BioInsumoDto
                {
                    Id = v.insumo_id,
                    Clave = v.clave,
                    Nombre = v.nombre,
                    Categoria = v.categoria,
                    Unidad = v.unidad,
                    StockMinimo = v.stock_minimo,
                    StockMaximo = v.stock_maximo,
                    ExistenciaTotal = v.existencia_total,
                    CaducidadProxima = v.caducidad_proxima,
                    Activo = v.activo,
                    ActualizadoEn = v.actualizado_en
                })
                .ToList();
        }

        private List<BioLoteDto> LeerLotesPorVencer()
        {
            return db.vw_bio_lotes.AsNoTracking()
                .Where(l => l.estado_caducidad == "Por vencer" && l.existencia > 0)
                .OrderBy(l => l.caducidad)
                .ToList()
                .Select(v => new BioLoteDto
                {
                    Id = v.lote_id,
                    InsumoId = v.insumo_id,
                    Clave = v.clave,
                    Insumo = v.insumo,
                    Categoria = v.categoria,
                    Lote = v.lote,
                    Caducidad = v.caducidad,
                    DiasParaVencer = v.dias_para_vencer,
                    EstadoCaducidad = v.estado_caducidad,
                    Existencia = v.existencia,
                    Notas = v.notas
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
