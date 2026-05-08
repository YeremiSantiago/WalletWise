using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WalletWise.Application.Dtos.Users;
using WalletWise.Application.Interfaces;

namespace WalletWise.WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [SwaggerTag("Operaciones de perfil de usuario")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("me")]
        [SwaggerOperation(Summary = "Consultar perfil",
            Description = "Devuelve el perfil del usuario autenticado")]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileResponseDto>> GetCurrentUserProfile()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return NotFound();

            var result = await _authService.GetCurrentUserProfile(userId);

            if (result.IsSuccess == false || result.Value is null)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }
                                        
    }
}
