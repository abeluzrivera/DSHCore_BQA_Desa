USE [msdb]
GO

-- Eliminar si existe (idempotencia)
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'JOB_Baseline_Carga_Inicial')
    EXEC msdb.dbo.sp_delete_job @job_name = N'JOB_Baseline_Carga_Inicial', @delete_unused_schedule = 1;
GO

EXEC msdb.dbo.sp_add_job
    @job_name         = N'JOB_Baseline_Carga_Inicial',
    @description      = N'Ejecucion unica para capturar la linea base del dia de migracion en DB_ODS. Ejecutar manualmente una sola vez post-despliegue.',
    @category_name    = N'[Uncategorized (Local)]',
    @owner_login_name = N'sa',
    @enabled          = 1;
GO

EXEC msdb.dbo.sp_add_jobstep
    @job_name          = N'JOB_Baseline_Carga_Inicial',
    @step_name         = N'Ejecutar SP_Baseline_Carga_Inicial',
    @subsystem         = N'TSQL',
    @command           = N'EXEC DB_ODS.reportes.SP_Baseline_Carga_Inicial;',
    @database_name     = N'DB_ODS',
    @on_success_action = 1,  -- Quit with success
    @on_fail_action    = 2;  -- Quit with failure
GO

EXEC msdb.dbo.sp_add_jobserver
    @job_name    = N'JOB_Baseline_Carga_Inicial',
    @server_name = N'(LOCAL)';
GO

-- Verificacion post-ejecucion
SELECT name, enabled, description
FROM msdb.dbo.sysjobs
WHERE name = 'JOB_Baseline_Carga_Inicial';
-- Resultado esperado: 1 fila con enabled = 1
