using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Domain.Entities.Seguridad;

namespace SDH.infrastructure.Persistence.DataConfigurations.Seguridad
{
    /// <summary>
    /// Configuración de EF Core para la entidad Usuario - Solo configuraciones no soportadas por Data Annotations
    /// </summary>
    public class UsuarioConfiguration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {

            // 1. Tabla y Esquema
            builder.ToTable("Tbl_Maest_Usuario", "seguridad");

            // 2. Clave Primaria
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("Id_Usuario")
                   .ValueGeneratedOnAdd();

            // 3. Propiedades Escalares
            builder.Property(u => u.Username)
                   .HasColumnName("Codigo_Usuario")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(u => u.Email)
                   .HasColumnName("Email")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("Clave_Hash")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(u => u.FullName)
                   .HasColumnName("Nombre_Asesor")
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(u => u.SystemRole)
                   .HasColumnName("Rol_Sistema")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(u => u.IsActive)
                   .HasColumnName("Esta_Activo")
                   .IsRequired();

            builder.Property(u => u.IsDeleted)
                   .HasColumnName("Esta_Eliminado")
                   .IsRequired();

            builder.Property(u => u.IsLockedOut)
                   .HasColumnName("Esta_Bloqueado");

            builder.Property(u => u.LastLoginAt)
                   .HasColumnName("Fecha_Ultimo_Acceso")
                   .HasColumnType("datetime2(7)");

            // 4. Auditoría
            builder.Property(u => u.CreatedAt)
                   .HasColumnName("Fecha_Creacion")
                   .HasColumnType("datetime2(7)")
                   .IsRequired();

            builder.Property(u => u.CreatedBy)
                   .HasColumnName("Usuario_Creacion")
                   .HasMaxLength(50);

            builder.Property(u => u.LastModifiedAt)
                   .HasColumnName("Fecha_Modificacion")
                   .HasColumnType("datetime2(7)");

            builder.Property(u => u.LastModifiedBy)
                   .HasColumnName("Usuario_Modificacion")
                   .HasMaxLength(50);

            // 5. Índices Recomendados (Opcional, pero muy recomendado para seguridad)
            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_Usuario_Email");

            builder.HasIndex(u => u.Username)
                   .IsUnique()
                   .HasDatabaseName("IX_Usuario_CodigoUsuario");

            // Índices únicos (no soportado por Data Annotations)
            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IX_Usuario_CodigoUsuario");

            // Configuración adicional para búsquedas
            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Usuario_EstaActivo");
        }
    }
}
