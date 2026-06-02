using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Domain.Entities.Operative;
using SDH.Domain.Entities.Parametro;

namespace SDH.infrastructure.Persistence.DataConfigurations.Operativo
{
    /// <summary>
    /// Configuración de EF Core para la entidad MaestroClienteSQL - Solo configuraciones no soportadas por Data Annotations
    /// </summary>
    public class ClienteConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            // 1. Tabla y Esquema
            builder.ToTable("Tbl_Maest_Cliente", "operativo");

            // 2. Clave Primaria
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id)
                   .HasColumnName("Id_Cliente")
                   .ValueGeneratedOnAdd();

            // 3. Propiedades Escalares
            builder.Property(c => c.Identification)
                   .HasColumnName("Identificacion_Cliente")
                   .HasMaxLength(32)
                   .IsRequired();

            builder.Property(c => c.IdentificationType)
                   .HasColumnName("Id_Tipo_Identificacion")
                   .IsRequired();

            builder.HasOne<CatalogItem>()
                   .WithMany()
                   .HasForeignKey(c => c.IdentificationType)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Cliente_TipoIdentificacion");

            builder.Property(c => c.FullName)
                   .HasColumnName("Nombre_Completo")
                   .HasMaxLength(250); // Puede ser nulo por tu diseño

            builder.Property(c => c.IsVerified).HasColumnName("Esta_Verificado");
            builder.Property(c => c.IsApproved).HasColumnName("Esta_Aprobado");
            builder.Property(c => c.IsDeleted).HasColumnName("Esta_Eliminado");
            builder.Property(c => c.IsAnonymized).HasColumnName("Esta_Anonimizado");

            // Auditoría
            builder.Property(c => c.CreatedAt)
                   .HasColumnName("Fecha_Creacion")
                   .HasColumnType("datetime2(3)")
                   .IsRequired();

            builder.Property(c => c.CreatedBy)
                   .HasColumnName("Usuario_Creacion")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(c => c.LastModifiedAt)
                   .HasColumnName("Fecha_Modificacion")
                   .HasColumnType("datetime2(3)");

            builder.Property(c => c.LastModifiedBy)
                   .HasColumnName("Usuario_Modificacion")
                   .HasMaxLength(100);

            builder.Property(c => c.LegalExpirationDate)
                   .HasColumnName("Fecha_Expiracion_Legal")
                   .HasColumnType("Date");

            // Configurar el campo privado de respaldo para Financieros
            builder.HasMany(c => c.Financials)
                .WithOne() // <-- VACÍO: FinancieroCliente no necesita tener un 'public Cliente Cliente'
                .HasForeignKey(f => f.IdCliente) // <-- STRING: Le decimos a EF cómo se llama la columna en SQL
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Cliente_Financiero");

            // Configurar el campo privado de respaldo para Contactos
            builder.HasMany(c => c.Contacts)
                .WithOne() // <-- VACÍO
                .HasForeignKey(cc => cc.ClientId) // <-- STRING
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Cliente_Contacto");

            // Configurar el campo privado de respaldo para Oficializaciones
            builder.HasMany(c => c.Officializations)
                .WithOne() // <-- VACÍO
                .HasForeignKey(o => o.ClientId) // <-- STRING
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Cliente_Oficializacion");

            // Configurar el campo privado de respaldo para Direcciones
            builder.HasMany(c => c.Addresses)
                .WithOne() // <-- VACÍO
                .HasForeignKey(d => d.ClientId) // <-- STRING
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Cliente_Direccion");

            // Configurar acceso a los campos privados de las colecciones
            builder.Metadata.FindNavigation(nameof(Client.Financials))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata.FindNavigation(nameof(Client.Contacts))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata.FindNavigation(nameof(Client.Officializations))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata.FindNavigation(nameof(Client.Addresses))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            // indices para optimizacion de consultas
            builder.HasIndex(c => c.Identification)
                .IsUnique()
                .HasDatabaseName("IX_Cliente_Identificacion")
                .HasFilter("[Esta_Eliminado] = 0 AND [Esta_Anonimizado] = 0");

            builder.HasIndex(c => c.FullName)
                .HasDatabaseName("IX_Cliente_Nombre")
                .HasFilter("[Esta_Eliminado] = 0 AND [Esta_Anonimizado] = 0");
        }
    }
}
