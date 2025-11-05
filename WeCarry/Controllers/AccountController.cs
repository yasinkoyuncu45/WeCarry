using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using WeCarry.Models;
using WeCarry.Models.MVVM;
using WeCarry.Services;

namespace WeCarry.Controllers
{
    public class AccountController : Controller
    {
        // ------------------- DEPENDENCY INJECTION -------------------
        // Dış servisler ve repository'ler controller içine constructor üzerinden alınır.
        // Böylece controller, doğrudan veritabanına değil, aracı katmanlara erişir.

        private readonly IUserRepository _userRepository;      // Kullanıcı işlemleri için
        private readonly IEmailService _emailService;          // Mail gönderimi için
        private readonly IConfiguration _config;               // appsettings.json erişimi için
        private readonly IPasswordHasher<User> _passwordHasher;// Şifreleme işlemleri için
        private readonly Context _con;                         // Bazı direkt DB işlemleri için

        // Constructor — bağımlılıkları (servisleri) alıyoruz.
        public AccountController(
            IUserRepository userRepository,
            IEmailService emailService,
            IConfiguration config,
            IPasswordHasher<User> passwordHasher,
            Context con)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _config = config;
            _passwordHasher = passwordHasher;
            _con = con;
        }

        // ------------------- KULLANICI KAYIT (REGISTER) -------------------

        [HttpGet]
        public IActionResult Register()
        {
            // GET method — yalnızca boş formu görüntüler.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            // Normal kullanıcı kaydı (örneğin müşteri)
            user.ServiceTypeID = 3;
            ModelState.Remove(nameof(user.ServiceTypeID));

            // Eğer firma ismi girilmediyse varsayılan metin atanır
            if (string.IsNullOrWhiteSpace(user.Firm))
                user.Firm = "Firma Yok/Kullanıcı";

            // Validation başarısızsa formu yeniden göster
            if (!ModelState.IsValid)
                return View(user);

            // Aynı mail veya telefon daha önce kayıtlı mı kontrol et
            bool exists = await _userRepository.RegistrationCheck(user.Email, user.Telephone);
            if (!exists)
            {
                // Kullanıcı pasif oluşturulur, e-posta aktivasyonu bekler
                user.Active = false;
                user.ActivationCode = Guid.NewGuid().ToString();

                // Kullanıcı veritabanına eklenir
                bool added = _userRepository.AddUser(user);
                if (added)
                {
                    // Aktivasyon linki oluşturulur (BaseUrl config’ten alınır)
                    var domain = _config["BaseUrl"];
                    if (string.IsNullOrEmpty(domain))
                        domain = $"{Request.Scheme}://{Request.Host}";

                    var activationLink = $"{domain}/Home/Activate?code={user.ActivationCode}";

                    // Kullanıcıya gönderilecek HTML formatlı mail
                    var body = $@"
                    <div style='font-family:Arial, sans-serif; font-size:14px;'>
                        <p>Merhaba <b>{user.Name}</b>,</p>
                        <p>Hesabınızı aktifleştirmek için aşağıdaki linke tıklayın:</p>
                        <p><a href='{activationLink}' 
                              style='background:#007bff;color:white;padding:10px 15px;text-decoration:none;border-radius:5px;'>
                              Hesabımı Aktifleştir
                           </a></p>
                        <br/>
                        <p>Teşekkürler,<br/>Taşıyıcım Ekibi</p>
                    </div>";

                    // Mail gönderimi
                    await _emailService.SendEmailAsync("Hesap Aktivasyonu", body, user.Email);

                    TempData["Message"] = "Kaydınız başarılı! Lütfen e-posta adresinizi kontrol edin.";
                    TempData["MessageType"] = "info";
                    return RedirectToAction("Login");
                }

                TempData["Message"] = "Hata oluştu. Lütfen tekrar deneyiniz.";
                TempData["MessageType"] = "danger";
            }
            else
            {
                // Aynı e-posta veya numara zaten kayıtlıysa uyarı
                TempData["Message"] = "Bu mail ve/veya telefon zaten kayıtlı.";
                TempData["MessageType"] = "danger";
            }

            return View(user);
        }

