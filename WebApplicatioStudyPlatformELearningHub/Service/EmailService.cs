using StudyPlatformELearningHub.IService;
using System.Net.Mail;
using System.Net;

namespace StudyPlatformELearningHub.Service
{
    public class EmailService : IEmailService
    {
        public async Task<bool> SendEmailAsync(string email, string subject, string messageBody)
        {
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    using (SmtpClient smtpClient = new SmtpClient())
                    {
                        message.From = new MailAddress("StudyPlatformSupportAgent@gmail.com");
                        message.To.Add(email);
                        message.Subject = subject;
                        message.IsBodyHtml = true;
                        message.Body = messageBody;

                        smtpClient.Port = 587;
                        smtpClient.Host = "smtp.gmail.com";

                        smtpClient.EnableSsl = true;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential("replacyagent@gmail.com", "xjhxqygesqscfgxb");
                        smtpClient.Credentials = new NetworkCredential("smolskijt@gmail.com", "lsipcnuatekvjuhc");



                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                        await smtpClient.SendMailAsync(message);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                // Log the exception
                return false;
            }
        }
    }
}

