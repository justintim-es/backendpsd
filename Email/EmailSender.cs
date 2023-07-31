using System.Net.Mail;
using MimeKit;

namespace backend.Email;

public class EmailSender : IEmailSender {
    private readonly EmailConfiguration _emailConfig;

    public EmailSender(EmailConfiguration emailConfig) {
        _emailConfig = emailConfig;
    }
    public void SendEmail(EmailMessage message) {
        var emailMessage = CreateEmailMessage(message);

        Send(emailMessage);
    }
    private MimeMessage CreateEmailMessage(EmailMessage message) {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("thuisverzorgers.nl", _emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = string.Format(@"
        <!doctype html>
        <html lang=""en"">
        <body>
        <div style=""padding: 30px; background-color: #e0e0e0; text-align: center;"">
        <h1 style=""color: #d32f2f;"">Welkom bij thuisverzorgers.nl</h1>
        <h3 style=""color: #d32f2f;"">Bevestig alstublieft uw e-mail met de knop hieronder</h3>
        <br>        
        <a href=""http://localhost:4200/{0}"" style=""padding: 5px; color: #e0e0e0; background:#d32f2f; border-radius: 4px; text-shadow: 0 1px 1px rgba(0, 0, 0, 0.2); font-size: 125%;"">Bevestigen</a>
        </div>
        </body>
        </html>"" 
        ", message.Content)};
        return emailMessage;
    }
    private void Send(MimeMessage mailMessage) {
        using (var client = new MailKit.Net.Smtp.SmtpClient()) {
            try {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfig.Username, _emailConfig.Password);
                client.Send(mailMessage);
            }
            catch {}
            finally {
                client.Disconnect(true);
                client.Dispose();
            }
        }
    }
}