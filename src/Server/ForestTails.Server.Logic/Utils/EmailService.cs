using ForestTails.Server.Logic.Config;
using ForestTails.Server.Logic.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ForestTails.Server.Logic.Utils
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private const int TimeoutMilliseconds = 10000;

        private readonly SmtpSettings smtpSettings;
        private readonly ILogger<EmailService> logger;

        public EmailService(IOptions<SmtpSettings> smtpOptions, ILogger<EmailService> logger)
        {
            smtpSettings = smtpOptions.Value;
            this.logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(smtpSettings.Host))
            {
                logger.LogWarning("SMTP host is not configured. Email to {To} with subject {Subject} was not sent.", to, subject);
                return;
            }

            try
            {
                using var client = new SmtpClient(smtpSettings.Host, smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
                    EnableSsl = smtpSettings.EnableSsl,
                    Timeout = TimeoutMilliseconds
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings.SenderEmail, smtpSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);
                await client.SendMailAsync(mailMessage);
                logger.LogInformation("Email sent to {To} with subject {Subject}.", to, subject);
            }
            catch (SmtpFailedRecipientException smtpException)
            {
                logger.LogError(smtpException, "Failed to deliver email to recipient {To}.", to);
                throw new ValidationException($"Failed to deliver email to recipient {to}.");
            }
            catch (SmtpException smtpException)
            {
                logger.LogError(smtpException, "SMTP error occurred while sending email to {To} with subject {Subject}.", to, subject);
                throw new InfrastructureException($"SMTP error occurred while sending email to {to} with subject {subject}.");
            }
            catch (TimeoutException timeoutException)
            {
                logger.LogError(timeoutException, "Timeout occurred while sending email to {To} with subject {Subject}.", to, subject);
                throw new InfrastructureException($"Timeout occurred while sending email to {to} with subject {subject}.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An unexpected error occurred while sending email to {To} with subject {Subject}.", to, subject);
                throw new InfrastructureException($"An unexpected error occurred while sending email to {to} with subject {subject}.");
            }
        }
    }
}
