-- ===================================================================
-- Almacen Central - rediseno de movimientos como caja (punto de venta)
-- 2026-08-21
--
-- 1) Nuevo tipo de movimiento: Transferencia
--    (traspaso a otro almacen; se separa de Salida para que los
--     reportes distingan consumo de servicio vs traspaso interno)
-- 2) Nueva columna entregado_a: persona que recibe el material
-- 3) Vista vw_alm_movimientos actualizada con entregado_a
-- ===================================================================

-- ── 1. Columna entregado_a ────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('alm_movimiento') AND name = 'entregado_a')
BEGIN
    ALTER TABLE alm_movimiento ADD entregado_a NVARCHAR(160) NULL;
END
GO

-- ── 2. Ampliar el CHECK de tipo para admitir Transferencia ────────
IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_alm_movimiento_tipo'
      AND parent_object_id = OBJECT_ID('alm_movimiento'))
BEGIN
    ALTER TABLE alm_movimiento DROP CONSTRAINT CK_alm_movimiento_tipo;
END
GO

ALTER TABLE alm_movimiento WITH CHECK
    ADD CONSTRAINT CK_alm_movimiento_tipo
    CHECK (tipo IN ('Entrada', 'Salida', 'Merma', 'Ajuste', 'Transferencia'));
GO

-- ── 3. Vista con entregado_a ──────────────────────────────────────
IF OBJECT_ID('vw_alm_movimientos') IS NOT NULL
    DROP VIEW vw_alm_movimientos;
GO

CREATE VIEW vw_alm_movimientos AS
SELECT
    m.movimiento_id,
    m.tipo,
    m.ajuste_subtipo,
    m.fecha,
    m.cantidad,
    m.vale,
    m.programa,
    m.orden_suministro,
    m.area_destino,
    m.entregado_a,
    m.notas,
    m.imagen_base64,
    m.creado_en,
    l.lote,
    l.caducidad,
    a.articulo_id,
    a.clave_ssa,
    a.nombre,
    a.unidad_medida,
    p.nombre   AS proveedor,
    u.nombre_usuario AS usuario
FROM alm_movimiento m
JOIN  alm_lote      l ON l.lote_id     = m.lote_id
JOIN  alm_articulo  a ON a.articulo_id = m.articulo_id
LEFT JOIN alm_proveedor p ON p.proveedor_id = m.proveedor_id
JOIN  app_usuario   u ON u.usuario_id  = m.usuario_id;
GO
