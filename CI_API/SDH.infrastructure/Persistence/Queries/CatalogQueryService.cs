using Microsoft.EntityFrameworkCore;
using SDH.Application.DTOs;
using SDH.Application.Ports.Queries;  // Donde está ICatalogoQueryService
using SDH.Domain.Attributes;
using SDH.Domain.Enums;
using SDH.infrastructure.Persistence.Data;
using System.Reflection;

namespace SDH.Infrastructure.Persistence.Queries
{
    public class CatalogQueryService(ApplicationDbContext context) : ICatalogQueryService
    {
        public async Task<IReadOnlyList<CatalogDetail>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.VwCatalogosDetalle // Leemos directo de la vista!
                .AsNoTracking() // Obligatorio para máximo rendimiento
                .OrderBy(v => v.NombreGrupo)
                .ThenBy(v => v.OrdenVisual)
                .Select(v => new CatalogDetail(
                    v.IdItemCatalogo,
                    v.IdGrupoCatalogo,
                    v.NombreGrupo,
                    v.CodigoValor,
                    v.TextoVisual,
                    v.OrdenVisual,
                    v.EstaActivo,
                    v.EsSistema
                ))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CatalogDetail>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await context.VwCatalogosDetalle
                .AsNoTracking()
                .Where(v => v.EstaActivo) // Filtro en base de datos
                .OrderBy(v => v.NombreGrupo)
                .ThenBy(v => v.OrdenVisual)
                .Select(v => new CatalogDetail(
                    v.IdItemCatalogo, v.IdGrupoCatalogo, v.NombreGrupo, v.CodigoValor,
                    v.TextoVisual, v.OrdenVisual, v.EstaActivo, v.EsSistema
                ))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CatalogDetail>> GetByGroupNameAsync(string nombreGrupo, CancellationToken cancellationToken = default)
        {
            return await context.VwCatalogosDetalle
                .AsNoTracking()
                .Where(v => v.NombreGrupo == nombreGrupo && v.EstaActivo)
                .OrderBy(v => v.OrdenVisual)
                .Select(v => new CatalogDetail(
                    v.IdItemCatalogo, v.IdGrupoCatalogo, v.NombreGrupo, v.CodigoValor,
                    v.TextoVisual, v.OrdenVisual, v.EstaActivo, v.EsSistema
                ))
                .ToListAsync(cancellationToken);
        }

        public async Task<CatalogDetail?> GetItemByCodeAsync(string nombreGrupo, string codigoValor, CancellationToken cancellationToken = default)
        {
            return await context.VwCatalogosDetalle
                .AsNoTracking()
                .Where(v => v.NombreGrupo == nombreGrupo && v.CodigoValor == codigoValor)
                .Select(v => new CatalogDetail(
                    v.IdItemCatalogo, v.IdGrupoCatalogo, v.NombreGrupo, v.CodigoValor,
                    v.TextoVisual, v.OrdenVisual, v.EstaActivo, v.EsSistema
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<CatalogDiscrepancyDto>> CheckIntegrityAsync(CancellationToken ct = default)
        {
            // 1. Cargamos los Aggregate Roots con sus hijos (Navegación DDD)
            var gruposEnBD = await context.CatalogGroup
                .Include(g => g.Items)
                .AsNoTracking() // Solo lectura para diagnóstico
                .ToListAsync(ct);

            // 2. Obtenemos el "Mapa del Tesoro" desde el código (Reflexión)
            // Diccionario: Key = NombreGrupo, Value = Lista de Códigos constantes
            var mapaCodigo = GetConstantMapByGroup();

            var discrepancias = new List<CatalogDiscrepancyDto>();

            foreach (var grupoBD in gruposEnBD)
            {
                // ¿Existe este grupo en nuestras constantes de C#?
                if (!mapaCodigo.TryGetValue(grupoBD.GroupName, out var codigosEsperados))
                {
                    // ALERTA: El grupo entero existe en DB pero no en el código
                    discrepancias.Add(new CatalogDiscrepancyDto
                    {
                        GroupName = grupoBD.GroupName,
                        Source = "Grupo NO mapeado en C#"
                    });
                    continue;
                }

                // Validamos los ítems dentro de este Agregado
                foreach (var itemBD in grupoBD.Items.Where(i => i.IsActive))
                {
                    if (!codigosEsperados.Contains(itemBD.ValueCode))
                    {
                        discrepancias.Add(new CatalogDiscrepancyDto
                        {
                            GroupName = grupoBD.GroupName,
                            ValueCode = itemBD.ValueCode,
                            DisplayName = itemBD.DisplayName,
                            Source = "Ítem manual en DB (No existe en constantes del grupo)"
                        });
                    }
                }
            }

            return discrepancias;
        }

        private Dictionary<string, HashSet<string>> GetConstantMapByGroup()
        {
            var mapa = new Dictionary<string, HashSet<string>>();

            // 1. Buscamos todas las clases en el ensamblado del Dominio que tengan nuestro atributo
            var tiposConAtributo = typeof(CatalogGroups).Assembly.GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(MappedCatalog), true).Length != 0);

            foreach (var tipo in tiposConAtributo)
            {
                // 2. Extraer el nombre del grupo definido en el Atributo
                var atributo = (MappedCatalog)tipo.GetCustomAttributes(typeof(MappedCatalog), true).First();
                string nombreGrupo = atributo.Value;

                // 3. Extraer todos los valores de las constantes (public const string)
                var valoresConstantes = tipo.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                    .Select(f => f.GetValue(null)?.ToString() ?? string.Empty)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToHashSet();

                if (!mapa.ContainsKey(nombreGrupo))
                {
                    mapa.Add(nombreGrupo, valoresConstantes);
                }
            }

            return mapa;
        }

    }
}