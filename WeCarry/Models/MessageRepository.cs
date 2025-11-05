using Microsoft.EntityFrameworkCore;
using WeCarry.Models;
using WeCarry.Models.MVVM;

public class MessageRepository : IMessageRepository
{
    private readonly Context _con;

    public MessageRepository(Context con)
    {
        _con = con;
    }

    // ------------------------------------------------------------
    // 1️⃣ Yeni mesaj ekleme
    // ------------------------------------------------------------
    public async Task<Message?> AddAsync(int conversationId, int senderUserId, string body)
    {
        try
        {
            // İlgili sohbeti bul
            var conv = await _con.Conversations.FirstOrDefaultAsync(c => c.ConversationID == conversationId);
            if (conv == null)
                throw new InvalidOperationException("Sohbet bulunamadı.");

            if (conv.Status == ConversationStatus.Ended)
                throw new InvalidOperationException("Sohbet sonlandırılmış.");

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Mesaj boş olamaz.");

            var msg = new Message
            {
                ConversationID = conversationId,
                SenderUserID = senderUserId,
                Body = body.Trim(),
                CreatedAt = DateTime.UtcNow // UTC kullanımı → tutarlı saat kayıtları
            };

            // Mesaj ekleniyor
            _con.Messages.Add(msg);

            // Sohbet sıralamasında yukarıda gözüksün diye son mesaj zamanı güncellenir
            conv.LastMessageAt = msg.CreatedAt;

            await _con.SaveChangesAsync();
            return msg;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("⚠️ Message AddAsync - Geçersiz işlem: " + ex.Message);
            return null; // Üst katmanda kullanıcıya uygun mesaj gösterilebilir
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Message AddAsync Hatası: " + ex.Message);
            return null;
        }
    }

    // ------------------------------------------------------------
    // 2️⃣ Mesajları okundu olarak işaretle
    // ------------------------------------------------------------
    public async Task<int> MarkReadAsync(int conversationId, int meUserId)
    {
        try
        {
            // Karşı tarafın gönderdiği ve henüz okunmamış mesajlar
            var toRead = await _con.Messages
                .Where(m => m.ConversationID == conversationId &&
                            m.SenderUserID != meUserId &&
                            !m.IsRead)
                .ToListAsync();

            if (toRead.Count == 0)
                return 0; // Zaten okunmuş

            foreach (var m in toRead)
                m.IsRead = true;

            await _con.SaveChangesAsync();
            return toRead.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ MarkReadAsync Hatası: " + ex.Message);
            return 0;
        }
    }

    // ------------------------------------------------------------
    // 3️⃣ Kullanıcının toplam okunmamış mesaj sayısı
    // ------------------------------------------------------------
    public async Task<int> CountUnreadForUserAsync(int meUserId)
    {
        try
        {
            // Kullanıcının dahil olduğu tüm sohbetlerin ID’leri
            var myConvs = _con.Conversations
                .Where(c => c.StarterUserID == meUserId || c.OwnerUserID == meUserId)
                .Select(c => c.ConversationID);

            // Karşı tarafın gönderdiği ve okunmamış mesajların sayısı
            var count = await _con.Messages
                .Where(m => myConvs.Contains(m.ConversationID) &&
                            m.SenderUserID != meUserId &&
                            !m.IsRead)
                .CountAsync();

            return count;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ CountUnreadForUserAsync Hatası: " + ex.Message);
            return 0;
        }
    }
}
