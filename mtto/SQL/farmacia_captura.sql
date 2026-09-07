/* =====================================================================
   Etapa 2 - Tablas de captura de la app.

   REGLA DE ORO: nada de esto se guarda en farm_entrada_detalle ni en
   farm_salida_detalle. Esas dos son el espejo del Access y la
   sincronizacion las vacia y recarga; cualquier captura hecha aqui se
   perderia en el siguiente sync.

   Lo capturado vive en farm_movimiento y las vistas lo SUMAN a lo que
   viene del Access. Asi conviven las dos fuentes sin pisarse.
   ===================================================================== */

/* ---------------------------------------------------------------------
   Movimientos capturados en la app.
   signo: +1 suma al inventario, -1 resta. Se guarda explicito para que
   la vista no tenga que interpretar el texto del tipo.
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.farm_movimiento') IS NULL
CREATE TABLE dbo.farm_movimiento (
    movimiento_id   int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    tipo            nvarchar(30)   NOT NULL,
    signo           smallint       NOT NULL,
    codigobarras    nvarchar(200)  NOT NULL,
    lote            nvarchar(100)  NULL,
    caducidad       datetime       NULL,
    cantidad        decimal(18,2)  NOT NULL,
    fecha           datetime       NOT NULL CONSTRAINT DF_farm_movimiento_fecha DEFAULT GETDATE(),
    folio           nvarchar(100)  NULL,
    area_servicio   nvarchar(255)  NULL,
    responsable     nvarchar(255)  NULL,
    observaciones   nvarchar(500)  NULL,
    /* De donde salio: receta | colectivo | recepcion | manual */
    origen          nvarchar(30)   NULL,
    origen_id       int            NULL,
    usuario         nvarchar(40)   NULL,
    creado_en       datetime       NOT NULL CONSTRAINT DF_farm_movimiento_creado DEFAULT GETDATE(),
    CONSTRAINT CK_farm_movimiento_signo    CHECK (signo IN (-1, 1)),
    CONSTRAINT CK_farm_movimiento_cantidad CHECK (cantidad > 0)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_farm_movimiento_codigo')
    CREATE INDEX IX_farm_movimiento_codigo ON dbo.farm_movimiento (codigobarras, lote);
GO

/* ---------------------------------------------------------------------
   COM: orden de compra autorizada. Es el lado que faltaba para poder
   calcular el % de surtimiento (recibido contra autorizado).
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.farm_com') IS NULL
CREATE TABLE dbo.farm_com (
    com_id          int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    com_numero      nvarchar(100)  NOT NULL UNIQUE,
    id_proveedor    nvarchar(50)   NULL,
    proveedor       nvarchar(255)  NULL,
    fecha_documento datetime       NULL,
    observaciones   nvarchar(500)  NULL,
    usuario         nvarchar(40)   NULL,
    creado_en       datetime       NOT NULL CONSTRAINT DF_farm_com_creado DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.farm_com_detalle') IS NULL
CREATE TABLE dbo.farm_com_detalle (
    com_detalle_id      int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    com_id              int            NOT NULL,
    /* Se guarda la clave del cuadro basico: la COM se autoriza por clave,
       no por presentacion/codigo de barras. */
    clave               nvarchar(100)  NOT NULL,
    cantidad_autorizada decimal(18,2)  NOT NULL,
    CONSTRAINT FK_farm_com_detalle_com FOREIGN KEY (com_id)
        REFERENCES dbo.farm_com (com_id) ON DELETE CASCADE,
    CONSTRAINT CK_farm_com_detalle_cant CHECK (cantidad_autorizada > 0)
);
GO

