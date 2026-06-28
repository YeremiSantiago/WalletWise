using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WalletWise.Application.Dtos.Auth;
using WalletWise.Application.Dtos.Users;
using WalletWise.Application.Interfaces;
using WalletWise.WebApi.Common;

namespace WalletWise.WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [SwaggerTag("Operaciones de perfil de usuario")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;
        public UsersController(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        [HttpGet("me")]
        [SwaggerOperation(Summary = "Consultar perfil",
            Description = "Devuelve el perfil del usuario autenticado")]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileResponseDto>> GetCurrentUserProfile()
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _authService.GetCurrentUserProfile(userId);

            return result.ToOkResult(HttpContext);
        }

        [HttpPut("me")]
        [SwaggerOperation(Summary = "Actualizar nombre de perfil",
            Description = "Actualiza el nombre de perfil del usuario autenticado")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserProfileResponseDto>> UpdateProfile([FromBody] UpdateUserProfileRequestDto request)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _authService.UpdateProfileNameAsync(userId, request);

            return result.ToOkResult(HttpContext);
        }

    }
}
