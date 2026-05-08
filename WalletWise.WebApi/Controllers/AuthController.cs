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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
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
        [EnableRateLimiting("AuthLoginIp")]
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
    }
}