/* ---------------------------------------------------------------------
   Receta: lo PRESCRITO. El paciente se guarda aqui mismo (Cat_Pacientes
   del Access esta vacia y en la practica el nombre se venia anotando en
   el campo "entregado a" de la salida).
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.farm_receta') IS NULL
CREATE TABLE dbo.farm_receta (
    receta_id            int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    folio                nvarchar(50)   NOT NULL UNIQUE,
    fecha                datetime       NOT NULL CONSTRAINT DF_farm_receta_fecha DEFAULT GETDATE(),
    medico               nvarchar(255)  NULL,
    id_medico            nvarchar(50)   NULL,
    paciente_nombre      nvarchar(255)  NOT NULL,
    paciente_expediente  nvarchar(100)  NULL,
    servicio             nvarchar(255)  NULL,
    observaciones        nvarchar(500)  NULL,
    usuario              nvarchar(40)   NULL,
    creado_en            datetime       NOT NULL CONSTRAINT DF_farm_receta_creado DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.farm_receta_detalle') IS NULL
CREATE TABLE dbo.farm_receta_detalle (
    receta_detalle_id   int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    receta_id           int            NOT NULL,
    clave               nvarchar(100)  NOT NULL,
    cantidad_prescrita  decimal(18,2)  NOT NULL,
    /* Se va acumulando conforme se dispensa por FEFO. La diferencia
       contra lo prescrito ES el indicador de abasto. */
    cantidad_entregada  decimal(18,2)  NOT NULL CONSTRAINT DF_farm_receta_det_ent DEFAULT 0,
    CONSTRAINT FK_farm_receta_detalle_receta FOREIGN KEY (receta_id)
        REFERENCES dbo.farm_receta (receta_id) ON DELETE CASCADE,
    CONSTRAINT CK_farm_receta_detalle_cant CHECK (cantidad_prescrita > 0)
);
GO

/* ---------------------------------------------------------------------
   Cuadro AUTORIZADO por servicio (lo que hoy solo existe como consumo
   historico en vw_farm_servicio_clave_historica).
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.farm_servicio_clave') IS NULL
CREATE TABLE dbo.farm_servicio_clave (
    servicio_clave_id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    servicio          nvarchar(255)  NOT NULL,
    clave             nvarchar(100)  NOT NULL,
    cantidad_periodo  decimal(18,2)  NOT NULL,
    periodo           nvarchar(50)   NOT NULL CONSTRAINT DF_farm_servicio_clave_per DEFAULT N'Mensual',
    activo            bit            NOT NULL CONSTRAINT DF_farm_servicio_clave_act DEFAULT 1,
    usuario           nvarchar(40)   NULL,
    creado_en         datetime       NOT NULL CONSTRAINT DF_farm_servicio_clave_cre DEFAULT GETDATE(),
    CONSTRAINT UQ_farm_servicio_clave UNIQUE (servicio, clave)
);
GO

/* ---------------------------------------------------------------------
   Colectivo capturado en la app (cabecera; los renglones se guardan
   como movimientos con origen='colectivo').
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.farm_colectivo') IS NULL
CREATE TABLE dbo.farm_colectivo (
    colectivo_id   int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    folio          nvarchar(50)   NOT NULL UNIQUE,
    fecha          datetime       NOT NULL CONSTRAINT DF_farm_colectivo_fecha DEFAULT GETDATE(),
    servicio       nvarchar(255)  NOT NULL,
    entregado_por  nvarchar(255)  NULL,
    recibido_por   nvarchar(255)  NULL,
    turno          nvarchar(50)   NULL,
    observaciones  nvarchar(500)  NULL,
    usuario        nvarchar(40)   NULL,
    creado_en      datetime       NOT NULL CONSTRAINT DF_farm_colectivo_creado DEFAULT GETDATE()
);
GO

/* =====================================================================
   Vistas reconstruidas: ahora suman Access + capturas de la app.

   La version anterior hacia FULL OUTER JOIN entre entradas y salidas.
   Con tres fuentes eso ya no escala, asi que se normaliza todo a un solo
   flujo de movimientos con signo y se agrupa una sola vez. De paso el
   codigo queda mas corto y sin riesgo de volver a producir un producto
   cartesiano.
   ===================================================================== */
