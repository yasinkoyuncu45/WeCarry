using Microsoft.AspNetCore.Mvc;
using WeCarry.Models;
using WeCarry.Models.MVVM;

public class ChatController : Controller
{
    // ---------------- DEPENDENCY INJECTION ----------------
    // Repository katmanları: veri işlemleri buradan yapılır.
    private readonly IConversationRepository _convRepo; // sohbet/oda yönetimi
    private readonly IAdsRepository _adsRepo;           // ilan bilgileri
    private readonly IMessageRepository _msgRepo;       // mesaj yönetimi

    // Constructor - repository bağımlılıklarını alıyoruz.
    public ChatController(
        IConversationRepository convRepo,
        IAdsRepository adsRepo,
        IMessageRepository msgRepo)
    {
        _convRepo = convRepo;
        _adsRepo = adsRepo;
        _msgRepo = msgRepo;
    }

    // ---------------- İLAN ÜZERİNDEN SOHBET BAŞLATMA ----------------
    // Kullanıcı "İletişime Geç" butonuna bastığında tetiklenir.
    public async Task<IActionResult> WithAd(int adId)
    {
        var me = HttpContext.Session.GetInt32("UserId");
        if (me == null)
            return RedirectToAction("Login", "Account"); // Giriş yapılmamışsa

        var ad = await _adsRepo.GetByIdAsync(adId);
        if (ad == null || !ad.IsActive)
            return NotFound(); // İlan yoksa veya pasifse hata döner

        // Kullanıcı kendi ilanıyla sohbet başlatamaz
        if (ad.UserID == me.Value)
        {
            TempData["Message"] = "Kendi ilanınızla sohbet başlatamazsınız.";
            return RedirectToAction("Inbox", "Chat");
        }

        // Sohbet varsa getirir, yoksa oluşturur
        var conv = await _convRepo.GetOrCreateAsync(adId, me.Value, ad.UserID);

        // Yeni veya mevcut konuşma odasına yönlendir
        return RedirectToAction("Room", new { id = conv.ConversationID });
    }

    // ---------------- SOHBET ODASI ----------------
    // Belirli bir ConversationID’ye ait sohbet ekranını açar.
    public async Task<IActionResult> Room(int id)
    {
        var me = HttpContext.Session.GetInt32("UserId");
        if (me == null)
            return RedirectToAction("Login", "Account");

        // ViewModel içinde: ilan bilgisi, katılımcılar, mesajlar vs.
        var vm = await _convRepo.GetRoomViewAsync(id, me.Value);
        if (vm == null)
            return NotFound(); // Kullanıcı yetkisizse veya sohbet yoksa

        return View(vm);
    }

    // ---------------- GİRİŞ KUTUSU (AKTİF / ARŞİV) ----------------
    public async Task<IActionResult> Inbox(string tab = "active")
    {
        var me = HttpContext.Session.GetInt32("UserId");
        if (me == null)
            return RedirectToAction("Login", "Account");

        // Hangi sekme açık (aktif / arşiv)
        ViewBag.Tab = tab;

        // Aktif sohbetler veya arşivlenmiş olanları getirir
        var list = await _convRepo.ListForUserAsync(me.Value, active: tab != "archived");
        return View(list);
    }

    // ---------------- SOHBETİ SONLANDIRMA (ARŞİVE TAŞI) ----------------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> End(int id)
    {
        var me = HttpContext.Session.GetInt32("UserId");
        if (me == null)
            return Unauthorized(); // 401

        // Kullanıcı bu sohbetin tarafı mı kontrol et
        var ok = await _convRepo.EndAsync(id, me.Value);
        if (!ok)
            return Forbid(); // 403 - yetkisizse

        TempData["Message"] = "Sohbet sonlandırıldı.";
        return RedirectToAction("Inbox");
    }

    // ---------------- OKUNMAMIŞ MESAJLARI OKUNDU İŞARETLE ----------------
    [HttpPost]
    public async Task<IActionResult> MarkRead(int conversationId)
    {
        var me = HttpContext.Session.GetInt32("UserId");
        if (me == null)
            return Unauthorized();

        // Kullanıcı bu sohbette mi?
        var allowed = await _convRepo.UserIsParticipantAsync(conversationId, me.Value);
        if (!allowed)
            return Forbid();

        // Okunmamış mesajları "okundu" yap
        var count = await _msgRepo.MarkReadAsync(conversationId, me.Value);

        // JSON döndür (AJAX için)
        return Ok(new { marked = count });
    }

    // ---------------- OKUNMAMIŞ MESAJ SAYISI (Header Gösterimi) ----------------
    [HttpGet]
    public async Task<IActionResult> UnreadCount()
    {
        var me = HttpContext.Session.GetInt32("UserId");
        if (me == null)
            return Json(new { count = 0 }); // Giriş yapılmamışsa 0 dön

        var n = await _msgRepo.CountUnreadForUserAsync(me.Value);
        return Json(new { count = n }); // sayıyı frontend'e döner
    }
}
