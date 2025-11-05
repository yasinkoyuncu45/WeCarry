using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WeCarry.Models.MVVM
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "Email zorunlu alan")]
        [StringLength(50, ErrorMessage = "Email en fazla 50 karakter olabilir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [DisplayName("Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunlu alan")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Şifre 6-20 karakter olmalı")]
        [DataType(DataType.Password)]
        [DisplayName("Şifre")]
        public string? Password { get; set; }


    }
}
