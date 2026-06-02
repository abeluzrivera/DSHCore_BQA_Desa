using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Domain.Entities.Parametro;

namespace SDH.infrastructure.Persistence.DataConfigurations.Parametro
{
    /// <summary>
    /// Configuración de EF Core para la entidad ItemCatalogo - Solo configuraciones no soportadas por Data Annotations
    /// </summary>
    public class ItemCatalogoConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            // 1. Tabla y Esquema
            builder.ToTable("Tbl_Cat_Item", "parametro");

            // 2. Clave Primaria
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id)
                   .HasColumnName("Id_Item_Catalogo")
                   .ValueGeneratedNever();

            // Foreign Key Property
            builder.Property(i => i.GroupId)
                   .HasColumnName("Id_Grupo_Catalogo")
                   .IsRequired();

            // 3. Propiedades Escalares
            builder.Property(i => i.ValueCode)
                   .HasColumnName("Codigo_Valor")
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(i => i.DisplayName)
                   .HasColumnName("Texto_Visual")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(i => i.DisplayOrder)
                   .HasColumnName("Orden_Visual")
                   .IsRequired();

            builder.Property(i => i.IsActive)
                   .HasColumnName("Esta_Activo")
                   .HasDefaultValue(true) // Configurado por defecto a true a nivel BD
                   .IsRequired();

            // 4. Auditoría
            builder.Property(i => i.CreatedAt)
                   .HasColumnName("Fecha_Creacion")
                   .HasColumnType("datetime2(3)")
                   .HasDefaultValueSql("GETDATE()") // Consistencia con tu base de datos
                   .IsRequired();

            builder.Property(i => i.CreatedBy)
                   .HasColumnName("Usuario_Creacion")
                   .HasMaxLength(50);

            builder.Property(i => i.LastModifiedAt)
                   .HasColumnName("Fecha_Modificacion")
                   .HasColumnType("datetime2(3)");

            builder.Property(i => i.LastModifiedBy)
                   .HasColumnName("Usuario_Modificacion")
                   .HasMaxLength(50);

            // ──────────────────────────────────────────────────
            // 6. Índices
            // ──────────────────────────────────────────────────

            // Índice único compuesto para evitar códigos duplicados en un mismo grupo
            builder.HasIndex(i => new { i.GroupId, i.ValueCode })
                   .IsUnique()
                   .HasDatabaseName("UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor");

            // Índice optimizado para búsquedas de la aplicación (Performance)
            builder.HasIndex(i => new { i.GroupId, i.IsActive })
                   .HasDatabaseName("IX_ItemCatalogo_Buscador")
                   .IncludeProperties(i => new { i.ValueCode, i.DisplayName, i.DisplayOrder });

        }
    }
}
