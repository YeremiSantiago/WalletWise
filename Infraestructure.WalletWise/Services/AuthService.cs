using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Auth;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Setting;
using WalletWise.Application.Dtos.Users;
using System.Security.Claims;

namespace WalletWise.Infraestructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<IdentityUser> userManager, IOptions<JwtSettings> jwtSetting, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwtSettings = jwtSetting.Value;
            _logger = logger;
        }

        public async Task<Result<LoginResponseDto>> RegisterAsync(RegisterRequestDto register)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(register.Email);

                if (existingUser != null)
                {
                    return Result<LoginResponseDto>.Failure("Ese correo ya se encuentra registrado");
                }

                var newUser = new IdentityUser
                {
                    Email = register.Email,
                    UserName = register.Email
                };

                var createResult = await _userManager.CreateAsync(newUser, register.Password);

                if (createResult.Succeeded == false)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al registrar usuario {Email}: {Errors}", register.Email, errors);
                    return Result<LoginResponseDto>.Failure(errors);
                }

                var claims = new List<Claim>
                {
                    new("profile_name", register.Name),
                    new("registered_at", DateTime.UtcNow.ToString("O"))
                };

                var claimResult = await _userManager.AddClaimsAsync(newUser, claims);
                if (!claimResult.Succeeded)
                {
                    var errors = string.Join(", ", claimResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al registrar claims del usuario {Email}: {Errors}", register.Email, errors);
                    return Result<LoginResponseDto>.Failure("No se pudo registrar el perfil del usuario");
                }

                _logger.LogInformation("usuario creado exitosamente");

                var token = await GenerateJwtTokenAsync(newUser);

                return Result<LoginResponseDto>.Success(new LoginResponseDto
                {
                    Email = newUser.Email,
                    User = newUser.Email,
                    Token = token.Token,
                    Expiration = token.Expiration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al registrar usuario {Email}", register.Email);
                return Result<LoginResponseDto>.Failure("Ha ocurrido un error al registrar el usuario");
            }
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto login)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(login.Email);

                if (user is null)
                {
                    return Result<LoginResponseDto>.Failure("Las credenciales ingresadas son invalidas");
                }

                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Result<LoginResponseDto>.Failure("La cuenta se encuentra bloqueada. Intente mas tarde");
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, login.Password);

                if (!isPasswordValid)
                {
                    await _userManager.AccessFailedAsync(user);
                    return Result<LoginResponseDto>.Failure("Credenciales invalidas");
                }

                await _userManager.ResetAccessFailedCountAsync(user);

                _logger.LogInformation("Login exitoso");

                var token = await GenerateJwtTokenAsync(user);

                return Result<LoginResponseDto>.Success(new LoginResponseDto
                {
                    User = user.Id,
                    Email = user.Email,
                    Token = token.Token,
                    Expiration = token.Expiration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al iniciar sesión para {Email}", login.Email);
                return Result<LoginResponseDto>.Failure("Ha ocurrido un error al iniciar sesión");
            }
        }

        public async Task<Result<UserProfileResponseDto>> GetCurrentUserProfile(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                    return Result<UserProfileResponseDto>.Failure("Usuario no encontrado");

                var claims = await _userManager.GetClaimsAsync(user);

                var name = claims.FirstOrDefault(c => c.Type == "profile_name")?.Value;
                var registeredAtClaim = claims.FirstOrDefault(c => c.Type == "registered_at")?.Value;

                DateTime? registeredAt = null;

                if (DateTime.TryParse(registeredAtClaim, out var parsed))
                {
                    registeredAt = parsed;
                }

                var isActive = user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow;

                var response = new UserProfileResponseDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Name = name,
                    RegisteredAt = registeredAt,
                    IsActive = isActive
                };

                return Result<UserProfileResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario {UserId}", userId);
                return Result<UserProfileResponseDto>.Failure("No se pudo obtener el perfil del usuario");
            }
        }

        public async Task<Result<UserProfileResponseDto>> UpdateProfileNameAsync(string userId, UpdateUserProfileRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                    return Result<UserProfileResponseDto>.Failure("Usuario no encontrado");

                var claims = await _userManager.GetClaimsAsync(user);
                var currentNameClaim = claims.FirstOrDefault(c => c.Type == "profile_name");

                IdentityResult result;

                if (currentNameClaim is null)
                {
                    result = await _userManager.AddClaimAsync(user, new Claim("profile_name", request.Name));
                }
                else
                {
                    result = await _userManager.ReplaceClaimAsync(user, currentNameClaim, new Claim("profile_name", request.Name));
                }

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al actualizar nombre de perfil {UserId}: {Errors}", userId, errors);
                    return Result<UserProfileResponseDto>.Failure("No se pudo actualizar el nombre del perfil");
                }

                return await GetCurrentUserProfile(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar nombre de perfil {UserId}", userId);
                return Result<UserProfileResponseDto>.Failure("No se pudo actualizar el nombre del perfil");
            }
        }

        public async Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                    return Result<bool>.Failure("Usuario no encontrado");

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al cambiar contraseña {UserId}: {Errors}", userId, errors);
                    return Result<bool>.Failure(errors);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña {UserId}", userId);
                return Result<bool>.Failure("No se pudo cambiar la contraseña");
            }
        }

        public async Task<Result<bool>> LogoutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                    return Result<bool>.Failure("Usuario no encontrado");

                var result = await _userManager.UpdateSecurityStampAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al cerrar sesión {UserId}: {Errors}", userId, errors);
                    return Result<bool>.Failure("No se pudo cerrar sesión");
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión {UserId}", userId);
                return Result<bool>.Failure("No se pudo cerrar sesión");
            }
        }

        private async Task<(string Token, string Expiration)> GenerateJwtTokenAsync(IdentityUser user)
        {
            var securityStamp = await _userManager.GetSecurityStampAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new("security_stamp", securityStamp)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

            var token = new JwtSecurityToken(
               issuer: _jwtSettings.Issuer,
               audience: _jwtSettings.Audience,
               claims: claims,
               expires: expiration,
               signingCredentials: credentials
           );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration.ToString("O"));
        }
    }
}
