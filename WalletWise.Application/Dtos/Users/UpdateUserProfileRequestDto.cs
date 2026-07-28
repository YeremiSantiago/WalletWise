using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Application.Dtos.Users
{
    public class UpdateUserProfileRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
    }
}
