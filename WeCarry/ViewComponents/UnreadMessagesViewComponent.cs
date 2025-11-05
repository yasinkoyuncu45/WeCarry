using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WeCarry.Models;

public class UnreadMessagesViewComponent : ViewComponent
{
    private readonly IMessageRepository _msgRepo;
    private readonly IHttpContextAccessor _http;

    // NEDEN IHttpContextAccessor? ViewComponent içinde Session'a direkt erişmek için.
    public UnreadMessagesViewComponent(IMessageRepository msgRepo, IHttpContextAccessor http)
    {
        _msgRepo = msgRepo;
        _http = http;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Oturum yoksa (misafir), 0 dön ve sade link gösterelim.
        var me = _http.HttpContext?.Session.GetInt32("UserId");
        var count = 0;
        if (me.HasValue)
        {
            // NEDEN repository? Veritabanından okunmamış sayısını tek sorgu ile almak için.
            count = await _msgRepo.CountUnreadForUserAsync(me.Value);
        }

        return View(count); // count => View'e model olarak gidiyor
    }
}
