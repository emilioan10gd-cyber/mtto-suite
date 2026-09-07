-- ============================================================================
-- BIOMÉDICO: IMPORTACIÓN DE DATOS (ACTIVOS BIOMEDICO AREA.xlsx)
-- ============================================================================
-- Generado automáticamente a partir de las 11 hojas del Excel. 268 equipos
-- limpiados (se excluyeron filas vacías/merge-artifacts como 'TOTAL 52').
-- Estado inferido por palabras clave en Comentarios/Descripción:
--   'baja' -> De baja | 'no funciona'/'falla'/'falta' -> Fuera de servicio | resto -> Operativo
-- ============================================================================

USE [InventarioMantenimiento];
GO

-- ----------------------------------------------------------------------------
-- Áreas
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'RX Comodato')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'RX Comodato');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'Urgencias')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'Urgencias');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'Hemodiálisis')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'Hemodiálisis');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'UCIA')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'UCIA');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'Hospital')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'Hospital');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'Toco y Gine')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'Toco y Gine');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'UCIN y Pedia');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'QX')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'QX');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'CEYE')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'CEYE');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'Monitores')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'Monitores');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_area WHERE nombre = N'Ventiladores')
    INSERT INTO dbo.bio_area (nombre) VALUES (N'Ventiladores');
GO

