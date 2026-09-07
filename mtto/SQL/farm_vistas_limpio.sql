DROP VIEW IF EXISTS dbo.vw_farm_inventario;
DROP VIEW IF EXISTS dbo.vw_farm_lotes;
DROP VIEW IF EXISTS dbo.vw_farm_movimientos;
DROP VIEW IF EXISTS dbo.vw_farm_dashboard_kpi;
GO

CREATE VIEW dbo.vw_farm_inventario
AS
-- Un renglón por código de barras (sin inventar datos).
-- Entradas = cantidad recibida. Salidas = cantidad dispensada. Neto = Ent - Sal.
SELECT
    m.id,
    m.codigobarras,
    m.clave,
    m.descripcion_articulo_completa AS nombre,
    m.grupo_terapeutico,
    m.unidad_medida,
    m.insumo_cpm,
    m.clasificacion_aware,
    m.activo,

    -- Cuántas unidades vinieron
    ISNULL(SUM(ed.cantidad), 0) AS total_entradas,

    -- Cuántas salieron
    ISNULL(SUM(sd.cantidad), 0) AS total_salidas,

    -- Stock actual
    ISNULL(SUM(ed.cantidad), 0) - ISNULL(SUM(sd.cantidad), 0) AS existencia,

    ISNULL(cpm.cantidad_mensual, 0) AS cpm_mensual,

    -- Semáforo con lo que tenemos
    CASE
        WHEN ISNULL(SUM(ed.cantidad), 0) - ISNULL(SUM(sd.cantidad), 0) <= 0 THEN N'Sin stock'
        WHEN ISNULL(SUM(ed.cantidad), 0) - ISNULL(SUM(sd.cantidad), 0) < 50 THEN N'Bajo'
        ELSE N'Normal'
    END AS nivel,

    m.creado_en,
    m.actualizado_en
FROM dbo.farm_medicamento m
LEFT JOIN dbo.farm_entrada_detalle ed ON ed.codigo_barra = m.codigobarras
LEFT JOIN dbo.farm_salida_detalle sd ON sd.codigo_barra = m.codigobarras
LEFT JOIN dbo.cpm_clave cpm ON cpm.clave = m.clave
GROUP BY
    m.id, m.codigobarras, m.clave, m.descripcion_articulo_completa,
    m.grupo_terapeutico, m.unidad_medida, m.insumo_cpm, m.clasificacion_aware,
    m.activo, m.creado_en, m.actualizado_en, cpm.cantidad_mensual;
GO

CREATE VIEW dbo.vw_farm_lotes
AS
SELECT
    ed.id_detalle AS lote_id,
    ed.id_entrada,
    m.id AS medicamento_id,
    m.codigobarras,
    m.clave,
    m.descripcion_articulo_completa,
    ed.lote,
    ed.fecha_caducidad,
    ed.cantidad,
    ed.ubicacion,
    e.fecha_hora_recepcion AS fecha_entrada,
    CASE
        WHEN ed.fecha_caducidad IS NULL THEN N'Sin fecha'
        WHEN ed.fecha_caducidad < CAST(SYSDATETIME() AS DATE) THEN N'Vencido'
        WHEN DATEDIFF(DAY, CAST(SYSDATETIME() AS DATE), ed.fecha_caducidad) <= 30 THEN N'Por vencer'
        ELSE N'Vigente'
    END AS estado_caducidad,
    ed.creado_en
FROM dbo.farm_entrada_detalle ed
LEFT JOIN dbo.farm_entrada_encabezado e ON e.id_entrada = ed.id_entrada
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = ed.codigo_barra;
GO

CREATE VIEW dbo.vw_farm_movimientos
AS
SELECT
    'ENTRADA' AS tipo,
    e.id_entrada AS movimiento_id,
    ed.id_detalle AS detalle_id,
    ed.codigo_barra AS codigobarras,
    m.clave,
    m.descripcion_articulo_completa AS nombre,
    ed.cantidad AS cantidad_mov,
    ed.lote,
    ed.fecha_caducidad,
    e.fecha_hora_recepcion AS fecha,
    e.observaciones,
    m.id AS medicamento_id
FROM dbo.farm_entrada_detalle ed
LEFT JOIN dbo.farm_entrada_encabezado e ON e.id_entrada = ed.id_entrada
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = ed.codigo_barra

UNION ALL

SELECT
    'SALIDA' AS tipo,
    s.id_salida AS movimiento_id,
    sd.id_detalle_salida AS detalle_id,
    sd.codigo_barra AS codigobarras,
    m.clave,
    m.descripcion_articulo_completa AS nombre,
    sd.cantidad AS cantidad_mov,
    sd.lote,
    sd.caducidad,
    s.fecha_hora_salida AS fecha,
    s.observaciones,
    m.id AS medicamento_id
FROM dbo.farm_salida_detalle sd
LEFT JOIN dbo.farm_salida_encabezado s ON s.id_salida = sd.id_salida
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = sd.codigo_barra;
GO

CREATE VIEW dbo.vw_farm_dashboard_kpi
AS
SELECT
    (SELECT COUNT(*) FROM farm_medicamento) AS total_medicamentos,
    (SELECT ISNULL(SUM(e.cantidad), 0) - ISNULL(SUM(s.cantidad), 0)
     FROM farm_entrada_detalle e
     LEFT JOIN farm_salida_detalle s ON s.codigo_barra = e.codigo_barra) AS total_existencia,
    (SELECT COUNT(DISTINCT codigobarras)
     FROM (
        SELECT m.codigobarras,
               ISNULL(SUM(CASE WHEN ed.cantidad IS NOT NULL THEN ed.cantidad ELSE 0 END), 0) -
               ISNULL(SUM(CASE WHEN sd.cantidad IS NOT NULL THEN sd.cantidad ELSE 0 END), 0) AS exist
        FROM farm_medicamento m
        LEFT JOIN farm_entrada_detalle ed ON ed.codigo_barra = m.codigobarras
        LEFT JOIN farm_salida_detalle sd ON sd.codigo_barra = m.codigobarras
        GROUP BY m.codigobarras
     ) x
     WHERE exist < 50) AS medicamentos_bajo_stock;
GO
