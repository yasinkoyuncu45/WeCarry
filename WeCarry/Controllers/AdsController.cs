using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using WeCarry.Models;
using WeCarry.Models.MVVM;

namespace WeCarry.Controllers
{
    public class AdsController : Controller
    {
        // ---------------- DEPENDENCY INJECTION ----------------
        // Bu controller ilanlarla (Ads) ilgili tüm işlemleri yönetir.
        // Repository’ler kullanarak veritabanına erişim sağlanır.

        private readonly IAdsRepository _adsRepository;
        private readonly ICityRepository _cityRepository;
        private readonly Context _con;

        // Constructor: DI ile gerekli servisleri alıyoruz
        public AdsController(IAdsRepository adsRepository, ICityRepository cityRepository, Context con)
        {
            _adsRepository = adsRepository;
            _cityRepository = cityRepository;
            _con = con;
        }

        // ---------------- İLAN OLUŞTURMA (GET) ----------------
        [HttpGet]
        public async Task<IActionResult> AdEntry()
        {
            // Yeni ilan oluşturma sayfası (boş form)
            // Şehir listesini veritabanından çekip dropdown’a gönderiyoruz
            ViewBag.CityList = new SelectList(await _cityRepository.GetAllAsync(), "Name", "Name");
            return View();
        }

        // ---------------- İLAN OLUŞTURMA (POST) ----------------
        [HttpPost]
        public IActionResult AdEntry(Ads ads)
        {
            // Oturumdan kullanıcı bilgilerini al
            var userType = HttpContext.Session.GetString("UserType");
            var userId = HttpContext.Session.GetInt32("UserId");

            // Eğer kullanıcı giriş yapmamışsa
            if (string.IsNullOrEmpty(userType) || userId == null)
            {
                TempData["Message"] = "Oturum süresi dolmuş. Lütfen tekrar giriş yapınız.";
                return RedirectToAction("Login", "Account"); // AccountController’a yönlendir
            }

            // Repository aracılığıyla ilanı oluştur
            _adsRepository.AdsCreate(ads, userType, userId.Value);

            TempData["Message"] = "İlan başarıyla oluşturuldu.";
            TempData["MessageType"] = "success";

            // Ana sayfaya yönlendirme
            return RedirectToAction("Index", "Home");
        }

        // ---------------- KULLANICININ KENDİ İLANLARI ----------------
        public async Task<IActionResult> MyAds()
        {
            // Kullanıcı ID’sini session’dan al
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Bu kullanıcıya ait ilanları getir
            var ads = await _adsRepository.GetByUserIdAsync(userId.Value);

            // Sadece aktif olanları listele
            var activeAds = ads?.Where(a => a.IsActive).ToList() ?? new List<Ads>();

            return View(activeAds);
        }

        // ---------------- İLAN DÜZENLEME (GET) ----------------
        [HttpGet]
        public async Task<IActionResult> EditAd(int id)
        {
            // Düzenlenecek ilanı bul
            var ad = await _adsRepository.GetByIdAsync(id);
            if (ad == null) return NotFound();

            // Kullanıcı kendi ilanı dışında bir ilan düzenleyemesin
            var userId = HttpContext.Session.GetInt32("UserId");
            if (ad.UserID != userId)
                return Forbid(); // 403: yetkisiz işlem

            // Dropdown listeleri (şehir + hizmet türü)
            ViewBag.CityList = new SelectList(await _cityRepository.GetAllAsync(), "Name", "Name");
            ViewBag.ServiceTypes = new SelectList(_con.ServiceType, "ServiceTypeID", "Name");

            // EditAd.cshtml sayfasına Ads modelini gönderiyoruz
            return View(ad);
        }

        // ---------------- İLAN DÜZENLEME (POST) ----------------
        [HttpPost]
        public async Task<IActionResult> EditAd(Ads ad)
        {
            // Navigation property’ler validasyon dışında bırakılıyor
            ModelState.Remove("User");
            ModelState.Remove("ServiceType");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || ad.UserID != userId)
                return Forbid();

            // Eğer model geçerliyse güncelleme yapılır
            if (ModelState.IsValid)
            {
                var success = await _adsRepository.UpdateAsync(ad);

                if (success)
                {
                    TempData["Message"] = "İlan başarıyla güncellendi.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("MyAds");
                }

                // Repo false dönerse
                TempData["Message"] = "İlan güncellenemedi, lütfen tekrar deneyin.";
                TempData["MessageType"] = "danger";
            }
            else
            {
                TempData["Message"] = "Eksik veya hatalı alanlar var, lütfen kontrol edin.";
                TempData["MessageType"] = "warning";
            }

            // Hata olursa dropdown listeleri tekrar doldur
            ViewBag.CityList = new SelectList(await _cityRepository.GetAllAsync(), "Name", "Name");
            ViewBag.ServiceTypes = new SelectList(_con.ServiceType, "ServiceTypeID", "Name");
            return View(ad);
        }

        // ---------------- İLAN SİLME ----------------
        public async Task<IActionResult> DeleteAd(int id)
        {
            if (id != 0)
            {
                var success = await _adsRepository.DeleteAsync(id);

                if (success)
                    TempData["Message"] = "İlan başarıyla kaldırıldı.";
                else
                    TempData["Message"] = "Hata: ilan kaldırılamadı.";
            }

            return RedirectToAction("MyAds");
        }
    }
}
