using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;

namespace mtto.Controllers
{
    /// <summary>
    /// Catálogos para llenar los combos de los formularios.
    /// Solo lectura: son listas de configuración, no datos de operación.
    /// </summary>
    [RoutePrefix("api/catalogos")]
    public class CatalogosController : ApiController
    {
        private readonly Model1 db = new Model1();

        /// <summary>
        /// GET api/catalogos — todos los combos de un jalón, para cachearlos al
        /// abrir la app en vez de pedirlos formulario por formulario.
        /// </summary>
        [HttpGet, Route("")]
        [ResponseType(typeof(CatalogosDto))]
        public IHttpActionResult Todos()
        {
            return Ok(new CatalogosDto
            {
                Categorias = LeerCategorias(),
                Almacenes = LeerAlmacenes(),
                Proveedores = LeerProveedores(),
                Unidades = LeerUnidades(),
                Estados = LeerEstados(),
                TiposMovimiento = LeerTiposMovimiento()
            });
        }

        [HttpGet, Route("categorias")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult Categorias() { return Ok(LeerCategorias()); }

        [HttpGet, Route("almacenes")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult Almacenes() { return Ok(LeerAlmacenes()); }

        [HttpGet, Route("proveedores")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult Proveedores() { return Ok(LeerProveedores()); }

        [HttpGet, Route("unidades")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult Unidades() { return Ok(LeerUnidades()); }

        [HttpGet, Route("estados")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult Estados() { return Ok(LeerEstados()); }

        [HttpGet, Route("tipos-movimiento")]
        [ResponseType(typeof(IEnumerable<TipoMovimientoDto>))]
        public IHttpActionResult TiposMovimiento() { return Ok(LeerTiposMovimiento()); }

        // ------------------------------------------------------------ internos

        private List<CatalogoItemDto> LeerCategorias()
        {
            return db.mtto_categoria.AsNoTracking().Where(c => c.activo)
                .OrderBy(c => c.nombre)
                .Select(c => new CatalogoItemDto { Id = c.categoria_id, Codigo = c.codigo, Nombre = c.nombre })
                .ToList();
        }

        private List<CatalogoItemDto> LeerAlmacenes()
        {
            return db.mtto_almacen.AsNoTracking().Where(a => a.activo)
                .OrderBy(a => a.nombre)
                .Select(a => new CatalogoItemDto { Id = a.almacen_id, Codigo = a.codigo, Nombre = a.nombre })
                .ToList();
        }

        private List<CatalogoItemDto> LeerProveedores()
        {
            return db.mtto_proveedor.AsNoTracking().Where(p => p.activo)
                .OrderBy(p => p.nombre)
                .Select(p => new CatalogoItemDto { Id = p.proveedor_id, Codigo = p.codigo, Nombre = p.nombre })
                .ToList();
        }

        private List<CatalogoItemDto> LeerUnidades()
        {
            return db.mtto_unidad_medida.AsNoTracking().Where(u => u.activo)
                .OrderBy(u => u.nombre)
                .Select(u => new CatalogoItemDto { Id = u.unidad_id, Codigo = u.abreviatura, Nombre = u.nombre })
                .ToList();
        }

        private List<CatalogoItemDto> LeerEstados()
        {
            return db.mtto_estado_articulo.AsNoTracking()
                .OrderBy(e => e.nombre)
                .Select(e => new CatalogoItemDto { Id = e.estado_id, Codigo = null, Nombre = e.nombre })
                .ToList();
        }

        private List<TipoMovimientoDto> LeerTiposMovimiento()
        {
            return db.mtto_tipo_movimiento.AsNoTracking()
                .OrderBy(t => t.tipo_id)
                .Select(t => new TipoMovimientoDto
                {
                    Id = t.tipo_id,
                    Nombre = t.nombre,
                    Signo = t.signo,
                    RequiereDestino = t.requiere_destino
                })
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
