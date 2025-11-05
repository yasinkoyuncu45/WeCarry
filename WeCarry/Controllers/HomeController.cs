using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WeCarry.Models;
using WeCarry.Models.MVVM;
using WeCarry.Services;
namespace WeCarry.Controllers
{
    public class HomeController : Controller
    {
       
       
        private readonly IUserRepository _userRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IAdsRepository _adsRepository;
        private readonly IEmailService _emailService;
       
        public HomeController(
           
            IUserRepository userRepository,
            ICityRepository cityRepository,
            IAdsRepository adsRepository,
            IEmailService emailService)
           
        {
           
            _userRepository = userRepository;
            _cityRepository = cityRepository;
            _adsRepository = adsRepository;
            _emailService = emailService;
           
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? service, string? from, string? to, string? city, int page = 1)
        {
            const int pageSize = 10;

            var query = _adsRepository.GetAll()
                .Include(a => a.User)
                .Include(a => a.ServiceType)
                .Where(a => a.IsActive);

            query = ApplyFilters(query, service, from, to, city);

            var totalCount = query.Count();

            var ads = query
    .OrderByDescending(a => a.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToList();

            var model = new MainPageModel
            {
                CityList = await _cityRepository.GetAllAsync(),
                UserList = await _userRepository.GetAllAsync(),
                Ads = ads,
                CurrentPage = page,
                TotalCount = totalCount
            };

            // ✅ Infinite Scroll için PartialView kontrolü
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_AdsPartial", model.Ads);

            return View(model);
        }


        // 🔍 Filtre Uygulama Metodu

        private static IQueryable<Ads> ApplyFilters(IQueryable<Ads> query, string? service, string? from, string? to, string? city)
        {
            // 🔹 Önce toplam kayıt sayısını kontrol et (çok büyükse EF.Functions.Like kullanılacak)
            int totalCount = query.Count();

            bool useClientFilter = totalCount < 1000; // 1000 altıysa AsEnumerable()

            if (useClientFilter)
            {
                // =====================================================
                // 🔸 KÜÇÜK VERİ (RAM’de filtreleme)
                // =====================================================
                var list = query
                    .Include(a => a.ServiceType)
                    .AsEnumerable(); // EF çevirme sorunu yaşamaz

                if (!string.IsNullOrEmpty(service))
                {
                    if (service.Equals("truck", StringComparison.OrdinalIgnoreCase))
                        list = list.Where(a => a.ServiceType?.Name?.Contains("Tır", StringComparison.OrdinalIgnoreCase) == true);
                    else if (service.Equals("tow", StringComparison.OrdinalIgnoreCase))
                        list = list.Where(a => a.ServiceType?.Name?.Contains("Çekici", StringComparison.OrdinalIgnoreCase) == true);
                }

                if (service?.Equals("truck", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (!string.IsNullOrEmpty(from))
                        list = list.Where(a => a.FoundCity.Contains(from, StringComparison.CurrentCultureIgnoreCase));

                    if (!string.IsNullOrEmpty(to))
                        list = list.Where(a => a.DestinationCity.Contains(to, StringComparison.CurrentCultureIgnoreCase));
                }

                if (service?.Equals("tow", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrEmpty(city))
                {
                    list = list.Where(a =>
                        a.FoundCity.Contains(city, StringComparison.CurrentCultureIgnoreCase) ||
                        a.DestinationCity.Contains(city, StringComparison.CurrentCultureIgnoreCase));
                }

                return list.AsQueryable(); // tekrar IQueryable döndür
            }
            else
            {
                // =====================================================
                // 🔸 BÜYÜK VERİ (veritabanı tarafında filtreleme)
                // =====================================================
                if (!string.IsNullOrEmpty(service))
                {
                    if (service.Equals("truck", StringComparison.OrdinalIgnoreCase))
                        query = query.Where(a => EF.Functions.Like(a.ServiceType.Name, "%Tır%"));
                    else if (service.Equals("tow", StringComparison.OrdinalIgnoreCase))
                        query = query.Where(a => EF.Functions.Like(a.ServiceType.Name, "%Çekici%"));
                }

                if (service?.Equals("truck", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (!string.IsNullOrEmpty(from))
                        query = query.Where(a => EF.Functions.Like(a.FoundCity, $"%{from}%"));
                    if (!string.IsNullOrEmpty(to))
                        query = query.Where(a => EF.Functions.Like(a.DestinationCity, $"%{to}%"));
                }

                if (service?.Equals("tow", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrEmpty(city))
                {
                    query = query.Where(a =>
                        EF.Functions.Like(a.FoundCity, $"%{city}%") ||
                        EF.Functions.Like(a.DestinationCity, $"%{city}%"));
                }

                return query;
            }
        }




        public IActionResult AboutUs()// hakkımızda
        {
            return View();
        }
        [HttpGet]
        public IActionResult Contact()//iletişim sayfası
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactFormDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // HTML formatlı body
            var body = $@"
        <p><b>Ad Soyad:</b> {model.Name}</p>
        <p><b>E-posta:</b> {model.Email}</p>
        <p><b>Telefon:</b> {model.Phone}</p>
        <p><b>Mesaj:</b><br/>{model.Message}</p>
        <hr/>
        <p style='font-size:12px;color:gray;'>Gönderim tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}</p>
    ";

            try
            {
                // 🔹 admin adresine gönder, reply-to kullanıcı olsun
                await _emailService.SendEmailAsync(
                    "Yeni İletişim Mesajı",
                    body,
                    "info@tasiyicin.com",  // senin admin mail adresin
                    model.Email            // reply-to: kullanıcının maili
                );

                TempData["Message"] = "Mesajınız başarıyla bize ulaştı. Teşekkür ederiz!";
                TempData["MessageType"] = "success";
                ModelState.Clear();
                return View(); // başarıyla aynı sayfaya dön
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Mesaj gönderilirken hata oluştu: " + ex.Message;
                TempData["MessageType"] = "danger";
                return View(model);
            }
        }
        public IActionResult Services()// hizmetlerimiz sayfası
        {
            return View();
        }

    }
}


