using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDH.Domain.Entities.Operative;

namespace SDH.infrastructure.Persistence.DataConfigurations.Operativo
{
    /// <summary>
    /// Configuración de EF Core para la entidad OficializacionCore - Solo índices adicionales
    /// </summary>
    public class OficializacionCoreConfiguration : IEntityTypeConfiguration<CoreOfficializations>
    {
        public void Configure(EntityTypeBuilder<CoreOfficializations> builder)
        {

            // 1. Tabla y Esquema
            builder.ToTable("Tbl_Oficializacion_Core", "operativo");

            // 2. Clave Primaria
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                   .HasColumnName("Id_Oficializacion_Core")
                   .ValueGeneratedOnAdd();

            builder.Property(c => c.ClientId)
                   .HasColumnName("Id_Cliente")
                   .IsRequired();

            builder.Property(o => o.SentJsonPayload)
                   .HasColumnName("Trama_Json_Enviada")
                   .HasColumnType("nvarchar(max)")
                   .IsRequired();

            builder.Property(o => o.CoreResponseCode)
                   .HasColumnName("Respuesta_Core_Codigo")
                   .HasMaxLength(10)
                   .IsRequired();

            // 4. Auditoría
            builder.Property(o => o.CreatedAt)
                   .HasColumnName("Fecha_Creacion")
                   .HasColumnType("datetime2(7)")
                   .IsRequired();

            builder.Property(o => o.CreatedBy)
                   .HasColumnName("Usuario_Creacion")
                   .HasMaxLength(100)
                   .IsRequired();

            // Índices para optimización de consultas
            builder.HasIndex(o => o.ClientId)
                .HasDatabaseName("IX_OficializacionCore_IdCliente");

            builder.HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_OficializacionCore_FechaCreacion");

            builder.HasIndex(o => o.CoreResponseCode)
                .HasDatabaseName("IX_OficializacionCore_RespuestaCoreCodigo");
        }
    }
}
