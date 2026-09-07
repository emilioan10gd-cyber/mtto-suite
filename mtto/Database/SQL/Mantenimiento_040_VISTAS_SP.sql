-- ============================================================================
-- MANTENIMIENTO (MTTO): VISTAS Y PROCEDIMIENTOS
-- ============================================================================
-- Reproducen las hojas "Inventario", "Entradas y Salidas" y "Dashboard".
-- ============================================================================

USE [InventarioMantenimiento];
GO

-- ============================================================================
-- VISTA: vw_mtto_inventario  (equivalente 1:1 a la hoja "Inventario")
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_mtto_inventario
AS
SELECT
    a.articulo_id,
    a.codigo                        AS [ID],
    a.nombre                        AS [Nombre del Articulo],
    c.nombre                        AS [Categoria],
    u.nombre                        AS [Unidad de Medida],
    al.nombre                       AS [Almacen],
    p.nombre                        AS [Proveedor],
    e.nombre                        AS [Estado],
    a.stock_actual                  AS [Stock Actual],
    a.stock_minimo                  AS [Stock Minimo],
    a.stock_maximo                  AS [Stock Maximo],
    a.costo_unitario                AS [Costo Unitario],
    a.valor_total                   AS [Valor Total],
    a.actualizado_en                AS [Ultima Actualizacion],
    -- semáforo de existencias para el dashboard
    CASE
        WHEN a.stock_actual <= 0                  THEN N'Sin stock'
        WHEN a.stock_actual < a.stock_minimo      THEN N'Bajo minimo'
        WHEN a.stock_maximo IS NOT NULL
             AND a.stock_actual > a.stock_maximo  THEN N'Sobre maximo'
        ELSE N'Normal'
    END                             AS [Nivel],
    a.categoria_id, a.unidad_id, a.almacen_id, a.proveedor_id, a.estado_id
FROM dbo.mtto_articulo a
JOIN dbo.mtto_categoria        c  ON c.categoria_id = a.categoria_id
JOIN dbo.mtto_unidad_medida    u  ON u.unidad_id    = a.unidad_id
JOIN dbo.mtto_almacen          al ON al.almacen_id  = a.almacen_id
JOIN dbo.mtto_estado_articulo  e  ON e.estado_id    = a.estado_id
LEFT JOIN dbo.mtto_proveedor   p  ON p.proveedor_id = a.proveedor_id;
GO

-- ============================================================================
-- VISTA: vw_mtto_movimientos  (hoja "Entradas y Salidas")
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_mtto_movimientos
AS
SELECT
    m.movimiento_id,
    m.folio                 AS [ID Movimiento],
    m.fecha                 AS [Fecha],
    t.nombre                AS [Tipo de Movimiento],
    a.codigo                AS [ID Articulo],
    a.nombre                AS [Nombre del Articulo],
    m.cantidad              AS [Cantidad],
    al.nombre               AS [Almacen],
    ald.nombre              AS [Almacen Destino],
    m.responsable           AS [Responsable],
    m.observaciones         AS [Observaciones],
    m.stock_previo,
    m.stock_resultante,
    t.signo,
    m.articulo_id, m.tipo_id, m.almacen_id
FROM dbo.mtto_movimiento m
JOIN dbo.mtto_tipo_movimiento t  ON t.tipo_id     = m.tipo_id
JOIN dbo.mtto_articulo        a  ON a.articulo_id = m.articulo_id
JOIN dbo.mtto_almacen         al ON al.almacen_id = m.almacen_id
LEFT JOIN dbo.mtto_almacen    ald ON ald.almacen_id = m.almacen_destino_id;
GO

-- ============================================================================
-- VISTA: vw_mtto_bajo_stock  (tarjeta "Artículos bajo stock mínimo")
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_mtto_bajo_stock
AS
SELECT
    a.articulo_id,
    a.codigo,
    a.nombre,
    c.nombre AS categoria,
    al.nombre AS almacen,
    a.stock_actual,
    a.stock_minimo,
    a.stock_minimo - a.stock_actual AS faltante
FROM dbo.mtto_articulo a
JOIN dbo.mtto_categoria c  ON c.categoria_id = a.categoria_id
JOIN dbo.mtto_almacen   al ON al.almacen_id  = a.almacen_id
WHERE a.stock_actual < a.stock_minimo;
GO

-- ============================================================================
-- VISTA: vw_mtto_resumen_categoria  (tabla "Resumen por categoría")
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_mtto_resumen_categoria
AS
SELECT
    c.categoria_id,
    c.codigo,
    c.nombre                                AS categoria,
    COUNT(a.articulo_id)                    AS total_articulos,
    ISNULL(SUM(a.stock_actual), 0)          AS total_existencias,
    ISNULL(SUM(a.valor_total), 0)           AS valor_total,
    SUM(CASE WHEN a.stock_actual < a.stock_minimo THEN 1 ELSE 0 END) AS bajo_minimo
