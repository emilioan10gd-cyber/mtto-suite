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
    [RoutePrefix("api/almacen-movimientos")]
    [RequiereArea("almacen")]
    public class AlmacenMovimientosController : ApiController
    {
        private readonly Model1 db = new Model1();

        // ── POST /api/almacen-movimientos ────────────────────────────────
        // Registra cualquier tipo de movimiento.
        // Las Entradas crean el lote si no existe; las Salidas/Mermas descontan.
        [HttpPost, Route("")]
        [ResponseType(typeof(AlmMovimientoDto))]
        public IHttpActionResult Registrar([FromBody] AlmRegistrarMovimientoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tipos = new[] { "Entrada", "Salida", "Merma", "Ajuste", "Transferencia" };
            if (!tipos.Contains(dto.Tipo))
                return BadRequest($"Tipo inválido. Debe ser uno de: {string.Join(", ", tipos)}.");

            // Cantidad > 0 para todos los tipos excepto Ajuste Exacto (puede ser 0)
            if (dto.Tipo != "Ajuste" && dto.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero.");

            if (dto.Tipo == "Ajuste")
            {
                var subtipos = new[] { "Agregar", "Reducir", "Exacto" };
                if (string.IsNullOrWhiteSpace(dto.AjusteSubtipo) || !subtipos.Contains(dto.AjusteSubtipo))
                    dto.AjusteSubtipo = "Exacto";
                if (dto.AjusteSubtipo != "Exacto" && dto.Cantidad <= 0)
                    return BadRequest("La cantidad debe ser mayor que cero.");
            }

            // ── Artículo ──────────────────────────────────────────────────
            var clave = (dto.ClaveArticulo ?? "").Trim().ToUpper();
            var articulo = db.alm_articulo.FirstOrDefault(a => a.clave_ssa == clave);
            if (articulo == null)
                return Content(HttpStatusCode.NotFound, new { error = $"No existe artículo con clave '{clave}'." });

            // ── Proveedor (crear si no existe) ────────────────────────────
            int? proveedorId = null;
            if (!string.IsNullOrWhiteSpace(dto.Proveedor))
            {
                var nombreProv = dto.Proveedor.Trim();
                var prov = db.alm_proveedor.FirstOrDefault(p => p.nombre == nombreProv);
                if (prov == null)
                {
                    prov = new alm_proveedor
                    {
                        nombre    = nombreProv,
                        rfc       = dto.Rfc?.Trim(),
                        activo    = true,
                        creado_en = DateTime.Now
                    };
                    db.alm_proveedor.Add(prov);
                    db.SaveChanges();
                }
                proveedorId = prov.proveedor_id;
            }

            // ── Lote (crear si no existe) ─────────────────────────────────
            var loteKey = (dto.Lote ?? "").Trim();
            var lote = db.alm_lote.FirstOrDefault(l => l.articulo_id == articulo.articulo_id && l.lote == loteKey);
            if (lote == null)
            {
                if (dto.Tipo != "Entrada" && dto.Tipo != "Ajuste")
                    return Content(HttpStatusCode.BadRequest,
                        new { error = $"El lote '{loteKey}' no existe para esta clave." });

                lote = new alm_lote
                {
                    articulo_id = articulo.articulo_id,
                    lote        = loteKey,
                    caducidad   = dto.Caducidad,
                    cantidad    = 0,
                    creado_en   = DateTime.Now
                };
                db.alm_lote.Add(lote);
                db.SaveChanges();
            }

            // ── Validar stock en salidas/mermas ───────────────────────────
            if (EsDescuento(dto.Tipo))
            {
                if (lote.cantidad < dto.Cantidad)
                    return Content(HttpStatusCode.BadRequest,
                        new { error = $"Stock insuficiente en lote '{loteKey}': disponible {lote.cantidad}, solicitado {dto.Cantidad}." });
            }

            // ── Actualizar cantidad del lote ───────────────────────────────
            if (dto.Tipo == "Entrada")
                lote.cantidad += dto.Cantidad;
            else if (EsDescuento(dto.Tipo))
                lote.cantidad -= dto.Cantidad;
            else // Ajuste
            {
                if (dto.AjusteSubtipo == "Agregar")
                    lote.cantidad += dto.Cantidad;
                else if (dto.AjusteSubtipo == "Reducir")
                {
                    if (lote.cantidad < dto.Cantidad)
                        return Content(HttpStatusCode.BadRequest,
                            new { error = $"Stock insuficiente en lote '{loteKey}': disponible {lote.cantidad}, solicitado {dto.Cantidad}." });
                    lote.cantidad -= dto.Cantidad;
                }
                else // Exacto
                    lote.cantidad = dto.Cantidad;
            }

            // Precio unitario: se captura aquí (al recibir, con la factura a
            // la vista) en vez de depender de que alguien lo edite aparte en
            // Catálogos — por eso casi nunca estaba lleno.
            if (dto.Tipo == "Entrada" && dto.PrecioUnitario.HasValue && dto.PrecioUnitario.Value > 0)
                articulo.precio_unitario = dto.PrecioUnitario.Value;

            // Actualizar fecha de artículo
            articulo.actualizado_en = DateTime.Now;

            // Actualizar caducidad si se proporcionó y no había
            if (dto.Caducidad.HasValue && lote.caducidad == null)
                lote.caducidad = dto.Caducidad;

            // ── Obtener usuario desde sesión ───────────────────────────────
            var sesion = ObtenerSesion();
            if (sesion == null)
                return Content(HttpStatusCode.Unauthorized, new { error = "Sesión inválida." });

            // ── Registrar movimiento ───────────────────────────────────────
            var mov = new alm_movimiento
            {
                tipo             = dto.Tipo,
                ajuste_subtipo   = dto.Tipo == "Ajuste" ? dto.AjusteSubtipo : null,
                lote_id          = lote.lote_id,
                articulo_id      = articulo.articulo_id,
                cantidad         = dto.Cantidad,
                fecha            = dto.Fecha ?? DateTime.Now,
                proveedor_id     = proveedorId,
                vale             = dto.Vale,
                programa         = dto.Programa,
                orden_suministro = dto.OrdenSuministro,
                area_destino     = dto.AreaDestino,
                entregado_a      = dto.EntregadoA,
                notas            = dto.Notas,
                imagen_base64    = dto.ImagenBase64,
                clues_origen           = dto.Tipo == "Entrada" ? dto.CluesOrigen : null,
                fuente_financiamiento  = dto.Tipo == "Entrada" ? dto.FuenteFinanciamiento : null,
                partida_presupuestal   = dto.Tipo == "Entrada" ? dto.PartidaPresupuestal : null,
                usuario_id       = sesion.usuario_id,
                creado_en        = DateTime.Now
            };
            db.alm_movimiento.Add(mov);
            db.SaveChanges();

            // ── Devolver detalle del movimiento creado ────────────────────
            var vista = db.vw_alm_movimientos.AsNoTracking()
                .FirstOrDefault(m => m.movimiento_id == mov.movimiento_id);

            return Created($"api/almacen-movimientos/{mov.movimiento_id}",
                vista == null ? (object)new { id = mov.movimiento_id } : MapearMovimiento(vista));
        }

        // ── POST /api/almacen-movimientos/caja ───────────────────────────
        // Registra una "venta" completa: varios articulos con un mismo
        // destino, en UNA sola transaccion. O entra todo, o no entra nada:
        // en una caja no puede quedarse media salida registrada.
        //
        // Si una linea no trae lote, se surte por FEFO (caduca primero, sale
        // primero), partiendo entre lotes si hace falta.
        [HttpPost, Route("caja")]
        [ResponseType(typeof(AlmCajaResultadoDto))]
        public IHttpActionResult RegistrarCaja([FromBody] AlmRegistrarCajaDto dto)
        {
            if (dto == null) return BadRequest("Cuerpo vacio.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tiposCaja = new[] { "Salida", "Merma", "Transferencia" };
            if (!tiposCaja.Contains(dto.Tipo))
                return BadRequest($"Tipo invalido para caja. Debe ser uno de: {string.Join(", ", tiposCaja)}.");

            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("No hay articulos que registrar.");

            var sesion = ObtenerSesion();
            if (sesion == null)
                return Content(HttpStatusCode.Unauthorized, new { error = "Sesion invalida." });

            var errores = new List<string>();
            var nuevos  = new List<alm_movimiento>();
            var fecha   = dto.Fecha ?? DateTime.Now;
            var ahora   = DateTime.Now;

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < dto.Items.Count; i++)
                    {
                        var it  = dto.Items[i] ?? new AlmCajaLineaDto();
                        var eti = $"Linea {i + 1} ({it.ClaveArticulo})";

                        if (it.Cantidad <= 0)
                        {
                            errores.Add($"{eti}: la cantidad debe ser mayor que cero.");
                            continue;
                        }

                        var clave = (it.ClaveArticulo ?? "").Trim().ToUpper();
                        var articulo = db.alm_articulo.FirstOrDefault(a => a.clave_ssa == clave);
                        if (articulo == null)
                        {
                            errores.Add($"{eti}: no existe esa clave.");
                            continue;
                        }

                        // Lotes de donde descontar: el exacto, o todos en orden FEFO.
                        List<alm_lote> candidatos;
                        var loteKey = (it.Lote ?? "").Trim();
                        if (loteKey.Length > 0)
                        {
                            var l = db.alm_lote.FirstOrDefault(
                                x => x.articulo_id == articulo.articulo_id && x.lote == loteKey);
                            if (l == null)
                            {
                                errores.Add($"{eti}: el lote '{loteKey}' no existe para esta clave.");
                                continue;
                            }
                            candidatos = new List<alm_lote> { l };
                        }
                        else
                        {
                            candidatos = db.alm_lote
                                .Where(x => x.articulo_id == articulo.articulo_id && x.cantidad > 0)
                                .ToList()
                                .OrderBy(x => x.caducidad.HasValue ? 0 : 1)   // sin fecha, al final
                                .ThenBy(x => x.caducidad)
                                .ToList();
                        }

                        // Ojo: los lotes vienen del contexto, asi que ya reflejan
                        // lo descontado por lineas anteriores del mismo carrito.
                        var disponible = candidatos.Sum(c => c.cantidad);
                        if (disponible < it.Cantidad)
                        {
                            errores.Add($"{eti}: stock insuficiente. Disponible {disponible}, solicitado {it.Cantidad}.");
                            continue;
                        }

                        var pendiente = it.Cantidad;
                        foreach (var l in candidatos)
                        {
                            if (pendiente <= 0) break;
                            if (l.cantidad <= 0) continue;

                            var toma = Math.Min(l.cantidad, pendiente);
                            l.cantidad -= toma;
                            pendiente  -= toma;

                            nuevos.Add(new alm_movimiento
                            {
                                tipo             = dto.Tipo,
                                ajuste_subtipo   = null,
                                lote_id          = l.lote_id,
                                articulo_id      = articulo.articulo_id,
                                cantidad         = toma,
                                fecha            = fecha,
                                proveedor_id     = null,
                                vale             = dto.Vale,
                                programa         = dto.Programa,
                                orden_suministro = null,
                                area_destino     = dto.AreaDestino,
                                entregado_a      = dto.EntregadoA,
                                notas            = dto.Notas,
                                imagen_base64    = dto.ImagenBase64,
                                usuario_id       = sesion.usuario_id,
                                creado_en        = ahora
                            });
                        }

                        articulo.actualizado_en = ahora;
                    }

                    // Una sola linea mala tumba toda la operacion.
                    if (errores.Count > 0)
                    {
                        tx.Rollback();
                        return Content(HttpStatusCode.BadRequest, new AlmCajaResultadoDto
                        {
                            Ok = false, Registrados = 0, Errores = errores,
                            Movimientos = new List<AlmMovimientoDto>()
                        });
                    }

                    foreach (var m in nuevos) db.alm_movimiento.Add(m);
                    db.SaveChanges();
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return Content(HttpStatusCode.InternalServerError,
                        new { error = "No se pudo registrar la operacion: " + ex.Message });
                }
            }

            var ids = nuevos.Select(m => m.movimiento_id).ToList();
            var detalle = db.vw_alm_movimientos.AsNoTracking()
                .Where(m => ids.Contains(m.movimiento_id))
                .ToList().Select(MapearMovimiento).ToList();

            return Ok(new AlmCajaResultadoDto
            {
                Ok = true,
                Registrados = detalle.Count,
                Errores = new List<string>(),
                Movimientos = detalle
            });
        }

        /// <summary>Tipos que restan existencia del lote.</summary>
        private static bool EsDescuento(string tipo) =>
            tipo == "Salida" || tipo == "Merma" || tipo == "Transferencia";

        private app_usuario ObtenerSesion()
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null || authHeader.Scheme != "Bearer") return null;
            var token = authHeader.Parameter;
            return db.app_sesion
                .Where(s => s.token == token && s.expira_en > DateTime.Now)
                .Select(s => s.app_usuario).FirstOrDefault();
        }

        private static AlmMovimientoDto MapearMovimiento(vw_alm_movimientos m) =>
            new AlmMovimientoDto
            {
                Id              = m.movimiento_id,
                Tipo            = m.tipo,
                Fecha           = m.fecha,
                Clave           = m.clave_ssa,
                Nombre          = m.nombre,
                Unidad          = m.unidad_medida,
                Lote            = m.lote,
                Caducidad       = m.caducidad,
                Cantidad        = m.cantidad,
                Vale            = m.vale,
                Programa        = m.programa,
                OrdenSuministro = m.orden_suministro,
                AreaDestino     = m.area_destino,
                EntregadoA      = m.entregado_a,
                Notas           = m.notas,
                Proveedor       = m.proveedor,
                Usuario         = m.usuario
            };

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
