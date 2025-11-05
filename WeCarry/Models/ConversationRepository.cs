using Microsoft.EntityFrameworkCore;
using WeCarry.Models;
using WeCarry.Models.MVVM;

public class ConversationRepository : IConversationRepository
{
    private readonly Context _con;
    public ConversationRepository(Context con) => _con = con;

    // ------------------------------------------------------------
    // 1️⃣ Mevcut bir sohbet varsa getir, yoksa oluştur
    // ------------------------------------------------------------
    public async Task<Conversation?> GetOrCreateAsync(int adId, int starterUserId, int ownerUserId)
    {
        try
        {
            var conv = await _con.Conversations.FirstOrDefaultAsync(c =>
                c.AdID == adId &&
                c.StarterUserID == starterUserId &&
                c.OwnerUserID == ownerUserId &&
                c.Status == ConversationStatus.Active);

            if (conv != null)
                return conv;

            conv = new Conversation
            {
                AdID = adId,
                StarterUserID = starterUserId,
                OwnerUserID = ownerUserId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow,
                Status = ConversationStatus.Active
            };

            _con.Conversations.Add(conv);
            await _con.SaveChangesAsync();
            return conv;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ GetOrCreateAsync Hatası: " + ex.Message);
            return null;
        }
    }

    // ------------------------------------------------------------
    // 2️⃣ Sohbet Odasını Detaylı Görüntüle (ChatRoom sayfası)
    // ------------------------------------------------------------
    public async Task<ChatRoomViewModel?> GetRoomViewAsync(int conversationId, int meUserId)
    {
        try
        {
            var conv = await _con.Conversations
                .Include(c => c.Ad)
                .FirstOrDefaultAsync(c => c.ConversationID == conversationId);

            if (conv == null) return null;

            var isParticipant = conv.StarterUserID == meUserId || conv.OwnerUserID == meUserId;
            if (!isParticipant) return null;

            var otherId = conv.StarterUserID == meUserId ? conv.OwnerUserID : conv.StarterUserID;
            var otherUserName = await _con.User
                .Where(u => u.UserID == otherId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync() ?? "Kullanıcı";

            var adTitle = conv.Ad?.AdvertisementText;
            if (!string.IsNullOrWhiteSpace(adTitle) && adTitle!.Length > 60)
                adTitle = adTitle.Substring(0, 60) + "...";

            var messages = await _con.Messages
                .Where(m => m.ConversationID == conversationId)
                .Include(m => m.SenderUser)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageItemVM
                {
                    MessageID = m.MessageID,
                    SenderUserID = m.SenderUserID,
                    SenderName = m.SenderUser != null
                        ? (m.SenderUser.Name + " " + m.SenderUser.Surname).Trim()
                        : "Kullanıcı",
                    Body = m.Body,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return new ChatRoomViewModel
            {
                ConversationID = conv.ConversationID,
                MeUserID = meUserId,
                OtherUserID = otherId,
                OtherUserName = otherUserName,
                AdID = conv.AdID,
                AdTitle = adTitle ?? "",
                IsEnded = conv.Status == ConversationStatus.Ended,
                Messages = messages
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ GetRoomViewAsync Hatası: " + ex.Message);
            return null;
        }
    }

    // ------------------------------------------------------------
    // 3️⃣ Gelen Kutusu (aktif veya arşiv)
    // ------------------------------------------------------------
    public async Task<IEnumerable<InboxItemVM>> ListForUserAsync(int meUserId, bool active)
    {
        try
        {
            var baseQuery = _con.Conversations
                .Where(c => (c.StarterUserID == meUserId || c.OwnerUserID == meUserId)
                         && (active ? c.Status == ConversationStatus.Active : c.Status == ConversationStatus.Ended))
                .Select(c => new
                {
                    c.ConversationID,
                    c.LastMessageAt,
                    c.Status,
                    OtherId = c.StarterUserID == meUserId ? c.OwnerUserID : c.StarterUserID,
                    AdTitle = c.Ad.AdvertisementText
                });

            var list = await baseQuery.OrderByDescending(x => x.LastMessageAt).ToListAsync();
            var ids = list.Select(x => x.ConversationID).ToList();

            var unreadCounts = await _con.Messages
                .Where(m => ids.Contains(m.ConversationID) && !m.IsRead && m.SenderUserID != meUserId)
                .GroupBy(m => m.ConversationID)
                .Select(g => new { ConversationID = g.Key, Count = g.Count() })
                .ToListAsync();

            var unreadMap = unreadCounts.ToDictionary(x => x.ConversationID, x => x.Count);

            var result = new List<InboxItemVM>();
            foreach (var x in list)
            {
                var otherName = await _con.User
                    .Where(u => u.UserID == x.OtherId)
                    .Select(u => (u.Name + " " + u.Surname).Trim())
                    .FirstOrDefaultAsync() ?? "Kullanıcı";

                var title = x.AdTitle ?? "";
                if (title.Length > 60)
                    title = title[..60] + "...";

                result.Add(new InboxItemVM
                {
                    ConversationID = x.ConversationID,
                    OtherUserName = otherName,
                    AdTitle = title,
                    LastMessageAt = x.LastMessageAt,
                    Unread = unreadMap.TryGetValue(x.ConversationID, out var c) ? c : 0,
                    IsEnded = x.Status == ConversationStatus.Ended
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ ListForUserAsync Hatası: " + ex.Message);
            return Enumerable.Empty<InboxItemVM>(); // Boş liste döndür
        }
    }

    // ------------------------------------------------------------
    // 4️⃣ Sohbeti Bitir
    // ------------------------------------------------------------
    public async Task<bool> EndAsync(int conversationId, int userId)
    {
        try
        {
            var conv = await _con.Conversations.FindAsync(conversationId);
            if (conv == null) return false;

            if (conv.StarterUserID != userId && conv.OwnerUserID != userId)
                return false;

            conv.Status = ConversationStatus.Ended;
            conv.EndedByUserID = userId;
            await _con.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ EndAsync Hatası: " + ex.Message);
            return false;
        }
    }

    // ------------------------------------------------------------
    // 5️⃣ Kullanıcı bu sohbette mi kontrol et
    // ------------------------------------------------------------
    public async Task<bool> UserIsParticipantAsync(int conversationId, int userId)
    {
        try
        {
            return await _con.Conversations.AnyAsync(c =>
                c.ConversationID == conversationId &&
                (c.StarterUserID == userId || c.OwnerUserID == userId));
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ UserIsParticipantAsync Hatası: " + ex.Message);
            return false;
        }
    }

    // ------------------------------------------------------------
    // 6️⃣ Sohbet Detayını Getir (Admin Panel)
    // ------------------------------------------------------------
    public async Task<Conversation?> GetDetailAsync(int conversationId)
    {
        try
        {
            return await _con.Conversations
                .Include(c => c.Ad)
                    .ThenInclude(a => a.User)
                .Include(c => c.StarterUser)
                .Include(c => c.OwnerUser)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.SenderUser)
                .FirstOrDefaultAsync(c => c.ConversationID == conversationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ GetDetailAsync Hatası: " + ex.Message);
            return null;
        }
    }

    // ------------------------------------------------------------
    // 7️⃣ Tüm Sohbetlerin Özetini Listele (Admin)
    // ------------------------------------------------------------
    public async Task<List<ConversationListItemVm>> GetAllSummariesAsync()
    {
        try
        {
            return await _con.Conversations
                .Select(c => new ConversationListItemVm
                {
                    ConversationID = c.ConversationID,
                    AdID = c.AdID,
                    StarterUserName = (c.StarterUser.Name + " " + c.StarterUser.Surname).Trim(),
                    OwnerUserName = (c.Ad.User.Name + " " + c.Ad.User.Surname).Trim(),
                    CreatedAt = c.CreatedAt,
                    EndedAt = c.Status == ConversationStatus.Ended ? c.LastMessageAt : (DateTime?)null,
                    EndedByUserName = c.EndedByUserID != null
                        ? c.Messages
                            .Where(m => m.SenderUserID == c.EndedByUserID)
                            .Select(m => (m.SenderUser.Name + " " + m.SenderUser.Surname).Trim())
                            .FirstOrDefault()
                        : null
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ GetAllSummariesAsync Hatası: " + ex.Message);
            return new List<ConversationListItemVm>();
        }
    }
}
