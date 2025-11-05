namespace WeCarry.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Basit e-posta gönderme (adres belirterek).
        /// </summary>
        Task SendEmailAsync(string subject, string body, string toEmail, string? replyTo = null);
        Task SendEmailToUserAsync(int userId, string subject, string body);
    }
}
