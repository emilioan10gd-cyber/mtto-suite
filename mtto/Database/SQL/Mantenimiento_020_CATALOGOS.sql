-- ============================================================================
-- MANTENIMIENTO (MTTO): CARGA DE CATÁLOGOS
-- ============================================================================
-- Origen: hoja "Catálogos" de MTTO_COMPLETE_DASHBOARD.xlsx
-- Idempotente: se puede re-ejecutar sin duplicar.
-- ============================================================================

USE [InventarioMantenimiento];
GO

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
