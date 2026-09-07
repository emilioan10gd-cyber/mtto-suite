/* =====================================================================
   Vistas de Farmacia  (reemplazan a las anteriores)

   Motivo del cambio: las vistas previas unian farm_entrada_detalle y
   farm_salida_detalle en el MISMO SELECT. Eso produce un producto
   cartesiano: con E entradas y S salidas salen E*S renglones, cada
   entrada se suma S veces y cada salida E veces. Por eso el paracetamol
   reportaba 21,265,182 piezas. Aqui se pre-agrega cada lado por separado.

   Ademas la categoria y el minimo/maximo ahora salen del cuadro basico
   (cpm_clave), porque en Access 1,024 de 1,471 medicamentos traen el
   grupo terapeutico vacio.

   El vocabulario de 'nivel', 'estado_caducidad' y 'tipo' esta calcado de
   los <option> de mttoweb; si se cambia aqui, se rompe el filtro.
   ===================================================================== */

/* ---------------------------------------------------------------------
   1) Lotes: un renglon por (codigo de barras, lote).
      existencia del lote = lo que entro de ese lote - lo que salio.
      FULL OUTER JOIN para no esconder lotes que solo aparecen en salidas
      (capturas donde el lote de salida no empata con ninguna entrada).
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_lotes
AS
WITH ent AS (
    SELECT
        ed.codigo_barra,
        LTRIM(RTRIM(UPPER(ISNULL(ed.lote, N''))))  AS lote_norm,
        MAX(ed.lote)                               AS lote,
        MIN(ed.fecha_caducidad)                    AS fecha_caducidad,
        SUM(CAST(ed.cantidad AS decimal(18,2)))    AS recibido,
        MAX(ee.fecha_hora_recepcion)               AS ultima_entrada,
        MAX(ed.ubicacion)                          AS ubicacion
    FROM dbo.farm_entrada_detalle ed
    LEFT JOIN dbo.farm_entrada_encabezado ee ON ee.id_entrada = ed.id_entrada
    GROUP BY ed.codigo_barra, LTRIM(RTRIM(UPPER(ISNULL(ed.lote, N''))))
),
sal AS (
    SELECT
        sd.codigo_barra,
        LTRIM(RTRIM(UPPER(ISNULL(sd.lote, N''))))  AS lote_norm,
        MAX(sd.lote)                               AS lote,
        MIN(sd.caducidad)                          AS caducidad,
        SUM(CAST(sd.cantidad AS decimal(18,2)))    AS despachado
    FROM dbo.farm_salida_detalle sd
    GROUP BY sd.codigo_barra, LTRIM(RTRIM(UPPER(ISNULL(sd.lote, N''))))
),
mezcla AS (
    SELECT
        COALESCE(e.codigo_barra, s.codigo_barra)        AS codigobarras,
        NULLIF(COALESCE(e.lote, s.lote), N'')           AS lote,
        COALESCE(e.fecha_caducidad, s.caducidad)        AS fecha_caducidad,
        ISNULL(e.recibido, 0)                           AS recibido,
        ISNULL(s.despachado, 0)                         AS despachado,
        ISNULL(e.recibido, 0) - ISNULL(s.despachado, 0) AS existencia,
        e.ubicacion,
        e.ultima_entrada
    FROM ent e
    FULL OUTER JOIN sal s
        ON  s.codigo_barra = e.codigo_barra
        AND s.lote_norm    = e.lote_norm
)
SELECT
    m.id                            AS medicamento_id,
    m.clave,
    x.codigobarras,
    m.descripcion_articulo_completa AS nombre,
    x.lote,
    x.fecha_caducidad,
    x.recibido,
    x.despachado,
    x.existencia,
    x.ubicacion,
    x.ultima_entrada,
    CASE
        WHEN x.fecha_caducidad IS NULL                                              THEN N'Sin fecha'
        WHEN x.fecha_caducidad <  CAST(GETDATE() AS date)                           THEN N'Vencido'
        WHEN x.fecha_caducidad <  DATEADD(day, 30, CAST(GETDATE() AS date))         THEN N'Critico'
        WHEN x.fecha_caducidad <  DATEADD(day, 90, CAST(GETDATE() AS date))         THEN N'Por vencer'
        ELSE N'Vigente'
    END                             AS estado_caducidad
FROM mezcla x
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = x.codigobarras;
GO

/* ---------------------------------------------------------------------
   2) Inventario: un renglon por medicamento, SIEMPRE (1,471), aunque no
      tenga movimientos. Se apoya en vw_farm_lotes para no repetir la
      logica de caducidad.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_inventario
AS
WITH resumen AS (
    SELECT
        codigobarras,
        SUM(recibido)   AS total_entradas,
        SUM(despachado) AS total_salidas,
        SUM(existencia) AS existencia,
        MIN(CASE WHEN existencia > 0 THEN fecha_caducidad END) AS caducidad_proxima,
        MAX(CASE WHEN existencia > 0 AND fecha_caducidad < CAST(GETDATE() AS date)
                 THEN 1 ELSE 0 END)                            AS tiene_vencidos,
        SUM(CASE WHEN existencia > 0 THEN 1 ELSE 0 END)        AS lotes_con_stock
    FROM dbo.vw_farm_lotes
    GROUP BY codigobarras
)
SELECT
    m.id,
    m.clave,
    m.codigobarras,
    /* La descripcion oficial del cuadro basico gana cuando Access viene vacio */
    COALESCE(NULLIF(LTRIM(RTRIM(m.descripcion_articulo_completa)), N''),
             NULLIF(LTRIM(RTRIM(c.descripcion)), N''),
             m.clave)                                   AS nombre,
    /* El grupo terapeutico de Access esta casi siempre vacio, y el del CPM
       trae algunos '#N/A' de la hoja de origen. Ambos se descartan. */
    COALESCE(NULLIF(NULLIF(LTRIM(RTRIM(c.grupo_terapeutico)), N''), N'#N/A'),
             NULLIF(NULLIF(LTRIM(RTRIM(m.grupo_terapeutico)), N''), N'#N/A'),
             N'Sin clasificar')                         AS categoria,
    m.unidad_medida,
    m.insumo_cpm,
    m.clasificacion_aware,
    m.activo,
    CASE WHEN c.clave IS NULL THEN 0 ELSE 1 END         AS en_cuadro_basico,
    ISNULL(c.cantidad_mensual, 0)                       AS cpm_mensual,

    ISNULL(r.total_entradas, 0)                         AS total_entradas,
    ISNULL(r.total_salidas,  0)                         AS total_salidas,
    ISNULL(r.existencia,     0)                         AS existencia,
    ISNULL(r.lotes_con_stock, 0)                        AS lotes_con_stock,
    r.caducidad_proxima,

    /* Nivel contra el consumo promedio mensual del cuadro basico.
       Sin CPM no hay minimo que comparar: se reporta Normal si hay stock. */
    CASE
        WHEN ISNULL(r.existencia, 0) <= 0                                   THEN N'Sin stock'
        WHEN ISNULL(c.cantidad_mensual, 0) > 0
             AND r.existencia < c.cantidad_mensual                          THEN N'Bajo minimo'
        WHEN ISNULL(c.cantidad_mensual, 0) > 0
             AND r.existencia > c.cantidad_mensual * 3                      THEN N'Sobre maximo'
        ELSE N'Normal'
    END                                                 AS nivel,

    CASE
        WHEN ISNULL(r.lotes_con_stock, 0) = 0                                       THEN N'Sin fecha'
        WHEN r.tiene_vencidos = 1                                                   THEN N'Con caducados'
        WHEN r.caducidad_proxima IS NULL                                            THEN N'Sin fecha'
        WHEN r.caducidad_proxima < DATEADD(day, 30, CAST(GETDATE() AS date))        THEN N'Critico'
        WHEN r.caducidad_proxima < DATEADD(day, 90, CAST(GETDATE() AS date))        THEN N'Por vencer'
        ELSE N'Vigente'
    END                                                 AS alerta_caducidad,

    m.creado_en,
    m.actualizado_en
