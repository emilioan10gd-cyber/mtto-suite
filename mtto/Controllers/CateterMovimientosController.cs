using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
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
    [RoutePrefix("api/cateter-movimientos")]
    [RequiereArea("cateter")]
    public class CateterMovimientosController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(PaginaDto<CateterMovimientoDto>))]
        public IHttpActionResult Listar(DateTime? desde = null, DateTime? hasta = null, string clave = null, string tipo = null, string clasificacion = null, string responsable = null, string estado = null, int pagina = 1, int tamano = 50)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }
            if (tamano < 1 || tamano > 500)
            {
                tamano = 50;
            }
            IQueryable<vw_cateter_movimientos> source = AplicarFiltros(db.vw_cateter_movimientos.AsNoTracking(), desde, hasta, clave, tipo, clasificacion);
            if (!string.IsNullOrWhiteSpace(responsable))
            {
                source = source.Where(m => m.responsable != null && m.responsable.Contains(responsable));
            }

            // Left join contra las columnas de cancelación (misma tabla base,
            // mapeo aparte: ver cateter_movimiento_estado). Sin fila = Activo.
            var conEstado =
                from m in source
                join e in db.cateter_movimiento_estado on m.movimiento_id equals e.movimiento_id into estados
                from est in estados.DefaultIfEmpty()
                select new { Movimiento = m, Estado = est };

            if (!string.IsNullOrWhiteSpace(estado))
            {
                conEstado = conEstado.Where(x => (x.Estado != null ? x.Estado.estado : "Activo") == estado);
            }

            int num = conEstado.Count();
            List<CateterMovimientoDto> datos = conEstado
                .OrderByDescending(x => x.Movimiento.fecha)
                .ThenByDescending(x => x.Movimiento.movimiento_id)
                .Skip((pagina - 1) * tamano).Take(tamano).ToList()
                .Select(x => Mapear(x.Movimiento, x.Estado))
                .ToList();
            return Ok<PaginaDto<CateterMovimientoDto>>(new PaginaDto<CateterMovimientoDto>
            {
                Total = num,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)num / (double)tamano),
                Datos = datos
            });
        }

        private static IQueryable<vw_cateter_movimientos> AplicarFiltros(IQueryable<vw_cateter_movimientos> consulta, DateTime? desde, DateTime? hasta, string clave, string tipo, string clasificacion)
        {
            if (desde.HasValue)
            {
                consulta = consulta.Where(m => m.fecha >= ((DateTime?)desde).Value);
            }
            if (hasta.HasValue)
            {
                DateTime fin = hasta.Value.Date.AddDays(1.0);
                consulta = consulta.Where(m => m.fecha < fin);
            }
            if (!string.IsNullOrWhiteSpace(clave))
            {
                consulta = consulta.Where(m => m.clave == clave);
            }
            if (!string.IsNullOrWhiteSpace(tipo))
            {
                consulta = consulta.Where(m => m.tipo == tipo);
            }
            if (!string.IsNullOrWhiteSpace(clasificacion))
            {
                consulta = consulta.Where(m => m.clasificacion == clasificacion);
            }
            return consulta;
        }

        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(CateterMovimientoDto))]
        public IHttpActionResult Obtener(int id)
        {
            vw_cateter_movimientos vw_cateter_movimientos2 = (db.vw_cateter_movimientos.AsNoTracking()).FirstOrDefault(x => x.movimiento_id == id);
            if (vw_cateter_movimientos2 == null)
            {
                return NotFound();
            }
            return Ok<CateterMovimientoDto>(MapearConEstado(vw_cateter_movimientos2));
        }

        // -----------------------------------------------------------------
        // Cancelar un movimiento ya registrado.
        //
        // No se toca ninguna tabla de existencias a mano: se genera un
        // movimiento de reversión con el MISMO procedimiento ya probado
        // (usp_cateter_registrar_movimiento), con origen/destino invertidos
        // según el tipo. El movimiento original solo se marca como
        // "Cancelado" (columnas agregadas por
        // Cateter_060_TERAPIA_INFUSION_Y_CANCELACION.sql); nunca se borra,
        // así que la trazabilidad para auditoría queda completa.
        // -----------------------------------------------------------------
        [HttpPost]
        [Route("{id:int}/cancelar")]
        [ResponseType(typeof(CateterMovimientoDto))]
        public IHttpActionResult Cancelar(int id, CancelarMovimientoDto entrada)
        {
            vw_cateter_movimientos original = db.vw_cateter_movimientos.AsNoTracking().FirstOrDefault(m => m.movimiento_id == id);
            if (original == null)
            {
                return NotFound();
            }

            cateter_movimiento_estado estadoOriginal = db.cateter_movimiento_estado.FirstOrDefault(e => e.movimiento_id == id);
            if (estadoOriginal == null)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    error = "Faltan las columnas de cancelación en la base de datos. Corre Database/SQL/Cateter_060_TERAPIA_INFUSION_Y_CANCELACION.sql en este servidor antes de usar esta función."
                });
            }
            if (estadoOriginal.estado == "Cancelado")
            {
                return Content(HttpStatusCode.BadRequest, new { error = "Este movimiento ya estaba cancelado." });
            }
            if (estadoOriginal.es_reversion)
            {
                return Content(HttpStatusCode.BadRequest, new { error = "No se puede cancelar un movimiento que en sí mismo es una reversión." });
            }

            string tipoReversion;
            string origenReversion;
            string destinoReversion;
            switch (original.tipo)
            {
                case "Traslado a stock":
                    tipoReversion = "Retorno a almacén";
                    origenReversion = original.ubicacion_destino;
                    destinoReversion = original.ubicacion_origen;
                    break;
                case "Retorno a almacén":
                    tipoReversion = "Traslado a stock";
                    origenReversion = original.ubicacion_destino;
                    destinoReversion = original.ubicacion_origen;
                    break;
                case "Salida":
                case "Merma":
                case "Ajuste (-)":
                    tipoReversion = "Ajuste (+)";
                    origenReversion = null;
                    destinoReversion = original.ubicacion_origen;
                    break;
                case "Ajuste (+)":
                    tipoReversion = "Ajuste (-)";
                    origenReversion = original.ubicacion_destino;
                    destinoReversion = null;
                    break;
                default:
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        error = $"Los movimientos de tipo \"{original.tipo}\" no se pueden cancelar automáticamente. Contacta a soporte técnico."
                    });
            }

            string razon = string.IsNullOrWhiteSpace(entrada?.Razon) ? null : entrada.Razon.Trim();
            string responsableActual = UsuarioActual();
            string observacionesReversion = "Reversión por cancelación del movimiento " + original.folio + "." +
                (razon != null ? " Razón: " + razon : "");

            SqlParameter outId = new SqlParameter("@movimiento_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.usp_cateter_registrar_movimiento @lote_id, @tipo, @cantidad, @ubicacion_origen, @ubicacion_destino, @responsable, @observaciones, @referencia_uso, @fecha, @movimiento_id OUTPUT",
                    new object[10]
                    {
                        new SqlParameter("@lote_id", original.lote_id),
                        new SqlParameter("@tipo", tipoReversion),
                        new SqlParameter("@cantidad", original.cantidad),
                        new SqlParameter("@ubicacion_origen", ((object)origenReversion) ?? ((object)DBNull.Value)),
                        new SqlParameter("@ubicacion_destino", ((object)destinoReversion) ?? ((object)DBNull.Value)),
                        new SqlParameter("@responsable", ((object)responsableActual) ?? ((object)original.responsable) ?? ((object)DBNull.Value)),
                        new SqlParameter("@observaciones", observacionesReversion),
                        new SqlParameter("@referencia_uso", DBNull.Value),
                        new SqlParameter("@fecha", DBNull.Value),
                        outId
                    });
            }
            catch (Exception ex)
            {
                SqlException ex2 = SqlErrorHelper.ErrorDeValidacion(ex);
                if (ex2 != null)
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "No se pudo generar la reversión de existencias: " + ex2.Message });
                }
                throw;
            }

            int reversionId = (int)outId.Value;
            cateter_movimiento_estado estadoReversion = db.cateter_movimiento_estado.First(e => e.movimiento_id == reversionId);
            estadoReversion.es_reversion = true;
            estadoReversion.reversion_de_id = id;

            estadoOriginal.estado = "Cancelado";
            estadoOriginal.razon_cancelacion = razon;
            estadoOriginal.cancelado_en = DateTime.Now;
            estadoOriginal.cancelado_por = responsableActual;

            db.SaveChanges();

            vw_cateter_movimientos actualizado = db.vw_cateter_movimientos.AsNoTracking().First(m => m.movimiento_id == id);
            return Ok<CateterMovimientoDto>(Mapear(actualizado, estadoOriginal));
        }

        private string UsuarioActual()
        {
            object usuario;
            Request.Properties.TryGetValue("UsuarioActual", out usuario);
            var cuenta = usuario as app_usuario;
            return cuenta?.nombre_usuario;
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CateterMovimientoDto))]
        public IHttpActionResult Registrar(CateterMovimientoInputDto entrada)
        {
            if (entrada == null)
            {
                return BadRequest("Cuerpo de la petición vacío.");
            }
            if (entrada.LoteId <= 0)
            {
                return BadRequest("Falta el lote.");
            }
            if (string.IsNullOrWhiteSpace(entrada.Tipo))
            {
                return BadRequest("El tipo de movimiento es obligatorio.");
            }
            if (entrada.Cantidad <= 0m)
            {
                return BadRequest("La cantidad debe ser mayor a cero.");
            }
            SqlParameter sqlParameter = new SqlParameter("@movimiento_id", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            try
            {
                db.Database.ExecuteSqlCommand("EXEC dbo.usp_cateter_registrar_movimiento @lote_id, @tipo, @cantidad, @ubicacion_origen, @ubicacion_destino, @responsable, @observaciones, @referencia_uso, @fecha, @movimiento_id OUTPUT", new object[10]
                {
                    new SqlParameter("@lote_id", entrada.LoteId),
                    new SqlParameter("@tipo", entrada.Tipo),
                    new SqlParameter("@cantidad", entrada.Cantidad),
                    new SqlParameter("@ubicacion_origen", ((object)entrada.UbicacionOrigen) ?? ((object)DBNull.Value)),
                    new SqlParameter("@ubicacion_destino", ((object)entrada.UbicacionDestino) ?? ((object)DBNull.Value)),
                    new SqlParameter("@responsable", ((object)entrada.Responsable) ?? ((object)DBNull.Value)),
                    new SqlParameter("@observaciones", ((object)entrada.Observaciones) ?? ((object)DBNull.Value)),
                    new SqlParameter("@referencia_uso", ((object)entrada.ReferenciaUso) ?? ((object)DBNull.Value)),
                    new SqlParameter("@fecha", ((object)entrada.Fecha) ?? DBNull.Value),
                    sqlParameter
                });
            }
            catch (Exception ex)
            {
                SqlException ex2 = SqlErrorHelper.ErrorDeValidacion(ex);
                if (ex2 != null)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        error = ex2.Message
                    });
                }
                throw;
            }
            int movimientoId = (int)sqlParameter.Value;
            vw_cateter_movimientos vw_cateter_movimientos2 = (db.vw_cateter_movimientos.AsNoTracking()).FirstOrDefault(m => m.movimiento_id == movimientoId);
            if (vw_cateter_movimientos2 != null)
            {
                return Ok<CateterMovimientoDto>(MapearConEstado(vw_cateter_movimientos2));
            }
            return NotFound();
        }

        [HttpPost]
        [Route("surtir")]
        [ResponseType(typeof(CateterSurtidoResultadoDto))]
        public IHttpActionResult Surtir(CateterSurtidoInputDto entrada)
        {
            if (entrada == null)
            {
                return BadRequest("Cuerpo de la petición vacío.");
            }
            if (string.IsNullOrWhiteSpace(entrada.Clave))
            {
                return BadRequest("La clave es obligatoria.");
            }
            if (entrada.Cantidad <= 0m)
            {
                return BadRequest("La cantidad debe ser mayor a cero.");
            }
            SqlParameter[] values = new SqlParameter[8]
            {
                new SqlParameter("@clave", entrada.Clave),
                new SqlParameter("@cantidad", entrada.Cantidad),
                new SqlParameter("@tipo", entrada.Tipo ?? "Traslado a stock"),
                new SqlParameter("@ubicacion_origen", entrada.UbicacionOrigen ?? "Almacén"),
                new SqlParameter("@ubicacion_destino", entrada.UbicacionDestino ?? "Stock"),
                new SqlParameter("@responsable", ((object)entrada.Responsable) ?? ((object)DBNull.Value)),
                new SqlParameter("@observaciones", ((object)entrada.Observaciones) ?? ((object)DBNull.Value)),
                new SqlParameter("@referencia_uso", ((object)entrada.ReferenciaUso) ?? ((object)DBNull.Value))
            };
            List<CateterSurtidoLineaDto> list = new List<CateterSurtidoLineaDto>();
            try
            {
                using (var conexion = new SqlConnection(db.Database.Connection.ConnectionString))
                using (var comando = new SqlCommand("EXEC dbo.usp_cateter_surtir_fefo @clave, @cantidad, @tipo, @ubicacion_origen, @ubicacion_destino, @responsable, @observaciones, @referencia_uso", conexion))
                {
                    comando.Parameters.AddRange(values);
                    conexion.Open();

                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            list.Add(new CateterSurtidoLineaDto
                            {
                                LoteId = lector.GetInt32(lector.GetOrdinal("lote_id")),
                                Lote = lector.GetString(lector.GetOrdinal("lote")),
                                Caducidad = lector.IsDBNull(lector.GetOrdinal("caducidad")) ? (DateTime?)null : lector.GetDateTime(lector.GetOrdinal("caducidad")),
                                Cantidad = lector.GetDecimal(lector.GetOrdinal("cantidad")),
                                Folio = lector.GetString(lector.GetOrdinal("folio"))
                            });
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number >= 50000 && ex.Number < 52000)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = ex.Message
                });
            }
            return Ok<CateterSurtidoResultadoDto>(new CateterSurtidoResultadoDto
            {
                Clave = entrada.Clave,
                CantidadSolicitada = entrada.Cantidad,
                Lineas = list
            });
        }

        [HttpGet]
        [Route("exportar")]
        public HttpResponseMessage Exportar(DateTime? desde = null, DateTime? hasta = null, string clave = null, string tipo = null, string clasificacion = null)
        {
            //IL_009e: Unknown result type (might be due to invalid IL or missing references)
            //IL_00a4: Expected O, but got Unknown
            //IL_0435: Unknown result type (might be due to invalid IL or missing references)
            //IL_043a: Unknown result type (might be due to invalid IL or missing references)
            //IL_043d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0447: Expected O, but got Unknown
            //IL_0449: Expected O, but got Unknown
            //IL_045a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0464: Expected O, but got Unknown
            //IL_0475: Unknown result type (might be due to invalid IL or missing references)
            //IL_047a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0499: Expected O, but got Unknown
            List<vw_cateter_movimientos> list = (from m in AplicarFiltros(db.vw_cateter_movimientos.AsNoTracking(), desde, hasta, clave, tipo, clasificacion)
                orderby m.fecha, m.movimiento_id
                select m).ToList();
            ExcelPackage val = new ExcelPackage();
            try
            {
                (string, double)[] columnas = new(string, double)[12]
                {
                    ("Folio", 12.0),
                    ("Fecha", 14.0),
                    ("Tipo", 20.0),
                    ("Clave", 16.0),
                    ("Artículo", 34.0),
                    ("Lote", 14.0),
                    ("Caducidad", 12.0),
                    ("Cantidad", 10.0),
                    ("Origen", 12.0),
                    ("Destino", 12.0),
                    ("Referencia/uso", 20.0),
                    ("Responsable", 18.0)
                };
                ExcelWorksheet val2 = ExcelExportHelper.CrearHojaConEncabezado(val, "Movimientos", "BITÁCORA DE LA CLÍNICA DE CATÉTER", $"Generado {DateTime.Now:dd/MM/yyyy HH:mm}", "#1F3864", columnas);
                int num = 5;
                foreach (vw_cateter_movimientos item in list)
                {
                    ((ExcelRangeBase)val2.Cells[num, 2]).Value = item.folio;
                    ((ExcelRangeBase)val2.Cells[num, 3]).Value = item.fecha;
                    ((ExcelRangeBase)val2.Cells[num, 4]).Value = item.tipo;
                    ((ExcelRangeBase)val2.Cells[num, 5]).Value = item.clave;
                    ((ExcelRangeBase)val2.Cells[num, 6]).Value = item.articulo;
                    ((ExcelRangeBase)val2.Cells[num, 7]).Value = item.lote;
                    ((ExcelRangeBase)val2.Cells[num, 8]).Value = item.caducidad;
                    ((ExcelRangeBase)val2.Cells[num, 9]).Value = item.cantidad;
                    ((ExcelRangeBase)val2.Cells[num, 10]).Value = item.ubicacion_origen;
                    ((ExcelRangeBase)val2.Cells[num, 11]).Value = item.ubicacion_destino;
                    ((ExcelRangeBase)val2.Cells[num, 12]).Value = item.referencia_uso;
                    ((ExcelRangeBase)val2.Cells[num, 13]).Value = item.responsable;
                    ((ExcelRangeBase)val2.Cells[num, 3]).Style.Numberformat.Format = "dd/mm/yyyy hh:mm";
                    ((ExcelRangeBase)val2.Cells[num, 8]).Style.Numberformat.Format = "dd/mm/yyyy";
                    ((ExcelRangeBase)val2.Cells[num, 9]).Style.Numberformat.Format = "#,##0";
                    ExcelExportHelper.EstiloFilaDatos(val2.Cells[num, 2, num, 13]);
                    num++;
                }
                byte[] asByteArray = val.GetAsByteArray();
                HttpResponseMessage val3 = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = (HttpContent)new ByteArrayContent(asByteArray)
                };
                val3.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                val3.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"Cateter_Movimientos_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };
                return val3;
            }
            finally
            {
                val?.Dispose();
            }
        }

        private CateterMovimientoDto MapearConEstado(vw_cateter_movimientos v)
        {
            cateter_movimiento_estado est = db.cateter_movimiento_estado.AsNoTracking().FirstOrDefault(e => e.movimiento_id == v.movimiento_id);
            return Mapear(v, est);
        }

        private static CateterMovimientoDto Mapear(vw_cateter_movimientos v, cateter_movimiento_estado est)
        {
            return new CateterMovimientoDto
            {
                Id = v.movimiento_id,
                Folio = v.folio,
                Fecha = v.fecha,
                Tipo = v.tipo,
                Clasificacion = v.clasificacion,
                Clave = v.clave,
                Articulo = v.articulo,
                Lote = v.lote,
                Caducidad = v.caducidad,
                Cantidad = v.cantidad,
                UbicacionOrigen = v.ubicacion_origen,
                UbicacionDestino = v.ubicacion_destino,
                ReferenciaUso = v.referencia_uso,
                Responsable = v.responsable,
                Observaciones = v.observaciones,
                SaldoOrigenResultante = v.saldo_origen_resultante,
                SaldoDestinoResultante = v.saldo_destino_resultante,
                Estado = est?.estado ?? "Activo",
                RazonCancelacion = est?.razon_cancelacion,
                CanceladoEn = est?.cancelado_en,
                CanceladoPor = est?.cancelado_por,
                EsReversion = est?.es_reversion ?? false
            };
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

