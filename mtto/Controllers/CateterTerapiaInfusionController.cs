using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using OfficeOpenXml;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/cateter-terapia-infusion")]
    [RequiereArea("cateter")]
    public class CateterTerapiaInfusionController : ApiController
    {
        private readonly Model1 db = new Model1();

        private string UsuarioActual()
        {
            object usuario;
            Request.Properties.TryGetValue("UsuarioActual", out usuario);
            var cuenta = usuario as app_usuario;
            return cuenta?.nombre_usuario;
        }

        // La captura sólo puede caer en el mes en curso: nada de adelantar
        // meses futuros ni "corregir" retroactivamente meses ya cerrados.
        // Devuelve null si la fecha es válida, o el mensaje de error si no.
        private static string ValidarFechaMesActual(DateTime fecha)
        {
            var hoy = DateTime.Now;
            if (fecha.Year != hoy.Year || fecha.Month != hoy.Month)
            {
                return $"Sólo se puede capturar en el mes en curso ({hoy:MMMM yyyy}). " +
                       "No se permite registrar en meses pasados ni futuros.";
            }
            if (fecha.Date > hoy.Date)
            {
                return "No se puede capturar una fecha futura.";
            }
            return null;
        }

        // ---------------------------------------------------------------
        // Registro de Sitios Anatómicos y Calibres
        // ---------------------------------------------------------------

        [HttpPost]
        [Route("registros")]
        [ResponseType(typeof(TerapiaRegistroDto))]
        public IHttpActionResult GuardarRegistro(TerapiaRegistroInputDto entrada)
        {
            if (entrada == null)
            {
                return BadRequest("Cuerpo de la petición vacío.");
            }

            int[] cantidades =
            {
                entrada.Msd, entrada.Msi, entrada.Mii,
                entrada.Calibre14, entrada.Calibre16, entrada.Calibre17, entrada.Calibre18,
                entrada.Calibre19, entrada.Calibre20, entrada.Calibre22, entrada.Calibre24
            };
            if (cantidades.Any(c => c < 0))
            {
                return BadRequest("Las cantidades no pueden ser negativas.");
            }
            if (cantidades.All(c => c == 0))
            {
                return BadRequest("Captura al menos una cantidad mayor a cero.");
            }

            DateTime fecha = entrada.Fecha ?? DateTime.Now;
            string errorFecha = ValidarFechaMesActual(fecha);
            if (errorFecha != null)
            {
                return BadRequest(errorFecha);
            }

            var registro = new cateter_terapia_registro
            {
                fecha = fecha,
                msd = entrada.Msd,
                msi = entrada.Msi,
                mii = entrada.Mii,
                calibre_14 = entrada.Calibre14,
                calibre_16 = entrada.Calibre16,
                calibre_17 = entrada.Calibre17,
                calibre_18 = entrada.Calibre18,
                calibre_19 = entrada.Calibre19,
                calibre_20 = entrada.Calibre20,
                calibre_22 = entrada.Calibre22,
                calibre_24 = entrada.Calibre24,
                creado_por = UsuarioActual(),
                creado_en = DateTime.Now
            };

            db.cateter_terapia_registro.Add(registro);
            db.SaveChanges();

            return Ok(MapearRegistro(registro));
        }

        [HttpGet]
        [Route("registros")]
        [ResponseType(typeof(PaginaDto<TerapiaRegistroDto>))]
        public IHttpActionResult ListarRegistros(int? anio = null, int? mes = null, int pagina = 1, int tamano = 50)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 500) tamano = 50;

            IQueryable<cateter_terapia_registro> consulta = AplicarFiltroMes(
                db.cateter_terapia_registro.AsNoTracking(), anio, mes)
                .OrderByDescending(r => r.fecha)
                .ThenByDescending(r => r.registro_id);

            int total = consulta.Count();
            List<TerapiaRegistroDto> datos = consulta
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToList()
                .Select(MapearRegistro)
                .ToList();

            return Ok(new PaginaDto<TerapiaRegistroDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)total / tamano),
                Datos = datos
            });
        }

        [HttpGet]
        [Route("registros/exportar")]
        public HttpResponseMessage ExportarRegistros(int? anio = null, int? mes = null)
        {
            List<cateter_terapia_registro> lista = AplicarFiltroMes(db.cateter_terapia_registro.AsNoTracking(), anio, mes)
                .OrderBy(r => r.fecha)
                .ThenBy(r => r.registro_id)
                .ToList();

            using (var paquete = new ExcelPackage())
            {
                (string, double)[] columnas =
                {
                    ("Fecha", 14), ("MSD", 8), ("MSI", 8), ("MII", 8), ("Tot. sitios", 10),
                    ("14", 6), ("16", 6), ("17", 6), ("18", 6), ("19", 6), ("20", 6), ("22", 6), ("24", 6),
                    ("Tot. catéteres", 12), ("Registrado por", 18)
                };
                var hoja = ExcelExportHelper.CrearHojaConEncabezado(
                    paquete, "Sitios y Catéteres", "REGISTRO DE SITIOS Y CATÉTERES",
                    SubtituloPeriodo(anio, mes), "#1F3864", columnas);

                int fila = 5;
                foreach (var r in lista)
                {
                    hoja.Cells[fila, 2].Value = r.fecha;
                    hoja.Cells[fila, 2].Style.Numberformat.Format = "dd/mm/yyyy";
                    hoja.Cells[fila, 3].Value = r.msd;
                    hoja.Cells[fila, 4].Value = r.msi;
                    hoja.Cells[fila, 5].Value = r.mii;
                    hoja.Cells[fila, 6].Value = r.msd + r.msi + r.mii;
                    hoja.Cells[fila, 7].Value = r.calibre_14;
                    hoja.Cells[fila, 8].Value = r.calibre_16;
                    hoja.Cells[fila, 9].Value = r.calibre_17;
                    hoja.Cells[fila, 10].Value = r.calibre_18;
                    hoja.Cells[fila, 11].Value = r.calibre_19;
                    hoja.Cells[fila, 12].Value = r.calibre_20;
                    hoja.Cells[fila, 13].Value = r.calibre_22;
                    hoja.Cells[fila, 14].Value = r.calibre_24;
                    hoja.Cells[fila, 15].Value = r.calibre_14 + r.calibre_16 + r.calibre_17 + r.calibre_18
                        + r.calibre_19 + r.calibre_20 + r.calibre_22 + r.calibre_24;
                    hoja.Cells[fila, 16].Value = r.creado_por;
                    ExcelExportHelper.EstiloFilaDatos(hoja.Cells[fila, 2, fila, 16]);
                    fila++;
                }

                return ArchivoExcel(paquete, $"Terapia_Sitios_Cateteres_{DateTime.Now:yyyy-MM-dd}.xlsx");
            }
        }

        [HttpDelete]
        [Route("registros/{id:int}")]
        public IHttpActionResult EliminarRegistro(int id)
        {
            var registro = db.cateter_terapia_registro.FirstOrDefault(r => r.registro_id == id);
            if (registro == null)
            {
                return NotFound();
            }

            db.cateter_terapia_registro.Remove(registro);
            db.SaveChanges();

            return Ok();
        }

        private static IQueryable<cateter_terapia_registro> AplicarFiltroMes(
            IQueryable<cateter_terapia_registro> consulta, int? anio, int? mes)
        {
            if (anio.HasValue) consulta = consulta.Where(r => r.fecha.Year == anio.Value);
            if (mes.HasValue) consulta = consulta.Where(r => r.fecha.Month == mes.Value);
            return consulta;
        }

        private static TerapiaRegistroDto MapearRegistro(cateter_terapia_registro r)
        {
            return new TerapiaRegistroDto
            {
                Id = r.registro_id,
                Fecha = r.fecha,
                Msd = r.msd,
                Msi = r.msi,
                Mii = r.mii,
                Calibre14 = r.calibre_14,
                Calibre16 = r.calibre_16,
                Calibre17 = r.calibre_17,
                Calibre18 = r.calibre_18,
                Calibre19 = r.calibre_19,
                Calibre20 = r.calibre_20,
                Calibre22 = r.calibre_22,
                Calibre24 = r.calibre_24,
                TotalSitios = r.msd + r.msi + r.mii,
                TotalCateteres = r.calibre_14 + r.calibre_16 + r.calibre_17 + r.calibre_18
                    + r.calibre_19 + r.calibre_20 + r.calibre_22 + r.calibre_24,
                CreadoPor = r.creado_por
            };
        }

        // ---------------------------------------------------------------
        // Eventos diarios
        // ---------------------------------------------------------------

        [HttpGet]
        [Route("eventos")]
        [ResponseType(typeof(PaginaDto<TerapiaEventoDto>))]
        public IHttpActionResult ListarEventos(int? anio = null, int? mes = null, int pagina = 1, int tamano = 200)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 500) tamano = 200;

            IQueryable<cateter_terapia_evento> consulta = AplicarFiltroMes(
                db.cateter_terapia_evento.AsNoTracking(), anio, mes)
                .OrderByDescending(e => e.fecha)
                .ThenByDescending(e => e.evento_id);

            int total = consulta.Count();
            List<TerapiaEventoDto> datos = consulta
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToList()
                .Select(MapearEvento)
                .ToList();

            return Ok(new PaginaDto<TerapiaEventoDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)total / tamano),
                Datos = datos
            });
        }

        [HttpGet]
        [Route("eventos/exportar")]
        public HttpResponseMessage ExportarEventos(int? anio = null, int? mes = null)
        {
            List<cateter_terapia_evento> lista = AplicarFiltroMes(db.cateter_terapia_evento.AsNoTracking(), anio, mes)
                .OrderBy(e => e.fecha)
                .ThenBy(e => e.evento_id)
                .ToList();

            using (var paquete = new ExcelPackage())
            {
                (string, double)[] columnas =
                {
                    ("Fecha", 14), ("Total eventos", 14), ("Catéteres colocados", 16),
                    ("Personas que instalaron", 18), ("Total intervenciones", 16), ("Registrado por", 18)
                };
                var hoja = ExcelExportHelper.CrearHojaConEncabezado(
                    paquete, "Eventos Diarios", "REGISTRO DE EVENTOS DIARIOS",
                    SubtituloPeriodo(anio, mes), "#1F3864", columnas);

                int fila = 5;
                foreach (var e in lista)
                {
                    hoja.Cells[fila, 2].Value = e.fecha;
                    hoja.Cells[fila, 2].Style.Numberformat.Format = "dd/mm/yyyy";
                    hoja.Cells[fila, 3].Value = e.total_eventos;
                    hoja.Cells[fila, 4].Value = e.total_cateteres_colocados;
                    hoja.Cells[fila, 5].Value = e.personas_instalaron;
                    hoja.Cells[fila, 6].Value = e.total_intervenciones;
                    hoja.Cells[fila, 7].Value = e.creado_por;
                    ExcelExportHelper.EstiloFilaDatos(hoja.Cells[fila, 2, fila, 7]);
                    fila++;
                }

                return ArchivoExcel(paquete, $"Terapia_Eventos_Diarios_{DateTime.Now:yyyy-MM-dd}.xlsx");
            }
        }

        [HttpPost]
        [Route("eventos")]
        [ResponseType(typeof(TerapiaEventoDto))]
        public IHttpActionResult GuardarEvento(TerapiaEventoInputDto entrada)
        {
            var error = ValidarEvento(entrada);
            if (error != null)
            {
                return BadRequest(error);
            }

            string errorFecha = ValidarFechaMesActual(entrada.Fecha);
            if (errorFecha != null)
            {
                return BadRequest(errorFecha);
            }

            var evento = new cateter_terapia_evento
            {
                fecha = entrada.Fecha.Date,
                total_eventos = entrada.TotalEventos,
                total_cateteres_colocados = entrada.TotalCateterColocados,
                personas_instalaron = entrada.PersonasInstalaron,
                total_intervenciones = entrada.TotalIntervenciones,
                creado_por = UsuarioActual(),
                creado_en = DateTime.Now
            };

            db.cateter_terapia_evento.Add(evento);
            db.SaveChanges();

            return Ok(MapearEvento(evento));
        }

        [HttpDelete]
        [Route("eventos/{id:int}")]
        public IHttpActionResult EliminarEvento(int id)
        {
            var evento = db.cateter_terapia_evento.FirstOrDefault(e => e.evento_id == id);
            if (evento == null)
            {
                return NotFound();
            }

            db.cateter_terapia_evento.Remove(evento);
            db.SaveChanges();

            return Ok();
        }

        private static string ValidarEvento(TerapiaEventoInputDto entrada)
        {
            if (entrada == null)
            {
                return "Cuerpo de la petición vacío.";
            }
            if (entrada.TotalEventos < 0 || entrada.TotalCateterColocados < 0
                || entrada.PersonasInstalaron < 0 || entrada.TotalIntervenciones < 0)
            {
                return "Las cantidades no pueden ser negativas.";
            }
            return null;
        }

        private static IQueryable<cateter_terapia_evento> AplicarFiltroMes(
            IQueryable<cateter_terapia_evento> consulta, int? anio, int? mes)
        {
            if (anio.HasValue) consulta = consulta.Where(e => e.fecha.Year == anio.Value);
            if (mes.HasValue) consulta = consulta.Where(e => e.fecha.Month == mes.Value);
            return consulta;
        }

        private static TerapiaEventoDto MapearEvento(cateter_terapia_evento e)
        {
            return new TerapiaEventoDto
            {
                Id = e.evento_id,
                Fecha = e.fecha,
                TotalEventos = e.total_eventos,
                TotalCateterColocados = e.total_cateteres_colocados,
                PersonasInstalaron = e.personas_instalaron,
                TotalIntervenciones = e.total_intervenciones
            };
        }

        // ---------------------------------------------------------------
        // Reportes: series completas (sin paginar) para graficar tendencias.
        // Un tamaño de página fijo se quedaría corto según crece la captura
        // mes a mes; esto trae todo lo que haya, ya filtrado si se pide.
        // ---------------------------------------------------------------
        [HttpGet]
        [Route("reportes")]
        public IHttpActionResult ObtenerReportes(int? anio = null, int? mes = null)
        {
            List<TerapiaRegistroDto> registros = AplicarFiltroMes(db.cateter_terapia_registro.AsNoTracking(), anio, mes)
                .OrderBy(r => r.fecha)
                .ToList()
                .Select(MapearRegistro)
                .ToList();

            List<TerapiaEventoDto> eventos = AplicarFiltroMes(db.cateter_terapia_evento.AsNoTracking(), anio, mes)
                .OrderBy(e => e.fecha)
                .ToList()
                .Select(MapearEvento)
                .ToList();

            return Ok(new { registros, eventos });
        }

        private static string SubtituloPeriodo(int? anio, int? mes)
        {
            if (anio.HasValue && mes.HasValue)
            {
                var nombreMes = new DateTime(anio.Value, mes.Value, 1).ToString("MMMM yyyy",
                    System.Globalization.CultureInfo.GetCultureInfo("es-MX"));
                return $"Periodo: {nombreMes} — Generado {DateTime.Now:dd/MM/yyyy HH:mm}";
            }
            return $"Todos los periodos — Generado {DateTime.Now:dd/MM/yyyy HH:mm}";
        }

        private static HttpResponseMessage ArchivoExcel(ExcelPackage paquete, string nombreArchivo)
        {
            byte[] bytes = paquete.GetAsByteArray();
            var respuesta = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            respuesta.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            respuesta.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = nombreArchivo
            };
            return respuesta;
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
