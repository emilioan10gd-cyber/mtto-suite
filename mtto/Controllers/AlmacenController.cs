using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Web.Hosting;
using mtto.Models;
using mtto.Utilidades;

namespace mtto.Controllers
{
    [RoutePrefix("api/almacen")]
    [RequiereArea("almacen")]
    public class AlmacenController : ApiController
    {
        private readonly Model1 db = new Model1();

        // ── GET /api/almacen ─────────────────────────────────────────────
        [HttpGet, Route("")]
        [ResponseType(typeof(PaginaDto<AlmArticuloDto>))]
        public IHttpActionResult Listar(string q = null, string nivel = null,
            string programa = null,
            string caducidad = null,       // porVencer | vencidos
            bool soloActivos = true, bool soloConExistencia = false,
            bool soloBajoMinimo = false, bool soloCpm = false,
            string ordenarPor = "clave",   // clave | stockDesc | stockAsc | caducidad | nombre
            int pagina = 1, int tamano = 60)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 500) tamano = 60;

            IQueryable<vw_alm_inventario> consulta = db.vw_alm_inventario.AsNoTracking();
            if (soloActivos)           consulta = consulta.Where(a => a.activo);
            if (!string.IsNullOrWhiteSpace(q))
                consulta = consulta.Where(a => a.nombre.Contains(q) || a.clave_ssa.Contains(q));
            if (!string.IsNullOrWhiteSpace(nivel))
                consulta = consulta.Where(a => a.nivel == nivel);
            if (!string.IsNullOrWhiteSpace(programa))
                consulta = consulta.Where(a => a.programa == programa.Trim());
            if (soloConExistencia)     consulta = consulta.Where(a => a.stock_actual > 0);
            if (soloBajoMinimo)        consulta = consulta.Where(a => a.nivel == "Bajo minimo" || a.nivel == "Sin stock");
            if (soloCpm)               consulta = consulta.Where(a => a.es_cpm);
            if (caducidad == "critico")
            {
                var ids = db.vw_alm_lotes.AsNoTracking()
                    .Where(l => l.cantidad > 0 && l.estado_caducidad == "Critico")
                    .Select(l => l.articulo_id).Distinct();
                consulta = consulta.Where(a => ids.Contains(a.articulo_id));
            }
            else if (caducidad == "porVencer")
            {
                // Crítico + Alerta (≤90 días, acción inmediata)
                var ids = db.vw_alm_lotes.AsNoTracking()
                    .Where(l => l.cantidad > 0 && (l.estado_caducidad == "Critico" || l.estado_caducidad == "Alerta"))
                    .Select(l => l.articulo_id).Distinct();
                consulta = consulta.Where(a => ids.Contains(a.articulo_id));
            }
            else if (caducidad == "vigilancia")
            {
                // 91-180 días — NOM-059: monitoreo activo
                var ids = db.vw_alm_lotes.AsNoTracking()
                    .Where(l => l.cantidad > 0 && l.estado_caducidad == "Vigilancia")
                    .Select(l => l.articulo_id).Distinct();
                consulta = consulta.Where(a => ids.Contains(a.articulo_id));
            }
            else if (caducidad == "vencidos")
            {
                var ids = db.vw_alm_lotes.AsNoTracking()
                    .Where(l => l.cantidad > 0 && l.estado_caducidad == "Vencido")
                    .Select(l => l.articulo_id).Distinct();
                consulta = consulta.Where(a => ids.Contains(a.articulo_id));
            }

            int total = consulta.Count();

            IQueryable<vw_alm_inventario> ordered;
            switch (ordenarPor)
            {
                case "stockDesc":  ordered = consulta.OrderByDescending(a => a.stock_actual).ThenBy(a => a.clave_ssa); break;
                case "stockAsc":   ordered = consulta.OrderBy(a => a.stock_actual).ThenBy(a => a.clave_ssa); break;
                case "caducidad":  ordered = consulta.OrderBy(a => a.caducidad_proxima == null).ThenBy(a => a.caducidad_proxima).ThenBy(a => a.clave_ssa); break;
                case "nombre":     ordered = consulta.OrderBy(a => a.nombre); break;
                default:           ordered = consulta.OrderBy(a => a.clave_ssa); break;
            }

            var datos = ordered.Skip((pagina - 1) * tamano).Take(tamano)
                .ToList().Select(MapearInventario).ToList();

