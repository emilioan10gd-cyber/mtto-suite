using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;
using mtto.Models;
using mtto.Utilidades;
using static mtto.Utilidades.LectorSql;

namespace mtto.Controllers
{
    /// <summary>
    /// Farmacia: inventario, lotes, bitacora y tablero.
    ///
    /// Se lee con SqlCommand y no con EF porque el origen son vistas
    /// (vw_farm_*) que no tienen llave primaria y EF6 no las sabe mapear.
    ///
    /// Los valores se leen por NOMBRE de columna y con Convert.ToXxx: asi un
    /// cambio de orden o de tipo en la vista (int vs decimal) no rompe el
    /// endpoint, que es lo que pasaba antes con GetDecimal(0).
    /// </summary>
    [RoutePrefix("api/farmacia")]
    [RequiereArea("farmacia")]
    public class FarmaciaController : ApiController
    {
        private readonly Model1 db = new Model1();

        private string Cadena { get { return db.Database.Connection.ConnectionString; } }

        // Los ayudantes de lectura viven en Utilidades/LectorSql.cs y se
        // comparten con los controllers de recetas, colectivos y compras.

        // ================================================================
        // Inventario
        // ================================================================
        private const string FiltroInventario = @"
            WHERE (@q        IS NULL OR m.clave LIKE @qLike OR m.nombre LIKE @qLike OR m.codigobarras LIKE @qLike)
              AND (@categoria IS NULL OR m.categoria        = @categoria)
              AND (@nivel     IS NULL OR m.nivel            = @nivel)
              AND (@alerta    IS NULL OR m.alerta_caducidad = @alerta)";

        private static void ParametrosInventario(SqlCommand cmd, string q, string categoria, string nivel, string alerta)
        {
            cmd.Parameters.AddWithValue("@q", Opcional(q));
            cmd.Parameters.AddWithValue("@qLike",
                string.IsNullOrWhiteSpace(q) ? (object)DBNull.Value : "%" + q.Trim() + "%");
            cmd.Parameters.AddWithValue("@categoria", Opcional(categoria));
            cmd.Parameters.AddWithValue("@nivel", Opcional(nivel));
            cmd.Parameters.AddWithValue("@alerta", Opcional(alerta));
        }

