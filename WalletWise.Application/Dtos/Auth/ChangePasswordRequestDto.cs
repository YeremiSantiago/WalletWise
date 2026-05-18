using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Application.Dtos.Auth
{
    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña Debe tener minimo 8 caracteres")]
        public string NewPassword { get; set; }
    }
}
