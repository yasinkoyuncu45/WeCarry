namespace WeCarry.Services
{
    public interface IChatService
    {
        Task SendAsync(int conversationId, int senderUserId, string body);
        Task<int> MarkReadAsync(int conversationId, int meUserId);
        Task<bool> UserIsParticipantAsync(int conversationId, int userId);
    }
}