        // ------------------- TAŞIYICI KAYIT -------------------

        [HttpGet]
        public IActionResult CarrierRegistration()
        {
            // Taşıyıcı kayıt sayfasını gösterir.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CarrierRegistration(User user)
        {
            // Model doğrulaması
            if (ModelState.IsValid)
            {
                bool exists = await _userRepository.RegistrationCheck(user.Email, user.Telephone);
                if (!exists)
                {
                    user.Active = false;
                    user.ActivationCode = Guid.NewGuid().ToString();

                    bool added = _userRepository.AddUser(user);
                    if (added)
                    {
                        // BaseUrl ayarını config’ten al, yoksa runtime host’tan üret
                        var domain = _config["BaseUrl"];
                        if (string.IsNullOrEmpty(domain))
                            domain = $"{Request.Scheme}://{Request.Host}";

                        var activationLink = $"{domain}/Home/Activate?code={user.ActivationCode}";

                        // Taşıyıcılara özel mail
                        var body = $@"
                        <div style='font-family:Arial, sans-serif; font-size:14px;'>
                            <p>Merhaba <b>{user.Name}</b>,</p>
                            <p>Taşıyıcı kaydınızı tamamlamak için aşağıdaki linke tıklayın:</p>
                            <p><a href='{activationLink}' 
                                  style='background:#28a745;color:white;padding:10px 15px;text-decoration:none;border-radius:5px;'>
                                  Hesabımı Aktifleştir
                               </a></p>
                            <br/>
                            <p>Teşekkürler,<br/>Taşıyıcım Ekibi</p>
                        </div>";

                        await _emailService.SendEmailAsync("Taşıyıcı Hesap Aktivasyonu", body, user.Email);

                        TempData["Message"] = "Kayıt başarılı! Aktivasyon linki e-posta adresinize gönderildi.";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Login");
                    }
                }
                else
                {
                    TempData["Message"] = "Bu Email veya Numara zaten kayıtlı.";
                    TempData["MessageType"] = "danger";
                }
            }

            return View(user);
        }

        // ------------------- GİRİŞ (LOGIN) -------------------

        [HttpGet]
        public IActionResult Login()
        {
            // Boş login sayfası döndürür.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            // E-posta veya şifre boşsa uyarı ver
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                TempData["Message"] = "Email ve şifre zorunludur.";
                TempData["MessageType"] = "warning";
                return View(user);
            }

            // Giriş kontrolü: repository metodu tuple döner (Id, Name, Type, Active)
            var (userId, name, userType, isActive) = await _userRepository.LoginCheckAsync(user.Email, user.Password);

            if (string.IsNullOrWhiteSpace(userType))
            {
                TempData["Message"] = "Email/Şifre yanlış.";
                TempData["MessageType"] = "danger";
                return View(user);
            }

            if (!isActive)
            {
                TempData["Message"] = "Hesabınız aktif değil. Aktivasyon linkini kontrol edin.";
                TempData["MessageType"] = "warning";
                return View(user);
            }

            // Session değişkenlerini set et
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("UserName", name);
            HttpContext.Session.SetString("UserType", userType.Trim());

            TempData["Message"] = $"Hoş geldin {name}, giriş başarılı.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Index", "Home");
        }

        // ------------------- ÇIKIŞ (LOGOUT) -------------------

        public IActionResult Logout()
        {
            // Tüm session bilgilerini temizler
            HttpContext.Session.Clear();
            TempData["Message"] = "Çıkış yapıldı.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Index", "Home");
        }

        // ------------------- ŞİFRE SIFIRLAMA -------------------

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            // Şifre sıfırlama formu
            return View(new MainPageModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([Bind("ForgotPasswordEmail")] MainPageModel model)
        {
            // Gereksiz property’leri ModelState’ten çıkar
            ModelState.Remove(nameof(model.Ads));
            ModelState.Remove(nameof(model.CityList));
            ModelState.Remove(nameof(model.UserList));

            if (!ModelState.IsValid)
                return View(model);

            var email = model.ForgotPasswordEmail?.Trim();
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(nameof(model.ForgotPasswordEmail), "E-posta zorunludur.");
                return View(model);
            }

            var user = await _con.User.FirstOrDefaultAsync(u => u.Email == email);

            // Her durumda aynı mesaj → güvenlik için (email var mı bilinmez)
            ViewData["Message"] = "Eğer bu e-posta sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderildi.";

            if (user == null)
                return View(new MainPageModel());

            // 32 byte rastgele token üret
            user.ResetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _con.SaveChangesAsync();

            // Link oluştur
            var domain = _config["BaseUrl"];
            if (string.IsNullOrEmpty(domain))
                domain = $"{Request.Scheme}://{Request.Host}";

            var resetLink = $"{domain}/Home/ResetPassword?token={WebUtility.UrlEncode(user.ResetToken)}&email={WebUtility.UrlEncode(user.Email)}";

            var expireLocal = TimeZoneInfo.ConvertTimeFromUtc(
                user.ResetTokenExpires.Value,
                TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
            );

            var body = $@"
                Merhaba {user.Name},
                <br/><br/>
                Şifrenizi sıfırlamak için <a href='{resetLink}'>buraya tıklayın</a>.
                <br/><br/>
                Bu bağlantı {expireLocal:dd.MM.yyyy HH:mm} tarihine kadar geçerlidir.
                <br/><br/>
                Eğer bu talebi siz yapmadıysanız, bu maili yok sayabilirsiniz.
            ";

            try
            {
                await _emailService.SendEmailAsync("Şifre Sıfırlama Bağlantınız", body, user.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mail gönderim hatası: " + ex.Message);
            }

            return View(new MainPageModel());
        }

        // ------------------- ŞİFRE YENİLEME -------------------

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return BadRequest("Geçersiz bağlantı.");

            var user = await _con.User.FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.ResetToken == token &&
                u.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Bağlantı geçersiz veya süresi dolmuş.");

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _con.User.FirstOrDefaultAsync(u =>
                u.Email == model.Email &&
                u.ResetToken == model.Token &&
                u.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", "Bağlantı geçersiz veya süresi dolmuş.");
                return View(model);
            }

            // Yeni şifreyi hashleyip kaydet
            user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpires = null;
            user.Active = true;
            await _con.SaveChangesAsync();

            TempData["Success"] = "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        // ------------------- HESAP AKTİVASYONU -------------------

        [HttpGet]
        public async Task<IActionResult> Activate(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                TempData["Message"] = "Aktivasyon kodu geçersiz.";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Login");
            }

            var user = await _con.User.FirstOrDefaultAsync(u => u.ActivationCode == code);
            if (user == null)
            {
                TempData["Message"] = "Aktivasyon kodu bulunamadı.";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Login");
            }

            // Hesap aktif hale getirilir
            user.Active = true;
            user.ActivationCode = null;
            await _con.SaveChangesAsync();

            TempData["Message"] = "Hesabınız başarıyla aktifleştirildi. Şimdi giriş yapabilirsiniz.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Login");
        }

        // ------------------- PROFİL GÖRÜNTÜLEME VE DÜZENLEME -------------------

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null) return RedirectToAction("Login", "Home");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int id, string Firm, string Telephone, string CurrentPass, string? NewPass, string? ConfirmPass)
        {
            // Şifre tekrarları uyuşmuyorsa hata
            if (!string.IsNullOrWhiteSpace(NewPass) && NewPass != ConfirmPass)
            {
                ModelState.AddModelError("", "Yeni şifre ile tekrarı uyuşmuyor!");
                var user = await _userRepository.GetByIdAsync(id);
                return View(user);
            }

            // Kullanıcı profilini güncelle (repository aracılığıyla)
            var success = await _userRepository.UpdateProfileAsync(id, Firm, Telephone, CurrentPass, NewPass);

            if (!success)
            {
                ModelState.AddModelError("", "Eski şifre yanlış veya güncelleme başarısız!");
                var user = await _userRepository.GetByIdAsync(id);
                return View(user);
            }

            TempData["Message"] = "Profil başarıyla güncellendi.";
            return RedirectToAction("Profile");
        }
    }
}
