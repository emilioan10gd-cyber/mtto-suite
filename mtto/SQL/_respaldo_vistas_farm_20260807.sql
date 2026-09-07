
CREATE VIEW dbo.vw_farm_inventario AS
SELECT
    CAST(ROW_NUMBER() OVER (ORDER BY m.clave) AS INT) AS articulo_id,
    m.clave AS codigo,
    m.descripcion_articulo_completa AS nombre,
    m.descripcion_articulo_completa AS descripcion,
    'Medicamentos' AS categoria,
    m.unidad_medida AS unidad,
    m.descripcion_articulo_completa AS presentacion,
    m.codigobarras AS codigo_barras,
    'Sin especificar' AS proveedor,
    ISNULL(SUM(CASE WHEN ed.fecha_caducidad >= GETDATE() THEN ed.cantidad ELSE 0 END), 0) AS total,
    ISNULL(SUM(CASE WHEN ed.fecha_caducidad >= GETDATE() THEN ed.cantidad ELSE 0 END), 0) AS total_vigente,
    ISNULL(SUM(CASE WHEN ed.fecha_caducidad < GETDATE() THEN ed.cantidad ELSE 0 END), 0) AS total_vencido,
    COUNT(DISTINCT CASE WHEN ed.fecha_caducidad >= GETDATE() THEN ed.lote END) AS lotes_con_existencia,
    MIN(CASE WHEN ed.fecha_caducidad >= GETDATE() THEN ed.fecha_caducidad END) AS caducidad_proxima,
    CASE
        WHEN ISNULL(SUM(CASE WHEN ed.fecha_caducidad >= GETDATE() THEN ed.cantidad ELSE 0 END), 0) = 0 THEN 'Sin stock'
        WHEN ISNULL(SUM(CASE WHEN ed.fecha_caducidad >= GETDATE() THEN ed.cantidad ELSE 0 END), 0) <= 10 THEN 'Bajo minimo'
        ELSE 'Normal'
    END AS nivel,
    CASE
        WHEN DATEDIFF(DAY, GETDATE(), MIN(CASE WHEN ed.fecha_caducidad >= GETDATE() THEN ed.fecha_caducidad END)) <= 30 THEN 'Critico'
        WHEN DATEDIFF(DAY, GETDATE(), MIN(CASE WHEN ed.fecha_caducidad >= GETDATE() THEN ed.fecha_caducidad END)) <= 90 THEN 'Por vencer'
        ELSE 'Vigente'
    END AS alerta_caducidad,
    m.activo,
    GETDATE() AS actualizado_en,
    1 AS categoria_id,
    1 AS unidad_id,
    1 AS proveedor_id,
    10 AS stock_minimo,
    100 AS stock_maximo
FROM dbo.farm_medicamento m
LEFT JOIN dbo.farm_entrada_detalle ed ON m.clave = ed.codigo_barra
GROUP BY
    m.clave,
    m.descripcion_articulo_completa,
    m.codigobarras,
    m.unidad_medida,
    m.activo;
GO
-- Vista: Lotes disponibles con stock
CREATE VIEW dbo.vw_farm_lotes AS
SELECT
    ed.id_detalle AS lote_id,
    ed.codigo_barra AS codigo_medicamento,
    ed.lote,
    ed.fecha_caducidad,
    CASE
        WHEN DATEDIFF(DAY, GETDATE(), ed.fecha_caducidad) <= 0 THEN 'Vencido'
        WHEN DATEDIFF(DAY, GETDATE(), ed.fecha_caducidad) <= 30 THEN 'Critico'
        WHEN DATEDIFF(DAY, GETDATE(), ed.fecha_caducidad) <= 90 THEN 'Por vencer'
        ELSE 'Vigente'
    END AS estado,
    ed.cantidad AS existencia,
    ed.ubicacion,
    ee.fecha_hora_recepcion,
    ee.num_factura,
    ed.precio_unitario
FROM dbo.farm_entrada_detalle ed
JOIN dbo.farm_entrada_encabezado ee ON ed.id_entrada = ee.id_entrada
WHERE ed.cantidad > 0
    AND ed.fecha_caducidad >= GETDATE();
GO
-- Vista: Movimientos (entradas y salidas)
CREATE VIEW dbo.vw_farm_movimientos AS
SELECT
    'Entrada' AS tipo_movimiento,
    ee.id_entrada AS movimiento_id,
    ed.codigo_barra,
    ed.lote,
    ed.cantidad,
    ee.fecha_hora_recepcion AS fecha,
    ee.observaciones,
    ee.num_factura AS referencia
FROM dbo.farm_entrada_detalle ed
JOIN dbo.farm_entrada_encabezado ee ON ed.id_entrada = ee.id_entrada
UNION ALL
SELECT
    'Salida' AS tipo_movimiento,
    se.id_salida AS movimiento_id,
    sd.codigo_barra,
    sd.lote,
    sd.cantidad,
    se.fecha_hora_salida AS fecha,
    se.observaciones,
    se.folio_documento AS referencia
FROM dbo.farm_salida_detalle sd
JOIN dbo.farm_salida_encabezado se ON sd.id_salida = se.id_salida;
GO
-- Vista: Dashboard KPIs
CREATE VIEW dbo.vw_farm_dashboard_kpi AS
SELECT
    COUNT(DISTINCT m.clave) AS medicamentos_activos,
    ISNULL(SUM(ed.cantidad), 0) AS piezas_vigentes,
    COUNT(CASE WHEN vi.nivel = 'Bajo minimo' THEN 1 END) AS bajo_minimo,
    COUNT(CASE WHEN vi.alerta_caducidad = 'Por vencer' THEN 1 END) AS por_caducar_90_dias
FROM dbo.farm_medicamento m
LEFT JOIN dbo.vw_farm_inventario vi ON m.clave = vi.codigo
LEFT JOIN dbo.farm_entrada_detalle ed ON m.clave = ed.codigo_barra AND ed.fecha_caducidad >= GETDATE();

