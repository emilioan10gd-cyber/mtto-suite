using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/biomedico-insumos")]
    [RequiereArea("biomedico")]
    public class BiomedicoInsumosController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(List<BioInsumoDto>))]
        public IHttpActionResult Listar(string q = null, string categoria = null, bool soloActivos = true, bool soloBajoMinimo = false)
        {
            var consulta = db.vw_bio_insumos.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                consulta = consulta.Where(i => i.nombre.Contains(q) || i.clave.Contains(q));
            if (!string.IsNullOrWhiteSpace(categoria))
                consulta = consulta.Where(i => i.categoria == categoria);
            if (soloActivos)
                consulta = consulta.Where(i => i.activo);
            if (soloBajoMinimo)
                consulta = consulta.Where(i => i.existencia_total < i.stock_minimo);

            var datos = consulta.OrderBy(i => i.nombre).ToList().Select(Mapear).ToList();
            return Ok(datos);
        }

        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(BioInsumoDto))]
        public IHttpActionResult Obtener(int id)
        {
            var v = db.vw_bio_insumos.AsNoTracking().FirstOrDefault(i => i.insumo_id == id);
            if (v == null) return NotFound();
            return Ok(Mapear(v));
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(BioInsumoDto))]
        public IHttpActionResult Guardar(BioInsumoInputDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");
            if (string.IsNullOrWhiteSpace(entrada.Clave)) return BadRequest("La clave es obligatoria.");
            if (string.IsNullOrWhiteSpace(entrada.Nombre)) return BadRequest("El nombre es obligatorio.");

            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.usp_bio_alta_insumo @clave, @nombre, @categoria, @unidad, @descripcion, @stock_minimo, @stock_maximo",
                    new object[]
                    {
                        new SqlParameter("@clave", entrada.Clave),
                        new SqlParameter("@nombre", entrada.Nombre),
                        new SqlParameter("@categoria", (object)entrada.Categoria ?? DBNull.Value),
                        new SqlParameter("@unidad", entrada.Unidad ?? "Pieza"),
                        new SqlParameter("@descripcion", (object)entrada.Descripcion ?? DBNull.Value),
                        new SqlParameter("@stock_minimo", entrada.StockMinimo),
                        new SqlParameter("@stock_maximo", (object)entrada.StockMaximo ?? DBNull.Value)
                    });
            }
            catch (Exception ex)
            {
                var validacion = SqlErrorHelper.ErrorDeValidacion(ex);
                if (validacion != null) return Content(HttpStatusCode.BadRequest, new { error = validacion.Message });
                throw;
            }

            var v = db.vw_bio_insumos.AsNoTracking().FirstOrDefault(i => i.clave == entrada.Clave);
            return v == null ? (IHttpActionResult)NotFound() : Ok(Mapear(v));
        }

        [HttpGet]
        [Route("{id:int}/lotes")]
        [ResponseType(typeof(List<BioLoteDto>))]
        public IHttpActionResult Lotes(int id, bool soloConExistencia = false)
        {
            var consulta = db.vw_bio_lotes.AsNoTracking().Where(l => l.insumo_id == id);
            if (soloConExistencia) consulta = consulta.Where(l => l.existencia > 0);

            var datos = consulta.OrderBy(l => l.caducidad ?? DateTime.MaxValue).ToList().Select(MapearLote).ToList();
            return Ok(datos);
        }

        [HttpGet]
        [Route("lotes/por-vencer")]
        [ResponseType(typeof(List<BioLoteDto>))]
        public IHttpActionResult LotesPorVencer(int dias = 90)
        {
            var limite = DateTime.Today.AddDays(dias);
            var datos = db.vw_bio_lotes.AsNoTracking()
                .Where(l => l.existencia > 0 && l.caducidad != null && l.caducidad <= limite)
                .OrderBy(l => l.caducidad)
                .ToList().Select(MapearLote).ToList();
            return Ok(datos);
        }

        private static BioLoteDto MapearLote(vw_bio_lotes v) => new BioLoteDto
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
        };

        private static BioInsumoDto Mapear(vw_bio_insumos v) => new BioInsumoDto
        {
            Id = v.insumo_id,
            Clave = v.clave,
            Nombre = v.nombre,
            Descripcion = v.descripcion,
            Categoria = v.categoria,
            CategoriaId = v.categoria_id,
            Unidad = v.unidad,
            StockMinimo = v.stock_minimo,
            StockMaximo = v.stock_maximo,
            ExistenciaTotal = v.existencia_total,
            CaducidadProxima = v.caducidad_proxima,
            Activo = v.activo,
            ActualizadoEn = v.actualizado_en
        };

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
