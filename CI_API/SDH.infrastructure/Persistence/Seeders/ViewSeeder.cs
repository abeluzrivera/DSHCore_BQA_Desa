using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Seeders
{
    public static class ViewSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
        {
            logger.LogInformation("Creando o actualizando vistas de base de datos...");

            await context.Database.ExecuteSqlRawAsync(@"
                CREATE OR ALTER VIEW [parametro].[Vw_Cat_Detalle_General]
                AS
                SELECT
                    i.Id_Item_Catalogo,
                    g.Id_Grupo_Catalogo,
                    g.Nombre_Grupo,
                    i.Codigo_Valor,
                    i.Texto_Visual,
                    i.Orden_Visual,
                    i.Esta_Activo,
                    g.Es_Sistema
                FROM parametro.Tbl_Cat_Item  AS i
                JOIN parametro.Tbl_Cat_Grupo AS g ON g.Id_Grupo_Catalogo = i.Id_Grupo_Catalogo;
            ");
            
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE OR ALTER VIEW [operativo].[vw_Clientes_Contactos_Detalle] 
                AS
                SELECT 
                    Mae.Identificacion_Cliente AS [Identificacion Cliente], 
                    Mae.Nombre_Completo AS [Nombre Completo], 
                    Cel.Valor_Contacto AS Celular, 
                    Cel.Source AS Fuente_Celular,
                    Cor.Valor_Contacto AS Correo,
                    Cor.Source AS Fuente_Correo,
                    Con.Valor_Contacto AS Telefono_Convencional,
                    Con.Source AS Fuente_Telefono_Convencional,
                    Dir_H.Direccion_Completa AS Direccion_Domicilio,
                    Dir_H.Source_Direccion AS Fuente_Direccion_Domicilio,
                    Dir_T.Direccion_Completa AS Direccion_Trabajo,
                    Dir_H.Source_Direccion AS Fuente_Direccion_Trabajo
                FROM operativo.Tbl_Maest_Cliente AS Mae
                LEFT JOIN operativo.Tbl_Contacto_Cliente AS Cel 
                    ON Mae.Id_Cliente = Cel.Id_Cliente AND Cel.Id_Tipo_Contacto = 101
                LEFT JOIN operativo.Tbl_Contacto_Cliente AS Cor
                    ON Mae.Id_Cliente = Cor.Id_Cliente AND Cor.Id_Tipo_Contacto = 102
                LEFT JOIN operativo.Tbl_Contacto_Cliente AS Con
                    ON Mae.Id_Cliente = Con.Id_Cliente AND Con.Id_Tipo_Contacto = 103
                LEFT JOIN operativo.Tbl_Direccion_Cliente AS Dir_H
                    ON Mae.Id_Cliente = Dir_H.Id_Cliente AND Dir_H.Id_Tipo_Direccion = 105
                LEFT JOIN operativo.Tbl_Direccion_Cliente AS Dir_T
                    ON Mae.Id_Cliente = Dir_T.Id_Cliente AND Dir_T.Id_Tipo_Direccion = 106;
            ");

            logger.LogInformation("Vistas sincronizadas correctamente.");
        }
    }
}
