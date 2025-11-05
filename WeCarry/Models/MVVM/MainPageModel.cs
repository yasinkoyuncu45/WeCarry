using System.ComponentModel.DataAnnotations;

namespace WeCarry.Models.MVVM
{
    public class MainPageModel
    {

        public IEnumerable<City> CityList { get; set; }
        public IEnumerable<User> UserList { get; set; }
        public IEnumerable<Ads> Ads { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string ForgotPasswordEmail { get; set; }
        
        // Pagination için

        public int TotalAds { get; set; }
        public int CurrentPage { get; set; }
        public int TotalCount { get; set; }

    }
}
