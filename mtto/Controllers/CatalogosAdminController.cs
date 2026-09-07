using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    /// <summary>
    /// Alta, edición y baja de los catálogos operativos (categorías, almacenes,
    /// proveedores, unidades). Nunca se borra una fila: los artículos y
    /// movimientos históricos quedan enlazados por su id, así que eliminar una
    /// fila de catálogo corrompería ese historial. "Dar de baja" (activo=false)
    /// la saca de los combos de alta de artículos/movimientos sin tocar nada
    /// de lo ya registrado.
    ///
    /// Deja fuera el catálogo de Estados de artículo: es una lista fija de la
    /// que depende el semáforo de inventario (Sin stock/Bajo mínimo/Normal),
    /// no algo que el usuario deba poder ampliar libremente.
    /// </summary>
    [RoutePrefix("api/catalogos-admin")]
    public class CatalogosAdminController : ApiController
    {
        private readonly Model1 db = new Model1();

        // ================================================================ Categorías

        [HttpGet, Route("categorias")]
        [ResponseType(typeof(System.Collections.Generic.IEnumerable<CatalogoAdminDto>))]
        public IHttpActionResult ListarCategorias()
        {
            return Ok(db.mtto_categoria.AsNoTracking().OrderBy(c => c.nombre).ToList().Select(c => new CatalogoAdminDto
            {
                Id = c.categoria_id,
                Codigo = c.codigo,
                Nombre = c.nombre,
                Descripcion = c.descripcion,
                Activo = c.activo
            }));
        }

        [HttpPost, Route("categorias")]
        [ResponseType(typeof(CatalogoAdminDto))]
        public IHttpActionResult CrearCategoria(CatalogoAdminInputDto entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada?.Codigo) || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("Código y nombre son obligatorios.");

            var fila = new mtto_categoria
            {
                codigo = entrada.Codigo.Trim(),
                nombre = entrada.Nombre.Trim(),
                descripcion = entrada.Descripcion?.Trim(),
                activo = true
            };
            db.mtto_categoria.Add(fila);
            if (!GuardarOReportarDuplicado("categoría")) return duplicadoResultado;

            return Content(HttpStatusCode.Created, MapearCategoria(fila));
        }

        [HttpPut, Route("categorias/{id:int}")]
        [ResponseType(typeof(CatalogoAdminDto))]
        public IHttpActionResult EditarCategoria(int id, CatalogoAdminInputDto entrada)
        {
            var fila = db.mtto_categoria.Find(id);
            if (fila == null) return NotFound();
            if (string.IsNullOrWhiteSpace(entrada?.Codigo) || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("Código y nombre son obligatorios.");

            fila.codigo = entrada.Codigo.Trim();
            fila.nombre = entrada.Nombre.Trim();
            fila.descripcion = entrada.Descripcion?.Trim();
            if (!GuardarOReportarDuplicado("categoría")) return duplicadoResultado;

            return Ok(MapearCategoria(fila));
        }

        [HttpPost, Route("categorias/{id:int}/estado")]
        [ResponseType(typeof(void))]
        public IHttpActionResult CambiarEstadoCategoria(int id, EstadoActivoDto entrada)
        {
            var fila = db.mtto_categoria.Find(id);
            if (fila == null) return NotFound();
            fila.activo = entrada.Activo;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static CatalogoAdminDto MapearCategoria(mtto_categoria c) => new CatalogoAdminDto
        {
            Id = c.categoria_id,
            Codigo = c.codigo,
            Nombre = c.nombre,
            Descripcion = c.descripcion,
            Activo = c.activo
        };

        // ================================================================ Almacenes

        [HttpGet, Route("almacenes")]
        [ResponseType(typeof(System.Collections.Generic.IEnumerable<CatalogoAdminDto>))]
        public IHttpActionResult ListarAlmacenes()
        {
            return Ok(db.mtto_almacen.AsNoTracking().OrderBy(a => a.nombre).ToList().Select(MapearAlmacen));
        }

        [HttpPost, Route("almacenes")]
        [ResponseType(typeof(CatalogoAdminDto))]
        public IHttpActionResult CrearAlmacen(CatalogoAdminInputDto entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada?.Codigo) || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("Código y nombre son obligatorios.");

            var fila = new mtto_almacen
            {
                codigo = entrada.Codigo.Trim(),
                nombre = entrada.Nombre.Trim(),
                ubicacion = entrada.Ubicacion?.Trim(),
                responsable = entrada.Responsable?.Trim(),
                activo = true
            };
            db.mtto_almacen.Add(fila);
            if (!GuardarOReportarDuplicado("almacén")) return duplicadoResultado;

            return Content(HttpStatusCode.Created, MapearAlmacen(fila));
        }

        [HttpPut, Route("almacenes/{id:int}")]
        [ResponseType(typeof(CatalogoAdminDto))]
        public IHttpActionResult EditarAlmacen(int id, CatalogoAdminInputDto entrada)
        {
            var fila = db.mtto_almacen.Find(id);
            if (fila == null) return NotFound();
            if (string.IsNullOrWhiteSpace(entrada?.Codigo) || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("Código y nombre son obligatorios.");

            fila.codigo = entrada.Codigo.Trim();
            fila.nombre = entrada.Nombre.Trim();
            fila.ubicacion = entrada.Ubicacion?.Trim();
            fila.responsable = entrada.Responsable?.Trim();
            if (!GuardarOReportarDuplicado("almacén")) return duplicadoResultado;

            return Ok(MapearAlmacen(fila));
        }

        [HttpPost, Route("almacenes/{id:int}/estado")]
        [ResponseType(typeof(void))]
        public IHttpActionResult CambiarEstadoAlmacen(int id, EstadoActivoDto entrada)
        {
            var fila = db.mtto_almacen.Find(id);
            if (fila == null) return NotFound();
            fila.activo = entrada.Activo;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static CatalogoAdminDto MapearAlmacen(mtto_almacen a) => new CatalogoAdminDto
        {
            Id = a.almacen_id,
            Codigo = a.codigo,
            Nombre = a.nombre,
            Ubicacion = a.ubicacion,
            Responsable = a.responsable,
            Activo = a.activo
        };

        // ================================================================ Proveedores

        [HttpGet, Route("proveedores")]
        [ResponseType(typeof(System.Collections.Generic.IEnumerable<CatalogoAdminDto>))]
        public IHttpActionResult ListarProveedores()
        {
            return Ok(db.mtto_proveedor.AsNoTracking().OrderBy(p => p.nombre).ToList().Select(MapearProveedor));
        }

        [HttpPost, Route("proveedores")]
        [ResponseType(typeof(CatalogoAdminDto))]
        public IHttpActionResult CrearProveedor(CatalogoAdminInputDto entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada?.Codigo) || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("Código y nombre son obligatorios.");

            var fila = new mtto_proveedor
            {
                codigo = entrada.Codigo.Trim(),
                nombre = entrada.Nombre.Trim(),
                contacto = entrada.Contacto?.Trim(),
                telefono = entrada.Telefono?.Trim(),
                correo = entrada.Correo?.Trim(),
                activo = true
            };
            db.mtto_proveedor.Add(fila);
            if (!GuardarOReportarDuplicado("proveedor")) return duplicadoResultado;

            return Content(HttpStatusCode.Created, MapearProveedor(fila));
        }

        [HttpPut, Route("proveedores/{id:int}")]
        [ResponseType(typeof(CatalogoAdminDto))]
        public IHttpActionResult EditarProveedor(int id, CatalogoAdminInputDto entrada)
        {
            var fila = db.mtto_proveedor.Find(id);
            if (fila == null) return NotFound();
            if (string.IsNullOrWhiteSpace(entrada?.Codigo) || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("Código y nombre son obligatorios.");

            fila.codigo = entrada.Codigo.Trim();
            fila.nombre = entrada.Nombre.Trim();
            fila.contacto = entrada.Contacto?.Trim();
            fila.telefono = entrada.Telefono?.Trim();
            fila.correo = entrada.Correo?.Trim();
            if (!GuardarOReportarDuplicado("proveedor")) return duplicadoResultado;

            return Ok(MapearProveedor(fila));
        }

        [HttpPost, Route("proveedores/{id:int}/estado")]
        [ResponseType(typeof(void))]
        public IHttpActionResult CambiarEstadoProveedor(int id, EstadoActivoDto entrada)
        {
            var fila = db.mtto_proveedor.Find(id);
            if (fila == null) return NotFound();
            fila.activo = entrada.Activo;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static CatalogoAdminDto MapearProveedor(mtto_proveedor p) => new CatalogoAdminDto
        {
            Id = p.proveedor_id,
            Codigo = p.codigo,
            Nombre = p.nombre,
            Contacto = p.contacto,
            Telefono = p.telefono,
            Correo = p.correo,
            Activo = p.activo
        };

        // ================================================================ Unidades de medida

        [HttpGet, Route("unidades")]
        [ResponseType(typeof(System.Collections.Generic.IEnumerable<CatalogoAdminDto>))]
        public IHttpActionResult ListarUnidades()
        {
            return Ok(db.mtto_unidad_medida.AsNoTracking().OrderBy(u => u.nombre).ToList().Select(MapearUnidad));
        }

        [HttpPost, Route("unidades")]
        [ResponseType(typeof(CatalogoAdminDto))]
        public IHttpActionResult CrearUnidad(CatalogoAdminInputDto entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada?.Nombre))
                return BadRequest("El nombre es obligatorio.");

            var fila = new mtto_unidad_medida
            {
                nombre = entrada.Nombre.Trim(),
                abreviatura = entrada.Abreviatura?.Trim(),
                permite_decimal = entrada.PermiteDecimal ?? true,
                activo = true
            };
            db.mtto_unidad_medida.Add(fila);
            if (!GuardarOReportarDuplicado("unidad")) return duplicadoResultado;

            return Content(HttpStatusCode.Created, MapearUnidad(fila));
        }

        [HttpPut, Route("unidades/{id:int}")]
        [ResponseType(typeof(CatalogoAdminDto))]
        public IHttpActionResult EditarUnidad(int id, CatalogoAdminInputDto entrada)
        {
            var fila = db.mtto_unidad_medida.Find(id);
            if (fila == null) return NotFound();
            if (string.IsNullOrWhiteSpace(entrada?.Nombre))
                return BadRequest("El nombre es obligatorio.");

            fila.nombre = entrada.Nombre.Trim();
            fila.abreviatura = entrada.Abreviatura?.Trim();
            fila.permite_decimal = entrada.PermiteDecimal ?? fila.permite_decimal;
            if (!GuardarOReportarDuplicado("unidad")) return duplicadoResultado;

            return Ok(MapearUnidad(fila));
        }

        [HttpPost, Route("unidades/{id:int}/estado")]
        [ResponseType(typeof(void))]
        public IHttpActionResult CambiarEstadoUnidad(int id, EstadoActivoDto entrada)
        {
            var fila = db.mtto_unidad_medida.Find(id);
            if (fila == null) return NotFound();
            fila.activo = entrada.Activo;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static CatalogoAdminDto MapearUnidad(mtto_unidad_medida u) => new CatalogoAdminDto
        {
            Id = u.unidad_id,
            Nombre = u.nombre,
            Abreviatura = u.abreviatura,
            PermiteDecimal = u.permite_decimal,
            Activo = u.activo
        };

        // ================================================================ Utilería

        private IHttpActionResult duplicadoResultado;

        /// <summary>
        /// Intenta guardar; si el índice único de nombre/código truena, arma la
        /// respuesta 400 en <see cref="duplicadoResultado"/> y regresa false.
        /// </summary>
        private bool GuardarOReportarDuplicado(string etiqueta)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex) when (SqlErrorHelper.EsViolacionDeUnicidad(ex))
            {
                duplicadoResultado = Content(HttpStatusCode.BadRequest, new { error = $"Ya existe un(a) {etiqueta} con ese nombre o código." });
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
