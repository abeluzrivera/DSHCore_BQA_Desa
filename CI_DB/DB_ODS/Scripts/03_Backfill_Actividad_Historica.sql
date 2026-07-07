USE [DB_ODS]
GO

-- =============================================
-- Backfill de actividad historica
-- Procesa todos los dias desde el dia siguiente
-- a la migracion hasta ayer.
--
-- Ejecutar UNA SOLA VEZ, despues de:
--   1. 02_Schema_Tablas_Reportes.sql
--   2. SP_Baseline_Carga_Inicial.sql  (y haberlo ejecutado)
--   3. SP_Reporte_Actividad_Diaria.sql
-- =============================================

DECLARE @FechaInicio date;
DECLARE @FechaFin    date;
DECLARE @FechaActual date;

-- Dia siguiente a la migracion
SET @FechaInicio = '2026-06-25';

-- Hasta ayer (el job nocturno tomara desde hoy en adelante)
SET @FechaFin = CAST(DATEADD(day, -1, GETDATE()) AS date);

SET @FechaActual = @FechaInicio;

PRINT 'Iniciando backfill desde ' + CONVERT(varchar, @FechaInicio) + ' hasta ' + CONVERT(varchar, @FechaFin);

WHILE @FechaActual <= @FechaFin
BEGIN
    PRINT 'Procesando: ' + CONVERT(varchar, @FechaActual);

    EXEC reportes.SP_Reporte_Actividad_Diaria @FechaReporte = @FechaActual;

    SET @FechaActual = DATEADD(day, 1, @FechaActual);
END

PRINT 'Backfill completado.';

-- Verificacion
SELECT
    Fecha_Reporte,
    SUM(Total_Registros) AS Total_Actividad
FROM reportes.Tbl_Sum_Actividad_Diaria
GROUP BY Fecha_Reporte
ORDER BY Fecha_Reporte;
-- Resultado esperado: una fila por cada dia entre 2026-06-25 y ayer
-- Los dias sin actividad no apareceran (es correcto)
