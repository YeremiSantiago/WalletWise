using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common.Pagination;
using WalletWise.WebApi.Common;

namespace WalletWise.WebApi.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [EnableRateLimiting("AuthenticatedUserApi")]
    [Authorize]
    [SwaggerTag("Proporciorna operaciones para gestionar transacciones")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("summary")]
        [SwaggerOperation(
            Summary = "Obtener el resumen de las transacciones",
            Description = "Te devuelve: total ingresos, gastos y balance de las transacciones filtradas.")]
        public async Task<ActionResult<TransactionSummaryResponseDto>> GetSummary([FromQuery] TransactionFilterParams filterParams)
        {
            var response = await _transactionService.GetSummaryAsync(filterParams);

            return response.ToOkResult(HttpContext);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener todas las transacciones",
            Description = "Te devuelve todas las transacciones registradas existentes"
            )]
        [ProducesResponseType(typeof(List<TransactionResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TransactionResponseDto>>> GetAllTransactions([FromQuery] TransactionFilterParams filterParams)
        {
            var response = await _transactionService.GetPagedTransactionsAsync(filterParams);

            return response.ToOkResult(HttpContext);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Obtener una transaccion por ID",
            Description = "Devuelve la transaccion registrada asociada al identificador proporcionado, si existe.")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TransactionResponseDto),StatusCodes.Status200OK)]
        public async Task<ActionResult<TransactionResponseDto>> GetTransactionById(int id)
        {
            var response = await _transactionService.GetTransactionByIdAsync(id);

            return response.ToOkResult(HttpContext);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear una transaccion",
            Description = "Te permite crear una transaccion y te la devuelve cuando es creada exitosamente")]
        [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<TransactionResponseDto>> CreateTransaction([FromBody] CreateTransactionRequestDto requestDto)
        {
            var response = await _transactionService.CreateTransactionAsync(requestDto);

            return response.ToCreatedResult(HttpContext, nameof(GetTransactionById), new { id = response.Value?.Id });
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Actualizar una transaccion existente",
            Description = "Te permite actualizar una transaccion existente y cuando la operacion sale exitosa te la devuelve")]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TransactionResponseDto),StatusCodes.Status200OK)]
        public async Task<ActionResult<TransactionResponseDto>> UpdateTransaction(int id, [FromBody] UpdateTransactionRequestDto requestDto)
        {
            var response = await _transactionService.UpdateTransactionAsync(id, requestDto);

            return response.ToOkResult(HttpContext);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Borrar transaccion existente",
            Description = "Te permite borrar una transaccion existente")]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> DeleteTransaction(int id)
        {
            var response = await _transactionService.DeleteTransactionAsync(id);

            return response.ToNoContentResult(HttpContext);
        }
    }
}
