using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WalletWise.Application.Dtos.Budget;
using WalletWise.Application.Interfaces;
using WalletWise.WebApi.Common;

namespace WalletWise.WebApi.Controllers
{
    [SwaggerTag("Gestion y Creacion de Presupuestos")]
    [Route("api/budgets")]
    [Authorize]
    [ApiController]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear un presupuesto",
            Description = "Crea un presupuesto de gasto para una wallet y categoría dentro de un período. Solo categorías de tipo Expense. En modo Strict, no se permite si el gasto existente supera el monto.")]
        [ProducesResponseType(typeof(BudgetResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<BudgetResponseDto>> CreateAsync([FromBody] CreateBudgetRequestDto dto)
        {
            var response = await _budgetService.CreateBudgetAsync(dto);

            return response.ToCreatedResult(HttpContext, nameof(GetBudgetById), new { id = response.Value?.Id });
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Obtener un presupuesto por ID",
            Description = "Devuelve el presupuesto con sus valores calculados: gasto acumulado, monto disponible, porcentaje de uso y si fue excedido.")]
        [ProducesResponseType(typeof(BudgetResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BudgetResponseDto>> GetBudgetById([FromRoute] int id)
        {
            var response = await _budgetService.GetBudgetByIdAsync(id);

            return response.ToOkResult(HttpContext);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener todos los presupuestos",
            Description = "Devuelve todos los presupuestos del usuario autenticado con sus valores calculados.")]
        [ProducesResponseType(typeof(List<BudgetResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetAllBudgetsAsync()
        {
            var response = await _budgetService.GetAllBudgetsAsync();

            return response.ToOkResult(HttpContext);
        }

        [HttpGet("wallets/{walletId}")]
        [SwaggerOperation(
            Summary = "Obtener presupuestos por wallet",
            Description = "Devuelve todos los presupuestos asociados a una wallet específica, con sus valores calculados.")]
        [ProducesResponseType(typeof(List<BudgetResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetAllBudgetsForWalletAsync([FromRoute] int walletId)
        {
            var response = await _budgetService.GetBudgetsByWalletAsync(walletId);

            return response.ToOkResult(HttpContext);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Actualizar un presupuesto existente",
            Description = "Permite actualizar el monto y/o el modo. No se pueden modificar wallet, categoría ni fechas. En modo Strict, el nuevo monto no puede ser menor al gasto acumulado.")]
        [ProducesResponseType(typeof(BudgetResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<BudgetResponseDto>> UpdateBudgetAsync([FromRoute] int id, [FromBody] UpdateBudgetRequestDto dto) 
        {
            var response = await _budgetService.UpdateBudgetAsync(id, dto);

            return response.ToOkResult(HttpContext);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Eliminar un presupuesto",
            Description = "Elimina permanentemente un presupuesto. Las transacciones asociadas no se ven afectadas.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteBudgetAsync([FromRoute] int id)
        {
            var response = await _budgetService.DeleteBudgetAsync(id);

            return response.ToNoContentResult(HttpContext);
        }
    }
}
