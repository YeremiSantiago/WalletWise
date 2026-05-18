using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Entities;

namespace WalletWise.WebApi.Controllers
{
    [Route("api/wallets")]
    [SwaggerTag("Proporciona operaciones CRUD para gestionar las wallets")]
    [Authorize]
    [ApiController]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary="Obtener todas las wallets existentes",
            Description="Te devuelve todas las wallets registradas existentes")]
        [ProducesResponseType(typeof(List<WalletResponseDto>),StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WalletResponseDto>>> GetAllWallets()
        {
            var wallets = await _walletService.GetAllAsync();

            return Ok(wallets.Value);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary="Obtiene una transaccion por ID",
            Description= "Devuelve la wallet registrada asociada al identificador proporcionado, si existe.")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(WalletResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<WalletResponseDto>> GetWalletById(int id)
        {
            var response = await _walletService.GetWalletByIdAsync(id);

            if(response.Value is null)
            {
                return NotFound(response);
            }

            return Ok(response.Value);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary="Crear wallet",
            Description="Te permite crear una wallet y te la devuelve si es creada existosamente")]
        [ProducesResponseType(typeof(WalletResponseDto),StatusCodes.Status201Created)]
        public async Task<ActionResult<WalletResponseDto>> CreateWallet([FromBody] CreateWalletRequestDto requestDto)
        {
            var response = await _walletService.CreateWalletAsync(requestDto);

            return CreatedAtAction(nameof(GetWalletById), new { id = response.Value.Id }, response.Value);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary="Actualizar wallet existente",
            Description="Te permite actualizar una wallet existe y te la devuelve si la operacion sale exitosa ")]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(WalletResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<WalletResponseDto>> UpdateWallet(int id, [FromBody] UpdateWalletRequestDto requestDto)
        {
            var response = await _walletService.UpdateWalletAsync(id, requestDto);

            if(response.Value == null)
            {
                return NotFound(response);
            }

            return Ok(response.Value);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary="Borrar wallet existente",
            Description="Te permite borrar una wallet existente")]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteWallet(int id)
        {
            var response = await _walletService.DeleteWalletAsync(id);

            if(response is null)
            {
                return NotFound(response);
            }

            return NoContent();
        }

    }
}
