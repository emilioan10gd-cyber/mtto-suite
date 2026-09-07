using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
    [RoutePrefix("api/cateter")]
    [RequiereArea("cateter")]
    public class CateterController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(PaginaDto<CateterArticuloDto>))]
        public IHttpActionResult Listar(string q = null, string categoria = null, string nivel = null, string alerta = null, bool soloActivos = true, bool soloConExistencia = false, int pagina = 1, int tamano = 50)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }
            if (tamano < 1 || tamano > 500)
            {
                tamano = 50;
            }
            IQueryable<vw_cateter_inventario> source = AplicarFiltros(db.vw_cateter_inventario.AsNoTracking(), q, categoria, nivel, alerta, soloActivos, soloConExistencia);
            int num = source.Count();
            List<CateterArticuloDto> datos = (from a in source
                orderby a.categoria, a.clave
                select a).Skip((pagina - 1) * tamano).Take(tamano).ToList()
                .Select(Mapear)
                .ToList();
            return Ok<PaginaDto<CateterArticuloDto>>(new PaginaDto<CateterArticuloDto>
            {
                Total = num,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)num / (double)tamano),
                Datos = datos
            });
        }

        private static IQueryable<vw_cateter_inventario> AplicarFiltros(IQueryable<vw_cateter_inventario> consulta, string q, string categoria, string nivel, string alerta, bool soloActivos, bool soloConExistencia)
        {
            if (!string.IsNullOrWhiteSpace(q))
            {
                consulta = consulta.Where(a => a.nombre.Contains(q) || a.clave.Contains(q) || a.descripcion.Contains(q) || a.marca.Contains(q) || a.referencia.Contains(q));
            }
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                consulta = consulta.Where(a => a.categoria == categoria);
            }
            if (!string.IsNullOrWhiteSpace(nivel))
            {
                consulta = consulta.Where(a => a.nivel == nivel);
            }
            if (!string.IsNullOrWhiteSpace(alerta))
            {
                consulta = consulta.Where(a => a.alerta_caducidad == alerta);
            }
            if (soloActivos)
            {
                consulta = consulta.Where(a => a.activo);
            }
            if (soloConExistencia)
            {
                consulta = consulta.Where(a => a.total > 0m);
            }
            return consulta;
        }

        [HttpGet]
        [Route("buscar")]
        [ResponseType(typeof(List<CateterArticuloDto>))]
        public IHttpActionResult PorClave(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
            {
                return BadRequest("Falta la clave.");
            }
            List<CateterArticuloDto> list = (from a in db.vw_cateter_inventario.AsNoTracking()
                where a.clave == clave
                orderby a.referencia
                select a).ToList().Select(Mapear).ToList();
            if (list.Count == 0)
            {
                return NotFound();
            }
            return Ok<List<CateterArticuloDto>>(list);
        }

        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(CateterArticuloDto))]
        public IHttpActionResult Obtener(int id)
        {
            vw_cateter_inventario vw_cateter_inventario2 = (db.vw_cateter_inventario.AsNoTracking()).FirstOrDefault(a => a.articulo_id == id);
            if (vw_cateter_inventario2 == null)
            {
                return NotFound();
            }
            return Ok<CateterArticuloDto>(Mapear(vw_cateter_inventario2));
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CateterArticuloDto))]
        public IHttpActionResult Guardar(CateterArticuloInputDto entrada)
        {
            if (entrada == null)
            {
                return BadRequest("Cuerpo de la petición vacío.");
            }
            if (string.IsNullOrWhiteSpace(entrada.Clave))
            {
                return BadRequest("La clave es obligatoria.");
            }
            if (string.IsNullOrWhiteSpace(entrada.Nombre))
            {
                return BadRequest("El nombre es obligatorio.");
            }
            if (entrada.StockMinimo < 0m)
            {
                return BadRequest("El stock mínimo no puede ser negativo.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                db.Database.ExecuteSqlCommand("EXEC dbo.usp_cateter_alta_articulo @clave, @nombre, @categoria, @unidad, @descripcion, @marca, @referencia, @calibre, @lumenes, @presentacion, @stock_minimo, @stock_maximo, @activo", new object[13]
                {
                    new SqlParameter("@clave", entrada.Clave),
                    new SqlParameter("@nombre", entrada.Nombre),
                    new SqlParameter("@categoria", entrada.Categoria ?? "Otros"),
                    new SqlParameter("@unidad", entrada.Unidad ?? "Pieza"),
                    new SqlParameter("@descripcion", ((object)entrada.Descripcion) ?? ((object)DBNull.Value)),
                    new SqlParameter("@marca", ((object)entrada.Marca) ?? ((object)DBNull.Value)),
                    new SqlParameter("@referencia", ((object)entrada.Referencia) ?? ((object)DBNull.Value)),
                    new SqlParameter("@calibre", ((object)entrada.Calibre) ?? ((object)DBNull.Value)),
                    new SqlParameter("@lumenes", ((object)entrada.Lumenes) ?? DBNull.Value),
                    new SqlParameter("@presentacion", ((object)entrada.Presentacion) ?? ((object)DBNull.Value)),
                    new SqlParameter("@stock_minimo", entrada.StockMinimo),
                    new SqlParameter("@stock_maximo", ((object)entrada.StockMaximo) ?? DBNull.Value),
                    new SqlParameter("@activo", entrada.Activo)
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
            string referencia = (string.IsNullOrWhiteSpace(entrada.Referencia) ? null : entrada.Referencia.Trim());
            IQueryable<vw_cateter_inventario> source = (db.vw_cateter_inventario.AsNoTracking()).Where(a => a.clave == entrada.Clave);
            source = ((referencia == null) ? source.Where(a => a.referencia == null) : source.Where(a => a.referencia == referencia));
            vw_cateter_inventario vw_cateter_inventario2 = source.FirstOrDefault();
            if (vw_cateter_inventario2 != null)
            {
                return Ok<CateterArticuloDto>(Mapear(vw_cateter_inventario2));
            }
            return NotFound();
        }

        [HttpDelete]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Eliminar(int id)
        {
            cateter_articulo cateter_articulo2 = (db.cateter_articulo).FirstOrDefault(a => a.articulo_id == id);
            if (cateter_articulo2 == null)
            {
                return NotFound();
            }
            int num = (db.cateter_lote).Count(l => l.articulo_id == id);
            if (num > 0)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = $"No se puede eliminar: la clave tiene {num} lote(s) registrado(s). " + "Desactívala en vez de borrarla para conservar el historial."
                });
            }
            db.cateter_articulo.Remove(cateter_articulo2);
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("{id:int}/estado")]
        [ResponseType(typeof(CateterArticuloDto))]
        public IHttpActionResult CambiarEstado(int id, [FromBody] CambioEstadoDto cambio)
        {
            if (cambio == null)
            {
                return BadRequest("Cuerpo de la petición vacío.");
            }
            cateter_articulo cateter_articulo2 = (db.cateter_articulo).FirstOrDefault(a => a.articulo_id == id);
            if (cateter_articulo2 == null)
            {
                return NotFound();
            }
            cateter_articulo2.activo = cambio.Activo;
            cateter_articulo2.actualizado_en = DateTime.Now;
            db.SaveChanges();
            vw_cateter_inventario vw_cateter_inventario2 = (db.vw_cateter_inventario.AsNoTracking()).FirstOrDefault(a => a.articulo_id == id);
            if (vw_cateter_inventario2 != null)
            {
                return Ok<CateterArticuloDto>(Mapear(vw_cateter_inventario2));
            }
            return NotFound();
        }

        [HttpGet]
        [Route("exportar")]
        public HttpResponseMessage Exportar(string q = null, string categoria = null, string nivel = null, string alerta = null, bool soloActivos = true, bool soloConExistencia = false)
        {
            //IL_037e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0383: Unknown result type (might be due to invalid IL or missing references)
            //IL_0386: Unknown result type (might be due to invalid IL or missing references)
            //IL_0390: Expected O, but got Unknown
            //IL_0392: Expected O, but got Unknown
            //IL_03a3: Unknown result type (might be due to invalid IL or missing references)
            //IL_03ad: Expected O, but got Unknown
            //IL_03be: Unknown result type (might be due to invalid IL or missing references)
            //IL_03c3: Unknown result type (might be due to invalid IL or missing references)
            //IL_03e2: Expected O, but got Unknown
            List<vw_cateter_inventario> list = (from a in AplicarFiltros(db.vw_cateter_inventario.AsNoTracking(), q, categoria, nivel, alerta, soloActivos, soloConExistencia)
                orderby a.categoria, a.clave
                select a).ToList();
            List<int> idsArticulos = list.Select(a => a.articulo_id).ToList();
            Dictionary<int, List<vw_cateter_lotes>> dictionary = (from l in (db.vw_cateter_lotes.AsNoTracking()).Where(l => idsArticulos.Contains(l.articulo_id)).ToList()
                orderby l.caducidad ?? DateTime.MaxValue, l.lote
                group l by l.articulo_id).ToDictionary((IGrouping<int, vw_cateter_lotes> g) => g.Key, (IGrouping<int, vw_cateter_lotes> g) => g.ToList());
            int num = 0;
            List<CateterInventarioExcelExporter.FilaExportacion> list2 = new List<CateterInventarioExcelExporter.FilaExportacion>();
            foreach (vw_cateter_inventario item in list)
            {
                num++;
                dictionary.TryGetValue(item.articulo_id, out var value);
                if (value == null || value.Count == 0)
                {
                    list2.Add(new CateterInventarioExcelExporter.FilaExportacion
                    {
                        Numero = num,
                        Clave = item.clave,
                        Descripcion = (item.descripcion ?? item.nombre),
                        TieneLote = false
                    });
                    continue;
                }
                foreach (vw_cateter_lotes item2 in value)
                {
                    list2.Add(new CateterInventarioExcelExporter.FilaExportacion
                    {
                        Numero = num,
                        Clave = item.clave,
                        Descripcion = (item.descripcion ?? item.nombre),
                        TieneLote = true,
                        Lote = item2.lote,
                        Caducidad = item2.caducidad,
                        CantidadExistente = item2.total,
                        EnStock = item2.en_stock
                    });
                }
            }
            byte[] array = CateterInventarioExcelExporter.Generar("HOSPITAL GENERAL REYNOSA \"DR. JOSÉ MARÍA CANTÚ GARZA\".", DateTime.Now, list2);
            HttpResponseMessage val = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = (HttpContent)new ByteArrayContent(array)
            };
            val.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            val.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"Cateter_Inventario_{DateTime.Now:yyyy-MM-dd}.xlsx"
            };
            return val;
        }

        internal static CateterArticuloDto Mapear(vw_cateter_inventario v)
        {
            return new CateterArticuloDto
            {
                Id = v.articulo_id,
                Clave = v.clave,
                Nombre = v.nombre,
                Descripcion = v.descripcion,
                Categoria = v.categoria,
                Unidad = v.unidad,
                Marca = v.marca,
                Referencia = v.referencia,
                Calibre = v.calibre,
                Lumenes = v.lumenes,
                Presentacion = v.presentacion,
                EnAlmacen = v.en_almacen,
                EnStock = v.en_stock,
                Total = v.total,
                TotalVigente = v.total_vigente,
                TotalVencido = v.total_vencido,
                StockMinimo = v.stock_minimo,
                StockMaximo = v.stock_maximo,
                CpmMensual = v.cpm_mensual,
                LotesConExistencia = v.lotes_con_existencia.GetValueOrDefault(),
                CaducidadProxima = v.caducidad_proxima,
                Nivel = v.nivel,
                AlertaCaducidad = v.alerta_caducidad,
                Activo = v.activo,
                UltimaActualizacion = v.actualizado_en,
                CategoriaId = v.categoria_id,
                UnidadId = v.unidad_id
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

