-- ============================================================
-- Módulo: Almacén Central de Material de Curación
-- Ejecutar contra la BD SQLEXPRESS del servidor mtto
-- Requiere: tablas app_usuario (ya existe)
-- ============================================================

-- ---- 1. Catálogo de artículos --------------------------------
IF OBJECT_ID('alm_articulo') IS NULL
CREATE TABLE alm_articulo (
    articulo_id     INT IDENTITY(1,1) PRIMARY KEY,
    clave_ssa       NVARCHAR(20)  NOT NULL,  -- ej. "060.030.0073"
    nombre          NVARCHAR(500) NOT NULL,
    unidad_medida   NVARCHAR(80)  NOT NULL,
    es_cpm          BIT           NOT NULL DEFAULT 0,
    precio_unitario DECIMAL(18,4) NULL,
    stock_minimo    DECIMAL(18,3) NOT NULL DEFAULT 0,
    stock_maximo    DECIMAL(18,3) NULL,
    programa        NVARCHAR(40)  NULL,   -- "U013", "INSABI", "COMPRA ESTATAL"
    activo          BIT           NOT NULL DEFAULT 1,
    creado_en       DATETIME2(0)  NOT NULL DEFAULT GETDATE(),
    actualizado_en  DATETIME2(0)  NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_alm_articulo_clave UNIQUE (clave_ssa)
);
GO

-- ---- 2. Proveedores ------------------------------------------
IF OBJECT_ID('alm_proveedor') IS NULL
CREATE TABLE alm_proveedor (
    proveedor_id  INT IDENTITY(1,1) PRIMARY KEY,
    nombre        NVARCHAR(200) NOT NULL,
    rfc           NVARCHAR(20)  NULL,
    activo        BIT           NOT NULL DEFAULT 1,
    creado_en     DATETIME2(0)  NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_alm_proveedor_nombre UNIQUE (nombre)
);
GO

-- ---- 3. Lotes (FEFO) -----------------------------------------
IF OBJECT_ID('alm_lote') IS NULL
CREATE TABLE alm_lote (
    lote_id     INT IDENTITY(1,1) PRIMARY KEY,
    articulo_id INT           NOT NULL REFERENCES alm_articulo(articulo_id),
    lote        NVARCHAR(60)  NOT NULL,
    caducidad   DATE          NULL,
    cantidad    DECIMAL(18,3) NOT NULL DEFAULT 0,
    creado_en   DATETIME2(0)  NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_alm_lote UNIQUE (articulo_id, lote)
);
GO

-- ---- 4. Movimientos ------------------------------------------
IF OBJECT_ID('alm_movimiento') IS NULL
CREATE TABLE alm_movimiento (
    movimiento_id    INT IDENTITY(1,1) PRIMARY KEY,
    tipo             NVARCHAR(20)  NOT NULL,  -- Entrada|Salida|Merma|Ajuste
    lote_id          INT           NOT NULL REFERENCES alm_lote(lote_id),
    articulo_id      INT           NOT NULL REFERENCES alm_articulo(articulo_id),
    cantidad         DECIMAL(18,3) NOT NULL,
    fecha            DATETIME2(0)  NOT NULL,
    proveedor_id     INT           NULL REFERENCES alm_proveedor(proveedor_id),
    vale             NVARCHAR(60)  NULL,
    programa         NVARCHAR(40)  NULL,
    orden_suministro NVARCHAR(120) NULL,
    area_destino     NVARCHAR(80)  NULL,
    notas            NVARCHAR(500) NULL,
    usuario_id       INT           NOT NULL REFERENCES app_usuario(usuario_id),
    creado_en        DATETIME2(0)  NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_alm_movimiento_tipo     CHECK (tipo IN ('Entrada','Salida','Merma','Ajuste')),
    CONSTRAINT CK_alm_movimiento_cantidad CHECK (cantidad > 0)
);
GO

-- ---- 5. Índices de búsqueda ----------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_alm_lote_caducidad')
    CREATE INDEX IX_alm_lote_caducidad ON alm_lote(caducidad) WHERE caducidad IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_alm_movimiento_fecha')
    CREATE INDEX IX_alm_movimiento_fecha ON alm_movimiento(fecha);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_alm_articulo_clave')
    CREATE INDEX IX_alm_articulo_clave ON alm_articulo(clave_ssa);
