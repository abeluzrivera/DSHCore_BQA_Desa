using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Domain.Entities.Operative;

namespace SDH.infrastructure.Persistence.DataConfigurations.Operativo
{
    /// <summary>
    /// Configuración de EF Core para la entidad FinancieroCliente - Solo índices adicionales
    /// </summary>
    public class FinancieroClienteConfiguration : IEntityTypeConfiguration<ClientFinancial>
    {
        public void Configure(EntityTypeBuilder<ClientFinancial> builder)
        {

            // 1. Tabla y Esquema
            builder.ToTable("Tbl_Financiero_cliente", "operativo");

            // 2. Clave Primaria
            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                   .HasColumnName("Id_Financiero_Cliente")
                   .ValueGeneratedOnAdd();

            builder.Property(f => f.IdCliente)
                   .HasColumnName("Id_Cliente")
                   .IsRequired();

            builder.Property(f => f.ClientId)
                   .HasColumnName("Tipo_Contabilidad")
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(f => f.AccountingAmount)
                   .HasColumnName("Monto_Contable")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            // Índices para optimización de consultas (Usando Shadow Property)
            builder.HasIndex(c => c.IdCliente)
                .HasDatabaseName("IX_FinancieroCliente_IdCliente");

            builder.HasIndex(f => f.ClientId)
                .HasDatabaseName("IX_FinancieroCliente_TipoContabilidad");
        }
    }
}
