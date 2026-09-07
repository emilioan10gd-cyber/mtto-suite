-- ============================================================================
-- BIOMÉDICO: VISTAS Y PROCEDIMIENTOS
-- ============================================================================

USE [InventarioMantenimiento];
GO

-- ============================================================================
-- VISTA: vw_bio_inventario  (listado de equipos)
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_bio_inventario
AS
SELECT
    q.equipo_id,
    q.nombre,
    q.descripcion,
    q.marca,
    q.modelo,
    q.numero_serie,
    ar.nombre                       AS area,
    ar.area_id,
    c.nombre                        AS categoria,
    q.categoria_id,
    e.nombre                        AS estado,
    e.es_baja,
    q.estado_id,
    q.ubicacion,
    q.comentarios,
    q.fecha_adquisicion,
    q.motivo_baja,
    q.activo,
    q.actualizado_en,
    q.creado_en,
    (SELECT COUNT(*) FROM dbo.bio_mantenimiento m WHERE m.equipo_id = q.equipo_id) AS total_mantenimientos,
    (SELECT COUNT(*) FROM dbo.bio_mantenimiento m WHERE m.equipo_id = q.equipo_id
       AND m.estado = N'Programado' AND m.fecha_programada < CAST(SYSDATETIME() AS DATE)) AS mantenimientos_vencidos,
    (SELECT COUNT(*) FROM dbo.bio_falla f WHERE f.equipo_id = q.equipo_id) AS total_fallas,
    (SELECT COUNT(*) FROM dbo.bio_manual mn WHERE mn.equipo_id = q.equipo_id) AS total_manuales
FROM dbo.bio_equipo q
JOIN dbo.bio_area ar          ON ar.area_id     = q.area_id
JOIN dbo.bio_estado_equipo e  ON e.estado_id    = q.estado_id
LEFT JOIN dbo.bio_categoria_equipo c ON c.categoria_id = q.categoria_id;
GO

-- ============================================================================
-- VISTA: vw_bio_lotes  (existencia por lote, FEFO)
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_bio_lotes
AS
SELECT
    l.lote_id,
    l.insumo_id,
    i.clave,
    i.nombre                       AS insumo,
    ic.nombre                      AS categoria,
    l.lote,
    l.caducidad,
    DATEDIFF(DAY, CAST(SYSDATETIME() AS DATE), l.caducidad) AS dias_para_vencer,
    CASE
        WHEN l.caducidad IS NULL THEN N'Sin caducidad'
        WHEN l.caducidad < CAST(SYSDATETIME() AS DATE) THEN N'Vencido'
        WHEN l.caducidad < DATEADD(DAY, 90, CAST(SYSDATETIME() AS DATE)) THEN N'Por vencer'
        ELSE N'Vigente'
    END                             AS estado_caducidad,
    ISNULL((SELECT SUM(mv.cantidad * t.signo) FROM dbo.bio_movimiento mv
              JOIN dbo.bio_tipo_movimiento t ON t.tipo_id = mv.tipo_id
             WHERE mv.lote_id = l.lote_id), 0) AS existencia,
    l.notas,
    l.creado_en
FROM dbo.bio_lote l
JOIN dbo.bio_insumo i ON i.insumo_id = l.insumo_id
LEFT JOIN dbo.bio_categoria_insumo ic ON ic.categoria_id = i.categoria_id;
GO

-- ============================================================================
-- VISTA: vw_bio_insumos  (catálogo de insumos + existencia total)
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_bio_insumos
AS
SELECT
    i.insumo_id,
    i.clave,
    i.nombre,
    i.descripcion,
    ic.nombre                       AS categoria,
    i.categoria_id,
    i.unidad,
    i.stock_minimo,
    i.stock_maximo,
    ISNULL((SELECT SUM(v.existencia) FROM dbo.vw_bio_lotes v WHERE v.insumo_id = i.insumo_id), 0) AS existencia_total,
    (SELECT MIN(v.caducidad) FROM dbo.vw_bio_lotes v WHERE v.insumo_id = i.insumo_id AND v.existencia > 0) AS caducidad_proxima,
    i.activo,
    i.actualizado_en,
    i.creado_en
FROM dbo.bio_insumo i
LEFT JOIN dbo.bio_categoria_insumo ic ON ic.categoria_id = i.categoria_id;
GO

