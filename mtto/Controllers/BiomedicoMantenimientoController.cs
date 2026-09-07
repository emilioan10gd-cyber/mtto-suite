using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/biomedico-mantenimiento")]
    [RequiereArea("biomedico")]
    public class BiomedicoMantenimientoController : ApiController
    {
        private readonly Model1 db = new Model1();

        /// <summary>
        /// GET api/biomedico-mantenimiento?desde=...&amp;hasta=... — para el
        /// calendario mensual. Sin rango, regresa todo lo Programado/Vencido.
        /// </summary>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(List<BioMantenimientoDto>))]
        public IHttpActionResult Listar(DateTime? desde = null, DateTime? hasta = null, int? equipoId = null, string estado = null)
        {
            MarcarVencidos();

            var consulta = db.vw_bio_mantenimiento.AsNoTracking().AsQueryable();
            if (desde.HasValue) consulta = consulta.Where(m => m.fecha_programada >= desde.Value);
            if (hasta.HasValue) consulta = consulta.Where(m => m.fecha_programada <= hasta.Value);
            if (equipoId.HasValue) consulta = consulta.Where(m => m.equipo_id == equipoId.Value);
            if (!string.IsNullOrWhiteSpace(estado)) consulta = consulta.Where(m => m.estado == estado);

            var datos = consulta.OrderBy(m => m.fecha_programada).ToList().Select(Mapear).ToList();
            return Ok(datos);
        }

        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(BioMantenimientoDto))]
        public IHttpActionResult Obtener(int id)
        {
            var v = db.vw_bio_mantenimiento.AsNoTracking().FirstOrDefault(m => m.mantenimiento_id == id);
            if (v == null) return NotFound();
            return Ok(Mapear(v));
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(BioMantenimientoDto))]
        public IHttpActionResult Programar(BioMantenimientoInputDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");
            if (entrada.Tipo != "Preventivo" && entrada.Tipo != "Correctivo")
                return BadRequest("El tipo debe ser Preventivo o Correctivo.");
            if (!db.bio_equipo.Any(e => e.equipo_id == entrada.EquipoId))
                return BadRequest("El equipo no existe.");

            var m = new bio_mantenimiento
            {
                equipo_id = entrada.EquipoId,
                tipo = entrada.Tipo,
                fecha_programada = entrada.FechaProgramada.Date,
                estado = "Programado",
                tecnico_responsable = entrada.TecnicoResponsable,
                proveedor = entrada.Proveedor,
                descripcion = entrada.Descripcion,
                frecuencia_meses = entrada.FrecuenciaMeses,
                actualizado_en = DateTime.Now,
                creado_en = DateTime.Now
            };
            db.bio_mantenimiento.Add(m);
            db.SaveChanges();

            var v = db.vw_bio_mantenimiento.AsNoTracking().First(x => x.mantenimiento_id == m.mantenimiento_id);
            return Ok(Mapear(v));
        }

        /// <summary>
        /// POST api/biomedico-mantenimiento/{id}/realizado — marca como
        /// completado y, si tiene FrecuenciaMeses, agenda automáticamente el
        /// siguiente preventivo esa cantidad de meses después.
        /// </summary>
        [HttpPost]
        [Route("{id:int}/realizado")]
        [ResponseType(typeof(BioMantenimientoDto))]
        public IHttpActionResult MarcarRealizado(int id, BioMantenimientoRealizadoDto entrada)
        {
            var m = db.bio_mantenimiento.FirstOrDefault(x => x.mantenimiento_id == id);
            if (m == null) return NotFound();

            var fechaRealizada = (entrada?.FechaRealizada ?? DateTime.Today).Date;
            m.estado = "Realizado";
            m.fecha_realizada = fechaRealizada;
            if (entrada != null)
            {
                if (!string.IsNullOrWhiteSpace(entrada.TecnicoResponsable)) m.tecnico_responsable = entrada.TecnicoResponsable;
                if (!string.IsNullOrWhiteSpace(entrada.Descripcion)) m.descripcion = entrada.Descripcion;
            }
            m.actualizado_en = DateTime.Now;

            if (m.frecuencia_meses.HasValue && m.frecuencia_meses.Value > 0 && m.tipo == "Preventivo")
            {
                db.bio_mantenimiento.Add(new bio_mantenimiento
                {
                    equipo_id = m.equipo_id,
                    tipo = "Preventivo",
                    fecha_programada = fechaRealizada.AddMonths(m.frecuencia_meses.Value),
                    estado = "Programado",
                    tecnico_responsable = m.tecnico_responsable,
                    proveedor = m.proveedor,
                    frecuencia_meses = m.frecuencia_meses,
                    actualizado_en = DateTime.Now,
                    creado_en = DateTime.Now
                });
            }

            db.SaveChanges();

            var v = db.vw_bio_mantenimiento.AsNoTracking().First(x => x.mantenimiento_id == id);
            return Ok(Mapear(v));
        }

        [HttpPost]
        [Route("{id:int}/cancelar")]
        [ResponseType(typeof(BioMantenimientoDto))]
        public IHttpActionResult Cancelar(int id)
        {
            var m = db.bio_mantenimiento.FirstOrDefault(x => x.mantenimiento_id == id);
            if (m == null) return NotFound();

            m.estado = "Cancelado";
            m.actualizado_en = DateTime.Now;
            db.SaveChanges();

            var v = db.vw_bio_mantenimiento.AsNoTracking().First(x => x.mantenimiento_id == id);
            return Ok(Mapear(v));
        }

        [HttpPost]
        [Route("{id:int}/frecuencia")]
        [ResponseType(typeof(BioMantenimientoDto))]
        public IHttpActionResult ActualizarFrecuencia(int id, BioFrecuenciaDto entrada)
        {
            if (entrada == null || entrada.FrecuenciaMeses <= 0)
                return BadRequest("La frecuencia debe ser mayor a 0.");

            var m = db.bio_mantenimiento.FirstOrDefault(x => x.mantenimiento_id == id);
            if (m == null) return NotFound();

            m.frecuencia_meses = entrada.FrecuenciaMeses;
            m.actualizado_en = DateTime.Now;
            db.SaveChanges();

            var v = db.vw_bio_mantenimiento.AsNoTracking().First(x => x.mantenimiento_id == id);
            return Ok(Mapear(v));
        }

        /// <summary>Programado con fecha ya pasada -&gt; Vencido, para que el dashboard y el calendario lo reflejen sin depender de un job aparte.</summary>
        private void MarcarVencidos()
        {
            var hoy = DateTime.Today;
            var vencidos = db.bio_mantenimiento.Where(m => m.estado == "Programado" && m.fecha_programada < hoy).ToList();
            if (vencidos.Count == 0) return;
            foreach (var m in vencidos) m.estado = "Vencido";
            db.SaveChanges();
        }

        private static BioMantenimientoDto Mapear(vw_bio_mantenimiento v) => new BioMantenimientoDto
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
        };

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
