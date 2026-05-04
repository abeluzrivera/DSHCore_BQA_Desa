using Microsoft.Extensions.DependencyInjection;
using SDH.Application.Ports.Services;
using SDH.Application.Services;

namespace SDH.Application
{
    /// <summary>
    /// ExtensiÃ³n para configurar la inyecciÃ³n de dependencias de la capa de aplicaciÃ³n
    /// </summary>
    public static class AppServiceInjection
    {
        /// <summary>
        /// Registra todos los servicios de aplicaciÃ³n en el contenedor de DI
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Servicios de aplicaciÃ³n existentes
            services.AddScoped<CustomerCommandService>();
            services.AddScoped<AutenticacionService>();
            services.AddScoped<CatalogoService>();
            services.AddScoped<CatalogoCommandService>();
            services.AddScoped<UserCommandService>();


            // Servicio de validaciÃ³n de archivos para carga masiva
            services.AddScoped<IFileValidationService, FileValidationService>();

            // Servicio de extracciÃ³n de IDs desde archivos para el modal de lote
            services.AddScoped<IFileExtractionService, FileExtractionService>();

            return services;
        }
    }
}

