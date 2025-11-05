using System.ComponentModel.DataAnnotations;

namespace WeCarry.Models.MVVM
{
    public class DriverUser
    {
        [Required(ErrorMessage = "Hizmet Türü seçimi zorunludur.")]
        [StringLength(50, ErrorMessage = "Hizmet türü en fazla 50 karakter olabilir.")]
        public string ServiceType { get; set; }

        [Required(ErrorMessage = "Ad zorunlu alan.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter aralığında olmalıdır.")]
        [RegularExpression(@"^[a-zA-ZÇçĞğİıÖöŞşÜü\s]+$", ErrorMessage = "Ad yalnızca harflerden oluşmalıdır.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad zorunlu alan.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter aralığında olmalıdır.")]
        [RegularExpression(@"^[a-zA-ZÇçĞğİıÖöŞşÜü\s]+$", ErrorMessage = "Soyad yalnızca harflerden oluşmalıdır.")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "E-posta zorunlu alan.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunlu alan.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Şifre en az 6, en fazla 50 karakter olmalıdır.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Telefon zorunlu alan.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarası tam 11 haneli olmalıdır.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Telefon numarası yalnızca rakamlardan oluşmalıdır.")]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "Firma adı zorunlu alan.")]
        [StringLength(100, ErrorMessage = "Firma adı en fazla 100 karakter olabilir.")]
        public string Firm { get; set; }

        [Required(ErrorMessage = "Adres zorunlu alan.")]
        [StringLength(250, MinimumLength = 10, ErrorMessage = "Adres en az 10, en fazla 250 karakter olmalıdır.")]
        public string InvoicesAddress { get; set; }
    }
}