CREATE OR ALTER VIEW dbo.vw_farm_lotes
AS
WITH flujo AS (
    /* Entradas registradas en el Access */
    SELECT ed.codigo_barra                        AS codigobarras,
           ed.lote,
           ed.fecha_caducidad,
           CAST(ed.cantidad AS decimal(18,2))     AS cantidad,
           1                                      AS signo,
           ee.fecha_hora_recepcion                AS fecha,
           ed.ubicacion
    FROM dbo.farm_entrada_detalle ed
    LEFT JOIN dbo.farm_entrada_encabezado ee ON ee.id_entrada = ed.id_entrada

    UNION ALL

    /* Salidas registradas en el Access */
    SELECT sd.codigo_barra,
           sd.lote,
           sd.caducidad,
           CAST(sd.cantidad AS decimal(18,2)),
           -1,
           se.fecha_hora_salida,
           NULL
    FROM dbo.farm_salida_detalle sd
    LEFT JOIN dbo.farm_salida_encabezado se ON se.id_salida = sd.id_salida

    UNION ALL

    /* Capturado en esta app (no lo toca la sincronizacion) */
    SELECT mv.codigobarras,
           mv.lote,
           mv.caducidad,
           mv.cantidad,
           mv.signo,
           mv.fecha,
           NULL
    FROM dbo.farm_movimiento mv
),
agrupado AS (
    SELECT
        f.codigobarras,
        LTRIM(RTRIM(UPPER(ISNULL(f.lote, N''))))                       AS lote_norm,
        MAX(f.lote)                                                    AS lote,
        MIN(f.fecha_caducidad)                                         AS fecha_caducidad,
        SUM(CASE WHEN f.signo > 0 THEN f.cantidad ELSE 0 END)          AS recibido,
        SUM(CASE WHEN f.signo < 0 THEN f.cantidad ELSE 0 END)          AS despachado,
        MAX(CASE WHEN f.signo > 0 THEN f.fecha END)                    AS ultima_entrada,
        MAX(f.ubicacion)                                               AS ubicacion
    FROM flujo f
    GROUP BY f.codigobarras, LTRIM(RTRIM(UPPER(ISNULL(f.lote, N''))))
)
SELECT
    m.id                            AS medicamento_id,
    m.clave,
    a.codigobarras,
    m.descripcion_articulo_completa AS nombre,
    NULLIF(a.lote, N'')             AS lote,
    a.fecha_caducidad,
    a.recibido,
    a.despachado,
    a.recibido - a.despachado       AS existencia,
    a.ubicacion,
    a.ultima_entrada,
    CASE
        WHEN a.fecha_caducidad IS NULL                                      THEN N'Sin fecha'
        WHEN a.fecha_caducidad <  CAST(GETDATE() AS date)                   THEN N'Vencido'
        WHEN a.fecha_caducidad <  DATEADD(day, 30, CAST(GETDATE() AS date)) THEN N'Critico'
        WHEN a.fecha_caducidad <  DATEADD(day, 90, CAST(GETDATE() AS date)) THEN N'Por vencer'
        ELSE N'Vigente'
    END                             AS estado_caducidad
FROM agrupado a
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = a.codigobarras;
GO

CREATE OR ALTER VIEW dbo.vw_farm_movimientos
AS
SELECT
    N'Entrada'                              AS tipo,
    N'Entrada'                              AS tipo_detalle,
    ed.id_detalle                           AS movimiento_id,
    ee.id_entrada                           AS documento_id,
    ed.codigo_barra                         AS codigobarras,
    m.clave,
    m.descripcion_articulo_completa         AS nombre,
    CAST(ed.cantidad AS decimal(18,2))      AS cantidad,
    ed.lote,
    ed.fecha_caducidad                      AS caducidad,
    ee.fecha_hora_recepcion                 AS fecha,
    ISNULL(NULLIF(LTRIM(RTRIM(ee.folio_documento)), N''),
           NULLIF(LTRIM(RTRIM(ee.num_factura)), N''))  AS folio,
    ee.orden_suministro                     AS area_servicio,
    CAST(NULL AS nvarchar(255))             AS responsable,
    ee.observaciones,
    N'Access'                               AS fuente
FROM dbo.farm_entrada_detalle ed
INNER JOIN dbo.farm_entrada_encabezado ee ON ee.id_entrada = ed.id_entrada
LEFT  JOIN dbo.farm_medicamento m         ON m.codigobarras = ed.codigo_barra

UNION ALL

