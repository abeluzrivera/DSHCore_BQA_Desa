using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SDH.Application.DTOs.Clients;
using SDH.Application.Models;
using SDH.Application.Ports.Services; // Ajusta si el namespace cambiÃ³
using SDH.Domain.Enums.LogicaNegocio;
using System.Collections.Concurrent;

namespace SDH.infrastructure.Persistence.Cache // Â¡Ojo! Este servicio orquesta, deberÃ­a ir en Application
{
    public sealed class CustomerCacheImplementation(
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache,
        ILogger<CustomerCacheImplementation> logger) : ICustomerCacheService
    {
        private const int CacheTtlMinutes = 15;
        private const int SearchTop = 10;
        private const string CacheKeyPrefix = "cliente:";
        private readonly SemaphoreSlimCache _locks = new();

        // NOTA: Eliminamos IClienteQueryService del constructor porque si este 
        // CacheService es Singleton, no puede recibir un servicio Scoped. 
        // Siempre lo resolveremos a travÃ©s del scopeFactory.

        public async Task<IReadOnlyList<CustomerSearchResultDto>> SearchByNameAndIdentificationAsync(
            string termino, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(termino) || termino.Trim().Length < 3)
                return [];

            string trimmed = termino.Trim().ToLowerInvariant();

            string key = SetKey(trimmed);

            logger.LogDebug("BuscarPorNombreAsync: '{Termino}'", trimmed);

            if (IsIdentificationLikely(trimmed))
            {
                if (cache.TryGetValue(key, out Application.Models.CustomerCacheModel? cachedUser) && cachedUser != null)
                {
                    logger.LogDebug("Cache HIT (Point-Read Individual): '{Cedula}'", trimmed);

                    // Retornamos directamente convirtiendo el modelo de cachÃ© al DTO esperado
                    return
                        [
                            MapToSearchResultDto(cachedUser) // (Ver helper abajo)
                        ];
                }
            }


            // ====================================================================
            // CASO B: EL USUARIO ESTÃ BUSCANDO POR NOMBRE (O una cÃ©dula parcial)
            // ====================================================================
            // No podemos iterar el cachÃ© individual. Usamos el "Query Cache".
            // Creamos una llave que represente ESTA bÃºsqueda en particular.


            // Buscamos si alguien mÃ¡s ya buscÃ³ "juan" recientemente
            if (cache.TryGetValue(key, out IReadOnlyList<CustomerSearchResultDto>? cachedSearch) && cachedSearch != null)
            {
                logger.LogDebug("Cache HIT (Por Lista de Nombre): '{Termino}'", trimmed);
                return cachedSearch; // Retornamos la lista completa en milisegundos
            }

            logger.LogDebug("Cache MISS total: '{Termino}'. Consultando a la BD...", trimmed);

            // ====================================================================
            // FALLBACK: IR A LA BASE DE DATOS Y ACTUALIZAR AMBOS CACHÃ‰S
            // ====================================================================
            IReadOnlyList<CustomerSearchResultDto> items;

            await using (AsyncServiceScope scope = scopeFactory.CreateAsyncScope())
            {
                var queryService = scope.ServiceProvider.GetRequiredService<IClienteQueryService>();
                items = await queryService.LightSearchByNameAndIdentificationAsync(trimmed, SearchTop, cancellationToken);
            }

            MemoryCacheEntryOptions searchCacheOptions = BuildCacheOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            MemoryCacheEntryOptions individualCacheOptions = BuildCacheOptions();   // Ej: 1 hora

            // 1. Guardamos la LISTA COMPLETA bajo la llave de bÃºsqueda por nombre
            cache.Set(key, items, searchCacheOptions);

            // 2. Aprovechamos el viaje para pre-cargar el cachÃ© individual por ID
            // (Por si luego el usuario le da clic a un cliente y lo busca por su CÃ©dula)
            foreach (CustomerSearchResultDto dto in items)
            {
                string individualKey = SetKey(dto.Identification);
                if (!cache.TryGetValue(individualKey, out _))
                {
                    cache.Set(individualKey, Application.Models.CustomerCacheModel.FromSearchResult(dto), individualCacheOptions);
                }
            }

            return items;
        }

        private static CustomerSearchResultDto MapToSearchResultDto(Application.Models.CustomerCacheModel userCache)
        {
            // Inversa de tu 'UserCacheModel.FromSearchResult(dto)'
            return new CustomerSearchResultDto
            (
                userCache.Id,
                userCache.Identification,
                userCache.FullName ?? string.Empty,
                userCache.IsVerified
            // Mapea el resto de propiedades necesarias
            );
        }

