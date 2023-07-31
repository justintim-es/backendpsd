namespace backend.Email;

public interface IEmailSender {
    void SendEmail(EmailMessage message);
}
