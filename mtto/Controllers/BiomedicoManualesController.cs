using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/biomedico-manuales")]
    [RequiereArea("biomedico")]
    public class BiomedicoManualesController : ApiController
    {
        private readonly Model1 db = new Model1();

        private static readonly string[] ExtensionesPermitidas = { ".pdf", ".doc", ".docx" };
        private const long TamanoMaximoBytes = 25 * 1024 * 1024; // 25 MB

        [HttpGet]
        [Route("equipo/{equipoId:int}")]
        [ResponseType(typeof(System.Collections.Generic.List<BioManualDto>))]
        public IHttpActionResult Listar(int equipoId)
        {
            var datos = db.bio_manual.AsNoTracking()
                .Where(m => m.equipo_id == equipoId)
                .OrderByDescending(m => m.subido_en)
                .ToList()
                .Select(Mapear)
                .ToList();
            return Ok(datos);
        }

        /// <summary>
        /// POST api/biomedico-manuales/equipo/{equipoId} — multipart/form-data
        /// con un campo "archivo". Guarda el archivo en disco y, si es PDF,
        /// extrae su texto para que quede disponible a Sugerencias.
        /// </summary>
        [HttpPost]
        [Route("equipo/{equipoId:int}")]
        [ResponseType(typeof(BioManualDto))]
        public async Task<IHttpActionResult> Subir(int equipoId)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Se esperaba multipart/form-data.");

            if (!db.bio_equipo.Any(e => e.equipo_id == equipoId))
                return NotFound();

            var carpeta = RutaCarpeta(equipoId);
            Directory.CreateDirectory(carpeta);

            var provider = new MultipartFileStreamProvider(carpeta);
            await Request.Content.ReadAsMultipartAsync(provider);

            var archivo = provider.FileData.FirstOrDefault();
            if (archivo == null) return BadRequest("No se recibió ningún archivo.");

            var nombreOriginal = archivo.Headers.ContentDisposition.FileName?.Trim('"') ?? "manual";
            var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                TryDelete(archivo.LocalFileName);
                return Content(HttpStatusCode.BadRequest, new { error = "Solo se aceptan archivos PDF, DOC o DOCX." });
            }

            var info = new FileInfo(archivo.LocalFileName);
            if (info.Length > TamanoMaximoBytes)
            {
                TryDelete(archivo.LocalFileName);
                return Content(HttpStatusCode.BadRequest, new { error = "El archivo no puede pasar de 25 MB." });
            }

            string textoExtraido = null;
            if (extension == ".pdf")
            {
                try { textoExtraido = PdfTextExtractor.ExtraerTexto(File.ReadAllBytes(archivo.LocalFileName)); }
                catch { /* extracción best-effort: si falla, el manual se guarda igual sin texto buscable */ }
            }

            var manual = new bio_manual
            {
                equipo_id = equipoId,
                nombre_archivo = nombreOriginal,
                ruta_archivo = archivo.LocalFileName,
                tipo_archivo = extension.TrimStart('.'),
                tamano_bytes = info.Length,
                texto_extraido = textoExtraido,
                subido_en = DateTime.Now
            };
            db.bio_manual.Add(manual);
            db.SaveChanges();

            return Ok(Mapear(manual));
        }

        [HttpGet]
        [Route("{id:int}/descargar")]
        public IHttpActionResult Descargar(int id)
        {
            var manual = db.bio_manual.AsNoTracking().FirstOrDefault(m => m.manual_id == id);
            if (manual == null || !File.Exists(manual.ruta_archivo)) return NotFound();

            var bytes = File.ReadAllBytes(manual.ruta_archivo);
            var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            respuesta.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            respuesta.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = manual.nombre_archivo
            };
            return ResponseMessage(respuesta);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Eliminar(int id)
        {
            var manual = db.bio_manual.FirstOrDefault(m => m.manual_id == id);
            if (manual == null) return NotFound();

            TryDelete(manual.ruta_archivo);
            db.bio_manual.Remove(manual);
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static string RutaCarpeta(int equipoId)
        {
            var baseDir = HostingEnvironment.IsHosted
                ? HostingEnvironment.MapPath("~/App_Data/biomedico-manuales")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "biomedico-manuales");
            return Path.Combine(baseDir, equipoId.ToString());
        }

        private static void TryDelete(string ruta)
        {
            try { if (!string.IsNullOrEmpty(ruta) && File.Exists(ruta)) File.Delete(ruta); }
            catch { /* limpieza best-effort */ }
        }

        private static BioManualDto Mapear(bio_manual m) => new BioManualDto
        {
            Id = m.manual_id,
            EquipoId = m.equipo_id,
            NombreArchivo = m.nombre_archivo,
            TipoArchivo = m.tipo_archivo,
            TamanoBytes = m.tamano_bytes,
            SubidoEn = m.subido_en
        };

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