        private static bool IsIdentificationLikely(string termino)
        {
            // LÃ³gica de negocio: Asumimos que si solo contiene dÃ­gitos (y opcionalmente cierta longitud),
            // es muy probable que el usuario estÃ© buscando una cÃ©dula especÃ­fica y no un nombre.
            // Ajusta la regla (ej. == 10) segÃºn tu paÃ­s/dominio.
            return termino.All(char.IsDigit);
        }


        public async Task<CustomerCacheModel?> GetSegmentedDetailAsync(
            string cedula,
            EnumQueryClientType tipo,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;

            string key = SetKey(cedula.Trim());

            // â”€â”€ Paso 1: Lectura RÃ¡pida (Sin Bloqueo) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (cache.TryGetValue(key, out Application.Models.CustomerCacheModel? cachedObj))
            {
                if (cachedObj!.HasSegment(tipo))
                {
                    logger.LogDebug("Cache HIT completo: {Cedula} / {Tipo}", cedula, tipo);
                    return cachedObj;
                }
            }

            // â”€â”€ Paso 2: Doble VerificaciÃ³n con Bloqueo (Stampede Protection) â”€â”€
            SemaphoreSlim sem = _locks.GetOrCreate(key);
            await sem.WaitAsync(cancellationToken);
            try
            {
                // Double-checked locking
                if (cache.TryGetValue(key, out cachedObj))
                {
                    if (cachedObj!.HasSegment(tipo))
                    {
                        logger.LogDebug("Cache HIT (post-lock): {Cedula} / {Tipo}", cedula, tipo);
                        return cachedObj;
                    }
                    logger.LogDebug("Cache HIT parcial (sin {Tipo}): {Cedula}", tipo, cedula);
                }
                else
                {
                    // Caso 3: CachÃ© Miss Total -> Cargar modelo base desde BD
                    logger.LogDebug("Cache MISS total: {Cedula}", cedula);

                    await using AsyncServiceScope scopeBase = scopeFactory.CreateAsyncScope();
                    var queryServiceBase = scopeBase.ServiceProvider.GetRequiredService<IClienteQueryService>();

                    var resultados = await queryServiceBase.LightSearchByNameAndIdentificationAsync(cedula, 1, cancellationToken);
                    var dto = resultados.FirstOrDefault(x => x.Identification == cedula.Trim());

                    if (dto is null) return null; // El cliente no existe

                    cachedObj = Application.Models.CustomerCacheModel.FromSearchResult(dto);
                }

                // â”€â”€ Paso 3: HidrataciÃ³n del Segmento â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                // Resolvemos el QueryService para la hidrataciÃ³n
                await using AsyncServiceScope scopeHydrate = scopeFactory.CreateAsyncScope();
                var queryServiceHydrate_clientQuery = scopeHydrate.ServiceProvider.GetRequiredService<IClienteQueryService>();

                object? segmento = tipo switch
                {
                    EnumQueryClientType.Contact =>
                        await queryServiceHydrate_clientQuery.ObtenerContactosAsync(cedula, cancellationToken),

                    EnumQueryClientType.Address =>
                        await queryServiceHydrate_clientQuery.ObtenerDireccionesAsync(cedula, cancellationToken),

                    EnumQueryClientType.Financiero => null, // Implementar cuando exista

                    _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "TipoConsulta no soportado")
                };

                if (segmento != null)
                {
                    cachedObj.Hydrate(tipo, segmento);
                    // Sobrescribimos en cachÃ© el objeto ahora hidratado
                    cache.Set(key, cachedObj, BuildCacheOptions());
                }

                return cachedObj;
            }
            finally
            {
                sem.Release();
            }
        }

        private static MemoryCacheEntryOptions BuildCacheOptions() => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheTtlMinutes),
            Priority = CacheItemPriority.Normal,
            Size = 1
        };

        private static string SetKey(string cedula) => $"{CacheKeyPrefix}{cedula}";

        // En ClienteCacheService.cs (Infraestructura)
        public async Task<CustomerCacheModel?> GetOrHydrateFullProfileAsync(string cedula, CancellationToken ct = default)
        {
            string key = SetKey(cedula.Trim());

            // â”€â”€ 1. LECTURA RÃPIDA: Â¿EstÃ¡ 100% completo en cachÃ©? â”€â”€
            if (cache.TryGetValue(key, out Application.Models.CustomerCacheModel? cachedObj) &&
                cachedObj!.HasSegment(EnumQueryClientType.Contact) &&
                cachedObj.HasSegment(EnumQueryClientType.Address))
            {
                return cachedObj; // Hit perfecto: 0 milisegundos
            }

            // â”€â”€ PROTECCIÃ“N CON SEMÃFORO â”€â”€
            SemaphoreSlim sem = _locks.GetOrCreate(key);
            await sem.WaitAsync(ct);
            try
            {
                // Double check por si otro hilo ya lo llenÃ³ mientras esperÃ¡bamos
                if (cache.TryGetValue(key, out cachedObj))
                {
                    // â”€â”€ 2. ESTÃ A MEDIAS: Hidratar solo lo que falta â”€â”€
                    await using AsyncServiceScope scopeHydrate = scopeFactory.CreateAsyncScope();
                    var queryService = scopeHydrate.ServiceProvider.GetRequiredService<IClienteQueryService>();

                    bool fueModificado = false;

                    if (!cachedObj.HasSegment(EnumQueryClientType.Contact))
                    {
                        var contactos = await queryService.ObtenerContactosAsync(cedula, ct);
                        cachedObj.Hydrate(EnumQueryClientType.Contact, contactos);
                        fueModificado = true;
                    }

                    if (!cachedObj.HasSegment(EnumQueryClientType.Address))
                    {
                        var direcciones = await queryService.ObtenerDireccionesAsync(cedula, ct);
                        cachedObj.Hydrate(EnumQueryClientType.Address, direcciones);
                        fueModificado = true;
                    }

                    if (fueModificado)
                    {
                        cache.Set(key, cachedObj, BuildCacheOptions()); // Actualizamos la cachÃ©
                    }

                    return cachedObj;
                }
                else
                {
                    // â”€â”€ 3. CACHE MISS TOTAL: Traer TODO de la BD en 1 solo Query â”€â”€
                    await using AsyncServiceScope scopeBase = scopeFactory.CreateAsyncScope();
                    var queryService = scopeBase.ServiceProvider.GetRequiredService<IClienteQueryService>();

                    var dtoCompleto = await queryService.ObtenerPorIdentificacionAsync(cedula, ct);
                    if (dtoCompleto == null) return null; // Cliente no existe

                    // Creamos el modelo de cachÃ© ya totalmente inflado
                    cachedObj = new Application.Models.CustomerCacheModel
                    {
                        Id = dtoCompleto.Id,
                        Identification = dtoCompleto.Identification,
                        FullName = dtoCompleto.FullName,
                        Contacts = dtoCompleto.Contacts,
                        Addresses = dtoCompleto.Addresses
                    };

                    cache.Set(key, cachedObj, BuildCacheOptions());
                    return cachedObj;
                }
            }
            finally
            {
                sem.Release();
            }
        }

        public Task InvalidateClienteAsync(string cedula)
        {
            cache.Remove(SetKey(cedula));
            return Task.CompletedTask;
        }

        public async Task RefreshSegmentAsync(string identification, EnumQueryClientType tipo, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identification)) return;
            string key = SetKey(identification.Trim());
            if (!cache.TryGetValue(key, out CustomerCacheModel? _))
            {
                logger.LogDebug("RefreshSegment no-op (not cached): {Id}/{Tipo}", identification, tipo);
                return;
            }
            SemaphoreSlim sem = _locks.GetOrCreate(key);
            await sem.WaitAsync(cancellationToken);
            try
            {
                if (!cache.TryGetValue(key, out CustomerCacheModel? cachedObj) || cachedObj is null)
                {
                    logger.LogDebug("RefreshSegment no-op (evicted): {Id}/{Tipo}", identification, tipo);
                    return;
                }
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                var queryService = scope.ServiceProvider.GetRequiredService<IClienteQueryService>();
                object? newSegment = tipo switch
                {
                    EnumQueryClientType.Contact => await queryService.ObtenerContactosAsync(identification, cancellationToken),
                    EnumQueryClientType.Address => await queryService.ObtenerDireccionesAsync(identification, cancellationToken),
                    _ => null
                };
                if (newSegment is null) { logger.LogWarning("RefreshSegment: tipo {Tipo} no soportado", tipo); return; }
                cachedObj.Hydrate(tipo, newSegment);
                cache.Set(key, cachedObj, BuildCacheOptions());
                logger.LogDebug("RefreshSegment OK: {Id}/{Tipo} actualizado en cache", identification, tipo);
            }
            finally { sem.Release(); }
        }

    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Utilidad interna: cachÃ© de semÃ¡foros
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    internal sealed class SemaphoreSlimCache
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _map = new();

        public SemaphoreSlim GetOrCreate(string key) =>
            _map.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }
}