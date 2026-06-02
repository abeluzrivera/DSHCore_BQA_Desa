SELECT *
  FROM [operativo].[Tbl_Direccion_Cliente]
  WHERE Id_Cliente IN (
      SELECT Id_Cliente 
      FROM [operativo].[Tbl_Maest_Cliente] 
      WHERE Identificacion_Cliente = '921157301'
  );