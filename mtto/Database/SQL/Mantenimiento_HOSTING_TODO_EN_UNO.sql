-- ============================================================================
-- MANTENIMIENTO (MTTO): SCRIPT UNICO PARA HOSTING
-- ============================================================================
-- Generado automaticamente a partir de los 4 scripts numerados. No lo edites
-- a mano: si cambias el esquema, cambia los scripts 010-040 y regenera.
--
-- DIFERENCIA CON LOS SCRIPTS LOCALES:
--   Este NO hace CREATE DATABASE ni USE. Los hostings compartidos te entregan
--   una base ya creada, con un nombre que ellos eligen, y el usuario no tiene
--   permiso para crear otras.
--
-- COMO USARLO en freeasphosting.net:
--   1. En el panel, crea la base de datos y anota nombre/usuario/contrasena.
--   2. Abre su consola SQL (o conectate con SSMS a su servidor).
--   3. Asegurate de estar posicionado en TU base, no en master.
--   4. Pega y ejecuta este archivo completo.
--   5. Copia esos datos al Web.Release.config antes de publicar la API.
--
-- Es idempotente: si algo falla a media carga, puedes volver a correrlo entero.
--
-- NOTA: si su consola web no acepta la palabra GO, ejecuta por bloques,
-- separando justo en cada linea que diga GO.
-- ============================================================================

SET NOCOUNT ON;
GO


-- ##########################################################################
-- ## ESQUEMA: tablas, indices y llaves foraneas
-- ## (origen: Mantenimiento_010_CREATE_DATABASE.sql)
-- ##########################################################################

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

GO


-- ##########################################################################
-- ## CATALOGOS
-- ## (origen: Mantenimiento_020_CATALOGOS.sql)
-- ##########################################################################

-- ============================================================================
-- MANTENIMIENTO (MTTO): CARGA DE CATÁLOGOS
-- ============================================================================
-- Origen: hoja "Catálogos" de MTTO_COMPLETE_DASHBOARD.xlsx
-- Idempotente: se puede re-ejecutar sin duplicar.
-- ============================================================================

-- ---------------------------------------------------------------- Unidades
MERGE dbo.mtto_unidad_medida AS t
USING (VALUES
    (N'Pieza',    N'pza', 0),
    (N'Kg',       N'kg',  1),
    (N'Gramo',    N'g',   1),
    (N'Litro',    N'L',   1),
    (N'Mililitro',N'mL',  1),
    (N'Metro',    N'm',   1),
    (N'Caja',     N'cja', 0),
    (N'Paquete',  N'paq', 0),
    (N'Rollo',    N'rll', 0),
    (N'Par',      N'par', 0),
    (N'Bulto',    N'blt', 0),
    (N'Kit',      N'kit', 0),
    (N'Galón',    N'gal', 1)
) AS s(nombre, abreviatura, permite_decimal)
    ON t.nombre = s.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, abreviatura, permite_decimal) VALUES (s.nombre, s.abreviatura, s.permite_decimal);
GO

-- ------------------------------------------------------- Estados de artículo
MERGE dbo.mtto_estado_articulo AS t
USING (VALUES
    (N'Disponible',    1),
    (N'Activo',        1),
    (N'Inactivo',      0),
    (N'Descontinuado', 0)
) AS s(nombre, es_activo)
    ON t.nombre = s.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, es_activo) VALUES (s.nombre, s.es_activo);
GO

-- -------------------------------------------------------- Tipos de movimiento
MERGE dbo.mtto_tipo_movimiento AS t
USING (VALUES
    (N'Entrada',        1, 0),
    (N'Salida',        -1, 0),
    (N'Ajuste (+)',     1, 0),
    (N'Ajuste (-)',    -1, 0),
    (N'Transferencia', -1, 1)   -- resta en origen, suma en destino
) AS s(nombre, signo, requiere_destino)
    ON t.nombre = s.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, signo, requiere_destino) VALUES (s.nombre, s.signo, s.requiere_destino);
GO

