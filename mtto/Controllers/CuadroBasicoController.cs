using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/cuadro-basico")]
    [RequiereArea("cateter")]
    public class CuadroBasicoController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("buscar")]
        [ResponseType(typeof(PaginaDto<CuadroBasicoItemDto>))]
        public IHttpActionResult Buscar(string q = null, string grupo = null, bool soloCateteres = true, int pagina = 1, int tamano = 30)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }
            if (tamano < 1 || tamano > 100)
            {
                tamano = 30;
            }
            if (string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(grupo) && !soloCateteres)
            {
                return BadRequest("Da un texto de búsqueda, un grupo, o deja soloCateteres activo: el cuadro básico tiene 5,222 claves y no se lista completo de un jalón.");
            }
            IQueryable<cpm_clave> source = ((IEnumerable<cpm_clave>)db.cpm_clave.AsNoTracking()).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                source = source.Where(c => c.descripcion.Contains(q) || c.clave.Contains(q));
            }
            if (!string.IsNullOrWhiteSpace(grupo))
            {
                source = source.Where(c => c.grupo == grupo);
            }
            if (soloCateteres)
            {
                source = source.Where(c => c.descripcion.Contains("CATETER") || c.descripcion.Contains("CATÉTER"));
            }
            int num = source.Count();
            List<string> collection = (db.cateter_articulo.AsNoTracking()).Select(a => a.clave).ToList();
            HashSet<string> yaCargadas = new HashSet<string>(collection);
            List<CuadroBasicoItemDto> datos = (from c in source.OrderBy(c => c.clave).Skip((pagina - 1) * tamano).Take(tamano)
                    .ToList()
                select new CuadroBasicoItemDto
                {
                    Clave = c.clave,
                    Descripcion = c.descripcion,
                    Grupo = c.grupo,
                    GrupoTerapeutico = c.grupo_terapeutico,
                    CantidadMensual = c.cantidad_mensual,
                    YaEnCatalogo = yaCargadas.Contains(c.clave)
                }).ToList();
            return Ok<PaginaDto<CuadroBasicoItemDto>>(new PaginaDto<CuadroBasicoItemDto>
            {
                Total = num,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)num / (double)tamano),
                Datos = datos
            });
        }

        [HttpGet]
        [Route("clave")]
        [ResponseType(typeof(CuadroBasicoItemDto))]
        public IHttpActionResult Obtener(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return BadRequest("Falta la clave.");
            }
            cpm_clave cpm_clave2 = (db.cpm_clave.AsNoTracking()).FirstOrDefault(x => x.clave == valor);
            if (cpm_clave2 == null)
            {
                return NotFound();
            }
            bool yaEnCatalogo = (db.cateter_articulo.AsNoTracking()).Any(a => a.clave == valor);
            return Ok<CuadroBasicoItemDto>(new CuadroBasicoItemDto
            {
                Clave = cpm_clave2.clave,
                Descripcion = cpm_clave2.descripcion,
                Grupo = cpm_clave2.grupo,
                GrupoTerapeutico = cpm_clave2.grupo_terapeutico,
                CantidadMensual = cpm_clave2.cantidad_mensual,
                YaEnCatalogo = yaEnCatalogo
            });
        }

        [HttpPost]
        [Route("agregar")]
        [ResponseType(typeof(CateterArticuloDto))]
        public IHttpActionResult Agregar(CuadroBasicoAltaDto entrada)
        {
            if (entrada == null)
            {
                return BadRequest("Cuerpo de la petición vacío.");
            }
            if (string.IsNullOrWhiteSpace(entrada.Clave))
            {
                return BadRequest("La clave es obligatoria.");
            }
            cpm_clave cpm_clave2 = (db.cpm_clave.AsNoTracking()).FirstOrDefault(c => c.clave == entrada.Clave);
            if (cpm_clave2 == null)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "La clave " + entrada.Clave + " no existe en el cuadro básico."
                });
            }
            if ((db.cateter_articulo).Any(a => a.clave == entrada.Clave && a.referencia == null))
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "La clave " + entrada.Clave + " ya está en el catálogo de la clínica. Si es otra presentación (distinta marca o referencia), agrégala desde Inventario capturando su referencia."
                });
            }
            string value = ((!string.IsNullOrWhiteSpace(entrada.Nombre)) ? entrada.Nombre : DerivarNombreCorto(cpm_clave2.descripcion));
            try
            {
                db.Database.ExecuteSqlCommand("EXEC dbo.usp_cateter_alta_articulo @clave, @nombre, @categoria, @unidad, @descripcion, @marca, @referencia, @calibre, @lumenes, @presentacion, @stock_minimo, @stock_maximo, @activo", new object[13]
                {
                    new SqlParameter("@clave", entrada.Clave),
                    new SqlParameter("@nombre", value),
                    new SqlParameter("@categoria", entrada.Categoria ?? "Otros"),
                    new SqlParameter("@unidad", "Pieza"),
                    new SqlParameter("@descripcion", ((object)cpm_clave2.descripcion) ?? ((object)DBNull.Value)),
                    new SqlParameter("@marca", DBNull.Value),
                    new SqlParameter("@referencia", DBNull.Value),
                    new SqlParameter("@calibre", DBNull.Value),
                    new SqlParameter("@lumenes", DBNull.Value),
                    new SqlParameter("@presentacion", DBNull.Value),
                    new SqlParameter("@stock_minimo", entrada.StockMinimo),
                    new SqlParameter("@stock_maximo", ((object)entrada.StockMaximo) ?? DBNull.Value),
                    new SqlParameter("@activo", true)
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
            if (!string.IsNullOrWhiteSpace(entrada.DescripcionPropia))
            {
                cateter_clave_descripcion cateter_clave_descripcion2 = (db.cateter_clave_descripcion).FirstOrDefault(d => d.Clave == entrada.Clave);
                if (cateter_clave_descripcion2 != null)
                {
                    cateter_clave_descripcion2.DescripcionPropia = entrada.DescripcionPropia;
                    cateter_clave_descripcion2.ActualizadoEn = DateTime.Now;
                }
                else
                {
                    db.cateter_clave_descripcion.Add(new cateter_clave_descripcion
                    {
                        Clave = entrada.Clave,
                        DescripcionPropia = entrada.DescripcionPropia,
                        ActualizadoEn = DateTime.Now
                    });
                }
                db.SaveChanges();
            }
            vw_cateter_inventario vw_cateter_inventario2 = (db.vw_cateter_inventario.AsNoTracking()).FirstOrDefault(a => a.clave == entrada.Clave);
            if (vw_cateter_inventario2 != null)
            {
                return Ok<CateterArticuloDto>(CateterController.Mapear(vw_cateter_inventario2));
            }
            return NotFound();
        }

        /// <summary>
        /// GET api/cuadro-basico/descripciones-propias — las descripciones que la
        /// clínica escribió por su cuenta, con la oficial al lado para comparar.
        ///
        /// A diferencia de Buscar, aquí sí se lista todo sin filtro: son las que
        /// alguien capturó a mano, así que son pocas por definición.
        /// </summary>
        [HttpGet]
        [Route("descripciones-propias")]
        [ResponseType(typeof(IEnumerable<DescripcionPropiaDto>))]
        public IHttpActionResult DescripcionesPropias()
        {
            var propias = db.cateter_clave_descripcion.AsNoTracking()
                .OrderBy(d => d.Clave)
                .ToList();

            var claves = propias.Select(d => d.Clave).ToList();

            var oficiales = db.cpm_clave.AsNoTracking()
                .Where(c => claves.Contains(c.clave))
                .ToDictionary(c => c.clave, c => c.descripcion);

            var enCatalogo = new HashSet<string>(
                db.cateter_articulo.AsNoTracking()
                    .Where(a => claves.Contains(a.clave))
                    .Select(a => a.clave)
                    .ToList());

            return Ok(propias.Select(d => new DescripcionPropiaDto
            {
                Clave = d.Clave,
                DescripcionPropia = d.DescripcionPropia,
                DescripcionOficial = oficiales.ContainsKey(d.Clave) ? oficiales[d.Clave] : null,
                ActualizadoEn = d.ActualizadoEn,
                EnCatalogo = enCatalogo.Contains(d.Clave)
            }));
        }

        [HttpPut]
        [Route("descripcion-propia")]
        [ResponseType(typeof(void))]
        public IHttpActionResult ActualizarDescripcionPropia(ActualizarDescripcionPropiaDto entrada)
        {
            if (entrada == null || string.IsNullOrWhiteSpace(entrada.Clave))
            {
                return BadRequest("La clave es obligatoria.");
            }
            cateter_clave_descripcion cateter_clave_descripcion2 = (db.cateter_clave_descripcion).FirstOrDefault(d => d.Clave == entrada.Clave);
            if (cateter_clave_descripcion2 != null)
            {
                if (string.IsNullOrWhiteSpace(entrada.DescripcionPropia))
                {
                    db.cateter_clave_descripcion.Remove(cateter_clave_descripcion2);
                }
                else
                {
                    cateter_clave_descripcion2.DescripcionPropia = entrada.DescripcionPropia;
                    cateter_clave_descripcion2.ActualizadoEn = DateTime.Now;
                }
            }
            else if (!string.IsNullOrWhiteSpace(entrada.DescripcionPropia))
            {
                db.cateter_clave_descripcion.Add(new cateter_clave_descripcion
                {
                    Clave = entrada.Clave,
                    DescripcionPropia = entrada.DescripcionPropia,
                    ActualizadoEn = DateTime.Now
                });
            }
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static string DerivarNombreCorto(string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                return "Sin descripción";
            }
            string text = Regex.Replace(descripcion.Trim(), "^(CATETERES?\\.?|EQUIPOS?\\.?|TUBOS\\.?|VALVULAS|BOLSAS\\.?|CONECTORES|ADAPTADORES|TAPONES\\.?|GUIAS\\.?|SISTEMA\\.?|AGUJAS)\\s*", "", RegexOptions.IgnoreCase).Trim(' ', '.', ',');
            if (text.Length > 90)
            {
                string text2 = text.Substring(0, 90);
                int num = text2.LastIndexOf(' ');
                if (num > 0)
                {
                    text2 = text2.Substring(0, num);
                }
                text = text2 + "…";
            }
            if (text.Length <= 0)
            {
                return "Sin descripción";
            }
            return char.ToUpper(text[0]) + text.Substring(1);
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