FROM dbo.farm_medicamento m
LEFT JOIN resumen   r ON r.codigobarras = m.codigobarras
LEFT JOIN dbo.cpm_clave c ON c.clave    = m.clave;
GO

/* ---------------------------------------------------------------------
   3) Movimientos: bitacora unificada de entradas y salidas.
      'tipo' se normaliza al vocabulario de la UI (Entrada / Salida /
      Merma / Ajuste (-)); 'tipo_detalle' conserva el texto literal que
      se capturo en Access (Receta, Colectivo, PISO, ...), que es lo que
      alimenta las pantallas de Recetas y Colectivos.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_movimientos
AS
SELECT
    N'Entrada'                              AS tipo,
    N'Entrada'                              AS tipo_detalle,
    ed.id_detalle                           AS movimiento_id,
    ee.id_entrada                           AS documento_id,
    ed.codigo_barra                         AS codigobarras,
    m.clave,
    m.descripcion_articulo_completa         AS nombre,
    CAST(ed.cantidad AS decimal(18,2))      AS cantidad,
    ed.lote,
    ed.fecha_caducidad                      AS caducidad,
    ee.fecha_hora_recepcion                 AS fecha,
    ee.folio_documento                      AS folio,
    ee.orden_suministro                     AS area_servicio,
    CAST(NULL AS nvarchar(200))             AS responsable,
    ee.observaciones
FROM dbo.farm_entrada_detalle ed
INNER JOIN dbo.farm_entrada_encabezado ee ON ee.id_entrada = ed.id_entrada
LEFT  JOIN dbo.farm_medicamento m         ON m.codigobarras = ed.codigo_barra

UNION ALL

SELECT
    CASE
        WHEN UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%CADUC%'
          OR UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%TEMPERATURA%'
          OR UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%INADECUAD%'      THEN N'Merma'
        WHEN UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%DUPLICAD%'
          OR UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%CORREGIR%'
          OR UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%PRUEBA%'         THEN N'Ajuste (-)'
        ELSE N'Salida'
    END                                     AS tipo,
    ISNULL(NULLIF(LTRIM(RTRIM(se.tipo_salida)), N''), N'Sin tipo')       AS tipo_detalle,
    sd.id_detalle_salida                    AS movimiento_id,
    se.id_salida                            AS documento_id,
    sd.codigo_barra                         AS codigobarras,
    m.clave,
    m.descripcion_articulo_completa         AS nombre,
    CAST(sd.cantidad AS decimal(18,2))      AS cantidad,
    sd.lote,
    sd.caducidad,
    se.fecha_hora_salida                    AS fecha,
    se.folio_documento                      AS folio,
    se.servicio_destino                     AS area_servicio,
    se.entregado_a                          AS responsable,
    se.observaciones
FROM dbo.farm_salida_detalle sd
INNER JOIN dbo.farm_salida_encabezado se ON se.id_salida = sd.id_salida
LEFT  JOIN dbo.farm_medicamento m        ON m.codigobarras = sd.codigo_barra;
GO

/* ---------------------------------------------------------------------
   4) KPIs del tablero. Se leen de vw_farm_inventario para que nunca
      discrepen de la tabla que ve el usuario.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_dashboard_kpi
AS
SELECT
    COUNT(*)                                                            AS total_medicamentos,
    SUM(CASE WHEN existencia > 0 THEN 1 ELSE 0 END)                     AS medicamentos_con_stock,
    SUM(CASE WHEN existencia > 0 THEN existencia ELSE 0 END)            AS total_existencia,
    SUM(CASE WHEN nivel = N'Sin stock'    THEN 1 ELSE 0 END)            AS medicamentos_sin_stock,
    SUM(CASE WHEN nivel = N'Bajo minimo'  THEN 1 ELSE 0 END)            AS medicamentos_bajo_minimo,
    SUM(CASE WHEN nivel = N'Sobre maximo' THEN 1 ELSE 0 END)            AS medicamentos_sobre_maximo,
    SUM(CASE WHEN alerta_caducidad = N'Con caducados' THEN 1 ELSE 0 END) AS medicamentos_con_caducados,
    SUM(CASE WHEN alerta_caducidad IN (N'Critico', N'Por vencer') THEN 1 ELSE 0 END) AS medicamentos_por_vencer_90,
    SUM(CASE WHEN en_cuadro_basico = 1 THEN 1 ELSE 0 END)               AS medicamentos_en_cuadro_basico
FROM dbo.vw_farm_inventario;
GO