SELECT
    CASE
        WHEN UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%CADUC%'
          OR UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%TEMPERATURA%'
          OR UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%INADECUAD%'      THEN N'Merma'
        WHEN UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%DUPLICAD%'
          OR UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%CORREGIR%'
          OR UPPER(ISNULL(se.tipo_salida, N'')) LIKE N'%PRUEBA%'         THEN N'Ajuste (-)'
        ELSE N'Salida'
    END,
    ISNULL(NULLIF(LTRIM(RTRIM(se.tipo_salida)), N''), N'Sin tipo'),
    sd.id_detalle_salida,
    se.id_salida,
    sd.codigo_barra,
    m.clave,
    m.descripcion_articulo_completa,
    CAST(sd.cantidad AS decimal(18,2)),
    sd.lote,
    sd.caducidad,
    se.fecha_hora_salida,
    ISNULL(NULLIF(LTRIM(RTRIM(se.folio_documento)), N''),
           N'S-' + CAST(se.id_salida AS nvarchar(20))),
    se.servicio_destino,
    se.entregado_a,
    se.observaciones,
    N'Access'
FROM dbo.farm_salida_detalle sd
INNER JOIN dbo.farm_salida_encabezado se ON se.id_salida = sd.id_salida
LEFT  JOIN dbo.farm_medicamento m        ON m.codigobarras = sd.codigo_barra

UNION ALL

SELECT
    mv.tipo,
    ISNULL(mv.origen, N'manual'),
    mv.movimiento_id,
    ISNULL(mv.origen_id, 0),
    mv.codigobarras,
    m.clave,
    m.descripcion_articulo_completa,
    mv.cantidad,
    mv.lote,
    mv.caducidad,
    mv.fecha,
    mv.folio,
    mv.area_servicio,
    mv.responsable,
    mv.observaciones,
    N'App'
FROM dbo.farm_movimiento mv
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = mv.codigobarras;
GO

/* ---------------------------------------------------------------------
   Avance de surtimiento: lo autorizado en la COM contra lo recibido.
   Lo recibido suma las entradas del Access ligadas a esa orden de
   suministro MAS las recepciones capturadas en la app.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_avance_com
AS
WITH recibido AS (
    /* Recepciones del Access, ligadas por la orden de suministro */
    SELECT LTRIM(RTRIM(ee.orden_suministro)) AS com_numero,
           m.clave,
           SUM(CAST(ed.cantidad AS decimal(18,2))) AS cantidad
    FROM dbo.farm_entrada_detalle ed
    INNER JOIN dbo.farm_entrada_encabezado ee ON ee.id_entrada = ed.id_entrada
    LEFT  JOIN dbo.farm_medicamento m         ON m.codigobarras = ed.codigo_barra
    WHERE NULLIF(LTRIM(RTRIM(ee.orden_suministro)), N'') IS NOT NULL
    GROUP BY LTRIM(RTRIM(ee.orden_suministro)), m.clave

    UNION ALL

    /* Recepciones capturadas en la app: el folio guarda el numero de COM */
    SELECT LTRIM(RTRIM(mv.folio)), m.clave, SUM(mv.cantidad)
    FROM dbo.farm_movimiento mv
    LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = mv.codigobarras
    WHERE mv.origen = N'recepcion'
      AND NULLIF(LTRIM(RTRIM(mv.folio)), N'') IS NOT NULL
    GROUP BY LTRIM(RTRIM(mv.folio)), m.clave
)
SELECT
    c.com_numero,
    c.fecha_documento,
    c.proveedor,
    d.clave                                          AS codigo,
    med.descripcion                                  AS articulo,
    d.cantidad_autorizada,
    ISNULL(r.cantidad, 0)                            AS cantidad_recibida,
    CASE WHEN d.cantidad_autorizada - ISNULL(r.cantidad, 0) > 0
         THEN d.cantidad_autorizada - ISNULL(r.cantidad, 0) ELSE 0 END AS pendiente,
    CAST(ROUND(100.0 * ISNULL(r.cantidad, 0) / NULLIF(d.cantidad_autorizada, 0), 0) AS decimal(18,0)) AS porcentaje_surtido
FROM dbo.farm_com c
INNER JOIN dbo.farm_com_detalle d ON d.com_id = c.com_id
LEFT  JOIN (
        SELECT com_numero, clave, SUM(cantidad) AS cantidad
        FROM recibido GROUP BY com_numero, clave
     ) r ON r.com_numero = c.com_numero AND r.clave = d.clave
