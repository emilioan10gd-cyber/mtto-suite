-- ============================================================================
-- ALMACÉN CENTRAL: ENLACE PROVEEDOR <-> RFC (N:1)
-- ============================================================================
-- Creado: 2026-09-02
--
-- Hasta ahora alm_proveedor tenía un ÚNICO campo rfc. Con este script se
-- cambia a un modelo donde un proveedor puede tener MÚLTIPLES RFC
-- (ej. cambió de nombre, tiene sucursales, etc.) y se controla cuál es
-- el RFC "activo" (el que se usa por defecto en nuevas entradas).
--
-- Migración: Los RFC existentes se copian a la nueva tabla y se marcan
-- como activos. Todos los datos históricos se preservan.
-- ============================================================================

USE [InventarioMantenimiento];
GO

-- ---- 1. Crear tabla alm_proveedor_rfc ----
IF OBJECT_ID('dbo.alm_proveedor_rfc', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.alm_proveedor_rfc (
        proveedor_rfc_id INT IDENTITY(1,1) PRIMARY KEY,
        proveedor_id     INT            NOT NULL,
        rfc              NVARCHAR(20)   NOT NULL,
        es_activo        BIT            NOT NULL CONSTRAINT DF_alm_prov_rfc_activo DEFAULT (1),
        creado_en        DATETIME2(0)   NOT NULL CONSTRAINT DF_alm_prov_rfc_creado DEFAULT (SYSDATETIME()),

        CONSTRAINT FK_alm_prov_rfc_prov FOREIGN KEY (proveedor_id) REFERENCES dbo.alm_proveedor(proveedor_id),
        -- Evitar RFC duplicados para el MISMO proveedor
        CONSTRAINT UQ_alm_prov_rfc_combo UNIQUE (proveedor_id, rfc),
        -- RFC debe tener por lo menos 8 caracteres
        CONSTRAINT CK_alm_prov_rfc_len CHECK (LEN(TRIM(rfc)) >= 8)
    );
    PRINT 'Tabla alm_proveedor_rfc creada';
END
GO

-- ---- 2. Migrar RFC existentes de alm_proveedor ----
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='alm_proveedor' AND COLUMN_NAME='rfc')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.alm_proveedor_rfc)
    BEGIN
        INSERT INTO dbo.alm_proveedor_rfc (proveedor_id, rfc, es_activo)
        SELECT proveedor_id, rfc, 1
        FROM dbo.alm_proveedor
        WHERE rfc IS NOT NULL AND TRIM(rfc) != '';

        PRINT CAST(@@ROWCOUNT AS NVARCHAR) + ' RFC migrados a la nueva tabla';
    END
END
GO

-- ---- 3. Índice de búsqueda ----
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_alm_prov_rfc_proveedor')
BEGIN
    CREATE INDEX IX_alm_prov_rfc_proveedor ON dbo.alm_proveedor_rfc(proveedor_id) INCLUDE (rfc, es_activo);
    PRINT 'Índice IX_alm_prov_rfc_proveedor creado';
END
GO

PRINT '=== Almacén Central: RFC enlazado a proveedor ===';
GO
