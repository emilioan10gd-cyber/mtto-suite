-- ============================================================================
-- VISTAS DE FARMACIA MIGRADAS DEL ACCESS
--
-- Regla de oro: no inventar datos. Usar solo lo que migrÃ³ bien.
-- - codigobarras: llave de join confiable (1,448 de 1,471 claves estÃ¡n en CPM)
-- - existencia = SUM(entrada) - SUM(salida), nunca solo suma de entradas
-- - "nivel" de existencia se calcula sobre vigente, no sobre total
-- - CPM se muestra pero NO se usa para calcular nada (en CatÃ©ter ya quedÃ³ claro)
-- ============================================================================

DROP VIEW IF EXISTS dbo.vw_farm_inventario;
DROP VIEW IF EXISTS dbo.vw_farm_lotes;
DROP VIEW IF EXISTS dbo.vw_farm_movimientos;
DROP VIEW IF EXISTS dbo.vw_farm_dashboard_kpi;
GO

CREATE VIEW dbo.vw_farm_inventario
AS
-- Un renglÃ³n por cÃ³digo de barras (la pantalla principal de catÃ¡logo).
-- Suma entradas y salidas separadas para mostrar cuÃ¡nto entra, cuÃ¡nto sale, y
-- cuÃ¡l es el neto real en existencia.
SELECT
    m.id,
    m.codigobarras,
    m.clave,
    m.descripcion_articulo_completa AS nombre,
    m.descripcion_articulo_completa AS descripcion,
    m.grupo_terapeutico,
    m.insumo_cpm,
    m.unidad_medida,
    m.clasificacion_aware,

    -- CuÃ¡nto se ha recibido (entrada) en total
    ISNULL(SUM(ed.cantidad), 0) AS total_entradas,

    -- CuÃ¡nto se ha dispensado/usado (salida) en total
    ISNULL(SUM(CASE WHEN sd.cantidad IS NOT NULL THEN sd.cantidad ELSE 0 END), 0) AS total_salidas,

    -- Existencia neta = entradas - salidas
    ISNULL(SUM(ed.cantidad), 0) - ISNULL(SUM(CASE WHEN sd.cantidad IS NOT NULL THEN sd.cantidad ELSE 0 END), 0) AS existencia,

    -- Del CPM: cuÃ¡nto consume el hospital al mes (informaciÃ³n, no cÃ¡lculo).
    -- Por regla de negocio, no se usa para establecer mÃ­nimos: consumo â‰  reorden.
    ISNULL(cpm.cantidad_mensual, 0) AS cpm_mensual,

    -- SemÃ¡foro simple por ahora. Datos reales: no hay stock_minimo ni stock_maximo
    -- en farm_medicamento, asÃ­ que la categorÃ­a es informativa.
    CASE
        WHEN ISNULL(SUM(ed.cantidad), 0) - ISNULL(SUM(CASE WHEN sd.cantidad IS NOT NULL THEN sd.cantidad ELSE 0 END), 0) <= 0
            THEN N'Sin stock'
        WHEN ISNULL(SUM(ed.cantidad), 0) - ISNULL(SUM(CASE WHEN sd.cantidad IS NOT NULL THEN sd.cantidad ELSE 0 END), 0) < 50
            THEN N'Bajo'
        ELSE N'Normal'
    END AS nivel,

    m.activo,
    m.creado_en,
    m.actualizado_en

FROM dbo.farm_medicamento m
LEFT JOIN dbo.farm_entrada_detalle ed ON ed.codigo_barra = m.codigobarras
LEFT JOIN dbo.farm_salida_detalle sd ON sd.codigo_barra = m.codigobarras
LEFT JOIN dbo.cpm_clave cpm ON cpm.clave = m.clave

GROUP BY
    m.id, m.codigobarras, m.clave, m.descripcion_articulo_completa,
    m.grupo_terapeutico, m.insumo_cpm, m.unidad_medida, m.clasificacion_aware,
    m.activo, m.creado_en, m.actualizado_en,
    cpm.cantidad_mensual;
GO

CREATE VIEW dbo.vw_farm_lotes
AS
-- Los lotes (renglones de entrada).
-- Cada renglÃ³n es un lote + fecha de entrada. Salidas se cuentan globalmente
-- en la vista de movimientos (se vuelven a contar juntas por cÃ³digo de barras).
SELECT
    ed.entrada_detalle_id AS lote_id,
    m.id AS medicamento_id,
    m.codigobarras,
    m.clave,
    m.descripcion_articulo_completa AS nombre,
    ed.lote,
    ed.caducidad,
    ed.cantidad AS cantidad_entrada,
    ed.entrada_encabezado_id,
    e.fecha AS fecha_entrada,
    CASE
        WHEN ed.caducidad IS NULL OR ed.caducidad < CAST(SYSDATETIME() AS DATE)
            THEN N'Vencido'
        WHEN DATEDIFF(DAY, CAST(SYSDATETIME() AS DATE), ed.caducidad) <= 30
            THEN N'Por vencer'
        ELSE N'Vigente'
    END AS estado_caducidad,
    ed.creado_en