-- ============================================================================
-- VISTA: vw_bio_mantenimiento  (calendario)
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_bio_mantenimiento
AS
SELECT
    m.mantenimiento_id,
    m.equipo_id,
    q.nombre                        AS equipo,
    ar.nombre                       AS area,
    m.tipo,
    m.fecha_programada,
    m.fecha_realizada,
    CASE
        WHEN m.estado = N'Programado' AND m.fecha_programada < CAST(SYSDATETIME() AS DATE)
            THEN N'Vencido'
        ELSE m.estado
    END                              AS estado,
    m.tecnico_responsable,
    m.proveedor,
    m.descripcion,
    m.frecuencia_meses,
    m.actualizado_en,
    m.creado_en
FROM dbo.bio_mantenimiento m
JOIN dbo.bio_equipo q ON q.equipo_id = m.equipo_id
JOIN dbo.bio_area ar  ON ar.area_id  = q.area_id;
GO

-- ============================================================================
-- VISTA: vw_bio_dashboard_kpi
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_bio_dashboard_kpi
AS
SELECT
    (SELECT COUNT(*) FROM dbo.bio_equipo)                                       AS total_equipos,
    (SELECT COUNT(*) FROM dbo.bio_equipo q JOIN dbo.bio_estado_equipo e ON e.estado_id = q.estado_id WHERE e.es_baja = 0) AS equipos_activos,
    (SELECT COUNT(*) FROM dbo.bio_equipo q JOIN dbo.bio_estado_equipo e ON e.estado_id = q.estado_id WHERE e.es_baja = 1) AS equipos_de_baja,
    (SELECT COUNT(*) FROM dbo.bio_mantenimiento
      WHERE estado = N'Programado' AND fecha_programada < CAST(SYSDATETIME() AS DATE))            AS mantenimientos_vencidos,
    (SELECT COUNT(*) FROM dbo.bio_mantenimiento
      WHERE estado = N'Programado'
        AND fecha_programada BETWEEN CAST(SYSDATETIME() AS DATE) AND DATEADD(DAY, 30, CAST(SYSDATETIME() AS DATE))) AS mantenimientos_proximos_30d,
    (SELECT COUNT(*) FROM dbo.vw_bio_insumos WHERE existencia_total < stock_minimo)                AS insumos_bajo_minimo,
    (SELECT COUNT(*) FROM dbo.vw_bio_lotes WHERE estado_caducidad = N'Por vencer' AND existencia > 0) AS lotes_por_vencer,
    (SELECT COUNT(*) FROM dbo.bio_falla WHERE estado = N'Abierta')                                 AS fallas_abiertas;
GO

-- ============================================================================
-- SP: usp_bio_alta_equipo  (upsert)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_bio_alta_equipo
    @equipo_id        INT           = NULL,   -- NULL = alta, con valor = edición
    @nombre           NVARCHAR(250),
    @area             NVARCHAR(80),
    @categoria        NVARCHAR(120) = NULL,
    @estado           NVARCHAR(40)  = N'Operativo',
    @descripcion      NVARCHAR(500) = NULL,
    @marca            NVARCHAR(120) = NULL,
    @modelo           NVARCHAR(120) = NULL,
    @numero_serie     NVARCHAR(120) = NULL,
    @ubicacion        NVARCHAR(150) = NULL,
    @comentarios      NVARCHAR(500) = NULL,
    @fecha_adquisicion DATE         = NULL,
    @equipo_id_out    INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @area_id INT, @categoria_id INT, @estado_id INT;

    SELECT @area_id = area_id FROM dbo.bio_area WHERE nombre = @area;
    IF @area_id IS NULL THROW 50120, 'Área no encontrada.', 1;

    IF @categoria IS NOT NULL
        SELECT @categoria_id = categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = @categoria;

    SELECT @estado_id = estado_id FROM dbo.bio_estado_equipo WHERE nombre = @estado;
    IF @estado_id IS NULL THROW 50121, 'Estado no encontrado.', 1;

    IF @equipo_id IS NULL
    BEGIN
        INSERT INTO dbo.bio_equipo
            (nombre, descripcion, marca, modelo, numero_serie, area_id, categoria_id,
             estado_id, ubicacion, comentarios, fecha_adquisicion)
        VALUES
            (@nombre, @descripcion, @marca, @modelo, @numero_serie, @area_id, @categoria_id,
             @estado_id, @ubicacion, @comentarios, @fecha_adquisicion);
        SET @equipo_id_out = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.bio_equipo SET
            nombre = @nombre, descripcion = @descripcion, marca = @marca, modelo = @modelo,
            numero_serie = @numero_serie, area_id = @area_id, categoria_id = @categoria_id,
            estado_id = @estado_id, ubicacion = @ubicacion, comentarios = @comentarios,
            fecha_adquisicion = @fecha_adquisicion, actualizado_en = SYSDATETIME()
        WHERE equipo_id = @equipo_id;
        SET @equipo_id_out = @equipo_id;
    END

    SELECT * FROM dbo.vw_bio_inventario WHERE equipo_id = @equipo_id_out;
