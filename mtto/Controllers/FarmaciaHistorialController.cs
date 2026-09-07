using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using mtto.Models;
using mtto.Utilidades;
using static mtto.Utilidades.LectorSql;

namespace mtto.Controllers
{
    /* =================================================================
       Recetas, Colectivos y Recepcion.

       Cada lista mezcla dos fuentes:
         - lo capturado en esta app (id positivo, se puede dispensar)
         - lo historico del Access (id negativo, solo lectura)

       Lo historico no trae "lo prescrito" ni "lo autorizado" porque el
       sistema anterior nunca lo guardo; esos campos viajan en null y la
       pantalla muestra un guion, no un 100% inventado.
       ================================================================= */

    public abstract class FarmaciaBaseController : ApiController
    {
        protected readonly Model1 db = new Model1();
        protected string Cadena { get { return db.Database.Connection.ConnectionString; } }

        protected string UsuarioActual()
        {
            object usuario;
            Request.Properties.TryGetValue("UsuarioActual", out usuario);
            var cuenta = usuario as app_usuario;
            return cuenta == null ? null : cuenta.nombre_usuario;
        }

        protected IHttpActionResult Conflicto(string mensaje)
        {
            return Content(HttpStatusCode.Conflict, new { error = mensaje });
        }

        protected IHttpActionResult ErrorPeticion(string mensaje)
        {
            return Content(HttpStatusCode.BadRequest, new { error = mensaje });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }

    // -----------------------------------------------------------------
    // Recetas
    // -----------------------------------------------------------------
    [RoutePrefix("api/farmacia/recetas")]
    [RequiereArea("farmacia")]
    public class FarmaciaRecetasController : FarmaciaBaseController
    {
        private const string Filtro = @"
            WHERE (@expediente IS NULL OR t.paciente_expediente LIKE @expLike
                                       OR t.paciente_nombre     LIKE @expLike)
              AND (@estado     IS NULL OR t.estado   = @estado)
              AND (@servicio   IS NULL OR t.servicio = @servicio)";

        private static void Parametros(SqlCommand cmd, string expediente, string estado, string servicio)
        {
            cmd.Parameters.AddWithValue("@expediente", Opcional(expediente));
            cmd.Parameters.AddWithValue("@expLike",
                string.IsNullOrWhiteSpace(expediente) ? (object)DBNull.Value : "%" + expediente.Trim() + "%");
            cmd.Parameters.AddWithValue("@estado", Opcional(estado));
            cmd.Parameters.AddWithValue("@servicio", Opcional(servicio));
        }

        [HttpGet, Route("")]
        public PaginaDto<FarmaciaRecetaDto> Listar(string expediente = null, string estado = null,
                                                   string servicio = null, int pagina = 1, int tamano = 20)
        {
            Paginar(ref pagina, ref tamano);
            var datos = new List<FarmaciaRecetaDto>();
            int total;

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                using (var cmd = Comando(cn, "SELECT COUNT(*) FROM vw_farm_receta_todas t " + Filtro))
                {
                    Parametros(cmd, expediente, estado, servicio);
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var sql = @"
                    SELECT t.id, t.folio, t.fecha, t.medico, t.paciente_nombre, t.paciente_expediente,
                           t.servicio, t.observaciones, t.total_lineas, t.total_prescrito,
                           t.total_entregado, t.estado, t.historica
                    FROM vw_farm_receta_todas t " + Filtro + @"
                    ORDER BY t.fecha DESC, t.id DESC
                    OFFSET @salta ROWS FETCH NEXT @toma ROWS ONLY";

                using (var cmd = Comando(cn, sql))
                {
                    Parametros(cmd, expediente, estado, servicio);
                    cmd.Parameters.AddWithValue("@salta", (pagina - 1) * tamano);
                    cmd.Parameters.AddWithValue("@toma", tamano);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var esHistorica = Si(r, "historica");
                            datos.Add(new FarmaciaRecetaDto
                            {
                                Id = Ent(r, "id"),
                                Folio = Txt(r, "folio"),
                                Fecha = Fecha(r, "fecha"),
                                Medico = Txt(r, "medico"),
                                PacienteNombre = Txt(r, "paciente_nombre"),
                                PacienteExpediente = Txt(r, "paciente_expediente"),
                                Servicio = Txt(r, "servicio"),
                                Observaciones = Txt(r, "observaciones"),
                                TotalLineas = Ent(r, "total_lineas"),
                                TotalPrescrito = r["total_prescrito"] == DBNull.Value
                                    ? (decimal?)null : Num(r, "total_prescrito"),
                                TotalEntregado = Num(r, "total_entregado"),
                                Estado = Txt(r, "estado"),
                                Historica = esHistorica
                            });
                        }
                    }
                }
            }

