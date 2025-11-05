using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeCarry.Models.MVVM
{
    public class Ads
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("İlan ID")]
        public int AdsID { get; set; }

        [Required]
        [DisplayName("Kullanıcı ID")]
        public int UserID { get; set; }  // Foreign key - ilanı veren kullanıcı

        // Navigation property
        [ForeignKey("UserID")]
        [DisplayName("Kullanıcı")]
        public User User { get; set; }

        [Required]
        [DisplayName("Hizmet Türü ID")]
        public int ServiceTypeID { get; set; }

        // Navigation property
        [ForeignKey("ServiceTypeID")]
        [DisplayName("Hizmet Türü")]
        public ServiceType ServiceType { get; set; }

        [Required(ErrorMessage = "Çıkış noktası zorunludur.")]
        [StringLength(50, ErrorMessage = "Çıkış noktası en fazla 50 karakter olabilir.")]
        [DisplayName("Çıkış Noktası")]
        public string FoundCity { get; set; }

        [Required(ErrorMessage = "Varış noktası zorunludur.")]
        [StringLength(50, ErrorMessage = "Varış noktası en fazla 50 karakter olabilir.")]
        [DisplayName("Varış Noktası")]
        public string DestinationCity { get; set; }

        [Required(ErrorMessage = "KM Ücreti zorunludur.")]
        [Range(0.01, 1000.00, ErrorMessage = "KM Ücreti 0.01 ile 1000 arasında olmalıdır.")]
        [Column(TypeName = "decimal(10,2)")]
        [DisplayName("KM Ücreti")]
        public decimal KmFee { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        [DisplayName("Açıklama")]
        public string? AdvertisementText { get; set; }

        [DisplayName("Durum (Aktif/Pasif)")]
        public bool IsActive { get; set; } = true;

        [DisplayName("Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
