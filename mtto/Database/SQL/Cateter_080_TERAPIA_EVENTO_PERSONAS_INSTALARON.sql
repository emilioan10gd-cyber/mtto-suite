-- ============================================================================
-- Cateter_080_TERAPIA_EVENTO_PERSONAS_INSTALARON.sql
--
-- La pestaña "Eventos Diarios" pasa de capturar "qué persona instaló cuántos
-- catéteres" (nombre + cantidad, una tabla aparte) a solo "cuántas personas
-- instalaron ese día" (un número, columna en la misma fila del evento).
--
-- Seguro: dbo.cateter_terapia_evento y dbo.cateter_terapia_evento_instalador
-- están vacías en producción (0 filas, verificado antes de este script — la
-- función se lanzó hace unas horas y nadie ha guardado un evento todavía).
-- Si en el futuro ya hay filas, el script avisa y no hace nada.
--
-- Ejecutar con:
--   sqlcmd -S <servidor> -d InventarioMantenimiento -E -I -b -f 65001 -i Cateter_080_TERAPIA_EVENTO_PERSONAS_INSTALARON.sql
-- ============================================================================

SET NOCOUNT ON;

IF OBJECT_ID('dbo.cateter_terapia_evento', 'U') IS NULL
BEGIN
    PRINT 'AVISO: no existe dbo.cateter_terapia_evento. No se hizo ningún cambio.';
    RETURN;
END

IF COL_LENGTH('dbo.cateter_terapia_evento', 'personas_instalaron') IS NOT NULL
BEGIN
    PRINT 'OK: dbo.cateter_terapia_evento ya tiene personas_instalaron, sin cambios.';
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.cateter_terapia_evento)
   OR (OBJECT_ID('dbo.cateter_terapia_evento_instalador', 'U') IS NOT NULL
       AND EXISTS (SELECT 1 FROM dbo.cateter_terapia_evento_instalador))
BEGIN
    PRINT 'AVISO: ya hay eventos o instaladores capturados. No se hizo ningún cambio automático;';
    PRINT '       revisa a mano cómo migrar el detalle por nombre a un solo total antes de correr esto.';
    RETURN;
END

BEGIN TRANSACTION;

-- La tabla de detalle por nombre ya no se usa (Eventos Diarios ahora sólo
-- captura el número de personas, no quiénes). Está vacía, así que se puede
-- quitar sin perder nada.
IF OBJECT_ID('dbo.cateter_terapia_evento_instalador', 'U') IS NOT NULL
    DROP TABLE dbo.cateter_terapia_evento_instalador;

ALTER TABLE dbo.cateter_terapia_evento
    ADD personas_instalaron INT NOT NULL CONSTRAINT DF_cte_personas_instalaron DEFAULT (0);

COMMIT TRANSACTION;

PRINT 'OK: dbo.cateter_terapia_evento_instalador eliminada y personas_instalaron agregada a cateter_terapia_evento.';
GO
