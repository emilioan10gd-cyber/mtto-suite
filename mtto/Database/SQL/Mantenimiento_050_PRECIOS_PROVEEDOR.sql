-- ============================================================================
-- MANTENIMIENTO (MTTO): COMPARATIVA DE PRECIOS POR PROVEEDOR
-- ============================================================================
-- Creado: 2026-07-28
--
-- Hasta ahora un artículo solo guardaba UN proveedor y UN costo_unitario
-- (mtto_articulo.proveedor_id / costo_unitario), que sigue existiendo y
-- representa el costo de referencia usado para valuar el inventario
-- (valor_total = stock_actual * costo_unitario).
--
-- Esta tabla es un catálogo aparte, independiente de esa valuación: permite
-- registrar CUÁNTO cobra CADA proveedor por el MISMO artículo, con distintos
-- niveles de precio (menudeo/mayoreo), para poder comparar y decidir a quién
-- comprarle. No tiene columna "activo": a diferencia de catálogos y artículos,
-- un precio de proveedor no tiene movimientos ni otros registros que dependan
-- de él, así que si ya no aplica simplemente se edita o se borra.
-- ============================================================================

USE [InventarioMantenimiento];
GO

IF OBJECT_ID('dbo.mtto_precio_proveedor', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_precio_proveedor (
        precio_id       INT IDENTITY(1,1) PRIMARY KEY,
        articulo_id     INT             NOT NULL,
        proveedor_id    INT             NOT NULL,
        -- 'Menudeo' (cantidad_minima=1, el default) o 'Mayoreo' (cantidad_minima
        -- es el mínimo de piezas al que aplica ese precio, p.ej. 12 = por docena).
        tipo_precio     NVARCHAR(20)    NOT NULL CONSTRAINT DF_mtto_precio_tipo DEFAULT (N'Menudeo'),
        cantidad_minima DECIMAL(18,3)   NOT NULL CONSTRAINT DF_mtto_precio_cant DEFAULT (1),
        -- Siempre precio POR UNIDAD (no precio total del lote), así todas las
        -- filas son directamente comparables entre sí sin conversiones.
        precio_unitario DECIMAL(18,4)   NOT NULL,
        notas           NVARCHAR(255)   NULL,
        actualizado_en  DATETIME2(0)    NOT NULL CONSTRAINT DF_mtto_precio_act DEFAULT (SYSDATETIME()),
        creado_en       DATETIME2(0)    NOT NULL CONSTRAINT DF_mtto_precio_creado DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_mtto_precio_articulo FOREIGN KEY (articulo_id) REFERENCES dbo.mtto_articulo(articulo_id),
        CONSTRAINT FK_mtto_precio_proveedor FOREIGN KEY (proveedor_id) REFERENCES dbo.mtto_proveedor(proveedor_id),
        CONSTRAINT CK_mtto_precio_tipo CHECK (tipo_precio IN (N'Menudeo', N'Mayoreo')),
        CONSTRAINT CK_mtto_precio_positivo CHECK (precio_unitario > 0),
        CONSTRAINT CK_mtto_precio_cantidad_positiva CHECK (cantidad_minima > 0),
        -- Mismo proveedor no puede repetir la misma combinación tipo+cantidad
        -- para el mismo artículo (sí puede tener Menudeo Y Mayoreo a la vez).
        CONSTRAINT UQ_mtto_precio_combo UNIQUE (articulo_id, proveedor_id, tipo_precio, cantidad_minima)
    );
    PRINT 'Tabla mtto_precio_proveedor creada';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_mtto_precio_articulo' AND object_id = OBJECT_ID('dbo.mtto_precio_proveedor'))
BEGIN
    CREATE INDEX IX_mtto_precio_articulo ON dbo.mtto_precio_proveedor(articulo_id);
    PRINT 'Índice IX_mtto_precio_articulo creado';
END
GO
