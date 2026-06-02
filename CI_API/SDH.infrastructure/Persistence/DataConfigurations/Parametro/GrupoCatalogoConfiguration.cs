using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Domain.Entities.Parametro;

namespace SDH.infrastructure.Persistence.DataConfigurations.Parametro
{
    /// <summary>
    /// Configuración de EF Core para la entidad GrupoCatalogo - Solo configuraciones no soportadas por Data Annotations
    /// </summary>
    public class GrupoCatalogoConfiguration : IEntityTypeConfiguration<CatalogGroup>
    {
        public void Configure(EntityTypeBuilder<CatalogGroup> builder)
        {

            // 1. Tabla y Esquema
            builder.ToTable("Tbl_Cat_Grupo", "parametro");

            // 2. Clave Primaria
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Id)
                   .HasColumnName("Id_Grupo_Catalogo")
                   .ValueGeneratedOnAdd();

            // 3. Propiedades Escalares
            builder.Property(g => g.GroupName)
                   .HasColumnName("Nombre_Grupo")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(g => g.Description)
                   .HasColumnName("Descripcion")
                   .HasMaxLength(250);

            // VALOR POR DEFECTO: EsSistema
            builder.Property(g => g.IsSystem)
                   .HasColumnName("Es_Sistema")
                   .HasDefaultValue(false)
                   .IsRequired();

            // 4. Auditoría
            // VALOR POR DEFECTO SQL: FechaCreacion
            builder.Property(g => g.CreatedAt)
                   .HasColumnName("Fecha_Creacion")
                   .HasColumnType("datetime2(3)")
                   .HasDefaultValueSql("GETDATE()") // Integrado de tu código
                   .IsRequired();

            builder.Property(g => g.CreatedBy)
                   .HasColumnName("Usuario_Creacion")
                   .HasMaxLength(50);

            builder.Property(g => g.LastModifiedAt)
                   .HasColumnName("Fecha_Modificacion")
                   .HasColumnType("datetime2(3)");

            builder.Property(g => g.LastModifiedBy)
                   .HasColumnName("Usuario_Modificacion")
                   .HasMaxLength(50);

            // 5. Configuración de Relaciones y Backing Fields

            // Acceso por campo para encapsulamiento
            builder.Metadata.FindNavigation(nameof(CatalogGroup.Items))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);

            // RELACIÓN 1:N
            builder.HasMany(g => g.Items)
                   .WithOne() // El hijo (ItemCatalogo) no tiene propiedad de navegación de regreso
                   .HasForeignKey(i => i.GroupId)
                   .OnDelete(DeleteBehavior.Restrict) // Excelente restricción de seguridad
                   .HasConstraintName("FK_Grupo_Item");

            // 6. Índices
            // ÍNDICE ÚNICO
            builder.HasIndex(g => g.GroupName)
                   .IsUnique()
                   .HasDatabaseName("UQ_GrupoCatalogo_NombreGrupo");
        }
    }
}
