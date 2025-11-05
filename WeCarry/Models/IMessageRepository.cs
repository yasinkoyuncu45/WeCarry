using WeCarry.Models.MVVM;

namespace WeCarry.Models
{
    public interface IMessageRepository
    {
        Task<Message> AddAsync(int conversationId, int senderUserId, string body);
        Task<int> MarkReadAsync(int conversationId, int meUserId);
        Task<int> CountUnreadForUserAsync(int meUserId);
    }
}
