using Microsoft.AspNetCore.SignalR;
using WeCarry.Services;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    // 1️⃣ Odaya katıl: Kullanıcı bu conversation için bir gruba eklenir
    public async Task JoinConversation(int conversationId)
    {
        var http = Context.GetHttpContext();
        var me = http?.Session.GetInt32("UserId");
        if (me == null)
            throw new HubException("Oturum bulunamadı.");

        // Kullanıcı bu sohbetin tarafı mı?
        var allowed = await _chatService.UserIsParticipantAsync(conversationId, me.Value);
        if (!allowed)
            throw new HubException("Yetkisiz erişim.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv-{conversationId}");
    }

    // 2️⃣ Mesaj gönderme: İstemciden gelir, ChatService'e aktarılır
    public async Task SendMessage(int conversationId, int senderUserId, string body)
    {
        var http = Context.GetHttpContext();
        var me = http?.Session.GetInt32("UserId");

        if (me == null || me.Value != senderUserId)
            throw new HubException("Kimlik doğrulama hatası.");

        if (string.IsNullOrWhiteSpace(body))
            throw new HubException("Mesaj boş olamaz.");

        // Mesajı DB’ye kaydedecek + gruba yayınlayacak servis
        await _chatService.SendAsync(conversationId, senderUserId, body.Trim());
    }

    // 3️⃣ Yazıyor bilgisi: diğer kullanıcıya "typing" bildirimi gönder
    public async Task Typing(int conversationId, int senderUserId)
    {
        var http = Context.GetHttpContext();
        var me = http?.Session.GetInt32("UserId");

        if (me == null || me.Value != senderUserId)
            return; // Sessizce reddet

        await Clients.OthersInGroup($"conv-{conversationId}")
            .SendAsync("UserTyping", new { conversationId, userId = senderUserId });
    }

    // (Opsiyonel) Bağlantı olaylarını izlemek için override edebilirsin
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Yeni bağlantı: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Bağlantı koptu: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }
}
