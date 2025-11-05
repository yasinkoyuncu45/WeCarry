using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WeCarry.Models;
using WeCarry.Models.MVVM;
using WeCarry.Services;

public class ChatService : IChatService
{
    private readonly Context _con;
    private readonly IMessageRepository _msgRepo;
    private readonly IConversationRepository _convRepo;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IEmailService _email; // Senin mevcut e-posta servisin

    public ChatService(
        Context con,
        IMessageRepository msgRepo,
        IConversationRepository convRepo,
        IHubContext<ChatHub> hub,
        IEmailService email)
    {
        _con = con;
        _msgRepo = msgRepo;
        _convRepo = convRepo;
        _hub = hub;
        _email = email;
    }

    public async Task SendAsync(int conversationId, int senderUserId, string body)
    {
        // 1) Kullanıcı doğrulama
        var allowed = await _convRepo.UserIsParticipantAsync(conversationId, senderUserId);
        if (!allowed)
            throw new InvalidOperationException("Yetkisiz işlem.");

        // 2) Sohbet durumu
        var conv = await _con.Conversations.FirstOrDefaultAsync(c => c.ConversationID == conversationId);
        if (conv == null)
            throw new InvalidOperationException("Sohbet bulunamadı.");
        if (conv.Status == ConversationStatus.Ended)
            throw new InvalidOperationException("Sohbet sonlandırılmış.");

        // 3) Mesaj validasyonu
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Mesaj boş olamaz.");
        if (body.Length > 2000)
            body = body.Substring(0, 2000);

        // 4) DB’ye kaydet
        var msg = await _msgRepo.AddAsync(conversationId, senderUserId, body.Trim());
        conv.LastMessageAt = msg.CreatedAt;
        await _con.SaveChangesAsync();

        // 5) Gönderen adı
        var senderName = await _con.User
            .Where(u => u.UserID == senderUserId)
            .Select(u => (u.Name + " " + u.Surname).Trim())
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(senderName))
            senderName = "Kullanıcı";

        // 6) SignalR yayını (sohbet odasına)
        await _hub.Clients.Group($"conv-{conversationId}").SendAsync("ReceiveMessage", new
        {
            conversationId,
            senderUserId,
            senderName,   // ✅ artık boş kalmaz
            body = msg.Body,
            createdAt = msg.CreatedAt.ToString("o") // ISO format
        });

        // 7) Karşı tarafı bul
        var receiverId = (senderUserId == conv.StarterUserID)
            ? conv.OwnerUserID
            : conv.StarterUserID;

        // 8) Navbar için genel bildirim
        await _hub.Clients.All.SendAsync("NewMessageForUser", new
        {
            conversationId,
            receiverUserId = receiverId,
            createdAt = msg.CreatedAt.ToString("o")
        });

        // 9) Opsiyonel e-posta bildirimi
        try
        {
            var link = $"https://tasiyicin.com/Chat/Room/{conversationId}";
            await _email.SendEmailToUserAsync(receiverId,
                "Taşıyıcım – Yeni mesajınız var",
                $"Merhaba, yeni bir mesaj aldınız. Görüntülemek için: {link}");
        }
        catch
        {
            // e-posta hatasını sessiz yutuyoruz
        }
    }


    public async Task<bool> UserIsParticipantAsync(int conversationId, int userId)
    {
        return await _con.Conversations
            .AnyAsync(c => c.ConversationID == conversationId &&
                           (c.StarterUserID == userId || c.OwnerUserID == userId));
    }
    public Task<int> MarkReadAsync(int conversationId, int meUserId)
    {
        return _msgRepo.MarkReadAsync(conversationId, meUserId);
    }
}
