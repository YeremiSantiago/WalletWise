using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using WalletWise.Application.Dtos.Auth;
using WalletWise.Application.Interfaces;

namespace WalletWise.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [SwaggerTag("Autenticacion y autorizacion de usuarios")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        [HttpPost("register")]
        [EnableRateLimiting("AuthRegisterByIp")]
        [SwaggerResponse(StatusCodes.Status201Created, "Usario registrado Exitosamente", typeof(LoginResponseDto))]
        [SwaggerOperation(
            Summary = "Registar nuevo usuario",
            Description="Crea una cuenta de usuario y devuelve un token JWT." +
            "La contraseña debe tener minimo 8 caracteres, una mayuscula, una minuscula y dígito ")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }

            return Created(string.Empty, result.Value);
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthLoginByIp")]
        [SwaggerOperation(
            Summary = "Autenticar usuario",
            Description = "Autentica el usuario ingresando el correo y contraseña y devuelve un token JWT")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
            {
                return Unauthorized(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpPut("me/password")]
        [SwaggerOperation(Summary = "Cambiar contraseña",
            Description = "Actualiza la contraseña del usuario autenticado")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return NotFound();
            }

          

            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }

        [Authorize]
        [HttpPost("logout")]
        [SwaggerOperation(Summary = "Cerrar sesión",
            Description = "Cierra la sesión e invalida el token JWT actual")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout()
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _authService.LogoutAsync(userId);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}
