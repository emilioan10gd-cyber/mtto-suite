using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/biomedico-movimientos")]
    [RequiereArea("biomedico")]
    public class BiomedicoMovimientosController : ApiController
    {
        private readonly Model1 db = new Model1();

        /// <summary>
        /// POST api/biomedico-movimientos — registra entrada/salida contra un
        /// lote. Para Entrada, si el lote no existe se crea (alta implícita).
        /// Para Salida sin lote indicado, resuelve FEFO: el lote con existencia
        /// cuya caducidad es más próxima.
        /// </summary>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(object))]
        public IHttpActionResult Registrar(BioMovimientoInputDto entrada)
        {
            if (entrada == null) return BadRequest("Cuerpo de la petición vacío.");
            if (string.IsNullOrWhiteSpace(entrada.ClaveInsumo)) return BadRequest("La clave del insumo es obligatoria.");
            if (entrada.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a cero.");
            if (string.IsNullOrWhiteSpace(entrada.Tipo)) return BadRequest("El tipo de movimiento es obligatorio.");

            var lote = entrada.Lote;
            var esSalida = entrada.Tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase)
                || entrada.Tipo.Equals("Ajuste (-)", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(lote))
            {
                if (!esSalida)
                    return BadRequest("El lote es obligatorio para registrar una entrada.");

                // FEFO: el lote con existencia cuya caducidad es más próxima.
                var insumo = db.bio_insumo.FirstOrDefault(i => i.clave == entrada.ClaveInsumo);
                if (insumo == null) return BadRequest("El insumo no existe.");

                var loteFefo = db.vw_bio_lotes.AsNoTracking()
                    .Where(l => l.insumo_id == insumo.insumo_id && l.existencia > 0)
                    .OrderBy(l => l.caducidad ?? DateTime.MaxValue)
                    .FirstOrDefault();

                if (loteFefo == null)
                    return Content(HttpStatusCode.BadRequest, new { error = "No hay existencia disponible en ningún lote de este insumo." });

                lote = loteFefo.lote;
            }

            var movimientoIdOut = new SqlParameter("@movimiento_id", SqlDbType.Int) { Direction = ParameterDirection.Output };

            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.usp_bio_registrar_movimiento @clave_insumo, @lote, @tipo_movimiento, @cantidad, @responsable, @observaciones, @caducidad, @movimiento_id OUTPUT",
                    new object[]
                    {
                        new SqlParameter("@clave_insumo", entrada.ClaveInsumo),
                        new SqlParameter("@lote", lote),
                        new SqlParameter("@tipo_movimiento", entrada.Tipo),
                        new SqlParameter("@cantidad", entrada.Cantidad),
                        new SqlParameter("@responsable", (object)entrada.Responsable ?? DBNull.Value),
                        new SqlParameter("@observaciones", (object)entrada.Observaciones ?? DBNull.Value),
                        new SqlParameter("@caducidad", (object)entrada.Caducidad ?? DBNull.Value),
                        movimientoIdOut
                    });
            }
            catch (Exception ex)
            {
                var validacion = SqlErrorHelper.ErrorDeValidacion(ex);
                if (validacion != null) return Content(HttpStatusCode.BadRequest, new { error = validacion.Message });
                throw;
            }

            return Ok(new { movimientoId = (int)movimientoIdOut.Value, loteUsado = lote });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
