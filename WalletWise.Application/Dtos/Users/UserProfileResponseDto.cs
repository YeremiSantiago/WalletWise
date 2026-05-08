using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Application.Dtos.Users
{
    public class UserProfileResponseDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public bool IsActive { get; set; }
    }
}
