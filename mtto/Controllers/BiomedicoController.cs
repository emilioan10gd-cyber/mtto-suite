using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/biomedico")]
    [RequiereArea("biomedico")]
    public class BiomedicoController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(PaginaDto<BioEquipoDto>))]
        public IHttpActionResult Listar(string q = null, string area = null, string categoria = null, string estado = null, bool soloActivos = true, int pagina = 1, int tamano = 50)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 500) tamano = 50;

            var consulta = AplicarFiltros(db.vw_bio_inventario.AsNoTracking(), q, area, categoria, estado, soloActivos);
            var total = consulta.Count();
            var datos = consulta.OrderBy(e => e.area).ThenBy(e => e.nombre)
                .Skip((pagina - 1) * tamano).Take(tamano).ToList()
                .Select(Mapear).ToList();

            return Ok(new PaginaDto<BioEquipoDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)total / tamano),
                Datos = datos
            });
        }

        private static IQueryable<vw_bio_inventario> AplicarFiltros(IQueryable<vw_bio_inventario> consulta, string q, string area, string categoria, string estado, bool soloActivos)
        {
            if (!string.IsNullOrWhiteSpace(q))
                consulta = consulta.Where(e => e.nombre.Contains(q) || e.marca.Contains(q) || e.modelo.Contains(q) || e.numero_serie.Contains(q));
            if (!string.IsNullOrWhiteSpace(area))
                consulta = consulta.Where(e => e.area == area);
            if (!string.IsNullOrWhiteSpace(categoria))
                consulta = consulta.Where(e => e.categoria == categoria);
            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(e => e.estado == estado);
            if (soloActivos)
                consulta = consulta.Where(e => e.activo);
            return consulta;
        }

        /// <summary>GET api/biomedico/bajas — apartado de equipos dados de baja.</summary>
        [HttpGet]
        [Route("bajas")]
        [ResponseType(typeof(List<BioEquipoDto>))]
        public IHttpActionResult Bajas(string q = null, string area = null)
        {
            var consulta = db.vw_bio_inventario.AsNoTracking().Where(e => e.es_baja);
            if (!string.IsNullOrWhiteSpace(q))
                consulta = consulta.Where(e => e.nombre.Contains(q) || e.marca.Contains(q) || e.numero_serie.Contains(q));
            if (!string.IsNullOrWhiteSpace(area))
                consulta = consulta.Where(e => e.area == area);

            var datos = consulta.OrderByDescending(e => e.actualizado_en).ToList().Select(Mapear).ToList();
            return Ok(datos);
        }

        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(BioEquipoDto))]
        public IHttpActionResult Obtener(int id)
        {
            var v = db.vw_bio_inventario.AsNoTracking().FirstOrDefault(e => e.equipo_id == id);
            if (v == null) return NotFound();
            return Ok(Mapear(v));
        }

        /// <summary>
        /// POST api/biomedico — alta o edición (Id presente = edición) vía
        /// usp_bio_alta_equipo. Es la única vía para agregar/editar un equipo.
        /// </summary>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(BioEquipoDto))]
        public IHttpActionResult Guardar(BioEquipoInputDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");
            if (string.IsNullOrWhiteSpace(entrada.Nombre)) return BadRequest("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(entrada.Area)) return BadRequest("El área es obligatoria.");

            var equipoIdOut = new SqlParameter("@equipo_id_out", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.usp_bio_alta_equipo @equipo_id, @nombre, @area, @categoria, @estado, @descripcion, @marca, @modelo, @numero_serie, @ubicacion, @comentarios, @fecha_adquisicion, @equipo_id_out OUTPUT",
                    new object[]
                    {
                        new SqlParameter("@equipo_id", (object)entrada.Id ?? DBNull.Value),
                        new SqlParameter("@nombre", entrada.Nombre),
                        new SqlParameter("@area", entrada.Area),
                        new SqlParameter("@categoria", (object)entrada.Categoria ?? DBNull.Value),
                        new SqlParameter("@estado", entrada.Estado ?? "Operativo"),
                        new SqlParameter("@descripcion", (object)entrada.Descripcion ?? DBNull.Value),
                        new SqlParameter("@marca", (object)entrada.Marca ?? DBNull.Value),
                        new SqlParameter("@modelo", (object)entrada.Modelo ?? DBNull.Value),
                        new SqlParameter("@numero_serie", (object)entrada.NumeroSerie ?? DBNull.Value),
                        new SqlParameter("@ubicacion", (object)entrada.Ubicacion ?? DBNull.Value),
                        new SqlParameter("@comentarios", (object)entrada.Comentarios ?? DBNull.Value),
                        new SqlParameter("@fecha_adquisicion", (object)entrada.FechaAdquisicion ?? DBNull.Value),
                        equipoIdOut
                    });
            }
            catch (Exception ex)
            {
                var validacion = SqlErrorHelper.ErrorDeValidacion(ex);
                if (validacion != null) return Content(HttpStatusCode.BadRequest, new { error = validacion.Message });
                throw;
            }

            var idNuevo = (int)equipoIdOut.Value;
            var v = db.vw_bio_inventario.AsNoTracking().FirstOrDefault(e => e.equipo_id == idNuevo);
            return v == null ? (IHttpActionResult)NotFound() : Ok(Mapear(v));
        }

        /// <summary>
        /// POST api/biomedico/{id}/estado — incluye dar de baja (Estado="De baja",
        /// MotivoBaja obligatorio en ese caso). No borra el equipo: conserva su
        /// historial de mantenimientos/fallas/manuales, solo cambia de estado.
        /// </summary>
        [HttpPost]
        [Route("{id:int}/estado")]
        [ResponseType(typeof(BioEquipoDto))]
        public IHttpActionResult CambiarEstado(int id, BioCambioEstadoDto cambio)
        {
            if (cambio == null || string.IsNullOrWhiteSpace(cambio.Estado))
                return BadRequest("El estado es obligatorio.");

            var equipo = db.bio_equipo.FirstOrDefault(e => e.equipo_id == id);
            if (equipo == null) return NotFound();

            var estado = db.bio_estado_equipo.FirstOrDefault(e => e.nombre == cambio.Estado);
            if (estado == null) return BadRequest("Estado no válido.");

            if (estado.es_baja && string.IsNullOrWhiteSpace(cambio.MotivoBaja))
                return BadRequest("El motivo de la baja es obligatorio.");

            equipo.estado_id = estado.estado_id;
            equipo.motivo_baja = estado.es_baja ? cambio.MotivoBaja.Trim() : null;
            equipo.actualizado_en = DateTime.Now;
            db.SaveChanges();

            var v = db.vw_bio_inventario.AsNoTracking().FirstOrDefault(e => e.equipo_id == id);
            return Ok(Mapear(v));
        }

        /// <summary>
        /// DELETE api/biomedico/{id} — borrado físico, solo si no tiene
        /// mantenimientos/fallas/manuales registrados. Si los tiene, se le pide
        /// dar de baja en vez de borrar (mismo criterio que Catéter).
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Eliminar(int id)
        {
            var equipo = db.bio_equipo.FirstOrDefault(e => e.equipo_id == id);
            if (equipo == null) return NotFound();

            var tieneHistorial = db.bio_mantenimiento.Any(m => m.equipo_id == id)
                || db.bio_falla.Any(f => f.equipo_id == id)
                || db.bio_manual.Any(m => m.equipo_id == id);

            if (tieneHistorial)
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "No se puede eliminar: el equipo tiene historial registrado (mantenimientos, fallas o manuales). Dalo de baja en vez de eliminarlo para conservar el expediente."
                });

            db.bio_equipo.Remove(equipo);
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("exportar")]
        public HttpResponseMessage Exportar(string q = null, string area = null, string categoria = null, string estado = null, bool soloActivos = true)
        {
            var lista = AplicarFiltros(db.vw_bio_inventario.AsNoTracking(), q, area, categoria, estado, soloActivos)
                .OrderBy(e => e.area).ThenBy(e => e.nombre).ToList();

            var filas = lista.Select((e, i) => new BioInventarioExcelExporter.FilaExportacion
            {
                Numero = i + 1,
                Area = e.area,
                Nombre = e.nombre,
                Marca = e.marca,
                Modelo = e.modelo,
                NumeroSerie = e.numero_serie,
                Estado = e.estado,
                Ubicacion = e.ubicacion
            }).ToList();

            var archivo = BioInventarioExcelExporter.Generar("HOSPITAL GENERAL REYNOSA \"DR. JOSÉ MARÍA CANTÚ GARZA\".", DateTime.Now, filas);

            var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(archivo) };
            respuesta.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            respuesta.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"Biomedico_Inventario_{DateTime.Now:yyyy-MM-dd}.xlsx"
            };
            return respuesta;
        }

        internal static BioEquipoDto Mapear(vw_bio_inventario v)
        {
            return new BioEquipoDto
            {
                Id = v.equipo_id,
                Nombre = v.nombre,
                Descripcion = v.descripcion,
                Marca = v.marca,
                Modelo = v.modelo,
                NumeroSerie = v.numero_serie,
                Area = v.area,
                AreaId = v.area_id,
                Categoria = v.categoria,
                CategoriaId = v.categoria_id,
                Estado = v.estado,
                EsBaja = v.es_baja,
                EstadoId = v.estado_id,
                Ubicacion = v.ubicacion,
                Comentarios = v.comentarios,
                FechaAdquisicion = v.fecha_adquisicion,
                MotivoBaja = v.motivo_baja,
                Activo = v.activo,
                ActualizadoEn = v.actualizado_en,
                CreadoEn = v.creado_en,
                TotalMantenimientos = v.total_mantenimientos,
                MantenimientosVencidos = v.mantenimientos_vencidos,
                TotalFallas = v.total_fallas,
                TotalManuales = v.total_manuales
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
