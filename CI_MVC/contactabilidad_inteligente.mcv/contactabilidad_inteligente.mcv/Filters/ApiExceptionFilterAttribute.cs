using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace contactabilidad_inteligente.mcv.Filters
{
    /// <summary>
    /// Catches unhandled exceptions in API controllers and returns a consistent JSON error response.
    /// Applied at controller level to avoid per-action generic exception catches.
    /// </summary>
    public sealed class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<ApiExceptionFilterAttribute>>();

            if (context.Exception is OperationCanceledException)
            {
                logger.LogInformation("Request cancelled.");
                context.Result = new StatusCodeResult(499);
                context.ExceptionHandled = true;
                return;
            }

            if (context.Exception is DbUpdateException or DbException)
            {
                logger.LogError(context.Exception, "Database error in API controller.");
                context.Result = new ObjectResult(new { success = false, message = "Error de base de datos." })
                {
                    StatusCode = 500
                };
                context.ExceptionHandled = true;
                return;
            }

            if (context.Exception is InvalidOperationException)
            {
                logger.LogError(context.Exception, "Invalid operation in API controller.");
                context.Result = new ObjectResult(new { success = false, message = "Error interno." })
                {
                    StatusCode = 500
                };
                context.ExceptionHandled = true;
                return;
            }

            logger.LogError(context.Exception, "Unhandled exception in API controller.");
            context.Result = new ObjectResult(new { success = false, message = "Error interno." })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }
}
