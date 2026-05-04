using Microsoft.Extensions.DependencyInjection;
using SDH.Application.Ports.Queries;
using SDH.Application.Ports.Services;
using SDH.Domain.Ports;
using SDH.Domain.Repositories;
using SDH.infrastructure.Persistence.Cache;
using SDH.infrastructure.Persistence.Ports.Services;
using SDH.infrastructure.Persistence.Queries;
using SDH.infrastructure.Persistence.Repositories;
using SDH.infrastructure.Persistence.Services;
using SDH.Infrastructure.Persistence.Queries;
using SDH.Infrastructure.Persistence.Repositories;

namespace SDH.infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            // Repositorios
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICatalogRepository, CatalogRepository>();

            // Query Services (CQRS Read Side)
            services.AddScoped<ICatalogQueryService, CatalogQueryService>();
            services.AddScoped<IClienteQueryService, ClienteQueryService>();
            services.AddScoped<IUserQueryService, UserQueryService>();

            // Infrastructure Services
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITokenGenerator, TokenGenerator>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<ICustomerCacheService, CustomerCacheImplementation>();
            services.AddSingleton<IConfigDecryptionService, ConfigDecryptionService>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
