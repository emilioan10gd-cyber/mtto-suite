-- Migración 003: Mejorar tabla mtto_precio_proveedor
-- Cambios: Agregar cantidad_maxima, remover tipo_precio (se deduce de rangos)

-- Respaldar datos existentes
SELECT * INTO #backup_precios FROM mtto_precio_proveedor;

-- Eliminar constraint único si existe
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
           WHERE TABLE_NAME='mtto_precio_proveedor' AND CONSTRAINT_TYPE='UNIQUE')
BEGIN
    ALTER TABLE mtto_precio_proveedor DROP CONSTRAINT [UQ_articulo_proveedor_tipo_cantidad];
END

-- Agregar columna cantidad_maxima si no existe
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME='mtto_precio_proveedor' AND COLUMN_NAME='cantidad_maxima')
BEGIN
    ALTER TABLE mtto_precio_proveedor ADD cantidad_maxima DECIMAL(18,3) NULL;
END

-- Remover columna tipo_precio si existe
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME='mtto_precio_proveedor' AND COLUMN_NAME='tipo_precio')
BEGIN
    ALTER TABLE mtto_precio_proveedor DROP COLUMN tipo_precio;
END

-- Crear unique constraint por articulo, proveedor, rango de cantidad
-- (evita solapamiento: no puede haber dos precios para el mismo articulo/proveedor/cantidad)
ALTER TABLE mtto_precio_proveedor ADD CONSTRAINT UQ_precio_rango
  UNIQUE (articulo_id, proveedor_id, cantidad_minima, cantidad_maxima);

-- Log de migración
INSERT INTO [dbo].[_migraciones] (nombre, ejecutado_en)
VALUES ('003_mejorar_precios_proveedor', GETDATE());
