/* =====================================================================
   Etapa 1 - Historial de Recetas, Colectivos y Recepcion.

   El Access nunca guardo lo PLANEADO (cantidad autorizada de una COM,
   cantidad prescrita de una receta, cuadro autorizado por servicio); solo
   guardo lo ENTREGADO. Estas vistas exponen lo entregado tal cual, sin
   inventar el lado que falta: los porcentajes de surtimiento y de abasto
   se quedan en NULL hasta que existan las tablas de captura (Etapa 2).
   ===================================================================== */

/* ---------------------------------------------------------------------
   Un renglon por documento de salida, con su medico y sus totales.
   'tipo_documento' agrupa el texto libre de Access, que trae decenas de
   variantes con errores de dedo (Recet, R3ECETA, HOSP COLECTIVBO...).
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_salida_documento
AS
WITH normalizado AS (
    SELECT
        se.id_salida,
        se.fecha_hora_salida,
        se.tipo_salida,
        se.servicio_destino,
        se.entregado_a,
        se.turno,
        se.observaciones,
        se.folio_documento,
        se.id_medico,
        UPPER(REPLACE(LTRIM(RTRIM(ISNULL(se.tipo_salida, N''))), N' ', N'')) AS t
    FROM dbo.farm_salida_encabezado se
)
SELECT
    n.id_salida,
    /* Folio_Documento viene vacio en todo el Access: se arma uno estable
       con el consecutivo del documento para que la pantalla no lo muestre
       en blanco. */
    ISNULL(NULLIF(LTRIM(RTRIM(n.folio_documento)), N''),
           N'S-' + CAST(n.id_salida AS nvarchar(20)))     AS folio,
    n.fecha_hora_salida                                    AS fecha,
    CASE
        WHEN n.t LIKE N'RECET%' OR n.t LIKE N'RECE%'
          OR n.t IN (N'R', N'R4', N'R3ECETA')              THEN N'Receta'
        WHEN n.t LIKE N'%COLECTIV%'                        THEN N'Colectivo'
        WHEN n.t LIKE N'%CADUC%' OR n.t LIKE N'%TEMPERATURA%'
          OR n.t LIKE N'%INADECUAD%'                       THEN N'Merma'
        WHEN n.t LIKE N'%DUPLICAD%' OR n.t LIKE N'%CORREGIR%'
          OR n.t LIKE N'%PRUEBA%'                          THEN N'Ajuste'
        WHEN n.t LIKE N'PIS%' OR n.t LIKE N'HOS%'
          OR n.t LIKE N'H' OR n.t LIKE N'HI%'              THEN N'Piso/Hospital'
        WHEN n.t = N'' OR n.t LIKE N'NO%APLICA%'           THEN N'Sin tipo'
        ELSE N'Otra salida'
    END                                                    AS tipo_documento,
    n.tipo_salida                                          AS tipo_capturado,
    n.servicio_destino                                     AS servicio,
    n.entregado_a,
    n.turno,
    n.observaciones,
    med.nombre_completo                                    AS medico,
    med.especialidad,
    ISNULL(d.lineas, 0)                                    AS total_lineas,
    ISNULL(d.entregado, 0)                                 AS total_entregado
FROM normalizado n
LEFT JOIN dbo.farm_medico med
       ON med.id_medico = CAST(n.id_medico AS nvarchar(50))
LEFT JOIN (
    SELECT id_salida, COUNT(*) AS lineas, SUM(CAST(cantidad AS decimal(18,2))) AS entregado
    FROM dbo.farm_salida_detalle
    GROUP BY id_salida
) d ON d.id_salida = n.id_salida;
GO

/* ---------------------------------------------------------------------
   Renglones de un documento de salida, con la clave del cuadro basico.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_salida_renglon
AS
SELECT
    sd.id_detalle_salida,
    sd.id_salida,
    m.id                                     AS medicamento_id,
    m.clave                                  AS codigo,
    sd.codigo_barra                          AS codigobarras,
    m.descripcion_articulo_completa          AS articulo,
    CAST(sd.cantidad AS decimal(18,2))       AS cantidad_entregada,
    sd.lote,
    sd.caducidad
FROM dbo.farm_salida_detalle sd
LEFT JOIN dbo.farm_medicamento m ON m.codigobarras = sd.codigo_barra;
GO

/* ---------------------------------------------------------------------
   Recepcion: lo recibido agrupado por orden de suministro.
   No hay cantidad autorizada en Access, asi que no se calcula ningun
   porcentaje aqui; el controller devuelve NULL y la pantalla lo indica.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_recepcion_historica
AS
SELECT
    /* Folio_Documento esta vacio en las 2,844 entradas; la orden de
       suministro y la factura son lo unico que identifica el documento. */
    ISNULL(NULLIF(LTRIM(RTRIM(ee.orden_suministro)), N''),
           ISNULL(NULLIF(LTRIM(RTRIM(ee.num_factura)), N''),
                  N'E-' + CAST(ee.id_entrada AS nvarchar(20)))) AS com_numero,
    MIN(ee.fecha_hora_recepcion)                        AS fecha_documento,
    MAX(ISNULL(p.nombre, ee.id_proveedor))              AS proveedor,
    m.clave                                             AS codigo,
    MAX(m.descripcion_articulo_completa)                AS articulo,
    SUM(CAST(ed.cantidad AS decimal(18,2)))             AS cantidad_recibida,
    COUNT(*)                                            AS renglones
FROM dbo.farm_entrada_detalle ed
INNER JOIN dbo.farm_entrada_encabezado ee ON ee.id_entrada = ed.id_entrada
LEFT  JOIN dbo.farm_medicamento m         ON m.codigobarras = ed.codigo_barra
LEFT  JOIN dbo.farm_proveedor_original p  ON p.id_proveedor = ee.id_proveedor
GROUP BY
    ISNULL(NULLIF(LTRIM(RTRIM(ee.orden_suministro)), N''),
           ISNULL(NULLIF(LTRIM(RTRIM(ee.num_factura)), N''),
                  N'E-' + CAST(ee.id_entrada AS nvarchar(20)))),
    m.clave;
GO

/* ---------------------------------------------------------------------
   Cuadro de facto por servicio: que claves ha consumido cada servicio.
   No es el cuadro AUTORIZADO (ese no existe todavia), es el historico.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_farm_servicio_clave_historica
AS
SELECT
    d.servicio,
    r.codigo,
    MAX(r.articulo)                     AS articulo,
    SUM(r.cantidad_entregada)           AS cantidad_periodo,
    COUNT(DISTINCT d.id_salida)         AS documentos,
    MIN(d.fecha)                        AS desde,
    MAX(d.fecha)                        AS hasta
FROM dbo.vw_farm_salida_documento d
INNER JOIN dbo.vw_farm_salida_renglon r ON r.id_salida = d.id_salida
WHERE d.servicio IS NOT NULL
  AND LTRIM(RTRIM(d.servicio)) <> N''
  AND r.codigo IS NOT NULL
GROUP BY d.servicio, r.codigo;
GO
