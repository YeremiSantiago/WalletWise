using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Auth;
using WalletWise.Application.Dtos.Users;
using WalletWise.Domain.Common;

namespace WalletWise.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto login);
        Task<Result<LoginResponseDto>> RegisterAsync(RegisterRequestDto register);
        Task<Result<UserProfileResponseDto>> GetCurrentUserProfile(string userId);
    }
}
