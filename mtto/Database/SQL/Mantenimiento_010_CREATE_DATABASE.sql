-- ============================================================================
-- MANTENIMIENTO (MTTO): BASE DE DATOS + ESQUEMA
-- ============================================================================
-- Creado: 2026-07-28
-- Origen: MTTO_COMPLETE_DASHBOARD.xlsx (Inventario del Área de Mantenimiento)
-- Hojas del Excel -> objetos SQL:
--   Catálogos          -> mtto_categoria, mtto_almacen, mtto_proveedor,
--                         mtto_unidad_medida, mtto_estado_articulo,
--                         mtto_tipo_movimiento
--   Inventario         -> mtto_articulo  (vista vw_mtto_inventario)
--   Entradas y Salidas -> mtto_movimiento
--   Dashboard          -> vistas vw_mtto_* (KPIs calculados, no se almacenan)
-- ============================================================================

IF DB_ID('InventarioMantenimiento') IS NULL
BEGIN
    CREATE DATABASE [InventarioMantenimiento] COLLATE Modern_Spanish_CI_AI;
    PRINT 'Base de datos InventarioMantenimiento creada';
END
GO

USE [InventarioMantenimiento];
GO

-- ============================================================================
-- 1. CATÁLOGOS DE LISTAS FIJAS
-- ============================================================================