FROM dbo.mtto_categoria c
LEFT JOIN dbo.mtto_articulo a ON a.categoria_id = c.categoria_id
GROUP BY c.categoria_id, c.codigo, c.nombre;
GO

-- ============================================================================
-- VISTA: vw_mtto_distribucion_estado  (gráfica "Distribución por estado")
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_mtto_distribucion_estado
AS
SELECT
    e.estado_id,
    e.nombre                     AS estado,
    COUNT(a.articulo_id)         AS total_articulos,
    ISNULL(SUM(a.valor_total),0) AS valor_total
FROM dbo.mtto_estado_articulo e
LEFT JOIN dbo.mtto_articulo a ON a.estado_id = e.estado_id
GROUP BY e.estado_id, e.nombre;
GO

-- ============================================================================
-- VISTA: vw_mtto_movimientos_mes  (gráfica "Movimientos por mes")
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_mtto_movimientos_mes
AS
SELECT
    YEAR(m.fecha)  AS anio,
    MONTH(m.fecha) AS mes,
    SUM(CASE WHEN t.signo > 0 THEN 1 ELSE 0 END) AS entradas,
    SUM(CASE WHEN t.signo < 0 THEN 1 ELSE 0 END) AS salidas,
    COUNT(*)                                     AS total_movimientos,
    ISNULL(SUM(CASE WHEN t.signo > 0 THEN m.cantidad END), 0) AS unidades_entrada,
    ISNULL(SUM(CASE WHEN t.signo < 0 THEN m.cantidad END), 0) AS unidades_salida
FROM dbo.mtto_movimiento m
JOIN dbo.mtto_tipo_movimiento t ON t.tipo_id = m.tipo_id
GROUP BY YEAR(m.fecha), MONTH(m.fecha);
GO

-- ============================================================================
-- VISTA: vw_mtto_dashboard_kpi  (fila de tarjetas superiores del Dashboard)
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_mtto_dashboard_kpi
AS
SELECT
    (SELECT COUNT(*) FROM dbo.mtto_articulo)                       AS total_articulos,
    (SELECT ISNULL(SUM(valor_total), 0) FROM dbo.mtto_articulo)    AS valor_total_inventario,
    (SELECT COUNT(*) FROM dbo.mtto_articulo
      WHERE stock_actual < stock_minimo)                           AS articulos_bajo_minimo,
    (SELECT COUNT(*) FROM dbo.mtto_articulo a
       JOIN dbo.mtto_estado_articulo e ON e.estado_id = a.estado_id
      WHERE e.es_activo = 1)                                       AS articulos_activos,
    (SELECT COUNT(*) FROM dbo.mtto_movimiento
      WHERE fecha >= DATEFROMPARTS(YEAR(SYSDATETIME()), MONTH(SYSDATETIME()), 1)
        AND fecha <  DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(SYSDATETIME()), MONTH(SYSDATETIME()), 1)))
                                                                   AS movimientos_del_mes;
GO

-- ============================================================================
-- SP: usp_mtto_registrar_movimiento
-- ----------------------------------------------------------------------------
-- Única vía autorizada para mover existencias: escribe el movimiento y ajusta
-- mtto_articulo.stock_actual en la misma transacción, dejando stock_previo /
-- stock_resultante para auditoría.
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_mtto_registrar_movimiento
    @codigo_articulo    NVARCHAR(20),
    @tipo_movimiento    NVARCHAR(40),
    @cantidad           DECIMAL(18,3),
    @responsable        NVARCHAR(120) = NULL,
    @observaciones      NVARCHAR(500) = NULL,
    @almacen_destino    NVARCHAR(80)  = NULL,   -- nombre o código, solo Transferencia
    @fecha              DATETIME2(0)  = NULL,
    @movimiento_id      INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @articulo_id INT, @almacen_id INT, @stock_previo DECIMAL(18,3);
    DECLARE @tipo_id INT, @signo SMALLINT, @requiere_destino BIT;
    DECLARE @destino_id INT = NULL, @stock_nuevo DECIMAL(18,3);

    IF @cantidad IS NULL OR @cantidad <= 0
        THROW 50010, 'La cantidad debe ser mayor a cero.', 1;

    SELECT @articulo_id = articulo_id, @almacen_id = almacen_id, @stock_previo = stock_actual
      FROM dbo.mtto_articulo WHERE codigo = @codigo_articulo;
    IF @articulo_id IS NULL
        THROW 50011, 'El artículo no existe.', 1;

    SELECT @tipo_id = tipo_id, @signo = signo, @requiere_destino = requiere_destino
      FROM dbo.mtto_tipo_movimiento WHERE nombre = @tipo_movimiento;
    IF @tipo_id IS NULL
        THROW 50012, 'Tipo de movimiento no válido.', 1;

    IF @requiere_destino = 1
    BEGIN
        SELECT @destino_id = almacen_id FROM dbo.mtto_almacen
         WHERE nombre = @almacen_destino OR codigo = @almacen_destino;
        IF @destino_id IS NULL
            THROW 50013, 'La transferencia requiere un almacén destino válido.', 1;
        IF @destino_id = @almacen_id
            THROW 50014, 'El almacén destino debe ser distinto al de origen.', 1;
    END

    SET @stock_nuevo = @stock_previo + (@signo * @cantidad);
    IF @stock_nuevo < 0
        THROW 50015, 'Existencia insuficiente para registrar la salida.', 1;

    BEGIN TRAN;

        UPDATE dbo.mtto_articulo
           SET stock_actual   = @stock_nuevo,
               actualizado_en = SYSDATETIME()
         WHERE articulo_id = @articulo_id;

        INSERT INTO dbo.mtto_movimiento
            (fecha, tipo_id, articulo_id, cantidad, almacen_id, almacen_destino_id,
             responsable, observaciones, stock_previo, stock_resultante, creado_por)
        VALUES
            (ISNULL(@fecha, SYSDATETIME()), @tipo_id, @articulo_id, @cantidad,
             @almacen_id, @destino_id, @responsable, @observaciones,
             @stock_previo, @stock_nuevo, SUSER_SNAME());

        SET @movimiento_id = SCOPE_IDENTITY();

    COMMIT TRAN;

    SELECT @movimiento_id AS movimiento_id,
           (SELECT folio FROM dbo.mtto_movimiento WHERE movimiento_id = @movimiento_id) AS folio,
           @stock_previo AS stock_previo,
           @stock_nuevo  AS stock_resultante;
