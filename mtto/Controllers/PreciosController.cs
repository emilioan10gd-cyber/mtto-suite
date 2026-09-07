using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    /// <summary>
    /// Comparativa de precios por proveedor: cuánto cobra cada proveedor por un
    /// mismo artículo (con niveles de menudeo/mayoreo), para decidir a quién
    /// conviene comprarle. Es independiente del costo_unitario de mtto_articulo,
    /// que sigue siendo el costo de referencia usado para valuar el inventario.
    ///
    /// Sin guardia de borrado: a diferencia de catálogos y artículos, un precio
    /// de proveedor no tiene movimientos ni otro registro que dependa de él, así
    /// que editarlo o borrarlo cuando ya no aplica es seguro.
    /// </summary>
    [RoutePrefix("api/precios")]
    public class PreciosController : ApiController
    {
        private readonly Model1 db = new Model1();

        /// <summary>
        /// GET api/precios/por-articulo/MT-0122?cantidad=10 — todos los precios aplicables
        /// para esa cantidad, ordenados por precio unitario. Si cantidad no se pasa, muestra todos.
        /// </summary>
        [HttpGet, Route("por-articulo/{codigo}")]
        [ResponseType(typeof(ComparativaPreciosDto))]
        public IHttpActionResult PorArticulo(string codigo, decimal? cantidad = null)
        {
            var articulo = db.mtto_articulo.AsNoTracking().FirstOrDefault(a => a.codigo == codigo);
            if (articulo == null) return NotFound();

            var filas = db.mtto_precio_proveedor.AsNoTracking()
                .Include(p => p.mtto_proveedor)
                .Where(p => p.articulo_id == articulo.articulo_id && p.mtto_proveedor.activo)
                .ToList();

            // Filtrar precios aplicables según cantidad si se especifica
            if (cantidad.HasValue)
            {
                filas = filas.Where(p =>
                    p.cantidad_minima <= cantidad.Value &&
                    (p.cantidad_maxima == null || p.cantidad_maxima >= cantidad.Value)
                ).ToList();
            }

            // Encontrar el más barato (sin agrupar por tipo, solo el global más barato)
            var masBarato = filas.Any() ? filas.Min(p => p.precio_unitario) : decimal.MaxValue;

            var precios = filas
                .OrderBy(p => p.precio_unitario)
                .Select(p => new PrecioProveedorDto
                {
                    Id = p.precio_id,
                    ArticuloCodigo = articulo.codigo,
                    ArticuloNombre = articulo.nombre,
                    Proveedor = p.mtto_proveedor.nombre,
                    TipoPrecio = DeducirTipoPrecio(p.cantidad_minima, p.cantidad_maxima),
                    CantidadMinima = p.cantidad_minima,
                    CantidadMaxima = p.cantidad_maxima,
                    PrecioUnitario = p.precio_unitario,
                    CostoTotal = cantidad.HasValue ? p.precio_unitario * cantidad.Value : (decimal?)null,
                    Notas = p.notas,
                    ActualizadoEn = p.actualizado_en,
                    EsMasBarato = p.precio_unitario == masBarato
                })
                .ToList();

            return Ok(new ComparativaPreciosDto
            {
                ArticuloCodigo = articulo.codigo,
                ArticuloNombre = articulo.nombre,
                CantidadConsultada = cantidad,
                Precios = precios
            });
        }

        private static string DeducirTipoPrecio(decimal cantidadMin, decimal? cantidadMax)
        {
            if (cantidadMin <= 1) return "Menudeo";
            if (cantidadMin <= 6) return "Pequeño";
            if (cantidadMin <= 24) return "Mayoreo";
            return "Gran Volumen";
        }

        /// <summary>POST api/precios — registra lo que un proveedor cobra por un artículo en un rango de cantidad.</summary>
        [HttpPost, Route("")]
        [ResponseType(typeof(PrecioProveedorDto))]
        public IHttpActionResult Crear(PrecioProveedorInputDto entrada)
        {
            var error = Validar(entrada);
            if (error != null) return BadRequest(error);

            var articulo = db.mtto_articulo.FirstOrDefault(a => a.codigo == entrada.ArticuloCodigo);
            if (articulo == null) return BadRequest($"No existe el artículo con código '{entrada.ArticuloCodigo}'.");

            var proveedor = db.mtto_proveedor.FirstOrDefault(p => p.nombre == entrada.Proveedor);
            if (proveedor == null) return BadRequest($"No existe el proveedor '{entrada.Proveedor}'.");

            var fila = new mtto_precio_proveedor
            {
                articulo_id = articulo.articulo_id,
                proveedor_id = proveedor.proveedor_id,
                cantidad_minima = entrada.CantidadMinima ?? 1,
                cantidad_maxima = entrada.CantidadMaxima,
                precio_unitario = entrada.PrecioUnitario,
                notas = entrada.Notas?.Trim(),
                actualizado_en = DateTime.Now,
                creado_en = DateTime.Now
            };
            db.mtto_precio_proveedor.Add(fila);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex) when (SqlErrorHelper.EsViolacionDeUnicidad(ex))
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = $"{proveedor.nombre} ya tiene un precio para {articulo.codigo} en ese rango de cantidad. Edita el existente en vez de duplicarlo."
                });
            }

            return Content(HttpStatusCode.Created, new PrecioProveedorDto
            {
                Id = fila.precio_id,
                ArticuloCodigo = articulo.codigo,
                ArticuloNombre = articulo.nombre,
                Proveedor = proveedor.nombre,
                TipoPrecio = DeducirTipoPrecio(fila.cantidad_minima, fila.cantidad_maxima),
                CantidadMinima = fila.cantidad_minima,
                CantidadMaxima = fila.cantidad_maxima,
                PrecioUnitario = fila.precio_unitario,
                Notas = fila.notas,
                ActualizadoEn = fila.actualizado_en,
                EsMasBarato = false
            });
        }

        /// <summary>PUT api/precios/5 — corrige un precio ya registrado.</summary>
        [HttpPut, Route("{id:int}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Editar(int id, PrecioProveedorInputDto entrada)
        {
            var error = Validar(entrada);
            if (error != null) return BadRequest(error);

            var fila = db.mtto_precio_proveedor.Find(id);
            if (fila == null) return NotFound();

            var proveedor = db.mtto_proveedor.FirstOrDefault(p => p.nombre == entrada.Proveedor);
            if (proveedor == null) return BadRequest($"No existe el proveedor '{entrada.Proveedor}'.");

            fila.proveedor_id = proveedor.proveedor_id;
            fila.cantidad_minima = entrada.CantidadMinima ?? 1;
            fila.cantidad_maxima = entrada.CantidadMaxima;
            fila.precio_unitario = entrada.PrecioUnitario;
            fila.notas = entrada.Notas?.Trim();
            fila.actualizado_en = DateTime.Now;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex) when (SqlErrorHelper.EsViolacionDeUnicidad(ex))
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "Ya existe otro precio de ese proveedor para este artículo en ese rango de cantidad."
                });
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// DELETE api/precios/5 — sin guardia: un precio de proveedor no tiene
        /// historial dependiente, así que borrarlo cuando ya no aplica es seguro.
        /// </summary>
        [HttpDelete, Route("{id:int}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Eliminar(int id)
        {
            var fila = db.mtto_precio_proveedor.Find(id);
            if (fila == null) return NotFound();

            db.mtto_precio_proveedor.Remove(fila);
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static string Validar(PrecioProveedorInputDto entrada)
        {
            if (entrada == null) return "Cuerpo de la petición vacío.";
            if (string.IsNullOrWhiteSpace(entrada.ArticuloCodigo)) return "El artículo es obligatorio.";
            if (string.IsNullOrWhiteSpace(entrada.Proveedor)) return "El proveedor es obligatorio.";
            if (entrada.PrecioUnitario <= 0) return "El precio unitario debe ser mayor a cero.";
            if (entrada.CantidadMinima.HasValue && entrada.CantidadMinima.Value <= 0) return "La cantidad mínima debe ser mayor a cero.";
            if (entrada.CantidadMaxima.HasValue && entrada.CantidadMaxima.Value <= 0) return "La cantidad máxima debe ser mayor a cero.";
            if (entrada.CantidadMinima.HasValue && entrada.CantidadMaxima.HasValue &&
                entrada.CantidadMinima.Value > entrada.CantidadMaxima.Value)
                return "La cantidad mínima no puede ser mayor que la máxima.";
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
