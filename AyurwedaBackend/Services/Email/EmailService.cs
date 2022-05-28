using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using AyurwedaBackend.Helpers;
namespace AyurwedaBackend.Services.Email
{

        public interface IEmailService
        {
            void Send(string to, string subject, string html, string from = null);
        }

        public class EmailService : IEmailService
        {
            private readonly MailSettings _mailSettings;

            public EmailService(IOptions<MailSettings> mailSettings)
            {
                _mailSettings = mailSettings.Value;
            }

            public void Send(string to, string subject, string html, string from = null)
            {
                // create message
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(from ?? _mailSettings.EmailFrom));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = html };

                // send email
                using var smtp = new SmtpClient();
                smtp.Connect(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }
    
}