END
GO

-- ============================================================================
-- SP: usp_mtto_alta_articulo  (upsert por código)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_mtto_alta_articulo
    @codigo         NVARCHAR(20),
    @nombre         NVARCHAR(250),
    @categoria      NVARCHAR(80),
    @unidad         NVARCHAR(40),
    @almacen        NVARCHAR(80),
    @estado         NVARCHAR(40)  = N'Disponible',
    @proveedor      NVARCHAR(150) = NULL,
    @stock_actual   DECIMAL(18,3) = 0,
    @stock_minimo   DECIMAL(18,3) = 0,
    @stock_maximo   DECIMAL(18,3) = NULL,
    @costo_unitario DECIMAL(18,4) = NULL,
    @descripcion    NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @categoria_id INT, @unidad_id INT, @almacen_id INT, @estado_id INT, @proveedor_id INT;

    SELECT @categoria_id = categoria_id FROM dbo.mtto_categoria       WHERE nombre = @categoria OR codigo = @categoria;
    SELECT @unidad_id    = unidad_id    FROM dbo.mtto_unidad_medida   WHERE nombre = @unidad;
    SELECT @almacen_id   = almacen_id   FROM dbo.mtto_almacen         WHERE nombre = @almacen OR codigo = @almacen;
    SELECT @estado_id    = estado_id    FROM dbo.mtto_estado_articulo WHERE nombre = @estado;
    IF @proveedor IS NOT NULL
        SELECT @proveedor_id = proveedor_id FROM dbo.mtto_proveedor    WHERE nombre = @proveedor OR codigo = @proveedor;

    IF @categoria_id IS NULL THROW 50020, 'Categoría no encontrada.', 1;
    IF @unidad_id    IS NULL THROW 50021, 'Unidad de medida no encontrada.', 1;
    IF @almacen_id   IS NULL THROW 50022, 'Almacén no encontrado.', 1;
    IF @estado_id    IS NULL THROW 50023, 'Estado no encontrado.', 1;

    MERGE dbo.mtto_articulo AS t
    USING (SELECT @codigo AS codigo) AS s
       ON t.codigo = s.codigo
    WHEN MATCHED THEN UPDATE SET
        nombre = @nombre, descripcion = @descripcion, categoria_id = @categoria_id,
        unidad_id = @unidad_id, almacen_id = @almacen_id, proveedor_id = @proveedor_id,
        estado_id = @estado_id, stock_actual = @stock_actual, stock_minimo = @stock_minimo,
        stock_maximo = @stock_maximo, costo_unitario = @costo_unitario,
        actualizado_en = SYSDATETIME()
    WHEN NOT MATCHED THEN INSERT
        (codigo, nombre, descripcion, categoria_id, unidad_id, almacen_id, proveedor_id,
         estado_id, stock_actual, stock_minimo, stock_maximo, costo_unitario)
        VALUES
        (@codigo, @nombre, @descripcion, @categoria_id, @unidad_id, @almacen_id, @proveedor_id,
         @estado_id, @stock_actual, @stock_minimo, @stock_maximo, @costo_unitario);

    SELECT * FROM dbo.vw_mtto_inventario WHERE [ID] = @codigo;
END
GO

PRINT '=== Vistas y procedimientos MTTO listos ===';
GO
