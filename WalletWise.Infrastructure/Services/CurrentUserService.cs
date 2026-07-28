using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Interfaces;

namespace WalletWise.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccesor;

        public CurrentUserService(IHttpContextAccessor httpContextAccesor)
        {
            _httpContextAccesor = httpContextAccesor;
        }

        public string? UserId => _httpContextAccesor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}

