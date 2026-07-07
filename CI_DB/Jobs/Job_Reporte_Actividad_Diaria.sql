USE [msdb]
GO

-- Eliminar si existe (idempotencia)
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'JOB_Reporte_Actividad_Diaria')
    EXEC msdb.dbo.sp_delete_job @job_name = N'JOB_Reporte_Actividad_Diaria', @delete_unused_schedule = 1;
GO

EXEC msdb.dbo.sp_add_job
    @job_name         = N'JOB_Reporte_Actividad_Diaria',
    @description      = N'Genera diariamente el reporte de contactos y direcciones confirmados o agregados por asesor. Corre a la 01:00 AM procesando el dia anterior.',
    @category_name    = N'[Uncategorized (Local)]',
    @owner_login_name = N'sa',
    @enabled          = 1;
GO

EXEC msdb.dbo.sp_add_jobstep
    @job_name          = N'JOB_Reporte_Actividad_Diaria',
    @step_name         = N'Ejecutar SP_Reporte_Actividad_Diaria',
    @subsystem         = N'TSQL',
    @command           = N'EXEC DB_ODS.reportes.SP_Reporte_Actividad_Diaria;',
    @database_name     = N'DB_ODS',
    @on_success_action = 1,  -- Quit with success
    @on_fail_action    = 2;  -- Quit with failure
GO

-- Schedule: diario a la 01:00 AM
EXEC msdb.dbo.sp_add_schedule
    @schedule_name     = N'SCH_Reporte_Diario_01AM',
    @freq_type         = 4,       -- Daily
    @freq_interval     = 1,       -- Every 1 day
    @active_start_time = 10000;   -- 01:00:00 AM (formato HHMMSS)
GO

EXEC msdb.dbo.sp_attach_schedule
    @job_name      = N'JOB_Reporte_Actividad_Diaria',
    @schedule_name = N'SCH_Reporte_Diario_01AM';
GO

EXEC msdb.dbo.sp_add_jobserver
    @job_name    = N'JOB_Reporte_Actividad_Diaria',
    @server_name = N'(LOCAL)';
GO

-- Verificacion post-ejecucion
SELECT j.name AS job_name, j.enabled, s.name AS schedule_name, s.active_start_time
FROM msdb.dbo.sysjobs j
JOIN msdb.dbo.sysjobschedules js ON js.job_id = j.job_id
JOIN msdb.dbo.sysschedules s ON s.schedule_id = js.schedule_id
WHERE j.name = 'JOB_Reporte_Actividad_Diaria';
-- Resultado esperado: 1 fila con enabled = 1, active_start_time = 10000
