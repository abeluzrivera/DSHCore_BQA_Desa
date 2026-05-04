-- =====================================================
-- Script de Datos de Prueba para Dashboard de Clientes
-- =====================================================
-- Este script inserta clientes de ejemplo con sus contactos
-- para probar la funcionalidad del dashboard
-- =====================================================

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @FechaActual DATETIME2(7) = GETDATE();
    DECLARE @UsuarioSistema NVARCHAR(100) = 'SISTEMA_SEED';

    -- =====================================================
    -- INSERTAR CLIENTES DE PRUEBA
    -- =====================================================
    
    -- Cliente 1: Ana Rodríguez - Todos los contactos verificados
    IF NOT EXISTS (SELECT 1 FROM operativo.Tbl_Maest_Cliente WHERE Identificacion_Cliente = '1234567890')
    BEGIN
        INSERT INTO operativo.Tbl_Maest_Cliente 
            (Identificacion_Cliente, Nombre_Completo, Fecha_Creacion, Usuario_Creacion)
        VALUES 
            ('1234567890', 'Ana María Rodríguez López', @FechaActual, @UsuarioSistema);

        DECLARE @IdCliente1 BIGINT = SCOPE_IDENTITY();

        -- Contactos verificados
        INSERT INTO operativo.Tbl_Traz_Contacto_Cliente 
            (Id_Cliente, Tipo_Medio_Contacto, Valor_Contacto, Estado_Verificacion, 
             Fecha_Creacion, Usuario_Creacion, Esta_Eliminado)
        VALUES 
            (@IdCliente1, 'TEL_C', '022451234', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente1, 'EMAIL', 'ana.rodriguez@ejemplo.com', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente1, 'DIR_D', 'Av. Principal 123, Quito', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente1, 'CEL', '0987654321', 'VERIF', @FechaActual, @UsuarioSistema, 0);

        -- Información financiera
        INSERT INTO operativo.Tbl_Traz_Financiero_cliente 
            (Id_Cliente, Tipo_Contabilidad, Monto_Contable)
        VALUES 
            (@IdCliente1, 'ACT', 15000.00),
            (@IdCliente1, 'PAS', 5000.00);

        PRINT 'Cliente Ana María Rodríguez López insertado correctamente.';
    END

    -- Cliente 2: Carlos Méndez - Contactos mixtos (algunos pendientes)
    IF NOT EXISTS (SELECT 1 FROM operativo.Tbl_Maest_Cliente WHERE Identificacion_Cliente = '0987654321')
    BEGIN
        INSERT INTO operativo.Tbl_Maest_Cliente 
            (Identificacion_Cliente, Nombre_Completo, Fecha_Creacion, Usuario_Creacion)
        VALUES 
            ('0987654321', 'Carlos Eduardo Méndez García', @FechaActual, @UsuarioSistema);

        DECLARE @IdCliente2 BIGINT = SCOPE_IDENTITY();

        -- Contactos mixtos
        INSERT INTO operativo.Tbl_Traz_Contacto_Cliente 
            (Id_Cliente, Tipo_Medio_Contacto, Valor_Contacto, Estado_Verificacion, 
             Fecha_Creacion, Usuario_Creacion, Esta_Eliminado)
        VALUES 
            (@IdCliente2, 'TEL_C', '023456789', 'PEND', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente2, 'EMAIL', 'carlos.mendez@ejemplo.com', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente2, 'DIR_T', 'Edificio Empresarial Torre A, Piso 5', 'PEND', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente2, 'CEL', '0991234567', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente2, 'WAPP', '0991234567', 'PEND', @FechaActual, @UsuarioSistema, 0);

        INSERT INTO operativo.Tbl_Traz_Financiero_cliente 
            (Id_Cliente, Tipo_Contabilidad, Monto_Contable)
        VALUES 
            (@IdCliente2, 'ACT', 25000.00),
            (@IdCliente2, 'PAS', 8000.00);

        PRINT 'Cliente Carlos Eduardo Méndez García insertado correctamente.';
    END

    -- Cliente 3: Lucía Pérez - Todos los contactos verificados
    IF NOT EXISTS (SELECT 1 FROM operativo.Tbl_Maest_Cliente WHERE Identificacion_Cliente = '1122334455')
    BEGIN
        INSERT INTO operativo.Tbl_Maest_Cliente 
            (Identificacion_Cliente, Nombre_Completo, Fecha_Creacion, Usuario_Creacion)
        VALUES 
            ('1122334455', 'Lucía Patricia Pérez Morales', @FechaActual, @UsuarioSistema);

        DECLARE @IdCliente3 BIGINT = SCOPE_IDENTITY();

        INSERT INTO operativo.Tbl_Traz_Contacto_Cliente 
            (Id_Cliente, Tipo_Medio_Contacto, Valor_Contacto, Estado_Verificacion, 
             Fecha_Creacion, Usuario_Creacion, Esta_Eliminado)
        VALUES 
            (@IdCliente3, 'TEL_C', '024567890', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente3, 'EMAIL', 'lucia.perez@ejemplo.com', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente3, 'DIR_D', 'Residencial Las Lomas, Torre 2, Apto 504', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente3, 'CEL', '0998765432', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente3, 'LINK', 'https://linkedin.com/in/luciaperez', 'VERIF', @FechaActual, @UsuarioSistema, 0);

        INSERT INTO operativo.Tbl_Traz_Financiero_cliente 
            (Id_Cliente, Tipo_Contabilidad, Monto_Contable)
        VALUES 
            (@IdCliente3, 'ACT', 35000.00),
            (@IdCliente3, 'PAS', 12000.00);

        PRINT 'Cliente Lucía Patricia Pérez Morales insertado correctamente.';
    END

    -- Cliente 4: Miguel Torres - Con contactos en ERROR
    IF NOT EXISTS (SELECT 1 FROM operativo.Tbl_Maest_Cliente WHERE Identificacion_Cliente = '5544332211')
    BEGIN
        INSERT INTO operativo.Tbl_Maest_Cliente 
            (Identificacion_Cliente, Nombre_Completo, Fecha_Creacion, Usuario_Creacion)
        VALUES 
            ('5544332211', 'Miguel Ángel Torres Sánchez', @FechaActual, @UsuarioSistema);

        DECLARE @IdCliente4 BIGINT = SCOPE_IDENTITY();

        INSERT INTO operativo.Tbl_Traz_Contacto_Cliente 
            (Id_Cliente, Tipo_Medio_Contacto, Valor_Contacto, Estado_Verificacion, 
             Fecha_Creacion, Usuario_Creacion, Esta_Eliminado)
        VALUES 
            (@IdCliente4, 'TEL_C', '025678901', 'ERROR', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente4, 'EMAIL', 'miguel.torres@dominioinvalido', 'ERROR', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente4, 'DIR_D', 'Dirección pendiente de actualizar', 'PEND', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente4, 'CEL', '099123', 'ERROR', @FechaActual, @UsuarioSistema, 0);

        INSERT INTO operativo.Tbl_Traz_Financiero_cliente 
            (Id_Cliente, Tipo_Contabilidad, Monto_Contable)
        VALUES 
            (@IdCliente4, 'ACT', 8000.00),
            (@IdCliente4, 'PAS', 15000.00);

        PRINT 'Cliente Miguel Ángel Torres Sánchez insertado correctamente.';
    END

    -- Cliente 5: María González - Sin contactos
    IF NOT EXISTS (SELECT 1 FROM operativo.Tbl_Maest_Cliente WHERE Identificacion_Cliente = '9988776655')
    BEGIN
        INSERT INTO operativo.Tbl_Maest_Cliente 
            (Identificacion_Cliente, Nombre_Completo, Fecha_Creacion, Usuario_Creacion)
        VALUES 
            ('9988776655', 'María Isabel González Vera', @FechaActual, @UsuarioSistema);

        DECLARE @IdCliente5 BIGINT = SCOPE_IDENTITY();

        INSERT INTO operativo.Tbl_Traz_Financiero_cliente 
            (Id_Cliente, Tipo_Contabilidad, Monto_Contable)
        VALUES 
            (@IdCliente5, 'ACT', 5000.00),
            (@IdCliente5, 'PAS', 2000.00);

        PRINT 'Cliente María Isabel González Vera insertado correctamente (sin contactos).';
    END

    -- Cliente 6: Jorge Ramírez - Contactos eliminados y activos
    IF NOT EXISTS (SELECT 1 FROM operativo.Tbl_Maest_Cliente WHERE Identificacion_Cliente = '6677889900')
    BEGIN
        INSERT INTO operativo.Tbl_Maest_Cliente 
            (Identificacion_Cliente, Nombre_Completo, Fecha_Creacion, Usuario_Creacion)
        VALUES 
            ('6677889900', 'Jorge Luis Ramírez Castro', @FechaActual, @UsuarioSistema);

        DECLARE @IdCliente6 BIGINT = SCOPE_IDENTITY();

        -- Contactos activos
        INSERT INTO operativo.Tbl_Traz_Contacto_Cliente 
            (Id_Cliente, Tipo_Medio_Contacto, Valor_Contacto, Estado_Verificacion, 
             Fecha_Creacion, Usuario_Creacion, Esta_Eliminado)
        VALUES 
            (@IdCliente6, 'EMAIL', 'jorge.ramirez@ejemplo.com', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            (@IdCliente6, 'CEL', '0997654321', 'VERIF', @FechaActual, @UsuarioSistema, 0),
            -- Contacto eliminado (no debe aparecer en dashboard)
            (@IdCliente6, 'TEL_C', '026789012', 'PEND', @FechaActual, @UsuarioSistema, 1),
            (@IdCliente6, 'DIR_D', 'Av. Amazonas 456', 'VERIF', @FechaActual, @UsuarioSistema, 0);

        INSERT INTO operativo.Tbl_Traz_Financiero_cliente 
            (Id_Cliente, Tipo_Contabilidad, Monto_Contable)
        VALUES 
            (@IdCliente6, 'ACT', 18000.00),
            (@IdCliente6, 'PAS', 7000.00);

        PRINT 'Cliente Jorge Luis Ramírez Castro insertado correctamente.';
    END

    COMMIT TRANSACTION;
    PRINT '========================================';
    PRINT 'Datos de prueba insertados exitosamente';
    PRINT '========================================';
    PRINT 'Total de clientes de prueba: 6';
    PRINT '- 2 con todos los contactos verificados';
    PRINT '- 2 con contactos pendientes';
    PRINT '- 1 con contactos en error';
    PRINT '- 1 sin contactos';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
        
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    PRINT 'ERROR: ' + @ErrorMessage;
    
    -- Re-lanzar el error para debugging
    THROW;
END CATCH;

GO

-- =====================================================
-- CONSULTA DE VERIFICACIÓN
-- =====================================================
SELECT 
    c.Identificacion_Cliente,
    c.Nombre_Completo,
    COUNT(ct.Id_Contacto_Cliente) as Total_Contactos,
    SUM(CASE WHEN ct.Estado_Verificacion = 'VERIF' THEN 1 ELSE 0 END) as Verificados,
    SUM(CASE WHEN ct.Estado_Verificacion = 'PEND' THEN 1 ELSE 0 END) as Pendientes,
    SUM(CASE WHEN ct.Estado_Verificacion = 'ERROR' THEN 1 ELSE 0 END) as Con_Error,
    SUM(CASE WHEN ct.Esta_Eliminado = 1 THEN 1 ELSE 0 END) as Eliminados
FROM operativo.Tbl_Maest_Cliente c
LEFT JOIN operativo.Tbl_Traz_Contacto_Cliente ct ON c.Id_Cliente = ct.Id_Cliente
GROUP BY c.Identificacion_Cliente, c.Nombre_Completo
ORDER BY c.Nombre_Completo;