END
GO

-- ============================================================================
-- SP: usp_bio_alta_insumo  (upsert por clave)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_bio_alta_insumo
    @clave        NVARCHAR(20),
    @nombre       NVARCHAR(250),
    @categoria    NVARCHAR(120) = NULL,
    @unidad       NVARCHAR(40)  = N'Pieza',
    @descripcion  NVARCHAR(500) = NULL,
    @stock_minimo DECIMAL(18,3) = 0,
    @stock_maximo DECIMAL(18,3) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @categoria_id INT;
    IF @categoria IS NOT NULL
        SELECT @categoria_id = categoria_id FROM dbo.bio_categoria_insumo WHERE nombre = @categoria;

    MERGE dbo.bio_insumo AS t
    USING (SELECT @clave AS clave) AS s
       ON t.clave = s.clave
    WHEN MATCHED THEN UPDATE SET
        nombre = @nombre, descripcion = @descripcion, categoria_id = @categoria_id,
        unidad = @unidad, stock_minimo = @stock_minimo, stock_maximo = @stock_maximo,
        actualizado_en = SYSDATETIME()
    WHEN NOT MATCHED THEN INSERT
        (clave, nombre, descripcion, categoria_id, unidad, stock_minimo, stock_maximo)
        VALUES
        (@clave, @nombre, @descripcion, @categoria_id, @unidad, @stock_minimo, @stock_maximo);

    SELECT * FROM dbo.vw_bio_insumos WHERE clave = @clave;
END
GO

-- ============================================================================
-- SP: usp_bio_registrar_movimiento
-- ----------------------------------------------------------------------------
-- Registra entrada/salida contra un lote específico. Para salidas sin lote
-- indicado, el controller debe resolver primero el lote FEFO (el de
-- caducidad más próxima con existencia > 0) antes de llamar este SP.
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_bio_registrar_movimiento
    @clave_insumo    NVARCHAR(20),
    @lote            NVARCHAR(60),
    @tipo_movimiento NVARCHAR(40),
    @cantidad        DECIMAL(18,3),
    @responsable     NVARCHAR(120) = NULL,
    @observaciones   NVARCHAR(500) = NULL,
    @caducidad       DATE          = NULL,   -- solo se usa si el lote no existe (alta implícita en Entrada)
    @movimiento_id   INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @insumo_id INT, @lote_id INT, @tipo_id INT, @signo SMALLINT, @existencia DECIMAL(18,3);

    IF @cantidad IS NULL OR @cantidad <= 0
        THROW 50130, 'La cantidad debe ser mayor a cero.', 1;

    SELECT @insumo_id = insumo_id FROM dbo.bio_insumo WHERE clave = @clave_insumo;
    IF @insumo_id IS NULL THROW 50131, 'El insumo no existe.', 1;

    SELECT @tipo_id = tipo_id, @signo = signo FROM dbo.bio_tipo_movimiento WHERE nombre = @tipo_movimiento;
    IF @tipo_id IS NULL THROW 50132, 'Tipo de movimiento no válido.', 1;

    SELECT @lote_id = lote_id FROM dbo.bio_lote WHERE insumo_id = @insumo_id AND lote = @lote;
    IF @lote_id IS NULL
    BEGIN
        IF @signo < 0
            THROW 50133, 'El lote no existe: no se puede registrar una salida contra un lote inexistente.', 1;

        INSERT INTO dbo.bio_lote (insumo_id, lote, caducidad) VALUES (@insumo_id, @lote, @caducidad);
        SET @lote_id = SCOPE_IDENTITY();
    END

    IF @signo < 0
    BEGIN
        SELECT @existencia = existencia FROM dbo.vw_bio_lotes WHERE lote_id = @lote_id;
        IF ISNULL(@existencia, 0) < @cantidad
            THROW 50134, 'Existencia insuficiente en el lote para registrar la salida.', 1;
    END

    INSERT INTO dbo.bio_movimiento (tipo_id, insumo_id, lote_id, cantidad, responsable, observaciones)
    VALUES (@tipo_id, @insumo_id, @lote_id, @cantidad, @responsable, @observaciones);

    SET @movimiento_id = SCOPE_IDENTITY();

    SELECT @movimiento_id AS movimiento_id, @lote_id AS lote_id;
END
GO

PRINT '=== Vistas y procedimientos Biomédico listos ===';
GO
