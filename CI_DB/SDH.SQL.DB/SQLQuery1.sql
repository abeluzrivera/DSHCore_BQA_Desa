SELECT 
      -- =========================================
      -- DATOS DEL CLIENTE (Maestro)
      -- =========================================
      C.[Id_Cliente]
      ,C.[Identificacion_Cliente]
      ,C.[Nombre_Completo]
      ,C.[Esta_Verificado]
      ,C.[Fecha_Creacion]       AS Fecha_Creacion_Cliente
      ,C.[Usuario_Creacion]     AS Usuario_Creacion_Cliente
      ,C.[Fecha_Modificacion]   AS Fecha_Mod_Cliente
      ,C.[Usuario_Modificacion] AS Usuario_Mod_Cliente

      -- =========================================
      -- DATOS DE CONTACTO (Trazabilidad)
      -- =========================================
      ,CO.[Id_Contacto_Cliente]
      ,CO.[Tipo_Medio_Contacto]
      ,CO.[Valor_Contacto]
      ,CO.[Estado_Verificacion] AS Estado_Verif_Contacto
      ,CO.[Id_Usuario_Verificador]
      ,CO.[Terminal_Modificacion]
      ,CO.[Esta_Eliminado]      AS Contacto_Eliminado
      ,CO.[Source]              AS Source_Contacto
      ,CO.[Fecha_Creacion]      AS Fecha_Creacion_Contacto
      ,CO.[Usuario_Creacion]    AS Usuario_Creacion_Contacto
      ,CO.[Fecha_Modificacion]  AS Fecha_Mod_Contacto
      ,CO.[Usuario_Modificacion] AS Usuario_Mod_Contacto

      -- =========================================
      -- DATOS DE DIRECCIÓN (Trazabilidad)
      -- =========================================
      ,D.[Id_Direccion_Cliente]
      ,D.[Tipo_Direccion]
      ,D.[Direccion_Completa]
      ,D.[Ciudad]
      ,D.[Provincia]
      ,D.[Codigo_Postal]
      ,D.[Pais]
      ,D.[Codigo_Pais]
      ,D.[Codigo_Ciudad]
      ,D.[Codigo_Provincia]
      ,D.[Latitud]
      ,D.[Longitud]
      ,D.[Es_Principal]
      ,D.[Source_Direccion]
      ,D.[Estado_Verificacion]  AS Estado_Verif_Direccion
      ,D.[Fecha_Creacion]       AS Fecha_Creacion_Direccion
      ,D.[Usuario_Creacion]     AS Usuario_Creacion_Direccion

  FROM [DB_ODS].[operativo].[Tbl_Maest_Cliente] C
  
  -- Unimos los contactos correspondientes al cliente
  LEFT JOIN [DB_ODS].[operativo].[Tbl_Traz_Contacto_Cliente] CO 
         ON C.[Id_Cliente] = CO.[Id_Cliente]
         
  -- Unimos las direcciones correspondientes al cliente
  LEFT JOIN [DB_ODS].[operativo].[Tbl_Traz_Direccion_Cliente] D 
         ON C.[Id_Cliente] = D.[Id_Cliente]

  -- Filtramos por la identificación solicitada
  WHERE C.[Identificacion_Cliente] = '916206840'