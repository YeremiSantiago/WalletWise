using Microsoft.AspNetCore.Mvc;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Interfaces;

namespace WalletWise.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetAllTransactions()
        {
            var response = await _transactionService.GetAllAsync();

            return Ok(response.Value);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> GetTransactionById(int id)
        {
            var response = await _transactionService.GetByIdAsync(id);

            if (response.Value is null)
            {
                return NotFound();
            }

            return Ok(response.Value);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionResponseDto>> CreateTransaction([FromBody] CreateTransactionRequestDto requestDto)
        {
            var response = await _transactionService.CreateTransactionAsync(requestDto);

            return CreatedAtAction(nameof(GetTransactionById), new {id = response.Value.Id}, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> UpdateTransaction(int id, [FromBody] UpdateTransactionRequestDto requestDto)
        {
            var response = await _transactionService.UpdateTransactionAsync(id, requestDto);

            if (response.Value is null)
            {
                return NotFound();
            }

            return Ok(response.Value);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTransaction(int id)
        {
            var response = await _transactionService.DeleteTransactionAsync(id);

            if (response is null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
