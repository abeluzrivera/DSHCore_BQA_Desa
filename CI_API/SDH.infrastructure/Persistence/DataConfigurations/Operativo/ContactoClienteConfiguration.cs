using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Domain.Entities.Operative;
using SDH.Domain.Entities.Parametro;

namespace SDH.infrastructure.Persistence.DataConfigurations.Operativo
{
    /// <summary>
    /// Configuraci�n de EF Core para la entidad ContactoCliente - Solo �ndices adicionales
    /// </summary>
    public class ContactoClienteConfiguration : IEntityTypeConfiguration<CustomerContacts>
    {
        public void Configure(EntityTypeBuilder<CustomerContacts> builder)
        {
            // 1. Tabla y Esquema
            builder.ToTable("Tbl_Contacto_Cliente", "operativo");

            // 2. Clave Primaria
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClientId)
                   .HasColumnName("Id_Cliente")
                   .IsRequired();

            builder.Property(c => c.Id)
                   .HasColumnName("Id_Contacto_Cliente")
                   .ValueGeneratedOnAdd();

            // 3. Propiedades Escalares (Traducción de Data Annotations)

            // Nota: La columna Id_Cliente se configura desde la entidad Padre (ClienteConfiguration) 
            // mediante un Shadow Property (HasForeignKey("IdCliente")), por lo que no hace falta definirla aquí.

            builder.Property(c => c.ContactMediumTypeId)
                   .HasColumnName("Id_Tipo_Contacto")
                   .IsRequired();

            builder.HasOne<CatalogItem>()
                   .WithMany()
                   .HasForeignKey(c => c.ContactMediumTypeId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Contacto_Cliente_TipoMedioContacto");

            builder.Property(c => c.ContactValue)
                   .HasColumnName("Valor_Contacto")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(c => c.VerifyStatusId)
                   .HasColumnName("Id_Estado_Verificacion")
                   .IsRequired()
                   .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);

            builder.HasOne<CatalogItem>()
                   .WithMany()
                   .HasForeignKey(c => c.VerifyStatusId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Contacto_Cliente_EstadoVerificado");

            builder.Property(c => c.VerifiedBy)
                   .HasColumnName("Usuario_Verificador")
                   .HasMaxLength(50)
                   .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);

            builder.Property(c => c.VerifiedAt)
                    .HasColumnName("Fecha_Verificacion")
                    .HasColumnType("datetime2(3)")
                    .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);

            builder.Property(c => c.IsDeleted)
                   .HasColumnName("Esta_Eliminado")
                   .IsRequired();

            builder.Property(c => c.Source)
                   .HasColumnName("Source")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(c => c.LOPDPStatusId)
                   .HasColumnName("Id_Estado_LOPDP");

            builder.HasOne<CatalogItem>()
                   .WithMany()
                   .HasForeignKey(c => c.LOPDPStatusId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Contacto_Cliente_EstadoLOPDP");


            // 4. Auditoría (Fechas y Usuarios)

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

            // ──────────────────────────────────────────────────
            // ÍNDICES PARA OPTIMIZACIÓN DE CONSULTAS
            // ──────────────────────────────────────────────────

            // 2. Índice compuesto (Tupla anónima fuertemente tipada)
            builder.HasIndex(c => new { c.ClientId, c.IsDeleted })
                .HasDatabaseName("IX_ContactoCliente_IdCliente_EstaEliminado");

            // 3. Índice simple normal
            builder.HasIndex(c => c.VerifyStatusId)
                .HasDatabaseName("IX_ContactoCliente_EstadoVerificacion");

            // 4. Índice de cobertura filtrado
            builder.HasIndex(c => new { c.ClientId, c.VerifyStatusId, c.ContactMediumTypeId })
                .IncludeProperties(c => new { c.ContactValue, c.IsDeleted })
                .HasFilter("[Esta_Eliminado] = 0")
                .HasDatabaseName("IX_Contacto_Cliente_Performance_Query");

            // 5. Índice de búsqueda inversa por valor de contacto
            builder.HasIndex(c => c.ContactValue)
                .HasDatabaseName("IX_Contacto_Cliente_Valor");
        }
    }
}
