DECLARE @Identificacion_Cliente VARCHAR(50) = '921157301';

SELECT  cc.Id_Tipo_Contacto, cc.Valor_Contacto, cc.Id_Estado_Verificacion, cc.Usuario_Verificador, cc.Fecha_Verificacion
  FROM [operativo].[Tbl_Contacto_Cliente] cc
  WHERE Id_Cliente IN (
      SELECT Id_Cliente 
      FROM [operativo].[Tbl_Maest_Cliente] 
      WHERE Identificacion_Cliente = @identificacion_Cliente
  );

  SELECT  cc.Id_Tipo_Direccion, cc.Direccion_Completa, cc.Estado_Verificacion, cc.Usuario_Verificador, cc.Fecha_Verificacion
  FROM [operativo].Tbl_Direccion_Cliente cc
  WHERE Id_Cliente IN (
      SELECT Id_Cliente 
      FROM [operativo].[Tbl_Maest_Cliente] 
      WHERE Identificacion_Cliente = @identificacion_Cliente
  );

  SELECT TOP (1000) *
  FROM [operativo].[Tbl_Maest_Cliente]
  where Identificacion_Cliente = @identificacion_Cliente
