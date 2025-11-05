using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeCarry.Models.MVVM
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("ID")]
        public int UserID { get; set; }

        [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Ad sadece harf içerebilir.")]
        [Required(ErrorMessage = "Ad Zorunlu Alan")]
        [DisplayName("Ad")]
        [StringLength(20)]
        public string Name { get; set; }

        [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Ad sadece harf içerebilir.")]
        [Required(ErrorMessage = "Soyad Zorunlu Alan")]
        [DisplayName("Soyad")]
        [StringLength(20)]
        public string Surname { get; set; }


        [Required(ErrorMessage = "Email Zorunlu Alan")]
        [StringLength(50)]
        [EmailAddress]
        public string Email { get; set; }


        [Required(ErrorMessage = "Şifre Zorunlu Alan")]
        [DisplayName("Şifre")]
        [StringLength(200)] // Hash uzun olacağı için 200 
        public string Password { get; set; }

        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Telefon sadece 11 haneli rakam olmalıdır.")]
        [Required(ErrorMessage = "Telefon No Zorunlu Alan")]
        [DisplayName("Telefon No")]
        [StringLength(11)]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "Adres Zorunlu Alan")]
        [DisplayName("Adres")]
        [StringLength(250)]
        public string? Address { get; set; }

        [DisplayName("Firma Adı")]
        [StringLength(100)]
        public string? Firm { get; set; }

        [Required(ErrorMessage = "Kullanıcı Türü Zorunlu Alan")]
        [DisplayName("Kullanıcı Türü")]
        public int UserTypeID { get; set; }
        [Required(ErrorMessage = "Hizmet Türü Zorunlu Alan")]
        [DisplayName("Hizmet Türü")]
        public int ServiceTypeID { get; set; }
        [DisplayName("Aktif/Pasif")]
        public bool Active { get; set; } = false;   // 🔹 Artık kullanıcılar varsayılan olarak pasif kaydedilecek

        [StringLength(100)]
        [DisplayName("Aktivasyon Kodu")]
        public string? ActivationCode { get; set; } // 🔹 Aktivasyon için benzersiz kod

        public string? ResetToken { get; set; }// şifre sıfırlama için
        public DateTime? ResetTokenExpires { get; set; } 

    }

}
