using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.IO;
using SDH.Application;
using SDH.infrastructure.Persistence;
using SDH.infrastructure.Persistence.Data;
using SDH.infrastructure.Persistence.Services;
using SDH.infrastructure.Persistence.Seeders;
using SDH.infrastructure.Startup;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine("Logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
string rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
string connectionString = ConfigCrypto.DecryptFromEnvironment(rawConnectionString);

// Configurar DbContext con opciones seg�n el entorno
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60); // Timeout de 60 segundos
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });

    // Solo habilitar logging detallado en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }

    // Suprimir advertencia de modelo pendiente durante desarrollo
    options.ConfigureWarnings(warnings =>
    {
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
    });
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Registrar cach� en memoria para cat�logos
builder.Services.AddMemoryCache(options =>
{
    // Limita la memoria a un m�ximo de elementos cacheados simult�neamente
    options.SizeLimit = 2000;
});

// Registrar la capa de persistencia (Repositorios y UnitOfWork)
builder.Services.AddPersistence(builder.Configuration);

// Registrar la capa de aplicaci�n (Servicios)
builder.Services.AddApplicationServices();

// Registrar inicializador de cach� de cat�logos
builder.Services.AddScoped<CatalogosCacheInitializer>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SmartHubAuth";
        options.Cookie.HttpOnly = true;
        // Use SameAsRequest in development to allow HTTP, Always in production for security
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization(options =>
{
    // Política base: cualquier usuario autenticado
    options.AddPolicy("Autenticado", policy => policy.RequireAuthenticatedUser());

    // Políticas por rol — los strings deben coincidir con RoleGroupMappings keys
    options.AddPolicy("SoloAdmin",       policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("AdminOSupervisor",policy => policy.RequireRole("ADMIN", "SUPERVISOR"));
    options.AddPolicy("TodosLosAgentes", policy => policy.RequireRole("ADMIN", "SUPERVISOR", "AGENTE"));
    options.AddPolicy("AccesoGeneral",   policy => policy.RequireRole("ADMIN", "SUPERVISOR", "AGENTE", "CONSULTA"));
});

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

try
{
    // 3. >>> AGREGA ESTO EN EL PIPELINE HTTP (Generalmente al principio) <<<
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        // Esto genera la interfaz gr�fica (UI)
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API de Cat�logos V1");
        });
    }

    // Seed initial data
    using (IServiceScope scope = app.Services.CreateScope())
    {
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (app.Environment.IsDevelopment())
        {
            // Aplicar migraciones pendientes
            await context.Database.MigrateAsync();
        }

        // Seed usuarios iniciales
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await UsuarioSeeder.SeedAsync(context, logger, app.Configuration);
        await ViewSeeder.SeedAsync(context, logger);
        await CatalogoSeeder.SeedAsync(context, logger);

        // Inicializar cach� de cat�logos
        try
        {
            CatalogosCacheInitializer cacheInitializer = scope.ServiceProvider.GetRequiredService<CatalogosCacheInitializer>();
            await cacheInitializer.InicializarCacheAsync();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "No se pudo inicializar el cach� de cat�logos. Se cargar� bajo demanda.");
        }
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"]        = "DENY";
        context.Response.Headers["X-XSS-Protection"]       = "1; mode=block";
        context.Response.Headers["Referrer-Policy"]        = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"]     = "camera=(), microphone=(), geolocation=()";

        // CSP: in Development allow VS Browser Link (localhost) and Hot Reload (ws://localhost)
        string csp = app.Environment.IsDevelopment()
            ? "default-src 'self'; " +
              "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://www.clarity.ms https://scripts.clarity.ms; " +
              "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
              "font-src 'self' https://fonts.gstatic.com; " +
              "img-src 'self' data: https://lh3.googleusercontent.com https://maps.gstatic.com https://*.googleapis.com https://c.clarity.ms; " +
              "connect-src 'self' http://localhost:* ws://localhost:* https://*.clarity.ms https://cdn.jsdelivr.net; " +
              "frame-src https://www.google.com https://maps.google.com https://www.google.com.ec; " +
              "frame-ancestors 'none';"
            : "default-src 'self'; " +
              "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://www.clarity.ms https://scripts.clarity.ms; " +
              "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
              "font-src 'self' https://fonts.gstatic.com; " +
              "img-src 'self' data: https://lh3.googleusercontent.com https://maps.gstatic.com https://*.googleapis.com https://c.clarity.ms; " +
              "connect-src 'self' https://*.clarity.ms; " +
              "frame-src https://www.google.com https://maps.google.com https://www.google.com.ec; " +
              "frame-ancestors 'none';";

        context.Response.Headers["Content-Security-Policy"] = csp;
        await next();
    });

    app.UseRouting();

    // Add authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    // Redirect root to login
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/Login");
        return Task.CompletedTask;
    });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Map Razor Pages
    app.MapRazorPages()
       .WithStaticAssets();

    app.Run();
}
catch (DbException ex)
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Database error during host startup.");
    throw;
}
catch (IOException ex)
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "I/O error during host startup.");
    throw;
}
catch (OperationCanceledException ex)
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Startup was canceled.");
    throw;
}
catch (Exception ex)
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Unhandled exception during host startup.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
