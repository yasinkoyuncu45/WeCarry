using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using WeCarry.Models.MVVM;  // User tablosunu görmek için

namespace WeCarry.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly Context _con;

        public EmailService(IConfiguration config, Context con)
        {
            _config = config;
            _con = con;
        }

        public async Task SendEmailAsync(string subject, string body, string toEmail, string? replyTo = null)
        {
            var host = _config["EmailSettings:SMTPServer"] ?? "tasiyicin.com";
            var user = _config["EmailSettings:UserName"]!;
            var pass = _config["EmailSettings:Password"]!;
            var port = int.TryParse(_config["EmailSettings:Port"], out var p) ? p : 587;
            var enableSsl = bool.TryParse(_config["EmailSettings:EnableSSL"], out var ssl) ? ssl : true;

            using var msg = new MailMessage
            {
                From = new MailAddress(user, "Taşıyıcı İletişim"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8
            };
            msg.To.Add(toEmail);
            if (!string.IsNullOrWhiteSpace(replyTo))
                msg.ReplyToList.Add(new MailAddress(replyTo));

            // TLS sürümü
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            async Task TrySendAsync(string h, int prt, bool ssl)
            {
                using var smtp = new SmtpClient(h, prt)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(user, pass),
                    EnableSsl = ssl,
                    Timeout = 20000
                };
                await smtp.SendMailAsync(msg);
            }

            try
            {
                await TrySendAsync(host, port, enableSsl);
            }
            catch (SmtpException ex) when (port == 587)
            {
                try
                {
                    await TrySendAsync(host, 465, true);
                }
                catch (Exception)
                {
                    throw new InvalidOperationException(
                        $"E-posta gönderilemedi. SMTP hata: {ex.StatusCode} - {ex.Message}", ex);
                }
            }
        }

        public async Task SendEmailToUserAsync(int userId, string subject, string body)
        {
            var user = await _con.User.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user?.Email != null)
            {
                await SendEmailAsync(subject, body, user.Email);
            }
        }
    }
}
