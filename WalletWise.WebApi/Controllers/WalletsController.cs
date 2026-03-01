using Microsoft.AspNetCore.Mvc;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Entities;

namespace WalletWise.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WalletResponseDto>>> GetAllWallets()
        {
            var wallets = await _walletService.GetAllAsync();

            return Ok(wallets.Value);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WalletResponseDto>> GetWalletById(int id)
        {
            var response = await _walletService.GetByIdAsync(id);

            if(response.Value is null)
            {
                return NotFound(response);
            }

            return Ok(response.Value);
        }

        [HttpPost]
        public async Task<ActionResult<WalletResponseDto>> CreateWallet([FromBody] CreateWalletRequestDto requestDto)
        {
            var response = await _walletService.CreateWalletAsync(requestDto);

            return CreatedAtAction(nameof(GetWalletById), new { id = response.Value.Id }, response);
        }

        [HttpPut("{id}")]
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