            return Ok(new PaginaDto<AlmArticuloDto>
            {
                Total = total, Pagina = pagina, Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)total / tamano),
                Datos = datos
            });
        }

        // ── GET /api/almacen/programas ───────────────────────────────────
        [HttpGet, Route("programas")]
        public IHttpActionResult Programas()
        {
            var lista = db.alm_articulo.AsNoTracking()
                .Where(a => a.activo && a.programa != null && a.programa != "")
                .Select(a => a.programa).Distinct().OrderBy(p => p).ToList();
            return Ok(lista);
        }

        // ── Catálogo ligero: valores ya usados de fuente de financiamiento y
        // partida presupuestal, para sugerir en vez de teclear cada vez y
        // evitar variantes con typo (p.ej. "U013" vs "U0013"). No son tablas
        // aparte porque siguen siendo texto libre — "agregar uno nuevo" es
        // simplemente escribirlo en un movimiento de Entrada.
        [HttpGet, Route("fuentes-financiamiento")]
        public IHttpActionResult FuentesFinanciamiento()
        {
            var lista = db.alm_movimiento.AsNoTracking()
                .Where(m => m.fuente_financiamiento != null && m.fuente_financiamiento != "")
                .Select(m => m.fuente_financiamiento).Distinct().OrderBy(x => x).ToList();
            return Ok(lista);
        }

        [HttpGet, Route("partidas-presupuestales")]
        public IHttpActionResult PartidasPresupuestales()
        {
            var lista = db.alm_movimiento.AsNoTracking()
                .Where(m => m.partida_presupuestal != null && m.partida_presupuestal != "")
                .Select(m => m.partida_presupuestal).Distinct().OrderBy(x => x).ToList();
            return Ok(lista);
        }

        // Entidad federativa y CLUES destino NO tienen histórico: viven como
        // valor único en alm_config (fila de configuración institucional que
        // GuardarConfig sobreescribe cada vez), no por movimiento — por eso
        // no hay "distinct" que ofrecer para ellos, a diferencia de Fuente,
        // Partida y CLUES origen. Se siguen precargando vía GET /config.

        // Único de los tres datos institucionales que sí varía por remisión
        // (distintos orígenes de suministro) y sí tiene su propia columna
        // en alm_movimiento — mismo criterio que Fuente/Partida.
        [HttpGet, Route("clues-origen")]
        public IHttpActionResult CluesOrigen()
        {
            var lista = db.alm_movimiento.AsNoTracking()
                .Where(m => m.clues_origen != null && m.clues_origen != "")
                .Select(m => m.clues_origen).Distinct().OrderBy(x => x).ToList();
            return Ok(lista);
        }

        // ── GET /api/almacen/dashboard ───────────────────────────────────
        [HttpGet, Route("dashboard")]
        [ResponseType(typeof(AlmDashboardDto))]
        public IHttpActionResult Dashboard()
        {
            var inv = db.vw_alm_inventario.AsNoTracking().Where(a => a.activo).ToList();
            var lotes = db.vw_alm_lotes.AsNoTracking().ToList();
            var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var movsMes = db.alm_movimiento.AsNoTracking().Count(m => m.fecha >= inicioMes);

            var salidasMes = db.vw_alm_movimientos.AsNoTracking()
                .Where(m => m.tipo == "Salida" && m.fecha >= inicioMes
                       && m.area_destino != null && m.area_destino != "")
                .GroupBy(m => m.area_destino)
                .Select(g => new AlmConsumoAreaDto
                {
                    Area = g.Key,
                    TotalSalidas   = g.Sum(x => x.cantidad),
                    NumMovimientos = g.Count()
                })
                .OrderByDescending(x => x.TotalSalidas)
                .Take(8)
                .ToList();

            var ultimosMov = db.vw_alm_movimientos.AsNoTracking()
                .OrderByDescending(m => m.creado_en)
                .Take(10)
                .ToList()
                .Select(m => new AlmMovimientoDto
                {
                    Id = m.movimiento_id, Tipo = m.tipo, Fecha = m.fecha,
                    Clave = m.clave_ssa, Nombre = m.nombre, Unidad = m.unidad_medida,
                    Lote = m.lote, Cantidad = m.cantidad,
                    AreaDestino = m.area_destino, Notas = m.notas, Usuario = m.usuario
                })
                .ToList();

            // NOM-241-SSA1-2012 / NOM-059-SSA1-2015: umbrales 30/90/180 días
            return Ok(new AlmDashboardDto
            {
                TotalArticulos        = inv.Count,
                ArticulosSinStock     = inv.Count(a => a.nivel == "Sin stock"),
                ArticulosBajoMinimo   = inv.Count(a => a.nivel == "Bajo minimo"),
                ArticulosNormal       = inv.Count(a => a.nivel == "Normal"),
                ArticulosSobreMaximo  = inv.Count(a => a.nivel == "Sobre maximo"),
                LotesCriticos         = lotes.Count(l => l.estado_caducidad == "Critico"    && l.cantidad > 0),
                LotesPorVencer90      = lotes.Count(l => l.estado_caducidad == "Alerta"     && l.cantidad > 0),
                LotesPorVencer180     = lotes.Count(l => l.estado_caducidad == "Vigilancia" && l.cantidad > 0),
                LotesVencidos         = lotes.Count(l => l.estado_caducidad == "Vencido"    && l.cantidad > 0),
                ValorTotalInventario  = inv.Sum(a => a.valor_total),
                MovimientosMes        = movsMes,
                ConsumoMesPorArea     = salidasMes,
                UltimosMovimientos    = ultimosMov
            });
        }

        // ── GET /api/almacen/{id} ────────────────────────────────────────
        [HttpGet, Route("{id:int}")]
        [ResponseType(typeof(AlmArticuloDto))]
        public IHttpActionResult Obtener(int id)
        {
            var a = db.vw_alm_inventario.AsNoTracking().FirstOrDefault(x => x.articulo_id == id);
            if (a == null) return NotFound();
            var dto = MapearInventario(a);
            // imagen_base64 no está en la vista (evitar payload gigante en listados)
            var img = db.alm_articulo.AsNoTracking()
                .Where(x => x.articulo_id == id)
                .Select(x => x.imagen_base64)
                .FirstOrDefault();
            dto.ImagenBase64 = img;
            return Ok(dto);
        }

        // ── GET /api/almacen/por-clave?clave=060.030.0073 ────────────────
        [HttpGet, Route("por-clave")]
        [ResponseType(typeof(AlmArticuloDto))]
        public IHttpActionResult PorClave(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave)) return BadRequest("Falta clave.");
            var a = db.vw_alm_inventario.AsNoTracking()
                .FirstOrDefault(x => x.clave_ssa == clave.Trim());
            if (a == null) return NotFound();
            return Ok(MapearInventario(a));
        }

        // ── GET /api/almacen/{id}/lotes ──────────────────────────────────
        [HttpGet, Route("{id:int}/lotes")]
        [ResponseType(typeof(List<AlmLoteDto>))]
        public IHttpActionResult Lotes(int id, bool soloConExistencia = false)
        {
            var consulta = db.vw_alm_lotes.AsNoTracking().Where(l => l.articulo_id == id);
            if (soloConExistencia) consulta = consulta.Where(l => l.cantidad > 0);
            var datos = consulta.OrderBy(l => l.caducidad).ToList().Select(MapearLote).ToList();
            return Ok(datos);
        }

        // ── GET /api/almacen/lotes/proximos-caducar ───────────────────────
        [HttpGet, Route("lotes/proximos-caducar")]
        [ResponseType(typeof(List<AlmLoteDto>))]
        public IHttpActionResult ProximosCaducar(int dias = 180)
        {
            var fecha = DateTime.Today.AddDays(dias);
            var datos = db.vw_alm_lotes.AsNoTracking()
                .Where(l => l.cantidad > 0 && l.caducidad != null && l.caducidad <= fecha)
                .OrderBy(l => l.caducidad).ToList().Select(MapearLote).ToList();
            return Ok(datos);
        }

        // ── GET /api/almacen/movimientos ─────────────────────────────────
        [HttpGet, Route("movimientos")]
        [ResponseType(typeof(PaginaDto<AlmMovimientoDto>))]
        public IHttpActionResult Movimientos(string q = null, string tipo = null,
            int? articuloId = null, DateTime? desde = null, DateTime? hasta = null,
            string area = null, string entregadoA = null,
            int pagina = 1, int tamano = 60)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 500) tamano = 60;

            IQueryable<vw_alm_movimientos> consulta = db.vw_alm_movimientos.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(tipo)) consulta = consulta.Where(m => m.tipo == tipo);
            if (articuloId.HasValue)              consulta = consulta.Where(m => m.articulo_id == articuloId);
            if (!string.IsNullOrWhiteSpace(q))
                consulta = consulta.Where(m => m.nombre.Contains(q) || m.clave_ssa.Contains(q) || m.lote.Contains(q));
            if (desde.HasValue) consulta = consulta.Where(m => m.fecha >= desde);
            if (hasta.HasValue) consulta = consulta.Where(m => m.fecha < DbFunctions.AddDays(hasta, 1));
            if (!string.IsNullOrWhiteSpace(area))
                consulta = consulta.Where(m => m.area_destino == area.Trim());
            if (!string.IsNullOrWhiteSpace(entregadoA))
                consulta = consulta.Where(m => m.entregado_a.Contains(entregadoA));

            int total = consulta.Count();
            var datos = consulta.OrderByDescending(m => m.fecha).ThenByDescending(m => m.movimiento_id)
                .Skip((pagina - 1) * tamano).Take(tamano).ToList().Select(MapearMovimiento).ToList();

            return Ok(new PaginaDto<AlmMovimientoDto>
            {
                Total = total, Pagina = pagina, Tamano = tamano,
                TotalPaginas = (int)Math.Ceiling((double)total / tamano),
                Datos = datos
            });
        }

        // ── GET /api/almacen/areas ───────────────────────────────────────
        // Areas de destino con la gente que ha recibido en cada una. Se saca
        // del historial, asi el catalogo se llena solo conforme se despacha
        // y no hay que mantener una tabla aparte.
        [HttpGet, Route("areas")]
        [ResponseType(typeof(List<AlmAreaDto>))]
        public IHttpActionResult Areas()
        {
            var pares = db.alm_movimiento.AsNoTracking()
                .Where(m => m.area_destino != null && m.area_destino != "")
                .Select(m => new { m.area_destino, m.entregado_a })
                .Distinct()
                .ToList();

            var datos = pares
                .GroupBy(p => p.area_destino)
                .OrderBy(g => g.Key)
                .Select(g => new AlmAreaDto
                {
                    Area = g.Key,
                    Receptores = g.Select(x => x.entregado_a)
                                  .Where(x => !string.IsNullOrWhiteSpace(x))
                                  .Distinct()
                                  .OrderBy(x => x)
                                  .ToList()
                })
                .ToList();

            return Ok(datos);
        }

        // ── GET /api/almacen/movimientos/exportar ────────────────────────
        [HttpGet, Route("movimientos/exportar")]
        public HttpResponseMessage ExportarMovimientos(string q = null, string tipo = null,
            DateTime? desde = null, DateTime? hasta = null,
            string area = null, string entregadoA = null)
        {
            IQueryable<vw_alm_movimientos> consulta = db.vw_alm_movimientos.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(tipo)) consulta = consulta.Where(m => m.tipo == tipo);
            if (!string.IsNullOrWhiteSpace(q))
                consulta = consulta.Where(m => m.nombre.Contains(q) || m.clave_ssa.Contains(q) || m.lote.Contains(q));
            if (desde.HasValue) consulta = consulta.Where(m => m.fecha >= desde);
            if (hasta.HasValue) consulta = consulta.Where(m => m.fecha < DbFunctions.AddDays(hasta, 1));
            if (!string.IsNullOrWhiteSpace(area))
                consulta = consulta.Where(m => m.area_destino == area.Trim());
            if (!string.IsNullOrWhiteSpace(entregadoA))
                consulta = consulta.Where(m => m.entregado_a.Contains(entregadoA));

            var movs = consulta
                .OrderByDescending(m => m.fecha).ThenByDescending(m => m.movimiento_id)
                .ToList();

            using (var paquete = new ExcelPackage())
            {
                var columnas = new (string, double)[]
                {
                    ("Fecha", 12), ("Tipo", 14), ("Clave SSA", 14), ("Artículo", 32),
                    ("Unidad", 10), ("Lote", 14), ("Caducidad", 12), ("Cantidad", 11),
                    ("Entregado a", 22), ("Área", 18), ("Programa", 14),
                    ("Vale", 14), ("Proveedor", 18), ("Usuario", 14)
                };

                var culturaEs = new CultureInfo("es-MX");
                var nombreMes = culturaEs.TextInfo.ToTitleCase(DateTime.Now.ToString("MMMM", culturaEs));
                var subtitulo = $"Mes de {nombreMes} {DateTime.Now.Year} (ALMACÉN CENTRAL)";

                var hoja = ExcelExportHelper.CrearHojaConEncabezado(
                    paquete, "Movimientos", "HISTORIAL DE TRANSACCIONES", subtitulo, "#5b4fcf", columnas);

                var fila = 5;
                foreach (var m in movs)
                {
                    hoja.Cells[fila,  2].Value = m.fecha;
                    hoja.Cells[fila,  3].Value = m.tipo;
                    hoja.Cells[fila,  4].Value = m.clave_ssa;
                    hoja.Cells[fila,  5].Value = m.nombre;
                    hoja.Cells[fila,  6].Value = m.unidad_medida;
                    hoja.Cells[fila,  7].Value = m.lote;
                    hoja.Cells[fila,  8].Value = m.caducidad;
                    hoja.Cells[fila,  9].Value = m.cantidad;
                    hoja.Cells[fila, 10].Value = m.entregado_a ?? "—";
                    hoja.Cells[fila, 11].Value = m.area_destino ?? "—";
                    hoja.Cells[fila, 12].Value = m.programa ?? "—";
                    hoja.Cells[fila, 13].Value = m.vale ?? "—";
                    hoja.Cells[fila, 14].Value = m.proveedor ?? "—";
                    hoja.Cells[fila, 15].Value = m.usuario;

                    hoja.Cells[fila, 2].Style.Numberformat.Format = "dd/mm/yyyy";
                    hoja.Cells[fila, 8].Style.Numberformat.Format = "dd/mm/yyyy";
                    hoja.Cells[fila, 9].Style.Numberformat.Format = "#,##0";

                    ExcelExportHelper.EstiloFilaDatos(hoja.Cells[fila, 2, fila, 15]);
                    fila++;
                }

                var bytes = paquete.GetAsByteArray();
                var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                respuesta.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                respuesta.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"Almacen-Movimientos_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };
                return respuesta;
            }
        }

        // ── GET /api/almacen/claves?q=... ────────────────────────────────
        // Busca por clave o nombre, y también por número de lote: la caja de
        // salida necesita poder encontrar un artículo escribiendo el lote
        // que trae la etiqueta, no solo la clave.
        [HttpGet, Route("claves")]
        [ResponseType(typeof(List<AlmClaveDto>))]
        public IHttpActionResult BuscarClaves(string q = null, int limite = 50)
        {
            IQueryable<vw_alm_inventario> consulta = db.vw_alm_inventario.AsNoTracking()
                .Where(a => a.activo);

            var loteCoincidentePorArticulo = new Dictionary<int, string>();
            var loteCoincidenteCaducidadPorArticulo = new Dictionary<int, DateTime?>();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var lotesCoincidentes = db.vw_alm_lotes.AsNoTracking()
                    .Where(l => l.cantidad > 0 && l.lote.Contains(q))
                    .Select(l => new { l.articulo_id, l.lote, l.caducidad })
                    .Take(200)
                    .ToList();
                foreach (var l in lotesCoincidentes)
                    if (!loteCoincidentePorArticulo.ContainsKey(l.articulo_id))
                    {
                        loteCoincidentePorArticulo[l.articulo_id] = l.lote;
                        loteCoincidenteCaducidadPorArticulo[l.articulo_id] = l.caducidad;
                    }

                var idsPorLote = loteCoincidentePorArticulo.Keys.ToList();

                consulta = consulta.Where(a =>
                    a.clave_ssa.Contains(q) || a.nombre.Contains(q) || idsPorLote.Contains(a.articulo_id));
            }

            var datos = consulta
                .OrderBy(a => a.clave_ssa)
                .Take(limite)
                .ToList()
                .Select(a => new AlmClaveDto
                {
                    ArticuloId = a.articulo_id,
                    Clave = a.clave_ssa,
                    Nombre = a.nombre,
                    Unidad = a.unidad_medida,
                    LoteCoincidente = loteCoincidentePorArticulo.ContainsKey(a.articulo_id)
                        ? loteCoincidentePorArticulo[a.articulo_id] : null,
                    LoteCoincidenteCaducidad = loteCoincidenteCaducidadPorArticulo.ContainsKey(a.articulo_id)
                        ? loteCoincidenteCaducidadPorArticulo[a.articulo_id] : null
                })
                .ToList();

            return Ok(datos);
        }

        // ── GET /api/almacen/proveedores ──────────────────────────────────
        [HttpGet, Route("proveedores")]
        [ResponseType(typeof(List<AlmProveedorDto>))]
        public IHttpActionResult Proveedores(string q = null)
        {
            var consulta = db.alm_proveedor.AsNoTracking().Where(p => p.activo);
            if (!string.IsNullOrWhiteSpace(q)) consulta = consulta.Where(p => p.nombre.Contains(q));
            var datos = consulta.OrderBy(p => p.nombre).Select(p => new AlmProveedorDto
            {
                Id = p.proveedor_id, Nombre = p.nombre, Rfc = p.rfc
            }).ToList();
            return Ok(datos);
        }

        // ── POST /api/almacen/proveedores ─────────────────────────────────
        [HttpPost, Route("proveedores")]
        [ResponseType(typeof(AlmProveedorDto))]
        public IHttpActionResult CrearProveedor([FromBody] dynamic body)
        {
            if (body == null || string.IsNullOrWhiteSpace((string)body.nombre))
                return BadRequest("Nombre del proveedor requerido.");

            string nombre = ((string)body.nombre).Trim();
            string rfc = ((string)(body.rfc ?? "")).Trim();

            if (db.alm_proveedor.Any(p => p.nombre.ToLower() == nombre.ToLower()))
                return Content(HttpStatusCode.Conflict, new { error = "Ya existe un proveedor con ese nombre." });

            var proveedor = new alm_proveedor
            {
                nombre = nombre,
                rfc = string.IsNullOrWhiteSpace(rfc) ? null : rfc,
                activo = true
            };

            db.alm_proveedor.Add(proveedor);
            db.SaveChanges();

            return Created($"api/almacen/proveedores/{proveedor.proveedor_id}", new AlmProveedorDto
            {
                Id = proveedor.proveedor_id,
                Nombre = proveedor.nombre,
                Rfc = proveedor.rfc
            });
        }

        // ── POST /api/almacen ────────────────────────────────────────────
        [HttpPost, Route("")]
        [ResponseType(typeof(AlmArticuloDto))]
        public IHttpActionResult Crear([FromBody] AlmGuardarArticuloDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (db.alm_articulo.Any(a => a.clave_ssa == dto.Clave.Trim()))
                return Content(HttpStatusCode.Conflict, new { error = "Ya existe un artículo con esa clave." });

            var ahora = DateTime.Now;
            var art = new alm_articulo
            {
                clave_ssa      = dto.Clave.Trim().ToUpper(),
                nombre         = dto.Nombre.Trim(),
                unidad_medida  = dto.Unidad.Trim(),
                es_cpm         = dto.EsCpm,
                precio_unitario= dto.PrecioUnitario,
                stock_minimo   = dto.StockMinimo,
                stock_maximo   = dto.StockMaximo,
                programa       = dto.Programa,
                imagen_base64  = dto.ImagenBase64,
                activo         = true,
                creado_en      = ahora,
                actualizado_en = ahora
            };
            db.alm_articulo.Add(art);
            db.SaveChanges();

            var vista = db.vw_alm_inventario.AsNoTracking()
                .FirstOrDefault(v => v.articulo_id == art.articulo_id);
            return Created($"api/almacen/{art.articulo_id}", MapearInventario(vista));
        }

        // ── PUT /api/almacen/{id} ────────────────────────────────────────
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Actualizar(int id, [FromBody] AlmGuardarArticuloDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var art = db.alm_articulo.Find(id);
            if (art == null) return NotFound();
            if (db.alm_articulo.Any(a => a.clave_ssa == dto.Clave.Trim() && a.articulo_id != id))
                return Content(HttpStatusCode.Conflict, new { error = "Esa clave ya está en uso." });

            art.clave_ssa       = dto.Clave.Trim().ToUpper();
            art.nombre          = dto.Nombre.Trim();
            art.unidad_medida   = dto.Unidad.Trim();
            art.es_cpm          = dto.EsCpm;
            art.precio_unitario = dto.PrecioUnitario;
            art.stock_minimo    = dto.StockMinimo;
            art.stock_maximo    = dto.StockMaximo;
            art.programa        = dto.Programa;
            if (dto.ImagenBase64 != null)           // null = no cambiar; "" = borrar imagen
                art.imagen_base64 = dto.ImagenBase64 == "" ? null : dto.ImagenBase64;
            art.actualizado_en  = DateTime.Now;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // ── PUT /api/almacen/{id}/stock-limites ──────────────────────────
        [HttpPut, Route("{id:int}/stock-limites")]
        public IHttpActionResult ActualizarLimites(int id,
            [FromBody] dynamic body)
        {
            var art = db.alm_articulo.Find(id);
            if (art == null) return NotFound();
            art.stock_minimo   = (decimal)(body.stockMinimo ?? 0);
            art.stock_maximo   = (decimal?)body.stockMaximo;
            art.actualizado_en = DateTime.Now;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // ── PUT /api/almacen/{id}/desactivar ─────────────────────────────
        [HttpPut, Route("{id:int}/desactivar")]
        public IHttpActionResult Desactivar(int id)
        {
            var art = db.alm_articulo.Find(id);
            if (art == null) return NotFound();
            art.activo         = false;
            art.actualizado_en = DateTime.Now;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // ── Mapeos ───────────────────────────────────────────────────────
        private static AlmArticuloDto MapearInventario(vw_alm_inventario v) =>
            v == null ? null : new AlmArticuloDto
            {
                Id                 = v.articulo_id,
                Clave              = v.clave_ssa,
                Nombre             = v.nombre,
                Unidad             = v.unidad_medida,
                EsCpm              = v.es_cpm,
                PrecioUnitario     = v.precio_unitario,
                StockMinimo        = v.stock_minimo,
                StockMaximo        = v.stock_maximo,
                Programa           = v.programa,
                Activo             = v.activo,
                StockActual        = v.stock_actual,
                LotesConExistencia = v.lotes_con_existencia,
                CaducidadProxima   = v.caducidad_proxima,
                ValorTotal         = v.valor_total,
                Nivel              = v.nivel,
                ActualizadoEn      = v.actualizado_en
            };

        private static AlmLoteDto MapearLote(vw_alm_lotes l) =>
            new AlmLoteDto
            {
                Id              = l.lote_id,
                ArticuloId      = l.articulo_id,
                Clave           = l.clave_ssa,
                Nombre          = l.nombre,
                Unidad          = l.unidad_medida,
                Lote            = l.lote,
                Caducidad       = l.caducidad,
                Cantidad        = l.cantidad,
                DiasParaCaducar = l.dias_para_caducar,
                EstadoCaducidad = l.estado_caducidad
            };

        private static AlmMovimientoDto MapearMovimiento(vw_alm_movimientos m) =>
            new AlmMovimientoDto
            {
                Id              = m.movimiento_id,
                ArticuloId      = m.articulo_id,
                Tipo            = m.tipo,
                AjusteSubtipo   = m.ajuste_subtipo,
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
                Usuario         = m.usuario,
                ImagenBase64    = m.imagen_base64
            };

        /// <summary>
        /// Exporta todo lo que cumpla el filtro actual a Excel.
        /// </summary>
        [HttpGet, Route("exportar")]
        public HttpResponseMessage Exportar(string q = null, string nivel = null, string programa = null,
            string caducidad = null, bool soloActivos = true, bool soloConExistencia = false, bool soloCpm = false)
        {
            // Aplicar filtros (misma lógica que Listar)
            IQueryable<vw_alm_inventario> consulta = db.vw_alm_inventario.AsNoTracking();
            if (soloActivos) consulta = consulta.Where(a => a.activo);
            if (!string.IsNullOrWhiteSpace(q))
                consulta = consulta.Where(a => a.nombre.Contains(q) || a.clave_ssa.Contains(q));
            if (!string.IsNullOrWhiteSpace(nivel))
                consulta = consulta.Where(a => a.nivel == nivel);
            if (!string.IsNullOrWhiteSpace(programa))
                consulta = consulta.Where(a => a.programa == programa.Trim());
            if (soloConExistencia) consulta = consulta.Where(a => a.stock_actual > 0);
            if (soloCpm) consulta = consulta.Where(a => a.es_cpm);

            // Caducidad filter
            if (!string.IsNullOrWhiteSpace(caducidad))
            {
                var estadosCaducidad = caducidad == "critico" ? new[] { "Critico" } :
                                      caducidad == "porVencer" ? new[] { "Critico", "Alerta" } :
                                      caducidad == "vigilancia" ? new[] { "Vigilancia" } :
                                      caducidad == "vencidos" ? new[] { "Vencido" } :
                                      new[] { "" };
                var ids = db.vw_alm_lotes.AsNoTracking()
                    .Where(l => l.cantidad > 0 && estadosCaducidad.Contains(l.estado_caducidad))
                    .Select(l => l.articulo_id).Distinct();
                consulta = consulta.Where(a => ids.Contains(a.articulo_id));
            }

            var articulos = consulta.OrderBy(a => a.clave_ssa).ToList().Select(MapearInventario).ToList();

            using (var paquete = new ExcelPackage())
            {
                var columnas = new (string, double)[]
                {
                    ("Clave SSA", 14), ("Artículo", 32), ("Unidad", 12),
                    ("Stock Actual", 12), ("Mín", 8), ("Máx", 8), ("Nivel", 14),
                    ("Programa", 16), ("CPM", 6), ("Valor Total", 14), ("Actualizado", 14)
                };

                var culturaEs = new CultureInfo("es-MX");
                var nombreMes = culturaEs.TextInfo.ToTitleCase(DateTime.Now.ToString("MMMM", culturaEs));
                var subtitulo = $"Mes de {nombreMes} {DateTime.Now.Year} (ALMACÉN CENTRAL)";

                var hoja = ExcelExportHelper.CrearHojaConEncabezado(
                    paquete, "Inventario", "INVENTARIO ALMACÉN CENTRAL", subtitulo, "#5b4fcf", columnas);

                var fila = 5;
                foreach (var a in articulos)
                {
                    hoja.Cells[fila, 2].Value = a.Clave;
                    hoja.Cells[fila, 3].Value = a.Nombre;
                    hoja.Cells[fila, 4].Value = a.Unidad;
                    hoja.Cells[fila, 5].Value = a.StockActual;
                    hoja.Cells[fila, 6].Value = a.StockMinimo;
                    hoja.Cells[fila, 7].Value = a.StockMaximo;
                    hoja.Cells[fila, 8].Value = a.Nivel ?? "—";
                    hoja.Cells[fila, 9].Value = a.Programa ?? "—";
                    hoja.Cells[fila, 10].Value = a.EsCpm ? "Sí" : "No";
                    hoja.Cells[fila, 11].Value = a.ValorTotal;
                    hoja.Cells[fila, 12].Value = a.ActualizadoEn;

                    hoja.Cells[fila, 5, fila, 7].Style.Numberformat.Format = "#,##0";
                    hoja.Cells[fila, 11].Style.Numberformat.Format = "$#,##0.00";
                    hoja.Cells[fila, 12].Style.Numberformat.Format = "mm-dd-yy";

                    ExcelExportHelper.EstiloFilaDatos(hoja.Cells[fila, 2, fila, 12]);
                    fila++;
                }

                var bytes = paquete.GetAsByteArray();
                var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                respuesta.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                respuesta.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"Almacen-Inventario_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };
                return respuesta;
            }
        }

        // ── GET /api/almacen/proveedores/{id}/rfc ──────────────────────────
        [HttpGet, Route("proveedores/{id:int}/rfc")]
        [ResponseType(typeof(AlmProveedorConRfcDto))]
        public IHttpActionResult ProveedorConRfc(int id)
        {
            var proveedor = db.alm_proveedor.AsNoTracking().FirstOrDefault(p => p.proveedor_id == id);
            if (proveedor == null) return NotFound();

            var rfcs = db.alm_proveedor_rfc.AsNoTracking()
                .Where(r => r.proveedor_id == id)
                .OrderByDescending(r => r.es_activo)
                .ThenByDescending(r => r.creado_en)
                .ToList();

            var dto = new AlmProveedorConRfcDto
            {
                Id = proveedor.proveedor_id,
                Nombre = proveedor.nombre,
                RfcActivo = rfcs.FirstOrDefault(r => r.es_activo)?.rfc,
                Rfc = rfcs.Select(r => new AlmProveedorRfcDto
                {
                    Id = r.proveedor_rfc_id,
                    ProveedorId = r.proveedor_id,
                    Rfc = r.rfc,
                    EsActivo = r.es_activo
                }).ToList()
            };
            return Ok(dto);
        }

        // ── POST /api/almacen/proveedores/{id}/rfc ──────────────────────────
        [HttpPost, Route("proveedores/{id:int}/rfc")]
        [ResponseType(typeof(AlmProveedorRfcDto))]
        public IHttpActionResult AgregarRfcProveedor(int id, [FromBody] dynamic body)
        {
            var proveedor = db.alm_proveedor.Find(id);
            if (proveedor == null) return NotFound();

            string nuevoRfc = ((string)(body.rfc ?? "")).Trim();
            if (string.IsNullOrWhiteSpace(nuevoRfc) || nuevoRfc.Length < 8)
                return BadRequest("RFC debe tener por lo menos 8 caracteres.");

            if (db.alm_proveedor_rfc.Any(r => r.proveedor_id == id && r.rfc == nuevoRfc))
                return Content(HttpStatusCode.Conflict, new { error = "Este RFC ya existe para este proveedor." });

            var rfcRecord = new alm_proveedor_rfc
            {
                proveedor_id = id,
                rfc = nuevoRfc,
                es_activo = true  // por defecto activo
            };

            db.alm_proveedor_rfc.Add(rfcRecord);
            db.SaveChanges();

            return Created($"api/almacen/proveedores/{id}/rfc", new AlmProveedorRfcDto
            {
                Id = rfcRecord.proveedor_rfc_id,
                ProveedorId = rfcRecord.proveedor_id,
                Rfc = rfcRecord.rfc,
                EsActivo = rfcRecord.es_activo
            });
        }

        // ── PUT /api/almacen/proveedores/{id}/rfc/{rfcId}/activar ─────────
        [HttpPut, Route("proveedores/{id:int}/rfc/{rfcId:int}/activar")]
        public IHttpActionResult ActivarRfc(int id, int rfcId)
        {
            var rfc = db.alm_proveedor_rfc.FirstOrDefault(r => r.proveedor_rfc_id == rfcId && r.proveedor_id == id);
            if (rfc == null) return NotFound();

            // Desactivar todos los RFC de este proveedor
            var otrosRfc = db.alm_proveedor_rfc.Where(r => r.proveedor_id == id && r.proveedor_rfc_id != rfcId).ToList();
            foreach (var otro in otrosRfc) otro.es_activo = false;

            // Activar el seleccionado
            rfc.es_activo = true;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // ── PUT /api/almacen/proveedores/{id}/rfc/{rfcId}/inactivar ──────
        [HttpPut, Route("proveedores/{id:int}/rfc/{rfcId:int}/inactivar")]
        public IHttpActionResult InactivarRfc(int id, int rfcId)
        {
            var rfc = db.alm_proveedor_rfc.FirstOrDefault(r => r.proveedor_rfc_id == rfcId && r.proveedor_id == id);
            if (rfc == null) return NotFound();

            rfc.es_activo = false;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // ── GET /api/almacen/config ─────────────────────────────────────
        // Datos institucionales fijos (entidad federativa, CLUES destino) que
        // necesita el formato SSA de inventario semanal — se capturan una
        // vez aquí en vez de repetirlos en cada entrada.
        [HttpGet, Route("config")]
        public IHttpActionResult ObtenerConfig()
        {
            var cfg = db.alm_config.AsNoTracking().FirstOrDefault();
            return Ok(new
            {
                EntidadFederativa = cfg?.entidad_federativa,
                CluesDestino      = cfg?.clues_destino,
                CluesOrigen       = cfg?.clues_origen
            });
        }

        [HttpPut, Route("config")]
        public IHttpActionResult GuardarConfig([FromBody] dynamic body)
        {
            var cfg = db.alm_config.FirstOrDefault();
            if (cfg == null) { cfg = new alm_config(); db.alm_config.Add(cfg); }
            cfg.entidad_federativa = (string)body.entidadFederativa;
            cfg.clues_destino      = (string)body.cluesDestino;
            cfg.clues_origen       = (string)body.cluesOrigen;
            cfg.actualizado_en     = DateTime.Now;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // ── GET /api/almacen/exportar-clues ──────────────────────────────
        // Replica exacta del "FORMATO DE REPORTE DE INVENTARIO SEMANAL POR
        // CLUES": un renglón por lote con existencia (no por artículo), con
        // los mismos encabezados, colores, anchos y formatos del original.
        [HttpGet, Route("exportar-clues")]
        public HttpResponseMessage ExportarInventarioClues()
        {
            var config = db.alm_config.AsNoTracking().FirstOrDefault();

            var lotes = db.alm_lote.AsNoTracking()
                .Include(l => l.alm_articulo)
                .Where(l => l.cantidad > 0 && l.alm_articulo.activo)
                .OrderBy(l => l.alm_articulo.clave_ssa).ThenBy(l => l.caducidad)
                .ToList();

            // La entrada más vieja de cada lote es la que lo dio de alta:
            // de ahí salen orden de suministro, proveedor y los datos
            // institucionales que se capturan al recibir.
            var entradasPorLote = db.alm_movimiento.AsNoTracking()
                .Where(m => m.tipo == "Entrada")
                .Include(m => m.alm_proveedor)
                .ToList()
                .GroupBy(m => m.lote_id)
                .ToDictionary(g => g.Key, g => g.OrderBy(m => m.creado_en).First());

            using (var paquete = new ExcelPackage())
            {
                var hoja = paquete.Workbook.Worksheets.Add($"INVENTARIO DISPONIBLE {DateTime.Now.Year}");

                // ── Título institucional (A1:R2) ──────────────────────────
                var titulo = hoja.Cells[1, 1, 2, 18];
                titulo.Merge = true;
                hoja.Cells[1, 1].Value = "FORMATO DE REPORTE DE INVENTARIO SEMANAL POR CLUES";
                titulo.Style.Font.Name = "Arial";
                titulo.Style.Font.Size = 10;
                titulo.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#B38E5D"));
                titulo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                titulo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                titulo.Style.WrapText = true;
                titulo.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                titulo.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                hoja.Row(1).Height = 12.75;
                hoja.Row(2).Height = 61.5;

                // Logo, esquina superior derecha (misma posición que el original).
                var rutaLogo = HostingEnvironment.MapPath("~/App_Data/plantillas/inventario_semanal_logo.png");
                if (rutaLogo != null && System.IO.File.Exists(rutaLogo))
                {
                    using (var img = Image.FromFile(rutaLogo))
                    {
                        var pic = hoja.Drawings.AddPicture("logo", img);
                        pic.SetPosition(0, 0, 15, 0);
                    }
                }

                // ── Encabezados (fila 3) ──────────────────────────────────
                string[] encabezados =
                {
                    "ENTIDAD FEDERATIVA", "CLUES DESTINO ", "CLUES ORIGEN", "ORDEN DE SUMINISTRO",
                    "RFC PROVEEDOR", "FUENTE DE FINACIAMIENTO ", "PARTIDA PRESUPUESTAL", "CLAVE/CNIS",
                    "DESCRIPCIÓN", "PRECIO UNITARIO", "VALOR TOTAL ", "INSUMO  CPM\n(SI / NO)",
                    "OBSERVACIONES ", "INVENTARIO DISPONIBLE", "UNIDAD DE MEDIDA", "LOTE",
                    "FECHA DE CADUCIDAD", "FECHA DE RECEPCIÓN"
                };
                var colorEncabezado = ColorTranslator.FromHtml("#13322B");
                for (int c = 0; c < encabezados.Length; c++)
                {
                    var celda = hoja.Cells[3, c + 1];
                    celda.Value = encabezados[c];
                    celda.Style.Font.Name = "Arial";
                    celda.Style.Font.Size = c == 8 ? 11 : 10; // I3 (Descripción) es 11
                    celda.Style.Font.Bold = true;
                    celda.Style.Font.Color.SetColor(Color.White);
                    celda.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    celda.Style.Fill.BackgroundColor.SetColor(colorEncabezado);
                    celda.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celda.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    celda.Style.WrapText = true;
                    celda.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                hoja.Row(3).Height = 25.5;
                hoja.View.FreezePanes(4, 1);

                // ── Anchos de columna (idénticos al original) ─────────────
                double[] anchos = { 20.75, 15.375, 16.875, 35.5, 24.875, 18.125, 20.625, 22.25, 36.25, 16.625,
                                     8.43, 15.75, 18.0, 17.25, 8.43, 17.75, 17.75, 18.875 };
                for (int c = 0; c < anchos.Length; c++) hoja.Column(c + 1).Width = anchos[c];

                // ── Datos (desde fila 4, un renglón por lote) ─────────────
                int fila = 4;
                foreach (var l in lotes)
                {
                    var art = l.alm_articulo;
                    alm_movimiento entrada;
                    entradasPorLote.TryGetValue(l.lote_id, out entrada);

                    hoja.Cells[fila, 1].Value  = config?.entidad_federativa;
                    hoja.Cells[fila, 2].Value  = config?.clues_destino;
                    hoja.Cells[fila, 3].Value  = config?.clues_origen;
                    hoja.Cells[fila, 4].Value  = entrada?.orden_suministro;
                    hoja.Cells[fila, 5].Value  = entrada?.alm_proveedor?.rfc;
                    hoja.Cells[fila, 6].Value  = entrada?.fuente_financiamiento;
                    hoja.Cells[fila, 7].Value  = entrada?.partida_presupuestal;
                    hoja.Cells[fila, 8].Value  = art.clave_ssa;
                    hoja.Cells[fila, 9].Value  = art.nombre;
                    hoja.Cells[fila, 10].Value = art.precio_unitario;
                    hoja.Cells[fila, 11].Formula = $"J{fila}*N{fila}";
                    hoja.Cells[fila, 12].Value = art.es_cpm ? "SI" : "NO";
                    hoja.Cells[fila, 13].Value = entrada?.notas;
                    hoja.Cells[fila, 14].Value = l.cantidad;
                    hoja.Cells[fila, 15].Value = art.unidad_medida;
                    hoja.Cells[fila, 16].Value = l.lote;
                    if (l.caducidad.HasValue) hoja.Cells[fila, 17].Value = l.caducidad.Value;
                    if (entrada != null) hoja.Cells[fila, 18].Value = entrada.creado_en.Date;

                    hoja.Cells[fila, 10].Style.Numberformat.Format = "\"$\"#,##0.00";
                    hoja.Cells[fila, 11].Style.Numberformat.Format = "\"$\"#,##0.00";
                    hoja.Cells[fila, 14].Style.Numberformat.Format = "#,##0";
                    hoja.Cells[fila, 17].Style.Numberformat.Format = "mm-dd-yy";
                    hoja.Cells[fila, 18].Style.Numberformat.Format = "mm-dd-yy";
                    hoja.Cells[fila, 14].Style.Font.Bold = true;

                    var rango = hoja.Cells[fila, 1, fila, 18];
                    rango.Style.Font.Name = "Arial";
                    rango.Style.Font.Size = 10;
                    rango.Style.WrapText = true;
                    rango.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rango.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    rango.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    rango.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rango.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rango.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rango.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    hoja.Cells[fila, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Descripción

                    fila++;
                }

                var bytes = paquete.GetAsByteArray();
                var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                respuesta.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                respuesta.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"Inventario-Semanal-CLUES_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };
                return respuesta;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
