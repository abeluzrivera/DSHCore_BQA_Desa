SELECT TOP (1000) [Id_Cliente]
      ,[Identificacion_Cliente]
      ,[Nombre_Completo]
      ,[Fecha_Creacion]
      ,[Usuario_Creacion]
      ,[Fecha_Modificacion]
      ,[Usuario_Modificacion]
      ,[Esta_Verificado]
  FROM [DB_ODS].[operativo].[Tbl_Maest_Cliente]
  where Identificacion_Cliente = '2106789012'

  select count(*) 
    FROM [DB_ODS].[operativo].[Tbl_Maest_Cliente]