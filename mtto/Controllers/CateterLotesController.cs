using System;
using System.Collections.Generic;
using System.Data;
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
    [RoutePrefix("api/cateter-lotes")]
    [RequiereArea("cateter")]
    public class CateterLotesController : ApiController
    {
        private readonly Model1 db = new Model1();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(PaginaDto<CateterLoteDto>))]
        public IHttpActionResult Listar(int? articuloId = null, string estado = null, bool soloConExistencia = false, int pagina = 1, int tamano = 100)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }
            if (tamano < 1 || tamano > 500)
            {
                tamano = 100;
            }
            IQueryable<vw_cateter_lotes> source = db.vw_cateter_lotes.AsNoTracking();
            if (articuloId.HasValue)
            {
                source = source.Where(l => l.articulo_id == ((int?)articuloId).Value);
            }
            if (!string.IsNullOrWhiteSpace(estado))
            {
                source = source.Where(l => l.estado_caducidad == estado);
            }
            if (soloConExistencia)
            {
                source = source.Where(l => l.total > 0m);
            }
            int num = source.Count();
            List<CateterLoteDto> datos = (from l in source
                orderby l.clave, l.caducidad ?? DateTime.MaxValue
                select l).Skip((pagina - 1) * tamano).Take(tamano).ToList()
                .Select(Mapear)
                .ToList();
            return Ok<PaginaDto<CateterLoteDto>>(new PaginaDto<CateterLoteDto>
            {
                Total = num,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)num / (double)tamano),
                Datos = datos
            });
        }

        [HttpGet]
        [Route("por-caducar")]
        [ResponseType(typeof(IEnumerable<CateterLoteDto>))]
        public IHttpActionResult PorCaducar(int top = 30)
        {
            if (top < 1 || top > 500)
            {
                top = 30;
            }
            List<CateterLoteDto> list = (from l in db.vw_cateter_lotes.AsNoTracking()
                where l.total > 0m && (l.estado_caducidad == "Vencido" || l.estado_caducidad == "Critico" || l.estado_caducidad == "Por vencer")
                orderby l.caducidad
                select l).Take(top).ToList().Select(Mapear)
                .ToList();
            return Ok<List<CateterLoteDto>>(list);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CateterLoteDto))]
        public IHttpActionResult Guardar(CateterLoteInputDto entrada)
        {
            if (entrada == null)
            {
                return BadRequest("Cuerpo de la petición vacío.");
            }
            if (string.IsNullOrWhiteSpace(entrada.Clave))
            {
                return BadRequest("La clave es obligatoria.");
            }
            if (string.IsNullOrWhiteSpace(entrada.Lote))
            {
                return BadRequest("El lote es obligatorio.");
            }
            if (entrada.CantidadInicial < 0m)
            {
                return BadRequest("La cantidad inicial no puede ser negativa.");
            }
            SqlParameter sqlParameter = new SqlParameter("@lote_id", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            try
            {
                // Los parámetros van POR NOMBRE, no por posición. El procedimiento
                // recibe @referencia entre @notas y @lote_id; cuando esta llamada
                // los mandaba posicionalmente y sin @referencia, SQL Server ligaba
                // el @lote_id OUTPUT contra @referencia y tumbaba el alta con el
                // error 8162 ("no se declaró como parámetro OUTPUT"), que al no
                // estar en el rango de validación se volvía un 500 pelón.
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.usp_cateter_alta_lote " +
                    "@clave = @clave, @lote = @lote, @caducidad = @caducidad, " +
                    "@cantidad_inicial = @cantidad_inicial, @ubicacion = @ubicacion, " +
                    "@responsable = @responsable, @observaciones = @observaciones, " +
                    "@notas = @notas, @referencia = @referencia, @lote_id = @lote_id OUTPUT",
                    new object[10]
                {
                    new SqlParameter("@clave", entrada.Clave),
                    new SqlParameter("@lote", entrada.Lote),
                    new SqlParameter("@caducidad", ((object)entrada.Caducidad) ?? DBNull.Value),
                    new SqlParameter("@cantidad_inicial", entrada.CantidadInicial),
                    new SqlParameter("@ubicacion", entrada.Ubicacion ?? "Almacén"),
                    new SqlParameter("@responsable", ((object)entrada.Responsable) ?? ((object)DBNull.Value)),
                    new SqlParameter("@observaciones", ((object)entrada.Observaciones) ?? ((object)DBNull.Value)),
                    new SqlParameter("@notas", ((object)entrada.Notas) ?? ((object)DBNull.Value)),
                    new SqlParameter("@referencia", string.IsNullOrWhiteSpace(entrada.Referencia)
                        ? (object)DBNull.Value : entrada.Referencia),
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
            // Si el procedimiento no devolvió el id, el cast directo reventaría
            // con un 500 sin explicación. Vale más decir que no se pudo.
            if (sqlParameter.Value == null || sqlParameter.Value == DBNull.Value)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "El lote no se pudo dar de alta: el procedimiento no devolvió su identificador."
                });
            }
            int loteId = (int)sqlParameter.Value;
            vw_cateter_lotes vw_cateter_lotes2 = (db.vw_cateter_lotes.AsNoTracking()).FirstOrDefault(l => l.lote_id == loteId);
            if (vw_cateter_lotes2 != null)
            {
                return Ok<CateterLoteDto>(Mapear(vw_cateter_lotes2));
            }
            return NotFound();
        }

        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(CateterLoteDto))]
        public IHttpActionResult Obtener(int id)
        {
            vw_cateter_lotes vw_cateter_lotes2 = (db.vw_cateter_lotes.AsNoTracking()).FirstOrDefault(l => l.lote_id == id);
            if (vw_cateter_lotes2 == null)
            {
                return NotFound();
            }
            return Ok<CateterLoteDto>(Mapear(vw_cateter_lotes2));
        }

        private static CateterLoteDto Mapear(vw_cateter_lotes v)
        {
            return new CateterLoteDto
            {
                Id = v.lote_id,
                ArticuloId = v.articulo_id,
                Clave = v.clave,
                Articulo = v.articulo,
                Categoria = v.categoria,
                Lote = v.lote,
                Caducidad = v.caducidad,
                DiasParaVencer = v.dias_para_vencer,
                EstadoCaducidad = v.estado_caducidad,
                EnAlmacen = v.en_almacen,
                EnStock = v.en_stock,
                Total = v.total,
                Notas = v.notas
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

