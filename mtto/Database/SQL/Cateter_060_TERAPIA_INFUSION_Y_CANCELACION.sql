-- ============================================================================
-- Cateter_060_TERAPIA_INFUSION_Y_CANCELACION.sql
--
-- SCRIPT 100% ADITIVO. No contiene DROP, DELETE ni TRUNCATE. No modifica ni
-- borra ninguna fila existente. Es seguro correrlo contra la base en
-- operación: cada cambio está protegido con una verificación de existencia
-- (si ya se aplicó antes, no hace nada y lo reporta con PRINT).
--
-- Ejecutar con:
--   sqlcmd -S <servidor> -d InventarioMantenimiento -E -I -b -f 65001 -i Cateter_060_TERAPIA_INFUSION_Y_CANCELACION.sql
--
-- Qué hace:
--   1) Agrega columnas de cancelación a la tabla base de movimientos de
--      catéter (estado, razón, fecha/usuario de cancelación, y el enlace a
--      la reversión). NO se toca la vista vw_cateter_movimientos: el join se
--      hace desde C# para no arriesgar romper la vista en producción.
--   2) Crea 3 tablas nuevas e independientes para Terapia de Infusión
--      Intravascular (no colisionan con nada existente).
-- ============================================================================

SET NOCOUNT ON;

-- ----------------------------------------------------------------------------
-- 1) Cancelación de movimientos — columnas nuevas en dbo.cateter_movimiento
-- ----------------------------------------------------------------------------
IF OBJECT_ID('dbo.cateter_movimiento', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.cateter_movimiento', 'estado') IS NULL
        ALTER TABLE dbo.cateter_movimiento
            ADD estado VARCHAR(20) NOT NULL CONSTRAINT DF_cateter_movimiento_estado DEFAULT ('Activo');

    IF COL_LENGTH('dbo.cateter_movimiento', 'razon_cancelacion') IS NULL
        ALTER TABLE dbo.cateter_movimiento ADD razon_cancelacion NVARCHAR(500) NULL;

    IF COL_LENGTH('dbo.cateter_movimiento', 'cancelado_en') IS NULL
        ALTER TABLE dbo.cateter_movimiento ADD cancelado_en DATETIME2 NULL;

    IF COL_LENGTH('dbo.cateter_movimiento', 'cancelado_por') IS NULL
        ALTER TABLE dbo.cateter_movimiento ADD cancelado_por NVARCHAR(120) NULL;

    -- es_reversion / reversion_de_id: cancelar un movimiento no revierte el
    -- stock "a mano" (no tocamos existencias directamente); en su lugar el
    -- backend registra un movimiento de reversión con el mismo procedimiento
    -- probado (usp_cateter_registrar_movimiento) y lo enlaza aquí.
    IF COL_LENGTH('dbo.cateter_movimiento', 'es_reversion') IS NULL
        ALTER TABLE dbo.cateter_movimiento
            ADD es_reversion BIT NOT NULL CONSTRAINT DF_cateter_movimiento_es_reversion DEFAULT (0);

    IF COL_LENGTH('dbo.cateter_movimiento', 'reversion_de_id') IS NULL
        ALTER TABLE dbo.cateter_movimiento ADD reversion_de_id INT NULL;

    PRINT 'OK: columnas de cancelación agregadas/verificadas en dbo.cateter_movimiento.';
END
ELSE
BEGIN
    PRINT 'AVISO: no existe dbo.cateter_movimiento con ese nombre exacto. No se hizo NINGÚN cambio.';
    PRINT '       Verifica el nombre real de la tabla base con: SELECT name FROM sys.tables WHERE name LIKE ''%movimiento%'';';
END
GO

-- ----------------------------------------------------------------------------
-- 2) Terapia de Infusión Intravascular — tablas nuevas
-- ----------------------------------------------------------------------------
IF OBJECT_ID('dbo.cateter_terapia_registro', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.cateter_terapia_registro (
        registro_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_cateter_terapia_registro PRIMARY KEY,
        fecha       DATETIME2 NOT NULL CONSTRAINT DF_ctreg_fecha DEFAULT (SYSDATETIME()),
        msd         BIT NOT NULL CONSTRAINT DF_ctreg_msd DEFAULT (0),
        msi         BIT NOT NULL CONSTRAINT DF_ctreg_msi DEFAULT (0),
        mii         BIT NOT NULL CONSTRAINT DF_ctreg_mii DEFAULT (0),
        calibre_14  BIT NOT NULL CONSTRAINT DF_ctreg_c14 DEFAULT (0),
        calibre_16  BIT NOT NULL CONSTRAINT DF_ctreg_c16 DEFAULT (0),
        calibre_17  BIT NOT NULL CONSTRAINT DF_ctreg_c17 DEFAULT (0),
        calibre_18  BIT NOT NULL CONSTRAINT DF_ctreg_c18 DEFAULT (0),
        calibre_19  BIT NOT NULL CONSTRAINT DF_ctreg_c19 DEFAULT (0),
        calibre_20  BIT NOT NULL CONSTRAINT DF_ctreg_c20 DEFAULT (0),
        calibre_22  BIT NOT NULL CONSTRAINT DF_ctreg_c22 DEFAULT (0),
        calibre_24  BIT NOT NULL CONSTRAINT DF_ctreg_c24 DEFAULT (0),
        creado_por  NVARCHAR(120) NULL,
        creado_en   DATETIME2 NOT NULL CONSTRAINT DF_ctreg_creado_en DEFAULT (SYSDATETIME())
    );
    PRINT 'OK: tabla dbo.cateter_terapia_registro creada.';
END
ELSE
    PRINT 'OK: dbo.cateter_terapia_registro ya existía, sin cambios.';
GO

IF OBJECT_ID('dbo.cateter_terapia_evento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.cateter_terapia_evento (
        evento_id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_cateter_terapia_evento PRIMARY KEY,
        fecha                     DATE NOT NULL,
        total_eventos             INT NOT NULL CONSTRAINT DF_cte_total_eventos DEFAULT (0),
        total_cateteres_colocados INT NOT NULL CONSTRAINT DF_cte_total_cat DEFAULT (0),
        total_intervenciones      INT NOT NULL CONSTRAINT DF_cte_total_interv DEFAULT (0),
        creado_por                NVARCHAR(120) NULL,
        creado_en                 DATETIME2 NOT NULL CONSTRAINT DF_cte_creado_en DEFAULT (SYSDATETIME()),
        actualizado_en            DATETIME2 NULL
    );
    PRINT 'OK: tabla dbo.cateter_terapia_evento creada.';
END
ELSE
    PRINT 'OK: dbo.cateter_terapia_evento ya existía, sin cambios.';
GO

IF OBJECT_ID('dbo.cateter_terapia_evento_instalador', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.cateter_terapia_evento_instalador (
        instalador_id   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_cateter_terapia_evento_instalador PRIMARY KEY,
        evento_id       INT NOT NULL CONSTRAINT FK_ctei_evento REFERENCES dbo.cateter_terapia_evento(evento_id),
        nombre_personal NVARCHAR(120) NOT NULL,
        cantidad        INT NOT NULL
    );
    PRINT 'OK: tabla dbo.cateter_terapia_evento_instalador creada.';
END
ELSE
    PRINT 'OK: dbo.cateter_terapia_evento_instalador ya existía, sin cambios.';
GO

PRINT '============================================================';
PRINT 'Script terminado. Ningún dato existente fue modificado ni borrado.';
PRINT '============================================================';
GO