-- ------------------------------------------------------------------ Categorías
MERGE dbo.mtto_categoria AS t
USING (VALUES
    (N'CAT-001', N'Plomería'),
    (N'CAT-002', N'Sanitarios'),
    (N'CAT-003', N'Refrigeración / HVAC'),
    (N'CAT-004', N'Electricidad'),
    (N'CAT-005', N'Iluminación'),
    (N'CAT-006', N'Pintura'),
    (N'CAT-007', N'Herramientas'),
    (N'CAT-008', N'Ferretería'),
    (N'CAT-009', N'PVC'),
    (N'CAT-010', N'Cobre'),
    (N'CAT-011', N'Químicos'),
    (N'CAT-012', N'Limpieza'),
    (N'CAT-013', N'Seguridad'),
    (N'CAT-014', N'Consumibles'),
    (N'CAT-015', N'Otros')
) AS s(codigo, nombre)
    ON t.codigo = s.codigo
WHEN NOT MATCHED THEN
    INSERT (codigo, nombre) VALUES (s.codigo, s.nombre);
GO

-- -------------------------------------------------------------------- Almacenes
MERGE dbo.mtto_almacen AS t
USING (VALUES
    (N'ALM-001', N'Almacén A', N'Área de Mantenimiento')
) AS s(codigo, nombre, ubicacion)
    ON t.codigo = s.codigo
WHEN NOT MATCHED THEN
    INSERT (codigo, nombre, ubicacion) VALUES (s.codigo, s.nombre, s.ubicacion);
GO

-- Proveedores: el Excel no trae ninguno capturado todavía (columna vacía en las
-- 220 filas de Inventario). La tabla queda lista para alta desde el dashboard.

PRINT '=== Catálogos MTTO cargados ===';
SELECT 'unidad_medida'   AS catalogo, COUNT(*) AS filas FROM dbo.mtto_unidad_medida
UNION ALL SELECT 'estado_articulo',  COUNT(*) FROM dbo.mtto_estado_articulo
UNION ALL SELECT 'tipo_movimiento',  COUNT(*) FROM dbo.mtto_tipo_movimiento
UNION ALL SELECT 'categoria',        COUNT(*) FROM dbo.mtto_categoria
UNION ALL SELECT 'almacen',          COUNT(*) FROM dbo.mtto_almacen
UNION ALL SELECT 'proveedor',        COUNT(*) FROM dbo.mtto_proveedor;
GO

GO


-- ##########################################################################
-- ## DATOS: 220 articulos
-- ## (origen: Mantenimiento_030_DATOS_INVENTARIO.sql)
-- ##########################################################################

-- ==========================================================================
-- MANTENIMIENTO (MTTO): CARGA DE ARTÍCULOS
-- ==========================================================================
-- Origen: hoja "Inventario" de MTTO_COMPLETE_DASHBOARD.xlsx
-- Filas: 220 artículos (MT-0001 .. MT-0220)
-- Generado: 2026-07-28
--
-- Idempotente: MERGE por codigo. Re-ejecutar actualiza, no duplica.
-- NOTA: el Excel trae Proveedor, Stock Máximo y Costo Unitario vacíos en las
--       220 filas; quedan NULL y se capturan desde el dashboard.
-- ==========================================================================

SET NOCOUNT ON;

-- Tabla temporal con los datos tal cual salen del Excel (texto),
-- para resolver las llaves foráneas por nombre en un solo MERGE.
-- COLLATE DATABASE_DEFAULT: sin esto las columnas heredan la collation de
-- tempdb (servidor, _CI_AS) y el JOIN contra los catálogos (_CI_AI) revienta.
IF OBJECT_ID('tempdb..#mtto_carga') IS NOT NULL DROP TABLE #mtto_carga;
CREATE TABLE #mtto_carga (
    codigo         NVARCHAR(20)  COLLATE DATABASE_DEFAULT,
    nombre         NVARCHAR(250) COLLATE DATABASE_DEFAULT,
    categoria      NVARCHAR(80)  COLLATE DATABASE_DEFAULT,
    unidad         NVARCHAR(40)  COLLATE DATABASE_DEFAULT,
    almacen        NVARCHAR(80)  COLLATE DATABASE_DEFAULT,
    proveedor      NVARCHAR(150) COLLATE DATABASE_DEFAULT NULL,
    estado         NVARCHAR(40)  COLLATE DATABASE_DEFAULT,
    stock_actual   DECIMAL(18,3),
    stock_minimo   DECIMAL(18,3),
    stock_maximo   DECIMAL(18,3) NULL,
    costo_unitario DECIMAL(18,4) NULL,
    actualizado_en DATETIME2(0)
);

