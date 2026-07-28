using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Application.Dtos.Auth
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es valido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña Debe tener minimo 8 caracteres")]
        public string Password { get; set; }
        [Required(ErrorMessage = "La confirmacion de la contraseña es obligatoria")]
        [Compare("Password", ErrorMessage = "La contraseñas no coinciden ")]
        public string ConfirmPassword { get; set; }
    }
}