OUTER APPLY (
        SELECT TOP 1 descripcion_articulo_completa AS descripcion
        FROM dbo.farm_medicamento WHERE clave = d.clave
     ) med;
GO

/* ---------------------------------------------------------------------
   Recetas capturadas en la app, con su avance de surtido.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_receta
AS
SELECT
    r.receta_id,
    r.folio,
    r.fecha,
    r.medico,
    r.paciente_nombre,
    r.paciente_expediente,
    r.servicio,
    r.observaciones,
    ISNULL(d.lineas, 0)              AS total_lineas,
    ISNULL(d.prescrito, 0)           AS total_prescrito,
    ISNULL(d.entregado, 0)           AS total_entregado,
    ISNULL(d.lineas_con_faltante, 0) AS lineas_con_faltante,
    CASE
        WHEN ISNULL(d.entregado, 0) = 0                     THEN N'Pendiente'
        WHEN ISNULL(d.lineas_con_faltante, 0) = 0           THEN N'Surtida'
        ELSE N'Parcial'
    END                              AS estado
FROM dbo.farm_receta r
LEFT JOIN (
    SELECT receta_id,
           COUNT(*)                  AS lineas,
           SUM(cantidad_prescrita)   AS prescrito,
           SUM(cantidad_entregada)   AS entregado,
           SUM(CASE WHEN cantidad_entregada < cantidad_prescrita THEN 1 ELSE 0 END) AS lineas_con_faltante
    FROM dbo.farm_receta_detalle
    GROUP BY receta_id
) d ON d.receta_id = r.receta_id;
GO

/* ---------------------------------------------------------------------
   Listas unificadas: lo capturado en la app + lo historico del Access,
   para que la pantalla muestre una sola lista.

   CONVENCION DE ID: positivo = capturado aqui (se puede dispensar);
   negativo = historico del Access (solo lectura). Es lo que permite que
   /recetas/{id} sepa a que fuente ir sin un parametro extra.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_receta_todas
AS
SELECT
    r.receta_id                     AS id,
    r.folio,
    r.fecha,
    r.medico,
    r.paciente_nombre,
    r.paciente_expediente,
    r.servicio,
    r.observaciones,
    r.total_lineas,
    CAST(r.total_prescrito AS decimal(18,2))    AS total_prescrito,
    CAST(r.total_entregado AS decimal(18,2))    AS total_entregado,
    r.lineas_con_faltante,
    r.estado,
    CAST(0 AS bit)                  AS historica
FROM dbo.vw_farm_receta r

UNION ALL

SELECT
    -d.id_salida,
    d.folio,
    d.fecha,
    d.medico,
    d.entregado_a,
    NULL,
    d.servicio,
    d.observaciones,
    d.total_lineas,
    NULL,
    CAST(d.total_entregado AS decimal(18,2)),
    NULL,
    N'Surtida',
    CAST(1 AS bit)
FROM dbo.vw_farm_salida_documento d
WHERE d.tipo_documento = N'Receta';
GO

CREATE OR ALTER VIEW dbo.vw_farm_colectivo_todos
AS
SELECT
    c.colectivo_id                  AS id,
    c.folio,
    c.fecha,
    c.servicio,
    c.entregado_por,
    c.recibido_por,
    c.turno,
    ISNULL(mv.lineas, 0)            AS total_lineas,
    ISNULL(mv.entregado, 0)         AS total_entregado,
    CAST(0 AS bit)                  AS historico
FROM dbo.farm_colectivo c
LEFT JOIN (
    SELECT origen_id, COUNT(*) AS lineas, SUM(cantidad) AS entregado
    FROM dbo.farm_movimiento
    WHERE origen = N'colectivo' AND origen_id IS NOT NULL
    GROUP BY origen_id
) mv ON mv.origen_id = c.colectivo_id

UNION ALL

SELECT
    -d.id_salida,
    d.folio,
    d.fecha,
    d.servicio,
    d.medico,
    d.entregado_a,
    d.turno,
    d.total_lineas,
    CAST(d.total_entregado AS decimal(18,2)),
    CAST(1 AS bit)
FROM dbo.vw_farm_salida_documento d
WHERE d.tipo_documento = N'Colectivo';
GO
