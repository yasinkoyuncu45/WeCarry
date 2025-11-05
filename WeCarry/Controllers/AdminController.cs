using Microsoft.AspNetCore.Mvc;
using WeCarry.Models;
using WeCarry.Models.MVVM;

namespace WeCarry.Controllers
{
    public class AdminController : Controller
    {
        // ------------------- DEPENDENCY INJECTION -------------------
        // Admin panelinde kullanıcı, ilan ve sohbet gibi verileri yöneteceğimiz için
        // ilgili repository’ler ve Context DI ile alınır.
        private readonly IWebHostEnvironment _env;              // Dosya yükleme işlemleri için
        private readonly Context _con;                          // Veritabanı bağlantısı
        private readonly IUserRepository _userRepository;       // Kullanıcı işlemleri
        private readonly ICityRepository _cityRepository;       // Şehir listesi işlemleri
        private readonly IAdsRepository _adsRepository;         // İlan işlemleri
        private readonly IConversationRepository _conversationRepository; // Sohbet işlemleri
        MainPageModel mpm = new MainPageModel();                // Görünüm için model

        // Constructor - bağımlılıkları alıyoruz
        public AdminController(
            IWebHostEnvironment env,
            Context con,
            IUserRepository userRepository,
            ICityRepository cityRepository,
            IAdsRepository adsRepository,
            IConversationRepository conversationRepository)
        {
            _env = env;
            _con = con;
            _userRepository = userRepository;
            _cityRepository = cityRepository;
            _adsRepository = adsRepository;
            _conversationRepository = conversationRepository;
        }

        // ------------------- ADMİN ANA SAYFASI -------------------
        public IActionResult Index()
        {
            // Admin panel ana ekranı (Dashboard)
            return View();
        }

        // ------------------- ADMİN ÇIKIŞ -------------------
        public IActionResult Logout()
        {
            // Oturumdaki tüm bilgileri temizle
            HttpContext.Session.Clear();
            TempData["Message"] = "Çıkış yapıldı.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Index", "Home");
        }

        // ------------------- KULLANICI LİSTESİ -------------------
        public IActionResult UserList()
        {
            // Tüm kullanıcıları getirip listeye gönderiyoruz
            List<User> users = _con.User.ToList();
            return View(users);
        }

        // ------------------- KULLANICI SİLME -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var ok = await _userRepository.DeleteUser(id);

            if (ok)
            {
                TempData["Message"] = "Kullanıcı silindi.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Silme işlemi başarısız.";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("UserList"); // Listeye geri dön
        }

        // ------------------- KULLANICI DÜZENLEME (GET) -------------------
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user); // Edit formu açılır
        }

        // ------------------- KULLANICI DÜZENLEME (POST) -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User model)
        {
            // Şifre alanını zorunluluk dışına alıyoruz
            ModelState.Remove(nameof(model.Password));

            if (!ModelState.IsValid)
                return View(model); // Hatalı formda yeniden göster

            var ok = await _userRepository.UpdateAsync(model);
            if (ok)
            {
                TempData["Message"] = "Kullanıcı başarıyla güncellendi.";
                TempData["MessageType"] = "success";
                return RedirectToAction("UserList");
            }

            TempData["Message"] = "Kullanıcı güncellenemedi.";
            TempData["MessageType"] = "danger";
            return View(model);
        }

        // ------------------- TÜM İLANLAR -------------------
        public IActionResult AdList()
        {
            // Tüm ilanları getirip listeye gönderiyoruz
            var ads = _adsRepository.GetAll();
            return View(ads);
        }

        // ------------------- İLAN DÜZENLEME (GET) -------------------
        [HttpGet]
        public async Task<IActionResult> EditAd(int id)
        {
            var ads = await _adsRepository.GetByIdAsync(id);
            if (ads == null)
                return NotFound();

            return View(ads);
        }

        // ------------------- İLAN DÜZENLEME (POST) -------------------
        [HttpPost]
        public async Task<IActionResult> EditAd(Ads ads)
        {
            var ok = await _adsRepository.UpdateAsync(ads);

            if (ok)
            {
                TempData["Message"] = "İlan başarıyla güncellendi.";
                TempData["MessageType"] = "success";
                return RedirectToAction("AdList");
            }

            TempData["Message"] = "İlan güncellenemedi.";
            TempData["MessageType"] = "danger";
            return View(ads);
        }

        // ------------------- İLAN SİLME -------------------
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ok = await _adsRepository.DeleteAsync(id);

            if (ok)
            {
                TempData["Message"] = "İlan başarıyla silindi.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "İlan silinemedi.";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("AdList");
        }

        // ------------------- TÜM SOHBETLER -------------------
        public async Task<IActionResult> Conversations()
        {
            // Sohbetlerin genel özet listesi
            var list = await _conversationRepository.GetAllSummariesAsync();
            return View(list);
        }

        // ------------------- SOHBET DETAYI -------------------
        public async Task<IActionResult> ConversationDetail(int id)
        {
            // Belirli bir sohbete ait detay ve mesajlar
            var conversation = await _conversationRepository.GetDetailAsync(id);
            if (conversation == null)
                return NotFound();

            // Mesajları tarihe göre sırala (eskiden yeniye)
            conversation.Messages = conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .ToList();

            return View(conversation);
        }
    }
}
