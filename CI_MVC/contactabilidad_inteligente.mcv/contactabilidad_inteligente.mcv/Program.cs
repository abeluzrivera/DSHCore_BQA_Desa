using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SDH.Application;
using SDH.infrastructure.Persistence;
using SDH.infrastructure.Persistence.Data;
using SDH.infrastructure.Persistence.Services;
using SDH.infrastructure.Persistence.Seeders;
using SDH.infrastructure.Startup;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddPersistence();

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

builder.Services.AddAuthorization();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

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

    await UsuarioSeeder.SeedAsync(context, logger);
    await CatalogoSeeder.SeedAsync(context, logger);

    // Inicializar cach� de cat�logos
    try
    {
        CatalogosCacheInitializer cacheInitializer = scope.ServiceProvider.GetRequiredService<CatalogosCacheInitializer>();
        await cacheInitializer.InicializarCacheAsync();
    }
    catch (Exception ex)
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
