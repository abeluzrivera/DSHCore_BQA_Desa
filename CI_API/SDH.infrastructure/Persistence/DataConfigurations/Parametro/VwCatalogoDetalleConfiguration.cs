using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Infrastructure.Persistence.ReadModels.Parametro;

namespace SDH.Infrastructure.Persistence.DataConfigurations.Parametro
{
    public class VwCatalogoDetalleConfiguration : IEntityTypeConfiguration<VwCatalogoDetalle>
    {
        public void Configure(EntityTypeBuilder<VwCatalogoDetalle> builder)
        {
            // 1. Especificar que es una Vista y no tiene Llave Primaria
            builder.HasNoKey();
            builder.ToView("Vw_Cat_Detalle_General", "parametro");

            // 2. Mapeo de Columnas
            builder.Property(v => v.IdItemCatalogo).HasColumnName("Id_Item_Catalogo");
            builder.Property(v => v.IdGrupoCatalogo).HasColumnName("Id_Grupo_Catalogo");
            builder.Property(v => v.NombreGrupo).HasColumnName("Nombre_Grupo");
            builder.Property(v => v.CodigoValor).HasColumnName("Codigo_Valor");
            builder.Property(v => v.TextoVisual).HasColumnName("Texto_Visual");
            builder.Property(v => v.OrdenVisual).HasColumnName("Orden_Visual");
            builder.Property(v => v.EstaActivo).HasColumnName("Esta_Activo");
            builder.Property(v => v.EsSistema).HasColumnName("Es_Sistema");
        }
    }
}