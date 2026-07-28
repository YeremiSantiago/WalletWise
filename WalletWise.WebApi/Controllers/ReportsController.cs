using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using WalletWise.Application.Dtos.Reports;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common.Enums;
using WalletWise.WebApi.Common;

namespace WalletWise.WebApi.Controllers
{
    [Authorize]
    [Route("api/reports")]
    [EnableRateLimiting("ReportsEndpoint")]
    [ApiController]
    [SwaggerTag("Operaciones para reportes y visualizaacion del historial financiero")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("summary")]
        [SwaggerOperation(
            Summary = "Resumen Global",
            Description = "Consultar el total de ingresos, gastos y balance acumulado del usuario.")]
        [ProducesResponseType(typeof(ReportSummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReportSummaryDto>> GetSummary()
        {
            var result = await _reportService.GetSummaryAsync();

            return result.ToOkResult(HttpContext);
        }

        [HttpGet("monthly")]
        [SwaggerOperation(Summary = "Resumen Mensual.",
           Description = "Generar un resumen que muestre ingresos, gastos y balance para cada mes de un año dado.")]
        [ProducesResponseType(typeof(IEnumerable<MonthlySummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MonthlySummaryDto>>> GetMonthly([FromQuery] int year)
        {
            var result = await _reportService.GetMonthlySummaryAsync(year);

            return result.ToOkResult(HttpContext);
        }

        [HttpGet("by-category")]
        [SwaggerOperation(
            Summary = "Reporte por categoría",
            Description = "Transacctiones agrupadas por categoría en un rango de fechas. Opcionalmente puede filtrarse por tipo.")]
        [ProducesResponseType(typeof(IEnumerable<CategoryReportItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryReportItemDto>>> GetByCategory([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] TypeTransaction? type)
        {
            var result = await _reportService.GetByCategoryAsync(start, end, type);

            return result.ToOkResult(HttpContext);
        }

        [HttpGet("top-categories")]
        [SwaggerOperation(
            Summary = "Top Categorías",
            Description = "Identificar y mostrar las categorías con mayor gasto en un período.")]
        [ProducesResponseType(typeof(IEnumerable<TopCategoryReportItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TopCategoryReportItemDto>>> GetTopCategories([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] int top = 5)
        {
            var result = await _reportService.GetTopCategoriesAsync(start, end, top);

            if (!result.IsSuccess)
                return result.ToOkResult(HttpContext);

            if (result.Value == null || !result.Value.Any())
                return Ok(new { message = "No hay datos de gastos en las categorías para el periodo de tiempo ingresado.", data = result.Value });

            return Ok(result.Value);
        }
        [HttpGet("comparison")]
        [SwaggerOperation(
            Summary = "Comparación Financiera",
            Description = "Comprobar el comportamiento financiero entre dos periodos (diferencia en ingresos, gastos y balance).")]
        [ProducesResponseType(typeof(ComparisonReportDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ComparisonReportDto>> GetComparison(
            [FromQuery] DateTime startA, [FromQuery] DateTime endA,
            [FromQuery] DateTime startB, [FromQuery] DateTime endB)
        {
            var result = await _reportService.GetComparisonAsync(startA, endA, startB, endB);

            return result.ToOkResult(HttpContext);
        }

        [HttpGet("export")]
        [SwaggerOperation(
            Summary = "Exportar Historial",
            Description = "Exportar las transacciones del historial de manera completa o filtrada en formato JSON.")]
        [ProducesResponseType(typeof(IEnumerable<TransactionResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> Export(
            [FromQuery] DateTime? start, [FromQuery] DateTime? end,
            [FromQuery] TypeTransaction? type, [FromQuery] int? categoryId,
            [FromQuery] string? search)
        {
            var result = await _reportService.ExportAsync(start, end, type, categoryId, search);

            if (!result.IsSuccess)
                return result.ToOkResult(HttpContext);

            if (result.Value == null || !result.Value.Any())
                return Ok(new { message = "No existen datos para exportar con los parámetros dados." });

            return Ok(result.Value);
        }
    }
}
