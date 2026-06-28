using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WalletWise.Application.Dtos.Auth;
using WalletWise.Application.Dtos.Users;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Infrastructure.Context;
using WalletWise.Infrastructure.Settings;

namespace WalletWise.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;
        private readonly IdentityAppDbContext _context;

        public AuthService(UserManager<IdentityUser> userManager, IOptions<JwtSettings> jwtSetting, ILogger<AuthService> logger, IdentityAppDbContext context)
        {
            _userManager = userManager;
            _jwtSettings = jwtSetting.Value;
            _logger = logger;
            _context = context;
        }

        public async Task<Result<LoginResponseDto>> RegisterAsync(RegisterRequestDto register)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(register.Email);

                if (existingUser != null)
                {
                    return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_USER_ALREADY_EXISTS);
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
                    return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_VALIDATION);
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
                    return Result<LoginResponseDto>.Failure(errors);
                }

                _logger.LogInformation("usuario creado exitosamente");

                var token = await GenerateJwtTokenAsync(newUser);

                var refreshToken = new RefreshToken
                {
                    Token = GenerateSecureRefreshToken(),
                    UserId = newUser.Id,
                    CreationDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsUsed = false,
                    IsRevoked = false
                };

                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();

                return Result<LoginResponseDto>.Success(new LoginResponseDto
                {
                    Email = newUser.Email,
                    User = newUser.Email,
                    Token = token.Token,
                    Expiration = token.Expiration,
                    RefreshToken = refreshToken.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al registrar usuario {Email}", register.Email);
                return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNEXPECTED);
            }
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto login)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(login.Email);

                if (user is null)
                {
                    return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_INVALID_CREDENTIALS);
                }

                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_FORBIDDEN);
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, login.Password);

                if (!isPasswordValid)
                {
                    await _userManager.AccessFailedAsync(user);
                    return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_INVALID_CREDENTIALS);
                }

                await _userManager.ResetAccessFailedCountAsync(user);

                _logger.LogInformation("Login exitoso");

                var token = await GenerateJwtTokenAsync(user);

                var refreshToken = new RefreshToken
                {
                    Token = GenerateSecureRefreshToken(),
                    UserId = user.Id,
                    CreationDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsUsed = false,
                    IsRevoked = false,
                };

                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                return Result<LoginResponseDto>.Success(new LoginResponseDto
                {
                    User = user.Id,
                    Email = user.Email,
                    Token = token.Token,
                    Expiration = token.Expiration,
                    RefreshToken = refreshToken.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al iniciar sesión para {Email}", login.Email);
                return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNEXPECTED);
            }
        }

        public async Task<Result<UserProfileResponseDto>> GetCurrentUserProfile(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                    throw new WalletWise.Application.Exceptions.NotFoundException("Usuario no encontrado");

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
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario {UserId}", userId);
                return Result<UserProfileResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNEXPECTED);
            }
        }

        public async Task<Result<UserProfileResponseDto>> UpdateProfileNameAsync(string userId, UpdateUserProfileRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                    throw new WalletWise.Application.Exceptions.NotFoundException("Usuario no encontrado");

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
                    return Result<UserProfileResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_VALIDATION);
                }

                return await GetCurrentUserProfile(userId);
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error al actualizar nombre de perfil {UserId}", userId);
                return Result<UserProfileResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNEXPECTED);
            }
        }

        public async Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                    throw new WalletWise.Application.Exceptions.NotFoundException("Usuario no encontrado");

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al cambiar contrasea {UserId}: {Errors}", userId, errors);
                    return Result<bool>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_VALIDATION);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error al cambiar contrasea {UserId}", userId);
                return Result<bool>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNEXPECTED);
            }
        }

        public async Task<Result<bool>> LogoutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                    throw new WalletWise.Application.Exceptions.NotFoundException("Usuario no encontrado");

                var result = await _userManager.UpdateSecurityStampAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al cerrar sesin {UserId}: {Errors}", userId, errors);
                    return Result<bool>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNEXPECTED);
                }

                 await _context.RefreshTokens
                    .Where(x => x.UserId == userId)
                    .ExecuteDeleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error al cerrar sesin {UserId}", userId);
                return Result<bool>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNEXPECTED);
            }
        }

        private async Task<(string Token, string Expiration)> GenerateJwtTokenAsync(IdentityUser user)
        {
            var securityStamp = await _userManager.GetSecurityStampAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new("security_stamp", securityStamp ?? string.Empty)
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

        private string GenerateSecureRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var storedToken = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

            if (storedToken is null)
            {
                return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_NOT_FOUND);
            }

            if (storedToken.User is null)
            {
                return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_NOT_FOUND);
            }

            if (storedToken.IsUsed == true)
            {
                var tokensActives = await _context.RefreshTokens
                    .Where(t => t.UserId == storedToken.UserId)
                    .ToListAsync();

                await _context.RefreshTokens.Where(x => x.UserId == storedToken.UserId)
                    .ExecuteDeleteAsync();

                return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNAUTHORIZED);
            }

            if (storedToken.IsRevoked == true)
            {
                return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNAUTHORIZED);
            }

            if (storedToken.IsExpired == true)
            {
                return Result<LoginResponseDto>.Failure(WalletWise.Application.Common.BusinessErrorCodes.ERR_UNAUTHORIZED);

            }

            storedToken.IsUsed = true;
            _context.RefreshTokens.Update(storedToken);

            var jwt = await GenerateJwtTokenAsync(storedToken.User);

            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureRefreshToken(),
                UserId = storedToken.User.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false,
            };

            await _context.RefreshTokens.AddAsync(refreshToken);

            await _context.SaveChangesAsync();

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                User = storedToken.User.Id,
                Email = storedToken.User.Email,
                Token = jwt.Token,
                Expiration = jwt.Expiration,
                RefreshToken = refreshToken.Token
            });
        }
    }
}
