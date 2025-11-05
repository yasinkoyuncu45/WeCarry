using WeCarry.Models.MVVM;

namespace WeCarry.Models
{
    public interface IConversationRepository
    {
        Task<Conversation> GetOrCreateAsync(int adId, int starterUserId, int ownerUserId);
        Task<ChatRoomViewModel?> GetRoomViewAsync(int conversationId, int meUserId);
        Task<IEnumerable<InboxItemVM>> ListForUserAsync(int meUserId, bool active);
        Task<bool> EndAsync(int conversationId, int byUserId);
        Task<bool> UserIsParticipantAsync(int conversationId, int userId);
        Task<List<ConversationListItemVm>> GetAllSummariesAsync();
        Task<Conversation?> GetDetailAsync(int conversationId);
    }
}
