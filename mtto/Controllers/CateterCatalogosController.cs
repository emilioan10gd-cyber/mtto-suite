using System;
using System.Collections.Generic;
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
    [RoutePrefix("api/cateter-catalogos")]
    [RequiereArea("cateter")]
    public class CateterCatalogosController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(CateterCatalogosDto))]
        public IHttpActionResult Todos()
        {
            return Ok<CateterCatalogosDto>(new CateterCatalogosDto
            {
                Categorias = LeerCategorias(),
                Ubicaciones = LeerUbicaciones(),
                TiposMovimiento = LeerTiposMovimiento(),
                Unidades = LeerUnidades()
            });
        }

        [HttpGet]
        [Route("categorias")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult Categorias()
        {
            return Ok<List<CatalogoItemDto>>(LeerCategorias());
        }

        /// <summary>
        /// GET api/cateter-catalogos/ubicaciones — por omisión solo las activas,
        /// que es lo que necesitan los combos. La pantalla de administración pide
        /// incluirInactivas=true para poder reactivar lo que se dio de baja.
        /// </summary>
        [HttpGet]
        [Route("ubicaciones")]
        [ResponseType(typeof(IEnumerable<CateterUbicacionDto>))]
        public IHttpActionResult Ubicaciones(bool incluirInactivas = false)
        {
            return Ok<List<CateterUbicacionDto>>(LeerUbicaciones(!incluirInactivas));
        }

        [HttpGet]
        [Route("tipos-movimiento")]
        [ResponseType(typeof(IEnumerable<CateterTipoMovimientoDto>))]
        public IHttpActionResult TiposMovimiento()
        {
            return Ok<List<CateterTipoMovimientoDto>>(LeerTiposMovimiento());
        }

        private List<CatalogoItemDto> LeerCategorias()
        {
            return (from c in db.cateter_categoria.AsNoTracking()
                where c.activo
                orderby c.nombre
                select new CatalogoItemDto
                {
                    Id = c.categoria_id,
                    Codigo = c.codigo,
                    Nombre = c.nombre
                }).ToList();
        }

        private List<CateterUbicacionDto> LeerUbicaciones(bool soloActivas = true)
        {
            return (from u in db.cateter_ubicacion.AsNoTracking()
                where !soloActivas || u.activo
                orderby u.activo descending, u.orden
                select new CateterUbicacionDto
                {
                    Id = u.ubicacion_id,
                    Codigo = u.codigo,
                    Nombre = u.nombre,
                    Descripcion = u.descripcion,
                    EsAlmacen = u.es_almacen,
                    Activo = u.activo
                }).ToList();
        }

        /// <summary>
        /// POST api/cateter-catalogos/ubicaciones — dar de alta un origen/destino
        /// nuevo (un salón, un carro de curaciones...) sin tocar la base de datos.
        /// </summary>
        [HttpPost]
        [Route("ubicaciones")]
        [ResponseType(typeof(CateterUbicacionDto))]
        public IHttpActionResult CrearUbicacion(CateterUbicacionCreateDto entrada)
        {
            if (entrada == null || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("El nombre es obligatorio.");

            var nombre = entrada.Nombre.Trim();
            if (nombre.Length > 80)
                return BadRequest("El nombre no puede pasar de 80 caracteres.");

            // nombre es UNIQUE en la tabla: se avisa aquí para no devolver un
            // error de SQL crudo. Se compara también contra las inactivas
            // porque el UNIQUE no distingue activo de inactivo.
            var choque = db.cateter_ubicacion.FirstOrDefault(u => u.nombre == nombre);
            if (choque != null)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = choque.activo
                        ? $"Ya existe una ubicación llamada \"{nombre}\"."
                        : $"Ya existe una ubicación llamada \"{nombre}\", pero está desactivada. Reactívala en vez de crear otra."
                });
            }

            var nueva = new cateter_ubicacion
            {
                codigo = SiguienteCodigo(),
                nombre = nombre,
                descripcion = string.IsNullOrWhiteSpace(entrada.Descripcion) ? null : entrada.Descripcion.Trim(),
                es_almacen = entrada.EsAlmacen,
                orden = (short)((db.cateter_ubicacion.Max(u => (short?)u.orden) ?? 0) + 1),
                activo = true,
                creado_en = DateTime.Now
            };

            db.cateter_ubicacion.Add(nueva);
            db.SaveChanges();

            return Created("api/cateter-catalogos/ubicaciones/" + nueva.ubicacion_id, Mapear(nueva));
        }

        /// <summary>
        /// PUT api/cateter-catalogos/ubicaciones/{id} — editar nombre, descripción
        /// o el carácter de almacén. El código no se toca: ya viaja en movimientos
        /// históricos y renombrarlo rompería la trazabilidad.
        /// </summary>
        [HttpPut]
        [Route("ubicaciones/{id:int}")]
        [ResponseType(typeof(CateterUbicacionDto))]
        public IHttpActionResult ActualizarUbicacion(int id, CateterUbicacionCreateDto entrada)
        {
            if (entrada == null || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("El nombre es obligatorio.");

            var nombre = entrada.Nombre.Trim();
            if (nombre.Length > 80)
                return BadRequest("El nombre no puede pasar de 80 caracteres.");

            var ub = db.cateter_ubicacion.FirstOrDefault(u => u.ubicacion_id == id);
            if (ub == null) return NotFound();

            if (db.cateter_ubicacion.Any(u => u.nombre == nombre && u.ubicacion_id != id))
                return Content(HttpStatusCode.BadRequest,
                    new { error = $"Ya existe otra ubicación llamada \"{nombre}\"." });

            ub.nombre = nombre;
            ub.descripcion = string.IsNullOrWhiteSpace(entrada.Descripcion) ? null : entrada.Descripcion.Trim();
            ub.es_almacen = entrada.EsAlmacen;
            db.SaveChanges();

            return Ok(Mapear(ub));
        }

        /// <summary>
        /// DELETE api/cateter-catalogos/ubicaciones/{id} — la desactiva, no la borra:
        /// su id sigue colgando de movimientos y existencias históricos.
        ///
        /// Se rechaza si todavía guarda piezas. Desactivarla con saldo las
        /// escondería del inventario sin que ningún movimiento las hubiera sacado,
        /// y el material aparecería como desaparecido.
        /// </summary>
        [HttpDelete]
        [Route("ubicaciones/{id:int}")]
        public IHttpActionResult EliminarUbicacion(int id)
        {
            var ub = db.cateter_ubicacion.FirstOrDefault(u => u.ubicacion_id == id);
            if (ub == null) return NotFound();

            // cateter_existencia no se expone como DbSet a propósito (los saldos
            // solo los mueven los SP), así que la consulta va en SQL directo.
            var piezas = db.Database.SqlQuery<decimal?>(
                "SELECT SUM(cantidad) FROM dbo.cateter_existencia WHERE ubicacion_id = @p0", id)
                .FirstOrDefault() ?? 0m;

            if (piezas > 0)
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = $"\"{ub.nombre}\" todavía tiene {piezas:0.###} piezas. " +
                            "Trasládalas a otra ubicación antes de desactivarla."
                });

            ub.activo = false;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// POST api/cateter-catalogos/ubicaciones/{id}/estado — reactivar una
        /// ubicación dada de baja. Es el reverso del DELETE: como aquel no borra
        /// nada, volver a prenderla recupera su historial intacto.
        /// </summary>
        [HttpPost]
        [Route("ubicaciones/{id:int}/estado")]
        [ResponseType(typeof(CateterUbicacionDto))]
        public IHttpActionResult CambiarEstadoUbicacion(int id, CambioEstadoDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");

            var ub = db.cateter_ubicacion.FirstOrDefault(u => u.ubicacion_id == id);
            if (ub == null) return NotFound();

            ub.activo = entrada.Activo;
            db.SaveChanges();

            return Ok(Mapear(ub));
        }

        /// <summary>UBI-001, UBI-002... El UNIQUE de codigo obliga a que no se repita.</summary>
        private string SiguienteCodigo()
        {
            var existentes = new HashSet<string>(db.cateter_ubicacion.Select(u => u.codigo).ToList());
            for (var n = 1; n < 1000; n++)
            {
                var candidato = "UBI-" + n.ToString("000");
                if (!existentes.Contains(candidato)) return candidato;
            }
            throw new InvalidOperationException("Se agotaron los códigos de ubicación (UBI-999).");
        }

        private static CateterUbicacionDto Mapear(cateter_ubicacion u)
        {
            return new CateterUbicacionDto
            {
                Id = u.ubicacion_id,
                Codigo = u.codigo,
                Nombre = u.nombre,
                Descripcion = u.descripcion,
                EsAlmacen = u.es_almacen,
                Activo = u.activo
            };
        }

        private List<CateterTipoMovimientoDto> LeerTiposMovimiento()
        {
            return (from t in db.cateter_tipo_movimiento.AsNoTracking()
                where t.activo
                orderby t.orden
                select new CateterTipoMovimientoDto
                {
                    Id = t.tipo_id,
                    Nombre = t.nombre,
                    Clasificacion = t.clasificacion,
                    RequiereOrigen = t.requiere_origen,
                    RequiereDestino = t.requiere_destino
                }).ToList();
        }

        private List<CatalogoItemDto> LeerUnidades()
        {
            return (from u in db.mtto_unidad_medida.AsNoTracking()
                where u.activo
                orderby u.nombre
                select new CatalogoItemDto
                {
                    Id = u.unidad_id,
                    Codigo = null,
                    Nombre = u.nombre
                }).ToList();
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