FROM dbo.farm_entrada_detalle ed
LEFT JOIN dbo.farm_entrada_encabezado e ON e.entrada_encabezado_id = ed.entrada_encabezado_id
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = ed.codigo_barra;
GO

CREATE VIEW dbo.vw_farm_movimientos
AS
-- HistÃ³rico de entradas y salidas (por separado, unificado para auditorÃ­a).
-- Entradas:
SELECT
    'ENTRADA' AS tipo_movimiento,
    e.entrada_encabezado_id AS movimiento_id,
    ed.entrada_detalle_id AS movimiento_detalle_id,
    ed.codigo_barra AS codigobarras,
    m.clave,
    m.descripcion_articulo_completa AS nombre,
    ed.cantidad AS cantidad,
    NULL AS cantidad_salida,
    ed.lote,
    ed.caducidad,
    e.fecha AS fecha_movimiento,
    NULL AS concepto_salida,
    e.creado_por AS responsable,
    e.observaciones,
    e.creado_en,
    m.id AS medicamento_id

FROM dbo.farm_entrada_detalle ed
LEFT JOIN dbo.farm_entrada_encabezado e ON e.entrada_encabezado_id = ed.entrada_encabezado_id
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = ed.codigo_barra

UNION ALL

-- Salidas:
SELECT
    'SALIDA' AS tipo_movimiento,
    s.salida_encabezado_id AS movimiento_id,
    sd.salida_detalle_id AS movimiento_detalle_id,
    sd.codigo_barra AS codigobarras,
    m.clave,
    m.descripcion_articulo_completa AS nombre,
    NULL AS cantidad,
    sd.cantidad AS cantidad_salida,
    sd.lote,
    sd.caducidad,
    s.fecha AS fecha_movimiento,
    sd.concepto AS concepto_salida,
    s.creado_por AS responsable,
    s.observaciones,
    s.creado_en,
    m.id AS medicamento_id

FROM dbo.farm_salida_detalle sd
LEFT JOIN dbo.farm_salida_encabezado s ON s.salida_encabezado_id = sd.salida_encabezado_id
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = sd.codigo_barra;
GO

CREATE VIEW dbo.vw_farm_dashboard_kpi
AS
-- KPIs para el tablero: totales por estado de caducidad, bajo stock, etc.
SELECT
    (SELECT COUNT(DISTINCT clave) FROM farm_medicamento) AS total_claves,
    (SELECT ISNULL(SUM(e.cantidad), 0) - ISNULL(SUM(s.cantidad), 0)
     FROM farm_entrada_detalle e
     LEFT JOIN farm_salida_detalle s ON s.codigo_barra = e.codigo_barra
                                      AND s.lote = e.lote
                                      AND s.entrada_detalle_id = e.entrada_detalle_id) AS total_piezas,
    (SELECT ISNULL(SUM(e.cantidad), 0) - ISNULL(SUM(s.cantidad), 0)
     FROM farm_entrada_detalle e
     LEFT JOIN farm_salida_detalle s ON s.codigo_barra = e.codigo_barra
                                      AND s.lote = e.lote
                                      AND s.entrada_detalle_id = e.entrada_detalle_id
     WHERE e.caducidad IS NULL OR e.caducidad >= CAST(SYSDATETIME() AS DATE)) AS piezas_vigentes,
    (SELECT ISNULL(SUM(e.cantidad), 0) - ISNULL(SUM(s.cantidad), 0)
     FROM farm_entrada_detalle e
     LEFT JOIN farm_salida_detalle s ON s.codigo_barra = e.codigo_barra
                                      AND s.lote = e.lote
                                      AND s.entrada_detalle_id = e.entrada_detalle_id
     WHERE e.caducidad IS NOT NULL AND e.caducidad < CAST(SYSDATETIME() AS DATE)) AS piezas_vencidas,
    (SELECT COUNT(DISTINCT m.id)
     FROM farm_medicamento m
     LEFT JOIN farm_entrada_detalle ed ON ed.codigo_barra = m.codigobarras
     LEFT JOIN farm_salida_detalle sd ON sd.codigo_barra = m.codigobarras
                                       AND sd.lote = ed.lote
                                       AND sd.entrada_detalle_id = ed.entrada_detalle_id
     WHERE ISNULL(SUM(ed.cantidad), 0) - ISNULL(SUM(sd.cantidad), 0) < 50) AS claves_bajo_stock;
GO

-- VerificaciÃ³n: cuÃ¡ntos datos quedan sin match
-- (Se espera ~23 entradas y ~11,134 salidas sin match al codigobarras)
/*
SELECT 'ENT sin match = ' + CAST(COUNT(*) AS VARCHAR) FROM farm_entrada_detalle WHERE NOT EXISTS(SELECT 1 FROM farm_medicamento WHERE codigobarras=farm_entrada_detalle.codigo_barra);
SELECT 'SAL sin match = ' + CAST(COUNT(*) AS VARCHAR) FROM farm_salida_detalle WHERE NOT EXISTS(SELECT 1 FROM farm_medicamento WHERE codigobarras=farm_salida_detalle.codigo_barra);
*/

