-- ============================================================================
-- Cateter_070_TERAPIA_REGISTRO_CANTIDADES.sql
--
-- Convierte las 11 columnas BIT (sí/no) de dbo.cateter_terapia_registro a
-- INT (cantidad): la captura pasa de "¿se usó este sitio/calibre?" a
-- "¿cuántos?", como una fila de Excel con una celda numérica por columna.
--
-- Seguro: dbo.cateter_terapia_registro está vacía en producción (0 filas,
-- verificado antes de este script — la función se lanzó hace unas horas y
-- nadie ha guardado un registro todavía), así que no hay datos que convertir
-- ni perder. Si en el futuro ya hay filas, el script avisa y no hace nada.
--
-- Ejecutar con:
--   sqlcmd -S <servidor> -d InventarioMantenimiento -E -I -b -f 65001 -i Cateter_070_TERAPIA_REGISTRO_CANTIDADES.sql
-- ============================================================================

SET NOCOUNT ON;

IF OBJECT_ID('dbo.cateter_terapia_registro', 'U') IS NULL
BEGIN
    PRINT 'AVISO: no existe dbo.cateter_terapia_registro. No se hizo ningún cambio.';
    RETURN;
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.cateter_terapia_registro') AND name = 'msd'
      AND system_type_id = TYPE_ID('bit')
)
BEGIN
    PRINT 'OK: cateter_terapia_registro ya tiene columnas de cantidad (INT), sin cambios.';
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.cateter_terapia_registro)
BEGIN
    PRINT 'AVISO: cateter_terapia_registro ya tiene filas capturadas. No se hizo ningún cambio automático;';
    PRINT '       revisa a mano cómo migrar sí/no (1) a una cantidad antes de correr esto.';
    RETURN;
END

BEGIN TRANSACTION;

ALTER TABLE dbo.cateter_terapia_registro DROP CONSTRAINT
    DF_ctreg_msd, DF_ctreg_msi, DF_ctreg_mii,
    DF_ctreg_c14, DF_ctreg_c16, DF_ctreg_c17, DF_ctreg_c18,
    DF_ctreg_c19, DF_ctreg_c20, DF_ctreg_c22, DF_ctreg_c24;

ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN msd INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN msi INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN mii INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN calibre_14 INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN calibre_16 INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN calibre_17 INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN calibre_18 INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN calibre_19 INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN calibre_20 INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN calibre_22 INT NOT NULL;
ALTER TABLE dbo.cateter_terapia_registro ALTER COLUMN calibre_24 INT NOT NULL;

ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_msd DEFAULT (0) FOR msd;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_msi DEFAULT (0) FOR msi;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_mii DEFAULT (0) FOR mii;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_c14 DEFAULT (0) FOR calibre_14;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_c16 DEFAULT (0) FOR calibre_16;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_c17 DEFAULT (0) FOR calibre_17;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_c18 DEFAULT (0) FOR calibre_18;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_c19 DEFAULT (0) FOR calibre_19;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_c20 DEFAULT (0) FOR calibre_20;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_c22 DEFAULT (0) FOR calibre_22;
ALTER TABLE dbo.cateter_terapia_registro ADD CONSTRAINT DF_ctreg_c24 DEFAULT (0) FOR calibre_24;

COMMIT TRANSACTION;

PRINT 'OK: columnas convertidas de BIT a INT (cantidad) en dbo.cateter_terapia_registro.';
GO