-- Unidad de Medida (Pieza, Kg, Litro, ...)
IF OBJECT_ID('dbo.mtto_unidad_medida', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_unidad_medida (
        unidad_id       INT IDENTITY(1,1) PRIMARY KEY,
        nombre          NVARCHAR(40)  NOT NULL,
        abreviatura     NVARCHAR(10)  NULL,
        permite_decimal BIT           NOT NULL CONSTRAINT DF_mtto_unidad_dec DEFAULT (0),
        activo          BIT           NOT NULL CONSTRAINT DF_mtto_unidad_act DEFAULT (1),
        CONSTRAINT UQ_mtto_unidad_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla mtto_unidad_medida creada';
END
GO

-- Estado del Artículo (Activo, Inactivo, Descontinuado, Disponible)
IF OBJECT_ID('dbo.mtto_estado_articulo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_estado_articulo (
        estado_id  INT IDENTITY(1,1) PRIMARY KEY,
        nombre     NVARCHAR(40) NOT NULL,
        -- cuenta como existencia utilizable en los KPIs del dashboard
        es_activo  BIT NOT NULL CONSTRAINT DF_mtto_estado_act DEFAULT (1),
        CONSTRAINT UQ_mtto_estado_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla mtto_estado_articulo creada';
END
GO

-- Tipo de Movimiento (Entrada, Salida, Ajuste (+), Ajuste (-), Transferencia)
IF OBJECT_ID('dbo.mtto_tipo_movimiento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_tipo_movimiento (
        tipo_id  INT IDENTITY(1,1) PRIMARY KEY,
        nombre   NVARCHAR(40) NOT NULL,
        -- efecto sobre stock_actual del almacén origen: +1 suma, -1 resta, 0 neutro
        signo    SMALLINT NOT NULL CONSTRAINT DF_mtto_tipomov_signo DEFAULT (0),
        -- si requiere almacén destino (Transferencia)
        requiere_destino BIT NOT NULL CONSTRAINT DF_mtto_tipomov_dest DEFAULT (0),
        CONSTRAINT UQ_mtto_tipomov_nombre UNIQUE (nombre),
        CONSTRAINT CK_mtto_tipomov_signo CHECK (signo IN (-1, 0, 1))
    );
    PRINT 'Tabla mtto_tipo_movimiento creada';
END
GO

-- ============================================================================
-- 2. CATÁLOGOS CON CÓDIGO (CAT-001, ALM-001, PRV-001)
-- ============================================================================

IF OBJECT_ID('dbo.mtto_categoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_categoria (
        categoria_id INT IDENTITY(1,1) PRIMARY KEY,
        codigo       NVARCHAR(20)  NOT NULL,   -- CAT-001
        nombre       NVARCHAR(80)  NOT NULL,
        descripcion  NVARCHAR(255) NULL,
        activo       BIT NOT NULL CONSTRAINT DF_mtto_categoria_act DEFAULT (1),
        creado_en    DATETIME2(0) NOT NULL CONSTRAINT DF_mtto_categoria_cre DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_mtto_categoria_codigo UNIQUE (codigo),
        CONSTRAINT UQ_mtto_categoria_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla mtto_categoria creada';
END
GO

IF OBJECT_ID('dbo.mtto_almacen', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_almacen (
        almacen_id  INT IDENTITY(1,1) PRIMARY KEY,
        codigo      NVARCHAR(20)  NOT NULL,   -- ALM-001
        nombre      NVARCHAR(80)  NOT NULL,
        ubicacion   NVARCHAR(150) NULL,
        responsable NVARCHAR(120) NULL,
        activo      BIT NOT NULL CONSTRAINT DF_mtto_almacen_act DEFAULT (1),
        creado_en   DATETIME2(0) NOT NULL CONSTRAINT DF_mtto_almacen_cre DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_mtto_almacen_codigo UNIQUE (codigo),
        CONSTRAINT UQ_mtto_almacen_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla mtto_almacen creada';
END
GO

IF OBJECT_ID('dbo.mtto_proveedor', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_proveedor (
        proveedor_id INT IDENTITY(1,1) PRIMARY KEY,
        codigo       NVARCHAR(20)  NOT NULL,   -- PRV-001
        nombre       NVARCHAR(150) NOT NULL,
        contacto     NVARCHAR(120) NULL,
        telefono     NVARCHAR(40)  NULL,
        correo       NVARCHAR(120) NULL,
        activo       BIT NOT NULL CONSTRAINT DF_mtto_proveedor_act DEFAULT (1),
        creado_en    DATETIME2(0) NOT NULL CONSTRAINT DF_mtto_proveedor_cre DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_mtto_proveedor_codigo UNIQUE (codigo),
        CONSTRAINT UQ_mtto_proveedor_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla mtto_proveedor creada';
END
GO

-- ============================================================================
-- 3. ARTÍCULO (hoja "Inventario")
-- ============================================================================
IF OBJECT_ID('dbo.mtto_articulo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_articulo (
        articulo_id     INT IDENTITY(1,1) PRIMARY KEY,
        codigo          NVARCHAR(20)  NOT NULL,   -- MT-0020
        nombre          NVARCHAR(250) NOT NULL,
        descripcion     NVARCHAR(500) NULL,
        categoria_id    INT NOT NULL,
        unidad_id       INT NOT NULL,
        almacen_id      INT NOT NULL,
        proveedor_id    INT NULL,
        estado_id       INT NOT NULL,

        stock_actual    DECIMAL(18,3) NOT NULL CONSTRAINT DF_mtto_art_stock  DEFAULT (0),
        stock_minimo    DECIMAL(18,3) NOT NULL CONSTRAINT DF_mtto_art_min    DEFAULT (0),
        stock_maximo    DECIMAL(18,3) NULL,
        costo_unitario  DECIMAL(18,4) NULL,

        -- Excel columna "Valor Total": se calcula, nunca se captura
        valor_total AS (CONVERT(DECIMAL(18,2), stock_actual * ISNULL(costo_unitario, 0))) PERSISTED,

        actualizado_en  DATETIME2(0) NOT NULL CONSTRAINT DF_mtto_art_act DEFAULT (SYSDATETIME()),
        creado_en       DATETIME2(0) NOT NULL CONSTRAINT DF_mtto_art_cre DEFAULT (SYSDATETIME()),

        CONSTRAINT UQ_mtto_articulo_codigo UNIQUE (codigo),
        CONSTRAINT FK_mtto_art_categoria FOREIGN KEY (categoria_id) REFERENCES dbo.mtto_categoria(categoria_id),
        CONSTRAINT FK_mtto_art_unidad    FOREIGN KEY (unidad_id)    REFERENCES dbo.mtto_unidad_medida(unidad_id),
        CONSTRAINT FK_mtto_art_almacen   FOREIGN KEY (almacen_id)   REFERENCES dbo.mtto_almacen(almacen_id),
        CONSTRAINT FK_mtto_art_proveedor FOREIGN KEY (proveedor_id) REFERENCES dbo.mtto_proveedor(proveedor_id),
        CONSTRAINT FK_mtto_art_estado    FOREIGN KEY (estado_id)    REFERENCES dbo.mtto_estado_articulo(estado_id),
        CONSTRAINT CK_mtto_art_stock_pos CHECK (stock_actual >= 0),
        CONSTRAINT CK_mtto_art_min_max   CHECK (stock_maximo IS NULL OR stock_maximo >= stock_minimo)
    );

    CREATE INDEX IX_mtto_art_categoria ON dbo.mtto_articulo(categoria_id) INCLUDE (stock_actual, valor_total);
    CREATE INDEX IX_mtto_art_almacen   ON dbo.mtto_articulo(almacen_id);
    CREATE INDEX IX_mtto_art_estado    ON dbo.mtto_articulo(estado_id);
    CREATE INDEX IX_mtto_art_nombre    ON dbo.mtto_articulo(nombre);

    PRINT 'Tabla mtto_articulo creada';
END
GO

-- ============================================================================
-- 4. MOVIMIENTO (hoja "Entradas y Salidas")
-- ============================================================================
IF OBJECT_ID('dbo.mtto_movimiento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mtto_movimiento (
        movimiento_id      INT IDENTITY(1,1) PRIMARY KEY,
        folio              AS ('MOV-' + RIGHT('000000' + CONVERT(VARCHAR(10), movimiento_id), 6)) PERSISTED,
        fecha              DATETIME2(0) NOT NULL CONSTRAINT DF_mtto_mov_fecha DEFAULT (SYSDATETIME()),
        tipo_id            INT NOT NULL,
        articulo_id        INT NOT NULL,
        cantidad           DECIMAL(18,3) NOT NULL,
        almacen_id         INT NOT NULL,          -- almacén origen
        almacen_destino_id INT NULL,              -- solo Transferencia
        responsable        NVARCHAR(120) NULL,
        observaciones      NVARCHAR(500) NULL,

        -- trazabilidad: existencia antes/después para auditar sin recalcular
        stock_previo       DECIMAL(18,3) NULL,
        stock_resultante   DECIMAL(18,3) NULL,

        creado_en          DATETIME2(0) NOT NULL CONSTRAINT DF_mtto_mov_cre DEFAULT (SYSDATETIME()),
        creado_por         NVARCHAR(120) NULL,

        CONSTRAINT FK_mtto_mov_tipo      FOREIGN KEY (tipo_id)            REFERENCES dbo.mtto_tipo_movimiento(tipo_id),
        CONSTRAINT FK_mtto_mov_articulo  FOREIGN KEY (articulo_id)        REFERENCES dbo.mtto_articulo(articulo_id),
        CONSTRAINT FK_mtto_mov_almacen   FOREIGN KEY (almacen_id)         REFERENCES dbo.mtto_almacen(almacen_id),
        CONSTRAINT FK_mtto_mov_almdest   FOREIGN KEY (almacen_destino_id) REFERENCES dbo.mtto_almacen(almacen_id),
        CONSTRAINT CK_mtto_mov_cantidad  CHECK (cantidad > 0),
        CONSTRAINT CK_mtto_mov_destino   CHECK (almacen_destino_id IS NULL OR almacen_destino_id <> almacen_id)
    );

    CREATE INDEX IX_mtto_mov_fecha    ON dbo.mtto_movimiento(fecha DESC);
    CREATE INDEX IX_mtto_mov_articulo ON dbo.mtto_movimiento(articulo_id, fecha DESC);
    CREATE INDEX IX_mtto_mov_tipo     ON dbo.mtto_movimiento(tipo_id, fecha DESC);

    PRINT 'Tabla mtto_movimiento creada';
END
GO

PRINT '=== Esquema MTTO listo ===';
GO
