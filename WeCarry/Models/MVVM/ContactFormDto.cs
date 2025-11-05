using System.ComponentModel.DataAnnotations;

namespace WeCarry.Models
{
    public class ContactFormDto
    {
        [Required(ErrorMessage = "Ad soyad gerekli")]
        public string Name { get; set; }

        [Required(ErrorMessage = "E-posta gerekli")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon gerekli")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Mesaj gerekli")]
        public string Message { get; set; }
    }
}
