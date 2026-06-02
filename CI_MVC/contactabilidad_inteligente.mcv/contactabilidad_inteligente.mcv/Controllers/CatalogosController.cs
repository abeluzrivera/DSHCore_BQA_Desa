using contactabilidad_inteligente.mcv.Helpers;
using Microsoft.AspNetCore.Mvc;
using SDH.Application.Ports.Queries;
using SDH.Application.Services;

namespace contactabilidad_inteligente.mcv.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogosController(
        CatalogoService queryService, ICatalogQueryService catalogoQueryService,
        CatalogoCommandService commandService) : ControllerBase
    {
        private readonly ICatalogQueryService _catalogoQueryService = catalogoQueryService;

        // ════════════════════════════════════════════════════════════════════
        // CATÁLOGOS DE CONTACTABILIDAD (Centralizados desde Helpers)
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/catalogos/error-codes
        /// Retorna el catálogo completo de códigos de error de contactabilidad
        /// agrupados por tipo (email, phone, address).
        /// </summary>
        [HttpGet("error-codes")]
        [ResponseCache(Duration = 3600)]
        public IActionResult GetErrorCodes()
        {
            var catalog = new Dictionary<string, List<ErrorCodeDto>>();

            foreach (var (category, codes) in ContactabilityLabelHelper.RejectionPills)
            {
                catalog[category] = codes.Select(c => new ErrorCodeDto
                {
                    Code = c.Code,
                    Label = c.ShortLabel
                }).ToList();
            }

            return Ok(catalog);
        }

        /// <summary>
        /// GET /api/catalogos/contact-types
        /// Retorna los tipos de contacto con sus metadatos completos (icon, label, order, etc.).
        /// </summary>
        [HttpGet("contact-types")]
        [ResponseCache(Duration = 3600)]
        public IActionResult GetContactTypes()
        {
            var types = ContactabilityLabelHelper.ContactTypes.Values
                .OrderBy(t => t.Order)
                .Select(t => new
                {
                    code = t.Code,
                    icon = t.Icon,
                    iconType = t.IconType,
                    label = t.Label,
                    order = t.Order,
                    shouldTruncate = t.ShouldTruncate
                });

            return Ok(types);
        }

        /// <summary>
        /// GET /api/catalogos/contact-states
        /// Retorna los estados de contactabilidad con sus metadatos visuales (Tailwind CSS).
        /// </summary>
        [HttpGet("contact-states")]
        [ResponseCache(Duration = 3600)]
        public IActionResult GetContactStates()
        {
            var states = ContactabilityLabelHelper.States.Values.Select(s => new
            {
                code = s.Code,
                bgClass = s.BgClass,
                borderClass = s.BorderClass,
                textClass = s.TextClass,
                state = s.State
            });

            return Ok(states);
        }

        // ════════════════════════════════════════════════════════════════════
        // CATÁLOGOS GENERALES (Lógica Original)
        // ════════════════════════════════════════════════════════════════════

        [HttpGet("{nombreGrupo}")]
        public async Task<IActionResult> ObtenerPorGrupo(string nombreGrupo)
        {
            var items = await queryService.ObtenerCatalogoPorGrupoAsync(nombreGrupo);
            return Ok(items);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> ObtenerRoles()
        {
            var roles = await queryService.ObtenerRolesSistemaAsync();
            return Ok(roles);
        }

        // ────────────────────────────────────────────────────────────
        // ESCRITURAS (CQRS: Van por el CommandService y el UnitOfWork)
        // ────────────────────────────────────────────────────────────

        [HttpPost("grupo")]
        public async Task<IActionResult> CrearGrupo([FromBody] CrearGrupoRequest request)
        {
            var resultado = await commandService.CrearGrupoAsync(
                request.NombreGrupo,
                request.Descripcion,
                request.EsSistema,
                request.UsuarioCreacion);

            if (!resultado.IsSuccess)
            {
                return BadRequest(new { Error = resultado.ErrorMessage });
            }

            return Ok(new { Mensaje = "Grupo creado", Id = resultado.Value });
        }

        [HttpGet("integrity-check")]
        public async Task<IActionResult> GetIntegrityReport()
        {
            var report = await _catalogoQueryService.CheckIntegrityAsync();

            return Ok(new
            {
                SystemTime = DateTime.Now,
                Status = report.Any() ? "Discrepancies Found" : "Healthy",
                report.Count,
                Data = report
            });
        }
    }

    // DTOs para las peticiones
    public record CrearGrupoRequest(string NombreGrupo, string Descripcion, bool EsSistema, string UsuarioCreacion);
    public record AgregarItemRequest(string NombreGrupo, string CodigoValor, string TextoVisual, int OrdenVisual, string UsuarioCreacion);

    // DTOs para catálogos de contactabilidad
    public class ErrorCodeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
