using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
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
    /// Inventario de mantenimiento. Se apoya en vw_mtto_inventario, que ya trae
    /// los catálogos resueltos por nombre y el semáforo de existencias.
    /// </summary>
    [RoutePrefix("api/inventario")]
    public class InventarioController : ApiController
    {
        private readonly Model1 db = new Model1();

        /// <summary>
        /// Listado con filtros y paginación.
        /// GET api/inventario?q=llave&amp;categoria=Plomería&amp;nivel=Bajo minimo&amp;pagina=1&amp;tamano=50
        /// </summary>
        [HttpGet, Route("")]
        [ResponseType(typeof(PaginaDto<ArticuloDto>))]
        public IHttpActionResult Listar(
            string q = null,
            string categoria = null,
            string almacen = null,
            string estado = null,
            string nivel = null,
            int pagina = 1,
            int tamano = 50)
        {
            if (pagina < 1) pagina = 1;
            // Tope para que un tamano=999999 no tumbe el hosting compartido.
            if (tamano < 1 || tamano > 500) tamano = 50;

            var consulta = AplicarFiltros(db.vw_mtto_inventario.AsNoTracking(), q, categoria, almacen, estado, nivel);

            var total = consulta.Count();

            var datos = consulta
                .OrderBy(a => a.Categoria).ThenBy(a => a.Nombre_del_Articulo)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToList()
                .Select(Mapear)
                .ToList();

            return Ok(new PaginaDto<ArticuloDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling(total / (double)tamano),
                Datos = datos
            });
        }

        private static IQueryable<vw_mtto_inventario> AplicarFiltros(
            IQueryable<vw_mtto_inventario> consulta, string q, string categoria, string almacen, string estado, string nivel)
        {
            if (!string.IsNullOrWhiteSpace(q))
                consulta = consulta.Where(a => a.Nombre_del_Articulo.Contains(q) || a.ID.Contains(q));
            if (!string.IsNullOrWhiteSpace(categoria))
                consulta = consulta.Where(a => a.Categoria == categoria);
            if (!string.IsNullOrWhiteSpace(almacen))
                consulta = consulta.Where(a => a.Almacen == almacen);
            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(a => a.Estado == estado);
            if (!string.IsNullOrWhiteSpace(nivel))
                consulta = consulta.Where(a => a.Nivel == nivel);
            return consulta;
        }

        /// <summary>
        /// GET api/inventario/exportar — mismos filtros que Listar, sin paginar:
        /// exporta todo lo que cumpla el filtro actual, no solo la página visible.
        /// Reproduce el formato de la hoja "Inventario" del Excel original
        /// (MTTO_COMPLETE_DASHBOARD.xlsx): misma banda de color, encabezados y columnas.
        /// </summary>
        [HttpGet, Route("exportar")]
        public HttpResponseMessage Exportar(string q = null, string categoria = null, string almacen = null, string estado = null, string nivel = null)
        {
            var articulos = AplicarFiltros(db.vw_mtto_inventario.AsNoTracking(), q, categoria, almacen, estado, nivel)
                .OrderBy(a => a.Categoria).ThenBy(a => a.Nombre_del_Articulo)
                .ToList();

            using (var paquete = new ExcelPackage())
            {
                var columnas = new (string, double)[]
                {
                    ("ID", 12), ("Nombre del Artículo", 30), ("Categoría", 16), ("Unidad de Medida", 14),
                    ("Almacén", 18), ("Proveedor", 16), ("Estado", 14), ("Stock Actual", 12),
                    ("Stock Mínimo", 13), ("Stock Máximo", 13), ("Costo Unitario", 14),
                    ("Valor Total", 16), ("Última Actualización", 16)
                };

                var culturaEs = new CultureInfo("es-MX");
                var nombreMes = culturaEs.TextInfo.ToTitleCase(DateTime.Now.ToString("MMMM", culturaEs));
                var subtitulo = $"Mes de {nombreMes} {DateTime.Now.Year} (HOSPITAL GENERAL)";

                var hoja = ExcelExportHelper.CrearHojaConEncabezado(
                    paquete, "Inventario", "INVENTARIO DEL ÁREA DE MANTENIMIENTO", subtitulo, "#1F3864", columnas);

                var fila = 5;
                foreach (var a in articulos)
                {
                    hoja.Cells[fila, 2].Value = a.ID;
                    hoja.Cells[fila, 3].Value = a.Nombre_del_Articulo;
                    hoja.Cells[fila, 4].Value = a.Categoria;
                    hoja.Cells[fila, 5].Value = a.Unidad_de_Medida;
                    hoja.Cells[fila, 6].Value = a.Almacen;
                    hoja.Cells[fila, 7].Value = a.Proveedor;
                    hoja.Cells[fila, 8].Value = a.Estado;
                    hoja.Cells[fila, 9].Value = a.Stock_Actual;
                    hoja.Cells[fila, 10].Value = a.Stock_Minimo;
                    hoja.Cells[fila, 11].Value = a.Stock_Maximo;
                    hoja.Cells[fila, 12].Value = a.Costo_Unitario;
                    hoja.Cells[fila, 13].Value = a.Valor_Total;
                    hoja.Cells[fila, 14].Value = a.Ultima_Actualizacion;

                    hoja.Cells[fila, 9, fila, 11].Style.Numberformat.Format = "#,##0";
                    hoja.Cells[fila, 12, fila, 13].Style.Numberformat.Format = "$#,##0.00";
                    hoja.Cells[fila, 14].Style.Numberformat.Format = "mm-dd-yy";

                    ExcelExportHelper.EstiloFilaDatos(hoja.Cells[fila, 2, fila, 14]);
                    fila++;
                }

                var bytes = paquete.GetAsByteArray();
                var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                respuesta.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                respuesta.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"Inventario_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };
                return respuesta;
            }
        }

        /// <summary>GET api/inventario/MT-0028 — se busca por código, no por id interno.</summary>
        [HttpGet, Route("{codigo}")]
        [ResponseType(typeof(ArticuloDto))]
        public IHttpActionResult Obtener(string codigo)
        {
            var fila = db.vw_mtto_inventario.AsNoTracking().FirstOrDefault(a => a.ID == codigo);
            if (fila == null) return NotFound();
            return Ok(Mapear(fila));
        }

        /// <summary>
        /// Alta o actualización de artículo. POST api/inventario
        /// Delega en usp_mtto_alta_articulo, que resuelve los catálogos por nombre
        /// y hace upsert por código.
        /// </summary>
        [HttpPost, Route("")]
        [ResponseType(typeof(ArticuloDto))]
        public IHttpActionResult Guardar(ArticuloInputDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");
            if (string.IsNullOrWhiteSpace(entrada.Codigo)) return BadRequest("El código es obligatorio.");
            if (string.IsNullOrWhiteSpace(entrada.Nombre)) return BadRequest("El nombre es obligatorio.");
            if (entrada.StockActual < 0) return BadRequest("El stock actual no puede ser negativo.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.usp_mtto_alta_articulo " +
                    "@codigo, @nombre, @categoria, @unidad, @almacen, @estado, @proveedor, " +
                    "@stock_actual, @stock_minimo, @stock_maximo, @costo_unitario, @descripcion",
                    new SqlParameter("@codigo", entrada.Codigo),
                    new SqlParameter("@nombre", entrada.Nombre),
                    new SqlParameter("@categoria", entrada.Categoria ?? (object)DBNull.Value),
                    new SqlParameter("@unidad", entrada.Unidad ?? (object)DBNull.Value),
                    new SqlParameter("@almacen", entrada.Almacen ?? (object)DBNull.Value),
                    new SqlParameter("@estado", (object)entrada.Estado ?? "Disponible"),
                    new SqlParameter("@proveedor", (object)entrada.Proveedor ?? DBNull.Value),
                    new SqlParameter("@stock_actual", entrada.StockActual),
                    new SqlParameter("@stock_minimo", entrada.StockMinimo),
                    new SqlParameter("@stock_maximo", (object)entrada.StockMaximo ?? DBNull.Value),
                    new SqlParameter("@costo_unitario", (object)entrada.CostoUnitario ?? DBNull.Value),
                    new SqlParameter("@descripcion", (object)entrada.Descripcion ?? DBNull.Value));
            }
            catch (SqlException ex)
            {
                // 50020-50023: catálogo no encontrado. Es culpa del cliente, no del servidor.
                if (ex.Number >= 50020 && ex.Number <= 50023)
                    return Content(HttpStatusCode.BadRequest, new { error = ex.Message });
                throw;
            }

            var creado = db.vw_mtto_inventario.AsNoTracking().FirstOrDefault(a => a.ID == entrada.Codigo);
            return creado == null ? (IHttpActionResult)NotFound() : Ok(Mapear(creado));
        }

        /// <summary>PUT api/inventario/MT-0028 — mismo upsert, con el código en la ruta.</summary>
        [HttpPut, Route("{codigo}")]
        [ResponseType(typeof(ArticuloDto))]
        public IHttpActionResult Actualizar(string codigo, ArticuloInputDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");
            if (!db.mtto_articulo.Any(a => a.codigo == codigo)) return NotFound();

            entrada.Codigo = codigo;
            return Guardar(entrada);
        }

        /// <summary>
        /// DELETE api/inventario/MT-0028
        ///
        /// Solo borra si el artículo nunca tuvo movimientos: es el único caso en
        /// que eliminarlo no rompe trazabilidad (p. ej. se dio de alta con datos
        /// equivocados y aún no se le registra ni una entrada). Si ya tiene
        /// historial, se rechaza con 400 y se sugiere darlo de baja cambiando su
        /// Estado en vez de borrarlo.
        /// </summary>
        [HttpDelete, Route("{codigo}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Eliminar(string codigo)
        {
            var articulo = db.mtto_articulo.FirstOrDefault(a => a.codigo == codigo);
            if (articulo == null) return NotFound();

            var totalMovimientos = db.mtto_movimiento.Count(m => m.articulo_id == articulo.articulo_id);
            if (totalMovimientos > 0)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = $"No se puede eliminar: tiene {totalMovimientos} movimiento(s) registrado(s). " +
                            "Da de baja el artículo (cambia su Estado a Inactivo o Descontinuado) en vez de eliminarlo."
                });
            }

            db.mtto_articulo.Remove(articulo);
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static ArticuloDto Mapear(vw_mtto_inventario v)
        {
            return new ArticuloDto
            {
                Id = v.articulo_id,
                Codigo = v.ID,
                Nombre = v.Nombre_del_Articulo,
                Categoria = v.Categoria,
                Unidad = v.Unidad_de_Medida,
                Almacen = v.Almacen,
                Proveedor = v.Proveedor,
                Estado = v.Estado,
                StockActual = v.Stock_Actual,
                StockMinimo = v.Stock_Minimo,
                StockMaximo = v.Stock_Maximo,
                CostoUnitario = v.Costo_Unitario,
                ValorTotal = v.Valor_Total ?? 0m,
                Nivel = v.Nivel,
                UltimaActualizacion = v.Ultima_Actualizacion,
                CategoriaId = v.categoria_id,
                UnidadId = v.unidad_id,
                AlmacenId = v.almacen_id,
                ProveedorId = v.proveedor_id,
                EstadoId = v.estado_id
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
