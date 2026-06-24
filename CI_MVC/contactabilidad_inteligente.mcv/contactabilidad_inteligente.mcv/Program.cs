using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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
using Serilog.Context;

IConfiguration bootstrapConfig = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

string rawLogPath = bootstrapConfig["SerilogSettings:LogPath"] ?? "Logs/log-.txt";
int retainedFiles = int.TryParse(bootstrapConfig["SerilogSettings:RetainedFileCountLimit"], out int parsed) ? parsed : 30;

string resolvedLogPath = Path.IsPathRooted(rawLogPath)
    ? rawLogPath
    : Path.Combine(Directory.GetCurrentDirectory(), rawLogPath);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: resolvedLogPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: retainedFiles,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
string rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
string connectionString = ConfigCrypto.DecryptFromEnvironment(rawConnectionString);

Log.Information("environment: {EnvironmentName}", builder.Environment.EnvironmentName);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });

    // El snapshot fue generado con EF Core 9; al correr con EF Core 10 las convenciones
    // difieren y disparan PendingModelChangesWarning sin cambios reales de modelo.
    // Se registra como Warning hasta que se regenere el snapshot con EF 10.
    options.ConfigureWarnings(w =>
        w.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 2000;
});

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddApplicationServices();

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

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
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

string rawDpPath = builder.Configuration["DataProtection:KeysPath"] ?? "DataProtection-Keys";
string dataProtectionPath = Path.IsPathRooted(rawDpPath)
    ? rawDpPath
    : Path.Combine(Directory.GetCurrentDirectory(), rawDpPath);
DirectoryInfo dpDirectory = new(dataProtectionPath);
if (!dpDirectory.Exists)
    dpDirectory.Create();
builder.Services.AddDataProtection()
    .SetApplicationName("SmartDataHub")
    .PersistKeysToFileSystem(dpDirectory);

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

        await context.Database.MigrateAsync();

        // Seed usuarios iniciales
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await UsuarioSeeder.SeedAsync(context, logger, app.Configuration);
        await ViewSeeder.SeedAsync(context, logger);
        await CatalogoSeeder.SeedAsync(context, logger);

        // Inicializar cache de catalogos
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

    app.Use(async (context, next) =>
    {
        var correlationId = context.TraceIdentifier;
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            await next();
        }
    });

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"]        = "DENY";
        context.Response.Headers["X-XSS-Protection"]       = "1; mode=block";
        context.Response.Headers["Referrer-Policy"]        = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"]     = "camera=(), microphone=(), geolocation=()";

        var connectSrc = app.Environment.IsDevelopment()
            ? "connect-src 'self' http://localhost:* ws://localhost:* https://*.clarity.ms https://cdn.jsdelivr.net"
            : "connect-src 'self' https://*.clarity.ms";

        string csp =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://www.clarity.ms https://scripts.clarity.ms; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "img-src 'self' data: https://lh3.googleusercontent.com https://maps.gstatic.com https://*.googleapis.com https://c.clarity.ms https://*.bing.com; " +
            $"{connectSrc}; " +
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