            return new PaginaDto<FarmaciaRecetaDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = TotalPaginas(total, tamano),
                Datos = datos
            };
        }

        [HttpGet, Route("{id:int}")]
        public FarmaciaRecetaDetalleDto Obtener(int id)
        {
            var respuesta = new FarmaciaRecetaDetalleDto { Detalle = new List<FarmaciaRecetaLineaDto>() };

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                var sqlCab = @"
                    SELECT t.id, t.folio, t.fecha, t.medico, t.paciente_nombre, t.paciente_expediente,
                           t.servicio, t.observaciones, t.total_lineas, t.total_prescrito,
                           t.total_entregado, t.estado, t.historica
                    FROM vw_farm_receta_todas t WHERE t.id = @id";
                using (var cmd = Comando(cn, sqlCab))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            throw new HttpResponseException(
                                Request.CreateErrorResponse(HttpStatusCode.NotFound, "No existe esa receta."));

                        respuesta.Receta = new FarmaciaRecetaDto
                        {
                            Id = Ent(r, "id"),
                            Folio = Txt(r, "folio"),
                            Fecha = Fecha(r, "fecha"),
                            Medico = Txt(r, "medico"),
                            PacienteNombre = Txt(r, "paciente_nombre"),
                            PacienteExpediente = Txt(r, "paciente_expediente"),
                            Servicio = Txt(r, "servicio"),
                            Observaciones = Txt(r, "observaciones"),
                            TotalLineas = Ent(r, "total_lineas"),
                            TotalPrescrito = r["total_prescrito"] == DBNull.Value
                                ? (decimal?)null : Num(r, "total_prescrito"),
                            TotalEntregado = Num(r, "total_entregado"),
                            Estado = Txt(r, "estado"),
                            Historica = Si(r, "historica")
                        };
                    }
                }

                if (id < 0)
                {
                    // Historica: los renglones son los de la salida del Access.
                    var sqlHist = @"
                        SELECT r.id_detalle_salida, r.codigo, r.articulo, r.cantidad_entregada, r.lote, r.caducidad
                        FROM vw_farm_salida_renglon r
                        WHERE r.id_salida = @idSalida ORDER BY r.articulo";
                    using (var cmd = Comando(cn, sqlHist))
                    {
                        cmd.Parameters.AddWithValue("@idSalida", -id);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                respuesta.Detalle.Add(new FarmaciaRecetaLineaDto
                                {
                                    Id = Ent(r, "id_detalle_salida"),
                                    Codigo = Txt(r, "codigo"),
                                    Articulo = Txt(r, "articulo"),
                                    CantidadPrescrita = null,
                                    CantidadEntregada = Num(r, "cantidad_entregada"),
                                    Pendiente = 0,
                                    EstadoLinea = "Surtida",
                                    Lote = Txt(r, "lote"),
                                    Caducidad = Fecha(r, "caducidad")
                                });
                            }
                        }
                    }
                }
                else
                {
                    var sqlDet = @"
                        SELECT d.receta_detalle_id, d.clave, d.cantidad_prescrita, d.cantidad_entregada,
                               (SELECT TOP 1 descripcion_articulo_completa FROM farm_medicamento
                                WHERE clave = d.clave) AS articulo
                        FROM farm_receta_detalle d
                        WHERE d.receta_id = @id ORDER BY d.receta_detalle_id";
                    using (var cmd = Comando(cn, sqlDet))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var prescrita = Num(r, "cantidad_prescrita");
                                var entregada = Num(r, "cantidad_entregada");
                                var pendiente = prescrita - entregada;

                                respuesta.Detalle.Add(new FarmaciaRecetaLineaDto
                                {
                                    Id = Ent(r, "receta_detalle_id"),
                                    Codigo = Txt(r, "clave"),
                                    Articulo = Txt(r, "articulo"),
                                    CantidadPrescrita = prescrita,
                                    CantidadEntregada = entregada,
                                    Pendiente = pendiente > 0 ? pendiente : 0,
                                    EstadoLinea = entregada <= 0 ? "Pendiente"
                                                : pendiente > 0 ? "Parcial" : "Surtida"
                                });
                            }
                        }
                    }
                }
            }

            return respuesta;
        }

        [HttpGet, Route("indicador-abasto")]
        public List<FarmaciaAbastoDto> IndicadorAbasto(int meses = 1)
        {
            if (meses < 1) meses = 1;
            if (meses > 36) meses = 36;

            var lista = new List<FarmaciaAbastoDto>();

            // Solo las recetas capturadas aqui tienen "lo prescrito"; las
            // historicas se cuentan aparte para no diluir el porcentaje.
            // Prescrito y entregado se toman SOLO de las recetas capturadas
            // aqui: mezclar el entregado de las historicas (que no tienen
            // prescrito) daba porcentajes arriba de 100.
            var sql = @"
                SELECT YEAR(t.fecha) AS anio, MONTH(t.fecha) AS mes,
                       COUNT(*)                                                      AS recetas,
                       SUM(t.total_lineas)                                           AS lineas,
                       SUM(CASE WHEN t.historica = 0 THEN t.total_prescrito END)     AS prescrito,
                       SUM(CASE WHEN t.historica = 0 THEN t.total_entregado END)     AS entregado,
                       SUM(CASE WHEN t.historica = 0 THEN t.lineas_con_faltante END) AS con_faltante,
                       SUM(CASE WHEN t.historica = 0 THEN 1 ELSE 0 END)              AS capturadas
                FROM vw_farm_receta_todas t
                WHERE t.fecha IS NOT NULL
                  AND t.fecha >= DATEADD(month, -@meses, CAST(GETDATE() AS date))
                  AND t.fecha <  DATEADD(day, 1, CAST(GETDATE() AS date))
                GROUP BY YEAR(t.fecha), MONTH(t.fecha)
                ORDER BY YEAR(t.fecha), MONTH(t.fecha)";

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var cmd = Comando(cn, sql))
                {
                    cmd.Parameters.AddWithValue("@meses", meses);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var capturadas = Ent(r, "capturadas");
                            var prescrito = r["prescrito"] == DBNull.Value ? (decimal?)null : Num(r, "prescrito");
                            var entregado = r["entregado"] == DBNull.Value ? 0m : Num(r, "entregado");

                            decimal? porcentaje = null;
                            if (capturadas > 0 && prescrito.HasValue && prescrito.Value > 0)
                            {
                                // Se acota a 100: entregar de mas no es 120% de abasto.
                                var p = Math.Round(100m * entregado / prescrito.Value, 0);
                                porcentaje = p > 100m ? 100m : p;
                            }

                            lista.Add(new FarmaciaAbastoDto
                            {
                                Anio = Ent(r, "anio"),
                                Mes = Ent(r, "mes"),
                                TotalRecetas = Ent(r, "recetas"),
                                RecetasCapturadas = capturadas,
                                TotalLineas = Ent(r, "lineas"),
                                TotalPrescrito = prescrito,
                                TotalEntregado = entregado,
                                LineasConFaltante = r["con_faltante"] == DBNull.Value
                                    ? (int?)null : Ent(r, "con_faltante"),
                                PorcentajeAbasto = porcentaje
                            });
                        }
                    }
                }
            }

            return lista;
        }

        [HttpPost, Route("")]
        public IHttpActionResult Guardar(FarmaciaNuevaRecetaDto entrada)
        {
            if (entrada == null) return ErrorPeticion("Falta el cuerpo de la peticion.");
            if (string.IsNullOrWhiteSpace(entrada.PacienteNombre))
                return ErrorPeticion("El nombre del paciente es obligatorio.");
            if (entrada.Detalle == null || entrada.Detalle.Count == 0)
                return ErrorPeticion("Agrega al menos una clave al detalle de la receta.");

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        var folio = FarmaciaInventario.SiguienteFolio(cn, tx, "farm_receta", "folio", "RX");

                        int recetaId;
                        var sqlCab = @"
                            INSERT INTO farm_receta
                                (folio, fecha, medico, paciente_nombre, paciente_expediente,
                                 servicio, observaciones, usuario)
                            OUTPUT INSERTED.receta_id
                            VALUES (@folio, @fecha, @medico, @paciente, @exp, @servicio, @obs, @usuario)";
                        using (var cmd = new SqlCommand(sqlCab, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@folio", folio);
                            cmd.Parameters.AddWithValue("@fecha", entrada.Fecha ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@medico", ONulo(entrada.Medico));
                            cmd.Parameters.AddWithValue("@paciente", entrada.PacienteNombre.Trim());
                            cmd.Parameters.AddWithValue("@exp", ONulo(entrada.PacienteExpediente));
                            cmd.Parameters.AddWithValue("@servicio", ONulo(entrada.Servicio));
                            cmd.Parameters.AddWithValue("@obs", ONulo(entrada.Observaciones));
                            cmd.Parameters.AddWithValue("@usuario", ONulo(UsuarioActual()));
                            recetaId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (var linea in entrada.Detalle)
                        {
                            if (linea == null || string.IsNullOrWhiteSpace(linea.Codigo) || linea.Cantidad <= 0)
                                continue;

                            var sqlDet = @"
                                INSERT INTO farm_receta_detalle (receta_id, clave, cantidad_prescrita)
                                VALUES (@receta, @clave, @cant)";
                            using (var cmd = new SqlCommand(sqlDet, cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@receta", recetaId);
                                cmd.Parameters.AddWithValue("@clave", linea.Codigo.Trim());
                                cmd.Parameters.AddWithValue("@cant", linea.Cantidad);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return Ok(new { receta = new { id = recetaId, folio = folio } });
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return ErrorPeticion(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Dispensa una linea por FEFO. Permite parcial a proposito: si no
        /// alcanza se entrega lo que hay y el faltante queda registrado, que
        /// es exactamente el indicador de abasto que se pidio.
        /// </summary>
        [HttpPost, Route("lineas/{id:int}/dispensar")]
        public IHttpActionResult Dispensar(int id, FarmaciaLineaEntradaDto entrada)
        {
            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        string clave = null, folio = null, paciente = null, servicio = null;
                        decimal prescrita = 0, entregada = 0;
                        int recetaId = 0;

                        var sql = @"
                            SELECT d.receta_id, d.clave, d.cantidad_prescrita, d.cantidad_entregada,
                                   r.folio, r.paciente_nombre, r.servicio
                            FROM farm_receta_detalle d
                            INNER JOIN farm_receta r ON r.receta_id = d.receta_id
                            WHERE d.receta_detalle_id = @id";
                        using (var cmd = new SqlCommand(sql, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            using (var r = cmd.ExecuteReader())
                            {
                                if (!r.Read())
                                {
                                    tx.Rollback();
                                    return Content(HttpStatusCode.NotFound, new
                                    {
                                        error = "No existe ese renglon de receta. Las recetas del sistema " +
                                                "anterior son de solo lectura."
                                    });
                                }
                                recetaId = Ent(r, "receta_id");
                                clave = Txt(r, "clave");
                                prescrita = Num(r, "cantidad_prescrita");
                                entregada = Num(r, "cantidad_entregada");
                                folio = Txt(r, "folio");
                                paciente = Txt(r, "paciente_nombre");
                                servicio = Txt(r, "servicio");
                            }
                        }

                        var pendiente = prescrita - entregada;
                        if (pendiente <= 0)
                        {
                            tx.Rollback();
                            return Conflicto("Ese renglon ya esta surtido por completo.");
                        }

                        var pedir = (entrada != null && entrada.Cantidad > 0)
                            ? Math.Min(entrada.Cantidad, pendiente)
                            : pendiente;

                        var ctx = new FarmaciaInventario.Movimiento
                        {
                            Tipo = "Salida",
                            Origen = "receta",
                            OrigenId = recetaId,
                            Folio = folio,
                            AreaServicio = servicio,
                            Responsable = paciente,
                            Observaciones = "Receta " + folio + " - " + paciente,
                            Usuario = UsuarioActual()
                        };

                        var surtido = FarmaciaInventario.Despachar(cn, tx, clave, pedir, ctx, permiteParcial: true);

                        using (var cmd = new SqlCommand(
                            "UPDATE farm_receta_detalle SET cantidad_entregada = cantidad_entregada + @ent WHERE receta_detalle_id = @id",
                            cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ent", surtido.Entregado);
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return Ok(surtido);
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return ErrorPeticion(ex.Message);
                    }
                }
            }
        }
    }

    // -----------------------------------------------------------------
    // Colectivos
    // -----------------------------------------------------------------
    [RoutePrefix("api/farmacia/colectivos")]
    [RequiereArea("farmacia")]
    public class FarmaciaColectivosController : FarmaciaBaseController
    {
        [HttpGet, Route("")]
        public PaginaDto<FarmaciaColectivoDto> Listar(string servicio = null, int pagina = 1, int tamano = 30)
        {
            Paginar(ref pagina, ref tamano);
            var datos = new List<FarmaciaColectivoDto>();
            int total;

            const string filtro = " WHERE (@servicio IS NULL OR t.servicio = @servicio)";

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                using (var cmd = Comando(cn, "SELECT COUNT(*) FROM vw_farm_colectivo_todos t" + filtro))
                {
                    cmd.Parameters.AddWithValue("@servicio", Opcional(servicio));
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var sql = @"
                    SELECT t.id, t.folio, t.fecha, t.servicio, t.entregado_por, t.recibido_por,
                           t.turno, t.total_lineas, t.total_entregado, t.historico
                    FROM vw_farm_colectivo_todos t" + filtro + @"
                    ORDER BY t.fecha DESC, t.id DESC
                    OFFSET @salta ROWS FETCH NEXT @toma ROWS ONLY";

                using (var cmd = Comando(cn, sql))
                {
                    cmd.Parameters.AddWithValue("@servicio", Opcional(servicio));
                    cmd.Parameters.AddWithValue("@salta", (pagina - 1) * tamano);
                    cmd.Parameters.AddWithValue("@toma", tamano);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            datos.Add(new FarmaciaColectivoDto
                            {
                                Id = Ent(r, "id"),
                                Folio = Txt(r, "folio"),
                                Fecha = Fecha(r, "fecha"),
                                Servicio = Txt(r, "servicio"),
                                EntregadoPor = Txt(r, "entregado_por"),
                                RecibidoPor = Txt(r, "recibido_por"),
                                Turno = Txt(r, "turno"),
                                TotalLineas = Ent(r, "total_lineas"),
                                TotalEntregado = Num(r, "total_entregado"),
                                Historico = Si(r, "historico")
                            });
                        }
                    }
                }
            }

            return new PaginaDto<FarmaciaColectivoDto>
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = TotalPaginas(total, tamano),
                Datos = datos
            };
        }

        [HttpGet, Route("servicios")]
        public List<CatalogoItemDto> Servicios(bool soloActivos = true)
        {
            var lista = new List<CatalogoItemDto>();

            // Los servicios autorizados van primero; luego los que solo
            // aparecen en el historico del Access.
            var sql = @"
                SELECT servicio, MAX(autorizado) AS autorizado, SUM(documentos) AS documentos
                FROM (
                    SELECT servicio, 1 AS autorizado, 0 AS documentos
                    FROM farm_servicio_clave WHERE activo = 1
                    UNION ALL
                    SELECT servicio, 0, COUNT(*)
                    FROM vw_farm_salida_documento
                    WHERE servicio IS NOT NULL AND LTRIM(RTRIM(servicio)) <> N''
                    GROUP BY servicio
                ) x
                GROUP BY servicio
                ORDER BY MAX(autorizado) DESC, SUM(documentos) DESC, servicio";

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var cmd = Comando(cn, sql))
                using (var r = cmd.ExecuteReader())
                {
                    var n = 0;
                    while (r.Read())
                    {
                        var nombre = Txt(r, "servicio");
                        lista.Add(new CatalogoItemDto { Id = ++n, Codigo = nombre, Nombre = nombre });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Cuadro del servicio. Si ya se autorizaron claves, se devuelven
        /// esas; si no, se devuelve lo que el servicio ha consumido, marcado
        /// como no autorizado para que la pantalla no lo confunda.
        /// </summary>
        [HttpGet, Route("servicios/{servicio}/claves")]
        public List<FarmaciaServicioClaveDto> ClavesDelServicio(string servicio)
        {
            var lista = new List<FarmaciaServicioClaveDto>();

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                var sqlAut = @"
                    SELECT sc.clave, sc.cantidad_periodo, sc.periodo,
                           (SELECT TOP 1 descripcion_articulo_completa FROM farm_medicamento
                            WHERE clave = sc.clave) AS articulo
                    FROM farm_servicio_clave sc
                    WHERE sc.servicio = @servicio AND sc.activo = 1
                    ORDER BY sc.clave";
                using (var cmd = Comando(cn, sqlAut))
                {
                    cmd.Parameters.AddWithValue("@servicio", servicio ?? "");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new FarmaciaServicioClaveDto
                            {
                                Codigo = Txt(r, "clave"),
                                Articulo = Txt(r, "articulo"),
                                CantidadPeriodo = Num(r, "cantidad_periodo"),
                                Periodo = Txt(r, "periodo"),
                                Autorizado = true
                            });
                        }
                    }
                }

                if (lista.Count > 0) return lista;

                var sqlHist = @"
                    SELECT TOP 200 codigo, articulo, cantidad_periodo, documentos, desde, hasta
                    FROM vw_farm_servicio_clave_historica
                    WHERE servicio = @servicio
                    ORDER BY cantidad_periodo DESC";
                using (var cmd = Comando(cn, sqlHist))
                {
                    cmd.Parameters.AddWithValue("@servicio", servicio ?? "");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var desde = Fecha(r, "desde");
                            var hasta = Fecha(r, "hasta");
                            lista.Add(new FarmaciaServicioClaveDto
                            {
                                Codigo = Txt(r, "codigo"),
                                Articulo = Txt(r, "articulo"),
                                CantidadPeriodo = Num(r, "cantidad_periodo"),
                                Documentos = Ent(r, "documentos"),
                                Periodo = desde.HasValue && hasta.HasValue
                                    ? "Consumido " + desde.Value.ToString("dd/MM/yyyy") + " a " + hasta.Value.ToString("dd/MM/yyyy")
                                    : "Historico",
                                Autorizado = false
                            });
                        }
                    }
                }
            }

            return lista;
        }

        [HttpPost, Route("claves")]
        public IHttpActionResult AsignarClave(FarmaciaServicioClaveEntradaDto entrada)
        {
            if (entrada == null || string.IsNullOrWhiteSpace(entrada.Servicio))
                return ErrorPeticion("Elige un servicio.");
            if (string.IsNullOrWhiteSpace(entrada.Codigo))
                return ErrorPeticion("Falta la clave.");
            if (entrada.CantidadPeriodo <= 0)
                return ErrorPeticion("La cantidad por periodo debe ser mayor que cero.");

            var sql = @"
                MERGE dbo.farm_servicio_clave AS destino
                USING (SELECT @servicio AS servicio, @clave AS clave) AS origen
                    ON destino.servicio = origen.servicio AND destino.clave = origen.clave
                WHEN MATCHED THEN
                    UPDATE SET cantidad_periodo = @cant, periodo = @periodo, activo = 1, usuario = @usuario
                WHEN NOT MATCHED THEN
                    INSERT (servicio, clave, cantidad_periodo, periodo, usuario)
                    VALUES (@servicio, @clave, @cant, @periodo, @usuario);";

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var cmd = Comando(cn, sql))
                {
                    cmd.Parameters.AddWithValue("@servicio", entrada.Servicio.Trim());
                    cmd.Parameters.AddWithValue("@clave", entrada.Codigo.Trim());
                    cmd.Parameters.AddWithValue("@cant", entrada.CantidadPeriodo);
                    cmd.Parameters.AddWithValue("@periodo",
                        string.IsNullOrWhiteSpace(entrada.Periodo) ? "Mensual" : entrada.Periodo.Trim());
                    cmd.Parameters.AddWithValue("@usuario", ONulo(UsuarioActual()));
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { ok = true });
        }

        /// <summary>
        /// Entrega colectiva. Es todo-o-nada: si una sola clave no alcanza,
        /// no se entrega ninguna, para que el vale no salga incompleto sin
        /// que nadie se entere.
        /// </summary>
        [HttpPost, Route("")]
        public IHttpActionResult Guardar(FarmaciaNuevoColectivoDto entrada)
        {
            if (entrada == null) return ErrorPeticion("Falta el cuerpo de la peticion.");
            if (string.IsNullOrWhiteSpace(entrada.Servicio))
                return ErrorPeticion("Elige el servicio al que se entrega.");
            if (entrada.Detalle == null || entrada.Detalle.Count == 0)
                return ErrorPeticion("Agrega al menos una clave al detalle.");

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        var folio = FarmaciaInventario.SiguienteFolio(cn, tx, "farm_colectivo", "folio", "COL");

                        int colectivoId;
                        var sqlCab = @"
                            INSERT INTO farm_colectivo
                                (folio, fecha, servicio, entregado_por, recibido_por, turno, observaciones, usuario)
                            OUTPUT INSERTED.colectivo_id
                            VALUES (@folio, @fecha, @servicio, @entrega, @recibe, @turno, @obs, @usuario)";
                        using (var cmd = new SqlCommand(sqlCab, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@folio", folio);
                            cmd.Parameters.AddWithValue("@fecha", entrada.Fecha ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@servicio", entrada.Servicio.Trim());
                            cmd.Parameters.AddWithValue("@entrega", ONulo(entrada.EntregadoPor));
                            cmd.Parameters.AddWithValue("@recibe", ONulo(entrada.RecibidoPor));
                            cmd.Parameters.AddWithValue("@turno", ONulo(entrada.Turno));
                            cmd.Parameters.AddWithValue("@obs", ONulo(entrada.Observaciones));
                            cmd.Parameters.AddWithValue("@usuario", ONulo(UsuarioActual()));
                            colectivoId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        var ctx = new FarmaciaInventario.Movimiento
                        {
                            Tipo = "Salida",
                            Origen = "colectivo",
                            OrigenId = colectivoId,
                            Folio = folio,
                            AreaServicio = entrada.Servicio.Trim(),
                            Responsable = entrada.RecibidoPor,
                            Observaciones = "Colectivo " + folio,
                            Usuario = UsuarioActual(),
                            Fecha = entrada.Fecha
                        };

                        decimal entregado = 0;
                        foreach (var linea in entrada.Detalle)
                        {
                            if (linea == null || string.IsNullOrWhiteSpace(linea.Codigo) || linea.Cantidad <= 0)
                                continue;

                            // permiteParcial:false -> si falta una clave,
                            // la excepcion revierte todo el vale.
                            var s = FarmaciaInventario.Despachar(cn, tx, linea.Codigo.Trim(),
                                                                 linea.Cantidad, ctx, permiteParcial: false);
                            entregado += s.Entregado;
                        }

                        tx.Commit();
                        return Ok(new { colectivo = new { id = colectivoId, folio = folio, entregado = entregado } });
                    }
                    catch (InvalidOperationException ex)
                    {
                        tx.Rollback();
                        return Conflicto(ex.Message + " No se entrego ninguna clave del vale.");
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return ErrorPeticion(ex.Message);
                    }
                }
            }
        }
    }

    // -----------------------------------------------------------------
    // Recepcion / compras
    // -----------------------------------------------------------------
    [RoutePrefix("api/farmacia/compras")]
    [RequiereArea("farmacia")]
    public class FarmaciaComprasController : FarmaciaBaseController
    {
        [HttpGet, Route("")]
        public List<FarmaciaAvanceCompraDto> Listar(string comNumero = null, int tope = 300)
        {
            if (tope < 1) tope = 300;
            if (tope > 2000) tope = 2000;

            var lista = new List<FarmaciaAvanceCompraDto>();

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();

                // 1. COM capturadas: aqui si hay contra que comparar.
                var sqlCom = @"
                    SELECT com_numero, fecha_documento, proveedor, codigo, articulo,
                           cantidad_autorizada, cantidad_recibida, pendiente, porcentaje_surtido
                    FROM vw_farm_avance_com
                    WHERE (@com IS NULL OR com_numero LIKE @comLike)
                    ORDER BY fecha_documento DESC, com_numero, codigo";
                using (var cmd = Comando(cn, sqlCom))
                {
                    cmd.Parameters.AddWithValue("@com", Opcional(comNumero));
                    cmd.Parameters.AddWithValue("@comLike",
                        string.IsNullOrWhiteSpace(comNumero) ? (object)DBNull.Value : "%" + comNumero.Trim() + "%");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new FarmaciaAvanceCompraDto
                            {
                                ComNumero = Txt(r, "com_numero"),
                                FechaDocumento = Fecha(r, "fecha_documento"),
                                Proveedor = Txt(r, "proveedor"),
                                Codigo = Txt(r, "codigo"),
                                Articulo = Txt(r, "articulo"),
                                CantidadAutorizada = Num(r, "cantidad_autorizada"),
                                CantidadRecibida = Num(r, "cantidad_recibida"),
                                Pendiente = Num(r, "pendiente"),
                                PorcentajeSurtido = r["porcentaje_surtido"] == DBNull.Value
                                    ? 0m : Num(r, "porcentaje_surtido"),
                                SinComCapturada = false
                            });
                        }
                    }
                }

                // 2. Recepciones historicas sin COM: se muestran igual, pero
                //    sin porcentaje porque no hay autorizado con que comparar.
                var restante = tope - lista.Count;
                if (restante > 0)
                {
                    var sqlHist = @"
                        SELECT TOP (@tope) h.com_numero, h.fecha_documento, h.proveedor, h.codigo,
                               h.articulo, h.cantidad_recibida, h.renglones
                        FROM vw_farm_recepcion_historica h
                        WHERE (@com IS NULL OR h.com_numero LIKE @comLike)
                          AND NOT EXISTS (SELECT 1 FROM farm_com c WHERE c.com_numero = h.com_numero)
                        ORDER BY h.fecha_documento DESC, h.com_numero, h.articulo";
                    using (var cmd = Comando(cn, sqlHist))
                    {
                        cmd.Parameters.AddWithValue("@tope", restante);
                        cmd.Parameters.AddWithValue("@com", Opcional(comNumero));
                        cmd.Parameters.AddWithValue("@comLike",
                            string.IsNullOrWhiteSpace(comNumero) ? (object)DBNull.Value : "%" + comNumero.Trim() + "%");
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                lista.Add(new FarmaciaAvanceCompraDto
                                {
                                    ComNumero = Txt(r, "com_numero"),
                                    FechaDocumento = Fecha(r, "fecha_documento"),
                                    Proveedor = Txt(r, "proveedor"),
                                    Codigo = Txt(r, "codigo"),
                                    Articulo = Txt(r, "articulo"),
                                    CantidadRecibida = Num(r, "cantidad_recibida"),
                                    Renglones = Ent(r, "renglones"),
                                    CantidadAutorizada = null,
                                    Pendiente = null,
                                    PorcentajeSurtido = null,
                                    SinComCapturada = true
                                });
                            }
                        }
                    }
                }
            }

            return lista;
        }

        [HttpPost, Route("")]
        public IHttpActionResult Guardar(FarmaciaComEntradaDto entrada)
        {
            if (entrada == null) return ErrorPeticion("Falta el cuerpo de la peticion.");
            if (string.IsNullOrWhiteSpace(entrada.ComNumero))
                return ErrorPeticion("El numero de COM es obligatorio.");
            if (entrada.Detalle == null || entrada.Detalle.Count == 0)
                return ErrorPeticion("Agrega al menos una clave al detalle de la COM.");

            using (var cn = new SqlConnection(Cadena))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        int comId;
                        var sqlCab = @"
                            INSERT INTO farm_com (com_numero, proveedor, fecha_documento, observaciones, usuario)
                            OUTPUT INSERTED.com_id
                            VALUES (@num, @prov, @fecha, @obs, @usuario)";
                        using (var cmd = new SqlCommand(sqlCab, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@num", entrada.ComNumero.Trim());
                            cmd.Parameters.AddWithValue("@prov", ONulo(entrada.Proveedor));
                            cmd.Parameters.AddWithValue("@fecha", ONulo(entrada.FechaDocumento));
                            cmd.Parameters.AddWithValue("@obs", ONulo(entrada.Observaciones));
                            cmd.Parameters.AddWithValue("@usuario", ONulo(UsuarioActual()));
                            comId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (var linea in entrada.Detalle)
                        {
                            if (linea == null || string.IsNullOrWhiteSpace(linea.Codigo) || linea.Cantidad <= 0)
                                continue;

                            using (var cmd = new SqlCommand(
                                "INSERT INTO farm_com_detalle (com_id, clave, cantidad_autorizada) VALUES (@com, @clave, @cant)",
                                cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@com", comId);
                                cmd.Parameters.AddWithValue("@clave", linea.Codigo.Trim());
                                cmd.Parameters.AddWithValue("@cant", linea.Cantidad);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return Ok(new { com = new { id = comId, comNumero = entrada.ComNumero.Trim() } });
                    }
                    catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                    {
                        tx.Rollback();
                        return Conflicto("Ya existe una COM con el numero " + entrada.ComNumero.Trim() + ".");
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return ErrorPeticion(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Recibe mercancia. El folio del movimiento guarda el numero de COM,
        /// que es lo que permite calcular despues el % de surtimiento.
        /// </summary>
        [HttpPost, Route("recepcion")]
        public IHttpActionResult Recepcion(FarmaciaRecepcionEntradaDto entrada)
        {
            if (entrada == null) return ErrorPeticion("Falta el cuerpo de la peticion.");
            if (string.IsNullOrWhiteSpace(entrada.Codigo))
                return ErrorPeticion("Falta el codigo del medicamento.");
            if (entrada.Cantidad <= 0)
                return ErrorPeticion("La cantidad debe ser mayor que cero.");

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

                        var ctx = new FarmaciaInventario.Movimiento
                        {
                            Tipo = "Entrada",
                            Origen = "recepcion",
                            Folio = string.IsNullOrWhiteSpace(entrada.ComNumero) ? null : entrada.ComNumero.Trim(),
                            Responsable = entrada.Responsable,
                            Observaciones = string.IsNullOrWhiteSpace(entrada.ComNumero)
                                ? "Recepcion sin COM" : "Recepcion contra COM " + entrada.ComNumero.Trim(),
                            Usuario = UsuarioActual()
                        };

                        FarmaciaInventario.Registrar(cn, tx, cb,
                            string.IsNullOrWhiteSpace(entrada.Lote) ? null : entrada.Lote.Trim(),
                            entrada.Caducidad, entrada.Cantidad, 1, ctx);

                        tx.Commit();
                        return Ok(new { ok = true, codigobarras = cb, cantidad = entrada.Cantidad });
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return ErrorPeticion(ex.Message);
                    }
                }
            }
        }
    }
}