GO

-- ---- 6. Vista: inventario total por artículo -----------------
IF OBJECT_ID('vw_alm_inventario') IS NOT NULL DROP VIEW vw_alm_inventario;
GO
CREATE VIEW vw_alm_inventario AS
SELECT
    a.articulo_id,
    a.clave_ssa,
    a.nombre,
    a.unidad_medida,
    a.es_cpm,
    a.precio_unitario,
    a.stock_minimo,
    a.stock_maximo,
    a.programa,
    a.activo,
    a.actualizado_en,
    ISNULL(SUM(l.cantidad), 0)                                              AS stock_actual,
    COUNT(CASE WHEN l.cantidad > 0 THEN 1 END)                              AS lotes_con_existencia,
    MIN(CASE WHEN l.cantidad > 0 AND l.caducidad IS NOT NULL THEN l.caducidad END) AS caducidad_proxima,
    ISNULL(SUM(l.cantidad * a.precio_unitario), 0)                          AS valor_total,
    CASE
        WHEN ISNULL(SUM(l.cantidad), 0) = 0                                  THEN 'Sin stock'
        WHEN ISNULL(SUM(l.cantidad), 0) < a.stock_minimo AND a.stock_minimo > 0 THEN 'Bajo minimo'
        WHEN a.stock_maximo IS NOT NULL AND ISNULL(SUM(l.cantidad), 0) > a.stock_maximo THEN 'Sobre maximo'
        ELSE 'Normal'
    END AS nivel
FROM alm_articulo a
LEFT JOIN alm_lote l ON l.articulo_id = a.articulo_id
GROUP BY a.articulo_id, a.clave_ssa, a.nombre, a.unidad_medida,
         a.es_cpm, a.precio_unitario, a.stock_minimo, a.stock_maximo,
         a.programa, a.activo, a.actualizado_en;
GO

-- ---- 7. Vista: lotes con estado de caducidad -----------------
IF OBJECT_ID('vw_alm_lotes') IS NOT NULL DROP VIEW vw_alm_lotes;
GO
CREATE VIEW vw_alm_lotes AS
SELECT
    l.lote_id,
    l.articulo_id,
    l.lote,
    l.caducidad,
    l.cantidad,
    l.creado_en,
    a.clave_ssa,
    a.nombre,
    a.unidad_medida,
    DATEDIFF(DAY, GETDATE(), l.caducidad)   AS dias_para_caducar,
    CASE
        WHEN l.caducidad IS NULL             THEN 'Sin fecha'
        WHEN l.caducidad < CAST(GETDATE() AS DATE) THEN 'Vencido'
        WHEN l.caducidad < DATEADD(DAY, 90, CAST(GETDATE() AS DATE)) THEN 'Por vencer'
        ELSE 'Vigente'
    END AS estado_caducidad
FROM alm_lote l
JOIN alm_articulo a ON a.articulo_id = l.articulo_id;
GO

-- ---- 8. Vista: movimientos con detalle -----------------------
IF OBJECT_ID('vw_alm_movimientos') IS NOT NULL DROP VIEW vw_alm_movimientos;
GO
CREATE VIEW vw_alm_movimientos AS
SELECT
    m.movimiento_id,
    m.tipo,
    m.fecha,
    m.cantidad,
    m.vale,
    m.programa,
    m.orden_suministro,
    m.area_destino,
    m.notas,
    m.creado_en,
    l.lote,
    l.caducidad,
    a.articulo_id,
    a.clave_ssa,
    a.nombre,
    a.unidad_medida,
    p.nombre AS proveedor,
    u.nombre_usuario AS usuario
FROM alm_movimiento m
JOIN  alm_lote      l ON l.lote_id     = m.lote_id
JOIN  alm_articulo  a ON a.articulo_id = m.articulo_id
LEFT JOIN alm_proveedor p ON p.proveedor_id = m.proveedor_id
JOIN  app_usuario   u ON u.usuario_id  = m.usuario_id;
GO

-- ---- 9. Actualizar constraint de area (si existe) -----------
-- La columna area en app_usuario no tiene CHECK a nivel BD (es solo NVARCHAR),
-- por lo que no hay nada que alterar aquí. El área "almacen" ya funciona.

PRINT '✓ Almacén Central: tablas y vistas creadas correctamente.';