-- ----------------------------------------------------------------------------
-- Categorías de equipo (derivadas de la Descripción del Activo)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Aspirador Portatil')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Aspirador Portatil');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Bascula')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Bascula');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Bomba');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Bomba Infusion');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Bombas De Infusion')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Bombas De Infusion');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Calentador')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Calentador');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Cama')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Cama');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Camas Hospitalarias');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Cardiotocografo')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Cardiotocografo');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Colposcopio')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Colposcopio');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Craneotomo')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Craneotomo');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Cuna De Calor Radiante')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Cuna De Calor Radiante');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Desfibrilador');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador Monitor')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Desfibrilador Monitor');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfribilador')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Desfribilador');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Digitalizador Rx')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Digitalizador Rx');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiografo')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Electrocardiografo');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiograma')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Electrocardiograma');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocauterio')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Electrocauterio');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Esterilizador  De Vapor Autogenerado')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Esterilizador  De Vapor Autogenerado');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Esterilizador De Vapor')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Esterilizador De Vapor');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Incubadora');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora Cerrada')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Incubadora Cerrada');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora De Traslado')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Incubadora De Traslado');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Lampara De Exploracion')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Lampara De Exploracion');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Maquina De Anestesia')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Maquina De Anestesia');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Maquina De Anestesio')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Maquina De Anestesio');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Mastografo')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Mastografo');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Quirurgica De Expulsion')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Mesa Quirurgica De Expulsion');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Quirurgica Universal')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Mesa Quirurgica Universal');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Qx')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Mesa Qx');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Monitor');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Monitor Cardiaco');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Monitor De Signos Vitales');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Monitor Sv');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Negatoscopio')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Negatoscopio');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Nota :      Son Como Dato')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Nota :      Son Como Dato');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Pistola Toma Biopsias')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Pistola Toma Biopsias');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Portatil')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Portatil');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Rayos X Fijo    (Sala )')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Rayos X Fijo    (Sala )');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Sierra Para Yeso')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Sierra Para Yeso');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Ultrasonido')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Ultrasonido');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Unidad De Plasma')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Unidad De Plasma');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Ventilador');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Ventilador Adulto-Pediatrico');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Mecanico')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Ventilador Mecanico');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Pediatrico Neonatal')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Ventilador Pediatrico Neonatal');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Ventilador Volumetrico');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico Adul/Ped')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Ventilador Volumetrico Adul/Ped');
IF NOT EXISTS (SELECT 1 FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico Neonatal')
    INSERT INTO dbo.bio_categoria_equipo (nombre) VALUES (N'Ventilador Volumetrico Neonatal');
GO

-- ----------------------------------------------------------------------------
-- Equipos
-- ----------------------------------------------------------------------------
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'PORTATIL', N'FUJIFILM', N'FDR NANO', N'6951395', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'RX Comodato'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Portatil'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'PORTATIL', N'FUJIFILM', N'FDR NANO', N'6951394', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'RX Comodato'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Portatil'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'RAYOS X FIJO    (SALA )', N'FUJIFILM', N'FDR SMART', N'DXB20A0353A', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'RX Comodato'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Rayos X Fijo    (Sala )'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MASTOGRAFO', N'FUJIFILM', N'AMULET FELICIA', N'AMFL/0057/C0', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'RX Comodato'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Mastografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'DIGITALIZADOR RX', N'FUJIFILM', N'FCR CAPSULA XLII', N'96373463', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'RX Comodato'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Digitalizador Rx'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ULTRASONIDO', N'FUJIFILM', N'ARIETTA 50', N'G3222755', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'RX Comodato'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ultrasonido'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'NOTA :      SON COMO DATO', NULL, NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'RX Comodato'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Nota :      Son Como Dato'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Desfibrilador Monitor', N'Lifepak', N'20E', N'47834510', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'CUBO DE SHOCK', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Desfibrilador Monitor', N'Lifepak', N'20E', N'47429656', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'CAMAS', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor de Signos Vitales', N'Mindray', N'uMEC12', N'KQ-98027085', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'CUBO DE SHOCK', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor de signos vitales', N'Goldway', N'G40E', N'CN 93904038', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'Cama 01', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor de signos vitales', N'Contec', N'CMS8000', N'23011400043', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'Cama 02', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Ventilador', N'Aomed', N'VG70', N'VG70(U)XZZT12361', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'CUBO DE SHOCK', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Ventilador', N'Aomed', N'VG70', N'VG70(U)XZZT19877', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'Cama 1', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bombas de Infusion', N'Fresenius', N'Optima ms sp', N'027180-21667786', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bombas De Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bombas de Infusion', N'Fresenius', N'Optima ms sp', N'027180-21963185', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bombas De Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bombas de Infusion', N'Fresenius', N'Optima ms sp', N'027180-21667802', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bombas De Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bombas de Infusion', N'Fresenius', N'Optima ms sp', N'027180-20072383', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bombas De Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Electrocardiografo', N'Contec', N'SG1200G', N'19122600095', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Electrocardiografo', N'Mindray', N'Beneheart R3', N'FK-56044185', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Lampara de exploracion', N'Rimsa', NULL, N'7123', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Lampara De Exploracion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Camas Hospitalarias', N'Linet', N'300 LT', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'shock', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Camas Hospitalarias', N'Orion', NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Camas Hospitalarias', N'Orion', NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Camas Hospitalarias', N'Orion', NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Camas Hospitalarias', N'Orion', NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Camas Hospitalarias', N'Orion', NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Camas Hospitalarias', N'Orion', NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Camas Hospitalarias', N'Hill-Rom', N'P8000', N'J332AQ2869', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Camas Hospitalarias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Negatoscopio', NULL, NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Negatoscopio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Negatoscopio', NULL, NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Negatoscopio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bascula', N'Nanofort', N'47909', N'BASC160-00437', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bascula'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bascula', N'Nanofort', N'47909', N'BASC160-00612', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bascula'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bascula', N'Givas', N'BS1600', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Urgencias'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bascula'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'DATASCOPE', N'TRIOM70', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hemodiálisis'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'DESFRIBILADOR', N'LIFEPAK', N'20E', N'47684905.0', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hemodiálisis'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfribilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'BOMBA', N'FRESENIUIS KABI', N'OPTIMA', N'210633420.0', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hemodiálisis'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ASPIRADOR PORTATIL', N'GONCO', N'7A-23B', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hemodiálisis'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Aspirador Portatil'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Electrocardiograma', N'EDAN', N'SE-12 EXPRESS', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiograma'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Desfibrilador', N'ZOLL M', N'BIPHASIC', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor cardiaco', N'EDAN', N'M80', N'360068-M1920714001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor cardiaco', N'GOLD WAY PHILIPS', N'G40E', N'CN93904030', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor cardiaco', N'GOLD WAY PHILIPS', N'G40E', N'CN93904035', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor cardiaco', N'GOLD WAY PHILIPS', N'G40E', N'CN93904043', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor cardiaco', N'GOLD WAY PHILIPS', N'G40E', N'CN93904031', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Ventilador mecanico', N'Hamilton', N'C2', N'8316', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Mecanico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Ventilador mecanico', N'Hamilton', N'C2', N'8315', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Mecanico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548923', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548928', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548943', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548944', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548980', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548981', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548983', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548989', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'MVP MSSP', NULL, N'21548921', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21548977', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21548964', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21548904', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21548959', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21548991', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21548923', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21548903', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21667650', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21667795', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Bomba infusion', N'OPTIMA MS', NULL, N'21667719', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Bomba Infusion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Ventilador mecanico', N'Mindrey', N'SV300', N'GB98010536', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Mecanico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Ventilador mecanico', N'Mindrey', N'SV300', N'GB98010537', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Mecanico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Ventilador mecanico', N'Mindrey', N'SV300', N'GB98010538', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Mecanico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Cama', N'LINET', N'Eleganza 5', N'20200191242', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cama'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), NULL, N'no funciona | resguardo 900', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Cama', N'LINET', N'Eleganza 5', N'20200191230', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cama'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), NULL, N'no funciona | resguardo 900', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Cama', N'LINET', N'Eleganza 5', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cama'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Cama', N'LINET', N'Eleganza 5', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cama'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Cama', N'LINET', N'Eleganza 5', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIA'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cama'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ELECTROCARDIOGRAFO', N'COMEN', N'H3', N'H319022206B', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ELECTROCARDIOGRAFO', N'BIONET10', N'EKG2000', N'EG0600255', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'DESFRIBILADOR', N'ZOLL', N'M SERIES', N'T14B133347', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfribilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO', N'AEOMED', N'VG70', N'VG70XZZT12315', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO', N'AEOMED', N'VG70', N'VG70XZZT9818', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO', N'AEOMED', N'VG70', N'VG70XZZT12287', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO', N'AEOMED', N'VG70', N'VG70XZZT12291', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO', N'AEOMED', N'VG70', N'VG70XZZT9824', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'CRC', N'VT-0', N'VITALTECH 1100', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'PHILIPS', N'G40E', N'CNAQ3904029', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'MINDRAY', N'UMEC12', N'KQ-7A007701', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'ADVANCED', N'PM-2000XPLUS', N'Y98001-A3J00135', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'DATASCOPE', N'TRIO', N'MC 23895-19', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'DESFRIBILADOR', N'LIFEPAK', N'20E', N'47699107', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfribilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ELECTROCARDIOGRAFO', N'PHILIPS', N'TC10', N'CND1811226', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ELECTROCARDIOGRAFO', N'PHILIPS', N'TC10', N'CND1811225', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocardiografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'MINDRAY', N'MEC-1000', N'AQ-8A117025', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'EDAN', N'M-80', N'360068-M19908060004', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'DESFRIBILADOR', N'HP', N'CODE MASTER', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfribilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'DESFRIBILADOR', N'ZOLL', N'R SERIES', N'AF19K107088', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfribilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR CARDIACO', N'PHILIPS', N'G40E', N'CN93904039', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Hospital'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Cardiaco'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'CARDIOTOCOGRAFO', N'PHILLIPS', N'ClearVue 350', N'M2702A', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cardiotocografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'COLPOSCOPIO', N'CARL ZEISS', NULL, N'GMBH', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Colposcopio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ELECTROCAUTERIO', N'COPER', NULL, N'SURGICAL', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocauterio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068-M19207140007', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068M-1861326002', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40', N'CN62639312', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40', N'CN93904033', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40', N'CN62639312', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'PM2000XLPRO', N'Y98002-A3J000325', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'PM2000XLPRO', N'ADV-140390', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ULTRASONIDO', N'PHILIPS', N'CLEAR VUE 350', N'45366148068', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ultrasonido'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'PISTOLA TOMA BIOPSIAS', N'BARD MAGNUM', N'MG1522', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Pistola Toma Biopsias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'PISTOLA TOMA BIOPSIAS', N'BARD MAGNUM', N'MG1522', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Pistola Toma Biopsias'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MESA QUIRURGICA UNIVERSAL', N'ADVANCED', N'OT-500', N'18-1532-004', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Quirurgica Universal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MAQUINA DE ANESTESIO', N'ADVANCED', N'AM-6000', N'5310530364002', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Maquina De Anestesio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MAQUINA DE ANESTESIO', N'ADVANCED', N'AM-6000', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Maquina De Anestesio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'CARDIOTOCOGRAFO', N'PHILIPS', NULL, N'DE65852716', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cardiotocografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'CARDIOTOCOGRAFO', N'EDAN', NULL, N'560034M19206130002', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cardiotocografo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'DESFIBRILADOR', N'ZOLL', NULL, N'1420800604001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'VERSAMED', NULL, N'IV21057', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MESA QUIRURGICA DE EXPULSION', N'ADVANCED', N'OT-500', N'5316165108-001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Quirurgica De Expulsion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MESA QUIRURGICA DE EXPULSION', NULL, NULL, NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Quirurgica De Expulsion'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ULTRASONIDO', N'SAMSUNG', N'SONO HACER 3', N'SOTEM3HJ600005W', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Toco y Gine'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ultrasonido'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ULTRASONIDO', N'ZONARE', N'ZONE SMART', N'06678C312F', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ultrasonido'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR PEDIATRICO NEONATAL', N'AEOMED', N'VGT70', N'ZZT12464', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Pediatrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR PEDIATRICO NEONATAL', N'AEOMED', N'VGT71', N'ZZT12403', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Pediatrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR PEDIATRICO NEONATAL', N'AEOMED', N'VGT72', N'ZZT12244', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Pediatrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR PEDIATRICO NEONATAL', N'PURITAN', N'840', N'3512152994', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Pediatrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'PEDIA', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'cuna de calor radiante', N'David', N'HKN 93b', N'24AHZH02006', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cuna De Calor Radiante'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'cuna de calor radiante', N'David', N'HKN 93b', N'24AHZH04011', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cuna De Calor Radiante'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'cuna de calor radiante', N'David', N'HKN 93b', N'24AHZH02001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cuna De Calor Radiante'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'cuna de calor radiante', N'David', N'HKN 93b', N'24AHZH0400', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cuna De Calor Radiante'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'cuna de calor radiante', N'Atmos care', N'Exspecte', N'IN28-668', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cuna De Calor Radiante'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'cuna de calor radiante', N'Atmos care', N'Exspecte', N'AP17-229', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Cuna De Calor Radiante'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Incubadora cerrada', N'David', N'YP-910', N'A06BAKD0100', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora Cerrada'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Incubadora cerrada', N'David', N'YP-910', N'06AHA2002', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora Cerrada'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Incubadora cerrada', N'David', N'YP-910', N'614103003', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora Cerrada'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Incubadora cerrada', N'David', N'YP-910', N'06AHAB02006', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora Cerrada'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Desfibrilador', N'Mediana', NULL, N'160919120009', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'ucin', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Desfibrilador', N'Lifepak', N'20e', N'47429656', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'urg ped', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Desfibrilador', N'Zoll', N'Mserie', N'010084794600068421T04E59169', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'ucin', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Desfibrilador', N'InnoMed', N'CardioAid360B', N'18206161', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'escolares', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Incubadora', N'insollet', N'c 200', N'WY29934', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Incubadora de traslado', N'V-808 ATOM', N'V-800', N'2340134', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Incubadora De Traslado'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'Edan', N'm80', N'360068-M18613260017', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'Edan', N'm9', N'101245-K12500210001-02', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'phillips', N'G40E', N'CN93904040', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'Edan', N'IM70', N'360080-M20603640023', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'Edan', N'm80', N'360068-M18613260013', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'Edan', N'm80', N'360068-M18613260016', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'Edan', N'm80', N'360068-M18613260018', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'Edan', N'm80', N'360068-M18613260012', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'phillips', N'G40E', N'CN93904041', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'phillips', N'G40E', N'CN93904036', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'DATASCOPE', N'TRIO M70', N'MC23889-19', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'EDAN', N'M9', N'101245-K12500210004-001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'ZEGEN', N'ZGN 7D', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor sv', N'ZEGEN', N'ZGN 7D', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'UCIN y Pedia'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor Sv'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Maquina de Anestesia', N'Drager', N'Perseus A500', N'ASTC-0064', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Maquina De Anestesia'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Maquina de Anestesia', N'Drager', N'Perseus A500', N'ASTC-0066', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Maquina De Anestesia'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Maquina de Anestesia', N'Drager', N'Atlan A350', N'ASRN-0426', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Maquina De Anestesia'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Maquina de Anestesia', N'Drager', N'Atlan A350', N'ASRN-0080', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Maquina De Anestesia'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Drager', N'Vista 120', N'VGSTJ0169', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Drager', N'Vista 120', N'VGSRF0957', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Drager', N'Vista 120', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Drager', N'Vista 120', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Electrocauterio', N'Alsa', N'Excell 400MCD', N'PE18045431', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocauterio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'SALA 1', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Electrocauterio', N'Alsa', N'Excell 400MCD', N'PE18046831', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocauterio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'SALA 2', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Electrocauterio', N'Alsa', N'Excell 400MCD', N'PE18047031', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocauterio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'SALA 3', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Electrocauterio', N'ALSA', N'Excell 400MCD', N'PE18046331', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Electrocauterio'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'SALA 4', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Mesa QX', N'ADVANCED', N'OT-500', N'17-T532-047', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Qx'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'SALA 1', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Mesa QX', N'ADVANCED', N'OT-500', N'18-T532-003', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Qx'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'SALA 2', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Mesa QX', N'ADVANCED', N'OT-500', N'18-1532-004', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Qx'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'SALA 4', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Mesa QX', N'MAQUET', N'ALPHASTAR', N'635', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Mesa Qx'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'SALA 3', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Desfibrilador', N'ZOLL', N'M series', N'T13K132868', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Desfibrilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Goldway', N'G40E', N'CN93904037', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Goldway', N'G40E', N'CN93904034', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Goldway', N'G40E', N'CN93904044', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Advanced', N'PM-2000 XL PRO', N'Y98002-A3J000324', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Advanced', N'PM-2000 XL PRO', N'Y98001-A3J000142', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'Goldway', N'G40E', N'CN93904042', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Monitor', N'EDAN', N'M80', N'36068-M19207140005', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Calentador', N'Covidien', N'warmTouch', N'SP17090505', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Calentador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Calentador', N'Covidien', N'warmTouch', N'SP17090420', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Calentador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Calentador', N'Covidien', N'warmTouch', N'SP17090401', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Calentador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'Calentador', N'Covidien', N'warmTouch', N'SP17090627', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'QX'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Calentador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ESTERILIZADOR DE VAPOR', N'SISTEMATIC', N'SISTEM2020', N'ES067-19', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'CEYE'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Esterilizador De Vapor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'CEYE', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ESTERILIZADOR DE VAPOR', N'SISTEMATIC', N'SISTEM2021', N'ES074-19', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'CEYE'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Esterilizador De Vapor'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'CEYE', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'UNIDAD DE PLASMA', N'LAOKEN', N'LKMJG-50', N'150109072010', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'CEYE'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Unidad De Plasma'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'CEYE', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'ESTERILIZADOR  DE VAPOR AUTOGENERADO', N'THERLAB', N'TE-A22FPS', N'190102', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'CEYE'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Esterilizador  De Vapor Autogenerado'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'CEYE', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'CRANEOTOMO', N'AESCULAP AG', NULL, N'2396', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'CEYE'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Craneotomo'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'SIERRA PARA YESO', N'CGOLDENVALL', N'AM AESCULAP-PLATZ', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'CEYE'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Sierra Para Yeso'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904040', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'ESCOLARES', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'IM70', N'360080-M20603640023', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'LACTANTES', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M9', N'101245-K12500210001-01', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'LACTANTES', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068-M18613260017', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068-M18613260013', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068-M18613260016', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ENLACE', N'93300', N'3300203976', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068-M18613260018', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068-M18613260012', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904041', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'UCIN', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904034', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX RECU C-2', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904044', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX RECU C-3', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904037', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX RECU C-1', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904327', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX RECU C-2', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'PM2000 A PRO', N'301448-M1812550025', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX 4', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'36068-M19207140005', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'PREOP C-1', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904042', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QXCUARTO MEDIC', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'2000 A PRO', N'301448-M18112550048', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX 2', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ZEGEN', N'ZGN-7D', NULL, (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'PM2000 XL  PRO', N'Y98002-A3J000234', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX RECU C-5', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'PM2000 XL PRO', N'498001-A3J000142', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX RECU C-4', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904038', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'URGENCIAS ADUL', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904039', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'PASA A CLINICA CATETER', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904036', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'URGENCIAS PED', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN9394035', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'TERAPIA INT ADUL', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN9394043', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'TERAPIA INT ADUL', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904031', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'TERAPIA INT ADUL', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904030', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'TERAPIA INT ADUL', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN PATRON', N'M80', N'360068-M18613260001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'TERAPIA INT ADUL', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN PATRON', N'M80', N'360068-M19207140001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'TERAPIA INT ADUL', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'S/D', N'ADV-140390', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'RESGUARDADO TOCO', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'PM2000XLPRO', N'Y98002-A3J000325', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'RECUP C-1', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'93904032', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'LABOR PARTO C-1', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068-M19207140007', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'EXP SALA 1', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'CN93904033', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'LABOR PARTO C-2', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068M-1861326002', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'LABOR C-3', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'EFFICIACM100', N'CN62639312', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'ESXP SALA 2 | MADERO', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'PHILIPS GOLWAY', N'G40E', N'cn93934039', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'CATETER', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'MINDRAY', N'UMEC12', N'KG98027085', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HOSPITALIZACION', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'MINDRAY', N'UMEC13', N'KG7A007701', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HOSPITALIZACION', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'CONTEC', N'CMS800', N'23011400043', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'DIRECCION', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'PM2000XLPRO', N'Y98001-A3J000135', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HOSPITALIZACION', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'DATASCOPE', N'TRIO', N'MC23891-19', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HEMODIALISIS', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'ADVANCED', N'PM2000APRO', N'301448-M18112550044', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'QX 1', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M80', N'360068-M19908060004', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HOSPITALIZACION', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'MINDRAY', N'UMEX12', N'KG-7A007700', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HOSP/PRIVADOS', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'DATASCOPE', N'TRIO M70', N'MC10109-E0', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'AMBULANCIA', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'DATASCOPE', N'TRIO M70', N'MC23889-19', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'ESCOLARES', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'EDAN', N'M9', N'101245-K12500210004-001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'TER.INT.NEO', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'DATASCOPE', N'TRIO', N'MC23895-19', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HOSPITALIZACION', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'CRC', N'VT-O', N'11001', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HOSPITALIZACION', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'MINDRAY', N'MRC-1000', N'AQ-8A117025', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), NULL, N'HOSP/PRIVADOS | TRANS MATERNO', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'MONITOR DE SIGNOS VITALES', N'GOLDWAY', N'UT4000', N'4PSBAIR-036', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Monitores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Monitor De Signos Vitales'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'De baja'), NULL, N'UCIN | BAJA ***', N'UCIN | BAJA ***';
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO NEONATAL', N'PURITAN BENET', N'840', N'3512152994', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'UCIN', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO NEONATAL', N'PURITAN BENET', N'840', N'3512160787', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), N'PEDIA', N'falta por verificar', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO NEONATAL', N'NEW PORT', N'E3600t', N'G1172010015', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), N'PEDIA', N'falla de microcontrolador', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO NEONATAL', N'NEW PORT', N'E3600t', N'G1172010016', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), N'PEDIA', N'falla de microcontrolador', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR VOLUMETRICO ADUL/PED', N'CARE FUSION', N'3100A', N'BJW01284', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Volumetrico Adul/Ped'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PEDIA', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR PEDIATRICO NEONATAL', N'NEW PORT', N'E3600t', N'G162110234', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Pediatrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), N'PEDIA', N'falla de software', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR PEDIATRICO NEONATAL', N'NEW PORT', N'E3600t', N'G172020250', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Pediatrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), N'PEDIA', N'falla de software', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR PEDIATRICO NEONATAL', N'AOMED', N'VG70', N'ZZT12464', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Pediatrico Neonatal'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PEDIA', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT12403', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PEDIA', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT12244', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PEDIA', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT9818', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PISO', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT12291', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PISO', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT12287', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PISO', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT9824', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PISO', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT2315', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'PISO', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT12361', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'URG', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT19877', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'URG', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT9877', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'UCIA', N'software', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'AOMED', N'VG70', N'ZZT9878', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'QX', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'HAMILTON', N'C2', N'8316', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), N'UCIA', N'falla de pantalla', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'HAMILTON', N'C3', N'8316', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), N'UCIA', N'falla de pantalla', NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR', N'MAGNAMED', N'REF-2018', N'5924', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'AMBULANCIA', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'MINDRAY', N'SV300', N'GB-98010538', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'UCIA', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'MINDRAY', N'SV300', N'GB-98010537', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'UCIA', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'MINDRAY', N'SV300', N'GB-98010536', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'UCIA', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'VERSAMED', N'GENERAL ELECTRIC', N'IV21057', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Operativo'), N'TOCO', NULL, NULL;
INSERT INTO dbo.bio_equipo (nombre, marca, modelo, numero_serie, area_id, categoria_id, estado_id, ubicacion, comentarios, motivo_baja)
SELECT N'VENTILADOR ADULTO-PEDIATRICO', N'NEW PORT', N'HTC', N'HT7P200793', (SELECT area_id FROM dbo.bio_area WHERE nombre = N'Ventiladores'), (SELECT categoria_id FROM dbo.bio_categoria_equipo WHERE nombre = N'Ventilador Adulto-Pediatrico'), (SELECT estado_id FROM dbo.bio_estado_equipo WHERE nombre = N'Fuera de servicio'), N'JEFATURA', N'Faltan accesorios', NULL;
GO

-- Un puñado de números de serie venían como celda numérica en el Excel
-- (pandas los volvió "210633420.0" en vez de "210633420"); se corrige aquí.
UPDATE dbo.bio_equipo SET numero_serie = LEFT(numero_serie, LEN(numero_serie)-2) WHERE numero_serie LIKE '%.0';
GO

PRINT '=== Datos importados: 268 equipos en 11 áreas ===';
GO