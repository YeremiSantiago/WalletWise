using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Application.Dtos.Auth
{
    public class LoginResponseDto
    {
        public string User { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public string Expiration { get; set; }
    }
}
