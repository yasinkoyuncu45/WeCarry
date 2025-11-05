using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeCarry.Models.MVVM
{
    public class TowTruckDriver
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TowTruckDriverID { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunlu alandır.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Kullanıcı adı 2-20 karakter arasında olmalıdır.")]
        [RegularExpression(@"^[a-zA-ZğüşöçıİĞÜŞÖÇ\s]+$", ErrorMessage = "Kullanıcı adı sadece harflerden oluşabilir.")]
        [DisplayName("Kullanıcı Adı")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Kullanıcı soyadı zorunlu alandır.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Kullanıcı soyadı 2-20 karakter arasında olmalıdır.")]
        [RegularExpression(@"^[a-zA-ZğüşöçıİĞÜŞÖÇ\s]+$", ErrorMessage = "Kullanıcı soyadı sadece harflerden oluşabilir.")]
        [DisplayName("Kullanıcı Soyadı")]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Email zorunlu alandır.")]
        [StringLength(50, ErrorMessage = "Email en fazla 50 karakter olabilir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [DisplayName("Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunlu alandır.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Şifre 6-20 karakter arasında olmalıdır.")]
        [DataType(DataType.Password)]
        [DisplayName("Şifre")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Telefon zorunlu alandır.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarası tam 11 haneli olmalıdır.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Telefon sadece rakamlardan oluşmalıdır.")]
        [DisplayName("Telefon")]
        public string? Telephone { get; set; }

        [Required(ErrorMessage = "Firma adı zorunlu alandır.")]
        [StringLength(100, ErrorMessage = "Firma adı en fazla 100 karakter olabilir.")]
        [DisplayName("Firma Adı")]
        public string? Firm { get; set; }  

        [Required(ErrorMessage = "Fatura adresi zorunlu alandır.")]
        [StringLength(250, MinimumLength = 10, ErrorMessage = "Fatura adresi 10-250 karakter arasında olmalıdır.")]
        [DisplayName("Fatura Adresi")]
        public string? InvoicesAddress { get; set; }

        [DisplayName("Aktif/Pasif")]
        public bool Active { get; set; }

        public ICollection<Ads>? Ads { get; set; }
    }
}
