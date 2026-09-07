-- ============================================================================
-- BIOMÉDICO: BASE DE DATOS + ESQUEMA
-- ============================================================================
-- Creado: 2026-08-19
-- Origen: ACTIVOS BIOMEDICO AREA.xlsx (11 hojas por servicio/área)
-- Área nueva e independiente (igual que Catéter/Farmacia): usuario propio con
-- area = 'biomedico' en app_usuario, separada por RequiereAreaAttribute.
-- ============================================================================

USE [InventarioMantenimiento];
GO

-- app_usuario.area tiene un CHECK que solo permitía farmacia/cateter/mtto;
-- hay que abrirlo para poder crear cuentas del área biomedico.
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_app_usuario_area')
BEGIN
    ALTER TABLE dbo.app_usuario DROP CONSTRAINT CK_app_usuario_area;
    ALTER TABLE dbo.app_usuario ADD CONSTRAINT CK_app_usuario_area
        CHECK (area IN (N'farmacia', N'cateter', N'mtto', N'biomedico'));
    PRINT 'CK_app_usuario_area actualizado para incluir biomedico';
END
GO

-- ============================================================================
-- 1. CATÁLOGOS
-- ============================================================================

-- Área/servicio del hospital (RX, Urgencias, Hemodiálisis, UCIA, Hospital,
-- Toco y Gine, UCIN y Pedia, QX, CEYE, Monitores, Ventiladores, ...)
IF OBJECT_ID('dbo.bio_area', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_area (
        area_id    INT IDENTITY(1,1) PRIMARY KEY,
        nombre     NVARCHAR(80) NOT NULL,
        activo     BIT NOT NULL CONSTRAINT DF_bio_area_act DEFAULT (1),
        creado_en  DATETIME2(0) NOT NULL CONSTRAINT DF_bio_area_cre DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_bio_area_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla bio_area creada';
END
GO

-- Tipo de equipo (Monitor, Ventilador, Desfibrilador, Bomba de Infusión, ...)
IF OBJECT_ID('dbo.bio_categoria_equipo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_categoria_equipo (
        categoria_id INT IDENTITY(1,1) PRIMARY KEY,
        nombre       NVARCHAR(120) NOT NULL,
        activo       BIT NOT NULL CONSTRAINT DF_bio_catequipo_act DEFAULT (1),
        creado_en    DATETIME2(0) NOT NULL CONSTRAINT DF_bio_catequipo_cre DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_bio_catequipo_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla bio_categoria_equipo creada';
END
GO

-- Estado operativo del equipo. "De baja" es terminal, no se borra el equipo:
-- se marca así (mismo criterio que activo/CambiarEstado en Catéter).
IF OBJECT_ID('dbo.bio_estado_equipo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_estado_equipo (
        estado_id  INT IDENTITY(1,1) PRIMARY KEY,
        nombre     NVARCHAR(40) NOT NULL,
        es_baja    BIT NOT NULL CONSTRAINT DF_bio_estequipo_baja DEFAULT (0),
        CONSTRAINT UQ_bio_estequipo_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla bio_estado_equipo creada';

    INSERT INTO dbo.bio_estado_equipo (nombre, es_baja) VALUES
        (N'Operativo', 0),
        (N'En mantenimiento', 0),
        (N'Fuera de servicio', 0),
        (N'De baja', 1);
    PRINT 'Catálogo bio_estado_equipo sembrado';
END
GO

-- Catálogo de insumos (electrodos, papel ECG, baterías, geles, ...)
IF OBJECT_ID('dbo.bio_categoria_insumo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_categoria_insumo (
        categoria_id INT IDENTITY(1,1) PRIMARY KEY,
        nombre       NVARCHAR(120) NOT NULL,
        activo       BIT NOT NULL CONSTRAINT DF_bio_catinsumo_act DEFAULT (1),
        creado_en    DATETIME2(0) NOT NULL CONSTRAINT DF_bio_catinsumo_cre DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_bio_catinsumo_nombre UNIQUE (nombre)
    );
    PRINT 'Tabla bio_categoria_insumo creada';
END
GO

-- ============================================================================
-- 2. EQUIPO (hoja por área del Excel)
-- ============================================================================
IF OBJECT_ID('dbo.bio_equipo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_equipo (
        equipo_id        INT IDENTITY(1,1) PRIMARY KEY,
        nombre            NVARCHAR(250) NOT NULL,
        descripcion       NVARCHAR(500) NULL,
        marca             NVARCHAR(120) NULL,
        modelo            NVARCHAR(120) NULL,
        numero_serie      NVARCHAR(120) NULL,
        area_id           INT NOT NULL,
        categoria_id      INT NULL,
        estado_id         INT NOT NULL,
        ubicacion         NVARCHAR(150) NULL,   -- cama/sala, ej. "Cama 01"
        comentarios       NVARCHAR(500) NULL,
        fecha_adquisicion DATE NULL,
        motivo_baja       NVARCHAR(500) NULL,
        activo            BIT NOT NULL CONSTRAINT DF_bio_equipo_act DEFAULT (1),
        actualizado_en    DATETIME2(0) NOT NULL CONSTRAINT DF_bio_equipo_upd DEFAULT (SYSDATETIME()),
        creado_en         DATETIME2(0) NOT NULL CONSTRAINT DF_bio_equipo_cre DEFAULT (SYSDATETIME()),

        CONSTRAINT FK_bio_equipo_area      FOREIGN KEY (area_id)      REFERENCES dbo.bio_area(area_id),
        CONSTRAINT FK_bio_equipo_categoria FOREIGN KEY (categoria_id) REFERENCES dbo.bio_categoria_equipo(categoria_id),
        CONSTRAINT FK_bio_equipo_estado    FOREIGN KEY (estado_id)    REFERENCES dbo.bio_estado_equipo(estado_id)
    );

    CREATE INDEX IX_bio_equipo_area     ON dbo.bio_equipo(area_id);
    CREATE INDEX IX_bio_equipo_estado   ON dbo.bio_equipo(estado_id);
    CREATE INDEX IX_bio_equipo_nombre   ON dbo.bio_equipo(nombre);
    CREATE INDEX IX_bio_equipo_serie    ON dbo.bio_equipo(numero_serie);
    CREATE INDEX IX_bio_equipo_modelo   ON dbo.bio_equipo(marca, modelo);

    PRINT 'Tabla bio_equipo creada';
END
GO

-- ============================================================================
-- 3. INSUMOS + LOTES (FEFO) + MOVIMIENTOS
-- ============================================================================
IF OBJECT_ID('dbo.bio_insumo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_insumo (
        insumo_id     INT IDENTITY(1,1) PRIMARY KEY,
        clave         NVARCHAR(20)  NOT NULL,
        nombre        NVARCHAR(250) NOT NULL,
        descripcion   NVARCHAR(500) NULL,
        categoria_id  INT NULL,
        unidad        NVARCHAR(40)  NOT NULL CONSTRAINT DF_bio_insumo_unidad DEFAULT (N'Pieza'),
        stock_minimo  DECIMAL(18,3) NOT NULL CONSTRAINT DF_bio_insumo_min DEFAULT (0),
        stock_maximo  DECIMAL(18,3) NULL,
        activo        BIT NOT NULL CONSTRAINT DF_bio_insumo_act DEFAULT (1),
        actualizado_en DATETIME2(0) NOT NULL CONSTRAINT DF_bio_insumo_upd DEFAULT (SYSDATETIME()),
        creado_en     DATETIME2(0) NOT NULL CONSTRAINT DF_bio_insumo_cre DEFAULT (SYSDATETIME()),

        CONSTRAINT UQ_bio_insumo_clave UNIQUE (clave),
        CONSTRAINT FK_bio_insumo_categoria FOREIGN KEY (categoria_id) REFERENCES dbo.bio_categoria_insumo(categoria_id),
        CONSTRAINT CK_bio_insumo_min_max CHECK (stock_maximo IS NULL OR stock_maximo >= stock_minimo)
    );
    PRINT 'Tabla bio_insumo creada';
END
GO

-- Lote con caducidad (FEFO). La existencia NO se guarda aquí: se deriva de
-- bio_movimiento (mismo criterio que cateter_lote/vw_cateter_lotes).
IF OBJECT_ID('dbo.bio_lote', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_lote (
        lote_id    INT IDENTITY(1,1) PRIMARY KEY,
        insumo_id  INT NOT NULL,
        lote       NVARCHAR(60) NOT NULL,
        caducidad  DATE NULL,
        notas      NVARCHAR(255) NULL,
        creado_en  DATETIME2(0) NOT NULL CONSTRAINT DF_bio_lote_cre DEFAULT (SYSDATETIME()),

        CONSTRAINT FK_bio_lote_insumo FOREIGN KEY (insumo_id) REFERENCES dbo.bio_insumo(insumo_id),
        CONSTRAINT UQ_bio_lote_insumo_lote UNIQUE (insumo_id, lote)
    );
    CREATE INDEX IX_bio_lote_caducidad ON dbo.bio_lote(insumo_id, caducidad);
    PRINT 'Tabla bio_lote creada';
END
GO

IF OBJECT_ID('dbo.bio_tipo_movimiento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_tipo_movimiento (
        tipo_id  INT IDENTITY(1,1) PRIMARY KEY,
        nombre   NVARCHAR(40) NOT NULL,
        signo    SMALLINT NOT NULL CONSTRAINT DF_bio_tipomov_signo DEFAULT (0),
        CONSTRAINT UQ_bio_tipomov_nombre UNIQUE (nombre),
        CONSTRAINT CK_bio_tipomov_signo CHECK (signo IN (-1, 0, 1))
    );
    INSERT INTO dbo.bio_tipo_movimiento (nombre, signo) VALUES
        (N'Entrada', 1),
        (N'Salida', -1),
        (N'Ajuste (+)', 1),
        (N'Ajuste (-)', -1);
    PRINT 'Tabla bio_tipo_movimiento creada y sembrada';
END
GO

IF OBJECT_ID('dbo.bio_movimiento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_movimiento (
        movimiento_id INT IDENTITY(1,1) PRIMARY KEY,
        fecha         DATETIME2(0) NOT NULL CONSTRAINT DF_bio_mov_fecha DEFAULT (SYSDATETIME()),
        tipo_id       INT NOT NULL,
        insumo_id     INT NOT NULL,
        lote_id       INT NOT NULL,
        cantidad      DECIMAL(18,3) NOT NULL,
        responsable   NVARCHAR(120) NULL,
        observaciones NVARCHAR(500) NULL,
        creado_en     DATETIME2(0) NOT NULL CONSTRAINT DF_bio_mov_cre DEFAULT (SYSDATETIME()),

        CONSTRAINT FK_bio_mov_tipo   FOREIGN KEY (tipo_id)   REFERENCES dbo.bio_tipo_movimiento(tipo_id),
        CONSTRAINT FK_bio_mov_insumo FOREIGN KEY (insumo_id) REFERENCES dbo.bio_insumo(insumo_id),
        CONSTRAINT FK_bio_mov_lote   FOREIGN KEY (lote_id)   REFERENCES dbo.bio_lote(lote_id),
        CONSTRAINT CK_bio_mov_cantidad CHECK (cantidad > 0)
    );
    CREATE INDEX IX_bio_mov_insumo ON dbo.bio_movimiento(insumo_id, fecha DESC);
    CREATE INDEX IX_bio_mov_lote   ON dbo.bio_movimiento(lote_id);
    PRINT 'Tabla bio_movimiento creada';
END
GO

-- ============================================================================
-- 4. MANTENIMIENTO (calendario preventivo/correctivo)
-- ============================================================================
IF OBJECT_ID('dbo.bio_mantenimiento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_mantenimiento (
        mantenimiento_id  INT IDENTITY(1,1) PRIMARY KEY,
        equipo_id         INT NOT NULL,
        tipo              NVARCHAR(20) NOT NULL,   -- Preventivo / Correctivo
        fecha_programada  DATE NOT NULL,
        fecha_realizada   DATE NULL,
        estado            NVARCHAR(20) NOT NULL CONSTRAINT DF_bio_mtto_estado DEFAULT (N'Programado'), -- Programado/Realizado/Vencido/Cancelado
        tecnico_responsable NVARCHAR(120) NULL,
        proveedor         NVARCHAR(150) NULL,
        descripcion       NVARCHAR(1000) NULL,
        frecuencia_meses  INT NULL,               -- si aplica, regenera el siguiente preventivo
        actualizado_en    DATETIME2(0) NOT NULL CONSTRAINT DF_bio_mtto_upd DEFAULT (SYSDATETIME()),
        creado_en         DATETIME2(0) NOT NULL CONSTRAINT DF_bio_mtto_cre DEFAULT (SYSDATETIME()),

        CONSTRAINT FK_bio_mtto_equipo FOREIGN KEY (equipo_id) REFERENCES dbo.bio_equipo(equipo_id),
        CONSTRAINT CK_bio_mtto_tipo   CHECK (tipo IN (N'Preventivo', N'Correctivo')),
        CONSTRAINT CK_bio_mtto_estado CHECK (estado IN (N'Programado', N'Realizado', N'Vencido', N'Cancelado'))
    );
    CREATE INDEX IX_bio_mtto_equipo ON dbo.bio_mantenimiento(equipo_id, fecha_programada DESC);
    CREATE INDEX IX_bio_mtto_fecha  ON dbo.bio_mantenimiento(fecha_programada);
    CREATE INDEX IX_bio_mtto_estado ON dbo.bio_mantenimiento(estado);
    PRINT 'Tabla bio_mantenimiento creada';
END
GO

-- ============================================================================
-- 5. MANUALES (documentos adjuntos por equipo)
-- ============================================================================
IF OBJECT_ID('dbo.bio_manual', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_manual (
        manual_id      INT IDENTITY(1,1) PRIMARY KEY,
        equipo_id      INT NOT NULL,
        nombre_archivo NVARCHAR(255) NOT NULL,
        ruta_archivo   NVARCHAR(500) NOT NULL,
        tipo_archivo   NVARCHAR(20) NULL,
        tamano_bytes   BIGINT NULL,
        texto_extraido NVARCHAR(MAX) NULL,   -- texto plano del PDF, para buscar por palabras clave
        subido_en      DATETIME2(0) NOT NULL CONSTRAINT DF_bio_manual_sub DEFAULT (SYSDATETIME()),

        CONSTRAINT FK_bio_manual_equipo FOREIGN KEY (equipo_id) REFERENCES dbo.bio_equipo(equipo_id)
    );
    CREATE INDEX IX_bio_manual_equipo ON dbo.bio_manual(equipo_id);
    PRINT 'Tabla bio_manual creada';
END
GO

-- ============================================================================
-- 6. FALLAS (historial de errores/incidencias -> "expediente" del equipo)
-- ============================================================================
IF OBJECT_ID('dbo.bio_falla', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bio_falla (
        falla_id       INT IDENTITY(1,1) PRIMARY KEY,
        equipo_id      INT NOT NULL,
        fecha          DATETIME2(0) NOT NULL CONSTRAINT DF_bio_falla_fecha DEFAULT (SYSDATETIME()),
        sintoma        NVARCHAR(1000) NOT NULL,
        causa          NVARCHAR(1000) NULL,
        solucion       NVARCHAR(1000) NULL,
        tecnico        NVARCHAR(120) NULL,
        estado         NVARCHAR(20) NOT NULL CONSTRAINT DF_bio_falla_estado DEFAULT (N'Abierta'), -- Abierta/Resuelta
        actualizado_en DATETIME2(0) NOT NULL CONSTRAINT DF_bio_falla_upd DEFAULT (SYSDATETIME()),
        creado_en      DATETIME2(0) NOT NULL CONSTRAINT DF_bio_falla_cre DEFAULT (SYSDATETIME()),

        CONSTRAINT FK_bio_falla_equipo FOREIGN KEY (equipo_id) REFERENCES dbo.bio_equipo(equipo_id),
        CONSTRAINT CK_bio_falla_estado CHECK (estado IN (N'Abierta', N'Resuelta'))
    );
    CREATE INDEX IX_bio_falla_equipo ON dbo.bio_falla(equipo_id, fecha DESC);
    PRINT 'Tabla bio_falla creada';
END
GO

PRINT '=== Esquema Biomédico listo ===';
GO