INSERT INTO #mtto_carga (codigo, nombre, categoria, unidad, almacen, proveedor, estado, stock_actual, stock_minimo, stock_maximo, costo_unitario, actualizado_en) VALUES
(N'MT-0020', N'Llave angular 1/2" a 1/2", marca Coflex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 10, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0021', N'Llave angular 1/2" a 3/8", marca Coflex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 15, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0022', N'Llave angular 1/2" a 3/8", marca Tiemme', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 8, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0083', N'Llave jardinera 1/2", marca Promine', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0101', N'Llave para jardín 3/4"', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0120', N'Llave de jardín 3/4"', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0005', N'Mezcladora para lavabo, marca Rugo', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0006', N'Mezcladora para tarja, marca Coflex Home', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0042', N'Mezcladora para fregadero 8", marca Rugo', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0218', N'Mezcladora de pared para tarja', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0010', N'Manguera para llave monomando 3/8" a 1/2" estándar, marca Coflex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 12, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0011', N'Manguera 1/2" a 1/2" para fregadero a lavabo, marca Coflex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 14, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0012', N'Manguera de lavado a fregadero 1/2" a 3/8", marca Coflex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 27, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0192', N'Manguera para jardín 5/8" 20m', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0008', N'Contracanasta para fregadero, marca Coflex, modelo PH-050', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 13, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0014', N'Trampa para tarja doble, marca Fleximatic', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0015', N'Trampa sencilla para fregadero', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 11, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0016', N'Trampa multiposición para fregadero, marca Coflex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0103', N'Cespol para fregadero, marca Helvex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0041', N'Fluxómetro mecánico para sanitario, 6 litros, marca Sloane', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0104', N'Fluxómetro (émbolo) SF-719, marca Helvex, 6 litros', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0085', N'Repuesto para válvula Sloane, modelo A1041A', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0107', N'Válvula de esfera 1/2"', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0108', N'Válvula 1-1/4" cierre rápido', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0119', N'Válvula de paso 3/4" de volante', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0161', N'Válvula pera para mezcladora, derecha, SH1012', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 8, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0094', N'Chupón reductor 2" a 1"', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0115', N'Reducción galvanizada 1/2" a 1/4", marca Gucci', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0197', N'Tapón cachucha 1/2"', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 8, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0013', N'Salida central para fregadero doble tarja, marca Coflex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 5, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0023', N'Brida flexible 4", marca Coflex', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 8, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0088', N'Extensión para lavabo 1" a 1"', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0080', N'Bomba sumergible para aguas residuales, 1 HP', N'Plomería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0003', N'Tapa para sanitario, marca Foset Aqua', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0007', N'Tapa para sanitario, marca Helvex', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0165', N'Cuello de cera para WC con guía', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 9, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0017', N'Juego de reparación para sanitario / herraje de 3"', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0066', N'Mecanismo de cierre de llave, modelo SF-015C, marca Helvex', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 8, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0098', N'Flotador 6" para cisterna', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0117', N'Scoot para sanitario', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0162', N'Torpedo armado SF110', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0185', N'Juego de reparación para sanitario 2", marca Coflex', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0193', N'Mecanismo de cierre SF0115-C, marca Helvex', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 5, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0019', N'Tubo cromado 1" a 1/4" para sanitario, marca Helvex', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0039', N'Accesorio de pedal para sanitario, marca Helvex', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0219', N'Barra de agarre para sanitario', N'Sanitarios', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0087', N'Compresor para refrigerador 1/3 HP', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0004', N'Motor para condensador 1 HP, marca US Motor', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0009', N'Motor para condensador 3/4 HP, marca US Motor', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0076', N'Bomba para condensados, marca Asurity', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00');

INSERT INTO #mtto_carga (codigo, nombre, categoria, unidad, almacen, proveedor, estado, stock_actual, stock_minimo, stock_maximo, costo_unitario, actualizado_en) VALUES
(N'MT-0055', N'Contactor 3 polos, 60 Amper, bobina 28/240', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0056', N'Contactor trifásico 60 Amper, bobina 24V', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0057', N'Contactor 2 polos, 40 Amper, 24V', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0058', N'Contactor 2 polos, 40 Amper, bobina 28/240', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0059', N'Capacitor de 10 microfaradios', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0064', N'Capacitor 80+5, marca Quality', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0062', N'Filtro línea de líquido, modelo QDM 1/2" a 1/2", marca Quality', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0063', N'Filtro de succión QDS-V 7/8"', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0029', N'Gas refrigerante R-134A, cilindro 12 kg', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0030', N'Gas refrigerante R-410A, cilindro 11 kg', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0060', N'Switch de presión 130 libras, marca Quality', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0061', N'Switch de presión 450 libras', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0054', N'Polea para turbina de A/C, marca Trane, 15 Toneladas', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0211', N'Banda A43', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 5, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0212', N'Banda A23', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 12, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0213', N'Banda A26', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 12, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0214', N'Banda A20', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0139', N'Aceite para bomba de vacío, 1/2 litro', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 8, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0141', N'Cinta para aislamiento de A/C 2", marca Froster', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0142', N'Cinta tipo momia para tubería de minisplit', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0143', N'Pasta térmica Thermo Trap', N'Refrigeración / HVAC', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0067', N'Caja de cable calibre 12, marca Viakon', N'Electricidad', N'Caja', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0068', N'Caja de cable calibre 10, marca Viakon', N'Electricidad', N'Caja', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0069', N'Caja de cable CAT CCTV 1 par 24 AWG, marca Viakon', N'Electricidad', N'Caja', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0044', N'Clavija sencilla de hule ovalada negra, 15A', N'Electricidad', N'Pieza', N'Almacén A', NULL, N'Disponible', 10, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0045', N'Clavija uso rudo 15A, marca Boltram', N'Electricidad', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0051', N'Clavija de bloqueo, modelo EL6-20P', N'Electricidad', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0050', N'Caja registro reforzada 2x4', N'Electricidad', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0053', N'Fotocelda genérica', N'Electricidad', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0182', N'Fotocelda, marca Foto Control', N'Electricidad', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0195', N'Tapa ciega metálica', N'Electricidad', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0001', N'Lámpara LED 18W, marca Maigoo, modelo MG01505', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 10, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0220', N'Lámpara LED 30x120 cm', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 16, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0027', N'Lámpara 17W, marca Philips, modelo F17D8/TL865', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 30, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0047', N'Lámpara fluorescente azul 18W para fototerapia, marca Osmar', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0049', N'Foco ahorrador 9W, marca Aski', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0154', N'Foco LED luz cálida 60W', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0046', N'Portalámpara cuadrado de porcelana, marca Akzi', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0018', N'Balastro electrónico 2x28W, marca Lummi', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 12, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0072', N'Reflector LED 50W, marca Steren', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0073', N'Reflector LED luz azul', N'Iluminación', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0035', N'Pintura verde, marca Comex', N'Pintura', N'Galón', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0202', N'Pintura, marca Berel, código 406P', N'Pintura', N'Galón', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0203', N'Pintura verde, código M4-07', N'Pintura', N'Galón', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0209', N'Pintura Ecoxica roja con catalizador', N'Pintura', N'Galón', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0034', N'Esmalte amarillo, marca Sayer', N'Pintura', N'Galón', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0040', N'Pintura esmalte código 40306P', N'Pintura', N'Galón', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0208', N'Pintura código 40306P, marca Sayer', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0074', N'Aerosol esmalte cromo', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 10, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0075', N'Aerosol esmalte transparente', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 10, 5, NULL, NULL, '2026-07-18T00:00:00');

INSERT INTO #mtto_carga (codigo, nombre, categoria, unidad, almacen, proveedor, estado, stock_actual, stock_minimo, stock_maximo, costo_unitario, actualizado_en) VALUES
(N'MT-0148', N'Pintura en aerosol blanca', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0149', N'Pintura en aerosol dorada', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0183', N'Barniz foto-celulosa', N'Pintura', N'Litro', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0190', N'Barniz aislante eléctrico para motor', N'Pintura', N'Litro', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0033', N'Rodillo para pintura 9"', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 16, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0036', N'Mini rodillo para pintura 4"', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0201', N'Extensión de aluminio para pintura', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0176', N'Brocha 2", marca Comex', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0177', N'Brocha 4", marca Comex', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0178', N'Brocha sintética 2"', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 8, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0179', N'Brocha uso doméstico 6"', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0180', N'Espátula flexible 2", marca Berel', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0181', N'Espátula flexible 2", marca Truper', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0137', N'Cinta para empapelar 2"', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 15, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0147', N'Removedor de pintura', N'Pintura', N'Litro', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0186', N'Linaza para tratamiento de madera', N'Pintura', N'Litro', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0210', N'Impermeabilizante negro, marca Impact', N'Pintura', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0043', N'Pistola calafateadora, marca Truper', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0206', N'Raspador 7", marca Truper', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0084', N'Disco de corte para metal 4-1/2"', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 120, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0097', N'Disco laminado grano 40', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 22, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0187', N'Disco de corte 14", marca Magita', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0031', N'Cepillo de copa (cerda) entrada 5/8", diámetro 5", marca Truper', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 12, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0038', N'Cepillo de copa (cerda) 5/8" diámetro, 3", marca Truper', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 11, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0032', N'Felpa 3/8"', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 9, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0166', N'Cepillo de alambre', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 15, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0159', N'Embudo', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0175', N'Triángulo para afilar 7"', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0081', N'Ocómetro de manija, modelo 110WC.4.8', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0095', N'Tanque de acetileno desechable', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0102', N'Hilo para güira, marca Truper, 2mm', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0158', N'Adaptador para gasera', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0160', N'Vejiga mediana para limpieza de desagüe', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0188', N'Filtro de aspiradora 20 galones, marca Ridgid', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0189', N'Hilo redondo para güira 220m, marca Truper', N'Herramientas', N'Rollo', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0215', N'Extensión de fibra de vidrio 5m', N'Herramientas', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0100', N'Tornillo pija para sanitario', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0168', N'Bisagra 4"', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0171', N'Bisagra de piso, marca Philips', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0169', N'Cerrojo 1/2"', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0037', N'Pasador 1/2"', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0196', N'Pasador 2"', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0024', N'Cerradura marca Fanal, modelo 132, con pasador de seguridad', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0071', N'Cerradura 60-11, marca Infra', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 20, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0025', N'Chapa para baño, marca Philips', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0086', N'Chapa para baño, marca Hermex', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0145', N'Chapa para puerta de aluminio', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0194', N'Candado Master N°8', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0184', N'Mosquetón 3/16" 2", marca Fiero', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0198', N'Remache 3/16" a 3/8", marca Fiero', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 150, 5, NULL, NULL, '2026-07-18T00:00:00');

INSERT INTO #mtto_carga (codigo, nombre, categoria, unidad, almacen, proveedor, estado, stock_actual, stock_minimo, stock_maximo, costo_unitario, actualizado_en) VALUES
(N'MT-0199', N'Remache 3/16" a 1/2", marca Fiero', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 250, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0200', N'Remache 3/16" a 5/8", marca Fiero', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 150, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0099', N'Abrazadera roscada 2", modelo HU120200', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 15, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0170', N'Soporte reforzado para lavabo', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 5, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0002', N'Rueda para plataforma, marca Ceye', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0167', N'Pistón para cierrapuertas', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0191', N'Rueda para diablito 8"', N'Ferretería', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0089', N'Cople 1" PVC hidráulico', N'PVC', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0092', N'Cople 3/4" PVC hidráulico', N'PVC', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0090', N'Codo 1", 90°, PVC', N'PVC', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0091', N'Codo 3/4" PVC hidráulico', N'PVC', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0093', N'Te 3/4" PVC hidráulico', N'PVC', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0205', N'Canaleta 1"', N'PVC', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0111', N'Cople de cobre 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0112', N'Cople de cobre 2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0113', N'Cople de cobre 1-1/4"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0127', N'Cople de cobre 3/4"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0132', N'Cople de cobre 1"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0109', N'Codo de cobre 90°, 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0114', N'Codo de cobre 1-1/4"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 5, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0122', N'Codo de cobre 1/2", 40°', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0126', N'Codo de cobre 3/4"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0105', N'Conector hembra de cobre 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0110', N'Conector macho de cobre 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0125', N'Conector hembra de cobre 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0128', N'Conector hembra de cobre 3/4"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0129', N'Conector macho de cobre 3/4"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0134', N'Conector hembra de cobre 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0135', N'Conector macho de cobre 1-1/4"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 5, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0106', N'Tuerca unión de cobre 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0116', N'Tuerca unión de cobre 2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0130', N'Tuerca unión de cobre 3/4"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0131', N'Tuerca unión de cobre 1"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0133', N'Tuerca unión de cobre 1" a 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0123', N'Tapón de cobre 1/2"', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 3, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0118', N'Pasta Flux 113 gr', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0136', N'Pasta Flux, marca Laco', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0065', N'Varilla de soldadura de plata al 15%', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 10, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0124', N'Preste de 1/2" de cobre', N'Cobre', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0028', N'Ácido, marca Shiny Side', N'Químicos', N'Litro', N'Almacén A', NULL, N'Disponible', 80, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0070', N'Removedor para linóleo (binolio), marca Fuera', N'Químicos', N'Galón', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0207', N'Pegamento para linóleo (dinolio)', N'Químicos', N'Litro', N'Almacén A', NULL, N'Disponible', 15, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0151', N'Sellador autoadhesivo, marca Circapro', N'Químicos', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0173', N'Sellador para goteras', N'Químicos', N'Galón', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0150', N'Silicón sellador elástico', N'Químicos', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0152', N'Silicón de alta temperatura', N'Químicos', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0096', N'Afloja Todo WD-40', N'Químicos', N'Pieza', N'Almacén A', NULL, N'Disponible', 5, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0153', N'Aceite 3 en 1', N'Químicos', N'Pieza', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0172', N'Aceite de dos tiempos para gasolina, marca Truper', N'Químicos', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0052', N'Kit analizador de cloro y pH, marca Panda', N'Químicos', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00');

INSERT INTO #mtto_carga (codigo, nombre, categoria, unidad, almacen, proveedor, estado, stock_actual, stock_minimo, stock_maximo, costo_unitario, actualizado_en) VALUES
(N'MT-0204', N'Cera líquida para piso', N'Limpieza', N'Litro', N'Almacén A', NULL, N'Disponible', 20, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0082', N'Líquido germicida para cuarto', N'Limpieza', N'Galón', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0163', N'Guante de látex uso industrial (ácidos y químicos)', N'Seguridad', N'Par', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0164', N'Guante 8/2', N'Seguridad', N'Par', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0121', N'Cartucho de filtro para mascarilla FC3, marca Kabel', N'Seguridad', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0155', N'Lija fina para metal N°99', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 100, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0156', N'Lija para madera N°250', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 50, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0157', N'Lija mediana N°80', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 60, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0138', N'Teflón 3/4" industrial', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 18, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0140', N'Cinta multiusos 2", marca 3M', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 4, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0144', N'Cinta Flex Tape tapagoteras', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0174', N'Cinta de aluminio 2"', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 7, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0146', N'Espuma de poliuretano', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0077', N'Catete 5/16" (paquete c/75 pzas)', N'Consumibles', N'Paquete', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0078', N'Catete 1/4" (paquete c/100 pzas)', N'Consumibles', N'Paquete', N'Almacén A', NULL, N'Disponible', 6, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0079', N'Catete 3/8" (paquete c/50 pzas)', N'Consumibles', N'Paquete', N'Almacén A', NULL, N'Disponible', 2, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0048', N'Empaque cónico, marca Helvex, refacción GRF-125', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 100, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0026', N'Frasco para muestra de 1 litro', N'Consumibles', N'Pieza', N'Almacén A', NULL, N'Disponible', 10, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0216', N'Rollo de pasto sintético', N'Otros', N'Rollo', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00'),
(N'MT-0217', N'Rollo de hule negro para concreto', N'Otros', N'Rollo', N'Almacén A', NULL, N'Disponible', 1, 5, NULL, NULL, '2026-07-18T00:00:00');

-- --------------------------------------------------------------------------
-- Validación: toda referencia de catálogo debe existir antes de mezclar.
-- --------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM #mtto_carga c LEFT JOIN dbo.mtto_categoria x ON x.nombre = c.categoria WHERE x.categoria_id IS NULL)
BEGIN
    SELECT DISTINCT N'Categoría faltante' AS problema, c.categoria AS valor
      FROM #mtto_carga c LEFT JOIN dbo.mtto_categoria x ON x.nombre = c.categoria
     WHERE x.categoria_id IS NULL;
    THROW 50030, 'Hay categorías del Excel que no existen en mtto_categoria. Ejecuta primero Mantenimiento_020_CATALOGOS.sql', 1;
END

IF EXISTS (SELECT 1 FROM #mtto_carga c LEFT JOIN dbo.mtto_unidad_medida x ON x.nombre = c.unidad WHERE x.unidad_id IS NULL)
    THROW 50031, 'Hay unidades de medida del Excel que no existen en mtto_unidad_medida.', 1;

IF EXISTS (SELECT 1 FROM #mtto_carga c LEFT JOIN dbo.mtto_almacen x ON x.nombre = c.almacen WHERE x.almacen_id IS NULL)
    THROW 50032, 'Hay almacenes del Excel que no existen en mtto_almacen.', 1;

IF EXISTS (SELECT 1 FROM #mtto_carga c LEFT JOIN dbo.mtto_estado_articulo x ON x.nombre = c.estado WHERE x.estado_id IS NULL)
    THROW 50033, 'Hay estados del Excel que no existen en mtto_estado_articulo.', 1;

-- --------------------------------------------------------------------------
-- MERGE por codigo
-- --------------------------------------------------------------------------
MERGE dbo.mtto_articulo AS t
USING (
    SELECT c.codigo, c.nombre,
           cat.categoria_id, u.unidad_id, al.almacen_id, p.proveedor_id, e.estado_id,
           c.stock_actual, c.stock_minimo, c.stock_maximo, c.costo_unitario, c.actualizado_en
      FROM #mtto_carga c
      JOIN dbo.mtto_categoria       cat ON cat.nombre = c.categoria
      JOIN dbo.mtto_unidad_medida   u   ON u.nombre   = c.unidad
      JOIN dbo.mtto_almacen         al  ON al.nombre  = c.almacen
      JOIN dbo.mtto_estado_articulo e   ON e.nombre   = c.estado
      LEFT JOIN dbo.mtto_proveedor  p   ON p.nombre   = c.proveedor
) AS s
   ON t.codigo = s.codigo
WHEN MATCHED THEN UPDATE SET
    nombre = s.nombre, categoria_id = s.categoria_id, unidad_id = s.unidad_id,
    almacen_id = s.almacen_id, proveedor_id = s.proveedor_id, estado_id = s.estado_id,
    stock_actual = s.stock_actual, stock_minimo = s.stock_minimo,
    stock_maximo = s.stock_maximo, costo_unitario = s.costo_unitario,
    actualizado_en = s.actualizado_en
WHEN NOT MATCHED THEN INSERT
    (codigo, nombre, categoria_id, unidad_id, almacen_id, proveedor_id, estado_id,
     stock_actual, stock_minimo, stock_maximo, costo_unitario, actualizado_en)
    VALUES
    (s.codigo, s.nombre, s.categoria_id, s.unidad_id, s.almacen_id, s.proveedor_id, s.estado_id,
     s.stock_actual, s.stock_minimo, s.stock_maximo, s.costo_unitario, s.actualizado_en);

DROP TABLE #mtto_carga;

PRINT '=== Inventario MTTO cargado ===';
SELECT COUNT(*) AS articulos FROM dbo.mtto_articulo;
GO

GO


-- ##########################################################################
-- ## VISTAS Y PROCEDIMIENTOS
-- ## (origen: Mantenimiento_040_VISTAS_SP.sql)
-- ##########################################################################

-- ============================================================================
-- MANTENIMIENTO (MTTO): VISTAS Y PROCEDIMIENTOS
-- ============================================================================
-- Reproducen las hojas "Inventario", "Entradas y Salidas" y "Dashboard".
-- ============================================================================

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

GO


-- ============================================================================
-- VERIFICACION FINAL
-- ============================================================================
PRINT '--- Conteos esperados: 220 articulos, 15 categorias, 138 bajo minimo ---';
SELECT
    (SELECT COUNT(*) FROM dbo.mtto_articulo)  AS articulos,
    (SELECT COUNT(*) FROM dbo.mtto_categoria) AS categorias,
    (SELECT COUNT(*) FROM dbo.mtto_articulo WHERE stock_actual < stock_minimo) AS bajo_minimo,
    (SELECT COUNT(*) FROM sys.views  WHERE name LIKE 'vw_mtto%')  AS vistas,
    (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'usp_mtto%') AS procedimientos;
GO
