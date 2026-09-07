using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/biomedico-catalogos")]
    [RequiereArea("biomedico")]
    public class BiomedicoCatalogosController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(BioCatalogosDto))]
        public IHttpActionResult Todos()
        {
            return Ok(new BioCatalogosDto
            {
                Areas = LeerAreas(),
                CategoriasEquipo = LeerCategoriasEquipo(),
                CategoriasInsumo = LeerCategoriasInsumo(),
                EstadosEquipo = LeerEstadosEquipo()
            });
        }

        [HttpGet]
        [Route("areas")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult Areas() => Ok(LeerAreas());

        [HttpGet]
        [Route("categorias-equipo")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult CategoriasEquipo() => Ok(LeerCategoriasEquipo());

        [HttpGet]
        [Route("categorias-insumo")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult CategoriasInsumo() => Ok(LeerCategoriasInsumo());

        [HttpGet]
        [Route("estados-equipo")]
        [ResponseType(typeof(IEnumerable<CatalogoItemDto>))]
        public IHttpActionResult EstadosEquipo() => Ok(LeerEstadosEquipo());

        [HttpPost]
        [Route("areas")]
        [ResponseType(typeof(CatalogoItemDto))]
        public IHttpActionResult CrearArea(BioCatalogoCreateDto entrada) =>
            CrearGenerico(entrada, nombre => db.bio_area.Any(a => a.nombre == nombre),
                nombre => { var a = new bio_area { nombre = nombre, activo = true, creado_en = DateTime.Now }; db.bio_area.Add(a); db.SaveChanges(); return new CatalogoItemDto { Id = a.area_id, Nombre = a.nombre }; });

        [HttpPost]
        [Route("categorias-equipo")]
        [ResponseType(typeof(CatalogoItemDto))]
        public IHttpActionResult CrearCategoriaEquipo(BioCatalogoCreateDto entrada) =>
            CrearGenerico(entrada, nombre => db.bio_categoria_equipo.Any(c => c.nombre == nombre),
                nombre => { var c = new bio_categoria_equipo { nombre = nombre, activo = true, creado_en = DateTime.Now }; db.bio_categoria_equipo.Add(c); db.SaveChanges(); return new CatalogoItemDto { Id = c.categoria_id, Nombre = c.nombre }; });

        [HttpPost]
        [Route("categorias-insumo")]
        [ResponseType(typeof(CatalogoItemDto))]
        public IHttpActionResult CrearCategoriaInsumo(BioCatalogoCreateDto entrada) =>
            CrearGenerico(entrada, nombre => db.bio_categoria_insumo.Any(c => c.nombre == nombre),
                nombre => { var c = new bio_categoria_insumo { nombre = nombre, activo = true, creado_en = DateTime.Now }; db.bio_categoria_insumo.Add(c); db.SaveChanges(); return new CatalogoItemDto { Id = c.categoria_id, Nombre = c.nombre }; });

        private IHttpActionResult CrearGenerico(BioCatalogoCreateDto entrada, Func<string, bool> existe, Func<string, CatalogoItemDto> crear)
        {
            if (entrada == null || string.IsNullOrWhiteSpace(entrada.Nombre))
                return BadRequest("El nombre es obligatorio.");
            var nombre = entrada.Nombre.Trim();
            if (existe(nombre))
                return Content(HttpStatusCode.BadRequest, new { error = $"Ya existe \"{nombre}\"." });
            return Ok(crear(nombre));
        }

        private List<CatalogoItemDto> LeerAreas() =>
            db.bio_area.AsNoTracking().Where(a => a.activo).OrderBy(a => a.nombre)
                .Select(a => new CatalogoItemDto { Id = a.area_id, Nombre = a.nombre }).ToList();

        private List<CatalogoItemDto> LeerCategoriasEquipo() =>
            db.bio_categoria_equipo.AsNoTracking().Where(c => c.activo).OrderBy(c => c.nombre)
                .Select(c => new CatalogoItemDto { Id = c.categoria_id, Nombre = c.nombre }).ToList();

        private List<CatalogoItemDto> LeerCategoriasInsumo() =>
            db.bio_categoria_insumo.AsNoTracking().Where(c => c.activo).OrderBy(c => c.nombre)
                .Select(c => new CatalogoItemDto { Id = c.categoria_id, Nombre = c.nombre }).ToList();

        private List<CatalogoItemDto> LeerEstadosEquipo() =>
            db.bio_estado_equipo.AsNoTracking().OrderBy(e => e.estado_id)
                .Select(e => new CatalogoItemDto { Id = e.estado_id, Nombre = e.nombre }).ToList();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