        [HttpGet, Route("")]
        public PaginaDto<FarmaciaInventarioDto> Inventario(
            string q = null, string categoria = null, string nivel = null, string alerta = null,
            int pagina = 1, int tamano = 20)
        {
            Paginar(ref pagina, ref tamano);
            var datos = new List<FarmaciaInventarioDto>();
            int total;

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM vw_farm_inventario m " + FiltroInventario, cn))
                {
                    ParametrosInventario(cmd, q, categoria, nivel, alerta);
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var sql = @"
                    SELECT m.id, m.clave, m.codigobarras, m.nombre, m.categoria, m.unidad_medida,
                           m.total_entradas, m.total_salidas, m.existencia, m.nivel,
                           m.alerta_caducidad, m.cpm_mensual, m.en_cuadro_basico,
                           m.caducidad_proxima, m.lotes_con_stock
                    FROM vw_farm_inventario m " + FiltroInventario + @"
                    ORDER BY m.nombre, m.codigobarras
                    OFFSET @salta ROWS FETCH NEXT @toma ROWS ONLY";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    ParametrosInventario(cmd, q, categoria, nivel, alerta);
                    cmd.Parameters.AddWithValue("@salta", (pagina - 1) * tamano);
                    cmd.Parameters.AddWithValue("@toma", tamano);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            datos.Add(new FarmaciaInventarioDto
                            {
                                Id = Ent(r, "id"),
                                Codigo = Txt(r, "clave"),
                                CodigoBarras = Txt(r, "codigobarras"),
                                Nombre = Txt(r, "nombre"),
                                Categoria = Txt(r, "categoria"),
                                UnidadMedida = Txt(r, "unidad_medida"),
                                TotalEntradas = Num(r, "total_entradas"),
                                TotalSalidas = Num(r, "total_salidas"),
                                Total = Num(r, "existencia"),
                                Nivel = Txt(r, "nivel"),
                                AlertaCaducidad = Txt(r, "alerta_caducidad"),
                                CpmMensual = Num(r, "cpm_mensual"),
                                EnCuadroBasico = Si(r, "en_cuadro_basico"),
                                CaducidadProxima = Fecha(r, "caducidad_proxima"),
                                LotesConStock = Ent(r, "lotes_con_stock")
                            });
                        }
                    }
                }
            }

            return new PaginaDto<FarmaciaInventarioDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = tamano > 0 ? (total + tamano - 1) / tamano : 0,
                Datos = datos
            };
        }

        // ================================================================
        // Lotes de un medicamento
        // ================================================================
        [HttpGet, Route("lotes")]
        public PaginaDto<FarmaciaLoteDto> Lotes(int? articuloId = null, string codigo = null,
                                                int pagina = 1, int tamano = 200)
        {
            Paginar(ref pagina, ref tamano, 1000);
            var datos = new List<FarmaciaLoteDto>();
            int total;

            const string filtro = @"
                WHERE (@articuloId IS NULL OR l.medicamento_id = @articuloId)
                  AND (@codigo     IS NULL OR l.clave = @codigo OR l.codigobarras = @codigo)";

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM vw_farm_lotes l " + filtro, cn))
                {
                    cmd.Parameters.AddWithValue("@articuloId", (object)articuloId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigo", Opcional(codigo));
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var sql = @"
                    SELECT l.medicamento_id, l.clave, l.codigobarras, l.lote, l.fecha_caducidad,
                           l.recibido, l.despachado, l.existencia, l.estado_caducidad, l.ubicacion
                    FROM vw_farm_lotes l " + filtro + @"
                    -- FEFO: primero lo que caduca antes; los sin fecha, al final.
                    ORDER BY CASE WHEN l.fecha_caducidad IS NULL THEN 1 ELSE 0 END,
                             l.fecha_caducidad
                    OFFSET @salta ROWS FETCH NEXT @toma ROWS ONLY";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@articuloId", (object)articuloId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigo", Opcional(codigo));
                    cmd.Parameters.AddWithValue("@salta", (pagina - 1) * tamano);
                    cmd.Parameters.AddWithValue("@toma", tamano);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var recibido = Num(r, "recibido");
                            var despachado = Num(r, "despachado");
                            var ubicacion = Txt(r, "ubicacion");

                            // Un lote en negativo significa que la salida se
                            // capturo con un lote que no empata con ninguna
                            // entrada (tipico: escribieron la caducidad ahi).
                            var notas = recibido == 0 && despachado > 0
                                ? "Solo tiene salidas: el lote no empata con ninguna entrada"
                                : ubicacion;

                            datos.Add(new FarmaciaLoteDto
                            {
                                Id = FarmaciaInventario.ArmarLoteId(Txt(r, "codigobarras"), Txt(r, "lote")),
                                MedicamentoId = Ent(r, "medicamento_id"),
                                Codigo = Txt(r, "clave"),
                                Lote = Txt(r, "lote"),
                                Caducidad = Fecha(r, "fecha_caducidad"),
                                EstadoCaducidad = Txt(r, "estado_caducidad"),
                                Cantidad = Num(r, "existencia"),
                                Recibido = recibido,
                                Despachado = despachado,
                                Notas = notas
                            });
                        }
                    }
                }
            }

            return new PaginaDto<FarmaciaLoteDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = tamano > 0 ? (total + tamano - 1) / tamano : 0,
                Datos = datos
            };
        }

        // ================================================================
        // Bitacora de movimientos
        // ================================================================
        [HttpGet, Route("movimientos")]
        public PaginaDto<FarmaciaMovimientoDto> Movimientos(
            string codigo = null, string tipo = null, int? articuloId = null,
            DateTime? desde = null, DateTime? hasta = null,
            int pagina = 1, int tamano = 20)
        {
            Paginar(ref pagina, ref tamano);
            var datos = new List<FarmaciaMovimientoDto>();
            int total;

            const string filtro = @"
                WHERE (@codigo IS NULL OR v.clave = @codigo OR v.codigobarras = @codigo)
                  AND (@tipo   IS NULL OR v.tipo  = @tipo)
                  AND (@articuloId IS NULL OR v.codigobarras =
                        (SELECT codigobarras FROM farm_medicamento WHERE id = @articuloId))
                  AND (@desde  IS NULL OR v.fecha >= @desde)
                  AND (@hasta  IS NULL OR v.fecha <  DATEADD(day, 1, @hasta))";

            Action<SqlCommand> ponParametros = cmd =>
            {
                cmd.Parameters.AddWithValue("@codigo", Opcional(codigo));
                cmd.Parameters.AddWithValue("@tipo", Opcional(tipo));
                cmd.Parameters.AddWithValue("@articuloId", (object)articuloId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@desde", (object)desde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@hasta", (object)hasta ?? DBNull.Value);
            };

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM vw_farm_movimientos v " + filtro, cn))
                {
                    ponParametros(cmd);
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var sql = @"
                    SELECT v.tipo, v.tipo_detalle, v.folio, v.fecha, v.clave, v.codigobarras,
                           v.nombre, v.lote, v.cantidad, v.area_servicio, v.responsable, v.observaciones
                    FROM vw_farm_movimientos v " + filtro + @"
                    ORDER BY v.fecha DESC, v.movimiento_id DESC
                    OFFSET @salta ROWS FETCH NEXT @toma ROWS ONLY";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    ponParametros(cmd);
                    cmd.Parameters.AddWithValue("@salta", (pagina - 1) * tamano);
                    cmd.Parameters.AddWithValue("@toma", tamano);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            datos.Add(new FarmaciaMovimientoDto
                            {
                                Tipo = Txt(r, "tipo"),
                                TipoDetalle = Txt(r, "tipo_detalle"),
                                Folio = Txt(r, "folio"),
                                Fecha = Fecha(r, "fecha"),
                                Codigo = Txt(r, "clave"),
                                CodigoBarras = Txt(r, "codigobarras"),
                                Nombre = Txt(r, "nombre"),
                                Lote = Txt(r, "lote"),
                                Cantidad = Num(r, "cantidad"),
                                AreaServicio = Txt(r, "area_servicio"),
                                Responsable = Txt(r, "responsable"),
                                Observaciones = Txt(r, "observaciones")
                            });
                        }
                    }
                }
            }

            return new PaginaDto<FarmaciaMovimientoDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = tamano > 0 ? (total + tamano - 1) / tamano : 0,
                Datos = datos
            };
        }

        // ================================================================
        // Escritura de movimientos (Etapa 2)
        // ================================================================
        private FarmaciaInventario.Movimiento Contexto(string tipo, string origen)
        {
            object usuario;
            Request.Properties.TryGetValue("UsuarioActual", out usuario);
            var cuenta = usuario as app_usuario;

            return new FarmaciaInventario.Movimiento
            {
                Tipo = string.IsNullOrWhiteSpace(tipo) ? "Salida" : tipo.Trim(),
                Origen = origen,
                Usuario = cuenta == null ? null : cuenta.nombre_usuario
            };
        }

        /// <summary>
        /// Despacha por FEFO: reparte la cantidad entre los lotes que caducan
        /// primero. No permite parcial: un despacho directo desde inventario
        /// se registra completo o no se registra.
        /// </summary>
        [HttpPost, Route("movimientos/surtir")]
        public IHttpActionResult Surtir(FarmaciaSurtirEntradaDto entrada)
        {
            if (entrada == null || string.IsNullOrWhiteSpace(entrada.Codigo))
                return BadRequest("Falta el codigo del medicamento.");
            if (entrada.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero.");

            var ctx = Contexto(entrada.Tipo, "manual");
            ctx.AreaServicio = entrada.AreaServicio;
            ctx.Responsable = entrada.Responsable;
            ctx.Observaciones = ArmarNota(entrada);

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        var salida = FarmaciaInventario.Despachar(
                            cn, tx, entrada.Codigo.Trim(), entrada.Cantidad, ctx, permiteParcial: false);
                        tx.Commit();
                        return Ok(salida);
                    }
                    catch (InvalidOperationException ex)
                    {
                        tx.Rollback();
                        return Content(HttpStatusCode.Conflict, new { error = ex.Message });
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return Content(HttpStatusCode.BadRequest, new { error = ex.Message });
                    }
                }
            }
        }

        /// Los datos clinicos no tienen columna propia: se conservan en la
        /// observacion para no perderlos.
        private static string ArmarNota(FarmaciaSurtirEntradaDto e)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(e.PacienteNombre)) partes.Add("Paciente: " + e.PacienteNombre.Trim());
            if (!string.IsNullOrWhiteSpace(e.PacienteExpediente)) partes.Add("Exp: " + e.PacienteExpediente.Trim());
            if (!string.IsNullOrWhiteSpace(e.Diagnostico)) partes.Add("Dx: " + e.Diagnostico.Trim());
            if (!string.IsNullOrWhiteSpace(e.Cie)) partes.Add("CIE: " + e.Cie.Trim());
            if (!string.IsNullOrWhiteSpace(e.Observaciones)) partes.Add(e.Observaciones.Trim());
            return partes.Count == 0 ? null : string.Join(" | ", partes);
        }

        /// <summary>Movimiento contra un lote elegido a mano (sin FEFO).</summary>
        [HttpPost, Route("movimientos")]
        public IHttpActionResult RegistrarMovimiento(FarmaciaMovimientoLoteEntradaDto entrada)
        {
            if (entrada == null) return BadRequest("Falta el cuerpo de la peticion.");
            if (entrada.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor que cero.");

            string codigobarras, lote;
            if (!FarmaciaInventario.PartirLoteId(entrada.LoteId, out codigobarras, out lote))
                return BadRequest("El lote indicado no es valido.");

            var signo = FarmaciaInventario.SignoDe(entrada.Tipo);
            var ctx = Contexto(entrada.Tipo, "manual");
            ctx.Responsable = entrada.Responsable;
            ctx.Observaciones = entrada.Observaciones;

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        DateTime? caducidad = null;
                        decimal existencia = 0;

                        var consulta = @"
                            SELECT TOP 1 fecha_caducidad, existencia
                            FROM vw_farm_lotes
                            WHERE codigobarras = @cb AND ISNULL(lote, N'') = ISNULL(@lote, N'')";
                        using (var cmd = new SqlCommand(consulta, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@cb", codigobarras);
                            cmd.Parameters.AddWithValue("@lote", ONulo(lote));
                            using (var r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    caducidad = Fecha(r, "fecha_caducidad");
                                    existencia = Num(r, "existencia");
                                }
                            }
                        }

                        if (signo < 0 && existencia < entrada.Cantidad)
                        {
                            tx.Rollback();
                            return Content(HttpStatusCode.Conflict, new
                            {
                                error = "El lote " + (lote ?? "(sin lote)") + " solo tiene " +
                                        existencia.ToString("0.##") + " y se piden " + entrada.Cantidad.ToString("0.##") + "."
                            });
                        }

                        FarmaciaInventario.Registrar(cn, tx, codigobarras, lote, caducidad,
                                                     entrada.Cantidad, signo, ctx);
                        tx.Commit();
                        return Ok(new { ok = true, lote = lote, cantidad = entrada.Cantidad, tipo = ctx.Tipo });
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return Content(HttpStatusCode.BadRequest, new { error = ex.Message });
                    }
                }
            }
        }

        /// <summary>Alta de un lote nuevo: es una entrada al inventario.</summary>
        [HttpPost, Route("lotes")]
        public IHttpActionResult GuardarLote(FarmaciaNuevoLoteDto entrada)
        {
            if (entrada == null || string.IsNullOrWhiteSpace(entrada.Codigo))
                return BadRequest("Falta el codigo del medicamento.");
            if (entrada.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero.");

            var ctx = Contexto("Entrada", "manual");
            ctx.Responsable = entrada.Responsable;
            ctx.AreaServicio = entrada.Ubicacion;
            ctx.Observaciones = entrada.Observaciones;

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        var cb = FarmaciaInventario.ResolverCodigoBarras(cn, tx, entrada.Codigo.Trim());
                        if (cb == null)
                        {
                            tx.Rollback();
                            return Content(HttpStatusCode.NotFound,
                                new { error = "No existe ningun medicamento con el codigo " + entrada.Codigo + "." });
                        }

                        FarmaciaInventario.Registrar(cn, tx, cb,
                            string.IsNullOrWhiteSpace(entrada.Lote) ? null : entrada.Lote.Trim(),
                            entrada.Caducidad, entrada.Cantidad, 1, ctx);

                        tx.Commit();
                        return Ok(new { ok = true, codigobarras = cb, lote = entrada.Lote, cantidad = entrada.Cantidad });
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return Content(HttpStatusCode.BadRequest, new { error = ex.Message });
                    }
                }
            }
        }

        // ================================================================
        // Catalogos
        // ================================================================
        [HttpGet, Route("catalogos")]
        public FarmaciaCatalogosDto Catalogos()
        {
            var categorias = new List<CatalogoItemDto>();
            var proveedores = new List<CatalogoItemDto>();

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                var sqlCategorias = @"
                    SELECT categoria, COUNT(*) AS cuantos
                    FROM vw_farm_inventario
                    WHERE categoria IS NOT NULL
                    GROUP BY categoria
                    ORDER BY categoria";
                using (var cmd = new SqlCommand(sqlCategorias, cn))
                using (var r = cmd.ExecuteReader())
                {
                    var n = 0;
                    while (r.Read())
                    {
                        var nombre = Txt(r, "categoria");
                        categorias.Add(new CatalogoItemDto
                        {
                            Id = ++n,
                            Codigo = nombre,
                            Nombre = nombre + " (" + Ent(r, "cuantos") + ")"
                        });
                    }
                }

                using (var cmd = new SqlCommand(
                    "SELECT id_proveedor, nombre FROM farm_proveedor_original ORDER BY nombre", cn))
                using (var r = cmd.ExecuteReader())
                {
                    var n = 0;
                    while (r.Read())
                    {
                        proveedores.Add(new CatalogoItemDto
                        {
                            Id = ++n,
                            Codigo = Txt(r, "id_proveedor"),
                            Nombre = Txt(r, "nombre")
                        });
                    }
                }
            }

            return new FarmaciaCatalogosDto
            {
                Categorias = categorias,
                Proveedores = proveedores,
                // Fijos: son las categorias a las que vw_farm_movimientos
                // normaliza el texto libre que se captura en Access.
                TiposMovimiento = new List<FarmaciaTipoMovimientoDto>
                {
                    new FarmaciaTipoMovimientoDto { Nombre = "Entrada",    Clasificacion = "Recepcion",  Signo = 1 },
                    new FarmaciaTipoMovimientoDto { Nombre = "Salida",     Clasificacion = "Despacho",   Signo = -1 },
                    new FarmaciaTipoMovimientoDto { Nombre = "Merma",      Clasificacion = "Baja",       Signo = -1 },
                    new FarmaciaTipoMovimientoDto { Nombre = "Ajuste (+)", Clasificacion = "Correccion", Signo = 1 },
                    new FarmaciaTipoMovimientoDto { Nombre = "Ajuste (-)", Clasificacion = "Correccion", Signo = -1 }
                }
            };
        }

        // ================================================================
        // Tablero
        // ================================================================
        private FarmaciaKpisDto LeerKpis(SqlConnection cn)
        {
            var sql = @"
                SELECT total_medicamentos, medicamentos_con_stock, total_existencia,
                       medicamentos_sin_stock, medicamentos_bajo_minimo,
                       medicamentos_con_caducados, medicamentos_por_vencer_90,
                       medicamentos_en_cuadro_basico
                FROM vw_farm_dashboard_kpi";

            using (var cmd = new SqlCommand(sql, cn))
            using (var r = cmd.ExecuteReader())
            {
                if (!r.Read()) return new FarmaciaKpisDto();

                return new FarmaciaKpisDto
                {
                    TotalMedicamentos = Ent(r, "total_medicamentos"),
                    MedicamentosConStock = Ent(r, "medicamentos_con_stock"),
                    PiezasVigentes = Num(r, "total_existencia"),
                    MedicamentosSinStock = Ent(r, "medicamentos_sin_stock"),
                    MedicamentosBajoMinimo = Ent(r, "medicamentos_bajo_minimo"),
                    MedicamentosConCaducados = Ent(r, "medicamentos_con_caducados"),
                    PiezasPorVencer90 = Ent(r, "medicamentos_por_vencer_90"),
                    MedicamentosEnCuadroBasico = Ent(r, "medicamentos_en_cuadro_basico")
                };
            }
        }

        [HttpGet, Route("dashboard/kpis")]
        public FarmaciaKpisDto Kpis()
        {
            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                return LeerKpis(cn);
            }
        }

        [HttpGet, Route("dashboard")]
        public FarmaciaDashboardDto Dashboard(int topBajoStock = 10, int mesesHistorico = 12)
        {
            if (topBajoStock < 1) topBajoStock = 10;
            if (topBajoStock > 100) topBajoStock = 100;
            if (mesesHistorico < 1) mesesHistorico = 12;
            if (mesesHistorico > 60) mesesHistorico = 60;

            var tablero = new FarmaciaDashboardDto
            {
                PorCategoria = new List<FarmaciaCategoriaResumenDto>(),
                PorNivel = new List<FarmaciaNivelResumenDto>(),
                PorMes = new List<FarmaciaMesResumenDto>(),
                BajoStock = new List<FarmaciaBajoStockDto>()
            };

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                tablero.Kpis = LeerKpis(cn);

                var sqlCategoria = @"
                    SELECT TOP 15 categoria,
                           COUNT(*) AS articulos,
                           SUM(CASE WHEN existencia > 0 THEN existencia ELSE 0 END) AS existencias,
                           SUM(CASE WHEN nivel = N'Bajo minimo' THEN 1 ELSE 0 END)  AS bajo_minimo
                    FROM vw_farm_inventario
                    GROUP BY categoria
                    ORDER BY SUM(CASE WHEN existencia > 0 THEN existencia ELSE 0 END) DESC";
                using (var cmd = new SqlCommand(sqlCategoria, cn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        tablero.PorCategoria.Add(new FarmaciaCategoriaResumenDto
                        {
                            Categoria = Txt(r, "categoria"),
                            TotalArticulos = Ent(r, "articulos"),
                            TotalExistencias = Num(r, "existencias"),
                            BajoMinimo = Ent(r, "bajo_minimo")
                        });
                    }
                }

                using (var cmd = new SqlCommand(
                    "SELECT nivel, COUNT(*) AS cuantos FROM vw_farm_inventario GROUP BY nivel", cn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        tablero.PorNivel.Add(new FarmaciaNivelResumenDto
                        {
                            Nivel = Txt(r, "nivel"),
                            TotalMedicamentos = Ent(r, "cuantos")
                        });
                    }
                }

                // Se descartan las fechas futuras: hay capturas con el ano mal
                // escrito que si no dejan meses vacios al final de la grafica.
                var sqlMes = @"
                    SELECT YEAR(fecha) AS anio, MONTH(fecha) AS mes,
                           SUM(CASE WHEN tipo = N'Entrada' THEN cantidad ELSE 0 END) AS entradas,
                           SUM(CASE WHEN tipo <> N'Entrada' THEN cantidad ELSE 0 END) AS salidas,
                           COUNT(*) AS movimientos
                    FROM vw_farm_movimientos
                    WHERE fecha IS NOT NULL
                      AND fecha >= DATEADD(month, -@meses, CAST(GETDATE() AS date))
                      AND fecha <  DATEADD(day, 1, CAST(GETDATE() AS date))
                    GROUP BY YEAR(fecha), MONTH(fecha)
                    ORDER BY YEAR(fecha), MONTH(fecha)";
                using (var cmd = new SqlCommand(sqlMes, cn))
                {
                    cmd.Parameters.AddWithValue("@meses", mesesHistorico);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            tablero.PorMes.Add(new FarmaciaMesResumenDto
                            {
                                Anio = Ent(r, "anio"),
                                Mes = Ent(r, "mes"),
                                UnidadesEntrada = Num(r, "entradas"),
                                UnidadesSalida = Num(r, "salidas"),
                                TotalMovimientos = Ent(r, "movimientos")
                            });
                        }
                    }
                }

                var sqlBajo = @"
                    SELECT TOP (@top) clave, nombre, existencia, cpm_mensual
                    FROM vw_farm_inventario
                    WHERE nivel = N'Bajo minimo'
                    ORDER BY existencia, nombre";
                using (var cmd = new SqlCommand(sqlBajo, cn))
                {
                    cmd.Parameters.AddWithValue("@top", topBajoStock);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            tablero.BajoStock.Add(new FarmaciaBajoStockDto
                            {
                                Codigo = Txt(r, "clave"),
                                Nombre = Txt(r, "nombre"),
                                Total = Num(r, "existencia"),
                                StockMinimo = Num(r, "cpm_mensual")
                            });
                        }
                    }
                }
            }

            return tablero;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
