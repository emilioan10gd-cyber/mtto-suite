using System;
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
using OfficeOpenXml;

namespace mtto.Controllers
{
    /// <summary>
    /// Bitácora de entradas y salidas.
    /// </summary>
    [RoutePrefix("api/movimientos")]
    public class MovimientosController : ApiController
    {
        private readonly Model1 db = new Model1();

        /// <summary>
        /// GET api/movimientos?desde=2026-07-01&amp;hasta=2026-07-31&amp;articulo=MT-0028&amp;tipo=Salida
        /// </summary>
        [HttpGet, Route("")]
        [ResponseType(typeof(PaginaDto<MovimientoDto>))]
        public IHttpActionResult Listar(
            DateTime? desde = null,
            DateTime? hasta = null,
            string articulo = null,
            string tipo = null,
            string almacen = null,
            int pagina = 1,
            int tamano = 50)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 500) tamano = 50;

            var consulta = AplicarFiltros(db.vw_mtto_movimientos.AsNoTracking(), desde, hasta, articulo, tipo, almacen);

            var total = consulta.Count();

            var datos = consulta
                .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.movimiento_id)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToList()
                .Select(Mapear)
                .ToList();

            return Ok(new PaginaDto<MovimientoDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling(total / (double)tamano),
                Datos = datos
            });
        }

        /// <summary>GET api/movimientos/12</summary>
        [HttpGet, Route("{id:int}")]
        [ResponseType(typeof(MovimientoDto))]
        public IHttpActionResult Obtener(int id)
        {
            var m = db.vw_mtto_movimientos.AsNoTracking().FirstOrDefault(x => x.movimiento_id == id);
            if (m == null) return NotFound();
            return Ok(Mapear(m));
        }

        private static IQueryable<vw_mtto_movimientos> AplicarFiltros(
            IQueryable<vw_mtto_movimientos> consulta, DateTime? desde, DateTime? hasta, string articulo, string tipo, string almacen)
        {
            if (desde.HasValue)
                consulta = consulta.Where(m => m.Fecha >= desde.Value);
            if (hasta.HasValue)
            {
                // 'hasta' se interpreta como día completo, no como medianoche.
                var fin = hasta.Value.Date.AddDays(1);
                consulta = consulta.Where(m => m.Fecha < fin);
            }
            if (!string.IsNullOrWhiteSpace(articulo))
                consulta = consulta.Where(m => m.ID_Articulo == articulo);
            if (!string.IsNullOrWhiteSpace(tipo))
                consulta = consulta.Where(m => m.Tipo_de_Movimiento == tipo);
            if (!string.IsNullOrWhiteSpace(almacen))
                consulta = consulta.Where(m => m.Almacen == almacen);
            return consulta;
        }

        /// <summary>
        /// GET api/movimientos/exportar — mismos filtros que Listar, sin paginar.
        /// Reproduce el formato de la hoja "Entradas y Salidas" del Excel original
        /// (MTTO_COMPLETE_DASHBOARD.xlsx): banda verde, mismas columnas y orden
        /// cronológico ascendente (como una bitácora que se va llenando).
        /// </summary>
        [HttpGet, Route("exportar")]
        public HttpResponseMessage Exportar(
            DateTime? desde = null, DateTime? hasta = null, string articulo = null, string tipo = null, string almacen = null)
        {
            var movimientos = AplicarFiltros(db.vw_mtto_movimientos.AsNoTracking(), desde, hasta, articulo, tipo, almacen)
                .OrderBy(m => m.Fecha).ThenBy(m => m.movimiento_id)
                .ToList();

            using (var paquete = new ExcelPackage())
            {
                var columnas = new (string, double)[]
                {
                    ("ID Movimiento", 14), ("Fecha", 13), ("Tipo de Movimiento", 15), ("ID Artículo", 14),
                    ("Nombre del Artículo", 30), ("Cantidad", 12), ("Almacén", 18),
                    ("Responsable", 16), ("Observaciones", 30)
                };

                var hoja = ExcelExportHelper.CrearHojaConEncabezado(
                    paquete, "Entradas y Salidas", "ENTRADAS Y SALIDAS", subtitulo: null, colorHex: "#548235", columnas: columnas);

                var fila = 5;
                foreach (var m in movimientos)
                {
                    hoja.Cells[fila, 2].Value = m.ID_Movimiento;
                    hoja.Cells[fila, 3].Value = m.Fecha;
                    hoja.Cells[fila, 4].Value = m.Tipo_de_Movimiento;
                    hoja.Cells[fila, 5].Value = m.ID_Articulo;
                    hoja.Cells[fila, 6].Value = m.Nombre_del_Articulo;
                    hoja.Cells[fila, 7].Value = m.Cantidad;
                    hoja.Cells[fila, 8].Value = m.Almacen;
                    hoja.Cells[fila, 9].Value = m.Responsable;
                    hoja.Cells[fila, 10].Value = m.Observaciones;

                    hoja.Cells[fila, 3].Style.Numberformat.Format = "mm-dd-yy";
                    hoja.Cells[fila, 7].Style.Numberformat.Format = "#,##0";

                    ExcelExportHelper.EstiloFilaDatos(hoja.Cells[fila, 2, fila, 10]);
                    fila++;
                }

                var bytes = paquete.GetAsByteArray();
                var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                respuesta.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                respuesta.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"Movimientos_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };
                return respuesta;
            }
        }

        /// <summary>
        /// Registra un movimiento. POST api/movimientos
        ///
        /// Va por usp_mtto_registrar_movimiento, que en una sola transacción
        /// escribe la bitácora Y ajusta mtto_articulo.stock_actual. Insertar
        /// directo en la tabla dejaría el stock sin actualizar.
        /// </summary>
        [HttpPost, Route("")]
        [ResponseType(typeof(MovimientoResultadoDto))]
        public IHttpActionResult Registrar(MovimientoInputDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");
            if (string.IsNullOrWhiteSpace(entrada.CodigoArticulo)) return BadRequest("El código del artículo es obligatorio.");
            if (string.IsNullOrWhiteSpace(entrada.Tipo)) return BadRequest("El tipo de movimiento es obligatorio.");
            if (entrada.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a cero.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var fila = db.Database.SqlQuery<FilaSpMovimiento>(
                    "EXEC dbo.usp_mtto_registrar_movimiento " +
                    "@codigo_articulo, @tipo_movimiento, @cantidad, @responsable, " +
                    "@observaciones, @almacen_destino, @fecha",
                    new SqlParameter("@codigo_articulo", entrada.CodigoArticulo),
                    new SqlParameter("@tipo_movimiento", entrada.Tipo),
                    new SqlParameter("@cantidad", entrada.Cantidad),
                    new SqlParameter("@responsable", (object)entrada.Responsable ?? DBNull.Value),
                    new SqlParameter("@observaciones", (object)entrada.Observaciones ?? DBNull.Value),
                    new SqlParameter("@almacen_destino", (object)entrada.AlmacenDestino ?? DBNull.Value),
                    new SqlParameter("@fecha", (object)entrada.Fecha ?? DBNull.Value))
                    .FirstOrDefault();

                if (fila == null)
                    return InternalServerError(new Exception("El procedimiento no devolvió resultado."));

                return Content(HttpStatusCode.Created, new MovimientoResultadoDto
                {
                    Id = fila.movimiento_id,
                    Folio = fila.folio,
                    StockPrevio = fila.stock_previo,
                    StockResultante = fila.stock_resultante
                });
            }
            catch (SqlException ex)
            {
                // 50010-50015 son las validaciones de negocio del SP (cantidad
                // inválida, artículo inexistente, existencia insuficiente...).
                // Son errores del cliente, no fallas del servidor.
                if (ex.Number >= 50010 && ex.Number <= 50015)
                    return Content(HttpStatusCode.BadRequest, new { error = ex.Message });
                throw;
            }
        }

        // NOTA: sin PUT ni DELETE a propósito. La bitácora es el registro de
        // auditoría del almacén: editarla o borrarla dejaría el stock actual sin
        // respaldo documental. Un movimiento equivocado se corrige registrando
        // el ajuste contrario, igual que en contabilidad.

        /// <summary>
        /// Espejo exacto del SELECT final de usp_mtto_registrar_movimiento.
        /// SqlQuery&lt;T&gt; empata por nombre de columna, así que estos nombres
        /// tienen que ser los del SP, no los del DTO público.
        /// </summary>
        private class FilaSpMovimiento
        {
            public int movimiento_id { get; set; }
            public string folio { get; set; }
            public decimal stock_previo { get; set; }
            public decimal stock_resultante { get; set; }
        }

        private static MovimientoDto Mapear(vw_mtto_movimientos m)
        {
            return new MovimientoDto
            {
                Id = m.movimiento_id,
                Folio = m.ID_Movimiento,
                Fecha = m.Fecha,
                Tipo = m.Tipo_de_Movimiento,
                CodigoArticulo = m.ID_Articulo,
                NombreArticulo = m.Nombre_del_Articulo,
                Cantidad = m.Cantidad,
                Almacen = m.Almacen,
                AlmacenDestino = m.Almacen_Destino,
                Responsable = m.Responsable,
                Observaciones = m.Observaciones,
                StockPrevio = m.stock_previo,
                StockResultante = m.stock_resultante,
                Signo = m.signo
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
