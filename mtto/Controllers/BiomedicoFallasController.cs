using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/biomedico-fallas")]
    [RequiereArea("biomedico")]
    public class BiomedicoFallasController : ApiController
    {
        private readonly Model1 db = new Model1();

        private static readonly Regex PalabraRegex = new Regex(@"[a-zA-Záéíóúñ0-9]{3,}", RegexOptions.Compiled);

        [HttpGet]
        [Route("equipo/{equipoId:int}")]
        [ResponseType(typeof(List<BioFallaDto>))]
        public IHttpActionResult Listar(int equipoId)
        {
            var datos = db.bio_falla.AsNoTracking()
                .Where(f => f.equipo_id == equipoId)
                .OrderByDescending(f => f.fecha)
                .ToList()
                .Select(Mapear)
                .ToList();
            return Ok(datos);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(BioFallaDto))]
        public IHttpActionResult Reportar(BioFallaInputDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");
            if (string.IsNullOrWhiteSpace(entrada.Sintoma)) return BadRequest("El síntoma es obligatorio.");
            if (!db.bio_equipo.Any(e => e.equipo_id == entrada.EquipoId)) return BadRequest("El equipo no existe.");

            var f = new bio_falla
            {
                equipo_id = entrada.EquipoId,
                fecha = DateTime.Now,
                sintoma = entrada.Sintoma.Trim(),
                tecnico = entrada.Tecnico,
                estado = "Abierta",
                actualizado_en = DateTime.Now,
                creado_en = DateTime.Now
            };
            db.bio_falla.Add(f);
            db.SaveChanges();

            return Ok(Mapear(f));
        }

        /// <summary>POST api/biomedico-fallas/{id}/resolver — captura causa/solución; queda disponible para futuras sugerencias en este equipo y en otros del mismo modelo.</summary>
        [HttpPost]
        [Route("{id:int}/resolver")]
        [ResponseType(typeof(BioFallaDto))]
        public IHttpActionResult Resolver(int id, BioFallaResolverDto entrada)
        {
            if (entrada == null || string.IsNullOrWhiteSpace(entrada.Solucion))
                return BadRequest("La solución es obligatoria para resolver la falla.");

            var f = db.bio_falla.FirstOrDefault(x => x.falla_id == id);
            if (f == null) return NotFound();

            f.causa = entrada.Causa;
            f.solucion = entrada.Solucion;
            f.estado = "Resuelta";
            f.actualizado_en = DateTime.Now;
            db.SaveChanges();

            return Ok(Mapear(f));
        }

        /// <summary>
        /// GET api/biomedico-fallas/sugerencias?equipoId=&amp;sintoma= — combina
        /// fallas ya resueltas (de este equipo y de otros del mismo modelo) y
        /// fragmentos del manual con palabras clave del síntoma. Búsqueda local
        /// por coincidencia de palabras, sin IA ni conexión externa.
        /// </summary>
        [HttpGet]
        [Route("sugerencias")]
        [ResponseType(typeof(BioSugerenciasResultadoDto))]
        public IHttpActionResult Sugerencias(int equipoId, string sintoma)
        {
            if (string.IsNullOrWhiteSpace(sintoma))
                return Ok(new BioSugerenciasResultadoDto { Sugerencias = new List<BioSugerenciaDto>() });

            var palabras = PalabraRegex.Matches(sintoma.ToLowerInvariant())
                .Cast<Match>().Select(m => m.Value).Distinct().ToList();
            if (palabras.Count == 0)
                return Ok(new BioSugerenciasResultadoDto { Sugerencias = new List<BioSugerenciaDto>() });

            var equipo = db.bio_equipo.AsNoTracking().FirstOrDefault(e => e.equipo_id == equipoId);
            if (equipo == null) return NotFound();

            var equipoIds = new HashSet<int> { equipoId };
            if (!string.IsNullOrWhiteSpace(equipo.marca) && !string.IsNullOrWhiteSpace(equipo.modelo))
            {
                var similares = db.bio_equipo.AsNoTracking()
                    .Where(e => e.equipo_id != equipoId && e.marca == equipo.marca && e.modelo == equipo.modelo)
                    .Select(e => e.equipo_id).ToList();
                foreach (var id in similares) equipoIds.Add(id);
            }

            var resultado = new List<BioSugerenciaDto>();

            // --- Fallas resueltas ---
            var fallas = db.bio_falla.AsNoTracking()
                .Where(f => equipoIds.Contains(f.equipo_id) && f.estado == "Resuelta")
                .Select(f => new { f.falla_id, f.equipo_id, f.sintoma, f.causa, f.solucion, f.fecha, EquipoNombre = f.bio_equipo.nombre })
                .ToList();

            foreach (var f in fallas)
            {
                var texto = ((f.sintoma ?? "") + " " + (f.causa ?? "") + " " + (f.solucion ?? "")).ToLowerInvariant();
                var relevancia = palabras.Count(p => texto.Contains(p));
                if (relevancia == 0) continue;

                resultado.Add(new BioSugerenciaDto
                {
                    Origen = "falla",
                    FallaId = f.falla_id,
                    EquipoNombre = f.EquipoNombre,
                    MismoEquipo = f.equipo_id == equipoId,
                    Titulo = f.sintoma,
                    Fragmento = null,
                    Causa = f.causa,
                    Solucion = f.solucion,
                    Fecha = f.fecha,
                    Relevancia = relevancia
                });
            }

            // --- Fragmentos de manuales ---
            var manuales = db.bio_manual.AsNoTracking()
                .Where(m => equipoIds.Contains(m.equipo_id) && m.texto_extraido != null)
                .Select(m => new { m.manual_id, m.equipo_id, m.nombre_archivo, m.texto_extraido, m.subido_en, EquipoNombre = m.bio_equipo.nombre })
                .ToList();

            foreach (var m in manuales)
            {
                var mejor = MejorFragmento(m.texto_extraido, palabras);
                if (mejor == null) continue;

                resultado.Add(new BioSugerenciaDto
                {
                    Origen = "manual",
                    ManualId = m.manual_id,
                    EquipoNombre = m.EquipoNombre,
                    MismoEquipo = m.equipo_id == equipoId,
                    Titulo = m.nombre_archivo,
                    Fragmento = mejor.Item1,
                    Fecha = m.subido_en,
                    Relevancia = mejor.Item2
                });
            }

            var top = resultado
                .OrderByDescending(r => r.MismoEquipo)
                .ThenByDescending(r => r.Relevancia)
                .ThenByDescending(r => r.Fecha)
                .Take(15)
                .ToList();

            return Ok(new BioSugerenciasResultadoDto { Sugerencias = top });
        }

        /// <summary>Parte el texto en bloques y regresa el que más palabras clave contiene, con contexto alrededor.</summary>
        private static Tuple<string, int> MejorFragmento(string texto, List<string> palabras)
        {
            const int tamanoBloque = 400;
            var mejorScore = 0;
            string mejorBloque = null;

            var textoLower = texto.ToLowerInvariant();
            for (var i = 0; i < texto.Length; i += tamanoBloque)
            {
                var largo = Math.Min(tamanoBloque, texto.Length - i);
                var bloqueLower = textoLower.Substring(i, largo);
                var score = palabras.Count(p => bloqueLower.Contains(p));
                if (score > mejorScore)
                {
                    mejorScore = score;
                    mejorBloque = texto.Substring(i, largo).Trim();
                }
            }

            return mejorScore == 0 ? null : Tuple.Create(mejorBloque, mejorScore);
        }

        private static BioFallaDto Mapear(bio_falla f) => new BioFallaDto
        {
            Id = f.falla_id,
            EquipoId = f.equipo_id,
            Fecha = f.fecha,
            Sintoma = f.sintoma,
            Causa = f.causa,
            Solucion = f.solucion,
            Tecnico = f.tecnico,
            Estado = f.estado
        };

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
