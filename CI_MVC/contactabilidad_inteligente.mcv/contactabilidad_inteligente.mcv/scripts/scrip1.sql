-- Created by GitHub Copilot in SSMS - review carefully before executing
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedimiento: P_GEN_MAT_ESTADO_CTA_DIGITAL
-- Descripción: Genera matriculación de estados de cuenta digitales
-- Mejoras aplicadas: 
--   - Eliminada inserción duplicada de tb_emails_clte
--   - Cambiado ANSI-89 JOIN a INNER JOIN explícito
--   - Eliminada validación ISNUMERIC duplicada
--   - Optimizados UPDATEs con MERGE
--   - Agregado manejo de errores con TRY...CATCH
--   - Agregado control de transacciones
-- =============================================
ALTER PROCEDURE [dbo].[P_GEN_MAT_ESTADO_CTA_DIGITAL]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- ========================================
        -- PASO 1: Limpiar tablas temporales
        -- ========================================
        TRUNCATE TABLE TB_ECD_DET_ACEPTACION_ESTADO_CUENTA_TMP;
        TRUNCATE TABLE TB_ECD_MAE_ACEPTACION_ESTADO_CUENTA_TEMP;
        TRUNCATE TABLE SVISF021;
        TRUNCATE TABLE tb_cltes_tc_estad_cta;
        TRUNCATE TABLE tb_cltes_tc_estad_cta_final;
        TRUNCATE TABLE tb_emails_clte;
        TRUNCATE TABLE tb_clte_emal_siaf;
        TRUNCATE TABLE TB_MAE_CLIENTES_ETL;
        TRUNCATE TABLE svisf117;
        TRUNCATE TABLE TB_ESTADOS_CTA_X_MATRICULAR;
        
        -- ========================================
        -- PASO 2: Cargar datos desde linked servers
        -- ========================================
        
        -- Cargar svisf117
        INSERT INTO svisf117
        SELECT *
        FROM LINK_91.DB_STAGING.DBO.svisf117;
        
        -- Cargar aceptaciones de estado cuenta (maestro)
        INSERT INTO TB_ECD_MAE_ACEPTACION_ESTADO_CUENTA_TEMP
        SELECT
            AceptacionEstadoCuentaId,
            NumeroAceptacion,
            TipoIdentificacion,
            Identificacion,
            RazonSocial,
            Todo,
            Email,
            Direccion,
            CodigoOficina,
            Oficina,
            EnvioMail,
            GETDATE() AS FECHA_CARGA
        FROM LINK_38.BD_EstadoCuentaDigital.DBO.TB_ECD_MAE_ACEPTACION_ESTADO_CUENTA;
        
        -- Cargar aceptaciones de estado cuenta (detalle)
        INSERT INTO TB_ECD_DET_ACEPTACION_ESTADO_CUENTA_TMP
        SELECT
            DetalleCuentaId,
            AceptacionEstadoCuentaId,
            CuentaCredimatic,
            CodigoCuenta,
            Cuenta,
            DescripcionProducto,
            TipoProducto,
            Aceptacion,
            Email,
            GETDATE() AS FECHA_CARGA
        FROM LINK_38.BD_EstadoCuentaDigital.DBO.TB_ECD_DET_ACEPTACION_ESTADO_CUENTA;
        
        -- Cargar SVISF021
        INSERT INTO SVISF021
        SELECT *
        FROM LINK_91.DB_STAGING.DBO.SVISF021;
        
        -- Cargar maestro de clientes
        INSERT INTO TB_MAE_CLIENTES_ETL
        SELECT *
        FROM LINK_91.db_staging.dbo.TB_MAE_CLIENTES_ETL;
        
        -- ========================================
        -- PASO 3: Procesar clientes de tarjetas de crédito
        -- ========================================
        INSERT INTO tb_cltes_tc_estad_cta
        SELECT 
            MAIDNN AS NUMERO_ID, 
            107 AS CODIGO_OFICINA,
            'Suc_Guayaquil' AS OFICINA, 
            'CARGA_MASIVA' AS usuario, 
            'oficina' AS CANAL, 
            C.MAPTOS AS CUENTA_CREDIMATIC,
            C.MABIN, 
            C.MATARJ, 
            (CAST(C.MABIN AS VARCHAR(10)) + DBO.LPAD(CAST(C.MATARJ AS VARCHAR(10)), 10, '0')) AS CUENTA
        FROM svisf021 C
        WHERE mainap = 'P'
            AND MACURO > 0
            AND NOT EXISTS (
                SELECT 1
                FROM TB_ECD_MAE_ACEPTACION_ESTADO_CUENTA_TEMP X
                INNER JOIN TB_ECD_DET_ACEPTACION_ESTADO_CUENTA_TMP Y 
                    ON X.AceptacionEstadoCuentaId = Y.AceptacionEstadoCuentaId
                WHERE ISNUMERIC(X.Identificacion) = 1
                    AND CAST(X.Identificacion AS DECIMAL(18, 0)) = C.MAIDNN
            );
        
        -- ========================================
        -- PASO 4: Consolidar con datos de clientes
        -- ========================================
        INSERT INTO tb_cltes_tc_estad_cta_final
        SELECT 
            y.tipo_id, 
            y.nombre_cliente, 
            x.NUMERO_ID, 
            x.CODIGO_OFICINA, 
            x.OFICINA, 
            x.usuario, 
            x.CANAL,
            x.CUENTA_CREDIMATIC, 
            x.MABIN, 
            x.MATARJ, 
            x.CUENTA, 
            NULL AS email  -- Inicializar en NULL
        FROM tb_cltes_tc_estad_cta x
        INNER JOIN TB_MAE_CLIENTES_ETL y 
            ON y.numero_id = x.numero_id;
        
        -- ========================================
        -- PASO 5: Actualizar emails desde tb_emails_clte
        -- ========================================
        
        -- Cargar emails (UNA SOLA VEZ - duplicado eliminado)
        INSERT INTO tb_emails_clte
        SELECT *
        FROM LINK_91.DB_STAGING.DBO.tb_emails_clte;
        
        -- Actualizar emails usando MERGE para mejor performance
        MERGE INTO tb_cltes_tc_estad_cta_final AS target
        USING (
            SELECT 
                numero_id,
                RTRIM(LTRIM(email_1)) AS email_1,
                ROW_NUMBER() OVER (PARTITION BY numero_id ORDER BY consecutivo ASC) AS rn
            FROM tb_emails_clte
        ) AS source
        ON target.numero_id = source.numero_id AND source.rn = 1
        WHEN MATCHED THEN
            UPDATE SET target.email = source.email_1;
        
        -- ========================================
        -- PASO 6: Actualizar emails desde tb_clte_emal_siaf (para NULLs)
        -- ========================================
        
        -- Cargar emails de SIAF
        INSERT INTO tb_clte_emal_siaf
        SELECT *
        FROM LINK_91.DB_STAGING.DBO.tb_clte_emal_siaf;
        
        -- Actualizar emails NULL usando MERGE
        MERGE INTO tb_cltes_tc_estad_cta_final AS target
        USING (
            SELECT 
                numero_id,
                RTRIM(LTRIM(email_1)) AS email_1,
                ROW_NUMBER() OVER (PARTITION BY numero_id ORDER BY consecutivo ASC) AS rn
            FROM tb_clte_emal_siaf
        ) AS source
        ON target.numero_id = source.numero_id 
            AND source.rn = 1 
            AND target.email IS NULL
        WHEN MATCHED THEN
            UPDATE SET target.email = source.email_1;
        
        -- ========================================
        -- PASO 7: Generar tabla final de matriculación
        -- ========================================
        INSERT INTO TB_ESTADOS_CTA_X_MATRICULAR
        SELECT 
            tipo_id,
            CASE 
                WHEN TIPO_ID = 'N' THEN DBO.LPAD(CAST(NUMERO_ID AS VARCHAR(10)), 10, '0')
                WHEN TIPO_ID = 'R' THEN DBO.LPAD(CAST(NUMERO_ID AS VARCHAR(13)), 13, '0')
                ELSE DBO.LPAD(CAST(NUMERO_ID AS VARCHAR(13)), 10, '0')
            END AS NUMERO_ID_DOS,
            nombre_cliente, 
            email, 
            CUENTA_CREDIMATIC, 
            CUENTA, 
            Y.DESLAR, 
            CODIGO_OFICINA, 
            OFICINA,
            usuario, 
            CANAL
        FROM tb_cltes_tc_estad_cta_final X
        INNER JOIN svisf117 Y 
            ON Y.CODIGO = X.MABIN
        WHERE DBO.f_valida_email_cliente(email) = 1;
        
        -- ========================================
        -- PASO 8: Retornar resultado final
        -- ========================================
        SELECT 
            tipo_id AS TIPO_ID, 
            NUMERO_ID_DOS AS NUMERO_ID, 
            nombre_cliente AS NOMBRE_CLIENTE, 
            email AS EMAIL,
            CUENTA_CREDIMATIC, 
            CUENTA, 
            DESLAR AS DESCRIPCIONPRODUCTO, 
            CODIGO_OFICINA, 
            OFICINA, 
            USUARIO, 
            CANAL
        FROM TB_ESTADOS_CTA_X_MATRICULAR;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();
        
        -- Registrar error (opcional - ajustar según necesidad)
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
        
    END CATCH;
    
END
GO