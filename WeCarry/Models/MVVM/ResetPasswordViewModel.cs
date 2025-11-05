using System.ComponentModel.DataAnnotations;

namespace WeCarry.Models.MVVM
{
    public class ResetPasswordViewModel
    {
        [Required] public string Token { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required, MinLength(6)] public string NewPassword { get; set; }
        [Required, Compare(nameof(NewPassword), ErrorMessage = "Şifreler uyuşmuyor.")] public string ConfirmPassword { get; set; }
    }
}
