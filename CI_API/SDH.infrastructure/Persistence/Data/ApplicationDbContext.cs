using Microsoft.EntityFrameworkCore;
using SDH.Application.Ports.Services;
using SDH.Domain.Contracts;
using SDH.Domain.Entities.Operative;
using SDH.Domain.Entities.Parametro;
using SDH.Domain.Entities.Seguridad;
using SDH.Domain.Enums;
using SDH.Infrastructure.Persistence.ReadModels.Parametro;

namespace SDH.infrastructure.Persistence.Data
{
    /// <summary>
    /// DbContext principal de la aplicación con todas las entidades del dominio
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        #region DbSets - Esquema Seguridad
        public DbSet<Users> Users => Set<Users>();
        #endregion

        #region DbSets - Esquema Operativo
        public DbSet<Client> Client => Set<Client>();
        public DbSet<ClientFinancial> ClientFinancial => Set<ClientFinancial>();
        public DbSet<CustomerContacts> CustomerContacts => Set<CustomerContacts>();
        public DbSet<CoreOfficializations> CoreOfficializations => Set<CoreOfficializations>();
        public DbSet<CustomerAddresses> CustomerAddresses => Set<CustomerAddresses>();
        #endregion

        #region DbSets - Esquema Parametro
        public DbSet<CatalogGroup> CatalogGroup => Set<CatalogGroup>();
        public DbSet<CatalogItem> CatalogItem => Set<CatalogItem>();
        public DbSet<VwCatalogoDetalle> VwCatalogosDetalle => Set<VwCatalogoDetalle>();
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la vista como keyless (solo lectura)
            modelBuilder.Entity<VwCatalogoDetalle>()
                .HasNoKey()
                .ToView("Vw_Cat_Detalle_General", "parametro");

            // Aplicar todas las configuraciones desde el assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // OPTIMIZACIÓN 1: Resolvemos las variables pesadas UNA SOLA VEZ fuera del bucle
            string usuarioActual = _currentUserService.ObtenerUsuarioActual() ?? GlobalVariables.SystemUser;
            DateTime fechaActual = DateTime.Now;

            // OPTIMIZACIÓN 2: Filtramos el ChangeTracker una sola vez, trayendo solo lo que cambió
            var entradasModificadas = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entradasModificadas)
            {
                // OPTIMIZACIÓN 3: Pattern matching. Si la entidad cumple el contrato, lo casteamos y aplicamos
                if (entry.Entity is IAuditableEntity auditable)
                {
                    auditable.LastModifiedAt = fechaActual;
                    auditable.LastModifiedBy = usuarioActual;
                }

                //if (entry.Entity is IVerificableEntity verificable)
                //{
                //    verificable.FechaVerificacion = fechaActual;
                //    verificable.UsuarioVerificador = usuarioActual;
                //}
            }

            // Dejamos que EF Core haga el guardado normal en la base de datos
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
