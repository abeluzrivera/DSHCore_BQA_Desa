using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Domain.Entities.Operative;
using SDH.Domain.Entities.Parametro;

namespace SDH.infrastructure.Persistence.DataConfigurations.Operativo
{
    /// <summary>
    /// Configuración de EF Core para la entidad DireccionCliente
    /// </summary>
    public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddresses>
    {
        public void Configure(EntityTypeBuilder<CustomerAddresses> builder)
        {
            // 1. Tabla y Esquema
            builder.ToTable("Tbl_Direccion_Cliente", "operativo");

            // 2. Clave Primaria
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                   .HasColumnName("Id_Direccion_Cliente")
                   .ValueGeneratedOnAdd();

            builder.Property(c => c.ClientId)
                   .HasColumnName("Id_Cliente")
                   .IsRequired();

            // 3. Propiedades Escalares
            // Nota: La relación con IdCliente se configura como Shadow Property 
            // en ClienteConfiguration (HasMany... HasForeignKey("IdCliente")).

            builder.Property(d => d.AddressTypeId)
                   .HasColumnName("Id_Tipo_Direccion");

            builder.HasOne<CatalogItem>()
                   .WithMany()
                   .HasForeignKey(c => c.AddressTypeId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Direccion_Cliente_Tipo_Direccion");

            builder.Property(d => d.FullAddress)
                   .HasColumnName("Direccion_Completa")
                   .HasMaxLength(500);

            builder.Property(d => d.City)
                   .HasColumnName("Ciudad")
                   .HasMaxLength(100);

            builder.Property(d => d.Province)
                   .HasColumnName("Provincia")
                   .HasMaxLength(100);

            builder.Property(d => d.PostalCode)
                   .HasColumnName("Codigo_Postal")
                   .HasMaxLength(20);

            builder.Property(d => d.Country)
                   .HasColumnName("Pais")
                   .HasMaxLength(100);

            builder.Property(d => d.CountryCode)
                   .HasColumnName("Codigo_Pais")
                   .HasMaxLength(10);

            builder.Property(d => d.CityCode)
                   .HasColumnName("Codigo_Ciudad")
                   .HasMaxLength(10);

            builder.Property(d => d.ProvinceCode)
                   .HasColumnName("Codigo_Provincia")
                   .HasMaxLength(10);

            builder.Property(d => d.ParishCode)
                   .HasColumnName("Codigo_Parroquia")
                   .HasMaxLength(10);

            builder.Property(d => d.Parish)
                   .HasColumnName("Parroquia")
                   .HasMaxLength(100);

            builder.Property(d => d.Latitude)
                   .HasColumnName("Latitud")
                   .HasPrecision(18, 10)
                   .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);

            builder.Property(d => d.Longitude)
                   .HasColumnName("Longitud")
                   .HasPrecision(18, 10)
                   .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);

            builder.Property(d => d.IsPrimary)
                   .HasColumnName("Es_Principal");

            builder.Property(d => d.IsDeleted)
                   .HasColumnName("Esta_Eliminado");

            builder.Property(d => d.Source)
                   .HasColumnName("Source_Direccion")
                   .HasMaxLength(50);

            builder.Property(d => d.VerificationStatusId)
                   .HasColumnName("Estado_Verificacion")
                   .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);

            builder.HasOne<CatalogItem>()
                   .WithMany()
                   .HasForeignKey(c => c.VerificationStatusId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Direccion_Cliente_Estado_verificacion");

            // 4. Auditoría
            builder.Property(d => d.CreatedAt)
                   .HasColumnName("Fecha_Creacion")
                   .HasColumnType("datetime2(7)");

            builder.Property(d => d.CreatedBy)
                   .HasColumnName("Usuario_Creacion")
                   .HasMaxLength(50);

            builder.Property(d => d.LastModifiedAt)
                   .HasColumnName("Fecha_Modificacion")
                   .HasColumnType("datetime2(7)");

            builder.Property(d => d.LastModifiedBy)
                   .HasColumnName("Usuario_Modificacion")
                   .HasMaxLength(50);

            builder.Property(d => d.VerifiedBy)
                   .HasColumnName("Usuario_Verificador")
                   .HasMaxLength(50)
                   .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);

            builder.Property(d => d.ApprovedBy)
                   .HasColumnName("Usuario_Aprobador")
                   .HasMaxLength(50);

            builder.Property(d => d.VerifiedAt)
                   .HasColumnName("Fecha_Verificacion")
                   .HasColumnType("datetime2(7)")
                   .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);

            builder.Property(d => d.ApprovedAt)
                   .HasColumnName("Fecha_Aprobacion")
                   .HasColumnType("datetime2(7)");

            builder.Property(d => d.LopdpStatus)
                   .HasColumnName("estado_LOPDP")
                   .HasMaxLength(20);

            // 2. Índice compuesto (Shadow Property + Propiedad Real)
            builder.HasIndex(d => new { d.ClientId, d.IsDeleted })
                .HasDatabaseName("IX_Contacto_Cliente_IdCliente_EstaEliminado");

            // Índice de cobertura: búsqueda por cliente + dirección principal
            builder.HasIndex(d => new { d.ClientId, d.IsPrimary })
                .IncludeProperties(d => new { d.FullAddress, d.City, d.Latitude, d.Longitude })
                .HasDatabaseName("IX_Direccion_Cliente_IdCliente");
        }
    }
}
